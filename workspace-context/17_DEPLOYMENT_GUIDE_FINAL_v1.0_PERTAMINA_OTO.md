# 17 — DEPLOYMENT GUIDE
## PERTAMINA OTO — Office Tools Organizer

> **Versi:** FINAL v1.0  
> **Tanggal:** 1 September 2026  
> **Status:** FINAL — PRE-IMPLEMENTATION DEPLOYMENT BASELINE  
> **Production readiness:** BELUM `PRODUCTION_READY`  
> **Bahasa:** Bahasa Indonesia — `ID-1.0`  
> **Baseline:** 550 fitur canonical; 33 Primary PDF Tool Cards; Dokumen 01–16 FINAL  
> **RC:** `RC-TBD`

Dokumen FINAL ini mengunci governance, alur, guard, evidence, dan handoff deployment pra-implementasi. Dokumen tidak memilih Production topology/provider/OS/network/signing/distribution/DR, tidak memuat command Produksi, dan bukan bukti deployment.

---

# 1. Informasi Dokumen

Owner, approver, executor, repository, service connection, change window, retention, dan review cadence memerlukan konfirmasi resmi. Revisi material mengikuti controlled revision dan tidak mengubah Dokumen 01–16 diam-diam.

# 2. Tujuan

Memastikan source resmi menghasilkan artifact yang traceable, reproducible sejauh platform mendukung, diuji, dipromosikan tanpa rebuild, dikonfigurasi aman, dapat dipulihkan, dimonitor, dan siap diserahterimakan.

# 3. Scope

Mencakup source/CI-CD/artifact, environment, Web/API/Worker/SignalR, SQL/EF/SQLite/storage, Desktop/Local Worker/Mobile, engine/provider, config/secret, feature rollout, migration, rollback, backup/restore/DR, observability, capacity, security, evidence, dan Incident handoff.

Tidak mengambil alih Application Cartography Dokumen 18 atau Handover Checklist penuh Dokumen 19.

# 4. Target Pembaca

Developer, reviewer, release integrator, QA/UAT, Security, Infrastructure/Network, DBA/storage/backup function, Administrator, Support/Incident Owner, dan Pertamina deployment authority. Fungsi tersebut bukan role aplikasi baru.

# 5. Deployment Principles

1. Source resmi dan protected review.
2. Build once, test, promote same immutable artifact.
3. Environment/identity/config/secret/data terpisah.
4. Least privilege, no manual Production mutation.
5. Compatibility sebelum rollout; Local-First tetap benar.
6. Migration/backup/rollback/monitoring siap sebelum perubahan.
7. Process up bukan deployment success; authoritative validation wajib.
8. Evidence append-oriented; failure tidak ditimpa.

# 6. Assumptions

Baseline teknologi/arsitektur Dokumen 04–05 tetap terkunci. UAT adalah spesifikasi FINAL tetapi actual execution/sign-off belum tersedia. Semua deployable, 33 Core, dan RC masih memerlukan implementation/release evidence.

# 7. Confirmation Gaps

Production topology/hosting/OS/container/proxy/DNS/TLS/port/firewall, SSO endpoint, secret/object-storage product, signing/distribution, capacity/SLA, backup retention/RPO/RTO/DR site, monitoring retention, dan authority tetap memakai tag konfirmasi Pertamina; tidak diisi dengan asumsi.

# 8. Architecture Deployment View

```text
Clients → approved SSO + HTTPS → Web/API/SignalR
→ application authorization/admission → persisted Job/queue
→ Server Worker/engine → SQL metadata + private secure storage
→ OpenTelemetry/APM + Enterprise Audit

Desktop → typed/versioned Named Pipe → isolated Local Worker
Desktop/Mobile → protected profile + SQLite/workspace
```

Physical placement fleksibel; logical/security/data boundaries tidak berubah.

# 9. Logical Deployables

Web, API, Server Worker, Desktop, Mobile/Tablet, Local Worker, SQL Server, Secure File/Object Storage, Monitoring/Telemetry, dan SQLite local state tetap distinct. Satu server sementara tidak menggabungkan release unit atau identity.

# 10. Environment Model

Minimum: Local Development, Shared Development, QA/Testing, Pilot, dan Production. Rincian authority ada pada `ENVIRONMENT_CONFIGURATION_MATRIX.md`. Semua promotion memakai exact artifact/digest dengan configuration/secret berbeda.

# 11. Local Development

Developer feedback, Unit/component/contract logic, synthetic data, approved local credential store, isolated workspace/SQLite/Local Worker. Tidak memakai Production data/secret, public AI, atau dijadikan baseline capacity.

# 12. Shared Development

Restricted shared integration dengan Dev-only identity/SQL/storage/queue/provider/telemetry. Artifact pipeline tetap digunakan. Co-hosting boleh sementara dengan process/config/data/log/health/resource separation dan bounded test window.

# 13. QA/Testing

Isolated QA menjalankan Integration/Contract/E2E, security/malicious corpus, migration/restore, Worker/IPC, bounded load/soak/fault, dan regression dengan synthetic/sanitized-approved data, stop condition, monitoring, cleanup, serta recovery point.

# 14. Pilot

Pilot memakai exact RC, participant/data/lane/identity/secret/monitoring/backup terkontrol. Tujuannya operational observation, bukan Production/HA proof. Fuzz, penetration, stress/DoS, debug, dan manual DB/storage edit dilarang.

# 15. Production

Production hanya menerima approved pipeline artifact; debug off; secret/config terpisah; monitoring aktif; verified backup/recovery point; migration/rollback plan; authority/change window; smoke/observation; Incident path. Physical topology masih belum dipilih.

# 16. Hostinger KVM 2 Boundary

Hostinger KVM 2 adalah `SINGLE_NODE_TEMPORARY` untuk Development, QA terbatas, atau Pilot terbatas setelah approval. Actual provisioned resource diverifikasi. Bukan Production baseline, bukan satu-satunya backup location, dan tidak boleh menciptakan vendor lock-in.

# 17. Production Approval Boundary

Technical green, QA pass, atau UAT readiness bukan Production approval. Production memerlukan human authority, exact RC/digests, UAT sign-off, security/VA-PT sesuai scope, infrastructure/network/data approval, migration/recovery readiness, dan support/incident readiness.

# 18. Network Architecture

Client-facing surface hanya approved Web/API/SignalR/compatibility/minimal health. Worker, SQL, storage, queue, admin, migration, backup, monitoring detail, dan provider interfaces berada pada restricted/internal boundary. Client tidak pernah direct SQL/storage.

# 19. DNS/TLS Principles

HTTPS, trusted certificate, full validation, approved DNS, minimal ingress/egress, firewall/allowlist, proxy/header/Origin/WebSocket control, private SQL/storage, restricted admin/health, dan authenticated SignalR wajib. Exact values: `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION`.

# 20. Identity/SSO Deployment

Pertamina Identity mengautentikasi user; API mengotorisasi resource. Workload identity Web/API/Worker/migration/backup/monitoring/deployment terpisah. SSO metadata/endpoints/session settings dan client redirect/config memerlukan `REQUIRES_PERTAMINA_SSO_CONFIRMATION`.

# 21. Configuration Hierarchy

```text
Default → Environment → Enterprise Policy → Feature Flag → Runtime Capability
```

Configuration typed, versioned, validated, environment-bound, integrity-controlled, dan auditable untuk perubahan sensitif. Unknown/unsafe value fail safely.

# 22. Secret Management

Secret berasal dari approved mechanism dan identity least-privilege; tidak berada di source, appsettings committed, artifact/image/package, pipeline output/log, release notes, screenshot, Issue, Audit, atau docs. Missing secret menyebabkan not-ready/fail-safe, bukan fallback hardcode.

# 23. Artifact Strategy

Artifact per release: Web, API, Server Worker, Desktop, Local Worker, Mobile, Database Migration Bundle, Configuration Template, OpenAPI, Release Notes, SBOM, Test/Security Evidence, dan Release Manifest. Masing-masing immutable, restricted, versioned, dan ber-digest.

# 24. Build Once/Promote

```text
Integrated commit → controlled release build
→ immutable artifact set + manifest/digests
→ Dev/QA validation → RC designation
→ same artifact IDs/digests → Pilot/Production
```

Perubahan bytes setelah QA menghasilkan build/artifact baru dan mengulang gate.

# 25. Versioning

Catat ProductVersion, BuildVersion, CommitId, API ContractVersion, CapabilityVersion, FeatureParameterSchemaVersion, DatabaseMigrationVersion, ConfigurationVersion, DesktopVersion, MobileVersion, LocalWorkerVersion, LocalSchemaVersion, dan IpcContractVersion. Release Manifest memetakan kombinasi yang sah.

# 26. Compatibility

Status client: `SUPPORTED`, `UPDATE_AVAILABLE`, `UPDATE_REQUIRED`, `UNSUPPORTED`. Uji API/OpenAPI/error/SignalR, Job/checkpoint/idempotency, schema/migration, SQLite, Desktop–Worker IPC, config/capability, provider/engine, dan mixed-version work. Breaking change memerlukan versioned path/order/recovery.

# 27. Azure DevOps Pipeline

Urutan minimum: trace/SDK/source validation → restore → Release build → Unit → static/secret/dependency/license → Architecture/Contract → package → SBOM → hashes/Release Manifest → immutable publish. Agent/image/feed/tool versions dikontrol dan dicatat.

# 28. Branch/Review Gate

Protected `main`, no direct/force push, short-lived task branch/worktree, PR traceability, scope/dependency/contract/config/migration risk, tests/evidence, human review, resolved blocking discussion. Codex tidak approve/merge/release.

# 29. Security Scan Gate

Confirmed secret, critical/policy-disallowed vulnerability, security-critical failure, unauthorized exposure, artifact mismatch, unapproved dependency/license, atau scanner unavailable menghasilkan STOP/FAIL/BLOCKED. Waiver scoped/expiring/approved tidak mengubah hasil menjadi PASS.

# 30. Test Gate

Gunakan risk-based Unit, Architecture, Contract, Integration, DB/storage/Worker/IPC, E2E, Security, Recovery, Performance, Platform, Regression, UAT readiness, dan post-deploy smoke. Status hanya `PASS`, `FAIL`, `BLOCKED`, `NOT_APPLICABLE`; missing/flaky/quarantined bukan PASS.

# 31. Artifact Integrity

Verifikasi immutable artifact ID, strong digest, build/commit, SBOM/provenance, package contents, approved source, manifest, access, dan promotion history. Release authority adalah original verified artifact, bukan hasil rebuild workstation.

# 32. Release Approval

Approval record memuat release/environment/artifact digests, gate results, known risk/waiver, migration/config/backup/rollback, scope, authority/time/conditions. Pipeline status tidak membuat approval. Exact approver/SoD/delegation tetap confirmation gap.

# 33. Web Deployment

Deploy immutable Web artifact dengan environment API/config, cache/version metadata, security headers/CSP, no secret/source map leakage, health, logging, graceful drain, compatibility, telemetry, dan previous compatible artifact rollback. Hosting/proxy/CDN final tidak dipilih.

# 34. API Deployment

API memakai typed config, separate identity, SQL/storage/queue/Audit readiness, `/health/live`, `/health/ready`, `/api/compatibility`, current authorization, idempotency/backpressure, safe logs, no false `202`, graceful admission stop, dan schema-compatible rollback.

# 35. Worker Deployment

Worker artifact membawa closed handler/engine registry dan supported Job/contract/schema versions. Not-ready berhenti claim; shutdown drain/checkpoint/release lease; stale completion ditolak; one active attempt; no user token; no mass retry; separate resource budget dan identity.

# 36. SignalR Deployment

SignalR tetap authenticated post-commit notification. Deployment memvalidasi authorization/group, event/revision compatibility, reconnect/backoff, duplicate/gap handling, dan REST reconciliation. Realtime down tidak boleh menyebabkan Job re-execution atau false failure.

# 37. SQL Server Deployment

SQL tetap satu logical server baseline dengan module-owned schemas, private access, least-privilege runtime/migration/backup identities, encrypted connection, versioned history, capacity/log/lock monitoring, backup/recovery readiness, dan no client/direct manual patch.

# 38. EF Core Migration

Migration berasal dari same release commit, dipaketkan/digest, reviewed, idempotent sejauh sesuai, diuji clean/prior/representative volume, mempunyai recovery point, compatibility/impact/stop/validation/rollback disposition, dan dijalankan controlled—bukan saat app startup.

# 39. SQLite Migration

Per profile/device/subject: preflight identity/schema/integrity/storage/WAL/active work/key, exclusive migration lease, transaction atau resumable journal, protected checkpoint, staged artifact publish, failure recovery, no silent reset/downgrade, dan source/workspace preservation.

# 40. Secure Storage Deployment

Private zones untuk quarantine/input/temp/result/secured/issue/export; opaque internal refs, owner/purpose/state/version/digest/key-ref/tombstone/hold/max-three; separate identity, encryption, quota/lifecycle/health/orphan reconciliation/backup. File payload bukan SQL blob default.

# 41. Desktop Packaging

Package candidate memuat WinUI 3 Desktop, Local Worker, approved runtime/native/engine resources, registry/localization/config schema, SQLite migrations, SBOM/digest/license/signing metadata. MSIX adalah kandidat, bukan keputusan final: `REQUIRES_PERTAMINA_DESKTOP_DISTRIBUTION_CONFIRMATION`.

# 42. Local Worker Packaging

Default satu signed Desktop release cohort: AppVersion, LocalWorkerVersion, IPC contract, handlers, engine versions, work-root/checkpoint schema. Named Pipe local-only, authenticated/versioned handshake, least privilege, closed operation allowlist, bounded resource, no arbitrary network/filesystem/SQL/shell.

# 43. Desktop Installation

Approved channel → verify publisher/signature/integrity → OS/architecture/runtime/storage preflight → explain scope/restart/data behavior → install Desktop+Worker cohort → protected profiles → first-run SQLite/IPC/compatibility/SSO/capability checks. Exact channel/signing belum dipilih.

# 44. Desktop Update

Compatibility check → user notice → approved download → signature/digest verify → stop/checkpoint active work → preserve local state → atomic cohort install → SQLite migrate → first-run verification. Arbitrary update URL/command dari server/AI/document dilarang.

# 45. Desktop Rollback

Hanya retained signed compatible cohort. Periksa SQLite/workspace/checkpoint/config/API compatibility. Jika state baru tidak dibaca old app, gunakan forward-fix/recovery; jangan silent downgrade/reset. Uninstall/rollback tidak menghapus source/output/workspace/signature tanpa explicit governed choice.

# 46. Mobile Packaging

MAUI package per approved platform membawa app identity/version, config schema, LocalSchema migrations, curated capability, secure-store/backup-exclusion declarations, permissions, SBOM/digest/signing evidence. Target OS/architecture belum dikarang.

# 47. Mobile Distribution

Enterprise store/MDM/side-load/channel, app/team identity, signing, SSO redirect, entitlement, ring, managed wipe, minimum OS, dan data preservation memerlukan `REQUIRES_PERTAMINA_MOBILE_DISTRIBUTION_CONFIRMATION`.

# 48. Mobile Update

Staged managed rollout dengan signature/integrity, compatibility/minimum-version, SQLite recovery, local work preservation, permission changes, offline behavior, update-required guidance, post-update validation, dan rollback limitation. Server tetap mendukung approved client window.

# 49. Provider/Engine Packaging

Setiap library/native/font/codec/OCR/model/AI/provider mencatat supplier/source/version/license/platform/digest/SBOM, Feature/handler compatibility, size/resource/egress/data policy, vulnerability, PoC, rollback/fallback. Belum approved tidak masuk Production. Public model/AI download dilarang.

# 50. Feature Flag Rollout

Status kandidat: `DISABLED`, `INTERNAL`, `PILOT`, `LIMITED`, `GENERAL`, mengikuti config canonical terbaru. Flag mempunyai owner, purpose, scope, default, version, expiry/review, monitoring, disable/cleanup. Tidak boleh mengubah Feature ID, business meaning, security, permission, Audit, atau Local-First.

# 51. Pre-Deployment Checklist

Gunakan `RELEASE_CHECKLIST_REGISTER.md`: exact approved artifact/environment/version, tests/scans/UAT/PoC, config/secret, dependency/provider, migration/backup/rollback, capacity, health/monitoring, known issue, Incident path, maintenance communication, authority.

# 52. Deployment Procedure

```text
Source/PR → build/test/scan/package/integrity
→ Dev smoke → QA functional/regression/security/performance/recovery
→ RC/UAT/approval → Production precheck/recovery point
→ controlled migration → deploy server components
→ health/readiness → staged flag/ring → client compatibility
→ smoke → observation → completion/handoff
```

Exact command/endpoint/service name menunggu approved environment runbook.

# 53. Startup/Dependency Order

Verifikasi network/time/identity/config/secret → SQL/storage/queue/Audit path → API/Worker readiness → Web/SignalR → clients. Migration selesai sebelum incompatible application admission. Worker claim setelah dependencies ready. Order dapat disesuaikan oleh topology-approved plan tanpa mengubah dependencies.

# 54. Smoke Testing

Gunakan synthetic/minimized data: liveness/readiness, compatibility, SSO/authz negative, SQL/storage/queue/Worker, Audit, Issue, SignalR+REST, secured-file operation, representative Core, `FEAT-PDF-005` Merge PDF, error path, Local-First zero server traffic. Tidak attack/load Production.

# 55. Post-Deployment Validation

Process/status alone tidak cukup. Validasi exact artifact/config/schema, health/dependency freshness, API contract, authn/authz, Job/Worker/queue, storage/data integrity, Audit/log/trace/metrics, client/version, user outcome, capacity trend, rollback readiness, dan communication.

# 56. Monitoring/Observability

OpenTelemetry/Application Insights baseline memuat safe environment/component/version/Feature/Job/Correlation/Error/outcome/duration/resource fields. Logs/traces/metrics/alerts/health/queue/DB/storage/Audit/Worker/provider terlihat sebelum release. Content/password/token/secret/OCR/signature/prompt/raw SQL/path dilarang.

# 57. Capacity Validation

Ukur CPU, RAM, disk/I/O, DB/log/pool, storage/temp/log growth, network, users/jobs/concurrency, queue age/depth/drain, upload/download, Audit/Issue attachments, OCR/AI/conversion. Nilai menjadi `MEASURED_BASELINE`, `PILOT_OBSERVATION`, `APPROVED_LIMIT`, atau `POLICY_CONFIGURABLE`.

# 58. Backpressure/Readiness

Gunakan input limits, rate/quota/concurrency, queue/admission, per-workload Worker limits, provider circuit, DB connection budget, bounded cleanup/retry, staged rollout, dan Local-First. Unsafe saturation menghasilkan safe 413/429/409/503/wait; no silent wait, false 202/Completed, atau duplicate effect.

# 59. Rollback

Catat trigger, authority/owner, Incident/Change, previous artifact/config, schema/data compatibility, data/security impact, flag/forward-fix/restore alternatives, communication, smoke, monitoring, evidence. Jangan rollback binary terhadap incompatible migrated data.

# 60. Database Rollback

Classify `REVERSIBLE`, `CONDITIONAL`, atau `IRREVERSIBLE` berdasarkan evidence. Additive no-op/app rollback dapat aman bila matrix lulus. Setelah destructive/data change, default forward-fix; restore/compensation/data repair memerlukan verified point, explicit risk/authority, dan full validation.

# 61. Configuration Rollback

Revert ke previous typed/versioned configuration melalui official control plane dengan authorization, concurrency, reauth/Audit bila sensitif, compatibility validation, reload/restart plan, expiry, smoke, dan evidence. Jangan edit file/variable Production ad hoc.

# 62. Backup

Operational backup mencakup SQL/log/history/permissions/outbox/idempotency/Audit, storage object/version/digest/key-ref/tombstone/hold, config/policy/artifact/SBOM, serta key-recovery metadata—bukan secret/key bytes. `Amankan ke Akun` adalah product feature, bukan operational backup.

# 63. Restore

Authorize exact backup set → verify manifest/integrity/key/config/artifact → restore isolated → validate SQL/storage reconciliation → restore app dependencies → authz/security/Audit/Job/client/Core smoke → observe → reopen. Backup tanpa restore test belum siap.

# 64. Disaster Recovery

Dependency-driven recovery: authority/inventory → network/DNS/TLS/time/identity → secret/key/config/artifact → SQL → storage → queue/Worker → API/Web/SignalR → monitoring/Audit → clients. DR site/region/replication/RPO/RTO/failover/failback/cadence menunggu approval.

# 65. Hotfix/Emergency Release

Incident → approved hotfix task → dedicated short branch/worktree → smallest fix + exact regression/security check → human review → immutable pipeline artifact → QA validation → approval → controlled rollout → verification/monitoring → Incident/PIR update. Emergency tidak mengizinkan direct patch atau gate deletion.

# 66. Deployment Evidence

Manifest memuat Release/Build/Commit/PR/Task/Requirement/Feature/Defect/Incident, artifact IDs/digests/SBOM, versions/config/schema, environment/strategy/timestamps, test/security/UAT/approval, migration/backup/rollback, health/smoke/monitoring, known issue, attempt outcome. Secret/content tidak disimpan.

# 67. Release Notes

Minimum: version/date, Features, fixes, known limitations/issues, DB/config changes, compatibility/minimum client, security notes aman, rollback/support reference. Jangan memuat secret, exploit detail, document/OCR/signature/prompt, raw SQL/stack/path, atau bearer link.

# 68. Audit Expectations

Gunakan existing typed Event Catalog Dokumen 10 untuk action yang sudah ada; jangan membuat Deployment EventType ad hoc. Pipeline history, release evidence, Enterprise Audit, Incident, UAT, Defect, Support, dan Observability tetap plane berbeda tetapi linked dengan safe opaque references.

# 69. Incident Handoff

Failure/safety/integrity/availability impact mengikuti Dokumen 16. Handoff memuat Incident ID, exact release/environment/artifact/config/schema, impact, containment, actions/outcomes, rollback/forward-fix/restore options, safe evidence, monitoring, decision needed. Deployment tidak mengarang severity/contact.

# 70. Security Hardening

Protected source/pipeline, least-privilege identities, environment isolation, artifact integrity, scans/VA-PT, secure transport, private data services, restricted admin, no debug/secret/source data leak, patching/rollback, egress allowlist/no public AI, safe logs, encrypted backup.

# 71. Data Protection

Production data tidak masuk non-Prod tanpa masking/authorization/approval. SQL metadata dan secure payload terpisah. Source/output tidak ditimpa/hilang. Encryption/key separation, ownership/purpose/state/version/digest/tombstone/hold/max-three, temp lifecycle, retention-aware cleanup, dan restore AuthZ reconciliation wajib.

# 72. Troubleshooting

Gunakan `DEPLOYMENT_RUNBOOK_REGISTER.md`. Mulai dari environment/artifact/version/config/secret/health/dependency/evidence yang aman. Stop pada unknown target, digest mismatch, schema drift, missing recovery point, unsafe capacity, access/authority gap, data/security anomaly. Jangan mencoba direct Production repair.

# 73. Environment Matrix

Authority berada pada `ENVIRONMENT_CONFIGURATION_MATRIX.md`. Setiap environment memiliki tujuan/user/data/access/artifact/config/secret/log/monitoring/backup/tests/prohibitions/approval/readiness yang eksplisit.

# 74. Configuration Matrix

Gunakan matrix Default/Environment/Enterprise Policy/Feature Flag/Runtime Capability/Secret Reference. Config schema/version/compatibility/effective source dan rollback dicatat. Runtime observation tidak disimpan sebagai business authority.

# 75. Artifact Register

Minimum: ReleaseId, BuildId, CommitId, Branch/PR, Product/Contract/Capability/Migration/Config/Desktop/Mobile/Worker versions, ArtifactName/Id/Hash, CreatedAtUtc, pipeline, promotion history, scan/test/UAT, approval, status, rollback reference. Tidak ada secret.

# 76. Deployment Checklist

Checklist pre/deploy/post berada pada `RELEASE_CHECKLIST_REGISTER.md`. Setiap step menjawab input, executor/authority, environment, artifact, approval, dependencies, downtime/data/security/capacity impact, backup/rollback, smoke, evidence, monitoring, completion/escalation, dan docs update.

# 77. Rollback Checklist

Pastikan trigger/authority/Incident, last-known-good artifact/config, compatibility/schema/data impact, backup, alternatives, communication, execution boundary, validation, monitoring, evidence, dan next decision. Rollback tidak berarti restore otomatis.

# 78. Restore Checklist

Pastikan backup set/manifest/digest/encryption/key/access/authority, isolated target, schema/storage/config/app/client compatibility, permission/tombstone/hold/Audit/outbox/idempotency/Job reconciliation, Core/security smoke, measured time/gap, monitoring, Incident/DR evidence.

# 79. Requirement Traceability

| Area | Requirement utama |
|---|---|
| Repository/CI-CD | `IR-DEVOPS-001..004`; `BR-GOV-003..004` |
| Environment | `NFR-ENV-001..002`; `NFR-ARCH-004` |
| Security | `SR-SEC-006`; `SR-SEC-010`; Dokumen 08–10 |
| Database/storage | `DR-DB-001..007`; Dokumen 07 |
| Recovery | `FR-REC-001..006`; `NFR-REC-001..005` |
| Monitoring | `NFR-MON-001..004` |
| PoC/release | `BR-GOV-006..009`; Definition of Done |
| Handover | `NFR-HAND-001` |

# 80. Feature Traceability

Deployment mencakup seluruh 550 Feature melalui manifest/capability/dependency/test mapping. Fokus: 33 Core, `FEAT-PDF-005`, `FEAT-PROG-001..009`, `FEAT-BACKUP-001..013`, `FEAT-VER-001..006`, `FEAT-AUTH-001..012`, `FEAT-AUDIT-001..008`, `FEAT-SUPPORT-001..008`, `FEAT-INTEL-001..002`. Tidak ada Feature ID baru.

# 81. API Traceability

Dokumen 06 tetap authority: Hybrid API, generated/versioned OpenAPI, URI major, file transfer, idempotency, queries, Pertamina auth boundary, SignalR envelope/REST recovery, admission/backpressure. Deployment memakai contract snapshot/compatibility; tidak membuat endpoint.

# 82. Database Traceability

Dokumen 07 tetap authority: one SQL Server, module-owned schemas/DbContexts/migrations, SQL metadata + secure object payload, optimistic concurrency, outbox/idempotency, SQLite per profile, selective secured files, max-three. `secured.*` canonical; operational backup terpisah.

# 83. Security Traceability

Dokumen 08–10 tetap authority: deny-by-default, no implicit content access, secure file/Worker/IPC/AI, secret/log exclusion, append Audit, scoped roles/permissions, supply-chain/test/VA-PT. Deployment tidak menambah break-glass, role, atau public provider.

# 84. Test/UAT Traceability

Dokumen 11–13 menentukan Unit/Test/UAT evidence. Artifact/environment/config/version harus sama dengan evidence. UAT readiness bukan execution; actual immutable RC acceptance dan Production approval masih diperlukan. Failure/rerun tidak ditimpa.

# 85. Deployment Acceptance Checklist

- [x] Dokumen 16 FINAL diverifikasi.
- [x] Dokumen 01–16 dan master terbaru diaudit.
- [x] Laporan Agent A–D tersedia dan diintegrasikan.
- [x] 85/85 bagian wajib tersedia.
- [x] 55/55 keputusan teknis aman AUTO-LOCKED.
- [x] 16 runbook entries dan 12/12 mandatory design tabletop reviews tersedia.
- [x] Build-once/promotion, environment isolation, compatibility, migration, rollback, restore, observability, Local-First, server budget, security, dan Codex boundary terkunci.
- [x] Hostinger hanya temporary dan Production topology tidak dikarang.
- [x] Tidak ada command/credential/Production change, role, permission, API, schema duplikat, Feature ID, vendor, SLA, RPO/RTO, atau distribution channel baru.
- [ ] Pertamina infrastructure/network/security/SSO/signing/distribution/backup-DR/organization approvals tersedia.
- [ ] Exact topology, pipeline resources, config/secret mechanism, RC/artifacts, UAT/VA-PT, backup/restore, capacity, operational drills, dan release authority tervalidasi.
- [ ] Environment-specific executable runbook disetujui dan berstatus sesuai target.

## Status Akhir

```text
Document status       : FINAL v1.0
Baseline readiness    : PRE-IMPLEMENTATION DEPLOYMENT BASELINE
Production readiness  : BELUM PRODUCTION_READY
Tabletop readiness    : 12/12 DESIGN REVIEW PASS; operational exercise BELUM
Next document         : 18_APPLICATION_CARTOGRAPHY.md
```
