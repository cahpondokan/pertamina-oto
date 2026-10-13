# APPLICATION CARTOGRAPHY DECISION REGISTER
## PERTAMINA OTO — Document 18

> Versi: FINAL v1.0
> Status: 30/30 AUTO-LOCKED
> Tanggal: 1 September 2026
> Baseline: 550 Feature ID canonical; 33 Primary PDF Tool Cards

AUTO-LOCK hanya berlaku untuk keputusan dokumentasi/pemetaan yang diturunkan dari Dokumen 01–17. Register ini tidak menambah module, deployable, database, endpoint, provider, role, permission, security zone resmi, atau Production topology.

| ID | Keputusan | Status | Dasar |
|---|---|---|---|
| CART-DEC-001 | Document 18 berstatus FINAL — PRE-IMPLEMENTATION APPLICATION CARTOGRAPHY BASELINE | AUTO-LOCKED | Readiness Dokumen 01–17 |
| CART-DEC-002 | Gunakan ID internal CART-* tanpa mengganti canonical ID | AUTO-LOCKED | Prompt Dokumen 18 |
| CART-DEC-003 | Cartography memakai multi-view, bukan satu diagram raksasa | AUTO-LOCKED | Readability/governance |
| CART-DEC-004 | Diagram bukan source of truth | AUTO-LOCKED | Traceability principle |
| CART-DEC-005 | Actor dibedakan dari workload/service identity | AUTO-LOCKED | Dokumen 08–10 |
| CART-DEC-006 | Module baseline tetap 12 module yang sudah ditetapkan | AUTO-LOCKED | Dokumen 04 + prompt |
| CART-DEC-007 | ContentIntelligence tetap terpisah dari Assistant | AUTO-LOCKED | Dokumen 04/08 |
| CART-DEC-008 | Building Blocks tetap domain-neutral dan tanpa mutable business state | AUTO-LOCKED | Dokumen 04–05 |
| CART-DEC-009 | Logical hosts meliputi Web, API, Worker, Desktop, Mobile, dan Local Worker | AUTO-LOCKED | Dokumen 04/17 |
| CART-DEC-010 | Local Worker ditampilkan walau exact repository path masih TBD | AUTO-LOCKED | Dokumen 17 |
| CART-DEC-011 | 550 Feature dipetakan melalui exact ID + governed family overlay | AUTO-LOCKED | Dokumen 03 |
| CART-DEC-012 | 33 Core memakai exact canonical mapping dan bundle resmi | AUTO-LOCKED | Dokumen 03 |
| CART-DEC-013 | UI card tidak menjadi endpoint/service/engine/table otomatis | AUTO-LOCKED | Dokumen 03/04/06 |
| CART-DEC-014 | Local-only Feature tidak mempunyai server-processing API | AUTO-LOCKED | Local-First + Dokumen 06 |
| CART-DEC-015 | REST/persisted state adalah server authority | AUTO-LOCKED | Dokumen 06 |
| CART-DEC-016 | SignalR hanya versioned at-least-once notification dengan REST recovery | AUTO-LOCKED | Dokumen 06 |
| CART-DEC-017 | Desktop–Local Worker memakai typed Named Pipe/local IPC | AUTO-LOCKED | Dokumen 04/08/17 |
| CART-DEC-018 | Engine/provider selalu di belakang adapter/resolver | AUTO-LOCKED | Dokumen 04–05 |
| CART-DEC-019 | Public AI dan silent public fallback dilarang | AUTO-LOCKED | Dokumen 04/08 |
| CART-DEC-020 | Assistant hanya memanggil canonical application service | AUTO-LOCKED | Dokumen 04/06/08 |
| CART-DEC-021 | SQL, SQLite, Local Workspace, dan Secure Storage mempunyai authority berbeda | AUTO-LOCKED | Dokumen 07 |
| CART-DEC-022 | Schema canonical memakai secured.*, bukan backup.* alternatif | AUTO-LOCKED | Dokumen 07/17 |
| CART-DEC-023 | Audit, History, telemetry, dan Issue tetap empat plane terpisah | AUTO-LOCKED | Dokumen 07/08/10 |
| CART-DEC-024 | Trust zone pada Dokumen 18 bersifat konseptual | AUTO-LOCKED | Official network belum tersedia |
| CART-DEC-025 | Hostinger KVM 2 hanya temporary non-Production | AUTO-LOCKED | Dokumen 17 |
| CART-DEC-026 | Production memakai placeholder PERTAMINA_APPROVED_PRODUCTION_INFRASTRUCTURE | AUTO-LOCKED | Dokumen 17 |
| CART-DEC-027 | Owner, CMDB, Application ID, criticality, dan classification tetap TBD resmi | AUTO-LOCKED | Anti-invention rule |
| CART-DEC-028 | Lifecycle tidak boleh dinaikkan ke Production tanpa evidence | AUTO-LOCKED | Dokumen 11–17 |
| CART-DEC-029 | Cartography diperbarui melalui impact analysis, regeneration, review, version, publish | AUTO-LOCKED | Maintenance governance |
| CART-DEC-030 | FINAL tidak sama dengan CARTOGRAPHY_COMPLETE, PRODUCTION_READY, atau HANDOVER_READY | AUTO-LOCKED | Known implementation/owner/topology gaps |

## Konflik yang diselesaikan secara logis

| Konflik | Disposisi |
|---|---|
| ContentIntelligence tidak tampak pada conceptual tree | Ditampilkan sebagai module baseline; exact repository placement tetap gap, tanpa membuat module baru |
| Local Worker tidak tampak pada conceptual host tree lama | Ditampilkan karena Dokumen 17 sudah mengunci deployable boundary |
| backup.* vs secured.* | secured.* dipakai sebagai schema canonical |
| UI PRIMARY_TOOL 70 vs Core 33 | Core hanya berasal dari daftar 33 Primary PDF Tool Cards |
| Data-owner label tidak one-to-one dengan module | Data ownership Dokumen 07 dipertahankan; exact module anchor tidak dikarang |
| SignalR vs persisted Job state | Persisted state menang; SignalR hanya notifikasi |

## Item yang tidak di-lock

- pemilik/organisasi resmi;
- Application ID/CMDB;
- official data classification/criticality;
- exact Production topology, network, DNS/IP/port/firewall;
- vendor/provider/storage/queue/monitoring product;
- API/internal event/IPC contract yang belum ada;
- actual repository path, artifact, version, evidence, PoC, test, UAT, atau operational drill.

