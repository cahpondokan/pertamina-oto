# AGENT C — DEPLOYMENT, RUNTIME & SECURITY-ZONE CARTOGRAPHY REPORT

**Task ID:** `DOC18-AGENT-C`  
**Status:** Rekomendasi specialist; keputusan final tetap milik user dan Integrator  
**Tanggal:** 2026-09-01  
**Target integrasi:** Document 18 — Application Cartography  
**Sumber berwenang:** Document 01–17 FINAL  
**Document 17 references:** master FINAL v1.0, `DEP-AUTO-001..055`, Environment/Configuration Matrix, Deployment Runbook Register, Release Checklist Register, dan specialist reports

Laporan ini memetakan komponen, runtime, environment, data, trust zone, network flow, health, capacity, dan recovery dependency. Laporan ini tidak menulis ulang prosedur deployment, memilih Production hosting, atau menetapkan IP/DNS/port/firewall/proxy final.

---

# 1. Deployable catalog

## 1.1 Client dan local-data catalog

| Attribute | PERTAMINA OTO Desktop | PERTAMINA OTO Mobile | PERTAMINA OTO Local Worker | SQLite | Local Workspace |
|---|---|---|---|---|---|
| Artifact | Signed WinUI 3 package/bundle + release manifest | Signed .NET MAUI platform package + release manifest | Signed binary/dependencies dalam Desktop compatibility cohort | Versioned schema/migration bundle dalam client artifact; DB dibuat per profile | App-managed folder/manifest schema; bukan executable artifact |
| Technology | .NET 10 LTS baseline, WinUI 3 | .NET 10/.NET MAUI baseline, platform adapter | .NET process + Named Pipe/local IPC + engine adapters | SQLite through typed repository/abstraction | Protected local filesystem + opaque manifest references |
| Process/runtime | In-process Desktop UI/application; heavy/risky work delegated | Mobile/Tablet app process; curated local functions | Isolated child process; no public service/port | Embedded local database service within client process boundary | No own process; accessed only by Client/Local Worker through scoped references |
| Host | Supported Windows user device | Approved Phone/Tablet device/platform | Same intended Windows user device as Desktop | Per device/profile | Per device/profile; source may remain outside managed root |
| Environment | Local Dev, Shared Dev/QA/Pilot client, Production client after distribution approval | Local Dev, QA/Pilot, Production client after distribution approval | Follows exact Desktop environment/release | Follows client environment and profile | Follows client environment and profile |
| Configuration | Signed defaults + integrity-controlled bootstrap + effective policy/capability | Same hierarchy with mobile entitlement/device capability | Handler allowlist, IPC/resource/work-root limits from compatible signed/config cohort | Local schema version, protection, migration/recovery/retention policy | Folder classes, quota, cleanup, recovery, backup-exclusion, save/export policy |
| Secret source | Approved Windows secure store/protected memory; no server secret | Approved platform secure store; no server secret | No ambient server secret; receives no end-user token | Encryption/key reference from approved OS key facility; no token ordinary field | No embedded secret; protection key follows profile/OS security boundary |
| Identity | Authenticated user + stable app/package/profile identity | Authenticated user + stable app/package/profile identity | Current user or stricter approved local identity; Desktop-launched instance/job binding | Principal/profile/device binding | User owns source/output; app profile controls managed artifacts |
| Network dependency | HTTPS API, authenticated SignalR, approved SSO/update channel; local-only Feature has zero server dependency | HTTPS API, authenticated SignalR if exposed, approved SSO/distribution; local-only path remains local | None by default; approved engine egress only if explicitly designed; IPC is local | None for local state; Audit/secured transfer use separate explicit sync paths | None for local-only work; explicit server execution or **Amankan ke Akun** crosses boundary |
| Data dependency | Local files/workspace/SQLite; server metadata/content only after explicit authorized route | App-private local data/SQLite; explicit file/share/camera inputs | Job-scoped source/artifact refs, working root, compatible checkpoint | Workspace/draft/job/history/preferences/recovery metadata only; no document/signature BLOB, password, token, full OCR | Source reference, draft, undo/redo, checkpoint, temp/cache/thumbnail/result/signature artifact as classified |
| Health check | App responsiveness, profile/schema/recovery state, disk, Worker handshake, API compatibility | App lifecycle, profile/schema/secure-store/device capability, API compatibility | Authenticated heartbeat, process/signature/version/IPC/handler/engine readiness | Open/integrity/schema/profile/version/migration state | Required-root access, free space, manifest consistency, residue/recovery state |
| Monitoring | Safe local logs/metrics, crash code, version, Feature/Job/Correlation; no content/secret | Safe local logs/metrics, platform capability/update state; no content/secret | Heartbeat, crash/recycle, duration, CPU/RAM/temp/output, Feature/Job/Correlation | Migration/integrity/size/write/recovery safe metrics | Capacity/growth/cleanup/recovery failures; filenames/paths minimized |
| Scaling | Device capability; batch/concurrency bounded; no horizontal client scaling | Curated exposure by device/resource; bounded foreground/background work | Per-device process pool/concurrency limits; recycle; no privileged service scaling | Serialized writer/bounded query; lifecycle cleanup | Quota, bounded temp/cache, batch/streaming, admission before disk pressure |
| Backup | User source/output outside app remain user-owned; app root excluded from automatic consumer-cloud backup per approved policy | App-private root excluded from automatic backup as required; user export separate | Reproducible from signed Desktop package; no independent user-data backup | Protected migration/recovery snapshot only; not enterprise server backup | Draft/recovery lifecycle per policy; no automatic server/cloud sync |
| Rollback reference | Document 17 client package rollback; `DRB-DESKTOP-UPDATE-001`, `DRB-CLIENT-INCOMPAT-001` | Document 17 managed update rollback; `DRB-MOBILE-UPDATE-001` | Compatible Desktop cohort rollback; `DRB-DESKTOP-UPDATE-001` | No silent reset/downgrade; `DRB-SQLITE-001` | Preserve source/output/workspace; Document 16 Local/Offline Incident |
| Runbook | Document 16 Client/Version, Local/Offline, Selective Sync; Document 17 distribution runbooks | Same plus mobile update/distribution runbook | Document 16 Worker/Queue and Local/Offline; Desktop update runbook | Document 16 Database/Local Incident; `DRB-SQLITE-001` | Document 16 Local/Offline, Selective Sync, Data Integrity |
| Functional owner | Client Application Owner — actual principal `REQUIRES_PERTAMINA_ORGANIZATION_CONFIRMATION` | Mobile Client Owner — actual principal TBC | Desktop/Processing Owner — actual principal TBC | Client Data/Storage Owner — actual principal TBC | Client Storage Owner; business content owner tetap user |
| Lifecycle | Install → first run → supported/update → repair/rollback → uninstall/data-preservation | Install → enrollment/first run → managed update → uninstall/wipe policy | Install with Desktop → handshake → execute/recycle → update/rollback/remove with cohort | Create/open → migrate → operate → checkpoint/reconcile → preserve/cleanup per profile | Create/import → draft/process → finalize/export → recovery/cleanup; never silent server sync |

## 1.2 Server, data-service, observability, dan provider catalog

| Attribute | PERTAMINA OTO Web | PERTAMINA OTO API | PERTAMINA OTO Worker | SQL Server | Secure File/Object Storage | Monitoring/Telemetry | Approved Provider/Engine |
|---|---|---|---|---|---|---|---|
| Artifact | Immutable Web/static artifact + version manifest/SBOM/digest | Immutable .NET API artifact + OpenAPI/config schema/SBOM/digest | Immutable Worker artifact + handler/engine manifest/SBOM/digest | Approved DB platform instance + reviewed migration artifact/schema manifest | Provisioned private service/zone config; object data is not app artifact | Instrumentation in hosts + versioned exporter/collector/dashboard/alert config | Approved library/native/model package or approved internal-provider profile |
| Technology | Responsive Web; exact host/proxy not selected | ASP.NET Core/.NET 10 LTS | .NET Worker + persisted Job/queue + engine adapters | SQL Server; schema/module ownership | Approved private file/object storage; product not selected | Microsoft.Extensions.Logging + OpenTelemetry + approved Azure Monitor/Application Insights integration | PDF/OCR/conversion/image engine or approved internal AI/provider behind adapter |
| Process/runtime | Separate Web process/server or static-host boundary | Separate API process and application services | Separate Server Worker process/pool | Database service | Storage service/boundary | Collector/export path + restricted backend; may include agents | In-process library, isolated Worker child, or approved remote/internal service by profile |
| Host | Non-Prod approved node(s); Production placeholder only | Non-Prod approved node(s); Production placeholder only | Non-Prod approved node(s); Production placeholder only | Internal host/service; temporary co-hosting only in approved non-Prod | Private service/directory; temporary same-node zone only in approved non-Prod | Separate process/sink preferred; actual topology TBC | Client/Local Worker/Server Worker or `ZONE-APPROVED-PROVIDER` |
| Environment | Shared Dev, QA, Pilot; Production TBC | Shared Dev, QA, Pilot; Production TBC | Shared Dev, QA, Pilot; Production TBC | Isolated per environment | Isolated per environment and purpose zone | Separate stream/access/retention per environment | Separate approved profile/credential/data route per environment |
| Configuration | API base, session/security headers, localization, safe cache/version policy | Typed endpoint/policy/limits/compatibility/feature/config schema | Handler/pool/resource/retry/checkpoint/provider profile configuration | DB/schema/growth/backup/connection policy; no client config | Zone/quota/retention/key/digest/lifecycle/endpoint policy | Exporter/sampling-safe fields/retention/alert/owner config | Profile version, Feature mapping, resource/data/egress/license/fallback policy |
| Secret source | Approved runtime boundary if server session host; never browser/static | Approved workload identity/secret service | Approved workload identity/secret service; no user token | Runtime/migration/backup/admin credentials separated | Scoped service identity/key reference; key separate from data | Approved exporter identity/credential; no content capture | Scoped service credential only for approved remote/internal provider; local package no embedded secret |
| Identity | Separate Web workload identity if applicable + end-user session | Dedicated API workload identity + user identity at request boundary | Dedicated purpose-scoped Worker identity | Separate runtime, migration, backup, and administration identities | Purpose/zone-scoped API/Worker/backup identities | Monitoring exporter/collector/operator identities separate | Package code identity or provider service identity; never user-token forwarding |
| Network dependency | Approved ingress/TLS; API/identity/session; no direct SQL/storage admin | Identity, SQL, storage, queue, Audit, SignalR, provider, telemetry | Queue/SQL/storage/provider/telemetry internal paths | API/Worker/migration/backup/admin internal only; never client | API/Worker/backup internal only; delegated narrow download only if approved | Host exporters → receiver/backend; operations access restricted | Approved egress/internal route, TLS, destination allowlist, timeout/circuit; no public AI |
| Data dependency | UI/session-safe state; no user content in static artifact | Authoritative metadata, Job state, fileRefs, policy, Audit intent; content streamed to storage | Persisted Job + job-scoped object/input/output/checkpoint | Server metadata/state, identity refs, Job/outbox/idempotency, Audit, Support metadata | Quarantine/input/processing/result/secured/Issue/export payloads | Safe metrics/traces/logs only; not Audit/business state/content | Minimum approved input/context and untrusted output; temporary lifecycle |
| Health check | Own liveness/readiness + asset/version integrity | `/health/live`, `/health/ready`, `/api/compatibility` | Internal heartbeat/readiness; not-ready stops claims | Connectivity, schema/migration compatibility, pool/IO/log/tempdb/backup state | Availability, capacity, latency, digest/finalization/cleanup/reconcile state | Receiver/export freshness, backlog/drop/error, alert pipeline | Safe profile availability/freshness/circuit/quota/version/capability status |
| Monitoring | Request/asset/cache/session-safe metrics and Correlation | Request/status/latency/rate/authz/capacity/Audit/error/Correlation | Queue/job/lease/checkpoint/crash/resource/engine/Correlation | Pool/query/lock/log/tempdb/IO/growth/backup/migration | Object/bytes/growth/temp/orphan/cleanup/digest/upload/download | Self-health, ingest/export lag, storage/retention, alert delivery | Latency/error/quota/circuit/resource/output-validation profile metrics; no payload |
| Scaling | Stateless scale candidate after measurement; cache/session compatibility required | Stateless/request/connection scaling after bottleneck evidence | Pools by workload/engine/provider; weighted admission | Scale/topology selected after DBA/capacity/recovery evidence | Capacity/throughput/lifecycle scale after measured growth | Scale by ingest/cardinality/retention; never sample mandatory Audit | Local first; provider concurrency/quota/circuit; approved fallback only |
| Backup | Artifact reproducible; server session/key/config recovery per approved policy | Artifact/config/policy/migration manifest; state primarily SQL/storage | Artifact/config/handler manifest; accepted work in persisted Job/outbox | DB/log backups according to approved recovery model | Separate encrypted version/digest/key/tombstone/hold-aware backup | Config/dashboard/alert inventory; telemetry retention is not authoritative backup | Package reproducible from artifact store; provider retention/backup governed separately |
| Rollback reference | `DRB-ROLLBACK-001`, API compatibility checks | `DRB-API-FAIL-001`, `DRB-ROLLBACK-001`, forward-fix if schema changed | `DRB-WORKER-FAIL-001`, queue drain/compatible checkpoint | `DRB-DB-MIG-FAIL-001`, `DRB-RESTORE-001`; no blind down migration | `DRB-STORAGE-FAIL-001`, `DRB-RESTORE-001` | `DRB-CONFIG-BAD-001`; preserve incident evidence/freshness | Feature/profile disable, last approved package/profile; PDF/OCR/AI runbooks |
| Runbook | Document 16 Web/API, Client/Version, SignalR | Document 16 Web/API, Identity, Audit, Incident; Document 17 API/rollback | Document 16 Worker/Queue, PDF/Conversion, OCR/AI | Document 16 Database/Data Integrity; Document 17 DB/restore | Document 16 Storage/Selective Sync/Data Integrity | Document 16 Monitoring/Alert, Incident Audit | Document 16 PDF/Conversion or OCR/AI; Document 17 queue/hotfix/provider handling |
| Functional owner | Web/Application Owner — actual principal TBC | API/Application Service Owner — actual principal TBC | Processing/Worker Owner — actual principal TBC | Database Owner/DBA function — actual principal TBC | Secure Storage Owner — actual principal TBC | Observability/Operations Owner — actual principal TBC | Capability/Engine/Provider Owner + Security review — actual principal TBC |
| Lifecycle | Build/promote → run/drain → update/rollback → retire | Build/promote → migrate-compatible run → drain/update/rollback → retire | Build/promote → claim/execute/drain → recycle/update/rollback → retire | Provision → migrate → operate/backup → archive/restore → decommission | Provision zones → ingest/promote/retain → archive/purge/restore → decommission | Configure → collect/retain/alert → revise → retire under evidence policy | Evaluate/PoC → approve/package/profile → operate/monitor → update/disable/rollback/remove |

## 1.3 Catalog completeness findings

- Queue/Job admission adalah operational dependency penting, tetapi product/topology queue belum dipilih dan tidak ditambahkan sebagai deployable baru dalam laporan ini.
- SignalR adalah API/realtime surface, bukan standalone source of truth atau otomatis deployable terpisah.
- SQLite dan Local Workspace adalah runtime/data boundaries, bukan server services.
- Secure File/Object Storage, Monitoring backend, Identity, dan Provider adalah service dependencies; exact product/topology tetap confirmation gap.
- Azure DevOps artifact storage adalah release/supply-chain dependency, bukan runtime data store atau client file storage.

---

# 2. Runtime map

| Runtime | Startup dependency | Shutdown behavior | Health | State authority | Retry | Crash impact | Recovery | Resource profile |
|---|---|---|---|---|---|---|---|---|
| In-process Desktop | Signed package, protected profile/folders, secure store, SQLite schema, feature registry; Worker/API only for affected capability | Stop dispatch, checkpoint/preserve draft, flush safe local state/log; no source delete | UI responsiveness, schema/profile/recovery, disk, Worker/API compatibility | Local SQLite/workspace for local state; API for server state | Bounded/idempotent by operation; no silent upload retry | UI crash must not corrupt source; draft/checkpoint recover if valid | Startup manifest/SQLite reconciliation; Document 16 Local/Offline | `LIGHT–MEDIUM`; UI responsiveness protected from engine work |
| Isolated Local Worker | Desktop-launched authenticated IPC, compatible version/handler, job root, engine, resource budget | Cancel/timeout/recycle at safe boundary; partial result not final | Heartbeat, signature/version/IPC/engine/resource | Desktop coordinator + local Job/checkpoint; Worker output untrusted until validated | Bounded attempt/checkpoint compatible only | Worker crash does not terminate UI; current job fails/retries/resumes safely | Relaunch compatible Worker; reconcile job/work root/checkpoint | `MEDIUM–HEAVY`, per-engine CPU/RAM/temp/output limits |
| Web process | Artifact/config, approved host/TLS, API/session/identity as applicable | Stop admission, drain bounded requests/session work, flush telemetry | Own liveness/readiness, asset/version/config | API/REST; browser/cache is not authority | Bounded transient retry only | Web unavailable affects UI access, not persisted Job authority | Rollback compatible Web artifact; client retry/refresh | `LIGHT–MEDIUM`, request/connection/cache oriented |
| API process | Config/secret, Identity, SQL, storage, queue/Audit/telemetry readiness | Stop new mutation/upload/Job admission; complete durable boundary; close streams/connections | `/health/live`, `/health/ready`, compatibility | SQL/object authoritative state through application services | Idempotency + bounded transient retry; no DB connection during wait | New server commands unavailable; accepted durable Job/state survives | Compatible rollback/forward-fix; dependency recovery; Document 16 Web/API | `LIGHT–MEDIUM`; streaming and connection budgets |
| Server Worker process | Workload identity, queue/Job store, SQL/storage, handler/engine/provider, temp, telemetry | Stop claims; drain/checkpoint/release lease; reject stale completion | Internal heartbeat/readiness; pool/handler/engine status | Persisted Job/lease/checkpoint; queue message is locator only | Bounded attempt/backoff/idempotency, one active attempt | Affected jobs delay/fail; API/UI remain; no partial final | Recycle/pool failover option, compatible artifact/profile, queue reconciliation | `HEAVY` or `PROVIDER_DEPENDENT`; isolated pools |
| Database service | Host/storage/config/key/cert, migration compatibility, identities | Controlled DB service stop only through infrastructure runbook; application drains first | Service/connectivity, pool, IO, log/tempdb, backup/migration | Authoritative server metadata and Audit store | DB client bounded transient/deadlock retry around safe commands | Broad server metadata/job/audit unavailable; privileged mutations fail closed | Backup/restore + SQL/object reconciliation; DB runbook | Critical `LIGHT` operations aggregate to high IO/connection demand |
| Storage service | Private endpoint/config/key/identity, capacity, lifecycle/hold | Stop new transfers/promotions; preserve staging/intent; no false final | Availability/capacity/latency/digest/finalization/cleanup | Object bytes/version/digest under SQL metadata/policy relationship | Idempotent upload/download/purge retry with leases/tombstone | File jobs/results/attachments/secured data unavailable; metadata may remain | Storage runbook, backup restore, SQL/object reconcile | `MEDIUM–HEAVY`; network/disk/IO/capacity dominated |
| Monitoring service | Exporter config/identity/network/backend/storage/clock | Flush bounded buffers; do not block business indefinitely or expose payload | Receiver/export freshness, backlog/drop/error, alert delivery | Observability only; not business state/Audit substitute | Bounded retry/buffering without unbounded local disk | Detection/diagnosis degraded; critical telemetry unknown is not healthy | Restore minimal recovery instrumentation early, then full backend/config | Cardinality/retention/ingest dependent; bounded sampling cannot drop mandatory Audit |
| Approved provider | Approved profile/identity/endpoint/license/capacity/data policy | Circuit/stop calls, expire temporary lease, no public fallback | Safe availability/freshness/circuit/quota/version | Provider output untrusted; actual result accepted only after application validation | Timeout/circuit/bounded retry by allowlisted failure | Only dependent Features degrade; local-safe path remains if valid | Approved fallback/last profile/disable; provider incident runbook | `PROVIDER_DEPENDENT`, possibly `HEAVY` preprocessing/output |

Runtime invariants:

- no runtime uses a shared human credential;
- client never connects directly to SQL Server or unrestricted storage/provider;
- Local Worker is not Server Worker;
- no process holds DB connection/transaction during file/engine/provider work or retry delay;
- startup failure never activates weak secret/config/public-provider fallback;
- graceful shutdown preserves durable intent and avoids false success.

---

# 3. Environment map

## 3.1 Purpose, placement, users, and data

| Environment | Logical deployables | Physical placement concept | User scope | Data type |
|---|---|---|---|---|
| Local Development | Desktop/Mobile/Web development client as needed, Local Worker, SQLite, Local Workspace; optional isolated Web/API/Worker/SQL/Storage/Monitoring for integration | Developer device and isolated local dependencies; not public and not capacity baseline | Individual developer/test identities; approved non-Prod identity for integration | Synthetic minimal only; no Production credential/content |
| Shared Development | Restricted client, Web, API, bounded queue/Worker, Dev SQL, Dev storage zones, Dev monitoring | May use one temporary physical node with logical separation | Authorized engineering/reviewer team | Synthetic by default; sanitized data only when explicitly approved |
| QA/Testing | QA clients, Web/API/SignalR, Worker pools, QA SQL/storage, evidence/monitoring; malicious corpus isolated | Isolated QA lane; may be temporary node only with test windows and blast-radius controls | Test, security, recovery, UAT-preparation identities | Synthetic/sanitized-approved; controlled corrupted/malicious corpus; never raw Production |
| Pilot | Approved clients, Pilot Web/API/SignalR, admission/Worker, Pilot SQL/storage, separate monitoring/alerts/backup | Dedicated logical lane; Hostinger KVM 2 only as approved limited non-Production pilot | Explicit participant/operator scope; no shared account | Synthetic or explicitly approved Pilot data under classification/retention |
| Production | Required logical deployables and dependencies only | `PERTAMINA_APPROVED_PRODUCTION_INFRASTRUCTURE` | Official users and workload/admin functions under approved scope | Governed operational data using official Pertamina classification |

## 3.2 Security, operations, and governance

| Environment | Network boundary | Secret boundary | Monitoring | Backup/recovery | Deployment source | Approval | Confirmation gap |
|---|---|---|---|---|---|---|---|
| Local Development | Loopback/local or restricted developer network; public exposure prohibited | Approved developer credential store; no repo/plain file | Local safe logs/metrics and component health | Reproducible fixture; local recovery tests as needed | Local/task build for feedback; shared dependency through approved source | Task/team policy | Developer tool/secret standard and non-Prod SSO profile |
| Shared Development | Restricted ingress; internal SQL/storage/admin; no public management | Separate non-Prod injected secret source | Environment/component-separated stream and health | Change recovery point/recreate evidence; backup not Production-grade claim | Pipeline artifact for shared environment | Shared environment owner + security/network as applicable | Exact host/network/secret/backup retention |
| QA/Testing | Isolated authorized test access; explicit security/load target/window | QA-specific least-privilege source | Full instrumentation and test evidence correlation | Verified pre-change point; restore/reconciliation rehearsal | Same immutable pipeline artifact | QA/security/test owner and window authority | Exact topology, failure-injection boundary, evidence retention |
| Pilot | Participant allowlist/restricted endpoint; private data/admin paths | Separate Pilot source with rotation/revocation readiness | Operational monitoring and alert rehearsal before use | Verified backup/restore/reconciliation set | Exact approved RC artifact | Pilot/Product/Security/Operations/Network authorities | Pilot data/classification, capacity, Hostinger approval, support route |
| Production | Minimal client-facing boundary; all data/admin/detailed health private | Pertamina-approved runtime secret/key mechanism | Active before rollout; approved sink/retention/owners/alerts | SQL/object/config/key/artifact/Audit aware; RPO/RTO/DR approved | Exact approved same RC artifact from Azure DevOps pipeline | Formal Pertamina approval | Infrastructure, network, security, SSO, backup/DR, owners, targets, evidence |

Environment is not selected by a source branch. The difference is approved configuration, secret, identity, data, provider profile, endpoint, monitoring, and authority.

---

# 4. Hostinger temporary topology

Hostinger KVM 2 is mapped only as:

```text
SINGLE_NODE_TEMPORARY
Allowed candidate: Shared Development / limited QA / approved limited Pilot
Prohibited classification: Production baseline
```

## 4.1 Logical layout on one temporary physical node

```text
HOSTINGER KVM 2 — TEMPORARY NON-PRODUCTION PHYSICAL NODE

Restricted ingress concept
        │
        ├── Web process boundary
        │     identity / config / read-only artifact-cache / log / health / budget
        │
        ├── API process boundary
        │     identity / config / no shared writable root / log / health / DB-connection budget
        │
        ├── Worker process boundary
        │     identity / config / job-scoped temp-work / log / heartbeat / CPU-RAM-time budget
        │
        ├── SQL Server service boundary
        │     runtime-migration-backup-admin identities / data-log-tempdb / internal health
        │
        ├── Secure Storage logical zones
        │     quarantine / input / processing / result / secured / issue / export
        │
        └── Monitoring collector boundary
              identity / safe exporter / own log-health-buffer / approved remote sink if available

Backups: outside the active node/failure domain
```

## 4.2 Mandatory logical separation

Even on one node, every logical component has its own:

- process/service boundary;
- least-privilege identity and credential reference;
- typed environment configuration;
- writable folder/storage purpose and ACL;
- component/environment-labelled structured logging;
- liveness/readiness/heartbeat state;
- CPU/RAM/connection/thread/temp/disk/concurrency budget;
- startup/drain/recovery/rollback reference;
- owner and incident escalation route.

Co-hosting does not permit shared identity, shared secret, shared writable application root, combined log without component/environment labels, health status that hides one failed process, or a Worker budget that starves API/SQL.

## 4.3 Boundary and limitations

- One physical node remains one failure, maintenance, storage-pressure, network, and administrative blast-radius domain.
- Same-node directories do not create real network/security-zone isolation.
- Active-node snapshot/backup is not a substitute for SQL/object/config/key-aware backup stored outside the node.
- Destructive/load/fault testing must not run concurrently with an active Pilot lane.
- Actual KVM plan resources, OS, patching, disk layout, virtual network, backup, and hPanel controls must be verified at provisioning; this report uses no fixed capacity number.
- Exit is triggered by security isolation, availability, maintenance, capacity/headroom, storage growth, backup/restore, or operational evidence—not by a guessed user count.
- `DRB-HOSTINGER-EXIT-001` is the migration/exit reference; target infrastructure remains unselected.

Hostinger KVM 2 must never be labelled Production, HA, DR, official capacity, long-term secured storage, or architecture dependency.

---

# 5. Production placeholder topology

No Production physical topology is selected. The only valid physical placeholder is:

```text
                    Official clients
            Desktop | Web | Mobile | Tablet
                           │
                  approved SSO + HTTPS
                           │
                           ▼
        PERTAMINA_APPROVED_PRODUCTION_INFRASTRUCTURE
        ┌──────────────────────────────────────────┐
        │ Approved enterprise edge / ingress      │
        │ Web + API + authenticated SignalR       │
        │ Application authorization/admission     │
        │ Persisted Job / Server Worker boundary  │
        │ Private SQL Server metadata boundary    │
        │ Private Secure Storage purpose zones    │
        │ Approved Observability boundary         │
        │ Approved Provider boundary if enabled   │
        └──────────────────────────────────────────┘
```

Logical invariants independent of future physical topology:

- only approved Web/API/SignalR/compatibility/minimal health surface is client-facing;
- SQL, storage, queue/Worker, provider adapter, management, and detailed health are private;
- runtime, migration, backup, administration, deployment, monitoring, and human identities are separated by purpose;
- data/config/secret/telemetry are isolated by environment;
- document payload is not stored as SQL BLOB by default;
- Local-First and explicit selective transfer remain unchanged;
- backup failure domain differs from active SQL/storage;
- health, monitoring, Audit, capacity/backpressure, and rollback evidence are active before rollout completion;
- physical single node, separated VMs, managed services, or HA are options requiring evidence and Pertamina approval—not decisions in this report.

Placeholder resolution:

`REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION`

---

# 6. Data-placement map

## 6.1 Authoritative placement

| Data/asset | Primary placement | Authority/transfer rule | Explicit prohibition |
|---|---|---|---|
| User Source File | User device/source location | User-selected input; source remains unchanged | No auto-upload, no server/telemetry/Audit copy |
| Draft/Working State | Protected Local Workspace; SQLite stores state/reference metadata | Local per profile/device | No generic sync or automatic server backup |
| Undo/Redo | Local SQLite/workspace artifact boundary | Local editing state only | No Enterprise Audit/SQL server payload sync |
| Checkpoint/Recovery | Local workspace for local Job; protected server processing zone for server Job | Job/generation/schema/engine/source/digest bound | No incompatible reuse or content in logs |
| Temp | App/Job-scoped local or server processing zone | Short lifecycle, quota/lease aware | Temporary does not mean lower classification; no silent promotion |
| Cache/Thumbnail/Preview | Protected local app cache by default | Reconstructible; reauthorize before regeneration | No automatic server sync or auth authority |
| Saved Visual Signature | Protected local profile artifact, opt-in | Explicit placement/finalization only; non-PKI | No auto-upload/sync/backup/telemetry/Audit/Issue copy |
| Client token/session | Approved OS secure credential/session boundary | Identity/session lifecycle only | No ordinary SQLite/file/config/log/clipboard |
| Offline grant | Protected minimum local capability projection | Time-bounded, local eligible use only | Not an API/Hub token or privileged permission cache |
| Server Metadata | SQL Server module/schema stores | Application service authority after authorization | Client has no SQL connectivity or direct query |
| Job/Idempotency/Outbox | SQL Server; approved protected local outbox for eligible offline metadata | Durable state/intent; bounded idempotent sync | Queue message/SignalR is not authority |
| Secured File Payload | Private Secure File/Object Storage `secured version` zone | Explicit **Amankan ke Akun**, owner/policy/quota/digest/version | No SQL BLOB default; max-three active/addressable versions |
| Server Input/Result | Private quarantine/input/processing/result storage zones | Explicit server execution route, purpose-bound lifecycle | No raw public URL/path; no cross-purpose access |
| Enterprise Audit | SQL Server `audit` evidence store + protected metadata outbox | Typed, minimized, append-oriented, permission-scoped | No document/OCR/signature/prompt/credential payload |
| Issue Message/Metadata | SQL Server `support` schema | Reporter/Support permission scope | Not Audit/general log or document store |
| Issue Screenshot/Attachment | Private secure attachment storage | User preview + explicit send; exact Issue scope | No automatic capture/upload or broad Support access |
| Telemetry/Logs/Traces | Approved Monitoring backend/stream per environment | Safe operational data only | No business truth, raw content, secret, path, provider payload |
| Build/Release Artifact | Restricted Azure DevOps artifact storage/repository path | Immutable pipeline artifact, digest/SBOM/provenance | Not runtime user storage or secret repository |
| Configuration/Policy | Versioned deployment config/control plane | Environment-bound, typed, validated | No branch-per-environment or manual Production row edit |
| Secret/Key | Approved secret/key/credential boundary | Runtime identity obtains exact purpose reference | No source/artifact/config/log/evidence/client distribution |
| Approved Provider Data | Temporary provider boundary only when authorized | Minimum approved pages/fields, purpose/lease/expiry | No public AI, broad context, retention assumption, or tool authority |
| Backup/Recovery Set | Separate protected recovery location/failure domain | SQL/object/config/key-ref/artifact/Audit-aware | Not active-node-only snapshot or general content browser |

## 6.2 Data-flow guard

```text
Source / Draft / Undo / Cache / Temp / Thumbnail / Signature
                         │
                         └── stay local by default

Selected validated file/result
     ├── explicit server execution upload, if Feature route requires and user authorizes
     └── explicit Amankan ke Akun, if user chooses

Safe offline Audit metadata
     └── separate protected Audit outbox; never document payload
```

No local state is sent to the server merely because a user signs in, reconnects, updates the client, opens History, or enables monitoring.

---

# 7. Trust/security-zone map

Zone names below are conceptual cartography labels, not official Pertamina network-zone names.

## 7.1 Components, trust, and interfaces

| Zone | Components | Trust posture | Authentication | Authorization | Inbound interface | Outbound interface |
|---|---|---|---|---|---|---|
| `ZONE-USER-DEVICE` | User OS/device, user source/output, camera/file picker/share surface | Untrusted/partially trusted environment and input | OS/user/device context; not business identity authority | User gesture/OS permission only; not OTO permission | Physical/user/OS input | To Client Application through explicit file/UI intents |
| `ZONE-CLIENT-APPLICATION` | Desktop, Mobile, Web browser client | Signed/managed code but device, cache, and input remain untrusted | Pertamina SSO/session; app/package identity where supported | UI exposure is not authority; server checks permission/owner/policy | User/OS input; HTTPS/SignalR response | HTTPS API, authenticated SignalR, local IPC, approved telemetry/update |
| `ZONE-LOCAL-PROCESSING` | Local Worker, local engine adapters, job work root | Confined scoped workload; not broadly trusted | Current-user/app/package/process/instance IPC binding | Closed operation allowlist + job/artifact scope | Authenticated versioned Named Pipe/local IPC only | Job-scoped filesystem; network denied by default |
| `ZONE-ENTERPRISE-EDGE` | Approved ingress, certificate termination, Web/API/SignalR exposure concept | Exposed boundary; never identity authority by itself | TLS/client session forwarding under trusted configuration | Route/method/size/rate/origin controls; no object authorization by edge alone | Approved client-facing HTTPS/SignalR | To Web/API only; management path separate |
| `ZONE-APPLICATION-SERVICE` | Web server boundary, API/application services, authorization/admission | Trusted policy-enforcement boundary after validated identity | Workload identity + validated user session/token | Permission + ownership/scope + policy/classification + state/version + reauth | Edge/internal approved service calls | SQL, queue/Worker, storage, provider adapter, telemetry |
| `ZONE-SERVER-WORKER` | Server Worker pools, handler/engine adapters | Scoped internal workload; internal network not automatically trusted | Purpose-scoped workload identity | Persisted Job/lease/handler/schema/object scope | Approved queue/Job locator; no user token | SQL/storage/provider/telemetry exact purpose routes |
| `ZONE-DATA` | SQL Server schemas, Job/Audit/Support metadata | High-integrity authoritative metadata boundary | Separate runtime/migration/backup/admin identities | Schema/module least privilege; application authorization precedes resolution | Internal application/migration/backup/admin only | Backup/monitoring; object references to storage, not public payload |
| `ZONE-SECURE-STORAGE` | Quarantine/input/processing/result/secured/Issue/export zones | Restricted content boundary; object key is not authority | Scoped API/Worker/backup identities | Owner/purpose/type/state/version/retention/hold/zone ACL | Internal upload/promote/read/delete contracts | Authorized streams, backup, cleanup/reconcile |
| `ZONE-OBSERVABILITY` | Logs, traces, metrics, collector/backend, alerts | Operations evidence only; not content/business/Audit authority | Exporter/collector/operator identity | Safe field projection and operations permission | Safe telemetry from components | Restricted dashboards/alerts/Incident references |
| `ZONE-ADMIN-SUPPORT` | Admin, Support, Auditor, monitoring consoles/routes | Privileged control plane with limited scope; no God Mode | Online SSO + reauthentication when required | Granular permission + admin/support/audit scope; content separated | Restricted authenticated UI/API | Official control-plane/application service only |
| `ZONE-APPROVED-PROVIDER` | Approved internal provider/service or controlled engine source | Purpose-scoped dependency; output and provider not instruction authority | Scoped service identity/certificate/approved channel | Feature/profile/purpose/data-minimization/capacity policy | Only approved API/Worker/client-adapter request | Validated response/temporary lifecycle/approved telemetry |

## 7.2 Encryption, data, monitoring, and gaps

| Zone | Encryption/protection | Allowed data | Prohibited data/action | Monitoring | Confirmation gap |
|---|---|---|---|---|---|
| `ZONE-USER-DEVICE` | OS/file controls as available; user source protection outside app governance | User-owned source/output | Treating filename/path/content as trusted command | Client-visible safe local status only | Supported device/endpoint protection/data policy |
| `ZONE-CLIENT-APPLICATION` | TLS; approved local DB/artifact protection; OS secure token store | Minimum profile/capability, selected local files, app state | Server secret, DB/storage master credential, public AI, direct SQL | App version/crash/resource/Feature/Correlation safe data | Distribution, secure-store, certificate, local encryption products |
| `ZONE-LOCAL-PROCESSING` | IPC ACL/peer binding; protected job roots | Exact Job inputs/outputs and typed operation | Public port, arbitrary path/SQL/shell/registry/network/provider/token | Heartbeat/crash/resource/output validation | Exact Windows/package identity and containment PoC |
| `ZONE-ENTERPRISE-EDGE` | Approved TLS end-to-end and certificate lifecycle | Headers/body within validated limits | Trusting forwarded identity/host; exposing management/data plane | Request rejection/rate/TLS/health safe metrics | DNS, certificate, port, proxy, firewall, segment |
| `ZONE-APPLICATION-SERVICE` | TLS/internal encrypted connections; runtime secret boundary | Authorized request metadata/stream and policy context | Stale privileged allow, raw content logs, arbitrary provider route | Request/authz/capacity/dependency/Audit intent | Hosting topology/service identity/secret mechanism |
| `ZONE-SERVER-WORKER` | Encrypted internal/service routes; isolated temp/work | Job-scoped file/content as required | Human impersonation, end-user token, arbitrary handler/egress | Job/lease/checkpoint/resource/engine profile | Queue/product/pool/isolation/resource targets |
| `ZONE-DATA` | Encrypted DB connection and approved at-rest/backup protection | Server metadata/state and safe Audit evidence | Direct client SQL; document BLOB default; shared runtime `db_owner` | Pool/query/IO/log/tempdb/backup/schema | SQL topology/license/HA/backup/DR/identities |
| `ZONE-SECURE-STORAGE` | Private encrypted storage; key separate; digest/integrity | Document/result/secured/Issue/export objects in exact zones | Public listing/link, raw path as auth, cross-zone broad access | Capacity/growth/orphan/cleanup/digest/access failures | Product, private connectivity, key/data residency/retention |
| `ZONE-OBSERVABILITY` | Protected transport/store/access/retention | Safe codes, IDs, durations, counts, versions, aggregates | Document/OCR/prompt/token/secret/signature/path/raw diagnostic | Self-health, freshness, drop/backlog, alert delivery | Backend, retention, SIEM route, ownership, threshold |
| `ZONE-ADMIN-SUPPORT` | HTTPS/session/reauth, restricted export storage | Safe control metadata, exact Issue attachment if permission, scoped Audit | General user content, token/password/signature, impersonation/break-glass | Privileged access/config/export/Audit-of-audit | Organizational scopes, SoD, support/audit assignment |
| `ZONE-APPROVED-PROVIDER` | Approved TLS/identity, temporary encrypted lifecycle | Minimum selected context for approved purpose | Public/unapproved endpoint, training/reuse assumption, secret/user token, tool authority | Availability/latency/quota/circuit/version safe metrics | Provider governance, egress, retention, location, incident/deletion |

Official security-zone names and mapping to network segments remain:

`REQUIRES_PERTAMINA_NETWORK_CONFIRMATION`  
`REQUIRES_PERTAMINA_SECURITY_CONFIRMATION`

---

# 8. Network-flow map

## 8.1 Conceptual allowed flows

| Flow | Protocol/interface concept | Authentication | Data | Controls | Exact network confirmation |
|---|---|---|---|---|---|
| Client → Web | HTTPS | Approved user/session flow as applicable | UI assets, safe requests/responses | TLS, host/origin/proxy rules, headers/body/rate limits | DNS, certificate, port, proxy, firewall, segment |
| Client → API | HTTPS REST | Pertamina Identity token/session validated by API | Typed command/query, explicit upload/download streams | Permission/owner/policy, size/rate/admission, safe errors | DNS, certificate, port, gateway/proxy, source exposure |
| Client → SignalR | Authenticated SignalR over approved HTTPS transport | Same current session boundary; no token in URL | Safe post-commit event envelope only | Server group/subscription auth, buffer/rate, REST reconcile | Hub route, transport, origin/proxy/timeouts/firewall |
| Client → Pertamina Identity | Approved SSO flow | Pertamina controls authentication | Auth protocol artifacts only; no app password capture | issuer/audience/lifetime/state/nonce/PKCE | SSO endpoints, DNS/cert/redirect/network policy |
| API → SQL Server | Internal encrypted DB connection | API workload DB identity | Parameterized metadata/state transactions | Schema least privilege, pool/timeout, no content BLOB default | Host/instance/DNS/cert/port/firewall/segment |
| Worker → SQL Server | Internal encrypted DB connection | Worker workload DB identity | Job/lease/checkpoint/outbox metadata | Purpose/schema least privilege; short transactions | Same network details TBC |
| API/Worker → Secure Storage | Private internal service/storage protocol | Scoped service identity | Purpose-bound streams/object metadata | Opaque ref, zone ACL, digest, quota, no listing | Endpoint/DNS/cert/port/private network/firewall |
| API → Worker/queue | Internal bounded Job/queue contract | Separate workload identities | Job locator/ID, never user token or arbitrary payload | Persisted Job reload, lease, handler/schema validation | Queue product/protocol/endpoint/segment/ACL |
| API/Worker → Approved Provider | Approved HTTPS/internal service route | Purpose-scoped service identity | Minimum selected authorized content/context | Egress allowlist, timeout/circuit/quota, no public fallback | Provider endpoint/DNS/cert/port/firewall/egress segment |
| Desktop → Local Worker | Named Pipe/local IPC | Current-user/app/package/process/instance handshake | Typed command/progress/error/job-scoped refs | Remote denied, ACL, size/rate/deadline/allowlist | Exact pipe identity/ACL platform PoC; no network port |
| Components → Telemetry receiver | Approved telemetry exporter/transport | Exporter/collector identity | Safe logs/traces/metrics only | Field allowlist, batching, retention, no content/secret | Receiver endpoint/DNS/cert/port/proxy/firewall/segment |
| Backup operator/service → SQL/Storage | Approved private backup path | Dedicated backup identity | Encrypted backup/recovery data | Least privilege, integrity, hold, access audit | Backup target/network/key route/ports/location |
| Admin/Support/Auditor → Application surfaces | Restricted HTTPS | SSO + permission + reauth where required | Scoped control/Audit/Issue projections | No direct DB/storage/Worker/provider management | Admin route/DNS/cert/port/firewall/device policy |

## 8.2 Explicitly disallowed flows

- Client → SQL Server direct.
- Client → unrestricted Secure Storage listing/path.
- Client/AI/document → arbitrary Provider/API/URL.
- Public network → Worker, SQL, storage, monitoring admin, or management port.
- Desktop → Local Worker via public TCP port.
- Worker/provider → user identity impersonation or end-user token reuse.
- Telemetry → document/OCR/signature/prompt/token/secret payload.
- Non-Production → Production data/secret/endpoint without approved masked/authorized process.

All exact DNS names, IP addresses, port numbers, firewall rules, proxy/gateway products, network segments, certificate authority/TLS profile, egress destinations, and time source:

`REQUIRES_PERTAMINA_NETWORK_CONFIRMATION`

---

# 9. Configuration/secret boundary

## 9.1 Configuration and source matrix

| Configuration class | Example | Approved conceptual source | Consumer | Secret? | Boundary |
|---|---|---|---|---:|---|
| Signed default | Safe application defaults, localization, schema version, safety ceiling | Immutable release artifact | All hosts/clients | No | Cannot be weaker than security invariants |
| Environment | Environment code, approved base endpoints, telemetry profile | Controlled deployment configuration | Web/API/Worker/clients as filtered | No, but integrity-sensitive | Versioned, environment-bound, validated, rollback reference |
| Enterprise policy | Limits, retention class, minimum client, server processing permission | Governed control plane/API | API and filtered clients | No | Authorization/reauth/Audit; cannot override hard safety |
| Feature flag | Exposure/ring/provider enablement/emergency disable | Governed typed configuration | API/clients/Worker resolver | No | Owner/scope/environment/reason/expiry/review/version |
| Runtime capability | Engine/resource/connection/provider health | Runtime resolver/health observation | Feature discovery/UI/operations | No | Observed state, freshness, not editable truth |
| User preference | Language/view/zoom/favorite/notification | Per-profile local and approved selective preference sync | Client | No | Lowest priority; cannot override policy/security |
| Secret reference | DB/storage/queue/provider/signing/key reference | Approved secret/key/credential service | Exact server/pipeline workload | Reference only | Never exposes secret value to ordinary config/client |

Document 17 composition view:

```text
Default
→ Environment
→ Enterprise Policy
→ Feature Flag
→ Runtime Capability
```

Restriction priority from the Security/Solution baseline:

```text
System Safety/Security
→ Enterprise Policy
→ Environment Configuration
→ Device Configuration
→ User Preference
```

## 9.2 Secret placement

| Secret type | Allowed boundary | Explicit exclusion |
|---|---|---|
| Web session/key material | Approved server runtime key/secret mechanism | Browser JS/static assets, repository, ordinary config/log |
| API DB/storage/queue/provider secret | API workload identity/approved runtime secret source | Client, artifact, source, response, telemetry |
| Worker DB/storage/provider secret | Worker-specific purpose-scoped runtime source | User token, shared API/human credential, queue payload |
| SQL migration/backup/admin credential | Separate controlled identity/process | Runtime app identity and developer personal credential |
| Client token/refresh credential | Approved OS secure store/protected session boundary | Plain file, SQLite, config, log, URL, clipboard |
| Offline grant | Protected minimum local projection | Raw token/claims, Admin/Support/Audit permission, server call credential |
| Storage encryption key | Approved key service/OS key facility; separate from data | Object metadata/plain config/repository |
| Code-signing private key | Approved signing service/key boundary | Prompt, repository, workstation file, package, logs |
| Provider credential | Approved server/provider workload secret boundary | Desktop/Mobile/Web client, document, AI context |

Environment isolation includes separate endpoint, identity, secret source/reference, configuration version, SQL, storage, queue, provider profile, telemetry, and authority. A source branch is not an environment boundary.

Confirmation:

`REQUIRES_PERTAMINA_SECURITY_CONFIRMATION`  
`REQUIRES_PERTAMINA_SSO_CONFIRMATION`  
`REQUIRES_PERTAMINA_CODE_SIGNING_CONFIRMATION`

---

# 10. Capacity/resource map

Capacity classes are planning categories, not SLAs or final numeric limits.

| Workload | Class | Dominant resources | Queue/concurrency/backpressure | Local fallback | Provider fallback | Feature flag role |
|---|---|---|---|---|---|---|
| Metadata API/profile/config/Feature discovery | `LIGHT` | API CPU, SQL connection/query, small network | Pagination, short DB lifetime, rate/concurrency limits; reject safely | Client can use cached UX hints only, not server authority | None | Staged exposure only; cannot bypass auth/policy |
| File upload/download | `MEDIUM`, can become `HEAVY` by size/concurrency | Network, stream, temp disk, storage IO, connection | Streaming/resumable, byte/chunk/range/time limits, admission, `413/429/503` as applicable | Local-only Feature avoids upload; downloaded content requires explicit action | None | Can narrow server exposure/admission during pressure |
| Merge/Split/Rotate local | `MEDIUM` local; potentially heavier for large files/pages | Device CPU/RAM/disk/IO | Local coordinator/Worker limits, batch/page/file bounds, no server queue | Primary valid path | Approved alternate engine only; no server/public fallback unless Feature profile allows | Can disable affected engine/profile, not change Local-First meaning |
| OCR | `HEAVY` | CPU, RAM, pixels/pages/temp/output | Per-engine queue/pool, selected pages, timeout/output limit, cleanup | Local-first where device capability supports | Approved internal/server provider for complex cases only; no public provider | Staged engine/profile exposure and emergency disable |
| Office conversion | `HEAVY` | CPU/RAM/temp/storage/native engine | Background queue, per-engine concurrency, timeout, output validation | Local if approved engine/capability valid | Approved alternate/server provider only | Provider/engine profile rollout/containment |
| AI Summarizer | `PROVIDER_DEPENDENT`, with possible heavy preprocessing | Provider quota/latency, context size, CPU/temp | Call/context/output/time/concurrency quota, circuit, queue/backpressure | Approved local model only if already supported and secure | Approved internal provider only; never public fallback | Scoped provider/Feature disable; cannot bypass permission/consent |
| Translate PDF | `PROVIDER_DEPENDENT` | Provider quota/latency, text/layout size, temp/network | Same as AI plus selected scope/language/output bounds | Approved local translation capability if valid | Approved internal provider only | Same controlled profile behavior |
| Job queue/lease/outbox | `LIGHT` per operation; operationally critical | SQL/queue IO, locks/timers, Worker throughput | Persistent bounded fair queue, weighted admission, one active attempt, oldest-age/drain monitoring | Local jobs use separate local coordinator | Not applicable | Feature/class rollout can reduce arrival; queue safety cannot be disabled |
| Enterprise Audit ingest/query | `LIGHT` per event; `MEDIUM–HEAVY` for backlog/export | SQL log/IO/storage/export CPU/network | Batch/idempotent ingest, keyset query, bounded export, retention/archive; fail closed for required privileged intent | Protected metadata outbox only | None | Mandatory Audit cannot be disabled/sampled by flag |
| Secured storage transfer | `MEDIUM–HEAVY` | Network, object storage, digest, SQL metadata | Explicit admission/quota, streaming, max-three, cleanup/reconcile | Local source/copy remains; no auto-transfer | None | Can stage exposure, not alter ownership/max-three |
| Issue screenshot/attachment | `MEDIUM` | Upload/network/storage/validation | Explicit send, size/type/count/rate limits, private object, retention/expiry | Issue text may proceed without attachment where contract permits | None | May limit attachment path while preserving issue text |

Common controls:

- admission before DB pool/temp/storage/Worker/provider exhaustion;
- cost-weighted per-user/category/global concurrency according to approved policy;
- bounded fair queues and gradual backlog release;
- retry budget, idempotency, exponential backoff/jitter;
- local processing first where valid;
- provider circuit and quota; no public fallback;
- low-priority cleanup/archive and capacity headroom monitoring;
- staged rollout/feature profile containment;
- no false `202`, false `COMPLETED`, duplicate effect, silent wait, or silent user-file deletion.

Numeric limits follow:

```text
TARGET_TO_BE_BASELINED
→ MEASURED_BASELINE
→ TARGET_TO_BE_APPROVED
→ POLICY_CONFIGURABLE
```

---

# 11. Health/monitoring map

## 11.1 Health semantics and signals

| Component | Liveness | Readiness | Key metrics | Alert concept |
|---|---|---|---|---|
| API | `/health/live` minimal process-alive | `/health/ready` only when mandatory config/dependencies/admission are safe | Rate/concurrency, latency, status/error, authz denial pattern, DB pool, body rejection, capacity | Not-ready, error/latency trend, authz/security anomaly, dependency stale, unsafe saturation |
| Worker | Process/heartbeat alive, internal only | Identity/queue/Job store/storage/handler/engine/temp/resource ready; not-ready stops claims | Job duration, concurrency, lease/checkpoint, crash/recycle, memory/temp, engine profile, queue drain | Worker missing/stale, crash loop, lease/stale completion, residual memory, queue not draining |
| SQL Server | DB service/process connectivity | Correct schema/migration/permission plus safe pool/IO/log state | Pool wait, query latency/CPU/reads, locks/deadlocks, log/tempdb, IO/free space, backup | Connection/pool exhaustion, blocking/deadlock trend, log/disk pressure, backup/migration failure |
| Secure Storage | Service/endpoint availability | Capacity/key/identity/zone operations/digest lifecycle safe | Bytes/objects/growth, upload/download latency, temp peak, orphan, cleanup, digest/finalization | Unavailable, capacity pressure, integrity mismatch, orphan/reconcile/cleanup backlog |
| SignalR | Hub/process surface reachable as part of API | Authenticated group/subscription/outbox/event flow compatible | Connections, reconnect, buffer, duplicate/gap/stale, REST reconciliation | Cross-user denial/security signal, reconnect storm, buffer pressure, persistent gap |
| Audit Pipeline | Process/outbox/ingestion path observable | `HEALTHY`, `DEGRADED`, or `UNHEALTHY` by durable intent/backlog/integrity | Intent latency, backlog age/count, ingest accepted/duplicate/reject, poison/version, archive/purge/export | Mandatory intent not durable, integrity/hold threat, backlog no progress, health falsely green |
| Issue Service | API/module/outbox available | Message submission durable; attachment/export dependencies safe for those paths | Submit/attachment/export success/failure, backlog, idempotency, storage capacity | Issue text cannot persist, attachment failure spike, Support export backlog/failure |
| SSO Dependency | OTO observes approved identity dependency reachability/freshness; does not own IdP liveness | Token/session validation metadata and approved flow current; no stale privileged fallback | Auth success/failure category, validation/revocation/refresh signals, dependency latency/freshness | Identity unavailable/stale metadata, replay/reuse, unusual failures; safe user guidance |
| Approved Provider | Safe profile heartbeat/status/freshness | Correct approved profile/version/capacity/circuit and policy | Latency/error, quota/concurrency, circuit, timeout, output validation, version | Provider stale/unavailable, circuit open, quota pressure, unsafe output; no public fallback |
| Client Compatibility | Compatibility endpoint/process reachable | Accurate platform/channel version policy and Feature/config schema | Status distribution, old-client calls, update-required/unsupported rate, contract mismatch | Unexpected broad block, stale/wrong minimum version, incompatible client traffic spike |

## 11.2 Evidence, correlation, runbook, and ownership

| Component | Log source | Trace source | Correlation | Runbook/reference | Functional owner |
|---|---|---|---|---|---|
| API | Structured API/application log | OpenTelemetry request/dependency traces | Correlation ID from edge through app/Job/Audit | Document 16 Web/API; `DRB-API-FAIL-001` | API/Application Owner TBC |
| Worker | Worker/engine structured log | Job/queue/storage/provider spans | Correlation + Job + attempt/lease | Document 16 Worker/Queue; `DRB-WORKER-FAIL-001`, `DRB-QUEUE-HIGH-001` | Processing/Worker Owner TBC |
| SQL | DB/platform safe operational log | DB dependency spans from callers; restricted DB telemetry | Correlation where safe, not query/content payload | Document 16 Database; `DRB-DB-MIG-FAIL-001`, `DRB-RESTORE-001` | Database Owner/DBA TBC |
| Storage | Storage/service lifecycle log | Upload/download/object dependency spans | Correlation + opaque object/Job ref within authorization | Document 16 Storage/Data Integrity; `DRB-STORAGE-FAIL-001` | Secure Storage Owner TBC |
| SignalR | API/Hub safe connection/event log | Connect/reconnect/publish spans | Correlation + event sequence/Job, never access grant | Document 16 SignalR | API/Realtime Owner TBC |
| Audit Pipeline | Audit intent/outbox/ingestion/lifecycle safe log | Audit pipeline spans without event payload | Audit Event ID + Correlation; reference is not permission | Document 16 Audit; Document 10 failure/recovery | Audit Platform Owner TBC |
| Issue Service | Support module/storage safe log | Submit/attachment/export spans | Issue + Job/Correlation under scope | Document 16 Issue Reporting; storage/API runbooks | Support Service Owner TBC |
| SSO Dependency | Safe auth/session outcome logs | Identity dependency spans without token/claims | Correlation/session reference opaque | Document 16 Identity/SSO; `DRB-SECRET-MISSING-001` if related | Identity Integration Owner TBC |
| Provider | Adapter/Worker safe profile logs | Provider dependency spans without request/response content | Correlation + Job + profile version | Document 16 PDF/OCR/AI; queue/hotfix references | Provider/Engine Owner TBC |
| Client Compatibility | API compatibility and client safe logs | Compatibility/config/feature discovery spans | Correlation + platform/version, no device secret | Document 16 Client/Version; `DRB-CLIENT-INCOMPAT-001` | Release/Compatibility Owner TBC |

Monitoring principles:

- environment/component/version/freshness labels are mandatory;
- unknown or stale critical telemetry is not `HEALTHY`;
- alert has owner, source policy, threshold status, route, deduplication, and Incident link;
- detailed health does not expose topology, credential, queue internals, path, content, or provider endpoint to clients;
- monitoring is not Enterprise Audit or persisted Job/business authority;
- actual targets/cadence remain `TARGET_TO_BE_APPROVED`.

---

# 12. Backup/recovery dependency map

## 12.1 Conceptual recovery order

```text
Incident authority / recovery scope / approved recovery point
                          │
                          ▼
1. Identity / Network / DNS / Certificate / Time
                          │
                          ▼
2. Secret / Key / Configuration / Policy / Trusted Artifact
                          │
                          ▼
3. SQL Server / schema / permissions / outbox / idempotency
                          │
                          ▼
4. Secure Storage / object / version / digest / key-ref / tombstone / hold
                          │
                          ▼
5. API control plane / authorization / compatibility authority
   (background Job admission remains closed until Worker path is ready)
                          │
                          ▼
6. Queue + Server Worker / lease / checkpoint / handler
                          │
                          ▼
7. Web + SignalR / client-facing access and REST reconciliation
   + enable only applicable API admission
                          │
                          ▼
8. Full Monitoring / alert / dashboard validation
                          │
                          ▼
9. Audit pipeline / backlog replay / query-export-lifecycle validation
                          │
                          ▼
10. Client Compatibility / Desktop-Mobile reconnect-update validation
```

This is a dependency map, not a step-by-step deployment procedure or RTO commitment.

## 12.2 Circular-dependency guards

- **Minimum recovery instrumentation** must be available throughout recovery even though full Monitoring validation is shown at step 8. Otherwise recovery becomes blind.
- Mandatory Audit intent/outbox data is restored with SQL at step 3, while full Audit dispatch/query/lifecycle validation occurs at step 9. This avoids treating Audit as absent until the end.
- Document 17 startup order validates network/identity/config/secret, then SQL/storage/queue/Audit, then API/Worker, Web/SignalR, and clients. The prompt's API-before-Worker recovery view is reconciled by bringing up API control-plane/compatibility in a non-admitting or capability-degraded state at step 5; background admission opens only after queue/Worker readiness at steps 6–7.
- Identity, DNS/certificate, time, secret/key, and configuration may depend on enterprise services outside OTO. Their recovery authority and fallback cannot be invented here.
- API readiness must not require Worker/provider for Features that can safely operate without them; use capability-specific degraded state.
- Worker must not claim work until SQL/queue/storage/handler dependencies are authoritative and ready.
- Web/clients consume `/api/compatibility`; they do not decide server schema or bypass unsupported state.

## 12.3 Backup dependency inventory

| Recovery domain | Required backup/reference | Validation dependency |
|---|---|---|
| Governance | Environment inventory, Incident ID/scope, approved recovery point, owner/authority | Document 16 Incident lifecycle and approval |
| Network/identity | DNS/certificate/time/SSO configuration inventory and secure recovery reference | Network/SSO authority, certificate/time validation |
| Secret/config/artifact | Secret/key recovery metadata, typed config/policy/flags, immutable artifact/digest/SBOM/release manifest | Correct environment/identity/version; no embedded secret |
| SQL Server | DB/log/migration history/module permissions/outbox/idempotency/Audit metadata | Integrity, schema, authorization, app compatibility |
| Secure Storage | Object/version/digest/key-ref/tombstone/retention/hold and catalog | SQL/object cross-store reconciliation; max-three secured versions |
| Job/Worker | Job/attempt/lease/checkpoint/handler compatibility | No duplicate/stale completion; safe resume |
| Monitoring | Exporter/dashboard/alert configuration and owner/escalation mapping | Freshness, safe data, alert delivery |
| Audit | Durable intent/outbox/events/archive/hold/lifecycle references | Append, dedup, late provenance, query/export scope |
| Client | Distribution/package/signing/compatibility metadata | Supported/update status, local state preservation, current auth |

Runbook linkage:

- Document 16: Identity/SSO, Web/API, Worker/Queue, Database, Storage, SignalR, Audit, Client/Version, Local/Offline, Selective Sync, Data Integrity.
- Document 17 register: `DRB-RESTORE-001`, `DRB-ROLLBACK-001`, `DRB-DB-MIG-FAIL-001`, `DRB-STORAGE-FAIL-001`, `DRB-WORKER-FAIL-001`, `DRB-HOSTINGER-EXIT-001`, and client update/migration entries.

RPO, RTO, schedule, retention, immutable/offline copy, DR site/region, replication/failover, key escrow, operator authority, and drill cadence:

`REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION`

---

# 13. Single-point-of-failure review

| Potential hidden SPOF | Current conceptual risk | Evidence/decision needed | Status |
|---|---|---|---|
| Hostinger KVM 2 physical node | Web/API/Worker/SQL/storage/monitoring share failure/maintenance/resource domain | Non-Prod only; external backup; exit trigger; recovery rehearsal | Known temporary SPOF; prohibited as Production baseline |
| One active disk/storage path on temporary node | SQL log/temp/object/log compete; disk loss/pressure broad impact | Per-process budgets, cleanup, outside-node backup, measured restore | Non-Prod risk accepted only by approval |
| Single ingress/proxy/certificate path | Client cannot reach Web/API/SignalR | Production topology, certificate renewal/failover, monitoring | TBC; do not invent redundancy |
| Single SQL Server instance | All server metadata/Job/Audit/Support depend on it | Availability target, backup/restore, topology/HA decision | `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` |
| Single secure storage service/location | File processing/secured/Issue/export unavailable or data loss | Failure domain, backup/version/digest/key restore evidence | TBC |
| Queue/Job store concentrated in SQL or one service | Worker admission/drain stalls | Product/topology, durability, capacity/recovery evidence | TBC; no new queue product selected |
| One Worker/pool for all workload classes | OCR/conversion/provider starves other Jobs | Pool/class separation concept, concurrency, backpressure, Pilot measurements | Logical controls required; physical scale TBC |
| One approved provider/engine | Dependent Features unavailable or supply-chain risk | Approved fallback/disable, local path, exit/rollback, incident terms | Feature-specific residual risk |
| Pertamina Identity dependency | Authentication/server privilege unavailable | SSO availability/revocation/offline policy, safe degraded behavior | `REQUIRES_PERTAMINA_SSO_CONFIRMATION` |
| DNS/certificate/time source | Broad connectivity/auth/expiry/lease failure | Ownership, monitoring, renewal/revocation/failover | `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION` |
| Secret/key service | API/Worker/storage/provider cannot start or decrypt | Availability, recovery/rotation, identity, no plaintext fallback | TBC security/infrastructure |
| Monitoring co-hosted only | Node loss removes evidence/alerts with services | Approved separate sink/retention, minimum recovery instrumentation | Non-Prod collector may co-host; Production TBC |
| Audit pipeline/storage | Privileged action may fail closed; evidence backlog | SQL/outbox durability, health, replay, retention/hold | Critical dependency; full topology TBC |
| Azure DevOps artifact/release source | No trusted deploy/rollback package available | Retention, availability, restricted publication, last approved artifact evidence | Supply-chain dependency TBC |
| Desktop/Mobile distribution/update channel | Unsupported client cannot recover/update | Enterprise distribution, signing, offline package, compatibility path | Distribution confirmation required |
| One human owner/knowledge path | Incident/deployment/recovery decisions stall | Functional owner, deputy, SoD, support handoff, runbook exercise | `REQUIRES_PERTAMINA_ORGANIZATION_CONFIRMATION` |
| Single backup location on active node | Common-mode failure destroys active and recovery copy | Separate failure domain and restore validation | Explicitly prohibited |

SPOF existence is not automatically a requirement to add infrastructure. It is an input to approved availability, cost, security, capacity, and recovery decisions.

---

# 14. Environment deployment matrix

Legend:

- `L` — local/client boundary;
- `R` — required logical component for that environment's server scope;
- `O` — optional, only when the test/use case needs it;
- `A` — approved conditional dependency/profile;
- `P` — Production logical need, physical placement unresolved;
- `–` — not normally present.

| Component | Local Development | Shared Development | QA/Testing | Pilot | Production |
|---|:---:|:---:|:---:|:---:|:---:|
| Desktop client | `L` | `O` | `R` for Desktop scope | `A` | `P` after distribution approval |
| Mobile/Tablet client | `O` | `O` | `R` for released mobile scope | `A` | `P` after distribution approval |
| Web client/host | `O` | `R` | `R` | `R` | `P` |
| API | `O` for integration | `R` | `R` | `R` | `P` |
| Server Worker | `O` for integration | `R` for background/server Features | `R` | `R` | `P` |
| Local Worker | `L` with Desktop heavy/risky scope | `L` | `R` for Desktop/IPC test | `A` | `P` with Desktop cohort |
| SQL Server | `O` isolated | `R` Dev-only | `R` QA-only | `R` Pilot-only | `P` internal |
| SQLite | `L` per profile | `L` per client | `L` per test profile | `L` per participant/client | `P` per client/profile |
| Local Workspace | `L` | `L` | `L` | `L` | `P` on user device |
| Secure File/Object Storage | `O` isolated | `R` Dev zones | `R` QA zones | `R` Pilot zones | `P` private |
| Monitoring/Telemetry | `L` basic | `R` component health | `R` full test observability | `R` operational/alert rehearsal | `P` active before rollout |
| Approved Provider/Engine | `A` non-Prod only | `A` separate Dev profile | `A` QA profile/corpus controls | `A` Pilot-approved profile | `P` only after provider approval |
| Azure DevOps artifact source | `O` for task/local build feedback | `R` pipeline artifact | `R` same artifact | `R` exact RC artifact | `P` approved same RC artifact |
| Backup/recovery location | `O` test snapshot | `R` change recovery point | `R` restore rehearsal | `R` verified set/test | `P` separate approved failure domain |

Matrix constraints:

- every environment has distinct endpoint, identity, data, config, secret, SQL/storage/queue/provider, telemetry, and deployment authority;
- Hostinger KVM 2 may fill physical placement only for Shared Development, limited QA, or approved limited Pilot;
- Production cells marked `P` resolve only inside `PERTAMINA_APPROVED_PRODUCTION_INFRASTRUCTURE`;
- exact platform exposure remains based on release/capability evidence, not presence of a deployable alone.

---

# 15. Diagram recommendations

The master Document 18 should contain small diagrams with opaque logical labels only. Recommended set:

| Diagram | Purpose | Required nodes/edges | Exclusions |
|---|---|---|---|
| C4-style System Context | Show users, clients, OTO server boundary, Identity, provider, monitoring | Desktop/Web/Mobile → Identity/API; API/Worker → data/provider; telemetry | IP, DNS, port, vendor topology, credential |
| Deployable/Runtime Map | Distinguish Desktop in-process, Local Worker, Web, API, Server Worker, SQL, Storage, Monitoring | Process/runtime labels, identity/config/health boundaries | Deployment commands and physical Production choice |
| Hostinger Single-Node Logical Separation | Prove six logical boundaries on one temporary node | Web/API/Worker/SQL/Storage/Monitoring + shared-node failure domain | Production label, capacity claim, fake network segmentation |
| Production Placeholder | Preserve architecture without invented topology | `PERTAMINA_APPROVED_PRODUCTION_INFRASTRUCTURE` containing logical zones | VM/container/cloud/HA/product selection |
| Trust-Zone Diagram | Show 11 conceptual zones and enforcement crossings | User Device → Client → Edge → App → Worker/Data/Storage/Provider; Admin/Observability side planes | Official zone names/segments until approved |
| Data-Placement Diagram | Prevent local/server leakage | Source/draft/cache/signature local; SQL metadata; storage payload; Audit; Issue; telemetry; artifact | Filenames, paths, real data, physical endpoints |
| Network-Flow Diagram | Show permitted protocol classes and denied direct flows | HTTPS/SignalR, internal SQL/storage/provider, Named Pipe, telemetry exporter | Exact DNS/IP/port/firewall/proxy |
| Recovery Dependency DAG | Detect circular recovery assumptions | Identity/network → secret/config → SQL → storage → API → Worker → Web → Monitoring → Audit → Client | Detailed restore commands or RTO numbers |
| Health Ownership Matrix | Link component, signal, alert, runbook, functional owner | API/Worker/SQL/Storage/SignalR/Audit/Issue/SSO/Provider/Compatibility | Sensitive telemetry and unconfirmed people names |

Recommended notation:

```text
[Solid box]       logical deployable/runtime
((Cylinder))      authoritative metadata store
[[Double box]]    restricted content store
<Dashed box>      external/approved dependency
── HTTPS/auth     authenticated network flow
·· Named Pipe ··  local IPC only
⇢ telemetry       safe one-way observability flow
✕                 explicitly prohibited flow
```

Each diagram should carry this note:

> Zone names and placements are conceptual. Exact Production infrastructure, DNS, certificate, port, proxy, firewall, and network segment require Pertamina confirmation.

---

# 16. Pertamina confirmation gaps

## 16.1 Document 17 reconciliation

- Master `docs/17-deployment/17_DEPLOYMENT_GUIDE_FINAL_v1.0_PERTAMINA_OTO.md` and its FINAL registers have been read and reconciled. No Document 17 source-availability gap remains in this report.

## 16.2 Infrastructure

`REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION`

- Production hosting/model: physical/virtual/managed/HA;
- server OS, patching, process/service/container applicability;
- ingress/load balancing, private connectivity, admin access, service placement;
- SQL/storage/queue/monitoring products, topology, availability, licensing, support;
- Hostinger KVM 2 actual plan/resources/OS/provisioning controls and approved non-Prod scope;
- environment ownership, maintenance, scaling, and decommission policy.

## 16.3 Network and security zones

`REQUIRES_PERTAMINA_NETWORK_CONFIRMATION`  
`REQUIRES_PERTAMINA_SECURITY_CONFIRMATION`

- official security-zone/network-segment names and mappings;
- DNS domains/ownership, IPs, ports/protocols, firewall sources/destinations, proxy/gateway, egress allowlists;
- TLS/certificate authority/profile/private-key boundary/renewal/revocation;
- SSO endpoints, redirect URIs, allowed origins, trusted forwarded headers;
- provider, telemetry, backup, time-sync, admin, and deployment network paths;
- host/process hardening, endpoint protection, vulnerability/VA-PT controls.

## 16.4 Identity, secrets, and ownership

`REQUIRES_PERTAMINA_SSO_CONFIRMATION`  
`REQUIRES_PERTAMINA_ORGANIZATION_CONFIRMATION`

- workload/service identity product and exact scopes;
- secret/key service, injection, rotation, revocation, recovery, audit;
- owner/on-call/escalation for every deployable, data plane, health signal, alert, backup, and provider;
- Admin/Support/Auditor/deployment/DBA/backup/monitoring SoD and access routes.

## 16.5 Backup, recovery, capacity, and monitoring

`REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION`

- RPO/RTO, backup frequency/retention, immutable/offline copy, DR location/data residency;
- replication/failover, key escrow, DNS/certificate failover, exercise cadence and authority;
- numeric CPU/RAM/disk/IO/network/connection/concurrency/queue/retention thresholds;
- monitoring backend/topology/retention/cardinality, SIEM route, health thresholds, alert cadence/owner;
- recovery order adjustments and minimal instrumentation bootstrap.

## 16.6 Distribution, signing, and provider

- `REQUIRES_PERTAMINA_CODE_SIGNING_CONFIRMATION`;
- `REQUIRES_PERTAMINA_DESKTOP_DISTRIBUTION_CONFIRMATION`;
- `REQUIRES_PERTAMINA_MOBILE_DISTRIBUTION_CONFIRMATION`;
- engine/model/provider product, source, license, integrity, egress, retention, fallback, removal, and incident obligations;
- Azure DevOps artifact retention/access/availability and recovery evidence.

These gaps do not permit Production topology invention, Hostinger-as-Production, client SQL access, embedded secret, public provider, automatic full-workspace sync, or missing monitoring/health.

---

# 17. Risks/conflicts

| Risk/conflict | Finding | Required treatment |
|---|---|---|
| Prompt recovery order versus Document 17 startup order | Prompt lists API before Worker; Document 17 groups queue/Audit readiness before API/Worker | Treat step-5 API as control-plane/compatibility with background admission closed; enable applicable admission only after queue/Worker readiness |
| Production map requested but topology invention prohibited | Physical placement cannot be completed | Use exact placeholder `PERTAMINA_APPROVED_PRODUCTION_INFRASTRUCTURE` plus logical invariants only |
| Hostinger one node versus security zones | Logical process/identity/storage separation is not physical network isolation | Label single failure/security domain; keep non-Prod; do not claim segmentation/HA |
| Hostinger actual resource values can change | Fixed capacity claim becomes stale/incorrect | Verify at provisioning; use measured budgets and no final numeric capacity in cartography |
| Monitoring appears late in recovery order | Recovery could be blind or create circular dependency | Bootstrap minimum instrumentation throughout; restore full monitoring later |
| Audit listed after Monitoring but Audit intent lives in SQL/outbox | Privileged actions could proceed without evidence if interpreted literally | Restore Audit durability with SQL; validate full pipeline after services; keep fail-closed rules |
| API readiness depends on every provider/Worker | One optional dependency can cause total outage | Capability-specific degraded readiness; mandatory path only controls global readiness |
| Queue not in requested deployable catalog | Hidden operational dependency | Map as dependency within API/Worker/SQL boundary; do not invent product/deployable |
| SignalR mistaken as source/deployable authority | Stale/gap events may drive wrong state | Treat as API realtime surface; REST/persisted state remains authority |
| SQLite/Workspace mistaken for server services | Local state could be uploaded or centrally administered | Keep per-profile device boundaries and no auto-sync |
| SQL/object authority split | Restore can produce orphan/resurrection/cross-user state | Mandatory SQL–object–digest–key–tombstone–hold–permission reconciliation |
| Secure Storage same-node directory in non-Prod | Directory boundary can be mistaken for enterprise object-storage security | Label temporary logical zone; Production product/private boundary TBC |
| Telemetry co-hosted with app | Node loss removes monitoring evidence; logs may compete for disk | Export to approved separate sink where available; cap buffers; Production topology TBC |
| Observability confused with Audit | Sampling/log loss can invalidate evidence claims | Maintain separate planes, permissions, retention, health, and recovery |
| Provider labelled “approved” but route/product unspecified | Public/unapproved endpoint could enter through config | Approved profile/identity/egress/data policy only; no public fallback; confirmation gate |
| Admin/Support zone interpreted as content clearance | Privileged metadata access expands to user documents | Role Matrix remains authoritative; no general content, impersonation, or break-glass |
| Public health exposes dependency details | Attackers learn topology/capacity | Minimal health response; detailed health restricted to operations scope |
| Environment separated only by config name | Cross-environment identity/data/secret leakage | Separate endpoint, identity, DB/storage/queue/provider/telemetry/authority, not branch label |
| Backup stored on active Hostinger disk/snapshot only | Common-mode loss and incomplete product restore | Separate failure domain and SQL/object/key/config-aware restore tests |
| Artifact repository unavailable | No trusted rollback/update package | Approved retention/access/recovery; immutable artifact references; no workstation rebuild |
| Client compatibility treated as UI-only | Unsupported client can call stale contracts | `/api/compatibility`, contract enforcement, filtered capability, approved update path |
| Capacity class used as final SLA | Under/overprovisioning and false Production claim | Measure → approve → configure; no final number in Document 18 |
| Physical separation used as security substitute | AuthZ/data minimization may be weakened | Enforce identity, authorization, encryption, validation, and least privilege at every topology |

No deployable, role, schema, endpoint, product, or infrastructure component is added by this report.

---

# 18. Suggested Document 18 text

The following text may be integrated into master Document 18 after Integrator review. Document 17 FINAL and its registers have already been reconciled into this report.

## Application Cartography Baseline

PERTAMINA OTO cartography distinguishes logical deployables, runtime processes, data stores, trust zones, environment instances, and physical placement. Co-hosting does not merge logical boundaries. Presence on an internal network does not create trust, permission, or content access.

The application catalog contains:

```text
Client/runtime
├── PERTAMINA OTO Desktop — WinUI 3
├── PERTAMINA OTO Mobile/Tablet — .NET MAUI
├── PERTAMINA OTO Web — Responsive Web client/host
├── PERTAMINA OTO Local Worker — isolated local process
├── SQLite — per-profile local metadata/state
└── Local Workspace — source/draft/checkpoint/temp/cache/result/signature boundary

Server/dependency
├── PERTAMINA OTO API — ASP.NET Core REST authority + SignalR surface
├── PERTAMINA OTO Worker — persisted Job execution workload
├── SQL Server — authoritative server metadata/Audit/Support state
├── Secure File/Object Storage — private purpose-separated payload zones
├── Monitoring/Telemetry — safe observability plane
└── Approved Provider/Engine — adapter-controlled local/internal dependency
```

Each catalog entry records artifact, technology, runtime/process, host, environment, typed configuration, secret source, identity, network/data dependency, health, monitoring, scaling, backup, rollback reference, runbook, functional owner, and lifecycle. Actual organization/principal names require `REQUIRES_PERTAMINA_ORGANIZATION_CONFIRMATION`.

SQLite, Local Workspace, SignalR, queue semantics, Secure Storage, Monitoring, Identity, and Provider are not reclassified arbitrarily as new products. SignalR remains notification-only; persisted REST/database state remains authority. Queue/product topology, monitoring backend, storage product, and provider remain confirmation items.

### Environment map

Five environments are separated:

| Environment | Purpose | Data | Physical posture |
|---|---|---|---|
| Local Development | Unit/component/local feedback | Synthetic minimal | Developer device and optional isolated dependencies |
| Shared Development | Shared integration/smoke | Synthetic by default | Temporary logically separated node permitted |
| QA/Testing | System/security/recovery/bounded performance | Synthetic/sanitized-approved; controlled malicious corpus | Isolated test lane and window |
| Pilot | Limited operational validation | Explicitly approved Pilot data | Dedicated logical lane; never Production |
| Production | Official operation | Governed operational data | `PERTAMINA_APPROVED_PRODUCTION_INFRASTRUCTURE` |

Every environment separates endpoint, identity, data, config, secret, SQL, storage, queue, provider profile, telemetry, deployment source/authority, and backup. Production data/secrets do not enter non-Production without approved authorization, masking, and handling. Source branch does not define environment.

### Hostinger KVM 2 boundary

Hostinger KVM 2 is `SINGLE_NODE_TEMPORARY` for Shared Development, limited QA, or an explicitly approved limited Pilot only. It is never a Production, HA, DR, official-capacity, or long-term-secured-storage baseline.

```text
HOSTINGER KVM 2 TEMPORARY NODE
├── Web process       — own identity/config/log/health/budget
├── API process       — own identity/config/log/health/connection budget
├── Worker process    — own identity/config/work/log/heartbeat/resource limits
├── SQL service       — separated DB identities/data-log health
├── Storage zones     — quarantine/input/processing/result/secured/issue/export
└── Monitoring        — separate collector/config/health; approved remote sink if available

Backup/recovery copy is outside the active node failure domain.
```

One node remains one common failure, maintenance, network, disk, and administrative blast-radius domain. Logical separation must not be presented as physical security segmentation. Actual Hostinger resource values are verified at provisioning; no fixed number in cartography becomes a Production claim.

### Production placeholder

No Production hosting, OS, VM/container, gateway, load balancer, managed service, or HA topology is selected. The physical placeholder is exactly:

`PERTAMINA_APPROVED_PRODUCTION_INFRASTRUCTURE`

The logical Production invariants remain Web/API/SignalR at the approved client edge; object-aware authorization/admission in the application service; Server Worker under workload identity; SQL and Secure Storage private; Monitoring/Audit separated by purpose; provider route approved; backups outside active failure domain; and Local-First behavior unchanged.

### Data placement

```text
User Source File                    → local user device
Draft / Working State              → Local Workspace + SQLite metadata
Undo / Redo                        → local
Cache / Thumbnail / Preview        → protected local cache
Saved Visual Signature             → protected local profile, opt-in, non-PKI
Server Metadata / Job / Outbox     → SQL Server
Secured File Payload               → private Secure Storage
Enterprise Audit                   → SQL Server Audit store/outbox metadata
Issue Message                      → SQL Server Support schema
Issue Screenshot                   → private secure attachment storage
Telemetry                          → approved observability backend
Build/Release Artifact             → restricted Azure DevOps artifact storage
Secret/Key                         → approved secret/key boundary
```

Draft, source, undo/redo, cache, thumbnail, temp, checkpoint, clipboard, password, and signature do not automatically go to the server. Server execution upload and **Amankan ke Akun** are separate explicit authorized paths. SQL Server is never client-facing and does not store document BLOB by default.

### Conceptual trust zones

The cartography uses conceptual labels:

```text
ZONE-USER-DEVICE
→ ZONE-CLIENT-APPLICATION
→ ZONE-ENTERPRISE-EDGE
→ ZONE-APPLICATION-SERVICE
→ ZONE-SERVER-WORKER
→ ZONE-DATA / ZONE-SECURE-STORAGE

ZONE-CLIENT-APPLICATION
·· authenticated local IPC ··
ZONE-LOCAL-PROCESSING

Side planes:
ZONE-OBSERVABILITY
ZONE-ADMIN-SUPPORT
ZONE-APPROVED-PROVIDER
```

These names are not official Pertamina network zones. Each crossing validates identity, typed/versioned contract, permission/scope, policy, encryption, data minimization, resource limits, safe monitoring, and current state. Administrator/Support/Auditor zones do not imply document content access.

### Network flows

Allowed conceptual flows are Client→Web/API through approved HTTPS; Client→authenticated SignalR; API/Worker→SQL internally; API/Worker→Secure Storage privately; API/Worker→approved provider through constrained egress; Desktop→Local Worker by authenticated versioned Named Pipe; and components→telemetry receiver with safe data. Client→SQL, client→storage listing, public→Worker/data/management, arbitrary provider URL, user token→Worker/provider, and content→telemetry are prohibited.

All exact DNS, IP, port, firewall, proxy/gateway, network segment, certificate/TLS profile, SSO endpoint, egress destination, and time source require `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION`.

### Configuration and secrets

Document 17 composes effective runtime configuration as Default → Environment → Enterprise Policy → Feature Flag → Runtime Capability. The restriction priority remains System Safety/Security → Enterprise Policy → Environment Configuration → Device Configuration → User Preference; the most restrictive safe result wins. Configuration is typed, validated, versioned, environment-bound, and rollback-aware. Feature flags support rollout/containment but cannot change Feature ID/business meaning, authorization, Audit, Local-First, public-AI block, maximum-three versions, or non-PKI scope.

Client may receive integrity-controlled API base address, compatibility metadata, filtered feature exposure, non-secret policy, and localization. Client never receives database/storage master credential, server/provider secret, private signing key, or unrestricted provider credential. Web/API/Worker/migration/backup/admin/exporter identities and secret references remain separated by purpose.

### Capacity map

Planning classes are `LIGHT`, `MEDIUM`, `HEAVY`, and `PROVIDER_DEPENDENT`. Metadata APIs are `LIGHT`; file transfer and Issue attachments are generally `MEDIUM`; OCR/Office conversion are `HEAVY`; AI Summarizer and Translate PDF are `PROVIDER_DEPENDENT`; queue/Audit operations are light per item but operationally critical; secured transfer becomes heavier by bytes/concurrency.

Control uses local processing first, streaming, bounded queues, weighted admission, per-class concurrency, quota, rate limit, backpressure, retry budget/idempotency/backoff, engine pools, provider circuits, cleanup/retention, and staged feature exposure. Provider fallback is only pre-approved and never public. Numeric values progress from `TARGET_TO_BE_BASELINED` to measured, approved, then policy-configurable; cartography does not invent final limits.

### Health and monitoring

API, Worker, SQL, Storage, SignalR, Audit Pipeline, Issue Service, SSO Dependency, Provider, and Client Compatibility each have separate liveness/readiness, metrics, logs/traces, alert concept, Correlation ID strategy, runbook, and functional owner. API exposes minimal `/health/live` and `/health/ready`; Worker health is internal; Audit uses `HEALTHY/DEGRADED/UNHEALTHY`; client compatibility uses `SUPPORTED/UPDATE_AVAILABLE/UPDATE_REQUIRED/UNSUPPORTED`.

Monitoring is active before rollout completion and excludes document/OCR/prompt/password/token/secret/signature/path/raw diagnostic. Unknown/stale critical telemetry is not healthy. Monitoring is not Enterprise Audit or persisted business state.

### Recovery dependencies

```text
Identity/Network/Time
→ Secret/Key/Configuration/Artifact
→ SQL Server
→ Secure Storage reconciliation
→ API control-plane/compatibility (background admission closed)
→ Queue/Worker
→ Web/SignalR + applicable API admission
→ Full Monitoring
→ Audit pipeline validation
→ Client Compatibility
```

Minimum recovery instrumentation runs throughout. Audit intent/outbox is restored with SQL; full Audit replay/query/lifecycle is validated later. This view reconciles Document 17 startup order by keeping background admission closed until queue/Worker readiness. Restore is incomplete until SQL/object/version/digest/key/tombstone/hold/permission, Job/idempotency/outbox/Audit, maximum-three secured versions, monitoring, and client compatibility reconcile without resurrection, duplicate effect, false success, or cross-user access.

Detailed Incident handling references Document 16. Deployment rollback/restore references Document 17 runbook register. RPO/RTO, backup/DR topology, immutable/offline copy, key escrow, failover, and drill cadence require `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION`.

### Confirmation boundary

Production infrastructure, official zone names, network rules, DNS/certificates/ports, SSO, service identities, secret/key products, owner/on-call functions, monitoring backend/thresholds, backup/DR, distribution/signing, capacity limits, and provider route remain confirmation gaps. Open confirmation never permits Hostinger-as-Production, Production topology invention, client SQL access, embedded secret, public AI/provider, local-state auto-sync, component without health/monitoring, or backup only on the active node.

---

**Keputusan tetap milik user dan Integrator.**
