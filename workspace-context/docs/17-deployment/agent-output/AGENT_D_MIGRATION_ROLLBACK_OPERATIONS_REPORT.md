# AGENT D — DATABASE MIGRATION, BACKUP, ROLLBACK, RECOVERY & OPERATIONAL VALIDATION REPORT

> **Task ID:** `DOC17-AGENT-D`  
> **Dokumen sasaran:** Document 17 — Deployment Guide  
> **Tanggal audit:** 1 September 2026  
> **Sumber:** Document 01–16 FINAL dan Master Project Summary v3.9  
> **Jenis artefak:** specialist report; bukan master, deployment approval, Production command, atau bukti eksekusi  
> **Baseline produk:** 550 Feature ID canonical; 33 Primary PDF Tool Cards; Local-First; satu SQL Server dengan schema ownership; SQLite per profil/perangkat; metadata SQL + payload secure storage  
> **Status rekomendasi:** `PRE-IMPLEMENTATION DEPLOYMENT/RECOVERY BASELINE` — `BELUM DEPLOYMENT_READY`

## Executive findings

1. Production tidak boleh menjalankan EF Core migration otomatis saat application startup. Migration harus berasal dari pipeline, ditinjau, diperiksa kompatibilitasnya, mempunyai recovery point, dan menghasilkan evidence.
2. Setiap `DbContext`/modul memiliki migration assembly dan migration history dalam schema miliknya. Modul tidak boleh mengubah tabel modul lain melalui migration, direct SQL, atau shared-table mutation.
3. Migration yang mengubah data tidak otomatis dapat di-`down`. Setelah data Production berubah, forward-fix sering lebih aman; restore, compensation, atau data repair memerlukan keputusan risiko tersendiri.
4. Restore produk mencakup SQL, secure storage, key/reference, application compatibility, authorization, Audit, Job/outbox/idempotency, dan smoke test. Database online saja bukan keberhasilan restore.
5. `Amankan ke Akun` (`FEAT-BACKUP-001..013`) adalah selective user-initiated secured storage, bukan operational server backup. Client local workspace juga bukan bagian otomatis dari server backup.
6. Nilai exact RPO, RTO, DR location, replication model, backup topology/schedule, observation duration, dan organizational authority tidak tersedia. Semuanya tetap `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` atau `TARGET_TO_BE_APPROVED`.
7. Tidak ada Production SQL command, restore, deployment, deletion, migration execution, vendor selection, atau perubahan master yang dilakukan dalam report ini.

---

# 1. SQL migration strategy

## 1.1 Prinsip wajib

- Migration menjadi artefak deployment yang dikontrol, bukan side effect saat API/Worker start.
- Source migration harus berasal dari repository dan commit yang sama dengan release; bundle/script, artifact, SBOM, configuration manifest, dan digest ditautkan ke pipeline run.
- Setiap migration mempunyai satu owner module, satu source version, satu target version, dan satu validation/rollback disposition yang eksplisit.
- Pipeline menghasilkan reviewed idempotent migration bundle atau equivalent controlled artifact; tidak ada baseline manual Production SQL patch.
- Dev, QA, dan Production mempunyai history terpisah. Sukses pada satu environment tidak membuktikan environment lain sudah dimigrasikan.
- Production compatibility diperiksa sebelum perubahan: application N, application N−1 bila masih didukung, schema current, schema target, data volume, storage, log/capacity, active jobs, dan dependency.
- Migration dan backfill berjalan dalam bounded work. Transaksi tidak mencakup pekerjaan object storage atau transformasi file yang panjang.
- Reference/status seed adalah migration eksplisit; historical code tetap dapat dibaca sesuai compatibility policy.
- Forward-fix menjadi default setelah data Production berubah, kecuali rollback telah dibuktikan aman dan disetujui.

## 1.2 Migration record minimum

| Field | Aturan |
|---|---|
| Migration ID | Stable ID dari migration artifact; tidak digunakan sebagai credential/permission |
| Source Version | Schema/migration version yang harus ada sebelum apply |
| Target Version | Schema/migration version yang diharapkan setelah apply |
| Affected Schema/Module | Satu atau beberapa owner module yang masing-masing mempunyai artefak terpisah dan dependency eksplisit |
| Backward Compatible? | `YES`, `CONDITIONAL`, atau `NO`, dengan supported application/schema matrix |
| Data Transformation? | `NONE`, `BOUNDED_BACKFILL`, `REWRITE`, atau jenis terkontrol lain; exact taxonomy perlu disetujui |
| Estimated Impact | Estimated rows/bytes/log growth/lock risk/duration/resource class; bukan janji tanpa measurement |
| Required Backup | Recovery point/backup-set reference dan verification result |
| Precondition | Current schema, free space, log/capacity, dependency, no conflicting operation, approved window bila perlu |
| Validation | Schema, constraint, row/invariant, permission, app compatibility, storage reconciliation, smoke result |
| Rollback Strategy | No-op/additive rollback, forward-fix, restore, compensation, atau data repair—dengan limitation |
| Owner | Owner module dan accountable deployment function; nama unit/principal menunggu konfirmasi |
| Evidence | Artifact/digest, pipeline, approval, result, timing, validation, incident/rollback reference |
| Environment History | Apply/validate outcome per environment; tidak ditimpa antar-environment |

Field tambahan yang diperlukan untuk traceability: commit, build/release, dependency migration, migration artifact digest, schema history before/after, data classification, risk level, maintenance/availability impact, execution function, approver reference, start/end UTC, stop condition, observation result, dan final disposition.

## 1.3 Controlled lifecycle

```text
Requirement/change approved
→ owner module creates versioned migration
→ static/schema/compatibility review
→ clean database and supported prior-version tests
→ QA dry run with representative volume
→ backup/recovery point verified
→ deployment authorization and readiness check
→ controlled migration apply
→ database/storage/application/security validation
→ bounded smoke test
→ observation window
→ close evidence or invoke approved recovery decision
```

Lifecycle labels di atas adalah process stages, bukan enum/API/database baru.

## 1.4 Environment history

Environment history minimum mencatat environment code, source/target observed version, migration artifact/digest, pipeline/release reference, start/end UTC, executor function, approval reference, outcome, warning/error code aman, validation result, evidence reference, rollback/forward-fix/incident link, dan current disposition. Record lama tidak ditimpa; correction menggunakan linked append/revision sesuai governance.

## 1.5 Stop conditions

Hentikan atau jangan mulai migration bila target tidak cocok, recovery point belum verified, disk/log/capacity tidak cukup, artifact/digest tidak sah, required secret/config tidak tersedia, permission/identity tidak tepat, dependency migration belum valid, unexpected schema drift terdeteksi, active workload tidak dapat dilindungi, atau validation oracle tidak siap. Jangan memaksa history table atau menandai migration berhasil secara manual.

---

# 2. Module/schema ownership

## 2.1 Canonical ownership dari Document 07

| Schema logis | Owner module | Migration boundary |
|---|---|---|
| `identity` | Account Administration — Identity | Principal mapping dan session/revocation metadata |
| `admin` | Account Administration — Authorization | Role, permission, assignment |
| `configuration` | Platform Configuration | Feature snapshot, exposure, flags, policies, compatibility |
| `workspace` | Workspace Metadata | Allowlisted account preferences/favorites/presets |
| `jobs` | Processing/Jobs | Job, upload, attempt, checkpoint, artifact, admission, idempotency, outbox |
| `secured` | Secured Files | Metadata Amankan ke Akun, versions, recycle lifecycle |
| `history` | History | Personal account activity projection |
| `audit` | Enterprise Audit | Safe append-oriented evidence dan export-request metadata |
| `support` | Issue and Support | Issue, attachment metadata, note, triage, export, idempotency, outbox |
| `assistant` | Assistant | Minimal invocation/workflow metadata |
| `intelligence` | Content Intelligence | Intelligence operation/stage metadata |

## 2.2 Enforcement rules

1. Setiap logical `DbContext` hanya mengekspos dan memigrasikan tabel owner module.
2. Satu migration tidak boleh melakukan DDL/DML pada tabel modul lain. Perubahan shared contract dipecah menjadi migration owner masing-masing dan diurutkan melalui dependency manifest.
3. Cross-module FK diminimalkan; cross-module reference memakai opaque logical ID dan application/internal contract setelah commit.
4. Shared `dbo` tidak digunakan sebagai dumping ground. Objek deployment/DBA di `dbo`, bila benar-benar diperlukan, harus mempunyai owner dan persetujuan eksplisit.
5. Tidak ada navigation property, direct `DbSet`, cascade delete, atau shared-table mutation lintas modul.
6. Cross-schema transaction hanya untuk invariant yang telah disetujui dan tidak dapat dipenuhi oleh owner transaction + outbox; defaultnya dihindari.
7. Cross-store SQL/object storage memakai staging → validation → metadata commit → promotion/finalization → reconciliation, bukan distributed transaction panjang.
8. Runtime, migration/DDL, backup/restore, dan human administration identity terpisah dan least-privilege.

## 2.3 Ownership dalam deployment

| Function | Responsibility | Tidak otomatis memperoleh |
|---|---|---|
| Module migration owner | Schema intent, transformation logic, validation oracle, compatibility and recovery notes | Production execution authority |
| Build/release function | Membuat immutable migration/release artifact dan provenance | Permission bisnis ke data/content |
| Deployment/operations approver | Menyetujui window, risk, sequence, stop/rollback decision | Hak menulis schema atau membaca payload secara otomatis |
| Authorized migration executor | Menjalankan approved artifact melalui controlled path | `db_owner`, shared credential, atau kebebasan direct patch |
| Security reviewer | Menilai secret, encryption, access, evidence, destructive/security risk | Business acceptance atau document-content access |
| Validation owner | Memverifikasi database/storage/app/security/smoke evidence | Hak mengubah hasil untuk membuat PASS |

Nama fungsi/unit, SoD, delegation, dan emergency authority adalah `REQUIRES_PERTAMINA_ORGANIZATION_CONFIRMATION`.

---

# 3. Expand/contract approach

## 3.1 Kapan digunakan

Gunakan expand/contract untuk rename/drop, perubahan contract lintas deployment, perubahan yang harus tetap kompatibel dengan dua application version, backfill besar, storage-reference evolution, atau rollout bertahap. Jangan memaksakannya untuk additive nonbreaking change kecil yang dapat divalidasi dalam satu release tanpa compatibility window.

## 3.2 Urutan

```text
Release N — EXPAND
add schema/column/index/contract baru tanpa menghapus yang lama
→ deploy code yang dapat bekerja dengan old + new representation
→ mulai bounded backfill atau dual-write hanya jika benar-benar diperlukan
→ reconcile dan ukur mismatch

Release N/N+1 — SWITCH
validate completeness/invariants
→ switch read authority secara governed
→ monitor errors, compatibility, lag, and data mismatch

Release berikutnya — CONTRACT
pastikan old client/code tidak lagi didukung atau aktif
→ backup/recovery point dan destructive approval
→ remove old representation
→ validate and observe
```

## 3.3 Compatibility matrix

Setiap plan mencatat kombinasi berikut sebagai `SUPPORTED`, `TRANSITION_ONLY`, atau `UNSUPPORTED`:

| Application | Old schema | Expanded schema | Contracted schema |
|---|---:|---:|---:|
| Previous supported app | Evidence required | Evidence required | Umumnya tidak didukung setelah minimum version policy efektif |
| Current release app | Evidence required | Evidence required | Evidence required sebelum contract |
| Rollback application | Harus dibuktikan | Harus dibuktikan | Tidak diasumsikan dapat bekerja |

Status exact hanya sah setelah contract/migration/compatibility tests Document 12 lulus.

## 3.4 Data backfill

- Backfill menggunakan bounded chunk, checkpoint, progress, pause/resume, stable ordering, retry idempotent, dan reconciliation.
- Catat total eligible, processed, skipped, failed, duplicate/conflict, oldest checkpoint, log/I/O/CPU impact, dan final invariant.
- Backfill tidak memegang transaksi panjang atau koneksi saat delay/heavy transform.
- Dual-write mempunyai comparison/reconciliation dan explicit end date; tidak menjadi permanent ambiguity.
- Feature flag hanya mengendalikan exposure/route yang compatible. Flag tidak membuat incompatible schema atau missing migration menjadi aman.

## 3.5 Contract gate

Contract/destructive cleanup hanya boleh dilakukan setelah old client/application retired sesuai approved compatibility policy, backfill 100% sesuai oracle yang disetujui, mismatch terselesaikan, rollback/restore impact dipahami, recovery point verified, QA dry run lulus, dan authorization tersedia.

---

# 4. Destructive migration controls

## 4.1 Destructive indicators

Destructive atau high-risk migration mencakup drop column/table/index yang memengaruhi service, type narrowing, semantic reinterpretation, non-reversible data rewrite, table replacement, large move/backfill, key/identity remap, ownership change, storage reference/key-path change, constraint tightening pada existing data, encryption/key-format change, dan migration yang dapat membuat old application incompatible.

## 4.2 Mandatory gate

| Control | Evidence minimum |
|---|---|
| Explicit review | Owner module, data/security, deployment/operations, dan authority yang berlaku |
| Impact analysis | Rows/bytes/log/lock/downtime, dependent modules, clients, jobs, storage, Audit, retention/hold |
| Recovery point | Verified backup/equivalent point, scope, consistency, integrity, accessibility, key readiness |
| Recovery strategy | Forward-fix, restore, compensation, atau repair; data-loss window dan limitation eksplisit |
| Maintenance window | Digunakan bila availability/locking/log/resource risk memerlukan; duration belum dikarang |
| QA dry run | Supported prior versions, representative volume, abnormal interruption, recovery and validation |
| Validation | Schema/data/storage/security/application/Core smoke and monitoring oracle |
| Approval | Target environment, exact artifact, risk, window, executor, stop/rollback authority |

## 4.3 Execution safeguards

- Resolve exact environment/database/schema/version before action.
- Freeze incompatible deployment and conflicting lifecycle/backfill operation.
- Preserve current and previous artifact/config/migration bundle.
- Monitor transaction log, locks/waits, I/O, CPU/RAM, connections, queue/jobs, storage, and Audit health using safe aggregate signals.
- Stop on unexpected schema drift, data invariant breach, unsafe lock/resource pressure, integrity mismatch, cross-owner effect, or evidence loss.
- Never delete data to make migration history green.
- Never edit migration history to conceal partial failure.

## 4.4 Codex boundary

Codex boleh membantu review scoped migration source, compatibility/test design, evidence summary, dan risk analysis pada repository/worktree yang diizinkan. Codex tidak boleh menjalankan destructive migration, Production command, restore, rollback, data repair, secret access, approval, atau menandai evidence PASS tanpa hasil nyata.

Trade-off besar—availability versus long migration, data-loss window versus restore, repair versus forward-fix—harus dilaporkan kepada Integrator dan authority manusia, bukan diputuskan otomatis.

---

# 5. SQLite migration

## 5.1 Baseline

- Satu SQLite store untuk setiap OS profile/device + Pertamina subject; account switch menutup store lama dan tidak menggabungkan data.
- `schema_migration` adalah durable local migration history; `PRAGMA user_version` hanya marker cepat jika digunakan.
- Stable migration ID, checksum, source/target schema, supported app range, applied time/result, dan recovery reference disimpan secara aman.
- Application memperoleh exclusive profile migration lease dan memblokir local Job dispatch/mutation selama migration.
- Business logic tetap melalui persistence abstraction; UI/Worker tidak mengakses SQLite arbitrer.

## 5.2 Preflight

Periksa profile/account binding, current schema dan supported app range, integrity/readability, available protected storage, battery/power bila relevan, WAL/SHM state, active operation/lease, required local artifacts, pending offline intent/outbox, encryption/key availability, backup capability, dan unsupported downgrade.

## 5.3 Backup/checkpoint

Migration backup lokal harus:

- profile-scoped, encrypted/protected, integrity-authenticated, dan temporary sesuai policy;
- mencakup SQLite DB, WAL/SHM state yang diperlukan, required app-managed artifacts, serta manifest yang membuatnya konsisten;
- tidak memasukkan external source file ke cloud/device backup otomatis;
- tidak menyimpan password/token/secret/signature bytes/full OCR;
- tidak menimpa source atau working result user;
- mempunyai cleanup hanya setelah migration/validation/recovery window disetujui.

Raw SQLite copy tanpa artifact manifest bukan backup yang cukup.

## 5.4 Transaction and resumable journal

- Gunakan single transaction untuk schema/data work yang bounded dan benar-benar transactional.
- Large data/file work memakai `migration_journal` yang resumable, phase marker, input/output identity, checksum, and completion validation.
- Publish artefak mengikuti staged write → flush/close → validate/digest → atomic publish → SQLite pointer commit.
- Setiap phase idempotent atau mempunyai explicit reconciliation; crash antara phases tidak membuat pointer ke artefak belum final.

## 5.5 Failure handling

| Failure point | Safe behavior |
|---|---|
| Sebelum commit | Transaction rollback; original/backup tetap; app tidak membuka mixed schema |
| Setelah schema commit sebelum artifact finalize | Startup recovery membaca journal, memverifikasi phase, lalu resume/reconcile atau kembali ke validated backup |
| Integrity/key/profile failure | Fail closed; read-only diagnostics; jangan membuat DB kosong |
| Disk low/full | Stop sebelum destructive step; pertahankan source/working state; cleanup hanya manifest-owned cache/temp |
| App crash/restart | Acquire new lease setelah stale-owner check; resume only from verified phase |
| Unsupported newer schema | Refuse open dengan guidance update; jangan downgrade otomatis |

## 5.6 Cache-only rebuild

Rebuild hanya boleh untuk store/partition yang terbukti **cache-only**, mempunyai authoritative source lain, tidak memuat draft, operation log, undo/redo, pending intent, local history authority, checkpoint, local result, atau unsynced state. Rebuild tidak boleh menjadi generic response untuk corruption. User workspace tidak dihapus agar migration berhasil.

## 5.7 Downgrade

Downgrade limitation harus eksplisit. Jika old app tidak memahami new schema, old app tidak dibuka terhadap store tersebut. Opsi aman adalah approved compatible app, forward migration, atau recovery snapshot yang tidak mengorbankan work setelah snapshot. Silent schema down-conversion dilarang.

## 5.8 Validation matrix

Uji setiap prior version yang masih didukung dengan dataset empty/normal/large; draft aktif; undo/redo; checkpoint; pending queue/outbox; secured cache; corrupt row/artifact; crash di setiap phase; low disk; account switch; old/new client; unsupported downgrade; and source-file safety. Evidence mengacu Document 11–13 tanpa menyimpan content sensitif.

---

# 6. Storage migration

## 6.1 Scope and boundary

Storage migration berlaku jika provider, container/share/bucket, opaque storage-reference format, encryption/key reference, path convention internal, zone, version model, ownership mapping, atau lifecycle policy berubah. Physical key/path tetap internal; tidak menjadi API, log, Audit payload, atau evidence umum.

## 6.2 Controlled sequence

```text
Inventory authoritative SQL metadata and approved storage manifest
→ authorize owner/scope/classification/retention/hold
→ create destination staging object under least privilege
→ copy/transform payload without changing current pointer
→ verify size/digest/encryption/key/access policy
→ map owner/version/purpose/state/tombstone/retention
→ commit metadata transition through owner module
→ promote destination and reconcile
→ validate read using authorized application path
→ observe retries/orphans/mismatch
→ clean old object only after approval and recovery window
```

## 6.3 Required migration manifest

Migration ID, source/destination profile reference, opaque object reference, owner/resource/version mapping, classification, expected size/digest, key-version reference, current state, tombstone/retention/hold, attempt/checkpoint, copy result, validation result, switch time, rollback reference, cleanup eligibility, environment, and safe evidence link.

Manifest tidak memuat payload, clear path/URL/SAS, credential, raw key, filename default, document/OCR text, atau unrestricted exception.

## 6.4 Integrity and access

- Authorize before source resolution and before destination availability.
- Copy identity/purpose is narrower than normal user access; migration identity does not gain business permission.
- Digest/integrity is verified after transfer and before pointer switch.
- Encryption and key-version compatibility are verified; no cleartext fallback.
- Maximum three active/addressable secured versions is revalidated; migration tidak menghidupkan tombstoned/expired version.
- Access policy, owner mapping, and cross-user denial are tested from application/service boundary.

## 6.5 Retry, orphan, and rollback

- Retry bounded and idempotent by migration/object identity.
- Detect destination-without-metadata, metadata-without-valid-destination, duplicate object, version mismatch, failed cleanup, and stale pointer.
- Keep old payload inaccessible-to-new-route but recoverable until switch validation and approved cleanup.
- Rollback sebelum switch menghapus/menandai destination staging melalui approved lifecycle; sesudah switch memakai reverse pointer only if verified, otherwise forward-fix/restore.
- Jangan menghapus source object sebelum metadata, digest, permission, version, retention, hold, application read, backup, dan observation validation lulus.

---

# 7. Backup scope

## 7.1 Operational backup set

| Scope | Yang dicakup | Catatan konsistensi |
|---|---|---|
| SQL Server | Database dan transaction log sesuai approved recovery model; schema/migration history; module grants/metadata | Recovery model, schedule, topology belum ditetapkan |
| Secure file/object storage | Secured payload, approved issue attachment/export, processing artifact yang retention-nya mewajibkan | Reconcile dengan SQL object/version/digest/key/tombstone/hold |
| Configuration | Versioned non-secret application/environment configuration dan policy/feature/compatibility manifest | Secret value tidak diekspor ke bundle biasa |
| Secret recovery process | Reference ke approved KMS/credential recovery/rotation procedure, custodianship, key version readiness | Bukan backup secret ke dokumen/evidence |
| Enterprise Audit | `audit` metadata, append/integrity/retention/hold state, approved archive manifest bila ada | Audit retention independen; purge tidak tersamar sebagai backup cleanup |
| Issue/support | Issue metadata dan approved attachment/object references/payload sesuai policy | Screenshot explicit user send; restricted access |
| Release artifacts | Immutable app artifact, digest/provenance, SBOM, config schema, compatibility manifest | Same artifact promoted Dev→QA→Production |
| Migration bundle | Reviewed migration artifact, order/dependency manifest, checksum, validation and recovery notes | No handwritten Production patch |
| Monitoring/operations config | Approved dashboards/alerts/health definition references without secret | Telemetry data retention terpisah |

## 7.2 Explicit exclusions/clarifications

- Client local workspace, draft, undo/redo, cache, temp, checkpoint utama, source file, clipboard, password, token, dan signature asset bukan server backup otomatis.
- Local migration backup adalah protected device recovery artifact, bukan enterprise server backup dan dikecualikan dari automatic cloud/device backup unless policy approves.
- `Amankan ke Akun` hanya mencakup hasil yang dipilih user dan metadata selective sync yang disetujui. Full Cloud Workspace tetap Future.
- Personal History bukan backup, Enterprise Audit, atau proof bahwa file masih tersedia.
- SQL-only backup tidak cukup untuk memulihkan secured files/issue attachments.
- Object-only copy tanpa SQL metadata, owner/version/key/tombstone/retention mapping tidak cukup.

## 7.3 Consistency unit

Setiap recovery point mempunyai manifest yang menghubungkan SQL backup/log position, storage inventory/reference generation, key/config/app compatibility version, migration history, Audit/archive state, issue attachment state, artifact version, timestamp, environment, integrity result, and retention/hold. Exact technology/topology tidak dipilih dalam report ini.

---

# 8. Backup security

## 8.1 Required controls

Backup harus:

1. encrypted in transit and at rest dengan approved cryptography;
2. access-controlled memakai least-privilege backup/restore identity yang terpisah dari runtime dan developer;
3. integrity/authenticity checked saat create, transfer, retention scan, dan sebelum restore;
4. separated from active data/failure domain sesuai approved design;
5. monitored untuk failure, age/freshness, capacity, integrity, retention, and restore-test status;
6. retained/disposed berdasarkan versioned policy dan legal/investigation hold;
7. auditable dengan safe metadata—tanpa payload/secret;
8. restorable melalui test evidence, bukan hanya `backup completed`;
9. protected dari overwrite/delete oleh normal application identity;
10. mapped to environment dan data classification.

## 8.2 Separation of access

Mampu membaca backup, menjadi DBA/operator, atau memiliki storage administration tidak otomatis memberi permission bisnis untuk membuka user document. Restore staging, catalog, key access, and validation account dibatasi. Content validation menggunakan digest/count/authorized synthetic or application-level checks whenever possible; document browsing bukan default.

## 8.3 Key and secret boundary

- Keys separated from data; backup only opaque key/version references unless approved key-recovery facility governs key material.
- Secret/connection/provider credential tidak berada dalam migration bundle, release artifact, SBOM, log, Audit, Issue, screenshot, atau evidence.
- Missing key/secret causes safe failure; tidak ada cleartext or public-provider fallback.
- Rotation/rewrap plan harus menjaga retained backup dapat direstore selama policy mewajibkan.
- Exact key custody, escrow, recovery, rotation, and break-glass require Pertamina confirmation.

## 8.4 Backup evidence minimum

Backup set/reference, environment, scope, recovery-point timestamp, source versions, encryption/integrity status, manifest digest, retention/hold policy version, execution function, approval/policy reference, monitoring status, last restore-test reference, and exception/incident link. Jangan masukkan key, password, token, physical storage path, SAS URL, content, atau raw diagnostics.

---

# 9. Restore process

## 9.1 Authorized flow

```text
Incident/Request
→ Authorization
→ Select Recovery Point
→ Validate Backup
→ Prepare Environment
→ Restore
→ Apply Compatible Application Version
→ Validate Database
→ Validate Storage
→ Validate Security
→ Smoke Test
→ Monitoring
→ Communication
→ Closure
```

## 9.2 Phase controls

| Phase | Required decision/evidence |
|---|---|
| Incident/Request | Reason, impact, scope, environment, Incident/Change reference, data-loss risk |
| Authorization | Requester, accountable approver, executor, data/security review, exact scope and recovery point |
| Select Recovery Point | Business/data cutoff, RPO comparison, app/schema/key/storage compatibility, post-point changes at risk |
| Validate Backup | Catalog/readability, integrity/authenticity, completeness, key access, chain/manifest consistency |
| Prepare Environment | Isolation, capacity, network/identity, secret/config access, artifact availability, monitoring and stop plan |
| Restore | Controlled identity/path, phase timestamps, error/status, no unauthorized exposure |
| Compatible Application | Exact application/config/schema/capability versions; no arbitrary latest/old binary assumption |
| Database validation | Schema/history/constraint/grants/row/invariant/outbox/idempotency/job/audit checks |
| Storage validation | Existence/version/digest/key/owner/state/tombstone/hold/orphan/max-three reconciliation |
| Security validation | AuthN/AuthZ, object ownership, least privilege, deleted data non-addressable, safe Audit/monitoring |
| Smoke | Bounded representative approved cases; Production uses read-only/synthetic canary only |
| Monitoring | Observation condition, backlog/queue/errors/resources/security/Audit health |
| Communication | Safe status, scope, limitation, user/operations action, next checkpoint |
| Closure | Validation complete, risk accepted by authority, evidence linked, incident/change/Problem updated |

## 9.3 Restore safety rules

- Restore is a deployment/incident operation and follows **Approved Deployment Procedure Required.**
- Do not overwrite the current Production environment without exact target resolution and authorization.
- Prefer isolated restore/validation when architecture permits; physical method is not selected here.
- Freeze unsafe writes/publication/purge when integrity is uncertain.
- Record post-recovery data gap and operations after recovery point; do not hide them.
- Restore failure never triggers repeated destructive attempts without reassessment.
- Database online, object count present, or health endpoint green alone is not PASS.

---

# 10. Restore validation

## 10.1 Minimum validation matrix

| Area | Checks | PASS condition |
|---|---|---|
| Schema version | Per-module migration history, expected schema, constraint/index compatibility | Exact expected versions; no drift/unknown partial migration |
| Row/integrity | Bounded counts, referential/domain invariant, uniqueness, rowversion, critical aggregate | Reconciled against recovery manifest/oracle |
| Storage reference | SQL reference resolves to valid authorized object | No orphan-addressable or metadata false availability |
| Secured-file access | Owner/current permission, encrypted read path, download/open via application | Correct owner allowed; wrong owner denied |
| Version count | `secured.Files`/versions, current pointer, tombstone, max-three | Maximum three active/addressable and current pointer valid |
| Account/permission metadata | Identity/admin/config state, revocation, assignment, policy version | No privilege elevation/stale unauthorized grant |
| Job state | Job/attempt/lease/checkpoint/artifact/reservation | No duplicate active attempt, false completed, stale lease, unsafe resume |
| Idempotency/outbox | Unique logical effect, pending/failed/replay state | Reconciliation can progress without duplicate effect |
| Audit availability | Schema, append, query, retention/hold, backlog/health | Mandatory intent durable and append still works |
| Issue data | Issue metadata, attachment references, access/triage status | Message/attachment separation and scope preserved |
| API readiness | `/health/ready`, compatibility contract, authorized metadata query | Fresh readiness and expected app/API contract |
| Worker readiness | Identity/version/lease/queue/provider/capability | Can safely accept supported work; no stale worker |
| Core smoke | Merge PDF local plus selected server/hybrid route | Source safe; expected result/state; no false success |
| Local Worker compatibility | Client/IPC/engine/schema/capability compatibility | Supported client path functions or is explicitly blocked |
| Security | SSO/auth, direct API denial, cross-user denial, secret/config boundary | No unauthorized access or weak fallback |
| Monitoring | API/Worker/SQL/storage/queue/Audit/provider signals fresh | No critical blind spot; thresholds policy-referenced |

## 10.2 Validation statuses

Gunakan `PASS`, `FAIL`, `BLOCKED`, atau `NOT_APPLICABLE` dalam evidence. `NOT_APPLICABLE` memerlukan scope reason. `BLOCKED` bukan PASS. Failure pada authorization, integrity, max-three, deleted/tombstoned addressability, Audit durability, application compatibility, or representative Core path membuat restore tidak dapat dinyatakan sukses.

## 10.3 Recovery objective evidence

Catat actual recovery-point gap dan elapsed recovery/validation time sebagai measured evidence. Jangan menyebut hasil memenuhi SLA/RPO/RTO sampai official target, clock boundary, service calendar, scope, and authority disetujui.

---

# 11. Application/config rollback

## 11.1 Decision matrix

| Option | Typical trigger | Main risk/data impact | Downtime | Approval | Validation | Communication |
|---|---|---|---|---|---|---|
| App-only rollback | New binary defect; schema remains backward compatible | Old app may not understand new schema/config/job/checkpoint | `TARGET_TO_BE_APPROVED`; evidence-based | Deployment approval; schema compatibility owner | Startup/readiness, API contract, Worker/client, Core smoke, no state regression | Version/scope/reason/known limitation/next step |
| Config rollback | Typed config change causes regression | Inverse may not restore side effects; stale cache/revision | Policy/config dependent | Configuration change authority; reauth if high impact | Effective provenance/version, impacted Features, permission/Audit, smoke | Exact old/new value category, not secret |
| Feature-flag disable | Scoped Feature path unsafe; compatible alternative exists | Active jobs/workflows, hidden data/state, broad disable | Usually low but not assumed | `configuration.feature.update` path and high-impact approval when broad | Exposure/capability, existing job behavior, local route, no security bypass | Affected Feature/platform, safe workaround, review/expiry |
| Provider fallback | Approved provider unavailable/defective | Output/fidelity/license/data-boundary incompatibility | Provider dependent | Approved provider profile and security/architecture decision | Contract, output quality, privacy, resource, no public AI | Provider profile category and affected capabilities only |
| DB rollback | Additive/reversible schema change proven safe | Down operation may lose data or break current app | Migration dependent | Destructive migration authority | Full DB/app/storage/security validation | Data/availability impact and limitation |
| DB restore | Corruption/irrecoverable migration/data state | Loss of post-recovery-point changes; cross-store mismatch | `TARGET_TO_BE_APPROVED` | Incident + backup/restore + deployment authority | Section 10 full validation | Recovery point, data gap, status, observation |
| Data repair | Narrow verified inconsistency not safely solved by restore | Incorrect selection/repair can worsen integrity; Audit/evidence risk | Scope dependent | Data/security/business/operations authority | Before/after invariant, object/Audit/app checks, regression | Scope, outcome, remaining risk—no content |

## 11.2 Selection rules

1. Prefer the least destructive option that restores correct and secure behavior.
2. Do not roll application back across an incompatible contracted schema.
3. Do not use feature flags to bypass permission, validation, quarantine, Audit, encryption, rate limit, or public-AI block.
4. Provider fallback is only an approved local/internal compatible provider; public AI endpoint remains `NO`.
5. Config rollback is a new governed change, not deletion of the original Audit trail.
6. When the choice can lose post-point data or broaden outage, escalate through Document 16 and record decision authority.

---

# 12. Database rollback

## 12.1 Honest reversibility classification

| Change type | Likely recovery posture |
|---|---|
| Pure additive compatible object | Roll application forward/back only if compatibility tests pass; schema may remain expanded |
| Index/nonsemantic optimization | Remove/recreate only through reviewed migration if operationally safe |
| Backfill preserving old source | Switch/read rollback may be possible while old representation is intact and reconciled |
| Rename via add/copy/switch | Revert switch only if writes have stayed consistent; otherwise compensation/forward-fix |
| Type narrowing/data rewrite | Usually not safely down; forward-fix, restore, or governed repair |
| Drop/removal | Restore or forward reconstruction from verified retained source; no assumption of reverse migration |
| Cross-store reference migration | Pointer rollback only if old object/reference/key remains valid and authorized |

## 12.2 Decision sequence

```text
Detect failure/incompatibility
→ stop unsafe writes and preserve evidence
→ identify commit/phase and authoritative state
→ determine whether business data changed
→ compare forward-fix, application rollback, compensation, data repair, and restore
→ assess post-point data loss and cross-store impact
→ obtain approval
→ execute only through controlled deployment procedure
→ validate Section 10 and observe
```

## 12.3 Rules

- Generated `Down` method or reverse script is not proof of safe rollback.
- If the migration committed data transformation, do not execute blind down migration.
- Restore requires coordinated SQL/object/config/key/application recovery point; SQL-only rewind may create orphan, resurrection, or permission/version mismatch.
- Compensation appends corrective state through owner contract; it does not rewrite Audit history.
- Data repair requires exact affected-set proof, read-only preview, expected before/after invariant, approval, bounded execution, retry/recovery plan, and independent validation.
- Manual Production SQL is not the baseline and is not included here.
- If rollback cannot preserve new data, business/security impact and recovery-point gap require explicit human decision.

---

# 13. Disaster Recovery inputs

## 13.1 Recovery dependency sequence

1. Network/identity dependency
2. Secret/config access
3. SQL Server
4. Secure Storage
5. API
6. Worker
7. Web
8. Monitoring
9. Audit pipeline
10. Client compatibility
11. Smoke test

Sequence dapat mempunyai parallel preparation only when dependency and security analysis proves it safe; validation authority tetap mengikuti persisted state, object consistency, and current authorization.

## 13.2 Required DR inputs

| Input | Status |
|---|---|
| RPO | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| RTO | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| DR location/geography/data residency | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| Replication model and consistency behavior | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| SQL edition/topology/recovery model/HA | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| Storage replication/versioning/immutability/offline copy | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| Key/KMS/HSM recovery and regional dependency | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| Identity/network/DNS/certificate dependencies | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| DR invocation, owner, delegation, SoD, communication | `REQUIRES_PERTAMINA_ORGANIZATION_CONFIRMATION` |
| Backup schedule/retention/restore-test cadence | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| Recovery priority and service degradation policy | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| Client minimum/supported version during DR | `TARGET_TO_BE_APPROVED` |

## 13.3 DR validation principles

- Drill on controlled non-Production or approved isolated environment with synthetic/sanitized data.
- Validate full dependency chain and application business outcome; infrastructure-only startup is insufficient.
- Measure recovery-point gap, restore duration, validation duration, and observation result without claiming official RPO/RTO.
- Test primary loss, dependency loss, corrupted/invalid backup, missing key/config, storage mismatch, incompatible app/schema, and failback decision.
- Preserve Audit/Incident/evidence separation; no content or secret in drill record.
- DR invocation, failover/failback, destructive action, and Production test require approved procedures outside this specialist report.

---

# 14. Deployment validation

## 14.1 Pre-deployment checklist

- [ ] Approved release scope, Requirement/Feature/Defect/Change links, environment, artifact/digest, pipeline, SBOM, and migration/config bundle resolved.
- [ ] Same application binaries passed Dev/QA; only environment-specific values are injected through approved boundary.
- [ ] UAT result/evidence and Production approval are distinct and available where required.
- [ ] No unresolved Blocker/Critical or disallowed security/data-loss finding.
- [ ] Dependency, license, secret/config reference, certificate, provider profile, and client compatibility ready.
- [ ] Migration plan, owner/module, dependency order, backup/recovery point, capacity/window, validation oracle, stop/rollback/forward-fix decision ready.
- [ ] Monitoring/alerts, incident escalation, runbook references, operator/support handoff, and observation ownership ready.

## 14.2 Component validation checklist

| Component | Minimum validation |
|---|---|
| API | Artifact/version, `/health/live` minimal, `/health/ready` fresh, contract/compatibility, auth, safe error/Correlation ID, rate/admission |
| Web | Correct API/environment, SSO/session, no debug/secret, responsive critical path, version/config |
| Worker | Version/identity, queue connection, lease/claim, supported engine/provider, resource limits, checkpoint compatibility |
| Database | Connectivity/TLS, expected schema/history, grants/least privilege, constraints/indexes, outbox/idempotency, no drift |
| Storage | Readiness/capacity, encrypted access, metadata/object/digest/key reference, owner/version/tombstone/hold, orphan status |
| SignalR | Authorized connection/subscription, safe event, disconnect does not stop Job, REST reconciliation |
| SSO | Approved identity route, issuer/audience/lifetime, permission revision/current session, no bypass/shared account |
| Authorization | Allow, deny, wrong owner, direct API, cross-user secured file, privileged action, current reauth |
| Audit | Mandatory intent durable, append/query/health/backlog, no secret/content canary |
| Issue Reporting | Message submit/idempotency, optional explicit attachment, reporter scope, safe diagnostics, support state |
| Secured Files | Explicit Amankan ke Akun, encrypted object, owner permission, list/download, max-three, wrong-owner denial |
| Local Worker | Client/IPC/engine/app version compatibility, source safety, local-only no upload |
| Client version | `/api/compatibility`, minimum/current/contract/capability version, supported/update behavior |
| Provider availability | Approved profile, health/circuit, capability/privacy/license; no public fallback |
| Feature flags | Effective exposure/provenance/version, no broad accidental enable, maintenance/rollback state |

## 14.3 Representative Core smoke

| Smoke | Purpose | Acceptance |
|---|---|---|
| Merge PDF local | Membuktikan Local-First core path | ≥2 synthetic PDFs; no server processing/upload; source unchanged; valid new result |
| One server/hybrid Feature | Membuktikan API/Job/Worker/storage/REST/SignalR contract when release scope includes it | Accepted durable Job, authorized status, safe event, validated result; exact Feature chosen from approved release capability |
| Amankan ke Akun | Membuktikan selective secured storage | Explicit selected result only; owner-bound; encrypted; metadata/object/version consistent |
| Issue submission | Membuktikan Support plane | Message accepted once; optional sanitized attachment explicit; Issue ID/state available |
| Audit event | Membuktikan mandatory safe evidence | Typed expected event/intent durable; query by authorized boundary; no payload/secret |

Production smoke hanya bounded read-only atau approved synthetic canary. Tidak menjalankan malicious corpus, load, fuzz, penetration, user document, debug, atau direct data mutation.

## 14.4 Post-deployment gate

- [ ] Application/config/migration/storage versions match release manifest.
- [ ] All component checks applicable are `PASS`; `BLOCKED`/`FAIL` disposition is explicit.
- [ ] Core smoke and security negative cases pass.
- [ ] Queue/jobs/outbox/Audit progress normally; no stale/duplicate/false terminal state.
- [ ] Backup/recovery reference remains valid after change.
- [ ] Observation window started with owner/escalation and rollback decision point.
- [ ] Evidence and operational handoff updated.

---

# 15. Observation window

## 15.1 Duration and ownership

Official duration: `TARGET_TO_BE_APPROVED`.

Observation window begins only after deployment/migration/smoke completion time is known. Exact duration, service calendar, thresholds, extension rule, owner/on-call, and exit authority require Pertamina confirmation. A fixed number is not invented in this report.

## 15.2 Signals

| Signal | Observe | Escalate/act when |
|---|---|---|
| Error rate | By component/version/environment/safe error class | Material deviation from approved baseline/threshold or unknown critical errors |
| API latency | Bounded endpoint categories and readiness | Sustained deviation, timeouts, not-ready, admission impact |
| CPU/RAM | Web/API/Worker/DB host or approved platform metric | Resource pressure affects latency, queue, jobs, migration, or stability |
| SQL health | Connectivity, pool, waits/locks, log/storage, query health, schema state | Not-ready, capacity/lock risk, drift, migration anomaly |
| Storage | Availability, capacity, transfer, integrity/orphan/reconcile | False available, object mismatch, capacity risk, unauthorized route |
| Queue depth | Depth/age/drain/admission | Growth without drain, stale items, capacity/backpressure breach |
| Failed jobs | Count/rate by Feature/provider/version | Spike, repeat, false terminal, broad Core impact |
| Retries | Attempts/backoff/retry reason | Hot loop, exhausted bounds, duplicate logical effects |
| Worker restarts | Restart/crash/lease/version | Repeated crash, stale completion, unavailable capability |
| Authentication failures | Safe aggregate by environment/reason | Unexpected spike, SSO/version/config issue, suspicious pattern |
| Authorization denials | Safe reason/permission category | Unexpected cross-scope pattern or newly denied required path; do not weaken controls |
| Audit backlog | Pending/failed/age/health | Mandatory intent unavailable, no forward progress, poison/version block |
| Issue spikes | Count/category/version/Feature | Release-correlated user impact or hidden failure |
| Provider health | Approved provider profile/circuit/latency/error | Unavailable/degraded or unsafe fallback pressure |
| SignalR | Connection/reconnect/error aggregate | Realtime degraded; ensure REST reconciliation and no Job failure assumption |

## 15.3 Exit/extend/rollback rules

Exit requires fresh and stable signals under approved conditions, no unresolved migration/storage/security/Audit anomaly, representative outcomes stable, queue/backlog progressing, and support/incident review complete. Extend when data is stale, workload has not reached representative level, risk remains, or corrective validation is pending. Trigger rollback/forward-fix/incident assessment when safety, integrity, authorization, availability, or critical business outcome degrades; a threshold breach is a decision input, not automatic destructive action.

## 15.4 Data boundary

Monitoring records safe metadata—Feature/Job/Correlation/Error ID, component/version/environment, outcome/count/duration/rate/bytes/queue age—and never document/OCR/signature/Assistant/Issue content, password/token/secret, raw SQL/stack/body, storage path/key, or whole Audit payload.

---

# 16. Failure scenarios

| Scenario | Immediate safe response | Recovery/decision | Validation and Document 16 link |
|---|---|---|---|
| Migration gagal sebelum commit | Stop; transaction rollback; preserve artifact/log/evidence; do not alter history manually | Diagnose precondition/artifact/schema; correct via new controlled attempt | Confirm source schema/data unchanged; create/link Incident if impact or repeated failure |
| Migration gagal sebagian | Freeze incompatible app/write path; identify committed phase/history/journal; preserve data | Forward-fix, compensation, repair, or restore based on exact state; no blind re-run | Full schema/data/storage/app/security checks; Data-Integrity Incident when uncertainty exists |
| App incompatible dengan schema | Stop/hold incompatible rollout; preserve last-known-good artifact and expanded schema | Deploy compatible app, safe app rollback only if matrix proves, or forward schema fix | Compatibility endpoint, worker/client, Core smoke; Incident escalation if Production affected |
| Storage unavailable | Stop false finalization/new secured availability; preserve SQL intent/staging; maintain valid local path | Bounded retry/backpressure/reconcile; approved storage recovery | No orphan addressability/false Completed; Storage Runbook + Incident |
| Secret missing | Fail deployment/startup safely; no hardcode/cleartext/public fallback | Restore approved secret/config access or deploy corrected reference; rotate if exposure suspected | Auth/service connectivity, no leakage; Security escalation when compromise suspected |
| Worker not ready | Stop dependent admission; keep API/read/local capability safe; do not force Job state | Correct version/identity/config/resource/provider or roll forward/back compatible worker | Lease/queue/checkpoint/job state, one active attempt; Worker Runbook |
| Queue spike | Apply approved admission/backpressure; protect foreground/local and Audit; no mass retry/delete | Scale/tune only by approved change; isolate failing class/provider; bounded drain | Depth/age/drain/retry/jobs/resources; Incident if threshold/impact trigger met |
| Client unsupported | Do not label service outage automatically; provide safe update/compatibility response | Approved minimum-version/channel rollout or compatible server path | `/api/compatibility`, capability/version, no forced update without reason |
| SignalR down | Mark realtime degraded; server Job continues; no re-execution | Reconnect with backoff/jitter; REST reconcile; bounded polling for authorized active jobs | Activity/job/issue persisted state; SignalR Runbook; Incident if broader impact |
| Audit write delayed | Preserve durable owner outbox/intent; privileged required action fails closed when intent not durable | Bounded idempotent replay/recovery; no disable/sampling/drop | Backlog/append/duplicate/health; Audit Runbook and security escalation if integrity at risk |
| Backup invalid | Do not restore; quarantine/reference invalid set; preserve evidence and alternate points | Select another verified point or rebuild backup capability through approved process | Integrity/readability/completeness/key test; Incident/PIR when recovery readiness affected |
| Restore validation failed | Keep environment isolated/not-ready; stop reopening writes/user access; preserve restored state/evidence | Diagnose, repeat only after decision; choose alternate point/forward repair/failback | Section 10 must pass; Data-Integrity/Security Incident as applicable; never declare recovered |

All failure responses use safe Error/Job/Correlation/Incident references. No password, token, document content, OCR text, signature, raw stack/SQL, physical storage key, or unrestricted diagnostic enters the incident/deployment record.

---

# 17. Evidence

## 17.1 Deployment/recovery evidence manifest

| Evidence field | Minimum content |
|---|---|
| Evidence ID/type | Unique reference and evidence category |
| Migration result | Migration ID, source/target, module/schema, outcome, timing, safe error code |
| Backup reference | Recovery point/set ID, manifest digest, integrity/verification result; no secret/path |
| Restore test result | Scope, point, environment, validation matrix, measured time/gap, result |
| Artifact/version | Build, commit, pipeline, application/Worker/client/config/schema/capability version, digest |
| Environment | Dev/QA/Production/DR lane code, topology profile reference, not credentials |
| Timestamp | UTC start/end/observed and clock source |
| Approver reference | Approved change/deployment/restore decision reference; no fake name |
| Health result | API/Worker/DB/storage/queue/Audit/provider freshness/status summary |
| Smoke result | Test ID, Feature ID, safe data manifest, result, evidence link |
| Monitoring snapshot | Bounded time window, safe aggregate metrics, baseline/threshold policy reference |
| Rollback decision | Trigger, options assessed, chosen disposition, risk/data impact, authority reference |
| Incident ID | Required when deployment/recovery is incident-driven or failure meets Incident criteria |

## 17.2 Evidence controls

- Evidence is append/revision-oriented and not silently overwritten.
- Large/restricted evidence uses approved restricted storage and opaque reference; not committed to repository by default.
- Access/classification/owner/retention/hold/expiry/disposal recorded.
- Artifact, backup, manifest, and result integrity use digest/provenance appropriate to approved toolchain.
- Failed/partial evidence remains; rerun creates linked result.
- Test and deployment evidence remain separate from Enterprise Audit, Incident, Issue, User History, and Observability while linked by safe IDs.

## 17.3 Prohibited evidence

Password, token, secret, connection string, key material, document/file bytes, OCR/extracted text, signature asset, Assistant prompt/chat, Issue message/screenshot unless in its approved restricted store, raw SQL/stack/body/claims/dump, physical path/object key/SAS URL, production dataset, or unrestricted log bundle.

---

# 18. Operational handoff

## 18.1 Minimum post-deployment package

| Item | Handoff content |
|---|---|
| Release version | Application/Worker/client/config/schema/capability versions, commit/pipeline/digest |
| Changed components | Web/API/Worker/DB/storage/client/provider/config scope and canonical Feature IDs |
| Known issues | Governed Defect/Problem/Known Error refs, impact, workaround, expiry/review, forbidden action |
| Feature flags | Effective Feature/environment/platform/scope/state/provenance and emergency-disable conditions |
| Migration result | Migration IDs, source/target, module/schema, result, duration, warnings, validation/evidence |
| Compatibility | Minimum/current client, API/contract/schema/capability/engine/provider compatibility |
| Monitoring view | Approved dashboard/alert references, freshness, key safe signals and threshold policy |
| Rollback reference | Approved plan/decision matrix, last-known-good artifact, recovery-point reference, limitation |
| Support runbook | Document 16 runbook/KB/communication refs; no Production command in handoff summary |
| Incident escalation | Triggers, Incident/Change refs, current owner function/route as officially configured |
| Observation status | Start, target policy, current signal summary, open risk, next checkpoint, exit authority |

## 18.2 Acceptance

Handoff completes only when receiving operations/support function acknowledges exact release/environment, current state, monitoring access, known risks, escalation path, rollback/recovery reference, and next checkpoint. Sending a document/link without acceptance is not handoff.

This section is a deployment-specific operational package only. Full organizational handover, repository/access inventory, knowledge transfer, side-by-side maintenance, and final acceptance belong to Document 19.

## 18.3 Security boundary

Handoff never includes secret, shared credential, document content, backup key, connection string, raw configuration value, physical storage path, or unrestricted Production log. Access is granted through approved identity/permission, not embedded in documentation.

---

# 19. Pertamina confirmation gaps

## 19.1 Backup/DR decisions

| Gap | Required marker/decision |
|---|---|
| Official RPO per data/service class | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| Official RTO and start/stop clock definition | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| SQL edition/topology/recovery model/HA/DR | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| Full/differential/log or equivalent schedule and retention | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| Backup destination, separation, immutable/offline copy | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| DR site/location, data residency, replication model and lag/consistency | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| Object storage versioning/replication/lifecycle/restore behavior | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| Key backup/escrow/recovery/rotation/DR dependency | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| Restore-test/DR drill/failover/failback cadence | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| Recovery priority and acceptable data-loss/degradation decision authority | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |

## 19.2 Deployment/operations decisions

| Gap | Required marker/decision |
|---|---|
| Deployment/migration/restore executor, approver, SoD, emergency authority | `REQUIRES_PERTAMINA_ORGANIZATION_CONFIRMATION` |
| Maintenance-window policy and user/business communication | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| Observation-window duration, thresholds, extension/exit/rollback authority | `TARGET_TO_BE_APPROVED` |
| Production access path, controlled agent, network, DB/storage administration boundary | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| Secret injection/KMS/certificate/connection custody | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| Artifact signing/provenance/SBOM format and retention | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| Client release channel/minimum version/forced-update policy | `TARGET_TO_BE_APPROVED` |
| Provider fallback profile and operational escalation | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| Backup/evidence/monitoring/incident retention and legal hold | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| Production smoke/canary scope and synthetic data policy | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| Numeric capacity/queue/resource/log-growth/latency/error thresholds | `TARGET_TO_BE_APPROVED` |

## 19.3 Evidence readiness gaps

- Exact RC/build/digest and deployed component inventory do not yet exist.
- UAT specification is FINAL, but execution/sign-off is not claimed.
- Recovery `REC-SYS-020`, migration, rollback, storage reconciliation, and representative deployment smoke have design coverage but no current release evidence.
- Document 16 tabletop is design-reviewed; operational exercise is not claimed.
- No approved backup topology, verified Production recovery point, restore-drill record, RPO/RTO evidence, or observation baseline is available in source documents.

These gaps block `DEPLOYMENT_READY`/`OPERATIONS_READY`; they do not reopen logical architecture decisions already locked.

---

# 20. Risks/conflicts

## 20.1 Inconsistency audit

| ID | Finding | Risk | Required treatment |
|---|---|---|---|
| `D17-D-001` | Document 04 uses example schema `backup.*`; Document 07 later locks secured-file schema as `secured.*` | Duplicate schema/ownership or migration applied to wrong boundary | Treat `secured.*` as canonical Document 07 ownership; report to Integrator; do not create `backup.*` silently |
| `D17-D-002` | “Backup” means both user feature Amankan ke Akun and operational backup | User expectation, wrong scope, missing local/server protection | Keep terms separate: **Secured Files/Amankan ke Akun** vs **Operational Backup/DR** |
| `D17-D-003` | `FEAT-BACKUP-006`/`FEAT-VER-004` restore user file/version, while Document 17 restores infrastructure | Wrong permission/process and accidental broad restore | Use Feature IDs only for product-level restore; infrastructure restore remains deployment/incident operation |
| `D17-D-004` | `DR-DB-005` says storage key, while Documents 06–08 prohibit exposing physical key/path | Secret-like reference leakage into API/log/evidence | Interpret as protected internal opaque storage reference; Integrator should normalize terminology without changing requirement intent |
| `D17-D-005` | Migration history per module is locked, but exact physical table/assembly naming and order are not | Implementers may invent shared history or unsafe order | Owner module defines artifact; exact convention requires controlled implementation decision, not report-created table |
| `D17-D-006` | Document 16 requires Approved Deployment Procedure, but no operational procedure/commands exist yet | False claim that report authorizes deployment | This report supplies governance/design text only; executable runbook requires approval and environment validation |
| `D17-D-007` | RPO/RTO/DR inputs are assigned to Document 17, but source provides no official values | Fake SLA/readiness claim | Keep all exact values at `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| `D17-D-008` | SQL backup may succeed while object storage/key/config is inconsistent | Incomplete restore, orphan, cross-user exposure, resurrected deletion | Require coordinated recovery manifest and full Section 10 reconciliation |
| `D17-D-009` | Down migration can be generated even when data semantics are irreversible | Data loss/corruption under time pressure | Classify reversibility honestly; prefer forward-fix; restore/repair needs explicit approval |
| `D17-D-010` | Expand/contract dual-write can become permanent or diverge | Mismatch and ambiguous authority | Define authoritative read, reconciliation, compatibility window, and contract gate |
| `D17-D-011` | Local corruption/migration failure may tempt reset/delete | Loss of drafts, unsynced intent, checkpoint, source reference | No silent reset; cache-only rebuild requires proof; preserve original/backup/artifacts |
| `D17-D-012` | Storage migration cleanup can delete old payload too early | Irrecoverable content loss | Delay cleanup until integrity/access/version/backup/observation approval; hold-aware lifecycle |
| `D17-D-013` | Operator/DBA/backup access can be mistaken for content permission | Privacy and authorization breach | Separate infrastructure access from business permission; validate via application boundary |
| `D17-D-014` | Feature-flag/config rollback can be used as emergency bypass | Security/Audit/permission weakening | Flags/config never bypass mandatory controls; mutation audited and approval-gated |
| `D17-D-015` | Green health or database online can be reported as recovered | Hidden object/auth/Audit/Core failure | Restore/deployment success requires full validation + observation |
| `D17-D-016` | No current RC/UAT/restore drill/operational tabletop evidence | Process appears complete but release is not ready | Keep status `BELUM DEPLOYMENT_READY`; collect release-specific evidence later |

## 20.2 Major residual risks

- Cross-store point-in-time consistency and key availability may dominate recovery outcome.
- Large migration/backfill can exhaust log/storage, lock critical tables, or delay foreground workload.
- Old clients/local SQLite may become incompatible after schema/contract changes.
- Restoration can resurrect deleted/tombstoned/expired data or stale permission if policy state is not reconciled.
- Audit/outbox backlog after restore may duplicate effect if idempotency and source sequence are not verified.
- Provider/engine version changes can make checkpoint/result validation incompatible.
- A solo developer may hold multiple process functions; human Production approval, security-sensitive review, and evidence independence still cannot be delegated to Codex.

## 20.3 Integrator decisions required

1. Confirm `secured.*` as the physical/logical schema term replacing the Document 04 example `backup.*` in downstream implementation references—without editing canonical Feature IDs.
2. Normalize “storage key” to protected internal opaque storage reference wherever it could be misunderstood as an exposed path/key.
3. Define exact migration artifact/history naming and dependency orchestration within Document 07 ownership.
4. Obtain Pertamina decisions for all Section 19 inputs.
5. Define and validate executable environment-specific deployment/rollback/restore runbooks after topology and access are approved; no commands are provided here.

---

# 21. Suggested Document 17 text

Teks berikut dapat diintegrasikan ke master Document 17 setelah Integrator review. Ini adalah baseline pra-implementasi, bukan approval atau execution evidence.

## 21.1 Status and scope

> **Status:** `FINAL — PRE-IMPLEMENTATION DEPLOYMENT/RECOVERY BASELINE`  
> **Deployment readiness:** `BELUM DEPLOYMENT_READY`  
> **Release candidate:** `RC-TBD`

Document 17 mengatur controlled deployment, SQL/SQLite/storage migration, operational backup, restore, rollback, DR inputs, post-deployment validation, observation, evidence, dan deployment-specific operational handoff. Dokumen tidak memuat secret, Production SQL command, vendor choice, official RPO/RTO, atau klaim bahwa deployment/restore telah dijalankan.

## 21.2 Migration

SQL Server migration dimiliki per module/DbContext dan schema. Production application startup tidak menjalankan automatic migration. Pipeline membuat reviewed idempotent migration artifact yang ditautkan ke commit/release/digest, menguji clean dan supported prior-version paths, memverifikasi target compatibility dan recovery point, menerapkan dalam controlled order, serta mencatat evidence per environment.

Migration record minimum: Migration ID, Source Version, Target Version, Affected Schema/Module, Backward Compatible?, Data Transformation?, Estimated Impact, Required Backup, Precondition, Validation, Rollback Strategy, Owner, Evidence, dan Environment History.

Modul tidak mengubah tabel modul lain. Cross-module FK/transaction/shared-table mutation diminimalkan. Cross-store operation menggunakan staging, validation, metadata commit, promotion, outbox/reconciliation—bukan transaksi panjang SQL + storage.

## 21.3 Expand/contract and destructive change

Expand/contract digunakan bila compatibility lintas release atau data backfill memerlukannya: add → compatible code → bounded migrate/reconcile → switch → remove pada release berikut setelah compatibility window. Pola tidak dipaksakan pada additive change sederhana.

Drop, type narrowing, rewrite, replacement, large move, key/reference remap, dan destructive change memerlukan explicit review, verified recovery point, impact/data-loss analysis, QA dry run, approved maintenance window bila relevan, stop condition, validation, dan forward-fix/restore/repair plan. Codex tidak menjalankan destructive migration.

## 21.4 SQLite

SQLite menggunakan per-profile store, stable migration ID/checksum, `schema_migration` durable history, optional `PRAGMA user_version` marker, exclusive migration lease, preflight, protected DB+artifact manifest backup, transaction untuk bounded change, dan resumable `migration_journal` untuk large file/data work. Failure preserves original/backup/draft/source; no silent reset or unsupported downgrade. Cache-only rebuild hanya jika cache authority dapat dibuktikan.

## 21.5 Storage migration

Perubahan provider/path/reference/key/version melakukan authorized metadata mapping dan staged payload copy, digest/encryption/access verification, owner/version/tombstone/retention/hold reconciliation, pointer switch melalui owner module, bounded retry, orphan detection, rollback reference, observation, dan cleanup hanya setelah approval. Physical path/key tidak diekspos.

## 21.6 Backup and security

Operational backup mencakup SQL Server/log sesuai approved recovery model, secure object/file storage, versioned non-secret configuration, secret-recovery process reference, Audit, Issue/attachment sesuai policy, release artifacts, migration bundle, and recovery manifest. Client local workspace bukan server backup. Amankan ke Akun hanya selected user result, bukan full workspace or enterprise backup.

Backup encrypted, access-controlled, integrity-checked, separated from active data, monitored, policy-retained, auditable, and restore-tested. Backup/DB/operator access does not grant user-content permission. Secret/key material stays in approved credential/key boundary.

## 21.7 Restore

```text
Incident/Request → Authorization → Select Recovery Point
→ Validate Backup → Prepare Environment → Restore
→ Apply Compatible Application Version
→ Validate Database → Validate Storage → Validate Security
→ Smoke Test → Monitoring → Communication → Closure
```

Restore success requires expected schema/history/constraints/grants; row/integrity checks; object/version/digest/key/owner/tombstone/retention/hold consistency; max-three secured versions; account/permission state; Job/outbox/idempotency; Audit/Issue availability; API/Worker/client readiness; Core smoke; no unauthorized access; and healthy monitoring. Database online alone is not success.

## 21.8 Rollback

Choose the least destructive approved option among app-only rollback, typed config rollback, Feature-flag disable, approved provider fallback, DB rollback, DB restore, forward-fix, compensation, or data repair. Record trigger, risk, data impact, downtime, approval, validation, and communication. Generated down migration is not proof of reversibility. After data transformation, forward-fix is preferred unless restore/repair is explicitly safer and approved. Public AI is never a fallback.

## 21.9 Disaster Recovery

Recovery sequence: network/identity → secret/config → SQL Server → secure storage → API → Worker → Web → monitoring → Audit pipeline → client compatibility → smoke test.

Exact RPO, RTO, DR location, replication model, topology, schedule, immutable/offline copy, key recovery, and drill cadence are `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION`.

## 21.10 Deployment validation and observation

Validate API, Web, Worker, Database, Storage, SignalR, SSO, Authorization, Audit, Issue Reporting, Secured Files, Local Worker compatibility, Client version, approved Provider, and Feature flags. Representative smoke covers Merge PDF local, one approved server/hybrid Feature when applicable, Amankan ke Akun, Issue submission, and typed Audit evidence.

Observation monitors error rate, API latency, CPU/RAM, SQL/storage, queue depth, failed Jobs, retries, Worker restart, authentication failure, authorization denial, Audit backlog, Issue spike, provider health, and SignalR degradation. Official observation duration remains `TARGET_TO_BE_APPROVED`; monitoring never records content or secret.

## 21.11 Failure and incident handoff

Migration partial failure, incompatible app/schema, storage/secret/Worker/queue/client/SignalR/Audit failure, invalid backup, or failed restore validation preserve source/state/evidence, stop false success, and invoke Document 16 when incident criteria are met. Rollback, hotfix, migration, infrastructure change, restore, secret/config rollout, or Production restart: **Approved Deployment Procedure Required.**

## 21.12 Evidence and handoff

Evidence records Migration result, Backup reference, Restore test result, Artifact/version, Environment, Timestamp, Approver reference, Health result, Smoke result, Monitoring snapshot, Rollback decision, and Incident ID when relevant. Sensitive payload is prohibited.

Post-deployment handoff supplies release version, changed components, known issues, Feature flags, migration result, compatibility, monitoring view, rollback reference, support runbook, incident escalation, and observation status. Full Handover Checklist remains Document 19.

## 21.13 Traceability

| Area | Requirements/features/documents |
|---|---|
| Deployment/pipeline | `IR-DEVOPS-001..004`, `NFR-ENV-001..004`, Document 08/12 |
| Database/storage | `DR-DB-001..007`, Document 07 |
| Backup/Secured Files | `FR-BACKUP-001..010`, `FEAT-BACKUP-001..013`, `FEAT-VER-001..006` |
| Recovery | `FR-REC-001..006`, `FEAT-REC-001..015`, `REC-SYS-001..020` |
| Monitoring | `NFR-MON-001..004`, Documents 08/10/12/16 |
| Release/UAT | Global acceptance Document 02, release gates Document 12, UAT entry/exit Document 13 |
| Incident | Document 16 Sections 28–31, 43–47, 51–55, 59–60 |
| Handover | `NFR-HAND-001`; Document 19 boundary |

## 21.14 Acceptance checklist

- [ ] Migration ownership/history/artifact/compatibility and recovery plan approved.
- [ ] SQL/SQLite/storage migration tests and representative prior-version evidence pass.
- [ ] Backup scope/security/integrity and verified recovery point exist.
- [ ] Restore drill validates SQL + storage + security + application + Core outcome.
- [ ] Rollback/forward-fix/data-repair decision matrix is release-specific.
- [ ] DR inputs, RPO/RTO, topology, authority, and cadence are approved.
- [ ] Component validation, security negative checks, and representative smoke pass.
- [ ] Observation duration/threshold/owner/escalation are approved and evidence is stable.
- [ ] Operational handoff is acknowledged; Document 16 references are current.
- [ ] No secret/content/raw diagnostic appears in artifact, logs, Audit, evidence, or handoff.
- [ ] UAT and Production approval are recorded separately.
- [ ] No unresolved safety, integrity, compatibility, recovery, or ownership gap remains.

---

**Serah terima kepada Integrator:** pertahankan `secured.*` sebagai canonical schema Document 07, bedakan Secured Files dari operational backup, normalkan istilah internal storage reference, konfirmasikan seluruh input Section 19, dan jangan mengubah status menjadi deployment-ready sampai migration/backup/restore/rollback/DR/smoke/observation evidence release-specific tersedia. Tidak ada Feature ID, endpoint, role, permission, table, vendor, SLA, command Production, atau master Document 17 baru yang dibuat oleh report ini.
