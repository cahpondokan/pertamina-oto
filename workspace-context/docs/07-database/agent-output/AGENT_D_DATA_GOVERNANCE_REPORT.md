# AGENT D — DATA GOVERNANCE + TRACEABILITY REPORT

**Task ID:** `DOC07-AGENT-D`  
**Scope:** database governance, security boundary, data lifecycle, dan traceability  
**Status:** Rekomendasi untuk Integrator; tidak mengubah master Document 07 atau Role Matrix  
**Tanggal:** 24 Agustus 2026

## Baseline dan guard

Laporan ini mengaudit:

- `01_BUSINESS_PROCESS_FINAL_v3.4_PERTAMINA_OTO.md`;
- `02_REQUIREMENT_SPECIFICATION_FINAL_v1.5_PERTAMINA_OTO.md`;
- `03_FEATURE_CATALOG_FINAL_v1.3_PERTAMINA_OTO.md`;
- `04_SOLUTION_DESIGN_FINAL_v1.2_PERTAMINA_OTO.md`;
- `05_TECHNICAL_REFERENCE_FINAL_v1.2_PERTAMINA_OTO.md`;
- `docs/06-api/06_API_DOCUMENTATION_WORKING.md`, yaitu baseline 06 terintegrasi yang tersedia di workspace.

Dokumen 06 yang tersedia masih bernama `WORKING`, berstatus `WORKING — DECISION PHASE`, versi 0.5, dan menyatakan final cross-document audit belum selesai, walaupun Decision Register sudah 9/9. Laporan tetap memakai keputusan 9/9 tersebut sebagai baseline yang tersedia dan mencatat status ini sebagai inkonsistensi untuk Integrator.

Guard yang dipertahankan:

- tidak membuat role baru dan tidak mengubah Role Matrix sebelum Document 09;
- istilah Superadmin/Support diperlakukan sebagai **permission-gated support surface**, bukan role baru atau authorization bypass;
- setiap data/table mempunyai satu owner module; modul lain memakai contract/application service, bukan direct SQL;
- SQL menyimpan metadata/reference; payload file memakai local filesystem atau secure file/object storage;
- password tidak disimpan; token/secret tidak menjadi ordinary database field;
- document/OCR/signature content tidak masuk audit, log, telemetry, atau safe diagnostics;
- User History, Enterprise Audit, Support Issue, dan Observability adalah data plane berbeda;
- retention numerik tidak dibuat tanpa policy/benchmark/security/legal basis yang disetujui.

## 1. Data ownership matrix

### 1.1 Legenda

Kelas di kolom “Sensitive?” mengacu pada working classification di Bagian 2, bukan pengganti skema klasifikasi resmi Pertamina.

| No | Important entity | Owner module / schema | Server / local | Purpose | Sensitive? | Retention baseline | Deletable? | Auditable? | Syncable? | Feature / Requirement references |
|---:|---|---|---|---|---|---|---|---|---|---|
| 1 | User identity/profile reference | Identity Module / `identity.*` | Server; cache minimal local | Subject identity, display profile, ownership anchor | YES—`C` | IdP/account policy; local cache time-bounded | By authoritative identity lifecycle, not arbitrary app delete | YES—login/profile/security changes as metadata | Server-authoritative; cache refresh only | `FR-AUTH-001..005`; `FR-ROLE-001..003`; `FEAT-AUTH-001..012` |
| 2 | Server session/revocation record | Identity Module / `identity.*` | Server | Session validity and revocation | YES—`C` | Session/IdP policy | YES—expire/revoke by policy | YES—metadata only | N/A; server authoritative | `FR-AUTH-002..005`; `FEAT-AUTH-004..008`; `API-DEC-007` |
| 3 | Offline claim/permission cache | Identity client boundary | Local secure store | Narrow offline authorization while cache is valid | YES—`C` | Time-bounded policy; exact TTL unresolved | YES—logout, revoke-sync, expiry, device cleanup | Security events only; no claim dump | Refresh from server; never authoritative upload | `FR-AUTH-003..005`; `FEAT-AUTH-005`, `FEAT-AUTH-012`; `API-DEC-007` |
| 4 | PDF/file password | No persistence owner | Process memory only | Decrypt/open authorized file for current operation | **YES—`X`** | Process scope only | Purge immediately after use | **NO value logging/audit**; only safe outcome/error code | **NO** | `FR-FILE-015`; `SR-SEC-003`; `FEAT-VALID-008`; `FEAT-PDF-062` |
| 5 | Access/refresh token | Identity/security credential boundary | Approved secure OS/server session storage only | Authenticate session | **YES—`X`** | Credential lifetime/revocation policy | YES—revoke/expire/secure wipe | Token value **NO**; auth event metadata YES | Refresh via approved identity flow, not ordinary data sync | `FR-AUTH-002..005`; `SR-SEC-002`; `FEAT-AUTH-006..008`; `API-DEC-007` |
| 6 | Application secret, service credential, encryption key | Security/Key Management boundary | Approved secret/key store | Service authentication and cryptography | **YES—`X`** | Rotation/key policy | YES—rotate/revoke; never ordinary CRUD | Access/rotation metadata only | **NO ordinary sync** | `SR-SEC-010`; Solution Design §26; `API-DEC-007` |
| 7 | Enterprise policy/configuration/feature flag | Admin/Policy Module / `admin.*` | Server; effective subset cache local | Limits, classification behavior, retention, feature availability, safety | YES—`C/A` | Current + controlled history/audit per policy | Authorized change only; no silent delete | **YES—append audit metadata** | Server-authoritative; cache refresh | `FR-ROLE-001..003`; `FEAT-ADMIN-004..018`; `FEAT-PROC-011`; Solution Design §19 |
| 8 | Canonical Feature Registry and schema/version metadata | Feature Registry logical module; physical schema unresolved | Server/shared contract + packaged local snapshot | Canonical Feature ID, contract, parameter schema, capability metadata | `I/C` | Active version + compatibility/support window | Controlled migration/deprecation only | YES—change metadata | Versioned distribution, not user-content sync | `FR-ASSIST-003..005`; `IR-API-010`; `FEAT-ASSIST-009`, `FEAT-ASSIST-022`; Solution Design §14; `API-DEC-002..003` |
| 9 | Original/source file and ordinary local final result | Workspace Module / user filesystem | Local filesystem | Input and output user work | **YES—`R`** | User-controlled; app must not overwrite/delete silently | Only explicit user/file-system action; original protected | Process metadata only; never content | **NO**, unless result explicitly secured | `FR-STOR-001..006`; `FEAT-EDIT-001`, `FEAT-EDIT-008..012`; `FEAT-STOR-001` |
| 10 | Local source/result reference, including local path | Workspace Module / local SQLite | Local SQLite | Reopen/recover local workspace without copying source | YES—`C/R` | While workspace/history/recovery needs it | YES—workspace/history cleanup; no server cascade | Safe operation metadata only | **NO full path sync** | `FR-STOR-001..006`; `FR-TRASH-009`; `FEAT-STOR-001`; `TECH-DEC-011` |
| 11 | Working state, autosave draft, undo/redo operations | Workspace/Working State Module / local SQLite + local files | Local | Non-destructive editing and recovery | **YES—`R`** | Until final/cancel/user deletion/recovery policy | YES—local Recycle Bin or cleanup by lifecycle | Final/status metadata only; not every content delta | **NO automatic sync** | `FR-STOR-002..005`; `FR-UI-020..021`; `FEAT-EDIT-001..011`; `FEAT-STOR-002..003` |
| 12 | Local checkpoint, local job state, Activity Center projection | Jobs/Recovery + Activity client / local SQLite | Local | Resume, retry, startup recovery, unified status | YES—`C`; checkpoint may be `R` | Active/recovery window; exact duration unresolved | YES after terminal state and recovery window | YES—safe state transitions | Job metadata may reconcile; checkpoint/content **NO** | `FR-REC-001..006`; `FEAT-REC-001..015`; `FEAT-PROG-001..008`; `TECH-DEC-011`; `API-DEC-008` |
| 13 | Cache, thumbnail, temporary/intermediate file | Workspace/Local Storage Module | Local filesystem/cache | Performance and safe processing | **YES—`R`** when derived from document | Minimum necessary; auto-clean after success/failure/expiry | **YES—direct cleanup, no Recycle Bin** | Cleanup failure metadata only | **NO** | `FR-STOR-004..005`; `FR-TRASH-007`; `FEAT-STOR-005..013`; `FEAT-TRASH-012` |
| 14 | Upload session (`uploadId`, range/digest/state/expiry) | File Transfer boundary; owner/schema not assigned in Documents 04–06 | Server SQL/transfer metadata | Resumable upload lifecycle and validation | YES—`C` | Typed expiry/cleanup policy; numeric TTL unresolved | YES—abort/expire/cleanup | YES—safe transfer state/errors | N/A | `IR-API-003`; `FR-FILE-010..015`; `FEAT-REC-008`; `API-AUTO-006`; `API-DEC-004` |
| 15 | Server processing temporary input/result payload | Jobs/Processing storage boundary | Secure server file/object storage | Optional/required server processing | **YES—`R`** | Temporary processing retention/expiry; numeric value unresolved | YES—terminal/abort/expiry cleanup | Access/state metadata only | Not a secured file; no cross-device sync | `FR-PROC-003..005`; `FR-STOR-005`; `FEAT-PROC-003..004`; `API-DEC-004` |
| 16 | Server job record | Jobs Module / `jobs.*` | Server SQL | Durable status, progress, result reference, ownership | YES—`C` | Operational/job retention policy; unresolved | Policy-managed; terminal history may expire | **YES—state transition metadata** | REST/SignalR projection only | `IR-API-004`; `FR-REC-001..006`; `FEAT-REC-010..015`; `FEAT-PROG-001..008`; `API-AUTO-002`; `API-DEC-008` |
| 17 | Job attempt, retry, resume, server checkpoint metadata | Jobs/Recovery Module / `jobs.*` | Server SQL; checkpoint payload may be secure object | Recovery and bounded retry under one logical Job ID | YES—`C/R` | Parent job/recovery policy | Policy cleanup after parent/recovery window | **YES—attempts append-only** | Server authoritative; safe status to client | `FR-REC-001..006`; `NFR-REC-004..005`; `FEAT-REC-001..015`; `API-DEC-005` |
| 18 | Idempotency record and request fingerprint | API Platform boundary; owner/schema unresolved | Server SQL/cache with persistence | Prevent duplicate mutation and return same logical resource | YES—`C` | Explicit expiry required; numeric value unresolved | YES—expiry cleanup only | YES—safe collision/reuse metadata | N/A | `API-DEC-005`; `IR-API-004..008`; `FEAT-REC-010..014` |
| 19 | Personal User History | History Module / local SQLite baseline; server projection unresolved | Local-first; Document 04/06 also imply server resource | User convenience: filename, feature, time, status, location, size, duration | YES—`C/R` | Until user deletion/local retention policy | **YES—user may delete item** | Deletion/action metadata may be audited; content not | Not listed for selective sync; default **NO** | `FR-HIST-001..003`; `FEAT-HIST-001..015`; `FEAT-STOR-014`; `TECH-DEC-011` |
| 20 | Enterprise audit event | Audit Module / `audit.*` | Server SQL | Mandatory enterprise evidence | **YES—`A/C`** | Official audit retention policy; numeric duration unresolved | **NO ad-hoc/user delete**; policy-managed purge only | Self-auditing controls; append behavior | Automatic metadata sync from offline queue | `FR-AUDIT-001..004`; `DR-DB-006`; `FEAT-AUDIT-001..008` |
| 21 | Offline audit queue | Audit Module / encrypted local store | Local, then server sync | Buffer required audit metadata while offline | **YES—`A/C`** | Until acknowledged server persistence + bounded retry | YES after acknowledged sync/expiry policy | YES—queue/sync state, not sensitive payload | **YES—automatic metadata batch** | `FR-AUDIT-003`; `FEAT-AUDIT-005..007`; `FEAT-PROC-013..014` |
| 22 | Structured log, trace, metric, alert | Observability Platform | Local buffer + approved server observability backend | Operations, diagnostics, performance, security monitoring | `I/C`; never `R/X` content | Monitoring policy; source explicitly leaves detail for later | Policy/sampling/retention managed | N/A; logging changes/config audited where required | Controlled telemetry pipeline only | `NFR-MON-001..004`; `FEAT-MON-001..017`; `TECH-DEC-012` |
| 23 | Secured-file metadata | Backup/Secured Files Module / `backup.*` | Server SQL | Ownership, MIME, size, storage reference, version, retention, state | **YES—`C`** | While secured file/version is active + account retention/tombstone policy | YES—authorized secured-file deletion | **YES—metadata/access/deletion** | **YES—approved metadata list/status** | `FR-BACKUP-001..010`; `DR-DB-005`; `FEAT-BACKUP-001..009`; `API-DEC-004` |
| 24 | Secured-file payload and version object | Backup/Version Module | Secure file/object storage | Explicit **Amankan ke Akun**, restore, cross-device access | **YES—`R`** | **Maximum three latest active versions**; older lifecycle per policy | YES—owner/permission/policy; fourth-version rotation | Access/create/restore/delete metadata only | Explicit user-selected payload; cross-device access | `FR-BACKUP-001..010`; `FR-VER-001..004`; `FEAT-BACKUP-001..007`; `FEAT-VER-001..006` |
| 25 | Local Recycle Bin item | Workspace/Recycle Module | Local filesystem + SQLite | Recover user-deleted local result/draft | **YES—`R/C`** | Configurable local lifecycle; 30 days is a reminder threshold, not locked auto-delete | YES—restore or permanent delete with confirmation | Destructive action metadata only | **NO** | `FR-TRASH-001..009`; `FEAT-TRASH-001..012` |
| 26 | Account recycle/tombstone for secured file | Backup/Secured Files Module / `backup.*` | Server SQL + secure object lifecycle | Server-side deletion recovery/retention distinct from local Recycle Bin | **YES—`R/C`** | Account retention policy; exact behavior/duration unresolved | Authorized restore/purge per policy | **YES—append audit metadata** | State visible across authorized devices | `FR-TRASH-001`, `FR-TRASH-006`, `FR-TRASH-009`; `FEAT-BACKUP-007`; `FEAT-ADMIN-009..010` |
| 27 | Favorite tool, selected preset, language/view preference | Workspace/UI Platform; server sync via Backup selective metadata | Local SQLite; optional server metadata | User productivity/preferences | YES—`C` | Until user reset/account policy | YES—user reset/delete | Only policy/security-relevant changes, not every preference | **Selective only** | `FR-BACKUP-008`; `FEAT-BACKUP-010..013`; `FEAT-FLOW-009..010`; `FEAT-UX-017..018`; `TECH-DEC-011` |
| 28 | Visual signature/paraf/stamp asset | Signature Asset Service | Local secure storage by default | Place visual non-PKI signature in working state | **YES—`R`** | Ephemeral unless user explicitly selects “save”; saved-asset duration unresolved | **YES—user-controlled secure delete** | Asset value NO; create/use/delete metadata only if policy requires | **NO automatic sync** | `FR-SIGN-001..003`; `SR-SEC-011..012`; `FEAT-SIGN-001..013` |
| 29 | Assistant conversation state | Assistant Module / local SQLite/minimal store | Local | Conversation continuity and clarification | **YES—`C/R`** | Until user clears/local retention policy | **YES—user clear** | Only safe intent/Feature/status metadata separately | **NO default sync** | `FR-ASSIST-015`; `SR-SEC-015..017`; `FEAT-ASSIST-024`, `FEAT-ASSIST-030` |
| 30 | Assistant workflow plan/preset/history metadata | Assistant/Workflow owner; preset service shared contract | Local; selected preset may sync | Validated plan, repeat workflow, safe recovery | YES—`C`; must exclude credentials/content | Until user delete/preset policy | YES | Safe Feature ID/status/correlation only | Selected preset metadata only | `FR-ASSIST-004..012`; `FEAT-ASSIST-007..018`, `FEAT-ASSIST-022..024` |
| 31 | OCR text, extracted document text, AI/translation request content | Processing/OCR/Content Intelligence boundary | Local preferred; approved internal server only when allowed | Transient processing input | **YES—`R`** | Minimum processing scope; output only if user intentionally saves | YES—immediate/terminal cleanup by lifecycle | **NO content audit/log** | **NO automatic sync**; server transfer only after policy/consent | `SR-SEC-014..016`, `SR-SEC-021`; `FEAT-ASSIST-019..021`; `FEAT-INTEL-001..002`; `FEAT-CONV-036` |
| 32 | Support issue metadata, user message, status, category, priority, internal note | Support Module / `support.*` | Server SQL | User report and controlled triage workflow | **YES—`C/R`**; free text may contain sensitive content | Issue retention policy; numeric duration unresolved | Not user arbitrary delete; authorized/policy lifecycle | **YES—status/triage/access changes**; keep out of Enterprise Audit payload | User sees own simplified status; not general sync | `FR-SUP-001..007`; `DR-DB-007`; `FEAT-SUPPORT-001..002`, `FEAT-SUPPORT-004..006` |
| 33 | Issue screenshot/attachment | Support Module | Secure file/object storage | Explicit evidence sent by user | **YES—`R`** | Attachment/issue retention policy; numeric duration unresolved | Authorized/policy cleanup | Upload/access/delete metadata only; never copy image to audit/log | **Explicit user send only**, not auto-sync | `FR-SUP-003..005`; `SR-SEC-019..020`; `DR-DB-007`; `FEAT-SUPPORT-003`, `FEAT-SUPPORT-005` |
| 34 | Issue export PDF/Excel | Support/Report Export Service | Generated server artifact or authorized download | Repair handoff and filtered reporting | **YES—`C/R`** | On-demand and minimum necessary; export TTL/storage policy unresolved | YES—automatic/authorized cleanup | **YES—export event and filter scope metadata** | No automatic sync | `FR-SUP-007`; `FEAT-SUPPORT-007..008`; `API-DEC-006` |
| 35 | Correlation ID and diagnostic linkage metadata | Originating resource owner; indexed by Jobs/Audit/Observability | Local + server | Trace request/job across components | YES—`C` due linkability; **not secret** | Follows parent record/log retention | Follows parent lifecycle; never deletion authority | YES—safe linkage | Only with approved metadata flows | `NFR-REC-004`; `FEAT-REC-011`; `FEAT-MON-013`; `API-AUTO-003` |
| 36 | `fileRef`, object/storage reference, storage key mapping | Owning domain module; storage adapter | Server metadata; local reference where applicable | Resolve authorized file without exposing physical location | **YES—`C/R`** | Follows referenced resource; stale-reference cleanup required | Follows parent/tombstone lifecycle | Access/resolve/failure metadata | Opaque reference only | `DR-DB-003..005`; `API-AUTO-006`; Solution Design §§8–9 |

### 1.2 Ownership invariants

1. Satu tabel/resource mempunyai satu logical owner dan satu migration owner.
2. Modul lain tidak melakukan direct SQL, direct object-store read, atau update terhadap tabel/resource owner lain.
3. Cross-module operation menggunakan typed contract/application service atau internal event setelah commit.
4. Cross-module foreign key diminimalkan. Opaque resource ID tidak memberi authorization.
5. Shared table hanya untuk platform data yang benar-benar shared; upload dan idempotency belum boleh menjadi ownerless shared table.
6. Identity, permission, ownership, classification, purpose, state, dan version diperiksa di boundary pemilik data.
7. Correlation ID, Job ID, Issue ID, `fileRef`, `uploadId`, storage key, dan idempotency key bukan access token.

## 2. Data classification

Working classification berikut diperlukan untuk membedakan kontrol teknis. Nama/level final harus dipetakan ke kebijakan klasifikasi resmi Pertamina pada Security Design; laporan ini tidak menetapkan taxonomy enterprise baru.

| Kode | Working class | Contoh | Minimum boundary |
|---|---|---|---|
| `I` | Internal operational | Feature ID, safe status code, app version, aggregate health | Internal access; data minimization; no public exposure by default |
| `C` | Confidential metadata | User/owner ID, role/permission snapshot, filename, local path, file size, issue status, Job ID, Correlation ID, storage reference | Authentication, object-aware permission, encryption/secure storage as policy requires, query/export restriction, retention control |
| `R` | Restricted content | Document/file payload, OCR/full extracted text, issue message/screenshot, signature asset, checkpoint containing content | Strongest content boundary; local-first; explicit server transfer; secure object/file storage; no audit/log/telemetry content; least privilege |
| `X` | Credential/secret | Password PDF, access/refresh token, service secret, encryption key | Never ordinary SQL/config/log/history/audit; approved secure store or process memory; rotation/revocation/secure wipe |
| `A` | Controlled audit/security evidence | Enterprise audit record, security evidence, policy-change event | Append-oriented integrity, restricted read/export, controlled retention/purge, tamper monitoring; content prohibitions still apply |

### 2.1 Sensitive metadata is still sensitive

Metadata-only tidak berarti non-sensitive. Kombinasi User ID, unit/fungsi, filename, Feature ID, file size, timestamps, Job ID, Correlation ID, issue status, dan storage reference dapat mengungkap aktivitas atau pola kerja. Karena itu:

- audit mengumpulkan hanya field minimum yang disetujui;
- filename/local path tidak otomatis masuk Enterprise Audit atau telemetry;
- exact local path tidak pernah disinkronkan ke server;
- query/export memakai allowlist dan authorization scope sebelum pagination/count;
- Correlation ID hanya untuk linkage, tidak dipakai sebagai search surface umum tanpa permission;
- safe diagnostics tidak boleh berkembang menjadi document-content dump.

### 2.2 Prohibited persistence matrix

| Data | Ordinary SQL | Local SQLite/history | Audit/log/telemetry | Automatic sync | Allowed handling |
|---|:---:|:---:|:---:|:---:|---|
| PDF/file password | **NO** | **NO** | **NO** | **NO** | Process memory only, then purge |
| Access/refresh token | **NO** | **NO** | **NO** | **NO** | Approved secure OS/server-session boundary |
| Service secret/encryption key | **NO** | **NO** | **NO** | **NO** | Approved secret/key management system |
| Visual signature asset | **NO by default** | Only explicit secure save | **NO asset value** | **NO** | Local ephemeral or explicit user-controlled secure storage |
| Document/OCR/full extracted text | **NO metadata table** | Only working/content store when required | **NO** | **NO** | Local file/content store or approved temporary secure server processing |
| Issue screenshot | **NO blob SQL** | Preview before send only | **NO** | **NO** | Explicit send to support secure object storage |

## 3. Retention matrix

“TBD policy” berarti Dokumen 01–06 belum mengunci angka. Nilai tidak boleh dipilih sendiri oleh implementation task.

| Data group | Retention trigger/baseline | Locked numeric value? | Disposal / transition rule | Policy owner needed |
|---|---|---:|---|---|
| Password PDF | Current operation only | N/A | Purge from memory after use/failure/cancel | Security invariant |
| Token/session | Identity lifetime/revocation | NO | Expire/revoke; clear local secure cache | Identity/Security |
| Offline claim cache | Valid offline-policy window | NO | Invalidate on expiry/logout/revocation sync | Identity/Security |
| Original local file | User-controlled filesystem lifecycle | N/A | App never silently deletes/overwrites | User + Workspace guard |
| Working state/draft | Active work + recovery need | NO | Finalize, cancel, user delete, or policy cleanup; eligible for local Recycle Bin where applicable | Workspace policy |
| Cache/thumbnail/temp | Minimum performance/processing need | NO | Direct cleanup after success/failure/expiry; bypass Recycle Bin | Workspace/System Safety |
| Local checkpoint/job/activity | Active/recovery window | NO | Purge after terminal state and recovery window, preserving required audit metadata | Jobs/Recovery policy |
| Upload session and chunks | Upload state + expiry | NO | Abort/expire/cleanup; prevent orphan chunks | API/File Transfer policy |
| Server temporary processing payload | Processing/validation lifecycle | NO | Purge after terminal/abort/expiry unless explicitly secured | Jobs/Storage policy |
| Server job/attempt metadata | Operational/support/recovery need | NO | Policy cleanup; attempts remain append-only during retained job lifecycle | Jobs/Support policy |
| Idempotency record | Safe retry window | NO | Expire/cleanup without removing underlying resource | API policy |
| Personal User History | Until user deletes or local retention policy | NO | Delete history item only; do not delete Enterprise Audit | History/User policy |
| Offline audit queue | Until server acknowledges persistence | NO | Purge local queue after acknowledged sync; bounded retry/dead-letter handling must be defined | Audit policy |
| Enterprise Audit | Official enterprise retention | NO | No user/ad-hoc delete; policy-authorized purge only | Audit/Compliance |
| Logs/traces/metrics | Monitoring/security policy | NO | Sampling, minimization, tiered retention, controlled purge | Observability/Security |
| Secured file | Until owner/policy deletion | NO | Account recycle/tombstone/secure purge as policy defines | Backup/Records policy |
| Secured file versions | Maximum three latest active versions | **YES—3** | Fourth version rotates oldest by explicit retention rule; no silent orphan object | Backup/Version policy |
| Local Recycle Bin | Configurable lifecycle | **30 days is reminder only** | Restore or permanent delete after confirmation; no assumption of auto-delete at day 30 | Workspace/Admin policy |
| Account recycle/tombstone | Separate server retention | NO | Restore or authorized secure purge | Backup/Admin policy |
| Saved visual signature | User explicit save + policy | NO | User-controlled secure delete; otherwise ephemeral | Signature/Security policy |
| Assistant conversation | Local minimal state until clear/policy | NO | User clear must not delete required audit metadata | Assistant/Privacy policy |
| OCR/AI transient content | Current processing need | NO | Purge after processing/expiry; retain result only on explicit save | Processing/Privacy policy |
| Support issue/message/note | Issue/support lifecycle | NO | Authorized retention/closure/purge; user does not administer internal ticket lifecycle | Support/Legal/Security |
| Issue screenshot/attachment | Issue attachment policy | NO | Secure purge with issue/tombstone policy; prevent orphan object | Support/Security |
| Issue export | On-demand minimum necessary | NO | Short-lived generation/download cleanup unless explicitly records-managed | Support/Records policy |
| Policy/config/feature flags | Active + governed change history | NO | Controlled deprecate/replace; change audit retained separately | Admin/Audit policy |

Retention values should be typed policy, environment-governed, tested, and auditable. Lower-priority configuration must not weaken System Safety/Security invariants.

## 4. Deletion rules

### 4.1 General rules

1. **Authorization first.** Delete/purge/restore checks authenticated subject, permission, object ownership, policy, classification, resource state, legal/retention hold if applicable, and current version.
2. **No ID-based authority.** Resource ID, storage key, `fileRef`, Correlation ID, Job ID, Issue ID, or idempotency key never proves access.
3. **No cross-module delete SQL.** Caller requests deletion through owner module contract.
4. **Explicit destructive confirmation.** Permanent local delete, secured-file delete, overwrite, and other irreversible actions follow confirmation policy.
5. **Payload/reference consistency.** Use a lifecycle state/tombstone so failed object deletion can retry without losing the only reference. Do not delete SQL reference first and silently orphan payload.
6. **Audit the action, not the content.** Record safe actor/resource/action/status/correlation metadata. Do not copy document, screenshot, password, signature, OCR text, token, or secret into audit.
7. **Idempotent deletion.** Transport retry must not create duplicate destructive effects or return access to a deleted object.
8. **No silent cascade into Audit.** Deleting User History, Assistant conversation, issue view state, local workspace, or secured file never deletes Enterprise Audit outside audit retention policy.

### 4.2 Domain rules

| Domain | Delete behavior |
|---|---|
| Original/source file | Never deleted or overwritten automatically by PERTAMINA OTO. |
| Local result/draft | Move to local Recycle Bin where applicable; offer Undo; permanent delete requires confirmation. |
| Cache/temp/thumbnail | Direct automatic cleanup; never consume Recycle Bin. |
| Personal User History | User may delete the history item. This does not delete result file, secured file, job audit, or Enterprise Audit unless a separate explicit authorized action exists. |
| Enterprise Audit | Immutable-style append behavior: no ordinary update/delete; correction is a new linked event. Purge only through authorized retention process. Exact tamper-evidence/WORM mechanism remains for Security/Database Design. |
| Secured file | Revoke normal access, enter account recycle/tombstone lifecycle if policy uses it, delete object securely, verify outcome, then finalize metadata/tombstone. Audit safe metadata. |
| Secured version rotation | Creating version 4 must atomically establish the new version and select the oldest active version for policy rotation. “Maximum 3 latest versions” must remain true; failed purge cannot create an invisible orphan. |
| Upload/server temp | Abort/expire/purge metadata and chunks/object; cleanup is retryable and monitored. Upload/session expiry never deletes an already-created secured file. |
| Signature asset | Ephemeral by default. Saved only on explicit user choice; user can securely delete it. Placing a signature in a final PDF does not justify retaining the reusable signature asset. |
| Assistant conversation | User clear removes local conversation state, not mandatory audit metadata. Preset/workflow deletion follows its own owner service. |
| Support issue | User may see own simplified status but does not administer internal deletion. Authorized retention/purge must handle issue row, note, attachment, exports, and tombstone consistently. |
| Issue screenshot/export | Delete through Support owner; never leave object-store orphan, cached preview, or downloadable stale export. |
| Token/secret/password | Credential lifecycle/secure wipe only; never ordinary business-data deletion because values should not be in ordinary persistence. |

### 4.3 Deletion acceptance checks

- deleted object cannot be fetched through old `fileRef`, URL, range request, export link, retry response, cache, or SignalR notification;
- ownership/authorization is re-evaluated on restore and every continuation cursor;
- storage cleanup is observable without logging payload/storage credential;
- retrying delete returns the same final resource state;
- policy-retained audit remains searchable by authorized audit functions;
- local and account Recycle Bin tests prove there is no cross-lifecycle accidental purge;
- backup version test proves only three latest active versions remain addressable.

## 5. Audit/history separation

### 5.1 Four distinct data planes

| Plane | Purpose | Typical fields | User visibility | Mutability/deletion | Content boundary |
|---|---|---|---|---|---|
| Personal User History | Convenience and repeat/open actions | Filename, feature, time, status, local result location, size, duration | User’s own history | Mutable projection; user-deletable | May contain sensitive local metadata; not compliance evidence |
| Enterprise Audit | Mandatory company evidence | Process/User/role/unit refs, Feature ID, timestamps, status, size, local/server, safe error/app version, Correlation ID where approved | Permission-gated Auditor/Admin surface; Role Matrix later | Append-oriented; no ad-hoc/user delete; policy retention | **No document, clipboard, password, token, screenshot, full OCR text, or signature** |
| Support Issue | Explicit user report and triage | User message, safe diagnostics, status, category/priority, internal note, optional screenshot reference | Reporter sees own simplified status; support permission sees authorized detail | Controlled workflow and retention; not user-history CRUD | May contain sensitive user-submitted content; store separately from Audit/Observability |
| Observability | Technical health/performance/security operations | Feature/Job/Correlation, safe status, duration, error code, app/engine version, aggregate metrics | Operational permission only | Sampling/retention policy | **No document/OCR/signature/credential content** |

### 5.2 Required separation behavior

- `FEAT-HIST-012` deletes Personal History only; it must not invoke Audit deletion.
- Audit may record that a history item, secured file, issue, or configuration was changed/deleted, but never copy the removed payload.
- Issue screenshot/message belongs to Support; Audit stores only safe event/reference metadata if required.
- Observability log window linked to an issue must be sanitized before association and remain governed by its own retention; Support must not receive unrestricted telemetry access.
- A User History row is not evidence that a file still exists. Opening a result rechecks current local/server resource state and permission.
- An Audit row is not authority to access the audited secured file.
- Deleting Assistant conversation preserves mandatory audit metadata such as safe intent category, canonical Feature ID, status, and Correlation ID where required.

### 5.3 Immutable-style audit baseline

Minimum behavior:

```text
Business/security event committed
→ create append audit event with immutable event identity
→ prohibit ordinary update/delete APIs
→ correction/annotation becomes a new linked event
→ offline event queues securely
→ server acknowledges persisted event
→ local queue purges acknowledged copy
→ retention purge only through authorized policy workflow
```

Sequence, event identity, actor/service identity, source, timestamp, Correlation ID, and safe before/after codes may support integrity. Exact cryptographic chaining, WORM storage, partitioning, and legal retention are not locked by Documents 01–06 and must not be invented here.

## 6. Storage-reference rules

### 6.1 SQL metadata versus payload

SQL may store:

- stable resource ID and owner subject/account reference;
- domain purpose and resource state;
- classification/policy label reference;
- MIME/extension, size, digest/hash when justified;
- opaque storage provider/key reference;
- logical file/version number and parent relationship;
- created/updated/expiry/retention/tombstone timestamps;
- safe validation/result/error codes;
- Job ID and Correlation ID where relevant;
- encryption-key **reference/version** if approved, never key material;
- access/retention policy reference.

SQL must not become the default blob store for document payload, secured file, processing temp, issue screenshot/attachment, or export artifact.

### 6.2 Typed opaque reference

`fileRef` contract remains typed and opaque:

```text
kind + id + version/state context
→ resolve through owning application service
→ authenticate
→ authorize permission + ownership + purpose + policy
→ validate lifecycle state and version
→ access storage through approved adapter
```

Rules:

- `uploadId` identifies transfer lifecycle; it is not interchangeable with `fileRef`.
- Only `READY` upload can become processing input.
- Physical local path, object storage key, arbitrary URL, filename, and client-supplied MIME are never authority.
- Local-only processing does not invent an upload or server copy.
- Temporary job result is not a secured file. Only explicit **Amankan ke Akun** creates the secured copy/version.
- Returning a storage key directly to client is prohibited as an authorization shortcut.
- Reference resolution is version/state-aware to prevent stale/deleted version access.
- Cross-module references use resource IDs/contracts; they do not authorize direct joins/writes across module schemas.

### 6.3 Storage lifecycle integrity

- Metadata creation and payload commit use explicit state transitions so partially written objects are not treated as final.
- Object digest/size is validated before `READY`/final status.
- Cleanup workers are bounded, retryable, observable, and permission-scoped.
- Object deletion failure leaves a controlled tombstone/retry state, not an unreferenced secret object.
- Restore creates a local working copy; it does not turn server storage into a live cloud workspace.
- Silent overwrite is prohibited. Version/conflict is detected before update.

### 6.4 Permission at the data boundary

1. Authorization is deny-by-default, permission-based, object-aware, and server-enforced.
2. Role groups permissions but is not final authorization; this report does not alter User/Administrator/Auditor definitions.
3. Administrator does not automatically read user document content.
4. Superadmin/Support naming denotes a permission-gated support surface; it is not a new role and does not grant general secured-file access.
5. Reporter can read only own authorized issue status/detail allowed by contract; support access is scoped and auditable.
6. Worker/internal provider uses separate service identity and job-scoped access; end-user token is not forwarded.
7. Authorization scope is applied before filter, pagination, count, export, object resolution, and download range handling.
8. Cursor, ETag, digest, Correlation ID, resource ID, and storage reference are never authorization substitutes.
9. Consistent `404` concealment may be used for sensitive resources after authentication to avoid existence disclosure, per API boundary.

## 7. Privacy/security risks

| Risk | Severity | Boundary/control required |
|---|---|---|
| Password stored in job parameter, autosave, idempotency snapshot, issue, or log | Critical | Password is memory-only; typed DTO redaction; persistence/log tests; no response echo |
| Token/secret placed in ordinary SQL/config/log | Critical | Approved secure store/key management; value filtering; rotation/revocation; secret scanning |
| IDOR via `fileRef`, Issue ID, Job ID, Correlation ID, storage key, or cursor | Critical | Server-side object authorization on every operation/continuation; identifiers are not authority |
| Administrator/support implicitly gains document access | Critical | Explicit object permission and purpose; support issue access never confers secured-file access |
| Signature asset auto-saved, backed up, synced, or logged | Critical | Ephemeral default; explicit save only; local secure storage; no auto-sync/log/audit value |
| Issue screenshot captures document/desktop/credential data | High | App-window target where possible, preview, markup, explicit send, secure storage, scoped support access, retention |
| Audit over-collects filename/path/content or sensitive free text | High | Allowlisted metadata schema; code values; no arbitrary message/details; periodic schema review |
| Free-form issue message copied into audit/telemetry | High | Support-only payload; Audit stores reference/status metadata, not issue text |
| Local SQLite exposes draft refs, paths, conversation, checkpoints | High | Minimize, encrypt/protect sensitive local state per Security Design, OS permissions, secure cleanup |
| Orphan object after failed DB/object deletion | High | Tombstone/state machine, retryable cleanup, reconciliation, orphan monitoring without content logging |
| Version rotation exposes more than three active versions or deletes wrong version | High | Atomic version decision, stable order/tie-breaker, ownership/version checks, retention tests |
| Audit row can be updated/deleted by ordinary CRUD | High | Append-only application contract, restricted DB permission, correction event, controlled retention purge |
| Retention defaults to “forever” because exact numbers are absent | High | Typed mandatory retention policy before production; owners and legal/security approval; expiry monitoring |
| Public/external AI receives document/OCR content | Critical | Approved local/enterprise-internal provider only; policy/classification/permission/data-minimization/consent |
| Correlation ID becomes public search/access key | Medium | Treat as confidential linkage metadata; permission-gated search; never resource authorization |
| Export PDF/Excel proliferates support/audit data | High | Permission-gated asynchronous/on-demand export, field allowlist, scope recheck, watermark/classification if policy requires, cleanup |
| Local and Account Recycle Bin share deletion implementation | High | Separate owner, storage, policy, restore, purge, and tests |
| Idempotency safe response snapshot captures payload/secret | High | Store only minimal safe response; fingerprint/hash and IDs, never file/password/token/content |
| Cross-module direct SQL bypasses owner validation | Critical | Module DB permission boundary, application contracts, architecture tests, migration ownership |
| Storage reference leaks physical path/object key | High | Opaque `fileRef`; never client authority; sanitize errors/logs |
| Audit offline queue is lost or duplicated | High | Encrypted durable local queue, stable event ID, idempotent server ingest, acknowledgement before purge, bounded retry |

## 8. Requirement/Feature traceability

| Governance concern | Requirement / decision references | Canonical Feature references | Data boundary outcome |
|---|---|---|---|
| Identity/session | `FR-AUTH-001..005`; `FR-ROLE-001..003`; `SR-SEC-001..002`; `API-DEC-007` | `FEAT-AUTH-001..012`; `FEAT-PROC-011` | Identity server-authoritative; local claim cache narrow/time-bounded; permission and ownership rechecked server-side |
| Password/token/secret | `FR-FILE-015`; `SR-SEC-002..003`, `SR-SEC-010`; `API-DEC-007` | `FEAT-VALID-008`; `FEAT-AUTH-006..008` | Password memory-only; credentials in approved secure boundaries; never log/audit/history/backup |
| Local workspace/storage | `FR-STOR-001..006`; `DEC-REQ-009`, `DEC-REQ-019` | `FEAT-EDIT-001..012`; `FEAT-STOR-001..016` | Source, working state, draft, undo/cache/temp/checkpoint local-first and non-destructive |
| Selective secured files | `FR-BACKUP-001..010`; `DR-DB-003..005`; `API-DEC-004` | `FEAT-BACKUP-001..013` | SQL metadata + secure object payload; explicit user-selected backup; ownership and encrypted transfer/storage |
| Three-version limit | `FR-VER-001..004`; `DEC-REQ-014` | `FEAT-VER-001..006` | Maximum three latest active secured-file versions; version 4 rotates oldest by policy |
| Personal History | `FR-HIST-001..003`; `DEC-REQ-011` | `FEAT-HIST-001..015`; `FEAT-STOR-014` | Personal convenience metadata; user-deletable; separate from Audit |
| Enterprise Audit | `FR-AUDIT-001..004`; `DR-DB-006`; `DEC-REQ-011` | `FEAT-AUDIT-001..008` | Metadata-only, offline queue + automatic batch sync, append-oriented, policy retention |
| Recycle/deletion | `FR-TRASH-001..009`; `DEC-REQ-012` | `FEAT-TRASH-001..012`; `FEAT-BACKUP-007`; `FEAT-ADMIN-009..010` | Local and account lifecycle separate; permanent delete confirmed; cache/temp bypass Recycle Bin |
| Recovery/job/correlation | `FR-REC-001..006`; `NFR-REC-001..005`; `IR-API-004..005`; `API-DEC-005`, `API-DEC-008` | `FEAT-REC-001..015`; `FEAT-PROG-001..008`; `FEAT-MON-013` | Durable Job ID, Correlation ID, append-only attempts, checkpoint/resume, REST authoritative state |
| File transfer/storage reference | `IR-API-003`; `FR-FILE-010..015`; `API-AUTO-006`; `API-DEC-004` | `FEAT-VALID-001..018`; `FEAT-REC-008..009`; `FEAT-BACKUP-004` | `uploadId` lifecycle separate from opaque `fileRef`; no path/key/URL authority |
| Signature | `FR-SIGN-001..003`; `SR-SEC-011..012`; `BR-SIGN-001` | `FEAT-SIGN-001..013` | Visual non-PKI only; asset ephemeral unless explicit save; no auto-sync/log/telemetry |
| Assistant privacy/state | `FR-ASSIST-003..015`; `SR-SEC-013..018`; `IR-API-010` | `FEAT-ASSIST-007..030` | Local minimal conversation, clearable, canonical tool boundary, no credentials/content automatic sync |
| PDF Intelligence/OCR | `FR-INTEL-001..003`; `SR-SEC-014..016`, `SR-SEC-021` | `FEAT-INTEL-001..002`; `FEAT-CONV-036`; `FEAT-ASSIST-019..021` | Restricted content, deterministic/local-first, approved internal provider only, no public endpoint |
| Support issue | `FR-SUP-001..007`; `SR-SEC-019..020`; `DR-DB-007`; `NFR-MON-004` | `FEAT-SUPPORT-001..008` | SQL issue metadata/text/note; secure object screenshot; explicit send; permission-gated triage/export |
| Observability | `NFR-MON-001..004`; `SR-SEC-004`; `IR-API-008` | `FEAT-MON-001..017`; `FEAT-SUPPORT-001` | Safe structured telemetry and diagnostics; content/credential prohibition; Correlation ID linkage |
| Module ownership | `NFR-ARCH-001..005`; `DR-DB-001..004`; Solution Design §§7–9 | Relevant owner service dependencies in Feature Catalog | One module owns each table/resource; no client/direct cross-module SQL; file payload outside SQL by default |
| Retention/policy | `FR-AUDIT-004`; `FR-TRASH-003..009`; `FR-VER-001..002`; Solution Design §19 | `FEAT-AUDIT-008`; `FEAT-ADMIN-009..013`; `FEAT-TRASH-006..012`; `FEAT-VER-001..002` | Policy-driven retention; system safety wins; exact unapproved durations remain unresolved |

Trace chain for implementation remains:

```text
Business Process
→ Requirement ID
→ canonical Feature ID
→ owning module/entity/schema
→ API/application contract
→ migration/storage policy
→ permission rule
→ unit/integration/architecture/security test
→ UAT/release evidence
```

Minimum implementation traceability fields should include owning module, source Requirement/Feature IDs, classification, retention policy ID/version, permission policy, schema migration owner, and relevant test/evidence IDs. These are design/governance metadata, not a proposal to create a single cross-module “everything” table.

## 9. Inconsistency audit

| ID | Finding | Evidence / impact | Integrator action; no silent decision |
|---|---|---|---|
| `INC-DATA-001` | Document 06 is not marked FINAL | File is `06_API_DOCUMENTATION_WORKING.md`, status WORKING v0.5, while task requests Documents 01–06 FINAL. Decision Register is 9/9 but final audit is pending. | Confirm/finalize Document 06 lineage before Document 07 declares all parents FINAL. |
| `INC-DATA-002` | Personal History physical placement is ambiguous | Feature Catalog `FEAT-HIST-*` uses `L-SYS`; Technical Reference allows local history in SQLite; selective-sync requirements do not list history. Solution Design lists server schema `history.*`, and Document 06 exposes `/history`. | Decide whether `/history` is local adapter, server history, or optional projection; define owner, sync, retention, and deletion without merging it with Audit. |
| `INC-DATA-003` | Upload session ownership/schema is undefined | API-DEC-004 locks durable upload lifecycle, but Solution Design module schema list has no explicit transfer/upload owner. | Assign one module/migration owner and table/object lifecycle; do not create ownerless shared table. |
| `INC-DATA-004` | Idempotency record ownership/schema is undefined | API-DEC-005 requires persisted idempotency across upload/job/secured copy and other mutation domains. | Define API-platform versus domain ownership and cleanup policy; prevent cross-module direct SQL and sensitive response snapshots. |
| `INC-DATA-005` | Retention durations are mostly unresolved | Audit, log, job, upload, idempotency, issue/attachment/export, secured-file/account recycle, signature, conversation, and checkpoint durations are not numeric. | Document policy owners and required decisions. Do not treat absent values as indefinite retention or invent defaults. |
| `INC-DATA-006` | Recycle “30 days” can be misread as auto-delete | `FR-TRASH-003` defines a reminder after 30 days; it does not lock automatic purge. | Model reminder threshold separately from retention/purge policy. |
| `INC-DATA-007` | Secured version 4 behavior is ambiguous | `FR-VER-001` says max 3; `FR-VER-002` says oldest is deleted **or moved according to retention policy**. Moving to an accessible retained version could violate max 3. | Define “three latest active/addressable versions” and the exact old-version tombstone/purge lifecycle. |
| `INC-DATA-008` | `DR-DB-003` is too permissive if read alone | It says document content is stored only if approved, while architecture says file is not SQL Blob default and approved payload belongs in secure file/object storage. | Clarify that approval does not make ordinary SQL Blob the default; exceptions require explicit architecture/security decision. |
| `INC-DATA-009` | Saved signature persistence lifecycle is incomplete | `FR-SIGN-002` permits securely saved signatures, while `FR-SIGN-001` prohibits save without user choice and security rules prohibit automatic sync/log. No owner schema, encryption, retention, or delete contract is final. | Define Signature Asset Service storage/lifecycle in Document 07/08; default local/ephemeral and no auto-sync remain locked. |
| `INC-DATA-010` | Support terminology can be mistaken for a new role | Requirements and Feature Catalog use Superadmin/Support surface, while initial roles remain User, Administrator, Auditor. Document 06 says support is permission-gated, not a new role. | Keep permission terminology and defer Role Matrix changes to Document 09. Do not create a Superadmin role here. |
| `INC-DATA-011` | Audit immutability mechanism is underspecified | Requirements prohibit arbitrary deletion and Feature Catalog has Audit Retention, but exact append-only DB permissions, correction model, tamper evidence, partition/purge mechanism are not locked. | Lock immutable-style application behavior now; defer physical integrity/WORM choice to Database/Security Design with compliance input. |
| `INC-DATA-012` | Issue content must not leak into Audit/Observability | Issue message/screenshot may legitimately contain sensitive content after explicit send, whereas Audit/log prohibit screenshot/document content. | Define separate support payload schema/object store and safe reference-only audit/telemetry mapping. |
| `INC-DATA-013` | Deletion cascade and orphan handling are not fully specified | Requirements define delete/recycle but not cross-store transaction/reconciliation for SQL metadata + object payload + exports + cached reference. | Define tombstone, retryable cleanup, reconciliation, and restore rules per owner module. |
| `INC-DATA-014` | Official data-classification taxonomy is not present | Feature/policy can consider classification, but Documents 01–06 do not define official labels/handling matrix. | Map the working `I/C/R/X/A` controls to approved Pertamina classification in Security Design; do not treat report labels as enterprise policy. |
| `INC-TRACE-001` | Undefined UI requirement references | Requirement traceability matrix references `UIR-UI-015..016` for Core Feature Surface, but Document 02 defines only `UIR-UI-001..014`. | Correct traceability to existing requirements or formally add requirements through Integrator/change control; do not invent IDs in Agent D report. |
| `INC-TRACE-002` | Recycle traceability omits `FR-TRASH-009` | Document 02 traceability matrix says `FR-TRASH-001..008`, while `FR-TRASH-009` is the critical local/account separation requirement and Feature Catalog uses `FR-TRASH-001..009`. | Correct the traceability range to include `FR-TRASH-009`. |

### 9.1 Integration priorities

Before schema design is considered stable, Integrator should resolve in this order:

1. confirm Document 06 final baseline;
2. assign owners for upload and idempotency persistence;
3. decide Personal History local/server projection without weakening deletion separation;
4. define typed retention policies and policy owners;
5. lock secured-file/account recycle/version-rotation lifecycle;
6. define signature asset persistence lifecycle;
7. lock audit append/correction/purge behavior;
8. correct the two traceability reference defects;
9. map working classification to official Pertamina classification;
10. verify deletion/orphan reconciliation across SQL and object storage.

No Role Matrix change, new role, production code, or master Document 07 edit is made by this report.
