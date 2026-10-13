# DATABASE DECISION REGISTER
## PERTAMINA OTO — Office Tools Organizer

> **Document:** 07 Database Design  
> **Version:** 1.0  
> **Date:** 24 Agustus 2026  
> **Status:** 12/12 AUTO-LOCKED  
> **Decision mode:** Technical derivation from Documents 01–06 FINAL  
> **Canonical feature baseline:** 550

---

# 1. Lock Rule

Keputusan di bawah tidak mengubah scope, arsitektur, vendor, standar Pertamina, Local-First, atau kontrak API. Semua keputusan merupakan penurunan teknis yang aman dari baseline FINAL.

Exact retention duration, encryption provider, SQL Server edition/topology, RPO/RTO, connection-pool size, batch size, dan performance target numerik tetap menjadi governed configuration/evidence. Nilai tersebut tidak ditebak di Document 07.

---

# 2. Decision Summary

| ID | Decision | Status |
|---|---|---|
| `DB-AUTO-001` | Satu SQL Server baseline, schema dan write owner per module | **AUTO-LOCKED** |
| `DB-AUTO-002` | SQL metadata + secure object payload; no SQL blob by default | **AUTO-LOCKED** |
| `DB-AUTO-003` | Internal narrow key + opaque public resource ID | **AUTO-LOCKED** |
| `DB-AUTO-004` | In-module FK; cross-module logical reference + owner contract | **AUTO-LOCKED** |
| `DB-AUTO-005` | Domain-owned idempotency and transactional outbox | **AUTO-LOCKED** |
| `DB-AUTO-006` | Persisted state, rowversion, short transaction, lease/sequence guards | **AUTO-LOCKED** |
| `DB-AUTO-007` | DbContext and migration ownership per module boundary | **AUTO-LOCKED** |
| `DB-AUTO-008` | SQLite per local profile/device; local files remain in local filesystem | **AUTO-LOCKED** |
| `DB-AUTO-009` | Explicit secured transfer; no generic workspace/full-table sync | **AUTO-LOCKED** |
| `DB-AUTO-010` | Personal History, Enterprise Audit, Support, and Observability separated | **AUTO-LOCKED** |
| `DB-AUTO-011` | Maximum three active/addressable secured-file versions | **AUTO-LOCKED** |
| `DB-AUTO-012` | Evidence-based indexes, keyset pagination, bounded cleanup/archive | **AUTO-LOCKED** |

---

# 3. Decision Records

## DB-AUTO-001 — SQL Server Topology and Module Ownership

**Status:** AUTO-LOCKED  
**Decision:** Gunakan satu Microsoft SQL Server database sebagai baseline awal. Pisahkan data menggunakan schema dan satu write owner per module.

```text
identity
admin
configuration
workspace
jobs
secured
history
audit
support
assistant
intelligence
```

Schema adalah logical ownership boundary dalam Modular Monolith, bukan alasan membuat microservice baru. Client tidak mengakses SQL secara langsung. Modul lain memakai application contract/internal event.

**Basis:** `DR-DB-001`, Solution Design §§7–9, `NFR-ARCH-001..005`.  
**Guard:** SQL Server edition, HA topology, dan physical separation memerlukan deployment/capacity evidence.

## DB-AUTO-002 — Payload Placement

**Status:** AUTO-LOCKED  
**Decision:** SQL menyimpan metadata, ownership, state, version, checksum, retention, dan opaque internal storage reference. File/document, checkpoint berisi content, issue attachment, export, OCR text, summary, dan translation result berada di local filesystem atau approved secure file/object storage.

**Basis:** `DR-DB-003..005`, `DR-DB-007`, Solution Design §§8–9, API Document 06 §20.  
**Guard:** Exception SQL blob memerlukan controlled architecture/security decision; bukan default implementation choice.

## DB-AUTO-003 — Key Strategy

**Status:** AUTO-LOCKED  
**Decision:** Tabel server ber-volume tinggi menggunakan narrow insertion-friendly internal key, ditambah opaque public ID dengan unique constraint. Canonical `FeatureId` tetap natural contract key. SQLite menggunakan opaque locally generated `TEXT` ID.

**Basis:** API opaque identifier contract, performance and IDOR boundaries.  
**Guard:** Internal key tidak diekspos dan bukan authorization authority.

## DB-AUTO-004 — Relationship Strategy

**Status:** AUTO-LOCKED  
**Decision:** FK dan cascade hanya di dalam satu ownership boundary. Cross-module relationship menyimpan logical public ID dan divalidasi melalui owner contract. Default delete behavior adalah `RESTRICT`/`NO ACTION`.

**Basis:** Solution Design §8.2.  
**Guard:** Audit dan independent-retention data tidak boleh hilang melalui cascade.

## DB-AUTO-005 — Idempotency and Outbox Ownership

**Status:** AUTO-LOCKED  
**Decision:** Tidak ada satu global table yang ditulis semua module. Idempotency record dan outbox mengikuti owning aggregate (`jobs`, `secured`, `support`, dan module mutating lain bila diperlukan), sehingga state + record/outbox dapat commit dalam satu transaction.

**Basis:** `API-DEC-005`, `API-DEC-008`.  
**Guard:** Raw idempotency key, raw request, secret, dan content tidak disimpan. Simpan scoped key hash, operation hash, request fingerprint, safe response identifiers, state, dan expiry.

## DB-AUTO-006 — Concurrency and Transaction Model

**Status:** AUTO-LOCKED  
**Decision:** Mutable aggregate memakai SQL Server `rowversion`/SQLite integer row version. Append-only stream memakai unique event/sequence. Job claim, upload offset, quota reservation, version rotation, cleanup, dan outbox claim memakai short atomic transaction/lease.

**Basis:** durable Job/API state, `If-Match`, retry/resume, maximum-three versions.  
**Guard:** Tidak ada database transaction atau connection selama upload stream, file I/O, engine processing, AI/provider call, SignalR delivery, atau user think-time. `READ_COMMITTED_SNAPSHOT` hanya diaktifkan setelah DBA review dan load test.

## DB-AUTO-007 — EF Core and Migration Ownership

**Status:** AUTO-LOCKED  
**Decision:** DbContext, mapping, migration assembly, dan migration history dipisahkan per module ownership boundary. Cross-module navigation/DbSet dan production startup migration dilarang.

**Basis:** `DR-DB-002`, modular ownership.  
**Guard:** Production memakai reviewed idempotent script/bundle melalui pipeline, backup, compatibility check, dan expand-contract migration.

## DB-AUTO-008 — Local SQLite Boundary

**Status:** AUTO-LOCKED  
**Decision:** Desktop dan Mobile/Tablet memakai SQLite per OS profile + Pertamina subject profile untuk metadata/state lokal. File bytes dan large artifacts tetap di app-managed local filesystem. Satu serialized persistence writer digunakan per profile.

**Basis:** `TECH-DEC-011`, `FR-STOR-001..006`, `FR-REC-001..006`.  
**Guard:** DB/WAL/backup tidak berada pada cloud-synced path; account switch tidak mencampur profile data.

## DB-AUTO-009 — Selective Sync

**Status:** AUTO-LOCKED  
**Decision:** Tidak ada generic database/table replication. Content outbound hanya melalui explicit eligible `secured_transfer` setelah user memilih **Amankan ke Akun**. Offline enterprise audit memakai metadata-only outbox terpisah. Allowlisted lightweight account preference dapat memakai typed `/account/preferences`; tidak memberi sync mapping kepada draft/source/workspace content.

**Basis:** `FR-BACKUP-001..010`, API Document 06 §§10.4, 16.1.  
**Guard:** Source, draft, undo/redo, cache, thumbnail, temp, checkpoint, clipboard, password, signature asset, local history, dan Assistant conversation tidak auto-sync.

## DB-AUTO-010 — Data-Plane Separation

**Status:** AUTO-LOCKED  
**Decision:** Personal History, Enterprise Audit, Support Issue, dan Observability memiliki owner, schema, retention, permission, dan deletion lifecycle terpisah.

Server `/history` adalah personal projection untuk server/account activity. Local-only processing history tetap di SQLite dan tidak otomatis diunggah. Personal History delete tidak menghapus audit, secured file, result, atau job evidence.

**Basis:** `FR-HIST-001..003`, `FR-AUDIT-001..004`, `FR-SUP-001..007`.  
**Guard:** Issue text/screenshot tidak disalin ke Audit/telemetry.

## DB-AUTO-011 — Secured Version Invariant

**Status:** AUTO-LOCKED  
**Decision:** Maksimum tiga versi terbaru yang aktif dan addressable. Pembuatan versi keempat secara atomic mengaktifkan versi baru dan men-tombstone versi aktif tertua untuk policy-driven purge/archive. Object deletion failure mempertahankan internal cleanup reference, tetapi versi tertua tidak lagi dapat diakses user.

**Basis:** `FR-VER-001..004`, `DEC-REQ-014`.  
**Guard:** Physical cleanup adalah idempotent/retryable workflow; tidak ada invisible orphan dan tidak pernah lebih dari tiga versi aktif/addressable.

## DB-AUTO-012 — Performance and Lifecycle Strategy

**Status:** AUTO-LOCKED  
**Decision:** Index mengikuti actual authorized query/filter/sort/cursor; large mutable lists menggunakan keyset pagination. Retention, cleanup, archive, export, dan orphan reconciliation berjalan bounded, batched, resumable, throttled, dan observable.

**Basis:** `NFR-PERF-003..013`, `API-DEC-006`, `API-DEC-009`.  
**Guard:** Tidak ada automatic full-text search, partitioning, columnstore, read replica, global index rebuild, deep offset, atau unbounded count/export tanpa evidence dan approval yang sesuai.

---

# 4. Governed Deferrals — Do Not Reopen Architecture

| Item | Owner document/evidence | Release guard |
|---|---|---|
| Official Pertamina data-classification labels | Document 08 Security Design | Map working controls before Production |
| SQLite encryption provider, key lifecycle, WAL/SHM protection | Document 08 + dependency review | Approved provider and security tests |
| Permission catalog/role mapping | Document 09 Role Matrix | Deny-by-default object-aware tests |
| Audit event catalog, legal retention, integrity/WORM choice | Document 10 Audit Trail Design + Compliance | No Production without approved policy |
| Numeric retention, queue, pool, batch, timeout, capacity | Test Plan/capacity policy | Benchmark and load-test evidence |
| SQL Server edition, HA, backup topology, RPO/RTO | Documents 17 and deployment/DBA review | Restore drill and approved recovery targets |
| Partitioning/compression/full-text/read replica | Measured scale-up decision | Not baseline |

These deferrals do not block the logical database design and do not authorize implementation defaults.

---

# 5. Final Status

```text
Database decisions : 12 / 12 AUTO-LOCKED
User approval       : NOT REQUIRED
Material conflict   : NONE
Feature baseline    : 550
```
