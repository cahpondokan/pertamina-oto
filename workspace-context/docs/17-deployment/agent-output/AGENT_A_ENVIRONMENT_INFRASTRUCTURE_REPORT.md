# AGENT A — Environment, Infrastructure, Network & Capacity Report

**Task ID:** DOC17-AGENT-A  
**Tujuan:** Rancangan baseline Deployment Guide PERTAMINA OTO untuk environment, topology, network, capacity, availability, backup/DR, dan hardening  
**Status:** Rekomendasi specialist; bukan perubahan terhadap master Document 17  
**Baseline sumber:** Document 01–16 FINAL dan `MASTER_PROJECT_SUMMARY.md` v3.9  
**Tanggal verifikasi profil sementara:** 1 September 2026

> **Batas keputusan:** Laporan ini mempertahankan logical separation dan flexible physical deployment. Laporan tidak memilih hosting Production, OS, container runtime, gateway, load balancer, cloud service, DNS, IP, port resmi, atau SLA. Semua keputusan Production ditandai `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION`; detail jaringan ditandai `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION`.

> **Profil Hostinger bersifat sementara:** Halaman resmi Hostinger saat verifikasi mencantumkan KVM 2 dengan 2 vCPU, RAM 8 GB, disk NVMe 100 GB, bandwidth 8 TB, I/O 300 MB/s, serta satu IPv4 dan IPv6. Hostinger menyatakan nama/ketersediaan plan dapat berbeda dan parameter dapat berubah tanpa pemberitahuan; nilai aktual harus diverifikasi pada hPanel saat provisioning. Sumber: [Hostinger — Parameters and limits of hosting plans](https://www.hostinger.com/support/6976044-parameters-and-limits-of-hosting-plans-in-hostinger/) dan [Hostinger — VPS Hosting](https://www.hostinger.com/vps-hosting?lang=en).

## 1. Environment strategy

### 1.1 Prinsip

1. Deployable tetap terpisah secara logis: Web, API, Worker, Desktop, Mobile, SQL Server, Secure File/Object Storage, Monitoring, dan Local Worker.
2. Satu node fisik sementara dapat menjalankan beberapa deployable, tetapi tidak menggabungkan process, identity, configuration, data, storage zone, log, health, atau resource budget.
3. Artefak rilis yang sama dipromosikan melalui pipeline; konfigurasi dan secret diinjeksikan per environment.
4. Environment memisahkan endpoint, credential, service identity, database, storage, queue, provider profile, telemetry, deployment authority, dan access route.
5. Development memakai data sintetis. Data Production tidak disalin ke non-Production tanpa masking, authorization, dan persetujuan resmi.
6. Local-only Feature tetap diproses lokal dan tidak dibuatkan processing API.
7. Pilot adalah environment terbatas untuk pembuktian operasional, bukan Production dan bukan tempat stress/fuzz/penetration test.
8. Production topology dipilih berdasarkan hasil load/recovery/security test dan persetujuan Pertamina, bukan berdasarkan keberhasilan pada satu VPS kecil.
9. RPO, RTO, retention, target availability, jumlah user, concurrency, dan capacity threshold tetap `TARGET_TO_BE_APPROVED` atau `POLICY_CONFIGURABLE`.

### 1.2 Alur promosi konseptual

```text
Local Development
→ Shared Development
→ QA/Testing
→ Pilot
→ future Pertamina-approved Production
```

Setiap perpindahan membutuhkan immutable artifact reference, version/digest, configuration/schema/capability compatibility, evidence gate, deployment approval, recovery point yang sesuai, serta post-deployment verification.

### 1.3 Environment matrix

| Atribut | Local Development | Shared Development | QA/Testing | Pilot | Production concept |
|---|---|---|---|---|---|
| **Purpose** | Feedback cepat, unit/component/contract logic, Local Worker | Integrasi awal bersama dan demonstrasi internal | Verifikasi system, contract, security, recovery, performance terbatas | Validasi operasional terkontrol dengan workload representatif | Operasi resmi setelah seluruh approval/gate |
| **Users** | Developer individual dan test identity | Tim development/reviewer berwenang | Tester, security reviewer, UAT preparation identity | Peserta pilot dan operator yang disetujui; jumlah tidak diasumsikan | Pengguna resmi sesuai scope Pertamina |
| **Data classification** | Synthetic minimal; tanpa identity/content Production | Synthetic; sanitized approved hanya bila kebutuhan dibuktikan | Synthetic, sanitized approved, boundary/corrupted/malicious corpus terkontrol | Synthetic atau data pilot yang secara eksplisit disetujui; Restricted content hanya dengan kontrol penuh | Operational data mengikuti klasifikasi resmi Pertamina |
| **Topology** | Client + Local Worker pada perangkat; server dependency lokal/isolated bila diperlukan | Logical Web/API/Worker/SQL/Storage/Monitoring; boleh satu node sementara | Stack/logical lane terisolasi; topology dicatat dalam manifest | Lane khusus terkontrol; tidak berbagi fault/load test dengan QA | Single node, separated VM, managed, atau HA—belum dipilih |
| **Network exposure** | Local/loopback atau jaringan developer terbatas; tidak publik | Restricted ingress dari sumber yang disetujui | Restricted test access; security test scope eksplisit | Client-facing terbatas kepada participant yang disetujui | Minimal public/client boundary; internal/admin/data private |
| **Identity** | Synthetic/local test identity untuk unit; approved non-prod SSO untuk integration | Approved non-prod SSO dan service identity terpisah | Approved non-prod identity/permission fixtures | Approved pilot identity; tidak memakai shared account | Approved Pertamina Identity dan workload identity per service |
| **Configuration** | Developer-safe default; no Production endpoint | Shared Dev typed/versioned config | QA typed/versioned config setara behavior Produksi bila diperlukan | Pilot config frozen/versioned dan dapat rollback | Environment-specific controlled config/policy |
| **Secret source** | Approved developer credential store; tidak dalam repo/file biasa | Approved non-prod secret source; injected | QA secret source terpisah dan least privilege | Pilot secret source terpisah; rotation/revocation siap | Pertamina-approved secret/key service—belum dipilih |
| **Database** | Isolated local/dev SQL Server bila server test diperlukan; SQLite per profile | Dev-only SQL database/instance terisolasi | QA SQL terisolasi; migration/recovery test terkontrol | Pilot SQL terisolasi dari Dev/QA | Satu logical SQL Server baseline dengan schema ownership; physical topology TBD |
| **Storage** | Local protected workspace dan synthetic object test area | Dev storage zones terpisah | QA quarantine/input/result/secured/issue/export zones terpisah | Pilot secure storage terpisah; quota/cleanup aktif | Private secure object storage terpisah dari SQL dan client |
| **Monitoring** | Local structured logs/metrics; safe debug | Dev telemetry stream terpisah | Full test instrumentation dan evidence correlation | Operational monitoring/alert rehearsal terpisah | Approved remote observability/APM + alert ownership |
| **Backup** | Reproducible fixture/config; local state disposable kecuali test recovery | Backup/recovery point sesuai shared change; bukan Production recovery | Verified pre-change point dan restore rehearsal | Verified backup + restore/reconciliation evidence | SQL/object/config/key-aware backup and DR policy TBD |
| **Scaling** | Tidak untuk sizing; ukur per-process bila relevan | Vertical/bounded concurrency sementara | Scale/load test pada isolated window dan stop condition | Staged rollout, queue, admission, headroom monitoring | Scale berdasarkan measured demand dan approved target |
| **Allowed testing** | Unit, architecture, contract logic, isolated component | Integration ringan, deployment smoke, shared functional | Integration, E2E, security, recovery, bounded performance/load/stress/soak | Business flow, canary, bounded representative load, monitoring/recovery drill yang disetujui | Bounded health/smoke/canary; no general testing |
| **Prohibited actions** | Production data/secret, public AI, shared DoS | Raw Production data, unbounded load, public AI, direct data patch | Unbounded attack, Production content, manual patch, testing di luar window | Fuzz, penetration, malicious corpus, stress/DoS, debug, direct DB/storage edit | Fuzz/load/malicious corpus/debug/untracked mutation/direct repair |
| **Approval** | Developer/team policy | Shared environment owner + security/network as applicable | QA/security/test owner dan test window approval | Pilot authority, Product/Security/Operations/Network approval | Formal Pertamina infrastructure, security, network, data, deployment approval |
| **Readiness** | Reproducible setup dan safe local test | Artifact/config/identity/data/monitoring manifest lengkap | Entry gate Document 12, recovery/cleanup/stop condition siap | QA/UAT/security gates, incident/rollback/recovery path, participant scope siap | `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` dan all Production gates |

### 1.4 Pemisahan saat satu physical node digunakan

Co-hosting tidak mengizinkan:

- shared process identity;
- shared writable application directory;
- shared secret;
- shared database/data set antar-environment;
- shared storage prefix/directory tanpa owner/purpose boundary;
- log file campuran tanpa environment/component fields;
- health endpoint tunggal yang menyembunyikan kegagalan komponen;
- resource tanpa limit yang membiarkan Worker menghabiskan API/SQL headroom;
- destructive QA test berjalan bersamaan dengan Pilot;
- backup aktif disimpan hanya pada disk aktif node yang sama.

## 2. Logical topology

### 2.1 Topologi server

```text
UNTRUSTED / PARTIALLY TRUSTED CLIENTS
Desktop | Web Browser | Mobile | Tablet
                  │
                  │ approved SSO + HTTPS
                  ▼
PUBLIC / CLIENT-FACING BOUNDARY
Ingress / certificate termination concept (product not selected)
       ├── Web process
       ├── API process
       └── Authenticated SignalR notification surface
                  │
                  ▼
APPLICATION SECURITY BOUNDARY
Authentication → session/account → permission/ownership/scope
→ policy/classification → state/version → capacity/admission
                  │
          ┌───────┴────────┐
          ▼                ▼
 Application Modules   Persisted Job / bounded queue
          │                │
          │                ▼
          │        INTERNAL WORKLOAD BOUNDARY
          │        Server Worker + engine adapters
          │        separate workload identity
          │                │
          └───────┬────────┘
                  ▼
         DATA AND STORAGE BOUNDARIES
    SQL Server metadata       Private Secure Storage
    module/schema ownership   quarantine/input/temp/result/
                              secured/issue/export zones
                  │
                  ▼
        MONITORING / AUDIT BOUNDARIES
 OpenTelemetry/APM/logs      Enterprise Audit (separate purpose)
```

### 2.2 Topologi lokal

```text
Desktop/Mobile application
        │
        ├── protected profile + SQLite/workspace
        │
        └── authenticated typed/versioned local IPC
                         │
                         ▼
                 Isolated Local Worker
                 job-scoped references
                 no network by default
```

Local Worker bukan Server Worker. Local-only processing tetap zero server upload/processing. Hanya explicit optional/server workflow atau **Amankan ke Akun** yang menggunakan boundary server sesuai permission/policy.

### 2.3 Flexible physical deployment

```text
Logical component                 Temporary physical option
Web                               same node, separate process/identity/config
API                               same node, separate process/identity/config
Worker                            same node, separate process/identity/resource limit
SQL Server                        same node only for approved temporary lane
Secure Storage                    private directory/approved store, separate zone
Monitoring collector              separate process; remote sink preferred if approved
```

Pemisahan ke VM/service lain tidak mengubah application business logic atau contract.

## 3. Trust/network boundaries

| Boundary | Boleh dipercaya untuk | Tidak boleh dipercaya untuk | Kontrol minimum |
|---|---|---|---|
| Client/device | Intent dan presentation | Role, owner, scope, path, provider, result authority | SSO, HTTPS, typed contract, server authorization |
| Identity | Subject/claim yang disetujui dan tervalidasi | Ownership atau permission resource OTO | Issuer/audience/lifetime validation, account/session checks |
| Public ingress | Meneruskan traffic yang diizinkan | Identity atau forwarded header tanpa validation | Minimal exposure, Host/proxy validation, rate/body/header limits |
| Web/API | Application authorization dan admission | Blind trust pada ID, queue, network internal, atau provider | Deny-by-default, policy, ownership, state/version, correlation |
| SignalR | Notifikasi state mungkin berubah | Business mutation atau source of truth | Authenticated connection, server group, safe post-commit event, REST reconcile |
| Internal Worker | Job tepat dalam workload scope | User token, arbitrary path/URL/SQL/shell/provider | Separate identity, bounded queue/lease, output validation |
| SQL Server | Authoritative metadata setelah authorization | Content blob default atau direct client trust | Internal-only, encrypted connection, schema least privilege |
| Secure Storage | Private content object sesuai owner/purpose/state | Raw key/path sebagai authorization | Private access, opaque refs, encryption, digest, zone permissions |
| Administration | Approved control-plane action | General content access | Restricted route, current authorization, reauth, Audit |
| Monitoring | Safe health/resource/security detection | Business truth, Audit substitute, content access | Safe allowlist, separate access/retention, alert ownership |
| Approved provider | Exact request/purpose yang disetujui | Public AI fallback atau instruction authority | Egress allowlist, scoped identity, minimization, timeout/circuit |

### 3.1 Boundary exposure policy

- Public/client-facing: hanya Web/API/SignalR/compatibility atau minimal health surface yang disetujui.
- Internal service: Worker, queue, engine, provider adapter, SQL, storage, and detailed health tidak client-facing.
- Database: hanya application runtime/migration/backup/admin identities dari jalur internal yang disetujui.
- Storage: tidak listing/public; download/upload melalui authorized flow atau delegated mechanism yang sempit bila disetujui.
- Administration: restricted route dan reauthentication untuk high-impact action; bukan public management port.
- Monitoring: safe aggregate; detailed topology/dependency hanya untuk operations permission.

## 4. Dev topology

### 4.1 Local Development

```text
Developer device
├── Desktop/Web/Mobile development client as needed
├── Local Application modules
├── Local Worker isolated process
├── protected test workspace + SQLite profile
└── optional isolated server dependencies for integration
```

Aturan:

- data default sintetis dan deterministic;
- tidak menggunakan credential/data Production;
- local secret berasal dari approved developer credential boundary, bukan repository;
- external provider dipakai hanya melalui approved non-Production profile; public AI tetap dilarang;
- unit/component/contract test tidak memerlukan shared infrastructure;
- Local Worker memiliki resource/time/temp limit dan no network default;
- setup harus reproducible dan tidak bergantung pada path/tool workstation yang tidak dinyatakan;
- local environment bukan baseline capacity Production.

### 4.2 Shared Development

Shared Dev dapat memakai satu node sementara, termasuk Hostinger KVM 2, dengan bentuk:

```text
Restricted client access
→ Web process
→ API process
→ Dev application modules
→ bounded Dev queue
→ Worker process
→ Dev-only SQL + Dev-only storage zones
→ Dev telemetry stream
```

Guard:

- process, service identity, config, log, health, data, and storage directory terpisah;
- synthetic data only by default;
- endpoint dan credential berbeda dari QA/Pilot/Production;
- no direct public management exposure;
- Worker tidak boleh menghabiskan seluruh CPU/RAM/disk sehingga API/SQL gagal diam-diam;
- shared integration/load run mempunyai window, owner, stop condition, dan cleanup;
- deployment mengikuti immutable artifact pipeline; tidak patch binary/config/data manual;
- jika satu KVM memuat beberapa logical lane, destructive/load testing harus diserialisasi dan dipisahkan dari Pilot.

### 4.3 Readiness Dev

- artifact/version/config manifest tersedia;
- service startup/readiness per process terlihat;
- database migration compatibility lulus;
- synthetic data seeding dan cleanup aman;
- no secret/content canary pada log/telemetry;
- Local-First negative server traffic test lulus;
- resource baseline per process tercatat;
- restore/recreate procedure diuji untuk shared lane.

## 5. QA topology

### 5.1 Bentuk logis

```text
QA test clients and approved test identity
→ restricted QA ingress
→ QA Web/API/SignalR
→ QA modules + bounded queues
→ QA Worker pools/classes
→ QA SQL + QA secure storage zones
→ QA monitoring/evidence sink
```

QA harus memisahkan:

- build/digest dan configuration/policy/schema/capability versions;
- identity/permission fixtures;
- SQL, storage, queue, provider profile, logs, and evidence;
- normal synthetic corpus dari quarantined malicious/corrupted corpus;
- functional lane dari bounded performance/fault-injection lane bila keduanya berpotensi saling mengganggu.

### 5.2 Testing yang diizinkan

- integration, contract, API, database/storage, Worker/IPC, E2E;
- security/privacy, malformed/corrupted/malicious corpus dalam runner terisolasi;
- bounded load, concurrency, stress, soak, recovery, fault injection;
- backup/restore and cross-store reconciliation;
- SignalR disconnect dengan REST reconciliation;
- upload/download resume, Worker crash/lease, storage/SQL unavailable;
- migration/forward-fix/rollback rehearsal;
- deployment smoke dan evidence collection.

### 5.3 Guard QA

- non-Production dan synthetic/sanitized-approved data saja;
- fault injection mempunyai exact target, blast radius, owner, timeout, stop condition, cleanup, dan recovery point;
- disk-full memakai quota-limited test area, bukan system/active workspace root;
- monitoring tetap aktif selama test;
- no unbounded attack/load atau test tanpa stop condition;
- no public AI or unapproved provider;
- no manual SQL/storage repair;
- no startup migration untuk model Production;
- hasil numerik tetap `MEASURED_BASELINE` sampai target disetujui.

### 5.4 Readiness QA

- environment manifest Document 12 lengkap;
- instrumentation/generator/corpus tervalidasi;
- capacity headroom untuk test window diketahui;
- destructive test tidak berbagi node dengan active Pilot;
- backup/recovery and reconciliation runbook tersedia;
- security/secret/logging guard lulus;
- test evidence disimpan terpisah dan dapat ditelusuri.

## 6. Pilot topology

### 6.1 Tujuan

Pilot mengukur penggunaan nyata terkontrol, workload mix, bottleneck pertama, queue/backpressure, usability operasi, monitoring, incident handoff, dan recovery readiness. Pilot tidak membuktikan Production capacity atau HA.

### 6.2 Bentuk logis

```text
Approved pilot participants
→ restricted pilot endpoint and SSO
→ Pilot Web/API/SignalR
→ Pilot modules + admission/queue
→ Pilot Worker
→ isolated Pilot SQL + Secure Storage
→ separate Pilot monitoring/alerts
→ approved backup/recovery point
```

### 6.3 Guard Pilot

- lane, identity, database, storage, queue, provider, secrets, logs, dan backup terpisah dari Dev/QA;
- participant dan Feature exposure dibatasi melalui policy/flag yang sudah ada;
- staged rollout dan rollback/disable path diuji;
- data yang digunakan mengikuti approval klasifikasi/retention; tidak otomatis memakai Production data;
- hanya bounded representative workload, canary, smoke, dan approved recovery drill;
- no fuzz, penetration, malicious corpus, stress/DoS, manual data patch, atau debug detail;
- Security Incident dan Incident Support route siap;
- local-capable path tetap tersedia ketika server/provider terganggu;
- Pilot pada Hostinger KVM 2 membutuhkan persetujuan eksplisit dan tidak boleh diberi label Production.

### 6.4 Pilot evidence

- active/peak users measured, tidak diasumsikan;
- request rate per API category;
- job/day dan concurrent jobs per workload class;
- upload/download bytes and duration;
- CPU/RAM/GC/disk/temp/network per process dan host;
- SQL pool/latency/log/tempdb and storage growth;
- queue arrival/service/depth/oldest age/drain/fairness;
- cleanup throughput versus growth;
- error/retry/cancel/failure and incident count;
- Local-First zero-server-load checks;
- backup/restore measurement;
- headroom, bottleneck, and scale/throttle trigger proposal.

## 7. Hostinger KVM 2 profile

### 7.1 Verified vendor profile

| Parameter | Nilai terverifikasi saat laporan | Interpretasi |
|---|---:|---|
| vCPU | 2 | Shared CPU budget harus dibagi Web/API/Worker/SQL/monitoring; HEAVY work mudah mengganggu control plane |
| RAM | 8 GB | Per-process budget wajib; SQL, Worker, dan file processing tidak boleh memakai whole-file buffering |
| NVMe disk | 100 GB | Active data, DB/log/temp/storage/logs pada satu disk berisiko cepat penuh; strict quota/cleanup/backup separation wajib |
| I/O | 300 MB/s | Nilai plan, bukan jaminan latency; SQL, temp conversion/OCR, download, backup dapat saling bersaing |
| Bandwidth | 8 TB | Kuota transfer, bukan throughput/SLA; upload/download/backup/monitoring harus diukur |
| Address | Satu IPv4 dan IPv6 | Tidak menciptakan private network separation fisik; exposure dibatasi firewall dan application boundaries |

Nilai berasal dari halaman resmi Hostinger dan wajib diperiksa kembali pada hPanel karena parameter dapat berubah.

### 7.2 Intended use

KVM 2 dapat dievaluasi untuk:

- Shared Development;
- QA functional/integration terbatas;
- bounded performance baseline pada isolated window;
- Pilot terbatas dengan approval;
- rehearsal deployment/backup/restore non-Production.

KVM 2 tidak dinyatakan cukup untuk Production, HA, official user volume, unbounded file processing, large OCR/conversion concurrency, atau long-term secured storage.

### 7.3 Logical layout pada satu node

```text
KVM 2 TEMPORARY NODE
├── Web process
│   ├── own identity/config/log/health
│   └── no direct SQL/storage administration
├── API process
│   ├── own identity/config/log/health
│   └── bounded API/DB connection budget
├── Worker process
│   ├── own identity/config/log/health
│   ├── bounded CPU/RAM/time/temp/concurrency
│   └── separate workload classes/queue
├── SQL Server logical instance/database
│   ├── separate runtime/migration/backup identities
│   ├── data/log/temp capacity monitoring
│   └── no client/public exposure
├── Secure storage root
│   ├── quarantine
│   ├── validated input
│   ├── processing/checkpoint
│   ├── result/secured version
│   └── issue/export zones
├── Monitoring collector/log pipeline
│   └── safe environment/component separation
└── Backup staging only
    └── final backup copy separate from active node
```

Directory atau service mechanism tepat bergantung pada OS/runtime yang disetujui dan tidak dipilih di laporan ini.

### 7.4 Resource assessment

| Area | Risiko pada KVM 2 | Kontrol sementara |
|---|---|---|
| CPU | Conversion/OCR dan compression dapat menyaturasi dua vCPU | Queue HEAVY class; serialize HEAVY execution sebagai initial safety candidate; validate before increasing; reserve API/SQL headroom |
| RAM | Worker/engine/SQL/GC bersaing; OOM dapat menurunkan seluruh node | Per-process working-set/budget monitoring, streaming, Worker recycle, reject/wait before unsafe saturation |
| Disk | Temp file, SQL data/log/temp, object payload, Audit, Issue screenshot, log, backup staging berbagi 100 GB | Zone quota, free-space preflight, short temp retention, bounded log rotation, growth alert, external/separate backup |
| I/O | OCR temp dan SQL/log/backup dapat menimbulkan contention | Tidak menjadwalkan heavy backup/cleanup bersamaan dengan test/pilot peak; measure latency/queue |
| Network | Transfer file dan backup menghabiskan quota/connection; throughput tidak dijamin | Streaming, resumable transfer, rate/concurrency limit, byte monitoring, staged rollout |
| Queue | Worker throughput lebih kecil dari arrival rate | Bounded persistent queue, oldest-age/drain monitoring, fair workload class, explicit backpressure |
| Concurrency | Parallel heavy Job dapat mengganggu API/SQL | Concurrency per class `TARGET_TO_BE_BASELINED`; conservative admission; no user-count assumption |
| Cleanup | Cleanup yang agresif dapat berebut I/O atau menghapus object aktif | Lease/owner/purpose-aware bounded batches; low-priority throttle; never manual delete user files |
| Backup | Whole-node backup tidak menjamin cross-store application consistency | SQL/object/config-aware backup + restore/reconcile test; off-node/separate copy required by approved policy |

“Serialize HEAVY” adalah kandidat aman untuk node sementara, bukan policy Production. Nilai concurrency akhir berasal dari measured baseline, correctness/security checks, queue drain, dan approved headroom.

### 7.5 Backup limitation

Hostinger mendokumentasikan backup/snapshot VPS dan menyatakan restore backup atau snapshot akan menimpa seluruh isi server saat ini. Backup otomatis mingguan tersedia menurut dokumentasi resmi, tetapi frekuensi/retention aktual tetap harus diperiksa pada account. Sumber: [Hostinger — How to back up or restore a VPS](https://support.hostinger.com/en/articles/1583232-how-to-back-up-or-restore-a-vps).

Implikasi:

- vendor whole-node backup bukan satu-satunya backup produk;
- restore tidak dilakukan sebagai langkah troubleshooting biasa;
- sebelum restore, exact recovery point, impact, current data preservation, key/config, dan cross-store consistency harus diperiksa;
- SQL metadata dan storage object/digest/version/tombstone/hold direkonsiliasi setelah restore;
- backup/snapshot aktif tidak menggantikan off-node/approved backup, migration recovery point, atau restore rehearsal;
- RPO/RTO tidak diinferensikan dari schedule vendor.

### 7.6 Exit criteria dari KVM 2

Pisahkan/migrasikan komponen ketika salah satu indikator tervalidasi:

- sustained CPU/RAM saturation mengganggu API/SQL/monitoring;
- queue tidak drain pada representative arrival rate;
- disk/temp/log/object growth tidak memiliki approved headroom;
- SQL I/O/pool latency dipengaruhi Worker;
- backup/restore window atau data size melampaui operasi aman;
- Pilot memerlukan isolation/availability/security yang tidak dapat diberikan single node;
- public/private/admin/data boundary tidak dapat dipenuhi;
- security/network/data residency policy tidak mengizinkan Hostinger;
- HA/DR atau availability target disetujui dan membutuhkan topology lain.

## 8. Production topology options

Tidak ada opsi yang dipilih final. Semua memerlukan `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION`.

### Option A — Single Node

Web, API, Worker, SQL, storage, dan collector berada pada satu node tetapi tetap terpisah secara logis.

- cocok hanya untuk temporary/non-Production atau sangat terbatas jika policy mengizinkan;
- satu failure domain dan maintenance domain;
- resource contention tinggi;
- tidak menyediakan HA;
- migrasi ke separation relatif langsung bila boundaries dipertahankan.

### Option B — Separated VMs

Web/API, Worker, SQL, storage gateway/adapter, dan monitoring ditempatkan pada VM/failure domain terpisah sesuai kebutuhan.

- isolation dan independent scaling lebih baik;
- network/service identity/firewall lebih kompleks;
- masih membutuhkan pilihan backup/HA untuk tiap komponen;
- mendukung pemisahan Worker-heavy dari API/SQL.

### Option C — Managed Services

Sebagian data, storage, queue, monitoring, atau compute memakai layanan terkelola yang disetujui Pertamina.

- mengurangi sebagian beban patch/HA/backup operasi;
- responsibility boundary, data residency, identity, key, network, portability, cost, dan service limits harus dinilai;
- tidak mengubah logical ownership atau Local-First;
- vendor/service tidak dipilih di sini.

### Option D — High Availability

Web/API/Worker redundant, data/storage mempunyai HA/replication/recovery yang disetujui, dan ingress/failover tidak menjadi single point of failure.

- availability dan maintenance flexibility tertinggi;
- cost, complexity, testing, monitoring, data consistency, failover, and operations maturity tertinggi;
- tetap membutuhkan DR; HA bukan backup;
- topology, region/site, quorum/failover, and service selection belum ditentukan.

### 8.1 Migration principle

```text
Preserve contracts and logical ownership
→ externalize environment configuration/secrets
→ move Worker first when compute contention dominates
→ move SQL/storage when data/I/O/security requires
→ scale stateless Web/API after state boundaries are proven
→ add redundancy only with failure/recovery evidence
```

Tidak ada in-place manual patch atau startup migration Production.

## 9. Network/TLS principles

### 9.1 Inbound

- HTTPS only untuk seluruh client-facing traffic;
- certificate validation end-to-end; trust bypass dilarang;
- expose minimum Web/API/SignalR/compatibility/health surface yang disetujui;
- SQL Server, queue, Worker, object storage, admin management, detailed health, dan monitoring backend tidak public;
- reverse-proxy forwarded host/protocol/client data hanya dipercaya dari proxy yang disetujui;
- Host, Origin, CORS, WebSocket Origin, request framing, header/body/time/upload limit divalidasi;
- SignalR authenticated dan notification-only; REST/persisted state tetap source of truth;
- admin endpoint restricted, online, current authorization, and reauthentication untuk high-impact action;
- health publik hanya status minimum; dependency/topology detail restricted.

### 9.2 Outbound

- default-deny atau minimum allowlist sesuai kemampuan infrastructure;
- hanya SSO, approved internal provider, approved monitoring, update/dependency source pada deployment path, time source, DNS/certificate service yang disetujui;
- Worker/engine tidak mempunyai network arbitrer;
- provider destination/profile dipilih dari approved configuration, bukan request user;
- public AI/translation endpoint secara teknis diblokir;
- no raw user token forwarded ke provider;
- outbound timeout, retry, circuit, concurrency, byte, dan purpose limit;
- egress log tidak memuat content, token, URL sensitif, atau provider payload.

### 9.3 Firewall dan administration

- inbound default deny kecuali surface yang disetujui;
- SQL/storage/management hanya dari source/service identity/network yang disetujui;
- administration memakai restricted access route; tidak ada management port public default;
- firewall infrastructure dan host-level control berlapis bila platform mendukung;
- rule mempunyai owner, purpose, source/destination class, review/expiry, dan evidence;
- emergency rule tetap tracked, time-bounded, reviewed, and removed;
- no DNS/IP/port number dibuat pada laporan ini.

### 9.4 DNS dan certificate lifecycle

Lifecycle minimum:

```text
approved domain/zone ownership
→ controlled record change and review
→ certificate issuance/validation
→ secure private-key boundary
→ inventory + expiry monitoring
→ automated or controlled renewal
→ pre-expiry verification
→ rotation/revocation/incident recovery
→ documented rollback/failover
```

Certificate tidak disimpan dalam repository/log/ordinary config. Renewal failure harus memberi alert sebelum expiry sesuai target yang disetujui.

### 9.5 Time synchronization

- seluruh server, SQL, Worker, monitoring, Audit, backup, dan deployment menggunakan approved synchronized time source;
- simpan UTC untuk event dan timezone hanya untuk display;
- clock source/freshness dipantau;
- time rollback/skew tidak boleh memperpanjang session/offline grant, lease, retention, certificate, atau delegated access;
- exact NTP/time service dan source allowlist memerlukan konfirmasi.

Seluruh domain, IP, port/protocol matrix, proxy/gateway, firewall sources, private connectivity, certificate authority, TLS profile, DNS ownership, egress destinations, and time source: `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION`.

## 10. SQL/storage placement

### 10.1 SQL Server

- internal only; tidak dapat diakses client;
- satu SQL Server baseline dengan schema/data ownership per modul;
- runtime, migration/DDL, backup/restore, deployment, dan human administration identity terpisah;
- no runtime `db_owner` atau shared human credential;
- encrypted connection with full certificate validation;
- approved at-rest and backup encryption;
- short transactions; no DB connection held during file streaming, queue wait, engine/provider processing, SignalR, atau retry delay;
- rowversion, unique constraints, idempotency, outbox, timeout, pagination, and bounded query;
- data/log/tempdb/growth/IO/pool/backup health dipantau;
- Production migration melalui controlled pipeline, bukan application startup.

### 10.2 Secure File/Object Storage

Byte dokumen tidak disimpan sebagai SQL Blob default. Storage private memuat purpose zone:

```text
upload quarantine
validated input
processing/checkpoint
job result
secured version
issue attachment
export staging
```

Setiap object memakai opaque reference dan metadata owner/purpose/type/size/digest/state/key reference/timestamp/retention. Raw path, bucket/share key, atau permanent URL tidak menjadi kontrak client/log.

Kontrol:

- private access dan least-privilege identity;
- encryption at rest serta key terpisah dari data;
- validate/digest sebelum promotion;
- quarantine tidak memberi Worker akses secured delete/write;
- lease-aware cleanup dan tombstone/retention/legal hold;
- backup/restore merekonsiliasi SQL metadata dengan object/version/digest/key/tombstone;
- maximum-three active secured versions diverifikasi setelah restore;
- storage health, free space, object growth, orphan, cleanup throughput, and backup status dipantau.

### 10.3 Backup location

- terpisah secara failure domain dari active SQL/storage/node;
- encrypted, integrity/authenticity verified, least privilege;
- key access terpisah;
- retention/immutable/offline copy sesuai policy yang belum ditentukan;
- restore staging tidak bercampur dengan active data;
- backup reader/operator tidak otomatis mendapat business content permission.

### 10.4 Monitoring/log/Audit placement

| Data plane | Placement principle | Batas |
|---|---|---|
| Monitoring metrics/traces | Approved separate telemetry stream/sink per environment | Safe technical health; no content/secret |
| Application logs | Structured, protected, rotated, environment/component-separated | No file bytes/OCR/prompt/token/path/raw error |
| Enterprise Audit | SQL metadata/evidence store per Document 10 | Separate permission, integrity, retention/hold |
| Issue/support | SQL message/safe metadata + exact attachment/export in secure storage | Support scope only |
| Backup | Separate protected recovery location | Not general archive or content browsing |

## 11. Monitoring placement

Baseline stack tetap Microsoft.Extensions.Logging + OpenTelemetry + Azure Monitor/Application Insights sesuai Document 05. Backend/connection/topology deployment aktual membutuhkan persetujuan Pertamina; laporan ini tidak memilih layanan baru.

### 11.1 Collection model

```text
Web/API/Worker/SQL/storage/host/local clients
→ structured logs + traces + metrics
→ environment/component-labelled collector/export path
→ approved restricted observability backend
→ alert + dashboard + incident reference
```

Pada single-node sementara, collector boleh co-host sebagai process terpisah, tetapi telemetry penting harus diekspor ke sink terpisah bila disetujui agar tidak hilang bersama node. Local-only processing tidak mengirim document content melalui telemetry.

### 11.2 Minimum signals

| Area | Signals |
|---|---|
| Host/process | CPU, RAM, GC, thread/handle, disk free/IO/latency/queue, network bytes/errors, process restart |
| Web/API | readiness, availability, request rate/concurrency, p50/p95/p99 bila valid, status/error, body rejection |
| Worker | health, job duration, peak/residual memory, crash/recycle, concurrency, engine/provider class |
| Queue | arrival/service/depth/oldest age/drain/fairness/retry/dead-letter equivalent if approved |
| SQL | connection/pool wait, query/log/tempdb/IO, waits/lock/deadlock, backup/migration state |
| Storage | capacity/growth, upload/download, temp peak, orphan/reconcile, cleanup, digest/finalization failure |
| SignalR | connection/reconnect/buffer/gap/REST reconciliation |
| Audit | intent latency, backlog/drain/duplicate/reject, safe event volume |
| Support | Issue submission/attachment/export failure and backlog safe aggregate |
| Security | auth/revocation, authorization probe, file reject, public-AI block, secret/content canary |
| Version | app/API/contract/capability/config/schema/provider profile deployed |

### 11.3 Alert principles

- threshold mempunyai environment, owner, source policy, freshness, and escalation route;
- alert deduplicated dan tidak mengirim content/secret;
- unknown/stale critical telemetry bukan healthy;
- health endpoint status minimum; detail restricted;
- alert tidak menggantikan business state, Enterprise Audit, atau persisted Job state;
- target/cadence tetap `TARGET_TO_BE_APPROVED`;
- monitoring harus aktif selama load, fault injection, deployment, restore, and incident recovery.

## 12. Capacity model

### 12.1 Kategori

```text
LIGHT
MEDIUM
HEAVY
EXTERNAL/INTERNAL_PROVIDER_DEPENDENT
```

Kategori adalah planning class, bukan SLA atau alasan menghapus Feature.

### 12.2 Workload classification

| Workload | Class | Dominant resource | Capacity behavior |
|---|---|---|---|
| Normal metadata API, feature discovery, safe profile/config read | `LIGHT` | API CPU, SQL connection/query, network small payload | Pagination, cache-safe projection, short DB lifetime, rate limit |
| Authentication/session/authorization checks | `LIGHT` per request; dependency-sensitive | Identity/network/CPU/cache/SQL | Fail closed, bounded retry, no stale privileged allow |
| File upload/download | `MEDIUM`, becomes `HEAVY` by size/concurrency | Network, disk/temp, storage IO, connections | Streaming/resumable, byte/chunk limit, quarantine, rate/concurrency/backpressure |
| Server PDF conversion/compression/render | `HEAVY` | CPU, RAM, temp disk, engine process | Background queue, per-engine pool, Worker isolation, output validation |
| OCR | `HEAVY` | CPU, RAM, image/temp disk, engine | Queue, selected pages, bounded parallelism, cleanup, local-first where supported |
| AI/internal provider invocation | `EXTERNAL/INTERNAL_PROVIDER_DEPENDENT` | Provider latency/quota, egress, preprocessing CPU, temp storage | Approved provider only, circuit, concurrency/context/output quota, no public fallback |
| Job queue metadata/lease/outbox | `LIGHT` per operation; operationally critical | SQL/queue IO, locks, timer, Worker throughput | Persistent bounded fair queue, atomic lease, oldest-age/drain monitoring |
| Enterprise Audit ingest | `LIGHT` per event; `MEDIUM` under backlog/export | SQL log/IO/storage, export CPU/network | Batch/idempotent ingest, backlog/drain, retention/archive, separate export job |
| Audit query/export | `MEDIUM` to `HEAVY` | SQL reads, temp/export storage, network | Bounded filter/date/row, async export, field projection, backpressure |
| Issue message/safe context | `LIGHT` | SQL/network | Small typed payload, idempotency, safe diagnostics |
| Issue screenshot/Support export | `MEDIUM` | Upload/storage/network/export CPU | Explicit send, size/format limit, private object, retention/expiry |
| Secured file/version upload/download | `MEDIUM` to `HEAVY` | Network, object storage, digest, SQL metadata | Explicit transfer, streaming, quota, max-three versions, reconciliation |
| SignalR notification | `LIGHT` per event; connection-heavy aggregate | Connection/buffer/network | Limit connection/subscription/buffer; REST fallback |
| Cleanup/archive/reconciliation | `MEDIUM`; can become `HEAVY` | Storage/SQL IO | Low-priority bounded batches, lease/hold-aware, pressure throttle |
| Concurrent users | Aggregate mix, not one workload class | API/identity/SignalR/SQL/network | Measure active/peak, request mix, connection behavior; no official count invented |
| Concurrent jobs | Class follows job mix | Worker/engine/temp/queue | Weighted admission and per-class concurrency; heavy isolation |

### 12.3 Required measurements

- active and peak users, measured only;
- request rate/concurrency by API category;
- jobs/day and concurrent jobs by workload class;
- CPU time, working/private memory, GC/LOH/pause, thread/handle per process/job;
- disk/temp peak/job, IOPS/bytes/latency/queue/free space;
- network bytes/job, throughput, retry/resume efficiency;
- SQL reads/writes/CPU/duration/log/tempdb, pool waits, locks/deadlocks;
- queue arrival/service/depth/oldest age/drain/fairness;
- storage rows/objects/bytes per day and cleanup throughput;
- Audit/Issue/History/Secured growth and retention/archive effect;
- backup size/window and measured restore/reconciliation time;
- error/cancel/retry/duplicate/reopen rate;
- host total plus per-process headroom.

### 12.4 Capacity gates

Nilai melalui lifecycle:

```text
TARGET_TO_BE_BASELINED
→ MEASURED_BASELINE
→ TARGET_TO_BE_APPROVED
→ POLICY_CONFIGURABLE
```

Tanpa target resmi, correctness/security/source-safety/backpressure tetap PASS/FAIL. Capacity test tanpa target numerik tidak boleh dinyatakan memenuhi Production SLA.

## 13. Scaling/backpressure

### 13.1 Urutan tindakan

```text
Measure workload and bottleneck
→ validate instrumentation and correctness
→ apply bounded admission/concurrency/queue
→ tune safe application/query/storage behavior
→ separate Worker when compute contention dominates
→ separate SQL/storage when data/I/O/security dominates
→ scale stateless Web/API when request/connection demand dominates
→ add redundancy/HA after failure and recovery evidence
```

### 13.2 Controls

- local processing first bila Feature/capability/policy mengizinkan;
- weighted cost per Job/Feature class;
- per-user/category/global concurrency yang disetujui;
- atomic capacity reservation before accepting work;
- persistent, bounded, fair queue separated by workload class;
- upload/download byte, stream, chunk, temp, time, and range limit;
- Worker CPU/RAM/time/temp/output/thread/child-process pools;
- provider context/call/time/concurrency quota and circuit;
- DB total connection budget across Web/API/Worker/monitoring;
- cleanup/archive low-priority bounded batch;
- retry attempt budget, idempotency, exponential backoff/jitter, gradual backlog release;
- staged Feature/pilot rollout and rollback/disable path;
- capacity monitoring and alert before unsafe saturation.

### 13.3 User-visible overload behavior

| Condition | Behavior |
|---|---|
| Request/file hard limit | Safe `413`; no partial promoted result |
| Caller quota/rate/concurrency | Safe `429` dan `Retry-After` bila sesuai contract |
| State/version/storage conflict | Safe `409`; refresh/reconcile, no overwrite |
| Shared unsafe saturation/security dependency unavailable | Safe `503`; no anonymous/stale privileged fallback |
| Worker/resource busy | Queue/wait/reject explicitly; Activity Center/REST persisted state |
| Provider slow/unavailable | Circuit/backpressure/wait or fail safely; no public AI fallback |
| Disk/storage pressure | Preflight stop/admission reduction; no deletion user file |

No Silent Wait tetap berlaku. Queue atau throttling tidak boleh menghasilkan false `202`, false Completed, duplicate logical effect, atau kehilangan accepted Job.

### 13.4 Scale triggers

Trigger berbasis trend/evidence, bukan angka rekaan:

- sustained saturation dan headroom di bawah approved band;
- queue oldest age/depth tidak pulih ketika arrival seharusnya dapat dilayani;
- API/DB pool checkout wait dan latency meningkat akibat Worker;
- temp/storage/log growth melebihi cleanup throughput;
- backup/restore window tidak memenuhi target proposal;
- provider concurrency/circuit sering membatasi business flow;
- single-node maintenance/failure unacceptable bagi Pilot/Production target;
- security/network boundary membutuhkan physical separation.

## 14. Availability options

### 14.1 Decision matrix

Semua nilai relatif dan konseptual.

| Option | Cost | Complexity | Availability | Security/isolation | Migration | Operations | Capacity |
|---|---|---|---|---|---|---|---|
| **Single Node** | Low | Low awal | Low; satu failure/maintenance domain | Logical only; blast radius tinggi | Mudah jika boundary disiplin | Sederhana tetapi recovery menyeluruh | Vertical only/practical ceiling cepat |
| **Separated VMs** | Medium | Medium | Medium; komponen dapat dipelihara/dipulihkan terpisah | Network/process/data isolation lebih baik | Bertahap dari single node | Patch/backup/monitor lebih banyak | Independent Worker/API/SQL scaling |
| **Managed Services** | Medium–High, model tergantung layanan | Medium; responsibility berubah | Medium–High menurut layanan/SLA yang disetujui | Kuat bila identity/network/key/data residency benar | Data/service migration perlu diuji | Sebagian operasi dialihkan; governance tetap | Elastic/limit tergantung layanan |
| **High Availability** | High | High | High bila desain/failover diuji | Isolation/redundancy baik; complexity risk meningkat | Paling kompleks | On-call, failover, patch, DR drill matang | Horizontal/partitioned sesuai component |

### 14.2 Selection criteria

- approved business availability target;
- actual user/workload/capacity baseline;
- data classification/residency and network policy;
- SSO/private connectivity/provider requirements;
- SQL/storage consistency and recovery model;
- maintenance and deployment strategy;
- backup/DR/RPO/RTO targets;
- operational team maturity and 24x7/on-call expectations;
- cost/licensing/support constraints;
- migration portability and vendor exit;
- VA/PT/security/compliance evidence;
- failure-domain and incident history.

### 14.3 Recommendation boundary

- Hostinger KVM 2 diperlakukan sebagai `Single Node Temporary` untuk non-Production/Pilot terbatas.
- Production minimal memerlukan assessment terhadap separated/managed/HA options; laporan tidak memilih.
- HA tidak menggantikan backup/DR.
- Managed service tidak menghapus responsibility untuk authorization, data minimization, Audit, encryption, retention, dan provider governance.
- Final selection: `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION`.

## 15. Backup/DR inputs

### 15.1 Critical components

| Priority dependency | Component/data | Recovery need |
|---|---|---|
| 1 | Governance/incident authority, environment inventory, approved recovery point | Declare scope, freeze unsafe writes, authorize recovery |
| 2 | Network, DNS, certificate, time synchronization, identity connectivity | Establish trusted access and valid time |
| 3 | Secrets/keys/service identities/configuration/policy/artifact/SBOM | Recover trusted runtime without embedding secret |
| 4 | SQL Server database/log/migration history/permissions/outbox/idempotency/Audit | Restore authoritative metadata and transaction state |
| 5 | Secure object storage versions/digest/key/tombstone/hold | Restore/reconcile content objects with SQL |
| 6 | Queue/Worker/checkpoint/job leases | Reconcile accepted work; reject stale/duplicate completion |
| 7 | API/Web/SignalR | Restore authorized business access and current-state reconciliation |
| 8 | Monitoring/logging/alerting/Audit pipeline | Verify recovery and detect regression |
| 9 | Desktop/Mobile distribution/compatibility | Ensure client can reconnect safely; local source remains safe |

Order is dependency-driven and must be adjusted by approved runbook; it is not an RTO commitment.

### 15.2 Backup needs

- SQL full/differential/log or equivalent according to approved recovery model;
- object storage content/metadata/version/digest/key reference/tombstone/retention/hold;
- configuration, policy, Feature exposure, compatibility, schema/migration versions;
- immutable artifact, digest/provenance, SBOM, deployment manifest;
- secret/key recovery metadata through approved KMS/HSM/credential boundary, never embedded backup;
- DNS/certificate inventory and secure key recovery/renewal path;
- monitoring configuration, dashboards, alert rules, owner/escalation mapping;
- Audit integrity and backlog/outbox state;
- incident/runbook/contact references when officially approved;
- backup catalog, encryption, integrity verification, access audit, retention, and disposal.

### 15.3 Restore validation

Restore belum selesai sampai memverifikasi:

- backup/key integrity dan authority;
- schema/migration/constraints/module permissions/application compatibility;
- SQL/object existence/version/digest/key/tombstone/retention/legal hold reconciliation;
- outbox/idempotency/job/lease/Audit and no duplicate logical effect;
- maximum-three secured version invariant;
- deleted/tombstoned/expired data tidak menjadi addressable;
- safe application authorization and cross-user denial;
- monitoring/Audit/alert functioning;
- representative 33 Core/local/server smoke dengan synthetic data;
- measured recovery time/evidence terhadap approved target bila target sudah ada.

### 15.4 DR inputs yang belum diputuskan

- RPO/RTO per component/data class;
- backup frequency/retention;
- immutable/offline copy requirement;
- DR site/region/location/data residency;
- replication/failover topology;
- key escrow/custody/recovery;
- DNS/certificate failover;
- operator role/SoD and break-glass;
- exercise cadence and evidence;
- provider dependency and exit path.

Semua: `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` dan, untuk network/DNS/certificate, `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION`.

## 16. Security hardening

### 16.1 Identity dan least privilege

- service account terpisah untuk Web, API, Worker, SQL runtime, migration, backup/restore, monitoring, dan deployment;
- workload identity tidak memakai role/session user;
- no shared admin credential atau default credential;
- deployment identity berbeda dari developer/runtime identity;
- Admin/Support/Auditor/DBA/backup operator tidak otomatis mendapat document content permission;
- reauthentication untuk high-impact control-plane action;
- credential/secret rotation and revocation path tersedia.

### 16.2 Host/process hardening

- approved supported OS/runtime baseline; OS final tidak dipilih di sini;
- security patching dengan test, staged rollout, rollback/forward-fix;
- process/service isolation and resource limits;
- no shared writable root antar-process kecuali explicit authorized channel;
- Worker restricted dari arbitrary SQL, filesystem, shell, process, registry, network, provider, dan secret;
- no debug/development mode di Pilot/Production concept;
- endpoint protection/malware/document quarantine sesuai approval;
- no startup migration Production;
- no manual in-place binary/config patch.

### 16.3 Network hardening

- HTTPS/TLS only dan full certificate validation;
- inbound/outbound least privilege firewall;
- no public SQL/storage/queue/management/detailed-health endpoint;
- restricted admin route;
- exact proxy/Host/Origin/CORS/WebSocket validation;
- egress provider allowlist; no public AI;
- rate limits/body/header/upload/timeouts and DoS/backpressure controls;
- approved synchronized time.

### 16.4 Data and file hardening

- SQL metadata only; content in private secure storage;
- storage zones/ACL/service identity/purpose separated;
- encryption in transit/at rest/backup;
- key separated from data and only injected/retrieved through approved boundary;
- random opaque object names and digest/state validation;
- temp/cache/checkpoint quota and lifecycle;
- lease/retention/hold-aware cleanup;
- backup separate from active data;
- no source overwrite or partial final promotion;
- file permission and path/reparse/traversal defenses.

### 16.5 Logging, monitoring, Audit

- structured safe fields: timestamp, environment/component/version, `FeatureId`, `JobId`, `CorrelationId`, status, duration, dan safe counters;
- never log content, OCR, prompt, password, token, secret, signature, clipboard, raw path/key, raw stack/SQL/body;
- protect access, transport, at-rest storage, rotation, retention, export;
- monitor auth/revocation, authorization probes, queue/Worker/storage/DB/provider pressure, backup/cleanup, public-AI block, version drift;
- Enterprise Audit remains separate and cannot be disabled to improve performance;
- clock/freshness, log pipeline failure, and telemetry gaps alert safely.

### 16.6 Supply chain and deployment

- protected branch/PR/human review;
- controlled restore, release build, unit/integration/contract/security tests;
- secret/dependency/static/config/API/deployment scans;
- approved artifact digest/provenance/SBOM;
- same binary promoted; environment value injected;
- emergency change accelerated but tracked, tested, approved, monitored, and post-reviewed;
- VA/PT before Production and after material attack-surface change according to approval.

## 17. Pertamina confirmation gaps

### 17.1 Network confirmations

| ID | Confirmation | Tag |
|---|---|---|
| `NET-GAP-01` | Domain/DNS zone ownership, naming, record approval, propagation/failover | `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION` |
| `NET-GAP-02` | Certificate authority, key custody, TLS versions/ciphers, renewal/revocation | `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION` |
| `NET-GAP-03` | Public/private ingress topology, reverse proxy/gateway/WAF/load balancer selection | `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION` |
| `NET-GAP-04` | Official protocol/port matrix without inventing values | `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION` |
| `NET-GAP-05` | Source IP/network allowlist, VPN/private access/admin route/bastion equivalent | `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION` |
| `NET-GAP-06` | SQL/storage/queue/monitoring private connectivity and firewall rules | `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION` |
| `NET-GAP-07` | SSO/provider/monitoring/update egress allowlist and proxy requirements | `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION` |
| `NET-GAP-08` | No-public-AI technical egress enforcement and verification | `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION` |
| `NET-GAP-09` | Approved time/NTP source, clock monitoring, and firewall rule | `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION` |
| `NET-GAP-10` | DDoS/edge protection, rate limits responsibility, and incident escalation | `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION` |
| `NET-GAP-11` | Hostinger Pilot exposure and whether platform/network is approved | `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION` |

### 17.2 Infrastructure confirmations

| ID | Confirmation | Tag |
|---|---|---|
| `INFRA-GAP-01` | Final Production hosting and physical topology | `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` |
| `INFRA-GAP-02` | OS, host image, runtime hosting model, container applicability | `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` |
| `INFRA-GAP-03` | SQL Server edition/licensing/HA/recovery model/topology | `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` |
| `INFRA-GAP-04` | Secure object storage product, encryption/key integration, data residency | `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` |
| `INFRA-GAP-05` | Queue implementation/placement/durability/workload isolation | `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` |
| `INFRA-GAP-06` | Monitoring backend connectivity, retention, access, alert owner | `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` |
| `INFRA-GAP-07` | Secret/KMS/HSM/credential service, custody, rotation, recovery | `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` |
| `INFRA-GAP-08` | Environment count/isolation and whether dedicated Dev/QA/Pilot stacks are required | `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` |
| `INFRA-GAP-09` | Hostinger KVM 2 eligibility for Shared Dev/QA/Pilot and data allowed | `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` |
| `INFRA-GAP-10` | User/workload baseline, concurrency, quota, cleanup, headroom, scale trigger | `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` |
| `INFRA-GAP-11` | Availability target, maintenance window, on-call/operations model | `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` |
| `INFRA-GAP-12` | Backup schedule/retention/target, off-node/immutable/offline copies | `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` |
| `INFRA-GAP-13` | RPO/RTO, DR site/region, failover, restore exercise cadence | `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` |
| `INFRA-GAP-14` | Data classification/residency/masking and Production-to-non-Production policy | `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` |
| `INFRA-GAP-15` | Patching, endpoint protection, vulnerability scan, VA/PT, artifact signing | `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` |
| `INFRA-GAP-16` | Deployment/rollback/migration/restore authority, SoD, emergency path | `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` |

Tidak adanya konfirmasi tidak mengizinkan weak fallback. SQL/storage tetap private, HTTPS tetap wajib, public AI tetap blocked, secrets tetap tidak boleh di-hardcode, dan Production tidak boleh diasumsikan.

## 18. Risks/conflicts

| ID | Risiko/konflik | Dampak | Mitigasi/rekomendasi |
|---|---|---|---|
| `R-17A-01` | Spesifikasi Hostinger plan dapat berubah | Sizing dokumentasi cepat usang | Verifikasi hPanel pada provisioning dan rekam manifest; jangan menjadikan angka sebagai contract |
| `R-17A-02` | KVM 2 single-node adalah satu failure/maintenance domain | Web/API/Worker/SQL/storage/monitoring dapat gagal bersama | Hanya temporary non-Prod/Pilot terbatas; backup off-node; readiness/recovery; exit triggers |
| `R-17A-03` | 2 vCPU dibagi engine HEAVY, API, SQL, collector | Starvation, latency, queue buildup | Conservative HEAVY admission/serialization candidate, per-process budget, measure and separate Worker |
| `R-17A-04` | RAM 8 GB dibagi SQL/Worker/Web/API | OOM/recycle dapat menurunkan seluruh node | Streaming, bounded memory, per-process monitoring, safe reject/wait |
| `R-17A-05` | 100 GB menampung DB/log/temp/object/log/staging | Disk-full, corruption risk, cleanup pressure | Zone quota, growth alert, short temp lifecycle, external backup/storage migration trigger |
| `R-17A-06` | I/O dan disk shared antara SQL, OCR/conversion, backup, cleanup | Noisy-neighbor dan unpredictable latency | Schedule/isolate heavy work, monitor disk queue/latency, separate data plane when measured |
| `R-17A-07` | Bandwidth 8 TB adalah quota, bukan throughput/SLA | Transfer performance/availability dapat diasumsikan berlebihan | Measure throughput/retry/resume; staged rollout; no SLA claim |
| `R-17A-08` | Vendor whole-VPS restore overwrites current node and may not be application-consistent | Data loss atau SQL/object mismatch | Approved recovery point, separate backup, SQL/object/key/config reconciliation, restore rehearsal |
| `R-17A-09` | One public address tidak menyediakan internal segmentation fisik | Management/SQL/storage exposure risk | Strict firewall/process identity; restricted admin; move to separated/private topology when required |
| `R-17A-10` | Hostinger approval, data residency, security control, support route tidak diketahui | Pilot mungkin melanggar Pertamina policy | `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` sebelum data/user pilot |
| `R-17A-11` | OS/runtime/container/gateway/load balancer belum dipilih | Procedure implementation belum lengkap | Pertahankan vendor-neutral logical requirements; controlled decision kemudian |
| `R-17A-12` | SQL Server edition/license/platform feasibility pada KVM belum dikonfirmasi | Deployable mungkin tidak legal/supported/cukup resource | Licensing/platform PoC dan approval; jangan mengganti database |
| `R-17A-13` | Capacity/user/concurrency/retention target belum ada | Over/under-provisioning dan false readiness | Measured baseline → target proposal → approval → policy |
| `R-17A-14` | Co-hosting multiple environment/lane dapat mencampur secret/data/log dan fault load | Cross-environment exposure atau invalid test | Separate identity/config/data/storage/telemetry; prefer separate instance/lane; no QA stress with Pilot |
| `R-17A-15` | Local-only Feature dapat diarahkan ke server demi utilisasi | Melanggar Local-First dan privacy | Zero processing API/upload test; Resolver/policy; server route hanya approved |
| `R-17A-16` | Provider/network outage dapat mendorong public AI fallback | Data/security violation | Egress allowlist, technical block, circuit/fail safely, preserve local path |
| `R-17A-17` | Monitoring co-host dapat hilang saat node gagal | Incident tanpa evidence/freshness | Export safe telemetry to approved separate sink; health heartbeat/external check setelah approval |
| `R-17A-18` | Backup disimpan pada active node | Node loss menghilangkan active dan backup | Separate failure-domain backup; encryption/integrity/access control |
| `R-17A-19` | HA dianggap menggantikan DR/backup | Corruption/operator/security event ikut direplikasi | Pisahkan HA, backup, DR, retention/hold, and recovery validation |
| `R-17A-20` | Emergency Incident mendorong direct patch/restart/migration/restore | Untracked state/data/security damage | Approved Deployment Procedure; accelerated but tracked, tested, authorized, monitored, post-reviewed |
| `R-17A-21` | Production topology dipilih sebelum load/security/recovery evidence | Cost atau risk tidak sesuai | Gunakan decision matrix dan gates; final selection oleh Pertamina |
| `R-17A-22` | RPO/RTO/SLA diambil dari vendor backup schedule | Komitmen palsu | Ukur restore; target tetap `TARGET_TO_BE_APPROVED`; jangan infer SLA |

Tidak ditemukan konflik yang membenarkan database tambahan, public AI, direct SQL client, atau Hostinger sebagai Production final. Komponen dan logical boundary yang sudah locked tetap dipertahankan.

## 19. Suggested Document 17 text

Teks berikut dapat digunakan sebagai baseline bagian **Environment, Infrastructure, Network & Capacity** pada master Document 17 setelah confirmation gaps dan design approval diselesaikan.

---

### Environment Strategy

PERTAMINA OTO memiliki Local Development, Shared Development, QA/Testing, Pilot, dan Production. Environment memisahkan endpoint, identity, credential, service account, configuration, SQL data, secure storage, queue, provider, logs/telemetry, backup, dan deployment authority.

Artefak yang sama dipromosikan melalui pipeline. Nilai environment dan secret diinjeksikan terpisah. Development menggunakan data sintetis. Data Production tidak disalin ke non-Production tanpa masking, authorization, dan persetujuan.

Local Development digunakan untuk unit/component/contract feedback dan Local Worker. Shared Development untuk integrasi awal. QA untuk system/security/recovery/performance verification. Pilot untuk penggunaan terkontrol dan capacity measurement. Production topology belum dipilih.

Satu server sementara boleh co-host Web, API, Worker, SQL, storage, dan collector hanya jika setiap process memiliki identity, configuration, log, health, data/storage zone, dan resource limit terpisah. Co-hosting tidak menghapus logical separation.

### Logical Topology

```text
Client
→ HTTPS + approved Pertamina Identity
→ client-facing Web/API/SignalR boundary
→ application authorization and modules
→ persisted Job + bounded queue
→ separate-identity Server Worker
→ internal SQL Server / private Secure Storage
→ separate Monitoring and Enterprise Audit purposes
```

SQL, storage, queue, Worker, admin management, detailed health, dan monitoring backend tidak public. SignalR hanya notification; REST/persisted state tetap source of truth.

Desktop/Mobile memakai protected local profile. Local Worker terisolasi melalui typed/versioned local IPC dan tidak mempunyai network arbitrer. Local-only Feature tidak menggunakan processing API atau upload.

### Development

Local Development memakai synthetic data dan approved developer credential store. Shared Development dapat menggunakan satu node sementara dengan Web/API/Worker/SQL/storage/monitoring terpisah secara logis. No Production data, public AI, unbounded load, atau direct data patch.

Shared environment harus mempunyai artifact/config manifest, separate service identities, safe logging, process health, DB migration compatibility, cleanup, recovery point, dan resource baseline per process.

### QA/Testing

QA mempunyai SQL, storage, queue, provider, identity fixtures, logs, dan evidence terpisah. QA dapat menjalankan Integration, E2E, Security, Recovery, bounded Performance/Load/Stress/Soak, fault injection, backup/restore, dan migration rehearsal.

Fault injection hanya pada non-Production dengan synthetic data, exact target, owner, blast radius, stop condition, monitoring, cleanup, dan recovery point. Destructive test tidak berbagi node dengan active Pilot.

### Pilot

Pilot adalah lane terkontrol, bukan Production. Participant, Feature exposure, data, workload, dan test dibatasi. Pilot memakai staged rollout, queue/backpressure, monitoring/alerts, backup/recovery, incident route, dan rollback/disable path.

Pilot merekam actual users/request/job mix, CPU/RAM/disk/network, SQL/storage, queue/drain, cleanup growth, failure/retry, backup/restore, bottleneck, dan headroom. Tidak ada jumlah user yang diasumsikan.

### Hostinger KVM 2 Temporary Profile

Pada 1 September 2026, sumber resmi Hostinger mencantumkan KVM 2: 2 vCPU, RAM 8 GB, NVMe 100 GB, I/O 300 MB/s, bandwidth 8 TB, dan satu IPv4/IPv6. Plan dapat berubah; verifikasi hPanel saat provisioning.

KVM 2 hanya dievaluasi untuk Shared Dev, QA, dan Pilot terbatas. Ia tidak dinyatakan cukup untuk Production.

Pada node tersebut:

- Web, API, dan Worker adalah process terpisah;
- service identity, configuration, secret, log, dan health terpisah;
- SQL tidak public dan memakai identity runtime/migration/backup terpisah;
- storage mempunyai quarantine/input/temp/result/secured/issue/export zones;
- Worker mempunyai CPU/RAM/time/temp/concurrency limits;
- queue bounded dan dipisahkan menurut workload;
- HEAVY Job dimulai dengan admission paling konservatif dan diukur sebelum concurrency dinaikkan;
- free disk, temp, SQL/log, object growth, queue age/drain, dan cleanup dipantau;
- backup akhir berada di luar active node.

Hostinger backup/snapshot adalah whole-server recovery. Restore dapat menimpa current server; ia tidak menggantikan SQL/object/config/key-aware backup dan reconciliation test.

### Production Options

Empat opsi dipertimbangkan tanpa memilih final:

1. Single Node — biaya/complexity rendah, availability/isolation rendah.
2. Separated VMs — isolation dan independent scaling lebih baik.
3. Managed Services — sebagian operasi dikelola tetapi governance/data/network tetap wajib.
4. High Availability — availability tertinggi dengan cost/complexity/operations maturity tertinggi.

Pemilihan mempertimbangkan cost, complexity, availability, security, migration, operations, capacity, data residency, RPO/RTO, licensing, and measured workload. Final selection adalah `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION`.

### Network and TLS

- HTTPS only dan full certificate validation;
- minimum client-facing endpoints;
- SQL/storage/queue/management/detailed health private;
- SignalR authenticated dan notification-only;
- admin route restricted dan high-impact action reauthenticated;
- inbound/outbound firewall least privilege;
- approved provider egress allowlist;
- public AI endpoint blocked;
- controlled DNS/certificate issuance, inventory, renewal, rotation, revocation, and recovery;
- approved time synchronization and clock monitoring.

Domain, IP, port matrix, gateway/proxy, private access, allowlist, certificate authority/TLS profile, DNS ownership, egress destinations, dan time source adalah `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION`.

### SQL and Secure Storage

SQL Server hanya menyimpan metadata/state dan tidak diakses client. Byte document disimpan pada private Secure File/Object Storage, bukan SQL Blob default. SQL runtime, migration, backup, dan administration identities terpisah.

Storage menggunakan opaque reference, owner/purpose/type/size/digest/state/key reference/retention, serta zone quarantine, validated input, processing, result, secured version, Issue attachment, dan export. Backup berada pada failure domain terpisah dari active data, encrypted, integrity-verified, dan least privilege.

### Monitoring

Gunakan Microsoft.Extensions.Logging, OpenTelemetry, dan approved Azure Monitor/Application Insights integration. Pantau host/process, API, Worker, queue, SQL, storage, SignalR, Audit, Issue, security signals, backup, cleanup, dan deployed versions.

Monitoring tidak menyimpan document/OCR/prompt/password/token/secret/signature/path/raw diagnostic. Unknown/stale critical telemetry bukan healthy. Alert mempunyai environment, freshness, threshold source, owner, dan escalation route setelah disetujui.

### Capacity and Backpressure

Klasifikasi:

- `LIGHT`: metadata API, normal authorization, small Issue metadata;
- `MEDIUM`: file transfer, screenshot, secured files, bounded query/export;
- `HEAVY`: conversion, OCR, render, large export/cleanup;
- `EXTERNAL/INTERNAL_PROVIDER_DEPENDENT`: approved AI/provider route.

Gunakan local processing first, streaming, bounded queue, weighted admission, concurrency/resource limits, quotas, backoff/jitter, cleanup, retention, staged rollout, and capacity monitoring.

Overload menghasilkan explicit wait/reject: `413`, `429`, `409`, atau `503` sesuai contract. Tidak ada false `202`, silent wait, duplicate effect, public AI fallback, atau penghapusan Feature hanya karena server kecil.

Nilai capacity melalui `TARGET_TO_BE_BASELINED → MEASURED_BASELINE → TARGET_TO_BE_APPROVED → POLICY_CONFIGURABLE`.

### Availability and DR

HA tidak menggantikan backup/DR. Recovery order memulihkan authority/network/time, secret/key/config/artifact, SQL, object storage, queue/Worker, API/Web, dan monitoring; urutan final mengikuti approved runbook.

Backup mencakup SQL/log, object/version/digest/key/tombstone/hold, configuration/policy/schema, artifact/SBOM, secret/key recovery metadata, DNS/certificate inventory, monitoring config, dan Audit/outbox. Restore memverifikasi cross-store consistency, authorization, max-three secured versions, tombstone, no duplicate effect, monitoring, Audit, dan synthetic smoke.

RPO/RTO, backup schedule/retention, immutable/offline copy, DR site/region, failover, key escrow, operator authority, dan drill cadence tidak ditentukan tanpa Pertamina approval.

### Security Hardening

- least-privilege service account per deployable/purpose;
- supported patched host/runtime;
- secure protocols and certificate validation;
- restricted administration and firewall;
- no default/shared credential or public management port;
- process/file/storage zone isolation;
- secret injection from approved boundary;
- encryption in transit/at rest/backup;
- protected safe logs and monitoring;
- synchronized time;
- artifact/SBOM/scans/human review;
- controlled migration/rollback/restore;
- Enterprise Audit tidak dinonaktifkan;
- emergency change accelerated but tracked and reviewed.

---

Sebelum teks ini masuk master Document 17, selesaikan seluruh `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION` dan `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION`, verifikasi spesifikasi Hostinger pada hPanel, tetapkan data/network/security approval untuk Pilot, dan validasi topology melalui load, recovery, security, and deployment rehearsal.
