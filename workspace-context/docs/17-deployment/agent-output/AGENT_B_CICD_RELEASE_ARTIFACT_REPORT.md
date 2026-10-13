# AGENT B — AZURE DEVOPS CI/CD, RELEASE & ARTIFACT GOVERNANCE REPORT

**Task ID:** `DOC17-AGENT-B`  
**Scope:** Source control, build/test/security pipeline, artifact governance, promotion, approval, deployment concept, rollback, hotfix, dan release evidence  
**Status:** Rekomendasi desain; bukan pipeline produksi, bukan persetujuan rilis, dan bukan bukti deployment  
**Source of truth:** Document 01–16 FINAL, terutama Document 05, 08, 11, 12, 13, dan 16

Laporan ini mempertahankan baseline berikut: Azure DevOps Git/Pipeline, structured monorepo, task branch/worktree berumur singkat, `.NET 10 LTS` dengan patch SDK yang disetujui dikunci di `global.json`, Central Package Management melalui `Directory.Packages.props`, MSTest + Microsoft.Testing.Platform, security scanning berlapis, environment terpisah, serta **build once—promote the same verified artifact**. Codex tidak menjadi approver dan tidak melakukan deployment Production.

Kondisi saat laporan dibuat: spesifikasi UAT sudah FINAL, tetapi pelaksanaan UAT dan sign-off release candidate belum direkam. Oleh karena itu laporan ini tidak menyatakan aplikasi, 33 Core Tools, release candidate, atau Production telah siap/lulus.

---

# 1. Source-control workflow

## 1.1 Model yang direkomendasikan

Gunakan alur trunk-based ringan dengan `main` sebagai satu-satunya garis integrasi dan kandidat sumber release. Jangan membentuk Git Flow multi-cabang yang tidak sebanding dengan kondisi satu developer + Codex.

```text
Task/Incident/Defect yang disetujui
→ task branch + isolated worktree
→ perubahan scoped + test/evidence lokal
→ Azure DevOps PR validation
→ human review
→ merge ke protected main
→ main validation + immutable artifact build
→ QA promotion/test
→ release-candidate designation
→ UAT readiness + human UAT sign-off
→ separate Production approval
→ promote artifact yang sama
```

Prinsip:

- satu branch/worktree mewakili satu Task ID dan scope yang dapat ditinjau;
- branch berumur singkat, tidak menjadi jalur release jangka panjang;
- perubahan kontrak, dependensi, arsitektur, konfigurasi keamanan, atau fondasi bersama hanya dilakukan bila task secara eksplisit mengizinkan;
- hasil kerja Codex tidak masuk `main` tanpa review manusia;
- hanya commit yang telah terintegrasi ke `main` dan lolos pipeline resmi dapat menjadi sumber release artifact;
- setiap release menautkan Task/Requirement/Feature/Defect/Incident yang relevan, commit, pipeline run, artifact digest, version, dan change summary;
- tidak ada direct Production patch, untracked binary copy, startup migration Production, atau rebuild manual dari workstation.

## 1.2 Identitas traceability

| Objek | Identitas minimum | Aturan |
|---|---|---|
| Task branch/worktree | Task ID + slug singkat | Nama persis mengikuti kebijakan repository yang disetujui; tidak mengandung data sensitif |
| Commit | Commit ID + Task ID pada metadata/PR | Pesan tidak memuat secret, document data, exploit detail, atau data pribadi |
| Pull Request | Task ID, scope, requirement/Feature/defect link, change summary, risk, test evidence | Missing traceability memblokir merge |
| Build | Immutable Build ID/BuildVersion + CommitId | Satu build tidak boleh menunjuk ke commit yang berubah |
| Artifact | Artifact ID/name/version + strong digest + build reference | Publication immutable/restricted; overwrite nama yang sama dilarang |
| Release candidate | ProductVersion candidate + BuildVersion + artifact-set manifest | Candidate adalah designation atas artifact yang sudah dibangun, bukan rebuild |
| Release | Release ID + approved version/tag + environment + promoted artifact-set digest | Release tag menunjuk exact integrated commit dan release record |

Exact branch/tag/version naming convention belum ditetapkan oleh sumber. Rekomendasi pola dapat berupa `task/<TASK-ID>-<slug>` dan tag versi produk yang terhubung ke Release ID, tetapi pola final memerlukan konfirmasi repository governance Pertamina.

---

# 2. Branch/PR controls

## 2.1 Protected `main`

`main` harus dilindungi dengan kontrol minimum berikut.

| Control | Rekomendasi | Kegagalan |
|---|---|---|
| Direct push | Ditolak; perubahan melalui PR | Merge/release diblokir |
| Force push/history rewrite | Ditolak | Aktivitas diperlakukan sebagai governance/security incident sesuai proses resmi |
| Human reviewer | Wajib; reviewer aktual dicatat | Codex atau pipeline tidak dapat menggantikan approval |
| PR validation | Build, Unit, Static/Security, Architecture, Contract, traceability | Required check gagal/absen = tidak dapat merge |
| Discussion resolution | Required concern diselesaikan atau mempunyai explicit disposition | Open blocking discussion = tidak dapat merge |
| Task scope | Allowed paths dan dampak shared contract/dependency diperiksa | Scope creep dikembalikan ke task atau task baru |
| Dependency change | Purpose, source, license, security, maintenance, compatibility, approval | Undeclared/unapproved dependency = block |
| Generated files | Hanya file yang memang dikendalikan source; release binary tidak dikomit | Binary/manual artifact tidak menjadi release source |
| Merge policy | Strategi sederhana dan konsisten; hasil merge memiliki traceability | Exact merge strategy menunggu kebijakan Pertamina |
| Bypass | Exceptional, time-bounded, logged, named authority, reason, post-review | Bukan hak Codex/developer otomatis |

## 2.2 PR template minimum

PR menyertakan:

1. Task ID, Requirement ID, canonical Feature ID, Defect/Incident ID bila relevan.
2. Tujuan dan change summary; area yang sengaja tidak diubah.
3. Risiko business/security/privacy/data/compatibility/deployment.
4. Perubahan dependency, database, API/IPC/event contract, configuration, capability, atau feature flag.
5. Test yang ditemukan, dijalankan, PASS/FAIL/BLOCKED/NOT_APPLICABLE, serta evidence aman.
6. Dampak release notes, migration, rollback, dan support/incident runbook.
7. Konfirmasi tidak ada secret/document content/password/signature/OCR/prompt/clipboard di diff, log, atau evidence.
8. Human review outcome dan unresolved limitation.

Karena hanya ada satu developer, proses tidak boleh mengarang beberapa tim/reviewer. Satu human integrator dapat melakukan review teknis sehari-hari jika diizinkan, tetapi perubahan security-sensitive dan Production approval mengikuti authority resmi yang terpisah bila kebijakan mensyaratkannya.

---

# 3. Build pipeline

## 3.1 Pipeline layers

Pipeline menggunakan clean, controlled agent/image, approved package sources, serta tool versions yang dipin. Tidak boleh bergantung pada path/tool yang hanya ada di workstation developer.

| Urutan | Stage | Tujuan dan kontrol minimum | Output/evidence | Gate |
|---:|---|---|---|---|
| 1 | Source/trace validation | Validasi branch policy, Task/PR metadata, commit, change scope, forbidden generated binary | Traceability summary | Missing/invalid = FAIL |
| 2 | SDK/version validation | Cocokkan SDK dengan `global.json`; validasi Central Package Management dan tidak ada floating/silent override | Resolved tool/SDK/package-source metadata aman | Mismatch/unapproved source = FAIL |
| 3 | Controlled restore | Restore direct/transitive dependency dari source yang disetujui; cache tidak boleh melewati integrity | Resolved dependency graph | Restore/integrity failure = FAIL |
| 4 | Release build | Build konfigurasi release dengan analyzer dan reproducible/deterministic setting sejauh didukung | Build log tersanitasi + component outputs | Compile/analyzer policy failure = FAIL |
| 5 | Unit test | MSTest pada Microsoft.Testing.Platform; deteksi zero/missing/ignored/reduced tests dan flakiness | Machine + human-readable result | Applicable required test gagal/absen = FAIL |
| 6 | Static analysis | Secure coding, unsafe API/source/config rule, analyzer sesuai policy | Finding summary tanpa source sensitif berlebihan | Disallowed finding/tool failure = FAIL/BLOCKED |
| 7 | Secret scan | Source, config, test fixture, generated output, log candidate, package manifest | Redacted finding reference | Confirmed secret = STOP; rotate/revoke workflow bila nyata |
| 8 | Dependency audit | Known vulnerabilities direct/transitive, provenance/source, outdated policy bila berlaku | Dependency/vulnerability summary | Critical dan policy-disallowed finding = STOP |
| 9 | License/policy check | Jalankan bila tool/data policy tersedia; cocokkan approved license/source | License/policy summary | Unknown/disallowed = BLOCKED, bukan PASS |
| 10 | Architecture test | Module/dependency direction dan boundary shared architecture | Test result | Required violation = FAIL |
| 11 | Contract test | OpenAPI/DTO/error/version, IPC/event, config/capability, job/recovery compatibility sesuai perubahan | Compatibility report | Unapproved breaking change = FAIL |
| 12 | Package | Bentuk artifact per deployable/component; exclude secrets, local/user data, test residue | Staged packages | Unexpected/missing content = FAIL |
| 13 | SBOM | Buat SBOM dari resolved graph bila format/tool telah disetujui | SBOM linked to build | Required SBOM/tool failure = BLOCKED/FAIL sesuai policy |
| 14 | Artifact hash | Hitung strong digest untuk setiap artifact dan release manifest | Digest manifest | Missing/mismatch = STOP |
| 15 | Publish artifact | Publikasikan immutable, restricted, versioned artifacts + evidence | Artifact IDs/URIs tanpa bearer token | Overwrite/publication failure = FAIL |

## 3.2 Log dan secret hygiene

- Environment-specific secret diambil saat runtime/deployment melalui identitas least-privilege yang disetujui; bukan embedded pada source/artifact/config template.
- Pipeline tidak melakukan echo terhadap secret, token, connection string, certificate material, SAS/bearer URL, document content, password PDF, signature image, OCR text, clipboard, atau AI payload.
- Diagnostic verbosity tetap tunduk pada allowlist; masking string bukan satu-satunya kontrol.
- Secret scan finding menyimpan fingerprint/reference dan lokasi terbatas, bukan menyalin nilai secret ke release evidence.
- Scanner atau pipeline task yang gagal tidak boleh diubah menjadi green dengan `continue-on-error` untuk gate wajib.
- Retry hanya untuk kegagalan infrastructure/transient yang diklasifikasikan; retry tidak menghapus failure awal atau mengganti evidence.

## 3.3 Pipeline-as-code boundary

YAML/template pipeline direview seperti source code. Perubahan pada service connection, environment approval, variable group, agent pool/image, scan exclusion, retention, signing, dan Production deployment permission diperlakukan sebagai perubahan privileged dan bukan task aplikasi biasa.

---

# 4. Test stages

## 4.1 Risk-based lanes

| Lane/trigger | Test minimum | Tujuan | Hasil yang memblokir |
|---|---|---|---|
| Local task | Affected build/test, lint/analyzer relevan, negative case perubahan | Feedback cepat sebelum PR | Failure harus dilaporkan; tidak boleh disembunyikan |
| Per commit/PR | Controlled restore, Release build, Unit fast suite, static/secret/dependency, Architecture, fast Contract, affected regression | Merge gate cepat dan deterministik | Required test/scanner fail, zero/missing tests, unapproved dependency/contract |
| Main | Full Unit, broader Integration/Contract yang terpengaruh, SBOM/digest, Core smoke | Membentuk artifact set authoritative | Build/integrity/critical boundary failure |
| QA promotion | Deployment/config/migration/health smoke; Integration; DB migration test; API; Storage; Worker/IPC; selected E2E; authorization/security; affected regression | Membuktikan artifact dan configuration di environment terkontrol | Wiring, ownership, migration, smoke, security, recovery failure |
| Scheduled | Broader E2E/platform, malicious corpus, recovery, compatibility, bounded load/soak, dependency rescan | Menemukan risiko mahal tanpa memperlambat setiap PR | Finding ditautkan ke affected artifact/release; status tidak hilang |
| Release candidate | Full required regression, security, performance baseline/approved target, recovery, compatibility, platform, UAT readiness | Entry gate ke UAT | Mandatory gate FAIL/BLOCKED atau evidence kurang |
| Post-deploy | Deployment smoke, health/readiness, critical route, authorization isolation, monitoring observation | Verifikasi environment tanpa memakai data sensitif | Deployment tidak dinyatakan berhasil bila smoke/monitoring gagal |

## 4.2 Status gate

Hanya gunakan `PASS`, `FAIL`, `BLOCKED`, atau `NOT_APPLICABLE` dengan evidence.

- `NOT_RUN`, `INCONCLUSIVE`, quarantined, missing, flaky, rerun-only-success, atau target belum disetujui bukan `PASS`.
- Jika target numerik performance/recovery belum disetujui, simpan `MEASURED_BASELINE` dan status `BLOCKED_PENDING_TARGET_APPROVAL` untuk gate yang memang membutuhkan target.
- PR menggunakan suite cepat; shared auth/registry/storage/Audit/Worker/contract changes wajib memicu suite lebih luas daripada path filtering biasa.
- UAT readiness hanya berarti entry package siap. UAT ACCEPTED dan Production approval merupakan keputusan manusia terpisah.
- Test aktif yang bersifat attack/load/fault tidak dijalankan pada Production.

---

# 5. Security gates

## 5.1 Decision matrix

| Kondisi | Default result | Tindakan berikutnya | Waiver |
|---|---|---|---|
| Build/required Unit gagal | `FAIL` | Perbaiki dan bangun artifact baru | Tidak boleh diubah menjadi PASS administratif |
| Secret terkonfirmasi | `STOP` | Batasi evidence, revoke/rotate bila nyata, review history/artifact, incident route sesuai dampak | Secret aktif tidak dipromosikan; Codex tidak membuat waiver |
| Critical vulnerability | `STOP` | Patch/replace/remove dependency atau explicit governed risk decision | Hanya authority resmi; policy dapat menetapkan non-waivable |
| High vulnerability | `STOP` menurut baseline Document 08 kecuali controlled exception | Evaluasi exploitability/exposure/compensating control | Explicit, scoped, expiring, approved |
| Security-critical test gagal | `STOP` | Fix + exact retest + affected regression | Tidak boleh ditutupi manual PASS |
| Unauthorized access/content leak/source corruption/false success | `STOP` | Incident/defect + correction + full relevant security/recovery evidence | Tidak dapat dianggap UAT known issue yang dapat diterima |
| Unapproved breaking contract | `FAIL` | Sediakan compatible version/path atau controlled approval | Approval harus menyebut consumer dan migration window |
| Artifact/SBOM/digest/provenance mismatch | `STOP` | Quarantine artifact, investigate build/publication chain, rebuild clean bila perlu | Artifact berintegritas gagal tidak dipromosikan |
| Required scanner/tool unavailable | `BLOCKED` | Restore tool/service atau pakai approved equivalent | Tidak boleh silent skip |
| License/policy unknown | `BLOCKED` | Review dependency/source/license | Tidak sama dengan PASS |
| Required VA/PT/security evidence missing | `BLOCKED` | Jalankan approved assessment sesuai scope | Tidak dapat diganti oleh static scan saja |

## 5.2 Waiver contract

Waiver bukan perubahan hasil test menjadi PASS. Waiver record minimum:

- `WaiverId`, finding/gate reference, affected component/artifact/digest/release/environment;
- severity dan scope yang dibatasi;
- alasan bisnis/teknis yang faktual;
- named owner, approving authority, approval time, dan source policy;
- compensating controls dan monitoring;
- expiry/review time serta follow-up Task/Defect;
- evidence location/classification dan final disposition.

Codex tidak membuat, menyetujui, memperpanjang, atau memalsukan waiver. Exact severity scoring, non-waivable category, waiver authority, dan expiry limit memerlukan kebijakan Pertamina. Baseline tidak bergantung pada paid extension; add-on berbayar hanya setelah license/security/data-residency approval.

---

# 6. Artifact catalog

Artifact dipublikasikan terpisah agar deployment, rollback, permission, dan retention dapat dikontrol per komponen. Satu release menggunakan **Release Manifest** yang mengikat seluruh artifact berikut.

| Artifact | Isi/tujuan | Metadata/integrity minimum | Larangan/catatan |
|---|---|---|---|
| Web Artifact | Static/server-hosted Web deliverable sesuai arsitektur | Product/Build/Commit, component version, digest, supported config/contract | Tidak berisi environment secret atau runtime user data |
| API Artifact | ASP.NET Core API deployable | API ContractVersion, Product/Build/Commit, digest, dependency/SBOM link | Tidak mengandung Production config/credential |
| Worker Artifact | Server Worker/background job deployable | Worker/capability/contract compatibility, Build/Commit, digest | Mixed-version queue/job compatibility wajib diuji |
| Desktop Package | WinUI Desktop distributable sesuai packaging approval | DesktopVersion, Product/Build/Commit, digest, supported LocalWorker/API/DB profile versions | Exact signing/distribution format belum dipilih |
| Local Worker Package | Official local Worker executable/package | LocalWorkerVersion, IPC/CapabilityVersion, compatible Desktop range, digest | Harus profile-scoped/least privilege; bukan server Worker |
| Mobile Package | MAUI Phone/Tablet package per platform yang disetujui | MobileVersion, Product/Build/Commit, supported API/capability, digest | Signing/store/MDM distribution memerlukan keputusan terpisah |
| Database Migration Bundle | Reviewed, idempotent migration artifact + manifest/preflight/verification/recovery notes | DatabaseMigrationVersion, from/to compatibility, Build/Commit, digest | Tidak dijalankan saat application startup Production; destructive rollback tidak diasumsikan |
| Configuration Template | Typed keys, schema/default safe bounds, environment placeholders | ConfigurationVersion/schema, compatible component range, digest | Tidak memuat secret, endpoint sensitif publik, atau nilai Production |
| OpenAPI Contract | Governed API contract snapshot | API ContractVersion, compatibility result, Build/Commit, digest | Breaking contract tanpa path/approval memblokir release |
| Release Notes | User/admin/support-facing perubahan dan known limitation | ProductVersion, Release ID/date, commit/build/artifact-set reference | Tidak memuat secret, exploit detail sensitif, document data |
| SBOM | Direct/transitive component/source/supplier/license sesuai data tersedia | Format/tool version, Build/Commit/artifact link, digest | Format/signing/provenance attestation menunggu approval |
| Test/Security Evidence | Gate summaries, test/scanner results, environment manifest, waivers references | Evidence ID, build/artifact digest, tool version, classification/retention | Restricted evidence dipisah; link tidak mengandung token/SAS |
| Release Manifest | Indeks authoritative seluruh artifact/version/hash/compatibility/evidence | Release/Build/Commit, artifact-set digest, timestamps | Manifest immutable; perubahan membentuk release/build baru |

Retention exact untuk artifact, SBOM, evidence, dan rollback set tidak boleh dikarang. Minimal, artifact Production yang aktif dan rollback target yang disetujui tidak boleh dihapus oleh cleanup otomatis.

---

# 7. Build-once promotion

## 7.1 Contract

```text
Integrated commit on main
→ one controlled Release build
→ immutable component artifacts + SBOM + hashes + Release Manifest
→ deploy to Dev/QA using environment values
→ test and designate exact artifact set as RC
→ promote same artifact IDs/digests to Pilot/Production
```

Tidak ada compile/package ulang untuk QA, Pilot, atau Production. Environment differences berasal dari:

- environment-scoped configuration yang terversi;
- secret reference/managed identity/service connection yang disetujui;
- capability/feature exposure yang typed, authorized, dan diaudit;
- deployment topology yang berada di luar application artifact.

## 7.2 Promotion guard

Sebelum promotion:

1. Ambil artifact hanya dari restricted pipeline storage berdasarkan immutable ID/digest.
2. Verifikasi Release Manifest, signature/attestation bila kelak diwajibkan, hash setiap artifact, dan hubungan build/commit.
3. Pastikan gate/evidence mengacu ke digest yang sama, bukan sekadar nomor versi yang dapat digunakan ulang.
4. Validasi environment configuration/schema, minimum/supported versions, contract/capability, database migration path, dan resource readiness.
5. Ambil approval environment dari authority manusia yang sah.
6. Rekam deployment attempt terpisah; retry deployment tidak membuat release/artifact baru dan tidak menimpa attempt evidence.

Jika artifact berubah setelah QA, ia menjadi artifact set/build baru dan seluruh gate relevan harus diulang. Manual file replacement atau rebuilding old source untuk rollback dilarang.

---

# 8. Versioning

## 8.1 Version roles

| Version/reference | Fungsi | Hubungan yang wajib dicatat |
|---|---|---|
| `ProductVersion` | Versi rilis yang dipahami user/support | Release ID, release notes, component compatibility set |
| `BuildVersion` | Identitas unik pipeline build immutable | Pipeline run, CommitId, agent/toolchain, artifact-set digest |
| `CommitId` | Provenance source exact | PR/tasks dan BuildVersion |
| `API ContractVersion` | Semantik/versioned API contract | API artifact, supported Desktop/Mobile/Web/client range |
| `CapabilityVersion` | Snapshot capability/exposure semantics yang diharapkan client/resolver | API/Worker/Desktop/Mobile/LocalWorker compatibility |
| `DatabaseMigrationVersion` | State/schema migration boundary | DB bundle, compatible application range, preflight/recovery |
| `DesktopVersion` | Versi Desktop package | Supported API/LocalWorker/SQLite profile/config/capability |
| `MobileVersion` | Versi Phone/Tablet package | Supported API/config/capability/platform distribution |
| `LocalWorkerVersion` | Versi local processing Worker | IPC/engine/checkpoint compatibility dengan Desktop |
| `ConfigurationVersion` | Schema/typed configuration snapshot | Environment deployment, compatible component range |

Semua version tidak perlu sama. Yang wajib adalah satu **compatibility manifest** yang dapat menjawab apakah kombinasi component/config/contract/schema tersebut `SUPPORTED`, `UPDATE_REQUIRED`, atau `UNSUPPORTED`.

## 8.2 Rules

- Version yang ditampilkan ulang tidak boleh menunjuk bytes atau contract berbeda.
- BuildVersion dan artifact digest tidak dapat digunakan ulang.
- ProductVersion dapat mencakup beberapa component version, tetapi mapping-nya immutable pada Release Manifest.
- Database/config/contract/capability change membutuhkan explicit compatibility evidence dan release-note entry.
- Client lama tidak diasumsikan kompatibel; supported/minimum window berasal dari controlled policy/configuration.
- Exact format versi, numbering cadence, pre-release label, dan tag convention memerlukan konfirmasi Pertamina.

---

# 9. Contract compatibility

## 9.1 Compatibility surfaces

| Surface | Verifikasi minimum sebelum promotion |
|---|---|
| API/OpenAPI | Route, required/optional field, type, enum, media type, HTTP/error semantics, auth requirement, pagination/idempotency behavior |
| SignalR | Envelope/catalog/version/sequence; client dapat recovery melalui REST; forbidden fields tetap dilarang |
| Job/queue | Job state, retry/cancel/resume, checkpoint generation/schema/engine/source digest, result expiry, idempotency, mixed API/Worker rollout |
| IPC Desktop–Local Worker | Handshake, message/operation/error version, capability negotiation, fail-closed unsupported combination |
| Database | From/to migration, expand-migrate-contract, old/new application coexistence bila rolling/staged, forward-fix/rollback constraint |
| SQLite/local profile | Desktop/LocalWorker/profile schema dan recovery compatibility; source/draft/temp tetap local-only |
| Configuration | ConfigurationVersion/schema, typed bounds, unknown/missing key behavior, secret reference availability |
| Capability/Feature | Canonical Feature IDs tidak berubah; exposure bukan authority; authorization final tetap server/local policy sesuai route |
| Stored/outstanding work | Persisted Job state source of truth; old checkpoint/result/outbox/audit event tetap readable atau mendapat controlled terminal disposition |

## 9.2 Breaking-change rule

Perubahan route, required field, type/meaning, enum, status/error semantics, media type, auth scope, IPC/event schema, checkpoint interpretation, database schema, atau configuration semantics yang memutus consumer adalah breaking change. Release hanya dapat lanjut bila:

1. tersedia backward-compatible change atau versioned compatibility path;
2. consumer matrix dan upgrade order terdokumentasi;
3. contract test dan mixed-version/recovery test PASS;
4. migration/configuration/rollback impact dijelaskan;
5. breaking change yang memang diperlukan mendapat controlled human approval.

Feature flag tidak menyelesaikan incompatible binary/database contract. Ia hanya mengendalikan exposure/behavior yang sudah aman dan kompatibel.

---

# 10. Environment approval

## 10.1 Approval gates

| Gate | Prerequisite minimum | Approval/decision | Evidence |
|---|---|---|---|
| Development | PR/main pipeline stage sesuai scope; controlled Dev config | Human developer/integrator untuk merge/deploy Dev sesuai policy | PR, build, tests, artifact digest, Dev attempt |
| QA | Immutable artifact dari main; QA config/schema/migration preflight; automated gates PASS | Named QA/release authority aktual; bila satu orang merangkap, perannya dicatat jujur | QA deployment, smoke, integration/security/regression summary |
| UAT Ready | Semua mandatory Document 12 gate applicable PASS; no Blocker/Critical; immutable RC dan UAT package | Human readiness decision; bukan UAT sign-off | RC manifest, Test Summary, risks/known issues, lane/data/account readiness |
| UAT Sign-off | Document 13 entry/exit dan released/applicable scenarios; human UAT evidence | Authorized business/UAT signer | `ACCEPTED`, `ACCEPTED_WITH_KNOWN_ISSUES`, atau `REJECTED`; Production approval separate |
| Production | Same RC artifact; UAT sign-off; security/architecture approval sesuai policy; migration/config/rollback readiness; change window | Explicit human Production approval dari authority resmi | Release record, approvals, artifact/config/migration versions, rollback reference |

`REQUIRES_PERTAMINA_RELEASE_APPROVAL_CONFIRMATION`: official approver role, delegation, quorum, security/architecture review triggers, segregation-of-duties requirement, emergency approval, release window, dan repository evidence/retention.

Codex tidak dapat memenuhi human approval. Pipeline juga tidak boleh membuat approval otomatis seolah-olah manusia telah menyetujui.

## 10.2 Separation of identities

- Developer identity, pipeline build identity, deployment identity, dan runtime identity harus berbeda secara permission, walaupun organisasi awal kecil.
- Service connection dibatasi per environment dan artifact/action; Production access tidak diwariskan dari Dev/QA.
- UAT business acceptance tidak menyiratkan security risk acceptance atau Production approval.
- Bypass/override membutuhkan explicit authority, reason, expiry, audit/evidence, dan post-review; bukan fitur rutin.

---

# 11. Deployment strategies

## 11.1 Evaluation

| Strategy | Cocok bila | Kelebihan | Risiko/prasyarat | Rekomendasi |
|---|---|---|---|---|
| In-place | Single instance/noncritical Dev atau topology tidak mendukung staged deployment | Sederhana dan murah | Downtime, rollback lebih lambat, mixed config risk | Boleh untuk Dev; QA/Production hanya dengan approved downtime dan rollback |
| Rolling | Beberapa instance stateless/compatible dan health/drain tersedia | Mengurangi interruption; phased verification | Mixed-version contract/DB, sticky session/job/SignalR/Worker coordination | Pilihan Production hanya bila topology dan compatibility membuktikan dukungan |
| Blue/Green | Duplicate environment, traffic switch, data/schema compatibility, capacity tersedia | Cutover/rollback cepat | Biaya, state/queue/storage/database consistency, operational complexity | Bukan baseline; gunakan hanya setelah infrastructure approval |
| Slot/Staged | Hosting mendukung deployment slot/stage dan safe config separation | Warm-up, smoke sebelum swap | Slot-specific secret/config, database tetap shared, provider/queue side effect | Preferable untuk Web/API jika platform nyata mendukung |
| Feature-Flag Rollout | Behavior sudah ada dalam artifact kompatibel dan flag typed/authorized/audited | Scoped Pilot, fast disable | Flag debt, hidden dependency, tidak menyelesaikan migration/contract break | Gunakan sebagai pelengkap Pilot/Production, bukan pengganti deployment gate |

## 11.2 Recommendation by environment

| Environment | Konsep rekomendasi |
|---|---|
| Dev | Controlled in-place/redeploy dari pipeline dapat diterima. Gunakan synthetic data, Dev-only identity/config, dan basic smoke. |
| QA | Deploy artifact sama melalui staged/slot bila tersedia; jika tidak, controlled in-place dengan reset/recovery plan. Jalankan migration test dan boundary suites. |
| Pilot | Promote exact RC; scoped cohort/Feature flag bila capability memungkinkan; monitoring dan stop/rollback conditions eksplisit. Pilot bukan Production approval. |
| Production | Pilih slot/staged untuk topology yang mendukung atau rolling untuk multi-instance yang compatibility-safe. Blue/Green hanya jika duplicate infrastructure/data strategy disetujui. In-place adalah fallback yang membutuhkan approved downtime dan rollback. |

Desktop, Local Worker, dan Mobile bukan server deployment biasa:

- Desktop–Local Worker harus dipaketkan/promosikan dengan compatibility manifest dan update/rollback policy; jangan membiarkan pasangan version yang unsupported berjalan diam-diam.
- Mobile mengikuti signing/distribution channel yang disetujui; staged store/MDM rollout dan rollback capability platform harus dikonfirmasi.
- Server Worker rollout memperhatikan queue admission, lease, idempotency, persisted Job state, checkpoint compatibility, graceful drain, dan no silent loss.

## 11.3 Generic deployment sequence

```text
Approved release + environment
→ verify artifact/manifest/digest
→ validate config/secret references/capacity/dependencies
→ verify recovery point and rollback target
→ apply reviewed migration through pipeline when required
→ deploy component in approved order
→ readiness/health + smoke
→ observe errors, latency, queue/jobs, storage, DB, SignalR, Audit
→ complete or trigger rollback/incident path
```

Exact executable steps, service names, endpoints, agent pools, cloud/IIS/container topology, dan order komponen tetap diblokir sampai infrastructure inventory tersedia.

---

# 12. Rollback pipeline

## 12.1 Preconditions

Rollback hanya memakai retained, previously verified artifact/configuration yang digest-nya masih valid. Jangan rebuild versi lama dari source. Record minimum sebelum eksekusi:

- Release/Deployment/Incident ID dan affected environment/scope;
- current dan target artifact-set ID/digest;
- current dan target ConfigurationVersion;
- DatabaseMigrationVersion serta compatibility/forward-fix assessment;
- feature flags yang dapat dinonaktifkan dengan aman;
- approval authority, stop condition, smoke plan, evidence location.

## 12.2 Decision path

```text
Incident/bad release detected
→ contain new exposure/admission if safe and authorized
→ feature-specific compatible issue?
   → disable approved feature flag and verify
→ configuration-only regression?
   → restore previous verified config and verify
→ application rollback compatible with current DB/state/jobs?
   → promote retained previous artifact and verify
→ DB rollback destructive/unsupported or state already advanced?
   → do not force reverse migration; use approved forward-fix/hotfix/recovery path
```

## 12.3 Pipeline stages

1. **Initiate:** link Incident/change, declare impact, desired recovery outcome, current facts/unknowns.
2. **Authorize:** normal atau emergency human approval sesuai policy; Codex tidak menyetujui.
3. **Verify target:** retrieve by immutable ID, verify digest/manifest/SBOM/evidence and availability.
4. **Compatibility preflight:** DB/config/API/Worker/client/job/checkpoint/result compatibility; active migration and storage health.
5. **Quiesce/drain where required:** bounded admission control; accepted jobs tetap reconcilable; no silent cancellation.
6. **Promote previous artifact/config/flag state:** melalui deployment identity/pipeline.
7. **Smoke and security check:** readiness, critical flow, authorization isolation, storage/job/Audit correctness.
8. **Observe:** monitoring window/status berdasarkan approved target; jika belum ada angka resmi gunakan recorded observation, bukan SLA claim.
9. **Close/escalate:** record outcome, remaining risk, follow-up hotfix/PIR, and release/incident evidence.

Database rollback tidak otomatis aman. Gunakan expand-migrate-contract dan backward compatibility; untuk migration irreversibel, rollback aplikasi mungkin dilarang dan forward-fix menjadi jalur aman. Recovery point harus diverifikasi sebelum migration Production.

---

# 13. Hotfix flow

```text
Incident/Defect
→ authorized Hotfix Task dengan scope sempit
→ short-lived hotfix branch/worktree dari approved source state
→ fix + exact reproduction test + affected regression
→ build/unit/static/secret/dependency/architecture/contract gates
→ immutable new artifact + SBOM + digest
→ human review/PR
→ QA deployment + smoke/security/recovery checks
→ expedited but explicit approval
→ promote same artifact to Production
→ smoke + monitoring + incident verification
→ merge/release record + post-review/PIR as required
```

Aturan:

- Tidak ada direct Production patch, manual DLL/file copy, direct SQL repair, untracked config/secret change, atau bypass security gate.
- Hotfix boleh mempercepat kalender/meeting, bukan menghapus traceability, review, test, integrity, approval, rollback, monitoring, atau evidence.
- Fix berasal dari controlled source dan membentuk artifact immutable baru; failure evidence awal tidak ditimpa.
- Jika hotfix dibuat dari release commit yang berbeda dari `main`, perubahan wajib direkonsiliasi kembali ke `main` dan divergence diverifikasi.
- Exact original defect reproduction dijalankan lebih dahulu pada fix artifact, kemudian affected/shared regression.
- Security/dependency compromise juga memerlukan freeze affected version, credential rotation bila relevan, clean rebuild, impacted-artifact review, dan verified redeployment.

---

# 14. Release notes

## 14.1 Minimum template

```text
Product / ProductVersion
Release ID / Release Date (UTC and approved local display)
BuildVersion / CommitId / artifact-set reference

Features
- Canonical Feature IDs dan user-facing outcome yang benar-benar dirilis

Fixes
- Defect/Incident references dan concise safe outcome

Known Limitations / Known Issues
- affected scope, workaround yang disetujui, owner/target/disposition

Database Changes
- DatabaseMigrationVersion, compatibility dan operational note aman

Configuration Changes
- ConfigurationVersion, key/category impact tanpa secret/value sensitif

Compatibility
- supported/minimum client, API, capability, Desktop/Mobile/LocalWorker combinations

Security Notes
- user/admin action yang diperlukan; no exploit/secret detail

Rollback Notes
- retained target reference dan compatibility limitation

Support Reference
- support/incident/runbook route dan known-error references
```

Release notes dilarang memuat secret/token/key/connection string, bearer/SAS link, document content, password PDF, signature, OCR/clipboard/AI payload, raw SQL/stack/claim, storage reference sensitif, atau exploit instructions yang meningkatkan risiko. Restricted security detail ditempatkan pada evidence store berotorisasi dan hanya direferensikan secara aman.

Release notes bukan pengganti Release Manifest, test evidence, UAT sign-off, atau Production approval.

---

# 15. Evidence/audit

## 15.1 Release evidence manifest

Setiap deployment attempt, termasuk gagal/rollback, menghasilkan append-oriented evidence. Minimum:

| Area | Field/evidence minimum |
|---|---|
| Identity | Release ID, Deployment/Attempt ID, environment, actor/approver references, pipeline identity |
| Source/build | BuildVersion, pipeline run, CommitId, PR/task/change summary, release tag/version |
| Artifact | Artifact names/IDs, per-artifact digest, Release Manifest/artifact-set digest, SBOM link |
| Version | Product, API Contract, Capability, DatabaseMigration, Desktop, Mobile, LocalWorker, Configuration versions |
| Tests | Planned/executed/PASS/FAIL/BLOCKED/N/A summary; gate result; first failure + rerun disposition |
| Security | Scan/tool/version summary, findings by governed severity/status, waiver references/expiry, integrity result |
| Approval | Gate, authority, decision, timestamp, conditions, rationale/reference; no fake approval |
| Deployment | Start/end time, environment, strategy, target scope, artifact/config actually used, attempt outcome |
| Verification | Migration/config/health smoke, authorization/security smoke, monitoring observation |
| Recovery | Previous artifact/config, rollback readiness/decision/result, Incident/PIR link |
| Known state | Known issues/limitations, blockers, follow-up Task/Defect, support reference |

## 15.2 Evidence rules

- Evidence terikat BuildVersion + artifact digest + environment/config; nomor versi saja tidak cukup.
- Evidence tidak silently overwritten. Diagnostic rerun disimpan bersama failure awal.
- Large/restricted evidence tidak dikomit ke Git secara default dan tidak disalin ke release notes.
- Evidence link tidak memuat token, SAS, credential, atau secret query parameter.
- Access, retention, legal/investigation hold, archive, purge, dan artifact retention mengikuti policy; angka durasi tidak dikarang.
- Pipeline logs/observability, Azure DevOps approval history, enterprise Audit, Incident, Defect, UAT evidence, dan Support tetap plane berbeda namun saling ditautkan dengan opaque references.
- Jangan menciptakan EventType Audit baru secara ad hoc. Bila release/deployment event belum ada pada Document 10 Event Catalog, ajukan controlled catalog decision/versioning terlebih dahulu.
- Administrative change terhadap branch policy, pipeline, environment approval, service connection, scan exclusion, artifact retention, atau evidence harus highly controlled dan mempunyai audit/evidence sendiri sesuai catalog/policy yang disetujui.

## 15.3 Reproducibility and integrity

Catat SDK, package graph/source, relevant tool/scanner version, agent/image identity, source commit, build inputs, SBOM, dan artifact hashes. Reproducible build setting digunakan sejauh platform mendukung, tetapi hash build ulang tidak boleh diasumsikan identik tanpa bukti. Release authority adalah original verified artifact, bukan klaim bahwa source lama dapat dibangun kembali.

---

# 16. Codex boundary

| Codex boleh | Codex tidak boleh |
|---|---|
| Menyusun draft Azure Pipelines YAML/template dalam task/branch scoped | Memegang, meminta, menyalin, atau mengungkap Production secret/credential |
| Menambah draft build/test/package step yang telah diizinkan | Menyetujui PR, waiver, UAT, release, rollback, atau Production deployment |
| Membuat/merawat unit/architecture/contract test dalam scope | Deploy/rollback/migrate/restore/restart Production |
| Menyusun SBOM/hash/release-manifest generation draft | Bypass test/approval; memakai `continue-on-error` pada mandatory gate |
| Membuat draft release notes dan safe evidence summary | Menonaktifkan atau mengecilkan scan/test/assertion agar hijau |
| Menganalisis failure dari log/evidence yang telah disanitasi | Mengubah/menghapus/menimpa failure, approval, hash, artifact, atau audit evidence |
| Mengusulkan dependency dengan alasan dan review metadata | Menambah paid extension/provider/package tanpa approval |
| Menjalankan test/build lokal/non-Production yang diotorisasi | Membaca Production document/database/log/dump atau melakukan direct patch/query |
| Menandai blocker/unknown dan meminta controlled decision | Mengarang PASS, approver, SLA, timestamp, deployment result, atau compatibility |

Setiap task Codex wajib menyatakan Task ID, scope, allowed/do-not-touch paths, dependencies, acceptance criteria, tests, dan human reviewer. Pipeline green tidak memberikan Codex authority untuk merge atau release.

---

# 17. Pertamina confirmation gaps

| Gap keputusan | Dampak | Status/aksi |
|---|---|---|
| Official Dev/QA/UAT/Production approver, delegation, quorum, SoD | Approval environment tidak dapat difinalkan | `REQUIRES_PERTAMINA_RELEASE_APPROVAL_CONFIRMATION` |
| Azure DevOps organization/project/repository layout, branch naming, merge policy, code owner, bypass | Exact policy/YAML resource belum dapat dikunci | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| Agent pool/image/OS, network/egress, approved feeds, tool cache | Build reproducibility dan scanner connectivity | Confirm infrastructure inventory dan hardening |
| Hosting topology Web/API/Worker/SignalR/queue/storage/DB | Rolling/slot/blue-green dan deployment order tidak dapat dipilih | `BLOCKED_BY_DECISION: DEPLOYMENT_TOPOLOGY` |
| Environment service connections dan deployment identity model | Least privilege/separation tidak dapat diimplementasikan | Security + platform approval required |
| Production change window, maintenance/admission/drain policy | Rollout/rollback timing dan no-silent-wait behavior | `REQUIRES_PERTAMINA_RELEASE_APPROVAL_CONFIRMATION` |
| Version/tag/release numbering convention | Product/component/tag mapping belum final | Release governance approval required |
| Artifact store, immutability, access, retention, rollback depth | Rollback availability dan evidence retention | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| Vulnerability scoring, Critical/High policy, non-waivable findings, waiver authority/expiry | Security gate exact thresholds | Security policy confirmation required |
| Scanner/SAST/dependency/license products dan data residency | Exact steps/tool output belum dapat dipilih | Baseline tidak boleh tergantung paid extension |
| SBOM format, signing, provenance attestation, certificate/key ownership | Supply-chain evidence dan verification | Security/release approval required |
| Database technology operations, backup/recovery point, migration tooling, expand-contract window | Safe Production migration/rollback belum executable | `BLOCKED_BY_DECISION: DATABASE_DEPLOYMENT_AND_RECOVERY` |
| Configuration/secret store and rotation mechanism | Environment injection dan rollback config | Security/platform decision required |
| Desktop signing, packaging, update channel, rollback support | Desktop artifact promotion belum executable | Architecture/security/distribution approval required |
| Mobile signing/store/MDM tracks dan rollback capability | Mobile staged release belum executable | Architecture/security/distribution approval required |
| UAT authority, actual RC, evidence repository, sign-off retention | UAT/Production gate belum dapat PASS | Saat ini UAT execution/sign-off belum direkam |
| Performance/recovery numeric targets dan Production observation window | Numeric gate tidak dapat dinyatakan PASS | Gunakan `TARGET_TO_BE_BASELINED/APPROVED`; jangan mengarang SLA |
| VA/PT trigger/provider/scope dan release dependency | Production security evidence belum final | Pertamina security process confirmation required |
| Feature flag platform, owner, lifecycle, expiry/cleanup | Pilot/disable strategy dapat menambah risk debt | Configuration/policy decision required |
| Incident/emergency change authority, contact/on-call, PIR trigger | Hotfix/rollback escalation belum executable | Document 16 confirmation + release authority required |

Tidak satu gap pun membolehkan bypass test/security, rebuild per environment, direct Production patch, startup migration, fake approval, atau penghapusan evidence.

---

# 18. Risks/conflicts

| Risk/conflict | Dampak | Mitigasi rekomendasi |
|---|---|---|
| Satu developer menjadi author/reviewer/release operator | Review independence/SoD lemah | Rekam peran nyata; human merge gate; security/Production authority dikonfirmasi; jangan membuat tim palsu |
| UAT spec final tetapi execution belum ada | False Production-ready claim | Production gate tetap BLOCKED sampai immutable RC dan human sign-off tercatat |
| Infrastructure topology belum diketahui | Strategi deployment dapat over-engineered/tidak dapat dijalankan | Gunakan conditional recommendation; lakukan inventory sebelum executable runbook/YAML |
| Build-once vs environment-specific packaging | Risiko memasukkan config/secret dalam binary | Pisahkan artifact immutable dari configuration/secret injection; verify manifest per promotion |
| Database migration vs rollback aplikasi | Old binary mungkin incompatible dengan new schema/state | Expand-migrate-contract, compatibility tests, recovery point, forward-fix path; no blind reverse migration |
| Rolling API/Worker dengan long-running jobs/checkpoints | Duplicate/lost/incompatible work | Persisted Job state source of truth; idempotency, lease/drain, mixed-version/checkpoint tests |
| Desktop–Local Worker version skew | Local process gagal atau bypass | Compatibility handshake/fail-closed, paired package/update policy, retained rollback package |
| Mobile distribution delay | API contract/client range mismatch | Backward-compatible API, minimum/supported client policy, phased release evidence |
| Feature flag menjadi permanent hidden configuration | Security/behavior drift dan dead code | Owner, purpose, default, scope, expiry/review, audit, removal task |
| Paid/proprietary scanner dipilih tanpa approval | License/data-residency/vendor lock-in | Mulai dari approved baseline capability; product selection melalui dependency/security review |
| Scan outage dianggap green | Unknown security state | Required tool failure = BLOCKED; approved equivalent hanya melalui policy |
| Artifact retention terlalu pendek | Rollback target hilang | Protect active + approved rollback artifacts; policy-based cleanup with reference checks |
| Evidence/log terlalu detail | Secret/document/privacy leak | Closed allowlist, restricted store, sanitization tests, no bearer links/payload |
| Evidence dapat diubah/ditimpa | Approval dan release provenance tidak dapat dipercaya | Immutable/restricted publication, append attempt records, digest and access review |
| Hotfix urgency menekan gate | Direct patch/unreviewed regression | Expedited governed flow, new immutable artifact, exact retest, QA, approval, monitoring/PIR |
| Component versions dipaksa sama | Unnecessary coordinated release dan misleading compatibility | Pisahkan version roles; ikat melalui compatibility/Release Manifest |
| Component versions bebas tanpa manifest | Unsupported combination di Production | Compatibility matrix + preflight + contract/mixed-version tests |
| Signing/provenance dianggap tersedia padahal belum | False supply-chain assurance | Catat digest sekarang; signing/attestation hanya setelah key/format/authority approval |
| Document 10 belum tentu memiliki EventType deployment/release | Event catalog drift | Jangan invent EventType; ajukan controlled audit catalog decision |

---

# 19. Suggested Document 17 text

## 19.1 CI/CD principles

PERTAMINA OTO menggunakan Azure DevOps Git dan Azure DevOps Pipeline dengan structured monorepo. Perubahan dilakukan melalui short-lived Task branch/worktree, divalidasi oleh pipeline, ditinjau manusia, lalu digabung ke protected `main`. Direct unreviewed release, force push rutin, manual Production binary patch, startup migration Production, dan untracked configuration change dilarang.

Setiap perubahan dan release harus traceable ke Task/Requirement/Feature/Defect/Incident yang relevan, CommitId, BuildVersion, artifact digest, ProductVersion/release tag, change summary, test/security evidence, approval, serta rollback reference.

## 19.2 Build pipeline

Build pipeline minimum adalah:

```text
Source/trace validation
→ SDK/package/source validation
→ controlled restore
→ Release build
→ MSTest Unit on Microsoft.Testing.Platform
→ static + secret + dependency + applicable license/policy checks
→ Architecture + Contract tests
→ package component artifacts
→ SBOM
→ strong artifact digests + Release Manifest
→ immutable restricted publication
```

Patch SDK yang disetujui dikunci di `global.json`; package versions dikelola melalui `Directory.Packages.props`; floating/silent override serta dependency addition/upgrade tanpa approval dilarang. Pipeline tidak mencetak secret, token, connection string, document content, password, signature, OCR, clipboard, atau AI payload. Mandatory tool failure berstatus FAIL/BLOCKED, bukan silent PASS.

## 19.3 Risk-based tests

PR menjalankan restore/build/Unit/static-security/Architecture/Contract yang cepat. Main memperluas Unit dan affected Integration/Contract serta membentuk immutable artifacts. QA menjalankan deployment/config/migration smoke, Integration, DB/API/Storage/Worker/IPC, selected E2E, authorization/security, dan affected regression. Release Candidate menjalankan applicable full regression, security, performance, recovery, compatibility, platform, dan UAT readiness.

Gate hanya berstatus PASS, FAIL, BLOCKED, atau NOT_APPLICABLE dengan evidence. Missing/not-run/inconclusive/flaky/quarantined atau target belum disetujui bukan PASS. UAT readiness, UAT acceptance, dan Production approval adalah keputusan terpisah.

## 19.4 Security gates and waiver

Release/promotion berhenti untuk build atau required Unit failure, confirmed secret, Critical dan policy-disallowed vulnerability, security-critical failure, unauthorized disclosure/source corruption/false success, unapproved breaking contract, required scanner failure, dan artifact-integrity mismatch. High finding mengikuti baseline block pada Security Design kecuali explicit controlled exception yang sah.

Waiver harus memiliki ID, exact scope/finding/artifact/environment, reason, named owner/authority, compensating controls, evidence, expiry/review, dan follow-up. Waiver tidak mengubah test menjadi PASS. Codex tidak dapat membuat atau menyetujui waiver.

## 19.5 Artifacts and promotion

Pipeline menerbitkan Web, API, Server Worker, Desktop, Local Worker, Mobile, Database Migration Bundle, Configuration Template, OpenAPI Contract, Release Notes, SBOM, Test/Security Evidence, dan Release Manifest sebagai artifact terpisah. Setiap artifact memiliki BuildVersion, CommitId, component/contract version yang relevan, dan strong digest.

PERTAMINA OTO menerapkan **build once—promote the same artifact**. Artifact yang telah diuji di QA dan ditetapkan sebagai RC dipromosikan dengan ID/digest yang sama ke Pilot/Production. Environment-specific configuration dan secret diinjeksikan terpisah melalui identitas least-privilege. Perubahan bytes setelah QA membentuk artifact/build baru dan mengulang gate relevan.

## 19.6 Version and compatibility

ProductVersion, BuildVersion, CommitId, API ContractVersion, CapabilityVersion, DatabaseMigrationVersion, DesktopVersion, MobileVersion, LocalWorkerVersion, dan ConfigurationVersion mempunyai fungsi berbeda dan tidak harus bernilai sama. Release Manifest memetakan semuanya dan menyatakan supported component combinations.

Compatibility gate mencakup OpenAPI/error semantics, SignalR, Job/idempotency/result/checkpoint, Desktop–Local Worker IPC, database migration, SQLite/local profile, configuration, capability, dan outstanding persisted work. Breaking change memerlukan compatible/versioned path, consumer matrix, upgrade order, mixed-version/recovery tests, serta controlled approval.

## 19.7 Environment approvals

Development, QA, UAT Ready, UAT Sign-off, dan Production mempunyai gate/evidence sendiri. Production hanya dapat memakai pipeline artifact yang sama setelah human approval, security/architecture approval sesuai policy, UAT evidence/sign-off, migration/configuration readiness, dan verified rollback readiness.

Official approver, delegation, quorum, SoD, bypass/emergency authority, dan retention berstatus `REQUIRES_PERTAMINA_RELEASE_APPROVAL_CONFIRMATION`. Pipeline/Codex tidak boleh membuat approval palsu atau menyimpulkan Production approval dari technical green.

## 19.8 Deployment concept

Dev dapat memakai controlled in-place deployment. QA memakai slot/staged bila tersedia atau controlled in-place dengan recovery plan. Pilot memakai exact RC dengan scoped/feature-flag rollout jika platform mendukung. Production memilih slot/staged atau rolling hanya setelah topology, health/drain, state, queue, database, dan mixed-version compatibility terbukti. Blue/Green tidak menjadi baseline tanpa duplicate infrastructure/data/traffic-switch approval. Feature flag melengkapi, bukan menggantikan, artifact/contract/migration gate.

Exact commands, service names, endpoints, topology, dan credentials hanya ditetapkan setelah infrastructure inventory dan approval; tidak disimpan sebagai secret di dokumen/pipeline source.

## 19.9 Rollback and hotfix

Rollback memakai retained previous verified artifact/configuration, optional approved feature-flag disable, compatibility preflight, human approval, smoke, monitoring, Incident link, dan append-oriented evidence. Versi lama tidak dibangun ulang dari source. Migration irreversibel tidak dipaksa reverse; gunakan backward compatibility atau approved forward-fix.

Hotfix mengikuti Incident/Defect → scoped Task/branch → fix + exact test/regression → security gates → immutable artifact → human review → QA → approval → promote → verify/monitor → merge/release record. Emergency mempercepat proses tetapi tidak menghapus review, tests, security, integrity, approval, rollback, atau evidence. Direct Production patch dilarang.

## 19.10 Release evidence and notes

Release evidence minimum memuat Release/Deployment ID, environment, Product/Build/Commit, artifact IDs/hashes, test/security summaries, migration/configuration/component versions, approvals, deployment times/strategy/outcome, smoke, monitoring observation, rollback target/result, Incident link, dan known issues. Failure/rerun/rollback evidence tidak ditimpa.

Release notes memuat version/date, Features, Fixes, Known Limitations/Issues, Database Changes, Configuration Changes, Compatibility, Security Notes, Rollback Notes, dan Support Reference. Secret, exploit detail sensitif, document data, password, signature, OCR/clipboard/AI payload, raw SQL/stack/claim, dan bearer/storage links dilarang.

## 19.11 Codex and Production boundary

Codex boleh menyiapkan draft pipeline YAML, test/package step, manifest/SBOM/hash automation, release-note draft, dan failure analysis dalam task scoped. Codex tidak memegang Production secret, approve PR/release/waiver/UAT, deploy/rollback/migrate/restore Production, bypass test, menonaktifkan scan, mengubah evidence, atau mengarang PASS/approval/SLA.

## 19.12 Unresolved release decisions

Sebelum pipeline deployment executable difinalkan, Pertamina harus mengonfirmasi approval roles, Azure DevOps policy/resources, agent/feed/scanner/toolchain, hosting topology, service connections, artifact/evidence retention, version/tag convention, vulnerability/waiver policy, SBOM/signing/provenance, database migration/recovery, secret/configuration store, Desktop/Mobile distribution, change window, performance/recovery targets, VA/PT, dan Incident/emergency change authority.

---

**Agent B conclusion:** kontrak yang paling aman dan ringan untuk kondisi satu developer + Codex adalah protected-main + short-lived task branch/worktree + human merge gate, satu immutable multi-artifact build dari `main`, promotion artifact yang sama melalui Dev/QA/UAT/Production, risk-based testing, explicit security/approval gates, dan rollback/hotfix berbasis retained verified artifact. Semua detail yang bergantung pada topology, authority, secret store, signing, atau Production operations tetap menunggu keputusan resmi dan tidak boleh diasumsikan oleh pipeline maupun Codex.
