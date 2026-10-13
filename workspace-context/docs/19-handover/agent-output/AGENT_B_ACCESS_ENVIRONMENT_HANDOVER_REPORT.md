# AGENT B — ACCESS, ENVIRONMENT, CONFIGURATION, DEPLOYMENT & DATA HANDOVER REPORT

**Task ID:** `DOC19-AGENT-B`  
**Scope:** Environment, infrastructure, access, service identity, configuration, secret reference, database, storage, monitoring, deployment, rollback, backup/restore, DR input, signing, dan distribution handover  
**Status:** Pre-implementation handover design; tidak membuat akses, account, identity, secret, infrastructure, atau deployment  
**Authoritative baseline:** Document 01–17 FINAL; Document 06–09, 15–17 menjadi fokus utama

> **Source limitation:** Master Document 18 FINAL belum tersedia di workspace saat laporan ini dibuat. Empat laporan specialist Document 18 telah dibaca sebagai sumber interim. Semua ownership/topology/interface edge yang hanya berasal dari laporan specialist tetap `PARTIAL`/`REQUIRES_CONFIRMATION` sampai master Document 18 FINAL tersedia.

## Safety declaration

Laporan ini **tidak memuat dan tidak meminta** password, token, API key, private/signing key, connection secret, database credential, Production credential, connection string berisi rahasia, recovery code, certificate material, atau nilai secret apa pun. Identifier `SECREF-*`, `SID-*`, dan placeholder `<..._TBD>` adalah ID register konseptual—bukan credential, account, endpoint, atau secret-store item nyata.

Status yang digunakan:

| Status | Arti |
|---|---|
| `DESIGN_DEFINED` | Struktur/guard handover sudah dirancang, tetapi belum membuktikan resource nyata |
| `REQUIRES_CONFIRMATION` | Membutuhkan keputusan/owner/authority resmi Pertamina |
| `NOT_PROVISIONED` | Resource/identity/environment belum dibuktikan tersedia |
| `NOT_EVIDENCED` | Mungkin ada, tetapi tidak ada bukti yang diperiksa |
| `REHEARSAL_REQUIRED` | Harus diuji pada QA/isolated environment sebelum acceptance |
| `VERIFIED` | Hanya boleh dipakai setelah bukti aktual, reviewer, waktu, scope, dan hasil tersedia; tidak ada item yang dinyatakan VERIFIED oleh laporan ini |

---

# 1. Environment register

## 1.1 Environment identity, topology, and data

| Environment ID | Environment | Purpose | Logical topology | Physical location placeholder | Data classification | Deployables | Database | Storage |
|---|---|---|---|---|---|---|---|---|
| `ENV-LOCAL-DEV` | Local Development | Developer feedback; Unit/component/contract; local processing/IPC | Developer device → Desktop/Mobile dev host → per-profile SQLite/local workspace; optional isolated local API dependencies only when task requires | `<APPROVED_DEVELOPER_DEVICE_TBD>` | Synthetic only; local restricted test fixture if explicitly approved | Task/local builds for feedback; Desktop + Local Worker; optional app hosts | Per-profile SQLite; isolated developer SQL only if integration task requires | Isolated local workspace/temp; no Production object store |
| `ENV-SHARED-DEV` | Shared Development | Restricted shared integration and smoke | Separate logical Web/API/Worker/SQL/storage/queue/monitoring processes; co-hosting allowed only as temporary logical separation | `<APPROVED_SHARED_DEV_LOCATION_TBD>`; Hostinger KVM 2 may be temporary after approval | Synthetic | Exact pipeline artifacts for Web/API/Worker; approved client build | Dev-only SQL Server logical database/schemas | Dev-only private storage zones; no Production data |
| `ENV-QA` | QA/Testing | Integration, Contract, E2E, Security, migration/restore, Worker/IPC, bounded load/fault/regression | Isolated QA logical deployables/dependencies with separate identities/config/data/telemetry | `<APPROVED_QA_LOCATION_TBD>`; Hostinger KVM 2 may support limited QA after approval | Synthetic or sanitized-approved test data | Same immutable pipeline artifact set under test | QA-only SQL Server + migration target; SQLite/device fixtures | QA-only processing/secured/Issue/export zones and recovery fixtures |
| `ENV-PILOT` | Pilot | Controlled operational validation of exact RC | Exact RC across logical Web/API/Worker/data/monitoring and approved participant clients | `<APPROVED_PILOT_LOCATION_TBD>`; Hostinger KVM 2 may be temporary limited Pilot after approval | Approved Pilot data/classification only | Exact RC artifacts/digests; no rebuild | Pilot-only SQL Server/data boundary | Pilot-only private zones; separate backup target required |
| `ENV-PROD` | Production | Official governed operation | Logical Web/API/SignalR → application/Worker → private SQL/storage/queue/Audit/monitoring; exact physical topology TBD | `<PERTAMINA_APPROVED_PRODUCTION_INFRASTRUCTURE_TBD>` | Governed operational data | Only approved same pipeline RC artifacts/digests | Production SQL Server topology TBD; private, module-owned schemas | Production private storage/backup topology TBD; Hostinger KVM 2 prohibited as baseline |

## 1.2 Operational environment handover

| Environment ID | Configuration source | Secret source | Pipeline | Monitoring | Backup | Access owner | Support owner | Release version | Status | Confirmation gap |
|---|---|---|---|---|---|---|---|---|---|---|
| `ENV-LOCAL-DEV` | Safe defaults + developer-safe typed config | Approved developer credential boundary; reference only | Local/task validation plus official pipeline for shared artifact | Safe local/basic instrumentation | Fixture/recreate; protected local migration/checkpoint concept | `TBD_FUNCTION: Development Owner` | `TBD_FUNCTION: Development Support` | No release/RC evidenced | `NOT_EVIDENCED` | Device baseline, developer access, approved store, test data policy |
| `ENV-SHARED-DEV` | Versioned Dev environment config | Separate non-Prod mechanism TBD | Azure DevOps baseline; exact pipeline/resource ID TBD | Dev stream/component health; location TBD | Change recovery point; policy TBD | `TBD_FUNCTION: Shared Dev Environment Owner` | `TBD_FUNCTION: Development Operations` | No deployed BuildVersion evidenced | `NOT_PROVISIONED` | Infrastructure/network/identity/config/monitoring ownership |
| `ENV-QA` | Versioned QA config + frozen test manifest | Separate QA mechanism TBD | Same pipeline artifact; QA stage/approval TBD | Full test observability/evidence stream | Restore rehearsal set required | `TBD_FUNCTION: QA Environment Owner` | `TBD_FUNCTION: QA Support/Incident Owner` | No immutable RC evidenced | `REHEARSAL_REQUIRED` | Exact QA topology, pipeline, owner, backup/restore rehearsal, access |
| `ENV-PILOT` | Frozen/versioned Pilot config | Separate Pilot mechanism TBD | Exact RC promotion after approval | Operational alert rehearsal + Pilot observation | Verified backup/restore set required before Pilot acceptance | `TBD_FUNCTION: Pilot Deployment Owner` | `TBD_FUNCTION: Pilot Support Owner` | No RC/UAT sign-off evidenced | `REQUIRES_CONFIRMATION` | Participants/data/topology/capacity/authority/monitoring/backup |
| `ENV-PROD` | Controlled Production config/control plane | Approved Production mechanism TBD; never documented here | Exact approved RC promotion only | Active approved sink/alerts before rollout | Policy, recovery point, restore test, DR inputs required | `TBD_FUNCTION: Production Authority` | `TBD_FUNCTION: Production Support/Incident Owner` | None; actual Production release not evidenced | `REQUIRES_CONFIRMATION` | Official topology, authority, SSO/network/security, signing, backup/DR, capacity, release evidence |

Environment separation is not a label. Endpoint, identity, permission, SQL/storage/queue/provider, config, secret reference, telemetry, backup, deployment authority, and data must be separate. Hostinger KVM 2 remains `SINGLE_NODE_TEMPORARY` for approved Dev/limited QA/Pilot; it is not Production, HA proof, or sole backup location.

---

# 2. Infrastructure handover

## 2.1 Logical infrastructure inventory

| Infrastructure ID | Component | Purpose/zone | Public exposure | Dependencies | Health/evidence | Functional owner | Status/gap |
|---|---|---|---|---|---|---|---|
| `INF-WEB` | Web host | User-facing Web and optional protected server-session boundary | Approved HTTPS only | SSO/session, API, config, telemetry | Own liveness/readiness, exact artifact/config/version | `TBD_FUNCTION: Web Owner` | `DESIGN_DEFINED`; hosting/proxy/CDN TBD |
| `INF-API` | ASP.NET Core API | REST authority/admission/authorization | Approved HTTPS `/api/v1`, `/api/compatibility`, minimal health | Identity, config, SQL, storage, Job queue, Audit, telemetry | `/health/live`, `/health/ready`, safe dependency readiness | `TBD_FUNCTION: API Owner` | `DESIGN_DEFINED`; physical topology TBD |
| `INF-SIGNALR` | Activity Hub | Authenticated post-commit notification | Approved `/hubs/activity`; no command | API auth/session, owner outbox, persisted state | Connect/reconnect/gap/REST reconciliation | `TBD_FUNCTION: Realtime Owner` | `DESIGN_DEFINED`; proxy/scale TBD |
| `INF-WORKER` | Server Worker | Bounded server background execution | None; restricted internal boundary | Job store/queue, SQL, storage, engines/providers, config, telemetry | Readiness, claim/drain/lease/checkpoint/resource | `TBD_FUNCTION: Processing Operations` | `DESIGN_DEFINED`; host/scale/concurrency TBD |
| `INF-SQL` | SQL Server | Module-owned server metadata | None to clients; private only | Network/time/identity/config/custody reference/backup | Connection/readiness, DB/log/pool/lock/capacity, recovery point | `TBD_FUNCTION: Database Owner/DBA` | Product/topology/HA/RPO/RTO TBD |
| `INF-STORAGE` | Secure File/Object Storage | Private purpose-separated payload zones | No public listing/direct client authority | Identity, encryption custody, SQL metadata, lifecycle, backup | Availability/capacity/digest/orphan/restore | `TBD_FUNCTION: Storage Owner` | Product/topology/key custody TBD |
| `INF-QUEUE` | Persisted Job Store/Queue boundary | Durable dispatch/admission/lease | None | API, SQL/queue implementation, Worker, monitoring | Depth/age/claim/drain/retry/lease | `TBD_FUNCTION: Jobs/Queue Owner` | Technology and topology TBD; external broker not assumed |
| `INF-AUDIT` | Audit ingestion/store/export | Append-oriented controlled evidence | Permission-scoped API only | Owner outboxes, SQL, storage for exports, monitoring | Outbox/backlog/schema/sequence/freshness/export | `TBD_FUNCTION: Audit Owner` | Degraded policy/retention/hold/topology TBD |
| `INF-OBS` | Monitoring/Telemetry | Technical health, detection, dashboards/alerts | Restricted operations access | All components, exporter/sink, alert route | Freshness, ingest, dashboard/alert delivery | `TBD_FUNCTION: Observability Owner` | Backend/topology/retention/thresholds TBD |
| `INF-SSO` | Pertamina Identity integration | User authentication | Approved enterprise route only | Network/time/client registration/API validation | Safe auth outcome, revocation/session health | `TBD_FUNCTION: IAM Integration Owner` | `REQUIRES_PERTAMINA_SSO_CONFIRMATION` |
| `INF-CONFIG` | Typed configuration/control plane | Environment config, policy, flags, limits, compatibility | Admin API only where Document 06 defines route | Identity/AuthZ, SQL, Audit, deployment config | Version/provenance/validation/rollback | `TBD_FUNCTION: Configuration Owner` | Store/exact admin surfaces TBD |
| `INF-SECRET` | Approved custody/reference mechanism | Runtime secret/certificate reference resolution | No general user access | Enterprise security/identity; all service consumers | Reference access/rotation/revocation/failure | `TBD_FUNCTION: Security Custodian` | Product/process/owner TBD; no values in handover |
| `INF-BACKUP` | Backup/restore boundary | SQL/storage/config/artifact recovery sets | Restricted operations only | Data stores, custody metadata, isolated restore target | Job status/integrity/retention/restore test | `TBD_FUNCTION: Backup/Recovery Owner` | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| `INF-AZDO` | Azure DevOps Repo/Pipeline/Artifacts/Feed | Source, build, evidence, immutable artifacts | Restricted enterprise access | Identity/access policies, approved agent/feed/scans | PR/build/gates/digests/retention/promotion | `TBD_FUNCTION: DevOps Owner` | Org/project/resource/approver/retention TBD |

## 2.2 Network and placement handover boundary

Client-facing surface is limited to approved Web/API/SignalR/compatibility/minimal health. Worker, SQL, storage, queue, migration, backup, detailed monitoring/health, admin operations, and provider interfaces remain restricted/internal. The receiving team must populate an approved infrastructure inventory outside this public document with environment ID, asset ID, hosting product, physical region/location, DNS/certificate reference, network zone, ingress/egress rule reference, service identity reference, configuration version, health endpoint class, capacity profile, backup class, owner, runbook, and lifecycle status—without copying credentials.

## 2.3 Handover minimum

- exact asset and environment inventory approved;
- logical deployable-to-physical placement mapping;
- network/data-flow/firewall rule references without secret values;
- owner, deputy/escalation, maintenance, patching, capacity, and decommission responsibility;
- health/readiness and monitoring path;
- dependency/startup/recovery order;
- backup/restore/rollback references;
- infrastructure/security review and latest evidence date.

---

# 3. Access register

No access below is asserted to exist. `Current Owner = NOT_EVIDENCED` means no current named assignment was supplied. Target owner/approver are functions to be confirmed, not invented people or application roles.

| Access ID | System | Environment | Permission/role scope | Current owner | Target owner | Approver | Grant/revoke process | Reauth/MFA | Evidence required | Status/gap |
|---|---|---|---|---|---|---|---|---|---|---|
| `ACC-REPO` | Azure DevOps Repo | All source scopes | Read/contribute/PR policy administration separated | `NOT_EVIDENCED` | Repository Owner function TBD | DevOps governance TBD | Approved request → least scope → expiry/review; revoke on task/role end | Enterprise policy; policy admin high-impact | Grant record, branch policy, access review, revoke test | `REQUIRES_CONFIRMATION` |
| `ACC-PIPELINE` | Azure Pipeline | Dev/QA/Pilot/Prod stages | View/run/edit/approve/service-connection use separated | `NOT_EVIDENCED` | Pipeline Owner TBD | Release authority TBD | Environment-scoped grant; no inherited Production access | Required for privileged change/approval per policy | Pipeline permissions, approver record, audit history | `REQUIRES_CONFIRMATION` |
| `ACC-ARTIFACT` | Artifact Storage | All | Publish/read/promote/retain/delete separated | `NOT_EVIDENCED` | Artifact Custodian TBD | Release/Security authority TBD | Restricted immutable publication; revoke/delete highly controlled | Policy/high-impact | Artifact ACL, digest, retention, promotion/delete evidence | `REQUIRES_CONFIRMATION` |
| `ACC-FEED` | Package Feed | Build | Restore/publish/approve source separated | `NOT_EVIDENCED` | Dependency/Feed Owner TBD | DevOps/Security review TBD | Approved feed/source only; revoke compromised publisher | Enterprise policy | Feed policy, package provenance, publisher review | `REQUIRES_CONFIRMATION` |
| `ACC-ENV-DEV` | Shared Dev environment | Dev | Deployment/operations within Dev only | `NOT_EVIDENCED` | Dev Environment Owner TBD | DevOps/Environment authority TBD | Ticket/task-bound grant; no Production inheritance | Policy | Grant, environment scope, activity review | `NOT_PROVISIONED` |
| `ACC-ENV-QA` | QA environment | QA | Deploy/test/observe; destructive tests separately scoped | `NOT_EVIDENCED` | QA Environment Owner TBD | QA/Security owner TBD | Test-window and dataset scoped; revoke after campaign | Policy; high-risk test action as required | Approval, scope, test window, cleanup evidence | `REHEARSAL_REQUIRED` |
| `ACC-ENV-PILOT` | Pilot environment | Pilot | Approved rollout/observe/support scope | `NOT_EVIDENCED` | Pilot Owner TBD | Pilot authority TBD | Participant/change-window scoped | Required per high-impact policy | Grant, cohort/scope, monitoring, revoke evidence | `REQUIRES_CONFIRMATION` |
| `ACC-ENV-PROD` | Production environment | Production | Deploy/operate/approve separated; no developer implicit access | `NOT_EVIDENCED` | Production Authority TBD | Official Pertamina approver TBD | Formal change/access request, time-bound/JIT if approved, periodic review | Required according to Pertamina policy | Exact approval, grant/expiry, session/activity review, revocation | `REQUIRES_PERTAMINA_RELEASE_APPROVAL_CONFIRMATION` |
| `ACC-SQL` | SQL Server | Per environment | Runtime schema, migration, backup, read/monitor separated | `NOT_EVIDENCED` | DBA/Data Owner functions TBD | Infrastructure/Security/Data authority TBD | DB role mapped to purpose/environment; no client grant | Required for privileged DBA/migration/restore | Role grants, schema permissions, login audit, revoke test | `REQUIRES_CONFIRMATION` |
| `ACC-STORAGE` | Secure Storage | Per environment/zone | Exact object read/write/lifecycle by workload; no browsing grant | `NOT_EVIDENCED` | Storage Owner TBD | Security/Data authority TBD | Purpose/object/zone-scoped workload access | Privileged human access per policy | Policy/ACL, access test, denied cross-scope test | `REQUIRES_CONFIRMATION` |
| `ACC-CUSTODY` | Secret/certificate custody store | Per environment | Resolve/manage/rotate/revoke separated | `NOT_EVIDENCED` | Security Custodian TBD | Security authority TBD | Approved request; identity/purpose bound; no value in ticket/evidence | Required | Reference ACL, rotation/revoke evidence, access log | `REQUIRES_CONFIRMATION` |
| `ACC-MON` | Monitoring | Per environment | Safe dashboard/alert config/read separated | `NOT_EVIDENCED` | Observability Owner TBD | Operations/Security authority TBD | Environment and safe-field scope; no raw content | Policy | Dashboard ACL, alert-change audit, data-minimization test | `REQUIRES_CONFIRMATION` |
| `ACC-AUDIT` | Audit Console | Server environments | `audit.event.list/read`, `audit.export.create/read` in `AUDIT_SCOPE` | `NOT_EVIDENCED` | Audit Owner/Auditor function TBD | Audit governance TBD | Explicit grant, scope before query/export, periodic review | High-impact export per policy | Grant/scope, query/export Audit, revocation test | `REQUIRES_CONFIRMATION` |
| `ACC-SUPPORT` | Support Console | Server environments | `support.*` exact Issue/attachment/note/export scope | `NOT_EVIDENCED` | Support Owner TBD | Support/Security authority TBD | Queue/unit/assignment scoped; no general document access | High-impact attachment/export per policy | Grant/scope, access Audit, revoke test | `REQUIRES_CONFIRMATION` |
| `ACC-SSO-REG` | SSO Registration | Per client/environment | Register/read/update redirect/client metadata; no token value | `NOT_EVIDENCED` | IAM Integration Owner TBD | Pertamina IAM authority TBD | Official IAM request/change/revoke process | Required by IAM policy | Registration reference, allowed redirect/origin review, revoke test | `REQUIRES_PERTAMINA_SSO_CONFIRMATION` |
| `ACC-DESKTOP-SIGN` | Desktop signing service | Build/release | Submit approved package for signing; custody admin separate | `NOT_EVIDENCED` | Code-signing Custodian TBD | Security/Release authority TBD | Pipeline identity only; no developer material access | Required for custody admin | Signing receipt, publisher verification, revocation/renewal process | `REQUIRES_PERTAMINA_CODE_SIGNING_CONFIRMATION` |
| `ACC-MOBILE-SIGN` | Mobile signing service/profile | Build/release | Platform package signing; team/profile admin separate | `NOT_EVIDENCED` | Mobile Signing Custodian TBD | Mobile/Security authority TBD | Controlled build identity; no prompt/workstation material | Required | Signing/profile receipt, package verification, revocation process | `REQUIRES_PERTAMINA_MOBILE_DISTRIBUTION_CONFIRMATION` |
| `ACC-DISTRIBUTION` | Desktop/Mobile distribution channel | Pilot/Prod | Publish/ring/withdraw/read separated by platform | `NOT_EVIDENCED` | Distribution Owner TBD | Release/Mobile/Desktop authority TBD | Approved release/digest/channel/ring only | Required for publish/withdraw | Channel approval, exact version/digest, rollout/withdraw evidence | `REQUIRES_CONFIRMATION` |
| `ACC-BACKUP` | Backup system | Per environment | Schedule/status/read set; delete/retention separated | `NOT_EVIDENCED` | Backup Owner TBD | Data/Infrastructure authority TBD | Policy and environment scoped | Required for lifecycle/admin | Job status, immutable set manifest, ACL/retention review | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| `ACC-RESTORE` | Restore operation | Isolated target/Production by approval | Select exact set, restore, validate; no ordinary standing grant | `NOT_EVIDENCED` | Recovery Owner TBD | Incident/Data/Production authority TBD | Incident/change-bound temporary authority | Required | Approval, selected manifest, isolated restore, validation, revocation | `REHEARSAL_REQUIRED` |
| `ACC-SERVICE-DESK` | Service Desk/Incident system | All support scopes | Create/read/update exact ticket/Incident by assignment | `NOT_EVIDENCED` | Service Desk Owner TBD | Support/Incident authority TBD | Organization workflow; content-minimized | Policy | Assignment, access log, closure/revoke review | `REQUIRES_PERTAMINA_ORGANIZATION_CONFIRMATION` |

Access IDs are handover record identifiers, not usernames/groups. The receiving authority must replace `TBD` only in a restricted access-management system and link the approved record; do not paste account names or credential material into this report.

---

# 4. Service-identity register

All entries are conceptual workload identities. None is an actual account or authorization grant.

| Service Identity ID | Purpose | Least privilege | Environment | Custody/reference field | Owner | Rotation/revocation | Access path | Audit/evidence | Status |
|---|---|---|---|---|---|---|---|---|---|
| `SID-WEB` | Web host/session boundary where applicable | Invoke approved API/session services; no DB/storage/provider broad access | Separate per env | `SECREF-WEB-SESSION` record; actual store item not documented | Web + Security owners TBD | Per approved session/custody policy | Runtime injection/managed mechanism TBD | Startup/access/change evidence | `NOT_PROVISIONED` |
| `SID-API` | API runtime | Own module DB operations, authorized Job/storage/Audit intents only | Separate per env | `SECREF-API-RUNTIME` | API + Security owners TBD | Policy-based rotate/revoke; environment isolated | Approved runtime resolution | Auth/service access, config version, denied over-scope test | `NOT_PROVISIONED` |
| `SID-WORKER` | Claim and execute exact server Jobs | `jobs.worker.claim/update`, exact storage/provider/job scope; no human permission/token | Separate per env/workload class as approved | `SECREF-WORKER-RUNTIME` | Processing + Security owners TBD | Revoke on compromise/redeploy; no user token | Workload identity mechanism TBD | Job/lease/attempt/provider/storage access evidence | `NOT_PROVISIONED` |
| `SID-MIGRATION` | Apply reviewed SQL migration bundle | Exact owned schemas/migration history; no app runtime/content browse | Separate per env; Production temporary/controlled | `SECREF-MIGRATION-RUNTIME` | DBA/Deployment owners TBD | Time-bound/JIT and revoked after run where supported | Approved deployment pipeline only | Migration approval, package digest, DB permission/session evidence | `NOT_PROVISIONED` |
| `SID-STORAGE` | Storage adapter/object lifecycle | Exact environment/zone/object purpose read/write/lifecycle | Separate per env/zone | `SECREF-STORAGE-RUNTIME` | Storage + Security owners TBD | Rotate/revoke per platform policy | Workload identity through adapter | Access test, cross-zone denial, lifecycle evidence | `NOT_PROVISIONED` |
| `SID-MONITORING` | Export safe logs/metrics/traces | Append/export safe telemetry; no business DB/content read | Separate per env | `SECREF-MONITORING-RUNTIME` | Observability + Security owners TBD | Policy and sink change lifecycle | Approved exporter/collector | Exporter health, field-canary, config/access evidence | `NOT_PROVISIONED` |
| `SID-PIPELINE` | Build/publish/promote/deploy by environment | Task-specific repo/feed/artifact/environment permission; no blanket Production admin | Build + separate env service connections | `SECREF-PIPELINE-CONNECTION` | DevOps + Release owners TBD | Rotate/revoke on scope/agent/change; review regularly | Azure DevOps controlled service connection | Pipeline audit, artifact digest, approval, access review | `NOT_PROVISIONED` |
| `SID-DESKTOP-SIGNING` | Request Desktop package signing | Submit exact approved digest; no custody material retrieval | Build/release boundary | `SECREF-DESKTOP-SIGNING-CUSTODY` | Signing Custodian TBD | Certificate/profile lifecycle by authority | Approved signing service from pipeline | Receipt, publisher/timestamp/trust verification | `NOT_PROVISIONED` |
| `SID-MOBILE-SIGNING` | Request Mobile package signing | Submit exact platform package; no custody material retrieval | Build/release boundary | `SECREF-MOBILE-SIGNING-CUSTODY` | Mobile Signing Custodian TBD | Platform/profile lifecycle | Approved signing service from pipeline | Receipt, app/team identity verification | `NOT_PROVISIONED` |
| `SID-AUDIT-INGEST` | Append validated Audit events | `audit.event.ingest` for validated source/batch only | Separate per env | `SECREF-AUDIT-INGEST` | Audit + Security owners TBD | Revoke/rotate per workload policy | Internal ingestion boundary | Batch/source/auth/append/access evidence | `NOT_PROVISIONED` |
| `SID-BACKUP-RESTORE` | Backup and approved restore | Exact backup sets/targets; backup and restore authority separated where required | Separate per env/operation | `SECREF-BACKUP-RECOVERY` | Backup/Recovery + Security owners TBD | Time-bound restore authority; regular access review | Approved backup platform/pipeline | Job, set manifest, restore session, revocation evidence | `NOT_PROVISIONED` |

Human identities, application roles, and workload identities are never interchangeable. Workload identity has no User/Admin/Auditor/Support role and does not inherit end-user content authority.

---

# 5. Configuration register

## 5.1 Configuration principles

Priority remains System Safety/Security → Enterprise Policy → Environment Configuration → Device Configuration → User Preference. Most restrictive safe result wins. Configuration may select approved references/profiles and bounded values; it may not redefine Feature IDs, business meaning, permission, ownership, Audit, Local-First, max-three secured versions, non-PKI scope, or public-AI prohibition.

Logical `Key Name` below is a handover name proposal, not authorization to add an application key. Exact implementation name/schema must come from the owner contract and configuration catalog.

## 5.2 Register

| Configuration ID | Component / key name | Purpose | Environment/type/default | Sensitive? / secret reference? | Validation | Owner/change/Audit | Restart/rollback | Status |
|---|---|---|---|---|---|---|---|---|
| `CFG-ENV` | Platform / `EnvironmentCode` | Bind runtime/artifact/config to one approved environment | All; enum; no Production fallback | No / No | Allowed environment; artifact/environment match | Configuration Owner; pipeline-reviewed; Audit if privileged change | Restart/redeploy by component; revert previous version | `DESIGN_DEFINED` |
| `CFG-RELEASE` | All / `ReleaseManifestRef` | Bind Product/Build/Commit/artifact digests and compatibility | All; opaque reference; no default | Restricted metadata / No | Manifest integrity, build/commit/digests | Release Owner; pipeline only; release evidence | Redeploy exact prior compatible artifact | `DESIGN_DEFINED` |
| `CFG-SCHEMA` | All / `ConfigurationSchemaVersion` | Validate typed configuration format | All; version; pinned by artifact | No / No | Supported schema range; unknown fails safely | Configuration + component owner; contract review | Component-specific; rollback only compatible version | `DESIGN_DEFINED` |
| `CFG-API-BASE` | Web/Desktop/Mobile / `ApiBaseAddress` | Approved API destination per environment | Environment URI; no arbitrary/document-provided value | Integrity-sensitive / No | HTTPS, allowed environment/host, full TLS | Network/Configuration owner; controlled deployment; change evidence | Client reload/update; previous approved endpoint/config | `REQUIRES_CONFIRMATION` |
| `CFG-COMPAT` | API/Clients / `CompatibilityPolicyVersion` | Current/minimum client and supported contract window | Environment typed version; no unsafe default | No / No | Version ordering, platform/channel, rollback path | Configuration owner; high-impact approval/Audit | Dynamic or restart TBD; previous policy after validation | `REQUIRES_CONFIRMATION` |
| `CFG-CAPABILITY` | Registry/Clients/Resolver / `CapabilityVersion` | Bind canonical Feature/exposure/capability snapshot | Environment version; fail unavailable if unknown | No / No | Registry count/IDs/schema/handler/platform | Feature/Configuration owner; audited if exposure changes | Cache refresh; rollback verified snapshot | `DESIGN_DEFINED` |
| `CFG-FEATURE-FLAG` | Configuration / `FeatureExposureSnapshotVersion` | Approved rollout/containment only | Environment/scope typed; safe disabled until approved | No / No | Existing Feature only; owner/scope/expiry; hard invariants | `configuration.feature.update`; required Audit | Dynamic if supported; disable/revert prior version | `DESIGN_DEFINED` |
| `CFG-JOB-ADMISSION` | API/Worker / `JobAdmissionPolicyVersion` | Typed quota/concurrency/workload/admission profile | Environment policy reference; no invented numeric default | No / No | Within system safety/capacity ceiling | Limit/Processing owner; audited change | Dynamic/restart per implementation; previous approved profile | `REQUIRES_CONFIRMATION` |
| `CFG-FILE-LIMIT` | API/Upload/Storage / `FileTransferLimitPolicyVersion` | Size/count/chunk/batch/retention effective policy | Environment/Feature policy reference | No / No | Strictest safety/proxy/policy/quota/capacity value | Configuration/Storage/Security owners; audited change | Dynamic; rollback prior safe profile | `REQUIRES_CONFIRMATION` |
| `CFG-SQL-REF` | API/Worker/Migration / `SqlConnectionReference` | Point to approved environment-specific runtime connection material | Reference type; no value/default | Yes / `SECREF-API-RUNTIME`, `SECREF-WORKER-RUNTIME`, or migration record | Reference exists, audience/environment/least privilege, TLS readiness | Security/DB owner; custody process; no value in Audit | Restart/redeploy usually required; restore prior valid reference | `REQUIRES_CONFIRMATION` |
| `CFG-STORAGE-REF` | API/Worker/Storage / `StorageProfileReference` | Select approved private storage profile/zones | Reference type; no public URL/default | Yes / `SECREF-STORAGE-RUNTIME` | Environment/zone/purpose/encryption/capacity/health | Storage/Security owner; controlled change/Audit | Component refresh/restart TBD; previous profile only if data compatible | `REQUIRES_CONFIRMATION` |
| `CFG-SSO-REF` | Web/API/Clients / `SsoRegistrationReference` | Bind approved client/issuer/audience/redirect profile | Reference type; no actual endpoint/account in this report | Yes / `SECREF-SSO-REGISTRATION` | Registration/environment/client/platform compatibility | IAM/Security owner; official SSO process | Restart/client update possibly; prior approved registration | `REQUIRES_PERTAMINA_SSO_CONFIRMATION` |
| `CFG-TELEMETRY` | All / `TelemetryProfileReference` | Select safe sink/export/filter/sampling profile | Environment reference; safe local disabled/degraded default policy TBD | Potentially sensitive / `SECREF-MONITORING-RUNTIME` if needed | Environment, field allowlist, endpoint trust, buffer bounds | Observability/Security owner; audited privileged changes | Reload/restart TBD; previous safe exporter profile | `REQUIRES_CONFIRMATION` |
| `CFG-AUDIT` | Owner modules/Audit / `AuditPolicyVersion` | Event catalog/level/retention/degraded-mode reference | Environment policy version; no client downgrade | No secret / No | Event/version/category/required durability and size bounds | Audit owner; policy change controlled/Audited | Dynamic/versioned; rollback must preserve append/read compatibility | `REQUIRES_CONFIRMATION` |
| `CFG-PROVIDER` | Resolver/Worker / `ApprovedProviderProfileReference` | Select approved engine/internal provider profile | Feature/environment reference; no public fallback/default | Sensitive metadata / `SECREF-PROVIDER-RUNTIME` when service access needed | Approved supplier/version/license/egress/data policy/health/resource/PoC | Module/Security/Dependency owner; change + Audit/Release evidence | Drain/restart depending provider; approved compatible fallback only | `REQUIRES_CONFIRMATION` |
| `CFG-LOCAL-SCHEMA` | Desktop/Mobile / `LocalSchemaVersion` | Bind package to supported SQLite schema/migrations | Artifact version; pinned | No / No | Supported from/to app range; integrity/profile/storage preflight | Client/Workspace owner; package review | App restart/migration; no unsupported downgrade/reset | `DESIGN_DEFINED` |
| `CFG-IPC` | Desktop/Local Worker / `IpcContractVersion` | Desktop–Worker handshake compatibility | Artifact version; pinned cohort | No / No | App/Worker/handler/engine/work-root/checkpoint range | Desktop/Processing owner; contract/release evidence | Cohort update/rollback only when local schema/state compatible | `DESIGN_DEFINED` |
| `CFG-HEALTH` | Hosts/Monitoring / `HealthExposureProfile` | Minimal public and detailed internal health behavior | Environment typed profile; minimum public detail | Security-sensitive metadata / No | No topology/secret/dependency detail publicly | Operations/Security owner; controlled change | Reload/restart TBD; previous safe profile | `REQUIRES_CONFIRMATION` |
| `CFG-SUPPORT` | Issue/Support / `SupportPolicyVersion` | Typed status/scope/attachment/export limits and retention class | Environment policy version | Restricted metadata / No | Document 06/09/10 contract, hard content exclusions | Support/Security owner; audited high-impact changes | Dynamic/versioned; prior compatible policy | `REQUIRES_CONFIRMATION` |

No default may silently point a non-Production environment to Production. Missing environment-bound reference causes not-ready/fail-safe, not hardcoded fallback.

---

# 6. Secret-reference inventory

The `Approved Store` column intentionally contains only a store-class placeholder. The actual item identifier must live in the restricted custody/configuration system, not this report.

| Secret-reference ID | Purpose | Owner | Approved store | Consumer | Rotation/revocation | Access process | Status |
|---|---|---|---|---|---|---|---|
| `SECREF-WEB-SESSION` | Web session protection reference | Security + Web owner TBD | `<APPROVED_CUSTODY_STORE_TBD>` | `SID-WEB` | Pertamina policy; rotate on compromise/config lifecycle | Workload-bound runtime resolution | `NOT_PROVISIONED` |
| `SECREF-API-RUNTIME` | API database/service runtime references | Security + API/DB owners TBD | `<APPROVED_CUSTODY_STORE_TBD>` | `SID-API` | Environment-specific rotation/revoke | Least-privilege workload access | `NOT_PROVISIONED` |
| `SECREF-WORKER-RUNTIME` | Worker SQL/storage/queue runtime references | Security + Processing owners TBD | `<APPROVED_CUSTODY_STORE_TBD>` | `SID-WORKER` | Revoke/rotate by dependency or compromise | Exact workload/environment access | `NOT_PROVISIONED` |
| `SECREF-MIGRATION-RUNTIME` | Temporary migration access reference | DBA + Security + Deployment owners TBD | `<APPROVED_CUSTODY_STORE_TBD>` | `SID-MIGRATION` | Time-bound/JIT where supported; revoke after run | Approved pipeline/change only | `NOT_PROVISIONED` |
| `SECREF-STORAGE-RUNTIME` | Private object-storage access reference | Storage + Security owners TBD | `<APPROVED_CUSTODY_STORE_TBD>` | Storage adapter/Worker identities | Zone/environment scoped rotation | Workload access through adapter | `NOT_PROVISIONED` |
| `SECREF-SSO-REGISTRATION` | Approved SSO client/session registration reference | IAM + Security owners TBD | `<APPROVED_IAM_CUSTODY_TBD>` | Web/API/native clients as applicable | IAM lifecycle/revocation | Official Pertamina IAM process | `REQUIRES_PERTAMINA_SSO_CONFIRMATION` |
| `SECREF-PROVIDER-RUNTIME` | Approved enterprise-internal provider service reference | Provider + Security owners TBD | `<APPROVED_CUSTODY_STORE_TBD>` | Approved provider adapter identity | Provider/contract lifecycle | Purpose/audience-scoped workload access | `NOT_PROVISIONED` |
| `SECREF-MONITORING-RUNTIME` | Telemetry exporter/sink access reference | Observability + Security owners TBD | `<APPROVED_CUSTODY_STORE_TBD>` | `SID-MONITORING` | Sink/exporter lifecycle | Append/export-only workload path | `NOT_PROVISIONED` |
| `SECREF-AUDIT-INGEST` | Audit ingestion workload reference | Audit + Security owners TBD | `<APPROVED_CUSTODY_STORE_TBD>` | `SID-AUDIT-INGEST` | Source/workload lifecycle | Validated ingestion service only | `NOT_PROVISIONED` |
| `SECREF-PIPELINE-CONNECTION` | Environment-specific deployment/service-connection reference | DevOps + Security/Release owners TBD | `<AZURE_DEVOPS_PROTECTED_CONNECTION_TBD>` | `SID-PIPELINE` | Access review/rotation/revoke per environment | Protected pipeline only; approval gates | `NOT_PROVISIONED` |
| `SECREF-DESKTOP-SIGNING-CUSTODY` | Desktop signing-service custody reference | Code-signing Custodian TBD | `<APPROVED_SIGNING_SERVICE_TBD>` | `SID-DESKTOP-SIGNING` | Certificate/profile renewal/revocation policy | Approved pipeline submit-only | `REQUIRES_PERTAMINA_CODE_SIGNING_CONFIRMATION` |
| `SECREF-MOBILE-SIGNING-CUSTODY` | Mobile signing-service/profile custody reference | Mobile Signing Custodian TBD | `<APPROVED_MOBILE_SIGNING_SERVICE_TBD>` | `SID-MOBILE-SIGNING` | Platform/team profile lifecycle | Approved pipeline submit-only | `REQUIRES_PERTAMINA_MOBILE_DISTRIBUTION_CONFIRMATION` |
| `SECREF-BACKUP-RECOVERY` | Backup encryption/recovery custody metadata reference | Backup + Security Custodians TBD | `<APPROVED_RECOVERY_CUSTODY_TBD>` | `SID-BACKUP-RESTORE` | Policy/restore drill/compromise lifecycle | Restricted backup/restore process | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |

Rotation evidence records only reference ID, old/new version identifiers, owner/approver, time, consumer validation, revocation outcome, and Incident/change link. It never records material values.

---

# 7. SQL Server handover

## 7.1 Instance/database placeholders

| Field | Handover value/status |
|---|---|
| SQL Server instance identifier | `<SQL_INSTANCE_ID_TBD>` — identifier only, no host/credential invented |
| Database identifier | `<OTO_DATABASE_ID_TBD>` |
| Environment | Must be one exact environment; no shared Production/non-Production database |
| Product/version/edition/topology | `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` |
| Connection security | Encrypted certificate-validated connection; private access; trust bypass prohibited |
| Runtime access | Separate least-privilege identity per environment/purpose/schema |
| Migration access | `SID-MIGRATION`; pipeline/change-bound; not application startup |
| Backup/restore access | Separate governed identity/authority; not implicit DBA business-content permission |
| Migration version | `<DatabaseMigrationVersion_TBD>`; must match Release Manifest |
| Health status | `NOT_EVIDENCED` |

No connection string or credential is part of this handover report.

## 7.2 Schema/DbContext ownership

| Schema | Write/migration owner function | Responsibility | Handover status |
|---|---|---|---|
| `identity` | Account Administration — Identity | Principal/session/revocation metadata | Logical owner defined; named owner/DbContext artifact TBD |
| `admin` | Account Administration — Authorization | Roles/permissions/assignments | Logical owner defined; routes/persistence gaps remain |
| `configuration` | Platform Configuration | Feature snapshot, exposure, flags, policies, compatibility | Logical owner defined |
| `workspace` | Workspace Metadata | Allowlisted account preference/favorite/preset metadata | Logical owner defined |
| `jobs` | Processing/Jobs | Job/upload/attempt/checkpoint/artifact/admission/idempotency/outbox | Logical owner defined |
| `secured` | Secured Files | Explicit secured file/version/recycle metadata | Logical owner defined; canonical schema name |
| `history` | History | Personal server/account projection | Logical owner defined |
| `audit` | Enterprise Audit | Append-oriented event/ingestion/export metadata | Logical owner defined; lifecycle authority TBD |
| `support` | Issue and Support | Issue/attachment metadata/note/triage/export/outbox | Logical owner defined |
| `assistant` | Assistant | Minimal workflow/invocation metadata | Logical owner defined |
| `intelligence` | Content Intelligence | Server intelligence operation/stage metadata | Logical owner defined; module assignment TBD |

Each module DbContext owns its schema, migration assembly/history table, permissions, constraints, indexes, and recovery validation. No cross-module direct write or migration is allowed; cross-module references are logical IDs/contracts, not automatic FK authority.

## 7.3 Database handover checklist

- [ ] Actual instance/database/environment identifier approved.
- [ ] Schema → DbContext → migration assembly/history → functional owner mapped.
- [ ] Runtime, migration, monitoring, backup, restore identities and exact least privileges tested.
- [ ] Current `DatabaseMigrationVersion`, prior supported version, and Release Manifest linked.
- [ ] Clean/prior-version/representative-volume migration tests PASS.
- [ ] No Production startup migration; reviewed migration bundle/digest available.
- [ ] Backup schedule/status/manifest/encryption-custody reference/retention placeholder visible.
- [ ] Isolated restore test completed; schema, constraints, permissions, outbox, idempotency, Audit, Job, and storage reconciliation validated.
- [ ] Retention/archive/cleanup policy and legal/hold boundary approved; no cascade across independent lifecycles.
- [ ] Health/DB/log/pool/lock/I/O/capacity monitoring and runbook linked.
- [ ] Connection TLS/trust, private network access, least privilege, and failed-access evidence validated.
- [ ] Recovery point and rollback/forward-fix disposition approved for each release.

Current aggregate status: `NOT_EVIDENCED` and `REHEARSAL_REQUIRED`.

---

# 8. SQLite/local-state handover

SQLite is per local OS profile + Pertamina subject/device metadata/state. It is **not** the enterprise server database and does not contain the server system of record or file bytes by default.

| Handover item | Required information/guard | Current status |
|---|---|---|
| Local schema version | `LocalSchemaVersion` and supported app range in package/Release Manifest | No actual version evidenced |
| Migration catalog | Ordered IDs/checksums/from-to/supported app; exclusive profile migration lease | Design defined; executable package absent |
| Profile boundary | Close/open per account; never implicit merge across users/devices | Design defined |
| Path policy | App-owned per-profile protected root; physical path not documented as business contract | OS/platform path and ACL TBD |
| Data | Workspace metadata, operations, local Job/attempt/checkpoint, preferences, local history, pending intents, Audit outbox, secured metadata cache | Design defined |
| File bytes | Local workspace artifacts/filesystem; no ordinary SQLite document/signature BLOB | Design defined |
| Encryption/protection | DB, WAL, SHM, migration snapshot/journal, temp, crash/diagnostics, artifacts one protected boundary | Provider/key integration TBD; no cleartext fallback |
| Backup/checkpoint | Protected pre-migration snapshot/journal + artifact integrity manifest; raw DB copy insufficient | Actual mechanism/test absent |
| Recovery | Validate identity/profile/schema/storage/integrity; recover WAL; classify active work; no empty-profile reset | Rehearsal required |
| Local Worker access | Typed/versioned IPC only; no arbitrary SQLite read/write | Design defined; IPC PoC pending |
| Cleanup | State-aware, excludes active/referenced/draft/result/source; secure-delete absolute claim prohibited | Policy/implementation absent |
| Uninstall | Binary removal distinct from local data; present explicit preserve/remove choices; user files/source/output not silently removed | Distribution/UI validation pending |
| Update/rollback | Desktop+Local Worker cohort; migrate forward; older app cannot open unsupported newer schema; prefer forward-fix | Package/compatibility test absent |
| Account/logout/device loss | Credential/grant lifecycle separate; logout does not silently delete source/draft; account data disposition policy TBD | Security policy confirmation required |

SQLite handover acceptance requires a QA device/profile matrix, migration failure at each phase, disk-full/corruption/wrong-account/downgrade tests, artifact reconciliation, install/update/rollback/uninstall behavior, and recovery evidence. Current status: `REHEARSAL_REQUIRED`.

---

# 9. Storage handover

| Storage ID | Store | Owner/purpose | Payload/sensitivity | Encryption/access | Quota/retention/cleanup | Backup/restore | Health/monitoring | Status |
|---|---|---|---|---|---|---|---|---|
| `STOR-LOCAL-WORKSPACE` | Local Workspace | Workspace/client owner; source/draft/working/undo/checkpoint/local result | Restricted user content and confidential metadata | OS/profile protection; user selection; Local Worker exact job refs only | Device/storage policy; state-aware cleanup; no auto-sync or silent source removal | Local protected snapshot/artifact manifest policy TBD; user outputs not treated as cache | Disk headroom, integrity, orphan/temp, profile binding | Design defined; path/protection/backup test TBD |
| `STOR-LOCAL-TEMP` | Local Temporary | Workspace/Processing; staging/intermediate | Restricted transient content | Per-profile protected random names/ACL/reparse defenses | Bounded quota/expiry; not active/referenced; crash residue reconciliation | Not generic backup target | Capacity/cleanup failure/orphan trend | Implementation/rehearsal required |
| `STOR-SERVER-TEMP` | Server Processing Temporary/Result | Jobs/Storage owner; upload/quarantine/input/temp/result/checkpoint | Restricted content | Private purpose-separated zones; workload identity; encryption/digest | Policy-configurable expiry; active lease/ref protection; bounded reconcile | Only required operational recovery scope; not secured-file substitute | Availability/capacity/digest/orphan/cleanup/expiry | Product/topology/retention TBD |
| `STOR-SECURED` | Secure File/Object Storage | Secured Files owner; explicit Amankan ke Akun and versions | Restricted selected payload | Private exact owner/version; encryption/custody reference; no public listing | Quota, retention/recycle/hold, maximum three accessible versions; owner lifecycle cleanup | Operational backup distinct from product secure copy; restore AuthZ/version validation | Availability/capacity/version/digest/object-metadata reconcile | `REQUIRES_CONFIRMATION` |
| `STOR-ISSUE` | Issue Attachment/Support Export Storage | Support owner; explicit attachment and bounded exports | Restricted Issue screenshot/attachment/export | Exact reporter/Support parent scope; private/encrypted | Case/export retention and expiry; explicit-send; orphan cleanup | Policy-defined backup/restore; not general evidence store | Upload/failure/outbox/expiry/access denial | `REQUIRES_CONFIRMATION` |
| `STOR-AUDIT-EXPORT` | Audit Export Storage | Audit owner; bounded authorized evidence export | Controlled Audit evidence | AUDIT_SCOPE/reauth/current authorization; private/encrypted | Short/configurable expiry; hold/source retention separate | Export is derived; Audit Store backup is authoritative | Export Job/status/expiry/download/access | `REQUIRES_CONFIRMATION` |
| `STOR-BACKUP` | Backup Storage | Backup/Recovery owner; operational recovery sets | Encrypted SQL/storage/config/artifact recovery data | Restricted separate identity/custody; not same single node only | Retention/legal/DR policy TBD; deletion controlled | Is the backup target; restore only to authorized isolated target first | Job freshness/integrity/capacity/replication/restore-test status | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| `STOR-ARTIFACT` | Azure DevOps Artifact Storage | DevOps/Release owner; immutable build/release/rollback packages and evidence | Restricted supply-chain artifacts; no user content/secret | Restricted publish/promote/read/delete; strong digest/SBOM | Retention must preserve active and approved rollback artifacts | Repository/artifact recovery policy TBD; never rebuild old source for rollback | Availability/integrity/retention/promotion history | `REQUIRES_CONFIRMATION` |

Physical path/object key, raw delegated URL, and storage credential are never handover fields. The storage register records only opaque asset/zone identifiers and custody/access references in the restricted system.

---

# 10. Pipeline/deployment handover

## 10.1 Deployment handover record

| Field | Required handover value | Current status |
|---|---|---|
| Pipeline location | Azure DevOps organization/project/pipeline definition ID and reviewed YAML commit reference | `NOT_EVIDENCED` |
| Artifact | Release Manifest; Web/API/Worker/Desktop/Local Worker/Mobile/DB bundle/config template/OpenAPI/release notes/SBOM/test-security evidence IDs and digests | No artifact/RC evidenced |
| Environment | Exact `ENV-*` plus approved physical asset inventory | Only logical environment design exists |
| Approval | Human reviewer/release/environment/security/UAT/Production authority references as applicable | Official roles/records TBD |
| Configuration | `ConfigurationSchemaVersion`, environment config version, policy/flag/capability versions | Actual values TBD |
| Secret dependency | `SECREF-*` logical records resolved by approved runtime mechanism; no values | Mechanism/references not provisioned |
| Migration | DatabaseMigrationVersion + migration bundle/digest/owner/recovery point/validation | No executable bundle/evidence |
| Deployment sequence | Network/time/identity/config/custody → SQL/storage/queue/Audit → API/Worker → Web/SignalR → clients; topology-approved adjustment documented | Logical sequence defined; commands/services TBD |
| Health checks | `/health/live`, `/health/ready`, `/api/compatibility`, Worker/SQL/storage/queue/Audit/internal dependency readiness | Actual endpoints/resources not tested |
| Smoke | SSO/AuthZ negative, compatibility, SQL/storage/queue/Worker, Audit, Issue, SignalR+REST, secured-file, representative Core/Merge, Local-First zero-server traffic | Test design exists; execution absent |
| Monitoring | Dashboard/alerts/freshness/owners/runbooks active before rollout | Not provisioned/evidenced |
| Rollback | Retained previous compatible artifact/config, migration/data disposition, feature flag/forward-fix, Incident link | Runbook design exists; rehearsal absent |
| Incident route | Document 16 lifecycle + `RB-BAD-RELEASE-001` and Document 17 deployment runbooks | Owner/contact/tabletop execution incomplete |
| Evidence | Build/commit/digests/tests/scans/UAT/approval/config/migration/backup/health/smoke/monitoring/rollback/attempt outcome | No actual release evidence |

## 10.2 QA deployment rehearsal acceptance

The receiving team must successfully rehearse `DRB-QA-DEPLOY-001` before Production handover can be accepted:

1. Freeze exact QA environment/asset inventory, data, access window, owner, stop condition, and cleanup.
2. Select one immutable pipeline artifact set by Release Manifest/digest; no rebuild.
3. Verify QA config versions and `SECREF-*` resolution without exposing values.
4. Verify recovery point and retained compatible rollback target.
5. Run reviewed migration bundle through `SID-MIGRATION`; never application startup.
6. Deploy in approved dependency order with health/admission gates.
7. Run documented smoke and relevant negative/recovery checks.
8. Observe dashboards/alerts/queue/Worker/SQL/storage/Audit/Issue/SignalR/client compatibility.
9. Exercise at least one safe rollback/config-revert or controlled abort path appropriate to QA.
10. Record attempt, approvals, exact artifact/config/schema, results, first failures/reruns, rollback readiness, evidence location, owner acceptance, and gaps.

Until that evidence exists, deployment handover status is `REHEARSAL_REQUIRED`, not `VERIFIED` or `PRODUCTION_READY`.

---

# 11. Rollback handover

| Rollback ID/type | Trigger | Owner/approval | Artifact/reference | Data impact | Validation/communication | Incident/evidence | Status |
|---|---|---|---|---|---|---|---|
| `RBK-APP` App Rollback | Bad/incompatible application release, safe previous binary compatible | Deployment Owner + environment/Incident authority | Retained verified previous artifact/digest + previous compatible config | Must assess DB/schema/Job/checkpoint/SQLite/client state; no blind downgrade | Preflight compatibility → deploy → health/smoke/monitor; user/change communication | Incident/change, approval, current/target digests, outcome | `REHEARSAL_REQUIRED` |
| `RBK-CONFIG` Configuration Rollback | Typed config causes failure/risk | Configuration Owner; high-impact reauth/Audit and release authority as required | Previous versioned configuration, never ad hoc file edit | May affect exposure/admission/provider/compatibility; no data rollback assumed | Validate schema/compatibility/reload/restart; smoke and monitor | Audit event, Incident/change, config versions/evidence | `REHEARSAL_REQUIRED` |
| `RBK-FLAG` Feature Flag Disable | Scoped Feature risk/containment where compatible | Feature/Configuration Owner + Incident/release authority | Existing governed flag version and scope | Stops new exposure only; cannot reverse completed data or weaken security | Verify exact Feature/platform/cohort; monitor disabled behavior and Local-First | Incident, flag old/new version, approval, Audit, observation | `DESIGN_DEFINED` |
| `RBK-PROVIDER` Provider Fallback | Approved primary engine/provider unavailable/unsafe and compatible fallback exists | Module/Provider/Security owners; release/config approval | Pre-approved provider profile/version/digest only | Output fidelity/security/resource/checkpoint compatibility must be reassessed | Capability/PoC/license/security/health/output test; user limitation communication | Incident, provider profile change, tests/Audit/release evidence | `REQUIRES_CONFIRMATION`; no public fallback |
| `RBK-DB-RESTORE` Database Restore | Corruption/destructive migration/data loss where restore is approved | DBA/Recovery/Incident/Production authority | Exact verified backup set manifest + compatible artifact/config/storage set | Broad; may lose changes after recovery point and require cross-store reconciliation | Restore isolated first; full SQL/storage/AuthZ/Audit/Job/Core validation and communication | Incident/change, recovery point, impact acceptance, restore evidence | `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` |
| `RBK-DATA-REPAIR` Data Repair | Verified data integrity issue not safely solved by rollback/restore | Data Owner + Security/DBA/Incident/Production authority | Approved one-purpose repair plan/artifact, never manual direct edit | Exact rows/objects/versions; irreversible risk possible | Dry run on isolated copy, constraints/digest/AuthZ/Audit validation, affected-user process | Incident/problem/change, before/after safe counts/digests, approvals | `BLOCKED` until exact governed procedure exists |

Database restore and data repair are not ordinary deployment rollback. After destructive/data migration, forward-fix is the default unless verified evidence proves another route safer.

---

# 12. Backup/restore handover

## 12.1 Backup scope register

| Backup scope | Required contents/references | Encryption/access | Retention/status | Restore validation |
|---|---|---|---|---|
| SQL Server | DB/log/history, schema migration histories, permissions, outbox/idempotency/Audit/Job state | Encrypted; separate backup identity; no business content permission implied | Official retention TBD; no actual set evidenced | Schema/constraint/permission/outbox/idempotency/Audit append/Job reconciliation |
| Secure object storage | Object/version/digest/key-reference metadata/tombstone/hold and zone manifest | Encrypted/private; exact storage/backup identities | Retention/replication/topology TBD | Object-metadata/digest/version/owner/hold reconcile with SQL |
| Configuration/policy | Versioned typed config/policy/flag/compatibility and environment mapping | Restricted; no secret values | Retain per release/recovery policy TBD | Schema/compatibility/effective-source/rollback validation |
| Secret/custody recovery metadata | Approved reference identifiers, authority/process, recovery metadata—not material values | Separate highly restricted custody | Pertamina security policy | Resolve required references in isolated restore without exposure |
| Artifacts/SBOM | Immutable release/rollback artifacts, manifests/digests/SBOM/provenance/test evidence | Restricted artifact store | Must retain active/approved rollback sets; exact period TBD | Verify digest/build/commit/compatibility and deployability |
| Monitoring/alert config | Exporter/dashboard/alert/runbook/owner configurations | Restricted safe metadata | Retention/version policy TBD | Restore freshness/delivery without sensitive telemetry copy |
| SQLite/local protected profile | Only when approved device/profile backup is in scope: DB + required artifacts + integrity manifest | Per-device/profile protection; backup exclusion/security policy | Local policy TBD | Restore profile/account/schema/artifact binding; never raw DB alone |

## 12.2 Restore proof model

```text
Authorized exact backup set
→ manifest/digest/access/custody/config/artifact validation
→ isolated restore target
→ SQL restore
→ storage object restore/reconciliation
→ queue/outbox/idempotency/Audit/Job reconciliation
→ application/config/client compatibility
→ AuthZ/security/Audit/Core smoke
→ measured result/gap/observation
→ approved reopen or remediation
```

Required evidence: Backup Set ID, environment/scope, start/end UTC, owner/executor/approver, manifest and integrity result, encryption/custody reference, retention class, job/status/failure, isolated target, restored versions, SQL/storage counts and reconciliation, permission/AuthZ validation, Audit append test, Job/outbox/idempotency validation, representative Core smoke, measured recovery time/gap (not SLA), defects, cleanup, and sign-off.

No restore test evidence was found. Current status is `REHEARSAL_REQUIRED`, never `VERIFIED`. Official targets remain `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION`.

## 12.3 DR input register

- authority, Incident/change route, owner/deputy/on-call and communication tree;
- asset/dependency inventory and approved recovery priority;
- alternate site/topology/replication/failover/failback and capacity;
- DNS/TLS/time/SSO/network recovery ownership;
- custody/config/artifact recovery references;
- SQL/storage/queue/Audit/monitoring recovery order;
- RPO/RTO/retention/legal hold targets;
- client compatibility/distribution during recovery;
- exercise cadence, scenario, stop criteria, evidence, and gap acceptance.

All values are TBD until Pertamina approves them. No DR site or topology is invented.

---

# 13. Monitoring/alert handover

No numeric threshold is set here. `Threshold source` must be `MEASURED_BASELINE`, `PILOT_OBSERVATION`, `APPROVED_LIMIT`, `POLICY_CONFIGURABLE`, or an explicit security invariant.

| Dashboard/Metric ID | Purpose | Location | Owner | Alert | Threshold source | Runbook | Access | Status |
|---|---|---|---|---|---|---|---|---|
| `MON-API-HEALTH` | Liveness/readiness, latency/error, admission/false-202 detection | `<APPROVED_DASHBOARD_TBD>` + safe health endpoints | API/Operations owner TBD | API unavailable/not-ready/error trend | Baseline/approved availability rule; no invented value | `RB-API-OUTAGE-001`, `DRB-API-FAIL-001` | `monitoring.system.read`/operations scope | `NOT_PROVISIONED` |
| `MON-WORKER-HEALTH` | Worker readiness, claim/drain/lease/checkpoint/crash/resource | `<APPROVED_DASHBOARD_TBD>` | Processing owner TBD | Not-ready/stale lease/crash/resource pressure | Measured/approved limits | `RB-QUEUE-OVERLOAD-001`, `DRB-WORKER-FAIL-001` | `monitoring.job.read`/operations | `NOT_PROVISIONED` |
| `MON-SQL-HEALTH` | Connection/DB/log/pool/lock/I/O/capacity/recovery freshness | `<APPROVED_DASHBOARD_TBD>` | DBA/DB owner TBD | Unavailable, pool/log/lock/capacity risk | Vendor safe invariant + approved baseline | `RB-STORAGE-FAIL-001`, `DRB-DB-MIG-FAIL-001` | Restricted operations/DB monitoring | `NOT_PROVISIONED` |
| `MON-STORAGE-HEALTH` | Availability/capacity/digest/orphan/cleanup/backup | `<APPROVED_DASHBOARD_TBD>` | Storage owner TBD | Unavailable/capacity/integrity/cleanup failure | Policy + measured baseline | `RB-STORAGE-FAIL-001`, `DRB-STORAGE-FAIL-001` | Restricted operations | `NOT_PROVISIONED` |
| `MON-QUEUE` | Depth, oldest age, admission, claim, drain, fairness | `<APPROVED_DASHBOARD_TBD>` | Jobs/Queue owner TBD | Sustained age/depth/no drain | Approved workload/capacity policy | `RB-QUEUE-OVERLOAD-001`, `DRB-QUEUE-HIGH-001` | `monitoring.job.read` | `NOT_PROVISIONED` |
| `MON-JOB-FAILED` | Failed Jobs by safe Feature/error/workload/environment | `<APPROVED_DASHBOARD_TBD>` | Processing/Support owner TBD | Failure/security/data-integrity trend | Baseline + security invariant | `RB-DATA-INTEGRITY-001`, affected component runbook | Safe aggregate only | `NOT_PROVISIONED` |
| `MON-RETRY-RATE` | Retry attempt/backoff/storm/duplicate effect | `<APPROVED_DASHBOARD_TBD>` | Processing owner TBD | Retry storm/no progress/duplicate signal | Measured baseline + idempotency invariant | `RB-QUEUE-OVERLOAD-001` | `monitoring.job.read` | `NOT_PROVISIONED` |
| `MON-SIGNALR` | Connection/reconnect/gap/unknown version/REST recovery | `<APPROVED_DASHBOARD_TBD>` | Realtime owner TBD | Reconnect/gap/fallback failure trend | Measured baseline; correctness invariant | `RB-SIGNALR-001` | Safe operations aggregate | `NOT_PROVISIONED` |
| `MON-AUDIT` | Owner outbox/backlog/ingestion/schema/sequence/export/freshness | `<APPROVED_DASHBOARD_TBD>` | Audit owner TBD | Mandatory intent not durable/backlog/no progress | Security/Audit policy | `RB-AUDIT-FAIL-001` | Audit operations workload; not general Audit read | `NOT_PROVISIONED` |
| `MON-ISSUE` | Submit/attachment/outbox/status/export service health | `<APPROVED_DASHBOARD_TBD>` | Support owner TBD | Submit/outbox/attachment/export failure | Baseline/approved support rule | `RB-ISSUE-001` | Safe aggregate; Issue content permission separate | `NOT_PROVISIONED` |
| `MON-AUTHN` | Safe login/session/revocation/refresh failure outcome | `<APPROVED_DASHBOARD_TBD>` | IAM/Security owner TBD | Abnormal failure/replay/revocation signal | Security policy, no arbitrary counts | `RB-SSO-FAIL-001` | Restricted security/operations | `NOT_PROVISIONED` |
| `MON-AUTHZ` | Sensitive denial/IDOR/probe/abuse trend | `<APPROVED_DASHBOARD_TBD>` | Security owner TBD | Cross-user/probe/abuse invariant | Security policy | `RB-CROSS-USER-001` | Restricted security; no content/owner leakage | `NOT_PROVISIONED` |
| `MON-PROVIDER` | Approved engine/provider health, timeout/circuit/resource/version | `<APPROVED_DASHBOARD_TBD>` | Module/Provider owner TBD | Unavailable/circuit/security/invalid-output | Approved provider profile/baseline | `RB-AI-PROVIDER-001`, `RB-MALICIOUS-DOC-001` | Restricted operations aggregate | `NOT_PROVISIONED` |
| `MON-COMPAT` | Current/minimum client, contract/capability/IPC/schema mismatches | `<APPROVED_DASHBOARD_TBD>` | Release/Client owner TBD | Unsupported/update-required anomaly | Approved compatibility policy | `DRB-CLIENT-INCOMPAT-001` | Safe Admin/operations metadata | `NOT_PROVISIONED` |
| `MON-RESOURCE` | CPU/RAM/disk/temp/network/DB connections/storage growth | `<APPROVED_DASHBOARD_TBD>` | Infrastructure/Capacity owner TBD | Unsafe saturation/headroom/cleanup competition | Measured baseline/Pilot/approved limits | `DRB-QUEUE-HIGH-001`, component runbooks | Restricted aggregate | `NOT_PROVISIONED` |

Every alert must have owner, environment/component/version/freshness, threshold source/status, deduplication, severity policy, notification/escalation route, runbook, acknowledgement, evidence link, maintenance handling, and review date. An alert without an owner or actionable runbook cannot be accepted for Production handover.

---

# 14. Desktop/mobile distribution

## 14.1 Desktop handover

| Area | Required handover | Status/gap |
|---|---|---|
| Package | Immutable WinUI 3 Desktop + Local Worker compatibility cohort, runtime/native/engine resources, registry/localization/config schema, SQLite migrations, license/SBOM/digest/signing receipt | Actual package absent; MSIX remains candidate only |
| Signing | Approved publisher/custody service and verification/renewal/revocation process; no material in source/report | `REQUIRES_PERTAMINA_CODE_SIGNING_CONFIRMATION` |
| Distribution | Approved enterprise channel, scope/ring, publish/withdraw authority, availability and evidence | `REQUIRES_PERTAMINA_DESKTOP_DISTRIBUTION_CONFIRMATION` |
| Installation | Verify publisher/integrity/version/OS/architecture/runtime/storage/scope; explain data behavior; first-run SQLite/IPC/SSO/compatibility checks | PoC/UAT/rehearsal required |
| Update | Approved channel only; signature/digest; checkpoint active work; atomic cohort update; SQLite migration; post-check | Rehearsal required |
| Rollback | Retained signed compatible cohort; SQLite/workspace/checkpoint/API/config compatibility; forward-fix if state advanced | Rehearsal required |
| Local Worker | Same signed cohort by default; `LocalWorkerVersion` + `IpcContractVersion`; local-only Named Pipe; least privilege | Exact IPC/package PoC pending |
| Compatibility | `AppVersion`, Local Worker, IPC, LocalSchema, API/Contract/Capability/engine ranges in Release Manifest | Actual matrix absent |
| Uninstall/data preservation | Binary and local user data separated; explicit preserve/remove options; source/output/workspace/signature not silently removed | UI/security policy confirmation required |

## 14.2 Mobile/Tablet handover

| Area | Required handover | Status/gap |
|---|---|---|
| Package | Signed platform package with stable app identity/version, config schema, LocalSchema migrations, curated capabilities, secure-store/backup exclusions, permissions, SBOM/digest/signing receipt | Actual platform/package absent |
| Signing | Approved platform/team identity/custody/renewal/revocation; no material in report | `REQUIRES_PERTAMINA_CODE_SIGNING_CONFIRMATION` |
| Enterprise distribution | Approved MDM/store/side-load/channel, ring, publish/withdraw, participant/device scope | `REQUIRES_PERTAMINA_MOBILE_DISTRIBUTION_CONFIRMATION` |
| Update | Staged managed rollout; integrity/compatibility/minimum version; SQLite recovery/local work preservation; post-check | Rehearsal required |
| Device permission | Camera/file/share/notification requested at point of use; permission OS does not grant OTO business permission/upload | Device matrix/UAT required |
| Compatibility | MobileVersion + LocalSchema + API/Contract/Capability/platform ranges | Actual matrix absent |
| Data behavior | Draft/cache/signature/OCR/workspace remain local and no auto-sync; managed wipe/uninstall exact policy approved | Policy/distribution confirmation required |

Fresh offline install cannot invent authentication. Offline use requires a prior valid protected grant; package availability does not imply Feature/provider capability or permission.

---

# 15. Pertamina confirmation gaps

1. `BLOCKED_BY_DECISION: DOCUMENT_18_FINAL_INTEGRATION` — master Application Cartography FINAL, authoritative owner/component/interface/dependency/zone mapping belum tersedia.
2. `REQUIRES_PERTAMINA_ORGANIZATION_CONFIRMATION` — named owner, accountable/deputy/on-call/escalation, current/target access owner, approver, SoD/delegation.
3. `REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION` — physical environment/topology, hosting/OS/container/proxy, asset IDs, SQL/storage/queue/monitoring products, service placement, capacity.
4. `REQUIRES_PERTAMINA_NETWORK_CONFIRMATION` — DNS/TLS/certificate profile, ports/protocols, ingress/egress/firewall/private connectivity, admin/deployment/backup/provider paths.
5. `REQUIRES_PERTAMINA_SECURITY_CONFIRMATION` — custody store, workload identity product/scopes, access lifecycle/JIT/MFA/reauth, encryption/certificate custody, evidence/retention.
6. `REQUIRES_PERTAMINA_SSO_CONFIRMATION` — SSO registration/protocol/issuer/audience/claims/redirect/session/revocation/reauth/help route.
7. `REQUIRES_PERTAMINA_RELEASE_APPROVAL_CONFIRMATION` — repository/pipeline/environment approvers, Azure DevOps org/project/resource IDs, branch/service-connection/environment permissions, waiver/emergency process.
8. `REQUIRES_PERTAMINA_CODE_SIGNING_CONFIRMATION` — signing service/custody/algorithm/trust/timestamp/renewal/revocation/emergency re-sign.
9. `REQUIRES_PERTAMINA_DESKTOP_DISTRIBUTION_CONFIRMATION` — package technology, publisher identity, enterprise channel, install/update/rollback/uninstall/data policy.
10. `REQUIRES_PERTAMINA_MOBILE_DISTRIBUTION_CONFIRMATION` — platforms/OS/architecture/app/team identity/MDM-store/channel/rings/managed wipe/device policy.
11. `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION` — backup product/location/replication, RPO/RTO/retention, DR site/failover/failback, key recovery authority, restore cadence.
12. `BLOCKED_BY_DECISION: ACCESS_REGISTER_POPULATION` — actual grants/groups/identities/owners/expiry/evidence must be entered in restricted IAM/CMDB, never this report.
13. `BLOCKED_BY_DECISION: CONFIGURATION_AND_CUSTODY_PRODUCTS` — exact typed config store/control plane, custody store, injection/reload/rotation/audit/recovery behavior.
14. `BLOCKED_BY_DECISION: MONITORING_AND_ALERT_OPERATIONS` — backend/dashboard/alert delivery, safe schema, owners, thresholds, maintenance/on-call, retention.
15. `BLOCKED_BY_DECISION: QA_HANDOVER_REHEARSAL` — exact QA artifact/config/migration/backup/rollback/monitoring deployment has not been executed/evidenced.
16. `BLOCKED_BY_DECISION: DATABASE_STORAGE_HANDOVER` — actual instance/database/zone IDs, DbContext/migration packages, least privileges, backup/restore reconciliation evidence.
17. `BLOCKED_BY_DECISION: PROVIDER_ENGINE_HANDOVER` — approved provider/engine portfolio, PoC/license/security/SBOM/resource/egress/fallback/rollback/owner.
18. `TARGET_TO_BE_APPROVED` / `POLICY_CONFIGURABLE` — rate/quota/concurrency/size/timeout/capacity/retention/alert thresholds. No official numbers are invented.

These gaps do not permit credential exchange in documents, Hostinger-as-Production, public storage/AI, direct client SQL, manual Production patch, startup migration, unowned monitoring, or a false `VERIFIED` status.

---

# 16. Missing-access audit

| Audit item | Evidence inspected | Finding | Required remediation/status |
|---|---|---|---|
| Access without owner | No named access/asset owner records supplied; Document 18 ownership remains TBD | **OPEN—HIGH** | Populate functional + named owner/deputy/approver in restricted IAM/CMDB; periodic review |
| Secret in handover document | This report contains only conceptual `SECREF-*` and placeholders; no values requested/stored | **PASS for this artifact only** | Automated/manual secret scan before publication; keep references value-free |
| Production credential copied | None supplied or requested | **PASS for this artifact; environment unknown** | Verify restricted custody/access logs rather than sharing copies |
| Config not versioned | Design requires schema/version/provenance; actual config records absent | **OPEN** | Implement typed versioned config catalog + rollback evidence |
| Environment mixed | Logical separation locked; no actual topology inspected | **OPEN—HIGH** | Prove separate endpoint/identity/data/config/secret/storage/telemetry/authority |
| Backup not restorable | No restore execution evidence found | **OPEN—CRITICAL for Production handover** | Isolated QA restore rehearsal and validation; never mark VERIFIED before PASS |
| Monitoring without owner | All monitoring owners remain TBD | **OPEN—HIGH** | Assign owner/deputy/on-call and access scope per dashboard/metric |
| Alert without runbook | Design runbooks exist but thresholds/owners/exercises are incomplete | **OPEN** | Link exact alert → threshold source → owner → runbook → exercise/evidence |
| Pipeline without approval | Logical human gates exist; actual environment/approver records absent | **OPEN—HIGH** | Azure DevOps policy/service-connection/environment approval evidence |
| Service identity too broad | Identities are conceptual; actual scopes not inspectable | **OPEN—HIGH** | Purpose/environment least-privilege matrix and denied over-scope tests |
| SQL exposed to client | Architecture explicitly prohibits it; actual network not inspected | **PASS design / NOT_EVIDENCED runtime** | Private connectivity/firewall/access test evidence |
| Storage public | Design requires private exact-purpose zones; product/topology absent | **PASS design / NOT_EVIDENCED runtime** | Public-access disabled test and cross-scope denial evidence |
| Signing custody unmanaged | No approved signing service/owner/process evidenced | **OPEN—HIGH** | Official custody, pipeline identity, renewal/revocation and package verification |
| Hostinger treated as Production | Document 17 explicitly prohibits it; this report preserves temporary non-Prod status | **PASS design** | Asset label/guard/exit and external backup evidence if used |
| Production topology invented | This report uses placeholder only | **PASS for this artifact** | Populate only after official architecture/infrastructure approval |
| Access permission exceeds content authority | Admin/Support/Audit/DBA/Backup separation is designed; grants unavailable | **OPEN** | Resource/scope negative tests and access review evidence |
| Restore standing privilege | No grants evidenced | **OPEN** | Prefer Incident/change-bound temporary authority and revoke evidence |
| Artifact delete/retention access | Actual ACL/retention unknown | **OPEN** | Protect active/rollback sets; separate publish/promote/delete roles |

Overall missing-access audit result: **NOT READY FOR OPERATIONAL HANDOVER**. Design controls are coherent, but actual owners, grants, resources, evidence, and rehearsals are absent.

---

# 17. Evidence requirements

## 17.1 Evidence catalog

| Evidence ID family | Required proof | Classification/store | Acceptance |
|---|---|---|---|
| `EVD-ENV-*` | Asset/environment inventory, logical/physical mapping, data/classification, owners, readiness | Restricted CMDB/handover repository | Exact scope/version/reviewer/date; no credential |
| `EVD-ACCESS-*` | Approved grant/scope/expiry/approver, effective access test, denied over-scope test, revoke result, review | Restricted IAM evidence | Current and target owner; no raw session/token |
| `EVD-SID-*` | Workload identity purpose/audience/environment/permissions, access and denied tests, rotation/revoke | Restricted security evidence | Least privilege verified; human role not inherited |
| `EVD-CONFIG-*` | Schema/version/provenance/effective source/validation/change/Audit/reload/rollback | Restricted config evidence | No business-rule override or secret value |
| `EVD-PIPE-*` | Repo/PR/build/gate/artifact/SBOM/digest/promotion/approval | Azure DevOps/release evidence store | Exact Commit/Build/artifact; failures/reruns retained |
| `EVD-DB-*` | Schema/DbContext/migration/permissions/health/backup/restore/validation | Restricted DB/release evidence | Cross-module/write/connection/restore checks PASS |
| `EVD-STOR-*` | Zones/access/digest/lifecycle/capacity/orphan/backup/restore/denial | Restricted storage evidence | No public/cross-user/object-key leakage |
| `EVD-DEPLOY-*` | Exact attempt, environment, config/migration/health/smoke/monitoring/rollback outcome | Release evidence store | Same immutable artifact; human approval; safe evidence |
| `EVD-RESTORE-*` | Backup set/manifest/integrity/custody ref/isolated restore/reconciliation/security/Core smoke | Restricted recovery evidence | Completed exercise with defects/gaps and owner sign-off |
| `EVD-MON-*` | Dashboard/metric/alert/threshold source/owner/runbook/access/freshness/exercise | Observability governance store | Alert reaches accountable route and is actionable |
| `EVD-DIST-*` | Package digest/signing receipt/publisher/app identity/channel/ring/install/update/rollback/uninstall | Restricted release/distribution store | Package identity/integrity/compatibility/data behavior PASS |
| `EVD-HANDOVER-*` | Checklist, open gaps/risks, recipient review, QA rehearsal, authority sign-off | Handover repository | No `VERIFIED` until every mandatory item has evidence |

## 17.2 Evidence rules

- Evidence references exact environment, artifact/config/schema/version, owner/reviewer, UTC time, result, limitation, and expiry/review date.
- Approval, access, backup, alert, and deployment evidence are append-oriented; failure/rerun is not overwritten.
- Evidence links contain no bearer/SAS/credential query and do not copy restricted payload.
- Restricted evidence access is separate from Admin/Support/Audit business permissions.
- Screenshots are minimized and sanitized; do not capture custody values, console secret display, document/OCR/signature/prompt, or Production data.
- A checklist mark without linked, readable, current evidence is `NOT_EVIDENCED`.

## 17.3 Handover acceptance package

The recipient signs only after: owners and access are assigned/reviewed; exact QA environment exists; immutable artifact/config/migration and rollback targets are available; QA deployment and safe rollback/abort rehearsal pass; SQL/storage/SQLite recovery checks pass; backup restore is tested; dashboards/alerts/runbooks are active and exercised; client distribution path is approved; known gaps/risks have authority and due date; and all evidence can be retrieved without credential sharing.

---

# 18. Risks/conflicts

| Risk/conflict | Impact | Mitigation |
|---|---|---|
| Document 18 FINAL unavailable | Component/zone/owner edge may change during integration | Mark interim mappings PARTIAL; reconcile when FINAL appears |
| One developer plus Codex | Knowledge/approval/SoD concentration | Named functional owner/deputy, human gates, access review; Codex never owner/approver |
| Handover register mistaken for proof | False readiness | Status/evidence discipline; no VERIFIED without actual test and reviewer |
| Access list stores actual account/credential | Security disclosure and stale data | Store only record IDs; actual assignment in restricted IAM/CMDB |
| Config reference mistaken for secret | Values copied to docs or source | Strict `SECREF-*` indirection and runtime custody resolution |
| Environment separated only by config name | Cross-environment data/identity leakage | Separate endpoint/identity/DB/storage/queue/provider/telemetry/authority |
| Hostinger one node used as Production/backup | SPOF, no HA/security assurance | Non-Prod temporary guard, external backup, exit plan; Production topology TBD |
| No restore test | Backup gives false assurance | Mandatory isolated QA restore/reconcile exercise before acceptance |
| Restore treated as rollback | Broad data loss/inconsistency | Distinguish binary/config rollback, forward-fix, restore, data repair |
| Broad DBA/storage/backup privilege | Content bypass across roles | Purpose-specific identities, no client/direct browsing, exact object/schema scopes |
| Pipeline service identity spans all environments | Compromise reaches Production | Separate environment service connections/approvals; least privilege/JIT where approved |
| Signing process on developer workstation | Supply-chain compromise | Approved signing service/custody; submit-only pipeline identity; no material retrieval |
| Artifact retention insufficient | Rollback package unavailable | Protect active/approved rollback artifacts; tested repository recovery |
| Monitoring alert lacks owner/runbook | Silent or unactionable failure | Owner/deputy/threshold source/runbook/exercise mandatory |
| Thresholds invented during handover | False SLA/capacity assurance | Measured baseline/Pilot/approved limit lifecycle only |
| Admin Manual surfaces await API | Access granted to non-existent or unofficial control path | Document 06 route authority; no direct SQL/hidden endpoint workaround |
| Service identity inherits user token/role | Cross-user and audit confusion | Separate workload identity, purpose/audience/job scope, denied tests |
| Storage/SQL product selected ad hoc | Architecture/vendor/security drift | Pertamina infrastructure/security decision and PoC before provisioning |
| Client rollback after SQLite migration | Local state unreadable/corrupt | Compatibility matrix, forward-fix default after new state, no reset/downgrade |
| Mobile/Desktop channel unapproved | Cannot update/revoke/withdraw safely | Signing/distribution confirmation and update/rollback rehearsal |
| Backup/monitoring shares temporary single node | Node loss removes service and evidence | Separate approved sink/backup target; Production design TBD |
| Secret missing at startup triggers hardcode | Credential leak/bypass | Not-ready/fail-safe; restore approved reference only |
| Production topology filled from assumption | Unsafe handover and false authority | Keep placeholder until signed infrastructure/network/security decision |

---

# 19. Suggested Document 19 text

## 19.1 Handover principles

PERTAMINA OTO technical handover records identifiers, purpose, owner, approved store, consumer, rotation/revocation, access process, status, and evidence. It never records password, token, API/private/signing key material, connection secret, database/Production credential, or credential-bearing connection string. Actual assignments and custody item identifiers reside only in approved restricted IAM/CMDB/configuration systems.

Status `VERIFIED` requires actual scoped evidence, reviewer, time, and result. Documentation design alone is `DESIGN_DEFINED`; an unavailable resource is `NOT_PROVISIONED`; an uninspected claim is `NOT_EVIDENCED`; an operational path not exercised is `REHEARSAL_REQUIRED`.

## 19.2 Environment handover

The environment register contains Local Development, Shared Development, QA/Testing, Pilot, and Production. Each environment records Environment ID, purpose, logical/physical placement reference, data classification, deployables, SQL/SQLite/storage, config/custody source, pipeline/artifact, monitoring, backup, access/support owners, release version, readiness, and confirmation gaps.

Every environment has separate endpoint, identity, data, config, secret reference, SQL/storage/queue/provider, telemetry, backup, and deployment authority. Hostinger KVM 2 is temporary for approved Dev/limited QA/Pilot only. It is not Production, HA proof, or sole backup location. Production remains `<PERTAMINA_APPROVED_PRODUCTION_INFRASTRUCTURE_TBD>` until official approval.

## 19.3 Infrastructure and access

Logical infrastructure includes Web, API, SignalR, Server Worker, SQL Server, Secure File/Object Storage, Job Store/Queue, Audit, Monitoring, Pertamina Identity, typed Configuration, custody/reference service, Backup/Restore, and Azure DevOps. Client exposure is limited to approved Web/API/SignalR/compatibility/minimal health. Worker, SQL, storage, queue, migration, backup, detailed health/monitoring, admin, and provider interfaces are restricted/internal.

The access register covers Repo, Pipeline, Artifact Storage, Package Feed, Dev/QA/Pilot/Production, SQL, Secure Storage, custody store, Monitoring, Audit, Support, SSO registration, Desktop/Mobile signing and distribution, Backup, Restore, and Service Desk. Every grant records current/target owner, approver, permission/scope/environment, grant/revoke process, MFA/reauth policy, expiry/review, evidence, and status. Application role, resource ownership, operational access, and content clearance remain separate.

## 19.4 Service identities and configuration

Web, API, Worker, Migration, Storage, Monitoring, Pipeline, Desktop Signing, Mobile Signing, Audit Ingestion, and Backup/Restore use separate conceptual purpose-scoped workload identities per environment. User tokens/roles are never forwarded or inherited. Identity references are resolved at runtime through an approved mechanism and are not embedded in source, artifact, logs, tickets, screenshots, or this document.

Configuration is typed, versioned, validated, environment-bound, integrity-controlled, and auditable when sensitive. The hierarchy is System Safety/Security → Enterprise Policy → Environment → Device → User Preference. Configuration may reference approved profiles; it cannot change Feature meaning, permission, ownership, Audit, Local-First, max-three, non-PKI scope, or public-AI block. Missing environment-bound configuration or custody reference causes not-ready/fail-safe.

## 19.5 SQL Server and SQLite

SQL Server uses one baseline logical server/database per approved environment with module-owned schemas: `identity`, `admin`, `configuration`, `workspace`, `jobs`, `secured`, `history`, `audit`, `support`, `assistant`, and `intelligence`. Each schema has one DbContext/migration owner. Runtime, migration, monitoring, backup, and restore identities are separated. SQL is private; clients never access it. Migration is a reviewed pipeline artifact tied to Release Manifest, never an application-startup side effect.

SQLite is per local profile/device and stores local application metadata/state—not the enterprise server database. File bytes remain in the protected Local Workspace. Handover covers `LocalSchemaVersion`, migrations, protected DB/WAL/SHM/artifacts, profile/account binding, checkpoint/recovery, cleanup, uninstall preservation, and compatibility. Unknown newer schema, corruption, or failed migration never causes silent reset, downgrade, source deletion, or automatic server sync.

## 19.6 Storage

Storage handover separates Local Workspace, local/server temporary processing, Secured Files, Issue/Support, Audit export, Backup, and Artifact Storage. Every store records owner, purpose, payload/classification, encryption/custody reference, exact access scope, quota/retention/hold, cleanup, backup/restore, health, monitoring, and status. SQL holds metadata; file payload resides in the appropriate protected store. Physical path, object key, raw delegated URL, and credential are not handover fields.

## 19.7 Deployment and QA rehearsal

Deployment handover records pipeline definition, exact Release Manifest/artifact digests, environment, approvals, configuration/custody dependencies, migration bundle/version, dependency order, health, smoke, monitoring, rollback, Incident route, and evidence. Build once and promote the same artifact; environment differences are configuration/reference values, not source branches or rebuilds.

Before Production handover acceptance, the receiving team executes an approved QA deployment rehearsal using `DRB-QA-DEPLOY-001`: exact artifact/config/data/access/window, recovery point, pipeline migration, dependency-gated deployment, smoke/negative/recovery tests, monitoring/alert observation, safe rollback/abort exercise, cleanup, and reviewed evidence. Without that rehearsal, status remains `REHEARSAL_REQUIRED`.

## 19.8 Rollback, backup, restore, and DR

App rollback, configuration rollback, Feature flag disable, approved provider fallback, database restore, and data repair are different actions with separate trigger, owner, approval, artifact/reference, data impact, validation, communication, Incident link, and evidence. Rollback never rebuilds old source or blindly applies an older binary to incompatible data. After data-changing migration, forward-fix is default unless evidence approves restore/compensation.

Backup handover defines SQL, storage, configuration/policy, custody recovery metadata, artifacts/SBOM, monitoring configuration, and applicable local-profile scope. A backup is not `VERIFIED` until an authorized isolated restore validates manifest/integrity/access, SQL/storage reconciliation, permissions, Job/outbox/idempotency/Audit, application/client compatibility, and representative security/Core smoke. Official RPO/RTO/retention/DR topology remain `REQUIRES_PERTAMINA_BACKUP_DR_CONFIRMATION`.

## 19.9 Monitoring

Monitoring handover covers API, Worker, SQL, Storage, Queue, failed Jobs, retry rate, SignalR, Audit pipeline, Issue service, authentication, authorization, provider, client compatibility, and resource usage. Every dashboard/metric/alert has ID, purpose, approved location, owner/deputy, alert behavior, threshold source, runbook, access, freshness, and status. Thresholds come from measured baseline, Pilot observation, approved limit, policy, or hard security invariant—never an invented number.

## 19.10 Desktop and Mobile distribution

Desktop handover covers WinUI package, Local Worker signed compatibility cohort, `AppVersion`, `LocalWorkerVersion`, `IpcContractVersion`, `LocalSchemaVersion`, signing receipt, enterprise channel, install/update/rollback, and explicit uninstall/data preservation. MSIX remains a candidate until PoC/approval.

Mobile handover covers platform package/app identity, signing service/profile reference, enterprise distribution/rings, update/withdraw, secure store/backup exclusion, device permissions, LocalSchema/API/capability compatibility, and managed-wipe/data behavior. Exact channels remain subject to `REQUIRES_PERTAMINA_MOBILE_DISTRIBUTION_CONFIRMATION`.

## 19.11 Acceptance and open gaps

The handover is not operationally accepted until owners/approvers/access are assigned, environments and identities are provisioned, configuration/custody references resolve, QA deployment and rollback are rehearsed, SQL/storage/SQLite recovery is validated, backup restore is tested, dashboards/alerts/runbooks are exercised, distribution is approved, and the evidence package is reviewable without credential sharing.

Document 18 master integration, organization/owners, infrastructure/network/security/SSO, Azure DevOps resources/approvals, custody/config products, SQL/storage/queue/monitoring topology, signing/distribution, Backup/DR targets, provider portfolio, numeric limits, and official Production topology remain controlled confirmation items.

---

**Agent B conclusion:** the handover is structurally defined but not operationally ready. No actual environment, access grant, service identity, release artifact, Production topology, backup set, restore result, monitoring owner, signing path, or distribution channel has been evidenced. The safe next gate is to assign official owners in restricted systems, provision an isolated QA environment, and complete an evidence-backed QA deployment plus restore/rollback rehearsal—without exchanging credential material through documents.
