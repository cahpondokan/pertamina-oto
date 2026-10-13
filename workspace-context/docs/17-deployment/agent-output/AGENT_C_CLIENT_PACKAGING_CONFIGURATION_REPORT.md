# AGENT C — CLIENT PACKAGING, CONFIGURATION & UPDATE REPORT

**Task ID:** `DOC17-AGENT-C`  
**Status:** Rekomendasi specialist; keputusan final tetap milik user dan Integrator  
**Tanggal:** 2026-09-01  
**Target integrasi:** Document 17 — Deployment Guide  
**Sumber:** Document 01–16 FINAL

Laporan ini mendefinisikan boundary, acceptance, dan opsi kandidat. Laporan ini tidak memilih channel distribusi, web server/reverse proxy, container platform, MDM, application store, certificate, engine/vendor, atau topology Produksi.

---

# 1. Host packaging overview

## 1.1 Deployable dan release unit

| Host | Baseline teknologi | Release artifact konseptual | Release relationship |
|---|---|---|---|
| Web | Responsive Web pada host yang disetujui | Immutable `Pertamina.Oto.Web` artifact + static assets + manifest + SBOM/digest | Dapat dipromosikan terpisah jika compatible dengan API contract |
| API | ASP.NET Core, .NET 10 LTS baseline | Immutable `Pertamina.Oto.Api` artifact + contract/config schema manifest + SBOM/digest | Authority REST dan compatibility; schema/database migration bukan bagian startup |
| Server Worker | .NET Worker host | Immutable `Pertamina.Oto.Worker` artifact + handler/engine manifest + SBOM/digest | Dapat diskalakan/deploy terpisah; harus compatible dengan Job/queue/contract/schema |
| Windows Desktop | WinUI 3 | Signed Desktop package/bundle + Local Worker + approved runtime/native dependencies + manifest | Desktop dan Local Worker satu compatibility cohort |
| Local Worker | .NET executable/process lokal terisolasi | Signed binary/dependencies yang berasal dari release Desktop yang sama | Di-install/update bersama Desktop kecuali dependency model terpisah disetujui |
| Mobile/Tablet | .NET MAUI | Signed per-platform application package + approved managed resources/dependencies | Curated exposure; version dan entitlement per platform/channel |

Seluruh release artifact harus:

- dibuat oleh controlled pipeline dari commit dan dependency source yang disetujui;
- immutable setelah publikasi;
- mempunyai build/release ID, semantic application version, contract/config/migration versions yang relevan, digest, SBOM, provenance reference, dan test/gate evidence;
- memisahkan binary dari environment configuration dan secret;
- dipromosikan sebagai binary yang sama antar-environment bila format host memungkinkan;
- tidak memuat document/user content, Production data, credential, private signing key, unrestricted provider credential, atau debug artifact;
- mempunyai compatibility, rollback/forward-fix, monitoring, dan support reference.

## 1.2 Decision boundary

Package/runtime candidate tidak menjadi keputusan distribusi:

```text
Architecture baseline
→ enterprise/infrastructure constraints
→ security and code-signing process
→ offline/managed-distribution needs
→ dependency/engine PoC
→ install/update/uninstall PoC
→ Pertamina approval
→ final package/channel decision
```

Final Desktop package technology:

`REQUIRES_PERTAMINA_DESKTOP_DISTRIBUTION_CONFIRMATION`

Final Mobile/Tablet distribution:

`REQUIRES_PERTAMINA_MOBILE_DISTRIBUTION_CONFIRMATION`

Final server hosting, reverse proxy, service manager, container applicability, network/TLS termination, and topology:

`REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`

---

# 2. Web/API/Worker deployment inputs

## 2.1 Common server rules

1. Build sekali melalui pipeline; inject environment values terpisah saat deployment.
2. Development, QA/Test, Pilot, dan Production memisahkan endpoint, identity, credential, configuration, database/storage/queue/provider, telemetry, dan deployment authority.
3. Production tidak menjalankan debug, startup database migration, untracked config change, manual binary replacement, atau public-AI fallback.
4. Liveness hanya membuktikan process hidup. Readiness membuktikan host aman menerima work; detail dependency hanya untuk approved operations scope.
5. Secret absence menyebabkan startup/readiness failure yang aman, bukan default lemah atau embedded fallback.
6. Graceful shutdown tidak menerima work baru, menyelesaikan/menyerahkan work pada boundary aman, mempertahankan outbox/Audit intent, lalu keluar dalam timeout terkendali.
7. Rollback binary hanya dilakukan bila configuration/schema/contract tetap backward-compatible; jika tidak, gunakan forward-fix atau approved restore procedure.
8. Feature flag dapat melakukan staged rollout/emergency disable, tetapi tidak dapat mematikan authorization, validation, Audit, quarantine, rate limit, public-AI block, Local-First, atau hard safety ceiling.

## 2.2 Web host

| Input | Recommendation |
|---|---|
| Artifact | Pipeline-generated immutable Web artifact/static bundle, SBOM, digest, provenance, localization assets, dan version manifest. Format folder/archive/container belum dipilih. |
| Startup dependency | Approved hosting boundary, TLS/reverse-proxy configuration, environment bootstrap, API compatibility endpoint, identity/session configuration bila Web memiliki server-side session boundary, dan observability exporter. |
| Configuration | API base address, environment/channel code, localization, safe Web policy, cache/version metadata. Public config tetap integrity-controlled dan tidak boleh user-arbitrary pada Production. |
| Secret | Tidak ada secret dalam browser/static asset. Server-side session/anti-forgery/data-protection material, bila digunakan, berasal dari approved runtime secret/key boundary. |
| Service identity | Jika Web mempunyai server host/BFF, gunakan workload identity tersendiri dengan audience/purpose minimum; browser tetap memakai user session yang disetujui. |
| Health/readiness | Minimal liveness/readiness pada operations boundary; readiness memeriksa konfigurasi dan dependency wajib tanpa mengungkap topology/credential. |
| Logging | Structured safe fields: build/version, route template, status, duration, Correlation ID, safe error code; tanpa token, request body, document, raw user data, or session content. |
| Graceful shutdown | Stop new admission/session work, drain bounded active requests, flush safe telemetry, preserve session/Audit intent consistency. |
| Version endpoint | Web build/version dari signed artifact manifest atau restricted operations projection; `/api/compatibility` tetap authority client/API compatibility. |
| Feature flag | Web hanya merender server-filtered exposure; browser cannot enable hidden Feature or override policy. |
| Rollback | Promote previous signed Web artifact only if API/contract/cache compatibility passes; invalidate stale asset cache safely. |
| Deployment verification | Artifact digest/signature, CSP/security headers, no secret in assets/source maps, correct environment/API endpoint, compatibility, login/session, safe error, feature exposure, cache refresh, telemetry, and rollback readiness. |

Jangan memilih IIS, Kestrel exposure model, reverse proxy, container, ingress, load balancer, atau CDN final tanpa infrastructure approval.

## 2.3 API host

| Input | Recommendation |
|---|---|
| Artifact | Immutable .NET 10 API artifact, OpenAPI/contract manifest, configuration schema, SBOM, digest/provenance, migration compatibility declaration. |
| Startup dependency | Approved Identity validation metadata, SQL Server, secure object storage, Job/queue path, Audit intent/outbox, configuration/policy source, and observability. Dependency wajib memengaruhi readiness, bukan liveness. |
| Configuration | Bound typed options for allowed origins/proxy trust, limits, retention classes, feature/profile references, endpoint policy, and compatibility values. Unknown/unsafe values fail validation. |
| Secret | Database/storage/queue/provider credential only through approved workload identity/secret store; never config file, image, repository, log, or response. |
| Service identity | API identity terpisah, module/schema/object/queue permissions minimum; no deployment or human-admin rights. |
| Health/readiness | Existing `/health/live` and `/health/ready`; response Production minimal. Readiness covers mandatory path freshness without secret/topology/queue-detail leakage. |
| Logging | Structured safe identifiers and outcomes; no body, token/claim dump, raw SQL/stack/path/storage key, document/OCR/AI/Issue payload. |
| Graceful shutdown | Stop new mutation/upload/job admission, finish bounded in-flight requests, make durable owner/Audit outbox intent, close streams/connections, and return safe retry guidance. |
| Version endpoint | `GET /api/compatibility` returns `CurrentVersion`, `MinimumSupportedVersion`, `ContractVersion`, `CapabilityVersion`, status, and supported API major. |
| Feature flag | Typed/versioned configuration through official control plane; permission/ownership/policy remains enforced per request. |
| Rollback | Previous artifact only with API/config/database backward compatibility; server DB migration remains separate, approved pipeline step. |
| Deployment verification | Identity/authz negative tests, compatibility response, feature discovery, health, SQL/storage/queue/Audit, upload/download, idempotency, safe errors, rate/backpressure, public-AI negative check, monitoring, and representative Core path. |

## 2.4 Server Worker host

| Input | Recommendation |
|---|---|
| Artifact | Immutable .NET Worker artifact plus closed handler registry, engine/native/model manifest, SBOM, digest/provenance, and supported Job/contract/schema versions. |
| Startup dependency | Queue/claim source, persisted Job state, SQL/object storage, Audit/outbox, approved engine/provider, temp/work root, resource controls, and observability. |
| Configuration | Worker pool, handler allowlist, resource/time/temp/output/concurrency bounds, retry/checkpoint policy, engine/provider profile codes, and graceful-drain timeout. |
| Secret | Workload-scoped DB/queue/storage/provider secret only from approved runtime boundary; no end-user token. |
| Service identity | Identity distinct from API/deployment/human; queue claim, exact Job metadata, scoped object, provider purpose, and Audit append only as required. |
| Health/readiness | Internal/operations-only heartbeat and readiness with safe reason/freshness; no public control port. A Worker not ready stops claims. |
| Logging | Job/attempt/lease, Feature/Correlation, handler/engine profile, status, duration, bounded resource counters, safe error code; no input/output content or storage path. |
| Graceful shutdown | Stop new claims; complete only work that can finish within safe boundary; otherwise checkpoint/cancel/release lease according to policy; reject stale completion; flush outbox. |
| Version endpoint | Restricted heartbeat/registry projection or safe operations endpoint; include app build, handler registry, contract/capability, engine profile version, and freshness. No public endpoint. |
| Feature flag | Eligibility/admission resolved from approved profile and persisted Job; Worker does not trust queue message as authority or invent handlers. |
| Rollback | Drain affected pool, deploy last compatible signed artifact/profile, preserve one-active-attempt/lease/checkpoint semantics; no mass retry. |
| Deployment verification | Queue claim/lease, handler allowlist, workload authorization, engine/provider readiness, object/output validation, crash/recycle, checkpoint compatibility, no user token/public AI, bounded resources, stale completion rejection, and queue drain. |

## 2.5 Server migration boundary

- SQL Server migration is produced, reviewed, and executed by the approved pipeline/deployment process—not API/Web/Worker startup.
- Use expand/contract compatibility when two server versions can coexist during rolling deployment.
- Validate pre-change recovery point, migration idempotency, module permissions, constraints/indexes, outbox/idempotency, object reconciliation, and rollback/forward-fix decision.
- Do not downgrade database schema destructively merely to make an older binary start.

---

# 3. Desktop packaging

## 3.1 Baseline package contents

The Windows Desktop package should declare and version:

- `Pertamina.Oto.Desktop` WinUI 3 application;
- Local Worker executable and typed IPC contract version;
- approved .NET/Windows App SDK runtime dependency model;
- approved managed/native PDF, image, OCR, conversion, and model dependencies applicable to Desktop;
- feature/engine registry manifest;
- localization resources;
- non-secret default configuration and configuration schema;
- SQLite schema/migration bundle;
- icons, protocol/file-association declarations if approved;
- license/notices, SBOM, package digest, publisher/signing evidence, build/release ID;
- last-known-good/rollback compatibility metadata where supported.

No package may include server/database/storage/provider credential, private signing key, Production content, raw test data, public-AI endpoint fallback, or environment secret.

## 3.2 Candidate analysis

| Candidate | Strengths to validate | Constraints/PoC required |
|---|---|---|
| MSIX packaged WinUI 3 | Strong app identity, declarative package, signing/integrity, clean component inventory, association declarations, managed update compatibility | Enterprise distribution fit, offline dependencies, per-user/machine scope, Local Worker/native engines, package size, data preservation/uninstall behavior, downgrade/repair, corporate endpoint policy |
| Approved enterprise installer alternative | Potentially flexible scope, prerequisite/bootstrapper, offline media, native/engine placement, managed deployment integration | Must preserve stable app identity/security, transactional install, signing, repair/update/uninstall safety; avoid privileged custom action and arbitrary script |
| Unpackaged/signed application | May support constraints not met by packaged model | Higher burden for identity, secure storage, IPC ACL, update integrity, associations, cleanup, rollback; use only with evidence and approval |

MSIX is a candidate, not a final choice.

`REQUIRES_PERTAMINA_DESKTOP_DISTRIBUTION_CONFIRMATION`

## 3.3 Desktop package decisions to confirm

- stable Package/App/User Model identity across update;
- per-user installation as least-privilege candidate versus machine-wide enterprise need;
- supported CPU architectures and minimum Windows release derived from .NET 10, WinUI 3, Windows App SDK, drivers, and engine support matrix;
- framework-dependent versus self-contained runtime model;
- online managed distribution versus signed offline bundle;
- Desktop shortcut/start-menu behavior;
- repair, side-by-side pilot, downgrade, and uninstall behavior;
- organization/publisher display, install UI, localization, and support reference;
- policy for local logs, app-private data, update cache, and diagnostic export.

Machine-wide install or privileged helper/service must not be selected merely for convenience. If a privileged component is required, its exact capability, identity, ACL, update, audit, and attack surface need a separate approval.

---

# 4. Local Worker packaging

## 4.1 Recommended coupling

Default recommendation: **ship and update the Local Worker in the same signed Desktop release cohort**. This gives one source of truth for:

- Desktop app version;
- Local Worker binary/version;
- `IpcContractVersion`;
- handler/operation allowlist;
- engine/provider package versions;
- working-root schema;
- supported checkpoint version;
- rollback compatibility.

A separately serviced Worker is acceptable only if the distribution system enforces dependency ranges, atomic compatibility checks, signing, rollback, and no mixed-version unsafe startup.

## 4.2 Runtime boundary

- Worker runs as the intended current user or a stricter approved local identity; not Administrator by default.
- Desktop launches/binds the intended Worker instance. Do not install a high-privilege Windows service without a demonstrated requirement.
- No public/listening TCP port. Named Pipe/local IPC only, with remote access denied.
- Protected current-user/app/package/process ACL where supported; PID/pipe-name randomness alone is not authentication.
- Versioned authenticated handshake before operation dispatch.
- Closed allowlist accepts only canonical Feature ID, typed parameters, job-scoped artifact references, deadline/budget, and Correlation ID.
- Unique protected per-profile/job work root, random names, manifest, quota, traversal/reparse defense, no arbitrary filesystem/registry/SQL/shell/network/provider access.
- Heartbeat, hard frame/size/rate limits, cancellation, timeout, resource limits, crash containment, and recycle behavior.
- Partial output never becomes final; source remains unchanged; recovery uses compatible/integrity-valid checkpoint only.

## 4.3 Update and rollback

```text
Desktop package staged
→ package signature/digest verified
→ Desktop + Worker compatibility manifest checked
→ stop new local jobs
→ checkpoint/cancel at safe boundary
→ install cohort atomically where supported
→ validate Worker signature/version/IPC handshake
→ validate handler/engine registry
→ resume only compatible jobs
```

On failure, do not run a new Desktop against an old incompatible Worker or vice versa. Enter clear recovery/update-required state, preserve workspace, and use a signed compatible cohort.

---

# 5. Desktop install/update

## 5.1 Installation flow

```text
Receive package from approved channel
→ verify publisher/signature/integrity and package policy
→ check supported Windows/architecture/runtime/dependencies/storage
→ show version, size, scope, restart, local-data behavior, and permissions
→ install application + Local Worker cohort
→ create protected profile roots/config/log boundaries
→ first-run validation
```

Signed offline installation may be supported through an approved enterprise bundle containing all required runtime/native/engine dependencies, manifest, license, digest, and verification procedure. It must not reach public package/model sources silently.

## 5.2 Compatibility status behavior

| Status | Desktop behavior |
|---|---|
| `SUPPORTED` | Continue normally; refresh effective feature/capability state. |
| `UPDATE_AVAILABLE` | Inform user; allow deferral according to policy; do not mislabel as security block. |
| `UPDATE_REQUIRED` | Explain exact security/compatibility need safely; block incompatible server action; preserve local work. Eligible local capability may continue only when current offline/session/policy and local security remain valid. |
| `UNSUPPORTED` | Do not call incompatible server contracts; provide approved update/support path; preserve user state and avoid reset. |

Forced update is limited to genuine security/compatibility need. A configuration/marketing preference is not sufficient.

## 5.3 Update flow

```text
Check compatibility
↓
Notify user
↓
Download through approved channel
↓
Verify signature/integrity
↓
Preserve local state
↓
Install
↓
Migrate SQLite
↓
Verify
↓
Rollback/recovery bila gagal
```

Detailed requirements:

1. `GET /api/compatibility` provides compatibility facts, not an arbitrary executable URL.
2. The approved enterprise distribution channel or fixed integrity-controlled update service supplies the package.
3. The client does not execute arbitrary URL/package/command from server, AI, document, Issue, or user input.
4. Show version, package size, restart requirement, changed features, compatibility reason, local-state effect, known limitation, and action required.
5. Stop/cancel/checkpoint active work at a safe boundary; never corrupt a source or publish a partial result.
6. Preserve protected SQLite/workspace/draft/result/signature state according to the data contract before install.
7. Verify package publisher/signature/digest/version/architecture/dependency and anti-downgrade policy before activation.
8. Run versioned local migration, reconciliation, Worker handshake, feature registry, compatibility, and smoke checks.
9. Mark update successful only after validation; otherwise enter recovery with actionable safe error and Correlation/support reference.
10. Prevent retry/update loops; back off and retain last-known-good package/reference where distribution technology supports it.

Self-update code that downloads and replaces binaries outside the approved enterprise channel is prohibited.

---

# 6. Desktop rollback

Rollback has three separate dimensions:

| Dimension | Safe rule |
|---|---|
| Application package | Activate only a previously approved, signed, compatible artifact; never arbitrary downgrade. |
| Local SQLite schema | Old binary may open data only if its declared schema range supports current schema. No blind downgrade/reset. |
| Workspace/artifacts | Preserve source, user output, drafts, checkpoints, and manifest. Reconcile compatible state; never delete to make rollback appear successful. |

## 6.1 Failure decision

| Failure point | Recommended recovery |
|---|---|
| Before package activation | Keep current version; discard verified staging/update cache according to lifecycle. |
| After package activation, before local migration | Reactivate last-known-good signed package if supported and compatible. |
| During migration before commit | Restore protected pre-migration snapshot/journal after integrity validation; no empty/new DB fallback. |
| After successful migration without new user writes | Rollback only if tested reverse/restore path exists; otherwise use compatible forward-fix. |
| After migration with new-version user writes | Prefer forward-fix; old binary must not open unsupported schema. Preserve all data and enter recovery/read-only mode as designed. |
| Worker handshake/engine validation fails | Keep Desktop in explicit limited/recovery state; do not dispatch to mismatched Worker; repair/reinstall compatible cohort. |
| API compatibility fails | Preserve local work; block incompatible server operations; local eligible behavior follows valid policy. |

Rollback acceptance:

- package authenticity still valid and not revoked;
- no schema/data loss or profile/account mix-up;
- Local Worker/IPC/engine versions compatible;
- no partial output, duplicate job, silent upload, or stale server command;
- user receives clear state and next action;
- failed version evidence is preserved safely for Incident/Defect handling.

---

# 7. Desktop uninstall/data preservation

## 7.1 Data inventory and default posture

| Data class | Ownership/sensitivity | Uninstall posture |
|---|---|---|
| Application Binary | Product artifact | Remove with package. |
| Local Worker/runtime/engine binary | Product artifact | Remove with compatible Desktop cohort; remove unused update/model cache after reference check. |
| Local SQLite/WAL/SHM/migration journal | Protected per-profile metadata | Do not silently reset. Preserve for app-only/reinstall option or remove through explicit sensitive-data cleanup/policy. |
| Local Workspace | User working state managed by app | Preserve unless user/admin policy explicitly chooses app-data cleanup; identify only manifest-owned root. |
| Draft/checkpoint/recovery | Potential unsaved user work | Preserve by default or require explicit informed cleanup; never treat as disposable cache. |
| Temp | App-managed transient state | Eligible for bounded manifest-based cleanup after confirming no active/recovery/hold reference. |
| Cache/thumbnail/preview | Reconstructible but source-classified | Eligible for “remove app cache”; do not recursively delete user paths. |
| User Output / Save As | User file outside managed root or explicitly exported | Always preserve; never remove during uninstall. |
| Source file | External user source | Never modify or remove. |
| Secured local copy | Explicit local copy/working copy | Preserve unless explicit user action/policy; server secured lifecycle remains separate. |
| Secured cache metadata | Reconstructible account projection | May be removed with app cache/profile cleanup after credential/account separation. |
| Saved Signature Asset | Sensitive local non-PKI asset | Never auto-export/sync. Preserve or delete only by explicit sensitive-data choice/policy; no ordinary cache treatment. |
| Token/session credential | Credential, not user content | Remove/revoke through approved secure-store/SSO process where possible; do not preserve as ordinary reinstall data. |
| Offline grant | Protected time-bounded capability | Invalidate/remove according to approved uninstall/account policy; never convert to reusable token. |
| Local logs/diagnostics | Safe minimized operational data | Follow retention/incident hold; remove through controlled cleanup, not broad path deletion. |

## 7.2 User/admin options

The installer/distribution process should expose or enforce an approved equivalent of:

- **Remove app only** — remove binaries while preserving eligible local state/user files for reinstall;
- **Remove app cache** — remove app-managed temp/cache/thumbnail/update residue only;
- **Preserve user files** — always protect source, Save As output, explicit secured local copy, and unrelated files;
- **Sensitive local-data cleanup** — explicitly remove app-private profile data, SQLite, drafts/workspace, saved signature, offline state, and permitted logs according to policy and hold.

If the chosen package manager cannot present interactive uninstall choices, define an approved pre-uninstall/profile-cleanup workflow or managed policy. This behavior must be proved on the exact package technology; do not assume OS defaults preserve or remove the intended data.

## 7.3 Deletion boundary

- Cleanup resolves only manifest-owned app roots and rejects symlink/junction/reparse/path escape.
- Never recursively delete a user-selected source/output directory.
- Account logout/uninstall is not authority to delete server Secured Files.
- Incident/legal/investigation hold overrides ordinary cleanup where confirmed.
- Product claims only reference removal and OS delete request; absolute secure erase is not claimed for SSD/flash/copy-on-write/snapshot/backup.

---

# 8. SQLite migration

## 8.1 Migration contract

```text
detect app/profile/schema/artifact versions
→ validate package and migration compatibility range
→ acquire exclusive profile migration lock
→ preflight storage/power and required folder/key access
→ create protected profile-bound pre-migration snapshot/journal
→ apply ordered idempotent migrations
→ migrate/reconcile artifact manifest outside DB transaction as required
→ integrity/schema/profile/account checks
→ commit new schema marker
→ retain/expire snapshot by approved recovery policy
```

Requirements:

- each migration has from/to version, app compatibility range, transaction/restart behavior, expected space, and recovery step;
- database, WAL, SHM, migration backup/journal, temp, crash copy, and required artifacts share the approved protection boundary;
- key remains in approved OS credential/key facility, never beside data;
- no document/signature bytes, password, token, clipboard, or full OCR is introduced into ordinary SQLite;
- schema newer than app or corrupted DB produces explicit incompatible/recovery state, not silent reset;
- migration failure preserves original/snapshot and never creates an empty successful profile;
- external source and user output are never overwritten;
- artifact-manifest reconciliation detects missing/orphan/temp/checkpoint state without broad filesystem scan;
- login/account switch cannot open another profile;
- telemetry records only safe version/outcome/error/Correlation ID, not path/content.

## 8.2 Tests before release

- clean install/current schema;
- every supported previous version;
- no-profile and multiple isolated profiles;
- empty/normal/large state;
- active draft/checkpoint/undo history/pending Audit or secured transfer;
- low disk and permission/key-store failure;
- crash/restart at each migration phase;
- corrupted row/artifact/DB/WAL and missing artifact;
- update rollback/forward-fix and unsupported downgrade;
- account switch/logout/reinstall;
- local backup exclusion and no source overwrite.

Server SQL migration remains a separate pipeline-owned procedure; local SQLite migration at first run is not authority to run Production server migration.

---

# 9. Mobile packaging/distribution

## 9.1 Package baseline

The .NET MAUI Phone/Tablet application should produce signed platform-specific packages from the same controlled release manifest while preserving platform-specific:

- stable application/package identity and publisher/team binding;
- supported OS/device/architecture matrix;
- entitlements/capabilities and permission declarations;
- SSO redirect/app-link binding approved for Pertamina Identity;
- secure token/key storage adapter;
- protected SQLite/data/cache roots and backup-exclusion capability;
- camera/scanner, file picker, share/open-with, notification, orientation, touch/stylus behavior;
- curated Feature exposure and device capability adapter;
- approved native libraries/models/fonts/resources, license, size, and integrity;
- local migration, recovery, update, and uninstall semantics.

No App Store, Play Store, side-load method, enterprise signing program, or MDM product is selected here.

`REQUIRES_PERTAMINA_MOBILE_DISTRIBUTION_CONFIRMATION`

## 9.2 Security and privacy behavior

- Native SSO uses the approved PKCE/redirect flow; token/refresh credential stays in approved platform secure storage.
- Fresh installation offline cannot invent a session. Offline local capability requires a previously valid protected offline grant and policy.
- Camera/storage/photo/file access is requested at the point of explicit use with minimum OS scope; denial gives a useful alternative/explanation.
- Scanner staging is app-private; no automatic gallery save or cloud/device backup.
- Share/Open With input is untrusted and revalidated; receiving a file does not upload it.
- Notifications contain only safe status; no filename, content, OCR, signature, token, or sensitive Job detail.
- Local cache, drafts, signature, OCR, and full workspace do not auto-sync. **Amankan ke Akun** remains explicit.
- The application sandbox/root must be excluded from automatic consumer-cloud/device backup through an approved platform mechanism; inability to enforce the required policy is a release blocker for affected data.
- Uninstall behavior must distinguish app sandbox/secure credential removal from user-exported files and server Secured Files; exact OS behavior requires release PoC.

## 9.3 Minimum OS/device support

Do not hardcode a minimum OS from framework marketing support alone. Derive it from:

- .NET 10/.NET MAUI supported targets;
- Pertamina device fleet/security patch policy;
- SSO and secure-store behavior;
- backup exclusion/data protection;
- camera/file/share/notification APIs;
- native engine/model architecture;
- performance/resource and accessibility evidence;
- enterprise distribution/update mechanism.

---

# 10. Mobile update

```text
managed channel detects approved release
→ compatibility/minimum-version check
→ user/admin notification according to policy
→ package signature/integrity verified by approved platform process
→ active work reaches safe state
→ app update
→ protected SQLite migration and state reconciliation
→ SSO/secure-store/app-identity validation
→ capability/permission/feature refresh
→ smoke and recovery validation
```

Rules:

- `SUPPORTED`, `UPDATE_AVAILABLE`, `UPDATE_REQUIRED`, and `UNSUPPORTED` have the same semantic meaning as Desktop.
- Forced update is only for genuine security/compatibility need and uses approved managed distribution.
- Do not silently change entitlement/permission use; explain newly required camera/storage/notification capability.
- Preserve drafts/workspace/output during update; temporary cache cleanup must not remove recovery state.
- App identity/SSO redirect/secure-store key continuity is part of update acceptance.
- If update fails, preserve local data and show approved recovery path; do not reset SQLite/profile.
- If offline, an existing valid offline grant may allow eligible local features. Server-required/update-incompatible action waits or denies safely.
- A new install without previous protected grant requires online approved SSO before authenticated use.
- Do not download executable/model/package from public source as a workaround for unavailable enterprise distribution.

Channel, phased rollout, deferral, deadline, managed wipe, downgrade, and minimum-version enforcement:

`REQUIRES_PERTAMINA_MOBILE_DISTRIBUTION_CONFIRMATION`

---

# 11. Code-signing principles

Desktop and Mobile packages require approved enterprise integrity/signing. Server artifacts also require digest/provenance and signing/attestation if the approved pipeline standard requires it.

## 11.1 Required controls

- stable approved publisher/application identity;
- signing occurs in controlled pipeline/release boundary after tests and before publication;
- private signing key is stored in approved signing/KMS/HSM/service boundary and never exported to repository, developer workstation, prompt, config, package, or build log;
- signing workload identity is separate from developer/runtime/deployment identity and has minimum purpose;
- verify publisher, chain/trust, signature, artifact digest, version, architecture, and package manifest before install/update;
- record artifact, commit/build, SBOM, signer/certificate reference, signing time/status, and verification evidence without private material;
- define certificate issuance, approval, renewal, rotation, expiry, revocation, compromise, timestamping, and emergency re-sign/release process;
- block unsigned, modified, revoked, unexpected-publisher, or downgrade-violating package;
- ensure native libraries/engine/model packages have their own provenance/integrity chain and are covered by the release manifest;
- code/package signing remains unrelated to the product's visual Sign PDF feature, which is non-PKI.

Exact certificate authority/profile, key custody, signing service, algorithm, timestamp, trust deployment, revocation, and dual-sign/rotation procedure:

`REQUIRES_PERTAMINA_CODE_SIGNING_CONFIRMATION`

---

# 12. Client configuration

## 12.1 Configuration classes

| Class | Examples | Client handling |
|---|---|---|
| Compiled safe defaults | UI defaults, localization resource, schema version, hard safety ceiling | Signed package; lower layer cannot weaken security/policy. |
| Bootstrap non-secret | API base address, environment/channel code, compatibility metadata source, support reference | Integrity-controlled; environment-specific; not arbitrary user-editable in Production. |
| Authenticated effective configuration | Feature exposure, safe policy/limits, capability metadata, minimum version, approved provider profile status | Retrieve from official API; typed/versioned; filtered by platform/user/policy. |
| Device configuration | OS/device capabilities, storage availability, camera/touch/stylus, local Worker state | Locally measured; cannot override enterprise/system restriction. |
| User preference | Language, view, zoom, favorite, notification preference, supported save location | Per-profile; synced only if explicitly approved preference path. |
| Sensitive server configuration | Internal endpoint/topology, provider routing, detailed monitoring, storage/queue/DB settings | Server only; never client projection except safe profile/status code. |
| Secret | DB/storage credential, server/provider secret, private signing key, service key | Never shipped to or retrievable by client. |

Priority:

```text
System Safety/Security
→ Enterprise Policy
→ Environment Configuration
→ Device Configuration
→ User Preference
```

The most restrictive safe result wins.

## 12.2 Configuration rules

- bind to closed typed/versioned schema; reject unknown/invalid/high-risk value safely;
- separate configuration package/version from application version while declaring compatibility;
- protect API/update base addresses from local tampering and validate TLS fully;
- do not expose provider endpoint, credential, topology, storage path, connection string, or internal queue detail;
- cache only the minimum safe effective projection with provenance/version/expiry;
- reconnect/update re-fetches current configuration/permission/policy before queued server action;
- feature flag has owner, purpose, scope, environment, start/expiry/review, revision, and rollback;
- config change uses official control plane, optimistic concurrency, reauthentication where required, safe Audit, and staged verification;
- absent/invalid sensitive server config fails closed; no public service or hardcoded fallback.

---

# 13. Token/secret boundary

## 13.1 Client credentials

| Client | Approved boundary |
|---|---|
| Web | Opaque Secure/HttpOnly narrowly scoped session cookie and server-side credential boundary where applicable; no browser-accessible bearer/refresh storage. |
| Windows Desktop | Approved Windows secure OS credential storage/protected memory; package/app/profile identity binding. |
| Mobile/Tablet | Approved platform secure token/key facility; application/team identity and device/profile binding. |
| Offline | Separate protected, minimum, time-bounded offline capability projection; not access/refresh token or claim dump. |

Token/credential must never be stored in:

- plain file;
- ordinary SQLite field, WAL, migration journal, cache, or backup;
- app configuration/public bootstrap;
- URL/query/referrer;
- clipboard;
- log, telemetry, Audit, Issue, diagnostics, crash report, or screenshot;
- document, checkpoint, idempotency record, update manifest, package, or model cache.

## 13.2 Server and workload secrets

- API/Web/Worker identities are distinct and purpose-scoped.
- End-user token is not forwarded to Worker, storage, queue, engine, or provider.
- Runtime retrieves secret via approved environment-specific identity/secret boundary.
- Client never carries database credential, storage master credential, server secret, private signing key, or unrestricted provider credential.
- Secret exposure triggers controlled revoke/rotation and artifact/history/use review under Document 16; do not copy the secret into Incident evidence.
- Logout/account change clears in-memory/client credential state and invalidates offline authorization according to approved identity policy without deleting user source/output.

Exact SSO, OS secure-store adapter, credential migration across app-identity update, session revocation, and uninstall cleanup:

`REQUIRES_PERTAMINA_SSO_CONFIRMATION`  
`REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`

---

# 14. File association/share behavior

## 14.1 Candidate exposure

| Integration | Recommended posture |
|---|---|
| Desktop Open With | Register approved PDF and selected image/document types as a candidate handler only after exact capability and package identity validation. |
| Default PDF/image application | Do not take over silently. User or approved Administrator policy makes the explicit default-app decision. |
| Desktop drag/drop | Explicit foreground action; bounded number/size; validate final file handle, actual bytes/type, and permissions. |
| Mobile/Tablet Open With/share input | Accept only supported intent/type, copy to protected staging, validate actual bytes, and show selected Feature options. |
| Share target/output | Explicit user action; show destination/format/classification effect; never auto-upload or expose permanent link. |

## 14.2 Security and UX rules

- OS filename, extension, MIME, path, URI, share metadata, and provider app are untrusted.
- Reject traversal, UNC/device/ADS, link/reparse escape, unsupported scheme, fake extension, oversized/malformed input, and stale handle.
- File association invocation never becomes a shell command or arbitrary Worker operation.
- Opening a file creates a local working reference/copy according to platform policy; source remains unchanged.
- Open With/drag/drop/share does not grant server upload, **Amankan ke Akun**, AI/provider transfer, overwrite, or permanent operation.
- Multi-select is bounded and feature-compatible; unsupported combinations receive a clear explanation.
- Do not expose a dead button. Show `Unavailable on this platform`, `Requires connection`, `Requires update`, `Permission denied`, `Provider unavailable`, or other safe actionable state.
- Association changes during update preserve explicit user/admin default choice; do not reassert default silently.

Exact file types, verbs, icons, default-app policy, share-target availability, and enterprise association deployment:

`REQUIRES_PERTAMINA_DESKTOP_DISTRIBUTION_CONFIRMATION`  
`REQUIRES_PERTAMINA_MOBILE_DISTRIBUTION_CONFIRMATION`

---

# 15. Capability/version handshake

## 15.1 Version dimensions

| Version | Authority/use |
|---|---|
| `AppVersion` | Exact Desktop/Web/Mobile/API/Worker build identity. |
| `MinimumSupportedVersion` | Typed server policy for platform/channel; changed only for real security/compatibility need. |
| `ContractVersion` | API/OpenAPI request/response/event compatibility. |
| `CapabilityVersion` | Feature exposure/capability snapshot. |
| `ConfigurationSchemaVersion` | Client/server typed configuration interpretation. |
| `LocalSchemaVersion` | SQLite and local artifact-manifest compatibility. |
| `IpcContractVersion` | Desktop ↔ Local Worker handshake and message compatibility. |
| `Handler/EnginePackageVersion` | Worker handler, engine/native/model profile compatibility and provenance. |
| `FeatureParameterSchemaVersion` | Canonical Feature input/output/workflow parameter compatibility. |

## 15.2 Handshake flow

```text
verify local package/app identity/version
→ validate profile root, secure store, SQLite schema, recovery state
→ Desktop validates Local Worker version + authenticated IPC handshake
→ when online, call GET /api/compatibility with platform/version
→ evaluate SUPPORTED / UPDATE_AVAILABLE / UPDATE_REQUIRED / UNSUPPORTED
→ authenticate through approved Pertamina Identity when needed
→ retrieve /api/v1/configuration/effective
→ retrieve filtered /api/v1/features capability descriptors
→ combine platform/device/permission/policy/resource/engine/connectivity
→ expose, hide, fallback, wait, or explain
```

Effective availability:

```text
Deployed Platform Maximum
∩ Client/Contract Compatibility
∩ Device Capability
∩ Feature Flag/Environment Exposure
∩ Permission + Ownership + Policy
∩ Engine/Provider/Resource Availability
∩ Connectivity/Offline Eligibility
```

Rules:

- compatibility endpoint is stable safe pre-auth metadata; feature/config projection remains authenticated and filtered;
- cached capability is UX hint, not authorization or server health truth;
- reconnect/update/permission change invalidates stale projection and queued server authorization;
- Local Worker handshake failure cannot be bypassed by raw process/pipe call;
- unknown/newer schema/version fails safely with actionable update/recovery state;
- UI never enables an unsupported Feature merely because Feature ID exists;
- UI explains unavailable states without revealing internal topology, credential, hidden Feature, or sensitive policy detail.

---

# 16. Engine/provider packaging

## 16.1 Package inventory

Every managed library, native binary, codec, font, engine, OCR resource, ML/AI model, helper tool, or image/base dependency should record:

```text
ComponentId / Version / Supplier / ApprovedSource
DirectOrTransitive relationship
License and redistribution terms
Supported platform / OS / architecture
Artifact digest / signature / provenance / SBOM reference
Client/Worker/contract/capability compatibility range
Feature IDs and handler profile
Package/download/install size
CPU/RAM/temp/disk/thread/child-process/network budget
Data classification and egress behavior
Working/model/cache location and protection
Update, rollback, fallback, removal, and support policy
Known vulnerability/exception/expiry status
```

## 16.2 Delivery and activation

- Prefer bundling the approved engine with the signed release when size/license/platform allow.
- Optional/on-demand components may be obtained only from an approved enterprise artifact source through a fixed manifest and authenticated/integrity-verified channel.
- Never download AI/model/engine from an arbitrary public URL, model hub, document-provided link, or provider response.
- Stage to protected app-managed storage, verify signature/digest/size/license/compatibility, then activate atomically.
- Maintain current and last-known-good version only according to storage/rollback policy; cleanup is manifest/reference-aware.
- Engine/model selection remains behind Engine Resolver and approved configuration. UI/Assistant/user cannot choose vendor or path.
- Fallback is a pre-registered approved compatible provider with the same permission, classification, resource, privacy, and output controls—not a dynamic public fallback.
- Removal must confirm no active Job/checkpoint/rollback/reference and must not delete user source/output.
- Native child/helper process runs least privilege with bounded work root/resources/egress and code-integrity validation.

## 16.3 Update acceptance

- license and platform support still valid;
- vulnerability/dependency/SBOM scans pass or approved time-bound exception exists;
- functional fidelity, malicious input, resource, crash/recycle, output validation, and recovery PoC pass;
- Desktop/Worker/IPC/checkpoint compatibility declared;
- package size/storage/update time fit the target device/server budget;
- rollback or safe disable path exists;
- no public AI/model download, secret embedding, or content logging.

Final engine/provider/model source, redistribution, update cadence, cache path, and fallback policy:

`REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`

---

# 17. First-run validation

## 17.1 Desktop first run

```text
1. Verify package/app identity, publisher, AppVersion, architecture
2. Validate supported OS/runtime/dependency and required storage headroom
3. Create/open protected per-profile folders and log boundary
4. Validate OS secure token/key store adapter
5. Detect LocalSchemaVersion and run/recover SQLite migration
6. Reconcile local workspace/artifact/recovery manifest
7. Verify Local Worker signature/version/IPC/heartbeat/handler registry
8. Load localization and non-secret bootstrap configuration
9. If online, check API compatibility and approved SSO/session
10. Retrieve effective configuration and filtered Feature registry
11. Evaluate engine/provider/device/offline capability
12. Surface recovery/update/unavailable state and safe next action
```

## 17.2 Mobile/Tablet first run

```text
1. Verify app/package identity, AppVersion, OS/device/architecture
2. Validate protected app data, backup exclusion, secure token/key store
3. Detect/migrate/recover SQLite and local artifacts
4. Load language/localization and safe bootstrap config
5. Check API compatibility when online
6. Perform approved SSO or validate existing protected offline grant
7. Resolve curated Feature/device capability
8. Request camera/storage/share/notification permission only when used
9. Show update/recovery/limited state clearly
```

## 17.3 First-run outcomes

| Condition | Behavior |
|---|---|
| Fresh install, online | Compatibility → approved SSO → effective configuration/features → normal onboarding. |
| Fresh install, offline | No fabricated login/offline grant; explain online sign-in required. Local unauthenticated behavior only if explicitly required elsewhere. |
| Upgrade, offline, valid prior grant | Eligible local Features remain available within absolute expiry/policy; no server call/upload; server-dependent action waits. |
| SQLite migration blocked/corrupt | Preserve DB/workspace, enter recovery; no reset/empty profile. |
| Local Worker unavailable | Desktop remains alive; affected heavy/local-worker Features show repair/unavailable explanation. |
| API `UPDATE_REQUIRED` | Preserve state, block incompatible server action, present approved update flow; local safety policy determines eligible local continuation. |
| Provider/engine unavailable | Keep unaffected/local-safe capability; no public fallback. |
| Secure store unavailable | Fail authentication/secret-dependent operation safely; do not store token in file/SQLite. |
| Recovery residue found | Reconcile manifest/checkpoint and show recover/discard choice only where policy and state support it. |

## 17.4 Communication checklist

Install/update/first-run message includes, when relevant:

- application version/channel and publisher;
- package/download size and storage need;
- restart or sign-in requirement;
- changed/added/removed/limited Feature behavior;
- compatibility/minimum-version status;
- local SQLite/workspace/draft/signature effect;
- permission/capability required at point of use;
- known limitation and offline behavior;
- safe error/support reference and action required.

Do not expose internal endpoint/topology, provider credential/profile detail, raw migration/stack/path, package source token, or security weakness detail.

---

# 18. Pertamina confirmation gaps

## 18.1 Desktop distribution

`REQUIRES_PERTAMINA_DESKTOP_DISTRIBUTION_CONFIRMATION`

- MSIX versus approved enterprise installer/unpackaged model;
- per-user versus machine-wide installation;
- enterprise software distribution/update/offline-media channel;
- app/package identity, publisher, installation path, shortcut, repair, downgrade, rollback, side-by-side/pilot;
- minimum Windows/architecture/runtime strategy;
- file association/default-app policy;
- exact uninstall/profile/cache/signature/workspace preservation behavior;
- managed update notification, deferral, forced-update, and restart policy.

## 18.2 Mobile distribution

`REQUIRES_PERTAMINA_MOBILE_DISTRIBUTION_CONFIRMATION`

- supported mobile/tablet OS, architecture, fleet, and minimum version;
- enterprise distribution/managed update/side-load/store channel without product assumption;
- application identity, signing/team profile, SSO redirect/app-link, entitlement, device compliance;
- rollout/ring, deferral/deadline, forced update, managed wipe, and uninstall behavior;
- backup exclusion, secure-store, local-data retention, and permission policy.

## 18.3 Code signing

`REQUIRES_PERTAMINA_CODE_SIGNING_CONFIRMATION`

- certificate/identity authority, profile, publisher name, key custody, signing service;
- algorithm/key size, timestamp, trust distribution, expiry/renewal/rotation/revocation;
- emergency compromised-certificate and package re-sign/re-release process;
- server artifact/container signing/attestation applicability.

## 18.4 Infrastructure and operations

`REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`

- server OS/host/process manager, web server/reverse proxy, TLS termination, DNS, load balancer, container applicability, network segmentation;
- service identities, secret/key service, environment configuration injection, access/SoD;
- deployment strategy/rings, maintenance window, health gate, rollback/forward-fix, emergency change;
- SQL migration tool/authority, recovery point, RPO/RTO, backup/restore/DR;
- artifact repository, SBOM/provenance format, retention, model/engine distribution;
- monitoring/APM/SIEM routing, readiness thresholds, graceful shutdown/drain timeout;
- SSO/client secure-store integration and offline grant policy;
- numeric package/update/cache/log/storage limits and retention;
- user/admin communication and support route.

These gaps do not allow unsigned packages, public model download, embedded secrets, silent user-file deletion, startup Production DB migration, or uncontrolled self-update.

---

# 19. Risks/conflicts

| Risk/conflict | Impact | Recommendation |
|---|---|---|
| MSIX advantages versus Local Worker/native engine/offline/uninstall constraints | Package may not support required lifecycle as assumed | Treat MSIX as candidate; run install/update/rollback/uninstall/native-worker PoC before decision |
| Stable app identity changes across channel/update | Secure token store, profile isolation, IPC ACL, association, or SSO redirect can break | Lock identity/publisher per approved channel; test migration and cross-channel block |
| Machine-wide install used for convenience | Privilege escalation and larger blast radius | Prefer per-user least privilege candidate; require explicit enterprise need for privileged component |
| Desktop and Worker serviced separately | Mixed versions, unsafe IPC/checkpoint/output | One release cohort by default; explicit dependency range and atomic handshake if separated |
| Binary rollback after SQLite migration | Old client corrupts or cannot read new state | Declare schema compatibility; prefer forward-fix after new writes; no silent downgrade/reset |
| `UPDATE_REQUIRED` interpreted as full data lock | User loses access to safe local work | Block only incompatible/unsafe path unless security policy requires wider block; preserve workspace and explain |
| Offline fresh install expected to log in | Authentication cannot be proven | Require initial approved online SSO; offline only with prior protected grant |
| Public API base address treated as harmless editable text | Client can be redirected to malicious server | Non-secret but integrity-controlled and environment-bound; full TLS validation |
| Package manager uninstall removes app data differently from design | Draft/signature/profile loss or residue | Validate exact technology; provide pre-uninstall policy/workflow and data inventory |
| “Remove app cache” recursively targets broad path | Source/output deletion | Manifest-owned root only; path/reparse checks; no user-selected directory recursion |
| Saved signature treated as ordinary cache | Sensitive asset leak or unexpected deletion | Separate protected lifecycle and explicit cleanup choice/policy |
| Token preserved for reinstall | Credential exposure after uninstall/device handover | Remove/revoke credential through approved process; preserve user state separately |
| Code-signing key in repository/pipeline variable/log | Supply-chain compromise | Approved isolated signing boundary; workload identity; no key export; rotate/revoke procedure |
| Engine/model on-demand download from public source | Supply-chain/data-policy breach | Approved enterprise artifact source, signed manifest/digest/provenance, no arbitrary URL |
| Engine/model package size and native architecture | Failed install, disk/RAM pressure, device exclusion | Per-platform manifest, headroom preflight, optional approved components, measured support matrix |
| Engine fallback changes data route | Public/unapproved AI or privacy bypass | Only registered approved fallback with identical policy/security checks |
| Mobile OS backup cannot be excluded reliably | Signature/draft/OCR/token leakage | Platform PoC and enterprise policy; block affected capability/release if control unavailable |
| File association takes default after update | User/admin policy violation | Register Open With only; preserve explicit default choice |
| Server hosts co-located on small initial server | Resource starvation/shared failure | Keep deployable boundaries, budgets, readiness/backpressure, and measurement-driven split |
| Rolling API/Worker deployment with breaking schema | Old/new instances corrupt shared state | Expand/contract migration, compatibility matrix, staged drain, forward-fix plan |
| Worker/health endpoint made public for convenience | Attack and topology disclosure | Internal operations boundary only; no public control port |
| Client exposes provider/feature internals | Security detail leak and user selecting vendor | Safe profile/status only; Engine Resolver and server-filtered exposure remain authority |
| Forced update used as broad administrative control | Availability and offline disruption | Require documented security/compatibility reason, impact preview, staged validation, rollback, Audit |
| Emergency patch bypasses pipeline | Untracked artifact/config and lost evidence | Accelerated governed pipeline, immutable artifact, tests/scans, approval, monitoring, PIR |

No architecture or role change is required. Package/channel/vendor selections remain open confirmation items.

---

# 20. Suggested Document 17 text

The following text is suitable for controlled integration into master Document 17 after Integrator review.

## Application Packaging, Installation, Configuration, and Client Update

PERTAMINA OTO has six deployable application hosts: Web, API, Server Worker, Windows Desktop, Local Worker, and Mobile/Tablet. SQL Server, secure object storage, queue, Identity, monitoring, and approved provider/engine services are dependencies, not client-embedded components.

All release artifacts are generated by a controlled pipeline from approved sources. Each release has an immutable artifact, commit/build reference, application and contract/config/migration versions, digest, SBOM/provenance, test/security evidence, and rollback/forward-fix declaration. The same binary is promoted between environments where the host format permits; environment configuration and secrets are injected separately. Production does not allow startup database migration, manual binary replacement, untracked configuration, debug detail, embedded secret, or public-AI fallback.

### Server hosts

**Web** packages the approved application/static assets, localization, safe bootstrap configuration, and version manifest. Browser/static content contains no secret. If the Web host has a server-side session boundary, its workload identity and session/key material are separate and runtime-injected. Final web server, reverse proxy, TLS termination, container, and hosting topology require infrastructure approval.

**API** packages the .NET application, OpenAPI/contract manifest, configuration schema, and release evidence. It validates typed configuration and runtime secret resolution. `/health/live` proves process liveness; `/health/ready` reports only safe readiness. `GET /api/compatibility` remains the client compatibility authority. Shutdown stops new mutation/upload/job admission, lets bounded work reach a durable boundary, preserves owner/Audit outbox intent, and releases connections safely.

**Server Worker** packages a closed handler registry plus approved engine/native/model dependencies. It uses a dedicated purpose-scoped workload identity, reloads authoritative persisted Job state, accepts no end-user token, and has no public control endpoint. Shutdown stops new claims and completes, checkpoints, cancels, or releases the lease at a safe boundary. Rollback preserves one-active-attempt, stale-completion rejection, checkpoint compatibility, and queue reconciliation.

Server database migration is an approved pipeline/deployment operation, separate from Web/API/Worker startup. Use backward-compatible expand/contract changes where versions overlap, a verified recovery point, idempotent scripts, permission/constraint/outbox/object checks, and an explicit rollback or forward-fix decision.

### Windows Desktop and Local Worker

Windows Desktop uses WinUI 3. Its package includes or declares the Desktop application, Local Worker compatibility cohort, approved runtime/native/engine dependencies, feature/engine manifest, localization, safe configuration schema, SQLite migrations, license notices, SBOM, digest, and signing evidence.

MSIX and approved enterprise-installer alternatives are candidates only. The final technology, distribution channel, installation scope, minimum OS, runtime model, offline package, repair, update, file association, rollback, and uninstall semantics require:

`REQUIRES_PERTAMINA_DESKTOP_DISTRIBUTION_CONFIRMATION`

Local Worker is installed and updated with the Desktop release by default. It runs as the intended current user or stricter approved identity, not Administrator or a high-privilege service by convenience. It exposes no public port; communication uses authenticated, versioned local IPC with restrictive ACL and an instance/job binding. Only canonical Feature IDs, typed parameters, job-scoped artifact references, deadlines/budgets, and Correlation IDs are accepted. Arbitrary filesystem, SQL, shell, registry, network, provider, or secret access is prohibited. Worker crash does not terminate Desktop; partial output never becomes final.

### Desktop installation and update

The approved installation flow verifies package publisher/signature/integrity, version, OS/architecture/runtime/dependency, storage headroom, scope, and local-data behavior before installing Desktop and Local Worker as a compatible cohort. Offline installation uses only an approved signed bundle containing all required dependencies; it never downloads engines/models from a public source.

Compatibility states are `SUPPORTED`, `UPDATE_AVAILABLE`, `UPDATE_REQUIRED`, and `UNSUPPORTED`. Forced update is used only for genuine security or compatibility need. `UPDATE_REQUIRED` blocks incompatible server action and preserves local state; eligible local capability may continue only if current security/offline/session/policy remains valid.

```text
Check compatibility
→ Notify user
→ Download through approved channel
→ Verify signature/integrity
→ Preserve local state
→ Install
→ Migrate SQLite
→ Verify
→ Rollback/recovery if failed
```

`GET /api/compatibility` provides version facts and does not authorize an arbitrary package URL. Package acquisition occurs only through the approved enterprise channel. The application does not replace its own binaries through uncontrolled self-update. Before activation, it verifies publisher/signature/digest/version/architecture/dependency and applicable anti-downgrade policy. Active work reaches a safe checkpoint/cancel boundary, with source and finality protected.

Rollback activates only a previously approved signed compatible artifact. An older client cannot open a newer unsupported SQLite schema. If migration has committed and the user has produced new-version state, prefer a compatible forward-fix; do not reset or blindly downgrade data. Local Worker/IPC/engine compatibility is revalidated before job resume.

### Local data and uninstall

Application binaries and product dependencies are distinct from Local SQLite, workspace, draft, temp, cache, user output, source, secured local copy, saved signature, credential, offline grant, and logs.

- source and user Save As output are never removed;
- explicit secured local copies and drafts/workspace are preserved unless explicit approved cleanup is selected;
- temp/cache/thumbnail/update residue may be removed only from manifest-owned app roots when no recovery/hold reference exists;
- saved visual signature is a separate sensitive local asset, never ordinary cache or auto-export;
- token/session credential is removed/revoked through the approved secure-store/SSO process and never preserved as ordinary reinstall data;
- uninstall does not delete server Secured Files;
- cleanup never recursively deletes a user-selected path or follows link/reparse escape;
- no absolute secure-erase claim is made.

The distribution process provides or enforces approved equivalents for `Remove app only`, `Remove app cache`, `Preserve user files`, and `Sensitive local-data cleanup`. Exact behavior is proved on the chosen package technology.

### SQLite migration

Local migration validates package/profile/schema compatibility, obtains a profile lock, checks storage/key/folder readiness, creates a protected pre-migration snapshot/journal, applies ordered idempotent migrations, reconciles the artifact manifest, validates integrity/profile/account binding, and commits the new schema marker. DB/WAL/SHM/migration backup/temp/artifacts remain protected. Unknown newer schema or corruption enters recovery; it never silently creates an empty profile or overwrites external source.

### Mobile/Tablet

Mobile/Tablet uses .NET MAUI with signed platform-specific packages, stable application identity, approved SSO redirect, protected token/key store, SQLite migration, backup exclusion, device capability adapter, and least-privilege camera/file/share/notification declarations. Permissions are requested at the point of use. Share/Open With content is untrusted and does not imply upload. Scanner, cache, draft, signature, OCR, and workspace do not auto-sync.

No App Store, Play Store, side-load, enterprise signing, or MDM product is selected without approval:

`REQUIRES_PERTAMINA_MOBILE_DISTRIBUTION_CONFIRMATION`

Mobile updates use the approved managed channel, verify package integrity, preserve active work/local state, migrate SQLite, validate app identity/SSO/secure store, refresh capability and permission, and recover without reset on failure. Fresh offline installation cannot invent authentication; offline local use requires a prior valid protected grant.

### Code signing

Desktop and Mobile packages use the approved enterprise signing/integrity process. Private signing keys are never stored in source, prompt, workstation file, package, config, or log. Signing uses a separate least-privilege workload and controlled key/signing boundary. Install/update rejects unsigned, modified, revoked, unexpected-publisher, or disallowed downgrade artifacts. Certificate authority/profile, key custody, algorithm, timestamp, trust, renewal, rotation, revocation, and emergency re-sign process require:

`REQUIRES_PERTAMINA_CODE_SIGNING_CONFIRMATION`

Package signing is unrelated to the visual Sign PDF feature, which remains non-PKI.

### Configuration and credentials

Configuration priority is System Safety/Security → Enterprise Policy → Environment Configuration → Device Configuration → User Preference. The most restrictive safe result wins. Clients may receive integrity-controlled API base address, compatibility metadata, filtered feature exposure, non-secret policy, and localization. They never receive database/storage master credential, server/provider secret, private signing key, or unrestricted provider credential.

Web keeps browser credentials in an opaque protected server-session model where applicable. Desktop and Mobile use approved OS secure credential storage. Token is never stored in ordinary SQLite, file, config, URL, clipboard, log, telemetry, Audit, Issue, diagnostics, crash report, or package. Offline grant is a separate protected, narrow, time-bounded capability projection—not an access token.

### Association, capability, and first run

Desktop may register `Open With` for approved PDF/image types, and Mobile may receive approved file/share intents. The application never takes over the default handler without explicit user or Administrator policy. Every input is revalidated by actual bytes/type/size/path safety and staged privately. Open With, drag/drop, or share does not authorize server upload, AI transfer, overwrite, or **Amankan ke Akun**.

Startup validates package/app version, protected local schema/folders, secure store, Local Worker and IPC on Desktop, feature registry, language, recovery state, and update status. When online, the client calls compatibility, performs approved SSO, retrieves effective configuration and filtered Features, then computes availability from platform, version, device, permission, policy, engine/provider/resource, and connectivity. Cached capability is not authorization. Unavailable controls show a safe explanation rather than an unexplained dead button.

### Engine/provider packages

Every managed/native library, engine, model, font, and helper has an approved source, version, license, platform/architecture, digest/signature/provenance, SBOM, compatibility range, resource/data/network behavior, storage/cache lifecycle, update, rollback, fallback, and removal plan. Optional components come only from an approved enterprise artifact source using a fixed integrity-verified manifest. No AI/model/engine package is downloaded from an arbitrary public source. Fallback is pre-approved and enforces the same permission, classification, privacy, capacity, and output validation.

### Confirmation boundary

Final server hosting/topology, service identity, secret store, deployment strategy, migration/backup/DR, artifact repository, monitoring, engine distribution, numeric limits, and operational procedure remain:

`REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`

These open decisions do not permit unsigned packages, uncontrolled self-update, public model download, embedded server secrets, silent user-file deletion, high-privilege Local Worker, public Worker port, or startup Production database migration.

---

**Keputusan tetap milik user dan Integrator.**
