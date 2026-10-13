# REGISTER KEPUTUSAN DEPLOYMENT
## PERTAMINA OTO — Office Tools Organizer

> **Dokumen:** 17 — Deployment Guide  
> **Versi:** 1.0  
> **Tanggal:** 1 September 2026  
> **Status:** FINAL — 55/55 AUTO-LOCKED  
> **Baseline:** 550 fitur canonical; 33 Primary PDF Tool Cards; Dokumen 01–16 FINAL

---

# 1. Aturan

Keputusan ini menormalkan sumber berwenang dan laporan specialist. Register tidak memilih topology/hosting/OS/network/signing/distribution/DR Production, tidak membuat biaya/vendor/role/API/database baru, dan tidak mengotorisasi tindakan Produksi.

# 2. Keputusan

| ID | Keputusan | Status |
|---|---|---|
| `DEP-AUTO-001` | Bahasa utama Dokumen 17 adalah Bahasa Indonesia `ID-1.0` | **AUTO-LOCKED** |
| `DEP-AUTO-002` | Dokumen 17 FINAL sebagai baseline pra-implementasi; bukan `PRODUCTION_READY` | **AUTO-LOCKED** |
| `DEP-AUTO-003` | Baseline tetap 550 fitur canonical dan 33 Primary PDF Tool Cards | **AUTO-LOCKED** |
| `DEP-AUTO-004` | Azure DevOps Git dan Pipeline tetap platform repository/CI-CD baseline | **AUTO-LOCKED** |
| `DEP-AUTO-005` | Protected `main`, short-lived task branch/worktree, PR, dan human review wajib | **AUTO-LOCKED** |
| `DEP-AUTO-006` | Codex/pipeline tidak menggantikan reviewer atau approver manusia | **AUTO-LOCKED** |
| `DEP-AUTO-007` | Satu controlled build menghasilkan immutable artifact set | **AUTO-LOCKED** |
| `DEP-AUTO-008` | Artifact yang sama dipromosikan; tidak rebuild per environment | **AUTO-LOCKED** |
| `DEP-AUTO-009` | Web, API, Worker, Desktop, Local Worker, Mobile, migration, config, contract, SBOM, evidence dipaketkan terpisah dalam satu Release Manifest | **AUTO-LOCKED** |
| `DEP-AUTO-010` | Artifact tidak memuat environment secret, Production data, atau user content | **AUTO-LOCKED** |
| `DEP-AUTO-011` | Local Development, Shared Development, QA/Testing, Pilot, dan Production dipisahkan | **AUTO-LOCKED** |
| `DEP-AUTO-012` | Endpoint, identity, data, config, secret, storage, queue, provider, telemetry, dan authority terpisah per environment | **AUTO-LOCKED** |
| `DEP-AUTO-013` | Production hanya menerima exact pipeline artifact yang disetujui | **AUTO-LOCKED** |
| `DEP-AUTO-014` | Direct Production patch, binary copy, manual data edit, dan startup migration dilarang | **AUTO-LOCKED** |
| `DEP-AUTO-015` | Hostinger KVM 2 hanya temporary Dev/QA/Pilot terbatas; bukan Production baseline | **AUTO-LOCKED** |
| `DEP-AUTO-016` | Co-hosting satu node tidak menggabungkan deployable, identity, config, data, log, health, atau resource budget | **AUTO-LOCKED** |
| `DEP-AUTO-017` | Production physical topology/hosting/OS/container/gateway tetap confirmation gap | **AUTO-LOCKED** |
| `DEP-AUTO-018` | .NET LTS patch SDK dipin dan upgrade melalui controlled validation | **AUTO-LOCKED** |
| `DEP-AUTO-019` | Central Package Management, approved feed, dan resolved dependency graph menjadi build evidence | **AUTO-LOCKED** |
| `DEP-AUTO-020` | Build, Unit, Architecture, Contract, Integration, Security, Regression, Recovery, UAT readiness menjadi risk-based gates | **AUTO-LOCKED** |
| `DEP-AUTO-021` | Mandatory scanner/test unavailable berstatus `BLOCKED`, bukan PASS | **AUTO-LOCKED** |
| `DEP-AUTO-022` | Waiver tidak mengubah hasil menjadi PASS dan memerlukan authority/expiry/evidence | **AUTO-LOCKED** |
| `DEP-AUTO-023` | Artifact mempunyai strong digest, SBOM/provenance reference, immutable ID, dan restricted publication | **AUTO-LOCKED** |
| `DEP-AUTO-024` | Product, Build, Commit, Contract, Capability, Migration, Config, Desktop, Mobile, dan Worker version mempunyai fungsi terpisah | **AUTO-LOCKED** |
| `DEP-AUTO-025` | Status client compatibility hanya `SUPPORTED`, `UPDATE_AVAILABLE`, `UPDATE_REQUIRED`, `UNSUPPORTED` | **AUTO-LOCKED** |
| `DEP-AUTO-026` | Supported clients tidak diputus tanpa versioned compatibility path | **AUTO-LOCKED** |
| `DEP-AUTO-027` | Configuration/secret/policy/flag adalah environment difference; source branch bukan | **AUTO-LOCKED** |
| `DEP-AUTO-028` | Configuration typed, validated, versioned, environment-bound, dan auditable bila sensitif | **AUTO-LOCKED** |
| `DEP-AUTO-029` | Secret hanya dari approved runtime mechanism; tidak di source/artifact/log/evidence | **AUTO-LOCKED** |
| `DEP-AUTO-030` | HTTPS, trusted certificate, least-exposed endpoint, private SQL/storage, dan authenticated SignalR wajib | **AUTO-LOCKED** |
| `DEP-AUTO-031` | Exact DNS/TLS/port/firewall/proxy memerlukan network confirmation | **AUTO-LOCKED** |
| `DEP-AUTO-032` | Pertamina SSO endpoint/configuration tidak dikarang | **AUTO-LOCKED** |
| `DEP-AUTO-033` | Web/API/Worker memakai artifact, identity, config, health, logging, monitoring, drain, dan rollback boundary terpisah | **AUTO-LOCKED** |
| `DEP-AUTO-034` | Worker berhenti claim saat not-ready dan memakai graceful drain/checkpoint/lease semantics | **AUTO-LOCKED** |
| `DEP-AUTO-035` | EF Core migration adalah reviewed pipeline artifact, bukan side effect application startup | **AUTO-LOCKED** |
| `DEP-AUTO-036` | Migration mengikuti module/schema/DbContext ownership Dokumen 07 | **AUTO-LOCKED** |
| `DEP-AUTO-037` | `secured.*` adalah istilah canonical; contoh lama `backup.*` tidak dibuat sebagai schema duplikat | **AUTO-LOCKED** |
| `DEP-AUTO-038` | Expand → compatible deploy → bounded migrate → contract later digunakan bila perlu compatibility window | **AUTO-LOCKED** |
| `DEP-AUTO-039` | Forward-fix menjadi default setelah data berubah; down migration tidak diasumsikan aman | **AUTO-LOCKED** |
| `DEP-AUTO-040` | Direct Production SQL/history edit dan destructive migration otomatis dilarang | **AUTO-LOCKED** |
| `DEP-AUTO-041` | SQLite migration profile-scoped, transactional/journaled, dan tidak menghapus workspace/source diam-diam | **AUTO-LOCKED** |
| `DEP-AUTO-042` | Storage migration memakai staging, digest, reconciliation, ownership, tombstone/hold, dan delayed cleanup | **AUTO-LOCKED** |
| `DEP-AUTO-043` | “Amankan ke Akun” berbeda dari Operational Backup/DR | **AUTO-LOCKED** |
| `DEP-AUTO-044` | Backup belum siap sampai integrity, access, key, dan restore test terbukti | **AUTO-LOCKED** |
| `DEP-AUTO-045` | Restore mencakup SQL, storage, keys/refs, config, app compatibility, AuthZ, Job/outbox/idempotency, Audit, dan smoke | **AUTO-LOCKED** |
| `DEP-AUTO-046` | RPO/RTO/retention/DR site/topology tetap `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` | **AUTO-LOCKED** |
| `DEP-AUTO-047` | Rollback dipilih berdasarkan binary/config/schema/data compatibility dan Incident state | **AUTO-LOCKED** |
| `DEP-AUTO-048` | Feature flag hanya rollout/containment; tidak mengubah Feature ID/business/security/permission | **AUTO-LOCKED** |
| `DEP-AUTO-049` | Desktop package technology/distribution tetap kandidat sampai PoC/approval | **AUTO-LOCKED** |
| `DEP-AUTO-050` | Desktop dan Local Worker menjadi satu signed compatibility cohort secara default | **AUTO-LOCKED** |
| `DEP-AUTO-051` | Mobile package/signing/distribution/update tetap confirmation gap | **AUTO-LOCKED** |
| `DEP-AUTO-052` | Engine/provider hanya masuk release setelah PoC, license, security, integrity, resource, compatibility, dan rollback gate | **AUTO-LOCKED** |
| `DEP-AUTO-053` | Observability/Correlation/alert/health aktif sebelum rollout dinyatakan selesai | **AUTO-LOCKED** |
| `DEP-AUTO-054` | Capacity memakai measured baseline, Pilot observation, approved limit, queue/backpressure/local processing/staged rollout | **AUTO-LOCKED** |
| `DEP-AUTO-055` | Hotfix tetap memakai branch, test, scan, human review, immutable artifact, approval, verification; Codex tidak mengakses Production | **AUTO-LOCKED** |

# 3. Confirmation Gates

- `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION`;
- `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION`;
- `REQUIRES_PERTAMINA_SECURITY_CONFIRMATION`;
- `REQUIRES_PERTAMINA_SSO_CONFIRMATION`;
- `REQUIRES_PERTAMINA_CODE_SIGNING_CONFIRMATION`;
- `REQUIRES_PERTAMINA_DESKTOP_DISTRIBUTION_CONFIRMATION`;
- `REQUIRES_PERTAMINA_MOBILE_DISTRIBUTION_CONFIRMATION`;
- `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION`;
- release approver, topology, exact commands, target numbers, dan evidence retention.
