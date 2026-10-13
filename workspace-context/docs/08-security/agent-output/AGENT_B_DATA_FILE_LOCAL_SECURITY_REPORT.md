# AGENT B — Data, File, Storage & Local Security Report

**Task ID:** DOC08-AGENT-B  
**Scope:** data, file, storage, transfer, and local security for PERTAMINA OTO  
**Sources reviewed:** Documents 01–07 FINAL and the latest Master Project Summary  
**Status:** recommendation for incorporation into Document 08; this report does not modify the Document 08 master

This recommendation preserves the locked Local-First boundary. Source files, drafts, working state, undo/redo, cache, thumbnails, temporary artifacts, checkpoints, clipboard content, PDF passwords, and visual signatures are not silently uploaded or synchronized. “Amankan ke Akun” is an explicit, selective action, and the server retains at most three active/addressable secured versions.

The labels `I`, `C`, `R`, `X`, and `A` below are working engineering classes inherited from Document 07, not a replacement for official Pertamina information-classification labels. Their formal mapping must be approved before production.

## 1. Data classification

### 1.1 Working classes

| Code | Working meaning | Default security objective |
|---|---|---|
| `I` | Internal operational state | Prevent unauthorized modification and unintended disclosure. |
| `C` | Confidential metadata | Limit disclosure because names, paths, identifiers, history, and relationships can reveal business activity. |
| `R` | Restricted content | Protect document/image/OCR payload throughout its local or explicitly selected server lifecycle. |
| `X` | Credential or secret | Never persist in ordinary application stores; minimize lifetime and exposure. |
| `A` | Controlled audit evidence | Preserve integrity, actor/time/action attribution, retention, and access separation. |

Official labels override these working codes. A derived artifact inherits the highest applicable classification of its source unless an approved rule explicitly lowers it after an irreversible transformation.

### 1.2 Data-control matrix

`C/I/A` in this table means confidentiality, integrity, and availability. “High” availability means recovery must be designed; it does not promise continuous availability.

| Category | Class and CIA | Encryption need | Authorized storage | Retention and cleanup | Access rule | Failure recovery |
|---|---|---|---|---|---|---|
| User source document | `R`; C High, I High, A business-dependent | Device/volume protection where available; never decrypt/re-encrypt silently | User-selected filesystem only by default | User-controlled; app must not delete or overwrite it silently | Current OS user plus explicit app operation | Preserve original; create a new result or use explicit, confirmed replace with recoverable staging |
| Local final result | `R`; C High, I High, A High until handed off | Same as source; protected staging before final Save As | User-selected result path; protected local staging while incomplete | User-controlled after successful save; failed staging is bounded and cleaned | Explicit save/export action | Checksum and atomic publish; recover incomplete staging without replacing source |
| Workspace/draft/undo/operation state | `R/C`; C High, I High, A Medium | Protected app storage; sensitive fields encrypted when platform threat model requires | Per-profile protected app filesystem plus SQLite metadata | Bounded by workspace/recovery policy; never auto-sync | Same OS profile and Pertamina subject; typed app/worker access only | Manifest-driven startup recovery; never silently discard or reset |
| Temp/intermediate/checkpoint | `R`; C High, I High for resumable state, A Medium | Same protection boundary as working content; encryption when content may remain at rest | Private app-managed temp root, not shared/global temp | Shortest practical duration; lease-based cleanup after success, cancel, expiry, or crash | Owning operation only | Validate manifest, digest, and generation before reuse; otherwise quarantine/delete and restart safely |
| Cache/thumbnail/preview | `R/C`; C High, I Medium, A Low | Encrypt if it exposes restricted content and volume protection alone is insufficient | Private per-profile cache | Short, size-bounded, regenerable; remove with parent lifecycle | Owning profile; never a public gallery by default | Treat as untrusted/regenerable; do not block source recovery if missing |
| SQLite local state | `C` with references to `R`; C High, I High, A High for recovery | Approved SQLite-at-rest solution or equivalent OS protection; include WAL/SHM/backups | One database per OS profile/device boundary; payload remains outside SQLite | Bounded history and recovery retention; protected backup only | UI process or serialized local data owner; worker through typed IPC | Integrity check, consistent backup, migration journal, and explicit repair path; no silent reset |
| SQL Server metadata | `C/A`; C High, I High, A High | Encrypted transport; approved database and backup encryption | Module-owned SQL schemas; no default file BLOBs | Domain/audit/retention policy with documented purge | Service identity and least-privilege schema access; no direct client DB access | Point-in-time/approved restore plus reconciliation with object storage |
| Secured file/object payload | `R`; C High, I High, A High | Server-side encryption at rest plus TLS in transit; integrity digest | Private secure object/file storage referenced by opaque internal metadata | Maximum three active/addressable secured versions; tombstone and policy-driven purge | Authorization before storage-reference resolution; no key-as-proof | Restore object and metadata consistently; verify digest/key reference/version state |
| Processing upload/result | `R`; C High, I High, A Medium until retrieved | TLS plus server-side encryption in quarantine/processing/result zones | Private server transfer/processing storage only after resolver and policy approval | Short, purpose-bound, expiring; failed sessions and results cleaned idempotently | Owner, purpose, policy, and job binding on every operation | Resumable upload/checkpoint where supported; expired result requires explicit rerun/recreation |
| OCR/extracted text | `R`; C High, I High, A Low/Medium | Encrypt if persisted; prefer bounded protected artifacts | Local protected working artifact by default; approved server temp only for required server processing | Shortest necessary; delete derivatives with operation/workspace | Owning operation; never logs, telemetry, public AI, or generic search index | Re-run from authorized source; do not retain hidden recovery copies beyond policy |
| Scanner/camera input | `R`; C High, I High, A Medium until saved | Protected app staging | App-private capture staging; photo gallery only by explicit choice | Discarded frames/retakes short-lived; saved output user-controlled | Explicit capture/import and OS permission | Recover completed pages from manifest; discard unverifiable partial frames |
| Clipboard image/file content | `R`; C High, I Medium, A Low | Memory first; protected staging only after explicit paste/import | Memory or app-private operation staging | Do not create clipboard history; release promptly | Read only on user paste/import gesture, not background polling | User can paste again; no hidden server recovery |
| Saved visual signature/paraf | `R`; C High, I High, A user-dependent | Strong local protection; encrypt at rest when saved | App-private local asset only after explicit save | Until explicit deletion or approved local policy; no automatic server copy | Owning OS profile and Pertamina subject; explicit use per document | Restore only from protected local backup if policy permits; never infer or recreate |
| PDF password | `X`; C Critical, I High, A operation-only | Protected UI-to-operation channel and memory hygiene; no persistent encryption substitute | Volatile process memory only while required | Destroy references and best-effort clear buffers immediately after operation | Current foreground operation only | Ask user again; never recover from database, log, crash dump, diagnostic, or sync |
| Issue screenshot/attachment | `R`; C High, I High, A Medium | Protected local staging; TLS; encrypted server object storage if sent | Local preview first; server only after explicit Send | Local staging short; server follows support-case retention | Preview/confirm, case ACL, and support need-to-know | Message must survive attachment failure; user chooses retry or send without attachment |
| Audit metadata | `A`; C Medium/High, I High, A High | Encrypted transport/storage under platform policy | Dedicated local outbox and server audit store; metadata only | Approved audit retention; immutable/append-oriented controls | Separated audit readers; application can append but not rewrite | Idempotent outbox delivery, gap detection, restore verification |
| Backups | Inherit highest contained class; C/I/A High | Mandatory approved backup encryption and key separation | Approved protected backup target only | Explicit retention, expiry, restore-test, and legal-hold rules | Backup operators and recovery service identities only | Periodic restore drills; reconcile database, objects, manifests, and keys |

Classification is applied to content, derivatives, filenames, paths, thumbnails, OCR text, indexes, diagnostics, and backups—not just the primary file.

## 2. Local storage security

### 2.1 Workspace isolation

1. Use one protected application root per OS profile/device and Pertamina subject. A sign-in change must not expose the previous subject’s SQLite database, artifacts, recent-file metadata, or signature library.
2. Apply restrictive OS ACLs/permissions at the root and inherit them to files. Do not use shared desktop, downloads, public documents, global temp, camera roll, or removable storage for working artifacts unless the user explicitly exports there.
3. Keep the user’s source path outside the managed workspace. Store only a reference/fingerprint in SQLite; do not copy the source unless an operation requires a local working copy.
4. The UI process should be the local data owner. Local workers receive only operation-scoped handles or typed IPC references, not unrestricted access to the whole workspace or SQLite file.
5. Exclude the protected local-only root, including SQLite, WAL/SHM, artifacts, thumbnails, and migration backups, from automatic consumer-cloud/device backup where the OS provides such a control. A platform without an enforceable exclusion is a release blocker for sensitive content until risk is approved.
6. Do not treat a hidden filename, application-package location, or random identifier as an authorization boundary; use OS permissions and authenticated app context.

### 2.2 Safe names and paths

- Generate opaque, cryptographically strong random internal names and create them exclusively so an existing file is never followed or replaced. The original filename is display metadata only.
- Normalize user-visible names to a documented Unicode form; remove or replace control characters and path separators; reject `.`/`..`, absolute/UNC/device paths, alternate data stream syntax, reserved device names, trailing-dot/space ambiguities, and overlong names. Preserve the original display name separately only if safe.
- Resolve the intended managed root and candidate path canonically, then require the final opened object to remain under that root. String prefix checks alone are insufficient.
- On Windows, reject or safely handle symbolic links, junctions, mount points, and other reparse points in managed roots. Inspect the opened handle/final path and revalidate before rename or delete. On mobile platforms, apply the equivalent no-follow/open-by-handle control.
- Cleanup must enumerate only manifest-owned identifiers below a verified root. It must never recursively delete a user-supplied path or follow links out of the managed root.

### 2.3 Overwrite prevention and atomic publication

Use this publication order: create a new private staging file with exclusive creation, write, flush/close, validate format and digest, then atomically rename within the same filesystem and commit the SQLite pointer. A result defaults to Save As/new artifact. Replacing an existing user file requires an explicit choice, confirmation of the expected file identity/fingerprint, a recoverable strategy appropriate to the platform, and failure behavior that leaves the prior file intact. Source files are never silent overwrite targets.

Time-of-check/time-of-use risk must be reduced by handle-based operations, exclusive creation, expected identity checks, and revalidation at publication. A filename selected earlier is not proof that the file is still the same object.

### 2.4 Crash residue and deletion limits

Every local artifact has an owner operation/workspace, generation, state, created/last-used time, expected digest/size where applicable, and cleanup eligibility in the manifest. Startup recovery reconciles manifests and files, resumes only verified artifacts, and quarantines or removes unreferenced residue after the recovery window.

PERTAMINA OTO must not claim absolute secure deletion on SSD, flash, copy-on-write filesystems, snapshots, or managed mobile storage. Deletion means removing application references and requesting OS deletion. Confidentiality after deletion relies primarily on encryption, key lifecycle, short retention, protected backups, and avoiding unnecessary copies. If overwrite is technically available, it is defense in depth only and must not be represented as guaranteed sanitization.

## 3. SQLite security

SQLite is the source of truth for device-local application state only. File payloads, PDF passwords, access/refresh tokens, encryption keys, and saved signature bytes do not belong in ordinary SQLite columns.

### 3.1 Protection boundary

- Treat the main database, `-wal`, `-shm`, migration backup, export, crash copy, temporary database, and diagnostic copy as one security boundary with the same ACL, encryption, backup exclusion, retention, and cleanup rules.
- Place the database in the per-profile protected root. Deny access to other local users and prevent remote/network-share placement.
- Select an approved, supported SQLite encryption provider—or an approved equivalent platform control—before production. The decision must cover authenticated integrity, WAL/SHM, backups, migrations, key rotation, crash recovery, and supported Windows/mobile platforms. “Database file encryption” that leaves WAL or backups clear is not sufficient.
- Store the local database key only in an OS-protected credential/key facility bound to the application and profile/device. Do not store the key beside the database or derive it solely from a predictable identifier.

### 3.2 Data and access controls

- Use parameterized queries and typed repositories; allowlist dynamic sort/filter fields. Never concatenate names, paths, OCR text, or metadata into SQL.
- Use one serialized local writer or an equivalent well-defined concurrency mechanism. Workers use typed local IPC and cannot submit arbitrary SQL.
- Store normalized internal paths or artifact IDs, never an unvalidated path that cleanup later executes. Persist file identity/fingerprint and digest separately from display name.
- Minimize recent-file metadata and do not retain it after the user clears history or loses access. A recent-file record never grants access to the file.
- Saved visual-signature metadata may be stored only after explicit save; its bytes remain a protected local artifact. Clipboard content, document bytes, OCR bodies, and thumbnails remain outside SQLite.
- Bound history, operation events, sync attempts, and error details. Error fields must be structured and scrubbed of document content, password, signature, clipboard, OCR text, tokens, and raw paths where not essential.

### 3.3 Integrity, migration, and recovery

Before a migration, create a consistent, encrypted, profile-scoped backup and record schema version/checksum. Migrations are transactional where SQLite permits, idempotent on restart, and tested through upgrade/downgrade-support policy. Application binaries must refuse an unknown newer schema rather than attempting a destructive reset.

On startup: verify the profile binding, open with the approved key, complete or roll back interrupted migration, run bounded integrity checks, reconcile artifact manifests, recover operations from persisted state, and surface an actionable error if corruption remains. Never silently replace a corrupt database with an empty one. Recovery may restore a verified backup or create a new database only after preserving the damaged copy under protected, bounded diagnostic handling and obtaining an explicit user/admin decision where data would be lost.

Backups must capture SQLite and referenced local artifacts with a signed/authenticated integrity manifest or equivalent approved integrity control. Restore must not overwrite an external source file and must revalidate every restored artifact before reuse.

## 4. Server DB boundary

SQL Server stores authoritative server metadata, authorization relationships, job/transfer state, secured-version records, audit metadata, retention state, and internal storage references. It does not store document payloads by default.

1. Desktop/mobile clients never connect directly to SQL Server. All access is through authenticated application APIs.
2. Use separate least-privilege service identities per module/service. Runtime identities receive only required data operations; schema migration/DDL uses a separate controlled identity. Avoid `db_owner`, shared administrator credentials, and human accounts for service runtime.
3. Keep module-owned schemas and enforce in-module foreign keys. Validate cross-module logical references through application contracts; an identifier is not authorization.
4. Use parameterized commands/ORM binding, allowlisted query shapes, bounded pagination, and database timeouts. Do not emit sensitive parameters to SQL tracing or application logs.
5. Use row version/optimistic concurrency, short transactions, uniqueness constraints, domain idempotency, and outbox patterns for integrity. Authorization is evaluated before a storage reference is returned or resolved.
6. Encrypt client/service-to-database connections with certificate validation. Disable trust-bypass configuration in production.
7. Apply approved SQL data-at-rest and backup encryption. Encryption is supplementary to network segmentation, firewall rules, service identity, least privilege, monitoring, and separation of duties.
8. Database auditing records actor/service, action, object ID, outcome, and correlation ID—not file content, OCR text, password, signature payload, clipboard data, or screenshot bytes.
9. Backups and replicas inherit the production data class. Restrict restore/export, encrypt backup media, test restores, and log privileged recovery access.

A SQL restore is incomplete until secured object existence, digest, encryption-key reference, version/tombstone state, and retention/legal-hold state are reconciled.

## 5. File/object-storage security

### 5.1 Storage zones and references

Use logically and permission-separated zones for upload quarantine, validated input, processing/checkpoint, job result, secured version, issue attachment, and export staging. A worker allowed to read quarantined input should not automatically have write/delete access to secured versions. Promotion between zones is an explicit state transition after validation.

Object names are opaque random identifiers and reveal neither user, original filename, business unit, nor document type. The SQL record holds the internal storage reference, digest, size, validated media type, encryption-key reference, state, owner/purpose, timestamps, and retention information. Never return a raw filesystem path, bucket key, share path, or durable unrestricted URL to a client.

### 5.2 Authorization and integrity

- Authorize user, object, purpose, operation, current version, and retention state before resolving the internal storage reference. Repeat authorization for `HEAD`, every range request, retry, result retrieval, export, and delete.
- A GUID, object key, ETag, storage reference, or possession of a URL is not proof of authorization.
- If pre-signed/delegated URLs are approved, make them single-object, single-action, short-lived, audience-bound where supported, TLS-only, non-listing, and issued only after the API authorization check. Avoid placing them in logs, browser history, analytics, or notifications.
- Calculate a strong content digest while streaming. Preserve size and digest through quarantine, promotion, processing input, result creation, secured-version activation, backup, and restore. Digest detects corruption/identity; it is not a substitute for authorization or malware inspection.
- Use conditional writes/version guards and exclusive object creation. Prevent accidental replacement of an active object by a reused key.
- Deny public access, directory/container listing, anonymous reads, cross-account exposure, and server-side fetch from arbitrary user URLs.

### 5.3 Secured versions and recovery

“Amankan ke Akun” creates a new immutable secured version from the explicitly selected file/result. Activation, version numbering, digest/reference persistence, and retirement of the oldest version when version four is accepted must be one recoverable domain operation. At most three versions remain active/addressable; older content is tombstoned and purged asynchronously with retry and audit metadata.

Object deletion must honor active download/processing leases, legal hold, retention, and backup policy. Orphan reconciliation operates from authoritative metadata and verified prefixes, not a blind whole-store scan. Restore reconciles SQL and object state before making a version downloadable.

## 6. Temp/cache/checkpoint security

| Artifact | Required controls | Cleanup trigger | Recovery behavior |
|---|---|---|---|
| Local temp write | Private per-operation directory, random exclusive name, restrictive ACL, byte limit, digest, no link following | Successful atomic publish, cancel, expiry, logout policy, or verified orphan | Reuse only when manifest, operation generation, size, and digest match |
| Server upload chunks | Quarantine zone, session/owner/purpose binding, committed-offset record, per-chunk/running digest, aggregate limit | Completion/promotion, abort, session expiry, or rejected validation | Resume at authoritative committed offset; conflicting chunk quarantined/rejected |
| Processing intermediate | Isolated worker directory/container, no internet by default, CPU/RAM/time/output quotas, no durable public path | Job terminal state plus short diagnostic grace, subject to lease | Resume from compatible verified checkpoint or restart idempotently |
| Cache | Regenerable, private, integrity checked, size/age bounded, never used as authorization truth | LRU/age/size pressure, parent removal, sign-out policy | Regenerate from currently authorized source |
| Thumbnail/preview | Classified like source, private, no gallery/indexing, avoid sensitive display name, bounded dimensions | Parent lifecycle, clear-history, expiry | Regenerate only after access check |
| Local checkpoint | Generation/schema/engine version, source fingerprint, digest, private and encrypted as required | Successful finalization, incompatible upgrade, explicit discard, retention expiry | Offer resume only if all bindings match; otherwise explain restart |
| Server checkpoint | Persisted job binding, version, source digest, engine/config fingerprint, storage digest, lease | Job/result retention expiry or superseded attempt | Status exposes safe checkpoint metadata, never content/path/key |
| OCR text/temp image | Restricted, no logs/index/public AI, private memory/artifact, output/size bounded | Operation finalization/cancel/crash recovery expiry | Re-run from authorized source; do not preserve hidden text copies |
| Scanner retake/discard | App-private capture staging, permission and user gesture, no automatic gallery save | Retake/discard, completed document publication, expiry | Recover verified completed pages only |
| Clipboard staging | Created only by paste action, bounded type/size, no clipboard history | Import completion/cancel/short expiry | User pastes again |
| Signature working copy | App-private, non-PKI, content excluded from telemetry/audit/sync | Cancel, completed placement, or explicit asset deletion | Original saved local asset remains unchanged unless explicitly deleted |

Temp, cache, thumbnail, checkpoint, and undo locations receive the same confidentiality protections as the source because they can reproduce it. “Temporary” is a lifecycle property, not a security classification.

Cleanup is lease-aware and idempotent. It first changes metadata to cleanup-eligible/tombstoned, then removes the object, then records completion. If removal fails, the item remains non-addressable and a bounded retry worker continues. Cleanup errors are observable without logging content or secrets.

## 7. Upload/download security

### 7.1 Upload

No upload occurs for a local-only operation. For optional-server processing, upload begins only after the resolver chooses server execution, policy/capacity allow it, and the user has the required notice/choice. Required-server features use the same secure transfer contract and must state that server transfer is required before content leaves the device.

1. Authenticate and authorize the feature, object purpose, size/type policy, and current capacity before accepting bytes.
2. Bind each one-shot request or resumable session to owner, purpose, job intent, declared length/type, idempotency key, correlation ID, expiry, and upload state.
3. Stream to quarantine; never buffer the whole file in API or worker RAM. Enforce the byte limit while reading even if `Content-Length` is absent or false.
4. Compute digest during the stream, inspect type/content, and keep the object non-executable/non-downloadable until validation and malware/security inspection complete.
5. For resumable sessions, accept only the authoritative offset/range, prevent overlapping/conflicting chunks, persist the committed offset after durable write, and return it to the client. Completion is idempotent.
6. Reject compressed/decompressed, page, pixel, object, attachment, and nesting limits before invoking expensive processing where possible. Apply resource quotas throughout validation.
7. Abort/reject creates a tombstoned quarantine object. An idempotent cleanup worker removes chunks after active leases end; raw upload paths/keys never appear in responses or logs.

### 7.2 Download and result retrieval

Every retrieval performs fresh authentication, authorization, ownership/purpose/version checks, and retention-state checks before resolving storage. Use safe `Content-Type`, sanitized `Content-Disposition`, `X-Content-Type-Options: nosniff`, private/no-store caching where appropriate, ETag, bounded `Range`, and `If-Range`. Do not inline active content when attachment delivery is safer.

The result endpoint returns status/metadata first; bytes are retrieved through an authorized download endpoint or short-lived delegated mechanism approved under Section 5. The client downloads to a new private staging file, verifies expected length/digest, validates the completed result, then offers Save As. It never overwrites the source automatically.

An expired result returns a stable expired/not-available state (normally `410 Gone` for a known, formerly available result) without recreating work silently. The UI explains expiry and offers an explicit retry/rerun if the source and policy still permit it. A missing object that should still exist is an integrity incident, not ordinary expiry, and requires reconciliation.

### 7.3 Transport controls

Use approved TLS versions/cipher policy and full certificate validation for API, upload, download, object storage, SQL, and service-to-service traffic. Never disable validation or accept any certificate in production. Sensitive headers, delegated URLs, filenames, and query values must be redacted from access logs. Upload/download telemetry records counts, safe sizes/buckets, status, duration, object/job IDs, and correlation ID—not payload, OCR, password, signature, or screenshot content.

## 8. File validation

Validation is layered and stateful. Extension, declared MIME, and filename are hints; none independently proves content type or safety.

### 8.1 Common validation pipeline

1. **Preflight:** feature allowlist, declared extension/MIME/size/count, owner/purpose, policy, capacity, and rate limit.
2. **Streaming gate:** actual byte count, strong digest, early magic-byte/signature sniff, aggregate session quota, and timeout.
3. **Type consistency:** compare normalized extension, declared MIME, detected signature, and parser result. Reject unsupported, ambiguous, polyglot, truncated, or mismatched files unless a documented safe conversion path exists.
4. **Structural parse:** use a patched, memory-safe or isolated parser under CPU/RAM/time/output quotas. Validate object counts, pages, dimensions, frames, recursion, compression ratio, expanded bytes, attachments, and external references.
5. **Security inspection:** approved malware/content scanner and format-specific policy. A clean scanner result reduces risk but is not a guarantee of harmless content.
6. **Quarantine decision:** only `READY/VALIDATED` artifacts can become job input or secured content. Failed/inconclusive/timeout states remain non-addressable and are cleaned by policy.
7. **Output validation:** validate generated output again before download or secure-version activation; ensure expected type, size, digest, page/dimension bounds, and feature-specific security properties.

### 8.2 Filename normalization

The system keeps a sanitized display filename separate from the internal object key. Normalize Unicode consistently; reject path separators, traversal, NUL/control characters, Windows device names, UNC/absolute paths, drive prefixes, alternate data streams, trailing dots/spaces, bidi/control ambiguity where unsafe, and excessive length. Never use a client filename in a filesystem path, shell command, SQL identifier, log template, response header, or object key without contextual encoding/allowlisting.

### 8.3 Format-specific controls

| Input | Threats | Required checks and containment |
|---|---|---|
| PDF | Malformed xref/object streams, parser exploits, JavaScript/actions, launch/URI actions, embedded files, recursive objects, huge pages/images/fonts, encrypted input | Strict structural parse in isolated worker; page/object/decompressed/output quotas; block or neutralize unsupported active/embedded content; password requested only for the operation; validate every generated PDF |
| Office/OOXML | Macro/VBA, OLE/embedded executable, DDE/external links, relationship traversal, ZIP slip, decompression bomb, parser/renderer exploit | ZIP entry count/path/ratio/expanded-size limits; macro/active-content policy; reject executable/OLE or unsupported external fetch; no desktop Office automation baseline; isolate approved provider with no arbitrary network access |
| Image | Decoder vulnerabilities, pixel/decompression bomb, huge dimensions/frame count, crafted ICC/EXIF, animation, metadata leakage, polyglot | Magic/decoder agreement; width/height/pixel/frame/metadata limits before full decode; patched isolated decoder; strip metadata only when the feature/policy requires; validate re-encoded output |
| Archive/container | Path traversal, symlink entries, nested archive bomb, duplicate/case-colliding names | Deny unless feature explicitly requires; canonical entry validation, no links/device paths, recursion/entry/ratio/expanded-size limits, extract only to verified private root |
| OCR/scanner input | Adversarial image/PDF, excessive resolution/pages, hidden text leakage, inaccurate output treated as trusted instructions | Same file/image gates; pixel/page/time limits; isolate OCR; classify output as restricted and untrusted; no automatic execution, public AI, or log/index persistence |
| Clipboard file/image | Spoofed advertised type, oversized delayed-render data, link/path substitution | Read only on explicit paste; copy into bounded private staging; validate actual bytes and current file identity; never trust clipboard type/path alone |

Parser timeout, scanner unavailable, password required, and “cannot determine safely” are explicit states. They are not treated as success and must not lead to silent waiting or background execution of unvalidated content.

## 9. Sensitive feature security

### 9.1 Protect/Unlock PDF password

- The PDF password is accepted through a masked UI and passed only to the active operation. It is never written to log, telemetry, SQLite, SQL Server, object metadata, audit, crash report, diagnostic bundle, chat/Assistant history, issue report, checkpoint, autosave, backup, idempotency record, queue payload, or event.
- Avoid immutable string copies where the selected .NET/library interface permits; minimize scope and lifetime, prevent accidental exception/trace capture, and best-effort clear mutable buffers after use. Process-memory erasure cannot be guaranteed and must not be marketed as such.
- Disable password reveal/copy or clipboard integration unless an approved usability/security decision explicitly permits it. Never echo the password in error messages.
- `Protect PDF` creates and validates a new protected result; it does not replace the source silently. `Unlock PDF` requires a valid password and produces a new result; it is not an authorization bypass or DRM feature.
- Resume after process termination asks for the password again. Password absence is an actionable `WAITING_FOR_USER` condition, not a reason to persist it.

### 9.2 Visual signature/paraf

Visual signature is non-PKI and must not be described as a digital certificate, cryptographic signature, identity proof, or non-repudiation mechanism. Treat the image as a sensitive user asset.

- Canvas/upload/paste requires explicit user action and preview. Do not save by default; offer an explicit “save for reuse on this device/profile” choice.
- A saved asset is a protected local file with a random internal name and minimal SQLite metadata. It is not automatically uploaded, synchronized, backed up to consumer cloud, included in telemetry, diagnostic bundle, audit payload, thumbnail cache, or issue screenshot.
- Each use requires an explicit placement/preview/final action. Audit only safe metadata such as “visual signature placed,” actor, time, document/result ID, and outcome—never the signature bytes or reusable template.
- Deletion removes the app reference and requests OS deletion, with the secure-deletion limitation stated in Section 2.4.

### 9.3 Redact PDF

Redaction must remove or irreversibly transform the targeted underlying content in the produced result; drawing an opaque overlay alone is not redaction. The feature must address text objects, images, annotations/layers, hidden/extractable content, and derived OCR/text representations within the redacted region as supported by the approved engine. Preview is not proof of removal.

The final result is validated through render comparison plus negative extraction/search/copy checks and structural inspection appropriate to the engine. Clear or invalidate previews, thumbnails, OCR caches, undo artifacts, and checkpoints that contain pre-redaction content according to lifecycle policy. Preserve the original source unchanged. The UI must confirm finalization and disclose unsupported content that prevents a safe redaction.

### 9.4 OCR, scanner, and clipboard

- OCR data is sensitive and untrusted. Run local-first, under resource limits, without public AI; do not execute recognized text as commands or persist it to logs/telemetry/search indexes.
- Scanner/camera access is requested only for the foreground feature. Capture into app-private staging, do not auto-save to the gallery, strip location/device metadata when it is not required, and promptly clean retakes/discarded frames.
- Physical scanner integration must use an approved provider/driver boundary and validate returned files as untrusted. Do not grant a scanner helper broad workspace or database access.
- Read clipboard only after an explicit paste/import gesture. Do not poll or retain a clipboard history. Preview and validate before creating a draft. Do not silently clear the OS clipboard because it may belong to another application; release only PERTAMINA OTO’s own copy.

### 9.5 Issue screenshot

Screenshot capture/upload is always explicit. Default capture should be limited to the application surface where technically possible, followed by a preview that makes other applications, filenames, account details, or document content visible before Send. Offer crop/redaction/removal. Strip unnecessary image metadata.

The local screenshot is short-lived protected staging. The server copy uses case-bound authorization, private object storage, malware/image validation, retention, and support need-to-know access. It is never included automatically in diagnostics. If upload fails, preserve the issue text and let the user retry or send without the attachment.

## 10. Selective-sync security

Selective sync is a narrow secured-file transfer, not a cloud workspace or replication of SQLite tables.

1. Only an explicit “Amankan ke Akun” action may upload the selected file/result. The confirmation identifies the exact local artifact, account, size, type, and network/server requirement.
2. Source, draft, undo/redo, operation state, cache, thumbnail, temp, checkpoint, clipboard, PDF password, visual signature, local history, and Assistant conversation remain local-only and have no generic sync mapping.
3. Execution upload for a server-required job is purpose-bound processing transfer and does not create a secured version. The two intents, endpoints, retention rules, and UI language remain distinct.
4. Before upload, bind the local immutable snapshot to a source/result fingerprint and digest. If the file changes after confirmation, stop and ask the user to confirm the new content; never continue using only the same path/name.
5. The secured-file request uses a domain idempotency key. Replays return the same operation/version outcome; they do not create additional versions.
6. Server activation verifies upload digest, owner, policy, current version state, and expected base ETag/version. Version conflicts are surfaced. Do not use timestamps or last-write-wins to overwrite another secured version silently.
7. The server keeps no more than three active/addressable versions. Acceptance of a fourth atomically activates the new version and tombstones the oldest; purge is retryable and auditable.
8. Download creates a new verified local working copy or user-selected Save As result. It does not replace a local source automatically.
9. Local sync-state/cache is advisory. Server persisted secured-version state is authoritative; a local record or stale ETag never grants server access.
10. Restore/reconciliation validates version sequence, object reference, size, digest, encryption-key reference, tombstone, retention, and owner before re-exposing content.

Conflict UI must identify “local changed,” “server version advanced,” “access changed,” “remote version expired/deleted,” and “integrity mismatch” separately. User action is required for content/version choice; the system must not choose a winner on the user’s behalf.

## 11. Encryption recommendations

| Boundary | Recommendation |
|---|---|
| Local device | Require supported OS storage protection and restrictive ACLs. Add approved application/SQLite authenticated encryption for restricted local state where the threat model requires protection beyond full-device encryption. Encrypt sensitive artifacts individually or through an approved protected container if they may survive at rest. |
| SQLite | Use one approved cross-platform solution covering DB, WAL/SHM, backups, migration copies, crash recovery, and key rotation. Do not ship an unverified custom cipher or cleartext fallback. |
| Local temp/cache/checkpoint | Inherit the source’s at-rest requirement; do not exempt temporary or regenerable derivatives. Prefer minimizing lifetime/copies in addition to encryption. |
| SQL Server | Enforce encrypted, certificate-validated connections; use approved database/storage and backup encryption. Consider field-level protection only for metadata whose threat model requires it, because it adds query/key complexity. |
| Object/file storage | Use provider-supported server-side encryption with enterprise-managed keys where required, plus private access and object-level integrity metadata. Quarantine, result, secured, and backup copies all inherit encryption. |
| Backup/export | Encrypt before or at the protected target, maintain integrity/authenticity, separate key access from backup access, and test that keys are available during authorized restore. |
| Network/API | Approved TLS policy end to end, no validation bypass, no mixed content. Service-to-service mutual authentication/managed identity is required; mTLS may be used where the platform security architecture approves it. |
| Local IPC | Restrict named pipe/local IPC to the current app/profile/service identity, reject remote clients, authenticate the peer, version messages, and authorize each operation. Do not rely on secrecy of a pipe name. |

Use approved, standards-based authenticated encryption and libraries; do not invent cryptography or hard-code algorithms without the Pertamina cryptographic standard. Encryption does not replace authorization, type validation, malware inspection, integrity checks, retention, or least privilege.

## 12. Key-management principles

1. **Separate keys from data.** Local keys live in the OS-protected key/credential facility; server keys live in an approved KMS/HSM or managed enterprise key service. Raw keys never appear in SQLite, SQL rows, configuration files, object metadata, source code, logs, telemetry, diagnostics, tickets, or chat.
2. **Use envelope encryption where applicable.** Store only an opaque key identifier/version and wrapped data key; restrict unwrap operations to the exact service identity and purpose. Do not expose key references to clients unless they are non-sensitive and required by contract.
3. **Least privilege and separation of duties.** Runtime services can perform only required encrypt/decrypt operations. Key administrators, storage administrators, database operators, developers, and support staff do not automatically gain document-read access.
4. **Lifecycle.** Define generation, activation, rotation, rewrap/re-encryption, suspension, revocation, expiry, destruction, backup, disaster recovery, and compromise response. Rotation must not make existing authorized files unrecoverable.
5. **Local profile/device binding.** A copied local database/artifact should not become readable merely by copying the adjacent files. Define behavior for OS account removal, app reset, device loss, logout, and corporate device recovery without silently deleting user drafts.
6. **Availability.** Key-service outage fails closed for sensitive writes/reads while returning an actionable retriable status. Queueing must not store plaintext as a workaround.
7. **Audit.** Record safe key-use metadata—service identity, key ID/version, operation type, result, time, correlation ID—without raw key, plaintext, password, or document content.
8. **Memory.** Minimize plaintext/key lifetime, avoid unnecessary copies and dumps, and best-effort clear mutable buffers. Do not promise complete memory erasure under managed runtime/OS conditions.
9. **No custom crypto and no embedded master key.** Cryptographic provider, key custody, recovery/escrow, rotation periods, and approved algorithms require Security/IAM/Platform approval.

## 13. Cleanup/retention

Exact durations must be policy-driven configuration, not hard-coded guesses. Defaults follow data minimization: retain only as long as needed for the user-visible operation, recovery promise, security investigation, audit obligation, or legal hold.

| Data/artifact | Retention rule | Cleanup and evidence |
|---|---|---|
| External source/final file | User-controlled | Never app-deleted unless the user explicitly requests deletion of an app-owned result and the platform contract permits it |
| Active local draft/checkpoint | While workspace/recovery promise applies | Manifest tombstone, lease check, delete request, cleanup outcome; prompt before destructive loss where user work is affected |
| Local temp/intermediate | Short operation/recovery grace | Startup and scheduled manifest reconciliation; idempotent delete; no traversal/link following |
| Cache/thumbnail/preview | Short, age/size bounded | LRU/expiry plus parent lifecycle; deletion is regenerable and non-blocking |
| Clipboard copy | Operation lifetime | Release memory/staging after import/cancel; no history |
| PDF password | Only while operation needs it | Drop references and best-effort clear buffers; no recovery copy |
| Saved visual signature | Until explicit local deletion or approved device policy | Remove reference and request OS deletion; note media limitation; never auto-sync |
| Upload session/chunks | Until complete/abort/short expiry | Tombstone, end leases, idempotent purge, orphan reconciliation |
| Processing input/intermediate/checkpoint | Job plus bounded recovery/diagnostic window | Terminal-state lifecycle; retry cleanup; safe metadata retained as required |
| Job result | Explicit, communicated retrieval window | Expiry makes result non-addressable; object purge; status may retain safe evidence |
| Secured versions | Up to three active/addressable versions plus purge workflow | Fourth activation tombstones oldest atomically; retry purge; legal hold overrides deletion if authorized |
| Issue screenshot | Support-case policy only | Delete local staging after send/cancel; server lifecycle tied to case and access review |
| Audit metadata | Approved audit schedule | Controlled purge/archival, legal hold, integrity evidence |
| SQL/object/SQLite backups | Approved recovery schedule | Encrypted expiry and restore drill; deletion from active store does not imply immediate disappearance from valid backups |

Cleanup workers are rate-limited and storage-aware. They do not compete aggressively with foreground jobs, and they expose backlog, oldest eligible item, failures, and storage pressure without exposing filenames/content. Emergency pressure cleanup removes only regenerable/expired artifacts; it never deletes active drafts, checkpoints within the promised recovery window, or secured versions to free space silently.

## 14. Threat scenarios

| Scenario | Security impact | Detection/recovery expectation |
|---|---|---|
| Another local OS user copies workspace/SQLite/WAL | Restricted content and metadata disclosure | ACL/encryption denial; access audit where available; revoke local keys/device through approved incident process |
| Predictable temp name is pre-created or swapped | Overwrite, data injection, disclosure | Random exclusive create, final-handle path check, operation digest mismatch, fail safely |
| `../`, UNC, ADS, reserved name, or header injection in filename | Read/write outside root, unsafe response/log | Normalization rejection and security event with scrubbed name |
| Junction/symlink/reparse point redirects cleanup | Arbitrary file read/delete | No-follow/handle verification; stop cleanup, quarantine manifest entry, raise integrity alert |
| Source changes between selection and save | Wrong-file overwrite or stale processing | File identity/fingerprint/ETag mismatch; ask user to reselect/confirm |
| Crash leaves plaintext temp/OCR/signature copy | Confidentiality residue | Protected root, manifest recovery, short grace, idempotent cleanup, encryption/key controls |
| SQLite corruption or migration interruption | Loss of drafts/jobs/history | Consistent backup/migration journal/integrity check; no silent reset; explicit restore/repair |
| SQL injection or overprivileged service account | Metadata compromise and object-reference disclosure | Parameterization, least privilege, query monitoring, credential/key rotation, restore/audit review |
| Client tampers with object/storage reference | IDOR and unauthorized download | Ignore client storage keys; authorize domain object then resolve server-side reference |
| Extension/MIME spoof or polyglot | Parser exploit or policy bypass | Multi-signal type detection, strict parser, quarantine, mismatch rejection |
| ZIP/decompression/pixel bomb | CPU/RAM/storage exhaustion | Preflight plus streaming expanded-size, ratio, nesting, page/pixel/frame quotas and worker kill |
| Malformed PDF exploits parser | Worker compromise or denial of service | Patched isolated provider, no network, low privilege, quotas, output revalidation |
| Office macro/OLE/external link | Code execution or data exfiltration | Active-content policy, isolated non-Office-automation provider, deny external fetch, reject unsupported objects |
| Crafted image/ICC/EXIF exploits decoder | Code execution, memory exhaustion, privacy leak | Patched isolated decoder, dimension/frame/metadata limits, controlled re-encode |
| OCR text leaks to logs/search/public AI | Restricted-content disclosure | Data-flow prohibition, log tests, local-first provider, cleanup and incident response |
| Scanner helper or clipboard polling over-collects | Unintended collection | Foreground permission/gesture, scoped helper, no polling/history, preview and validation |
| Visual signature is synced or included in diagnostics | Reusable sensitive asset disclosure/misuse | No server mapping, diagnostic allowlist, local encryption, explicit use, metadata-only audit |
| PDF password captured by logging/crash/queue | Secret disclosure | Structured redaction, dump policy, ephemeral operation input, test canaries, ask again after restart |
| Redaction only draws a black box | Recoverable sensitive content | Structural removal/flattening, extraction/search/copy negative tests, cache/OCR invalidation |
| Upload fails after partial transfer | Orphaned restricted bytes/storage exhaustion | Persisted session state, expiry/tombstone, lease-aware purge, bounded retry/backpressure |
| Unauthorized `HEAD`/range/download replay | Partial/full content disclosure | Authorization on every method/range, short-lived delegation, ETag does not grant access |
| Secured upload replay creates versions 2–3 | Premature version eviction and duplication | Domain idempotency plus unique constraints; replay returns original version outcome |
| Local/server version conflict is resolved by timestamp | User content/version loss | Fingerprint/digest/ETag conflict state and explicit user decision |
| Object corrupted/missing after SQL restore | Broken or incorrect secured version | Reconciliation verifies existence, digest, key reference, version/tombstone before exposure |
| Issue screenshot captures other apps/sensitive content | Support-channel disclosure | App-surface capture, preview/crop/redact, explicit send, case ACL and retention |
| Ransomware/device/storage/key-service outage | Availability loss | Protected backups, restore drills, offline Local-First path where valid, explicit retriable/fail-closed status |

## 15. Mitigations

The following traceability list addresses each requested analysis topic directly.

| # | Topic | Required mitigation |
|---:|---|---|
| 1 | Local workspace isolation | Per OS profile/device and Pertamina subject; restrictive ACL; protected root; typed worker access; cloud-backup exclusion |
| 2 | Temp directory safety | App-managed private roots, random exclusive files, manifest ownership, limits, leases, canonical-root cleanup |
| 3 | Cache permissions | Same private ACL/classification as source; no shared/public cache; size/age bounds; regenerate after authorization |
| 4 | Predictable filename risk | Cryptographically random opaque internal names; exclusive creation; original name only as sanitized metadata |
| 5 | Path traversal | Reject absolute/traversal/device/ADS forms; canonical/handle-based containment; no client path in storage key |
| 6 | Symlink/reparse point | No-follow semantics, reparse inspection, final-handle path verification, no recursive cleanup through links |
| 7 | File overwrite prevention | Staged atomic publish, new result by default, expected file identity, explicit confirmed replacement only |
| 8 | Secure deletion limitation | No absolute claim; rely on minimization, encryption/key lifecycle, protected backups, and OS deletion request |
| 9 | Crash residue | Artifact manifest, protected staging, startup reconciliation, verified resume, bounded orphan cleanup |
| 10 | SQLite protection | Per-profile DB, approved encryption including WAL/SHM/backups, OS key store, parameterization, one writer, no secrets/payload |
| 11 | SQL Server access | API-only, module/service identities, least privilege, separate migrations, encrypted connection, audit and backup controls |
| 12 | Storage-reference security | Opaque internal reference; authorize domain object first; no raw key/path/durable URL; integrity metadata and reconciliation |
| 13 | Encryption at rest | Approved controls for local artifacts/SQLite, SQL, object storage, temp, cache, checkpoints, and backups |
| 14 | Encryption in transit | Certificate-validated TLS end to end; service mutual authentication; no validation bypass or sensitive URL logging |
| 15 | Key management | OS key store locally, KMS/HSM server-side, separation, rotation/recovery/audit, no key beside data/custom crypto |
| 16 | Upload validation | Auth/purpose/capacity preflight, stream-to-quarantine, byte counter/digest, structural/security scan, promotion only after ready |
| 17 | Filename normalization | Unicode policy, remove/reject unsafe characters/names/length, contextual header encoding, separate display name/object key |
| 18 | Extension vs MIME | Treat both as hints; compare extension, declared MIME, magic bytes, parser result; reject mismatch/ambiguity |
| 19 | Magic-byte/content validation | Streaming signature sniff plus strict structural parse and output revalidation; never rely on signature alone |
| 20 | File-size limit | Enforce declared and actual bytes, aggregate/session/result limits; communicate policy and reject early |
| 21 | Decompression bomb | Compressed/expanded bytes, ratio, entries, recursion, pages, pixels, frames, output and time quotas |
| 22 | Malformed PDF | Isolated patched parser, low privilege/no network, object/page quotas, active/embedded-content policy, timeout |
| 23 | Malicious Office file | Macro/OLE/DDE/external-link and ZIP controls; isolated approved provider; no desktop Office automation baseline |
| 24 | Image attack surface | Patched isolated decoder, pixel/dimension/frame/metadata limits, type agreement, safe output validation |
| 25 | OCR temporary content | Restricted/untrusted classification, local-first, protected bounded temp, no logs/index/public AI, prompt cleanup |
| 26 | Scanner input | Explicit permission/capture, app-private staging, scoped driver/provider, validate as untrusted, clean retakes |
| 27 | Clipboard image | User paste gesture only, no polling/history, bounded private copy, validate actual bytes/current path identity |
| 28 | Saved visual signature | Non-PKI sensitive local asset, explicit save/use/delete, encryption as required, no auto-sync/telemetry/audit payload |
| 29 | Redact PDF | Remove/flatten underlying content, negative extraction tests, output validation, pre-redaction derivative cleanup, source preserved |
| 30 | Protect/Unlock password | Volatile operation-only use; never log/store/sync/diagnose; valid password required; ask again after restart |
| 31 | Selective-sync integrity | Explicit immutable snapshot, digest/fingerprint, purpose separation, ETag/version conflict, user decision |
| 32 | Version integrity | Immutable version, unique sequence/idempotency, digest/reference/key metadata, atomic fourth-version tombstone |
| 33 | Failed upload cleanup | Persisted session state, tombstone, expiry, lease-aware idempotent purge, orphan/backlog monitoring |
| 34 | Download authorization | Fresh owner/object/purpose/version check for GET/HEAD/range; safe headers; verified staging; no source overwrite |
| 35 | Issue screenshot | App-surface capture where possible, preview/crop/redact, explicit send, protected staging/storage, case ACL/retention |
| 36 | Backup/restore | Encrypt/integrity-protect, least privilege, key availability, restore drill, SQL-object-local manifest reconciliation |

## 16. Acceptance criteria

### 16.1 Local files and workspace

- [ ] Two OS users or two Pertamina subjects on one device cannot enumerate or open each other’s SQLite, WAL/SHM, artifacts, thumbnails, checkpoints, or signatures.
- [ ] Local-only roots are demonstrably excluded from automatic cloud/device backup on every supported platform, or the release is blocked/explicitly risk-accepted.
- [ ] Tests with traversal, UNC, device names, ADS, Unicode ambiguity, long names, symlinks, junctions, and reparse points cannot escape managed roots during read, write, rename, or cleanup.
- [ ] Internal filenames are unpredictable and exclusively created; concurrent operations cannot replace each other’s files.
- [ ] Killing the app at every artifact-publication step leaves either the previous valid file or the new complete valid file, never a silent source replacement.
- [ ] Startup recovery identifies valid checkpoints, stale leases, partial files, and orphans without scanning/deleting outside the manifest root.
- [ ] Product and support text make no absolute secure-delete claim.

### 16.2 SQLite and server data

- [ ] SQLite encryption/protection covers DB, WAL, SHM, migration backups, crash copies, and local backup artifacts; no cleartext fallback is possible in production.
- [ ] A copied SQLite/artifact set is unreadable outside its authorized profile/device boundary under the approved threat model.
- [ ] PDF passwords, tokens, raw keys, signature bytes, document bytes, OCR bodies, clipboard content, and thumbnails are absent from ordinary SQLite and SQL tables.
- [ ] All SQLite/SQL queries are parameterized; dynamic query fields are allowlisted; fuzz tests cannot alter query structure.
- [ ] Local workers cannot issue arbitrary SQLite queries or access unrelated workspace artifacts.
- [ ] A failed SQLite migration restarts or restores safely and never silently creates an empty database.
- [ ] Desktop/mobile clients have no SQL Server credentials or network route for direct database access.
- [ ] Runtime database identities cannot perform schema administration or read another module beyond the approved contract.

### 16.3 Object storage and transfer

- [ ] No API response/log exposes raw object key, filesystem/share path, reusable storage credential, or durable unrestricted download URL.
- [ ] Changing a public ID, ETag, range, upload-session ID, or storage-like token cannot access another user’s content.
- [ ] Uploads that exceed actual-byte, expanded-byte, page, pixel, frame, entry, recursion, CPU, RAM, output, or time limits terminate predictably and remain quarantined.
- [ ] Unsupported/mismatched extension, MIME, magic bytes, parser result, corrupted structure, or inconclusive scan cannot transition to `READY`.
- [ ] PDF, Office, image, archive, OCR, and generated outputs are processed in the approved isolated/limited provider boundary and revalidated.
- [ ] Failed/expired upload sessions become non-addressable and are eventually purged; cleanup can be retried without deleting an active upload.
- [ ] Every GET, HEAD, and range retrieval rechecks authorization and current retention/version state.
- [ ] Download verifies length/digest into private staging and never overwrites a source without explicit confirmed user action.
- [ ] Expired result is clearly reported and cannot be revived or rerun without explicit user action.

### 16.4 Sensitive features

- [ ] Canary PDF passwords do not appear in application/server logs, telemetry, database, audit, dumps collected by normal diagnostics, queue/event payloads, backups, chat, or issue diagnostic bundles.
- [ ] Restart/resume of Protect/Unlock asks for the password again.
- [ ] Unlock fails closed for an invalid/missing password and never alters the source.
- [ ] Visual signature save is opt-in; its bytes remain local and are absent from sync, upload, telemetry, audit payload, and diagnostics.
- [ ] Signature UI and documentation label the feature non-PKI and make no cryptographic/non-repudiation claim.
- [ ] Redaction tests cannot recover targeted content through extraction, search, copy, hidden layer/annotation access, OCR derivative, or ordinary rendering paths supported by the approved engine.
- [ ] Scanner retakes, OCR intermediates, and clipboard staging are cleaned and never enter logs/public AI/generic indexes.
- [ ] Clipboard is accessed only after a paste/import gesture and no app clipboard history is created.
- [ ] Issue screenshot requires preview and explicit Send; attachment failure does not discard the issue message.

### 16.5 Selective sync, retention, and recovery

- [ ] Network activity tests prove that source, draft, undo, cache, thumbnail, temp, checkpoint, clipboard, password, signature, and local history do not auto-sync.
- [ ] Replaying the same secured-file idempotency key returns the original outcome and does not consume another version slot.
- [ ] A local file modified after confirmation is not uploaded under the earlier digest without a new user confirmation.
- [ ] Server/local ETag or digest conflict is presented to the user; no last-write-wins decision occurs silently.
- [ ] Creation of a fourth secured version leaves exactly three active/addressable versions and tombstones the oldest in a recoverable operation.
- [ ] Backup restore drills verify SQL metadata, object existence/digest/key reference/version/tombstone, SQLite manifest/artifacts, authorization, and key availability before release.
- [ ] Cleanup backlog, failures, and storage pressure are observable without document names/content/secrets, and emergency cleanup never silently removes active user work.

## 17. Risks/conflicts

No direct conflict was found between the requested baseline and Documents 01–07 FINAL. The following items remain decision or implementation blockers for a production security claim:

| Decision/dependency | Risk if unresolved | Required owner/output |
|---|---|---|
| Official Pertamina information-classification mapping | Working `I/C/R/X/A` labels may not match corporate handling rules | Information Security/Data Governance mapping and handling matrix |
| SQLite encryption provider and local key lifecycle | DB/WAL/backup leakage or unrecoverable local state | Security + Desktop/Mobile Architecture approved cross-platform design/PoC |
| Windows/mobile backup-exclusion mechanism | Local-only data could enter consumer cloud/device backup | Platform-specific verified configuration and release gate |
| Object storage provider, tenancy, KMS, delegated-download pattern | Incorrect ACL/key/URL isolation | Platform/Security deployment design and threat model |
| SQL encryption/topology, backup and privileged access | Metadata/backup exposure and uncertain recovery | DBA/Platform baseline plus restore drill |
| Exact retention, result expiry, issue attachment, audit, and legal hold | Excess retention or premature data loss | Data Governance/Legal/Operations retention schedule |
| Malware/content inspection provider and outage policy | Unsafe promotion or blocked operations | Security Engineering provider selection and fail-closed/inconclusive policy |
| PDF/Office/image/OCR/scanner provider feasibility | Parser escape, incomplete redaction, unsupported formats | Provider PoCs and security test evidence; align with Document 04 provider gates |
| Object-level authorization and support permissions | IDOR/support overreach | Document 09 authorization matrix and object ownership rules |
| Audit event details and diagnostic allowlist | Sensitive payload leakage or inadequate evidence | Document 10 schema/redaction policy |
| RPO/RTO and backup/restore topology | Availability promises cannot be tested | Document 17 operational targets and recovery runbook |
| Managed-runtime dump/memory policy | Password/plaintext may appear in privileged dumps | Endpoint/Operations crash-dump collection, access, redaction, and retention controls |

The report intentionally does not introduce DRM, PKI, automatic cloud workspace, permanent server document retention, or automatic signature synchronization. Visual signing remains non-PKI. Exact cryptographic algorithms, key periods, retention durations, and file-size limits must be approved configuration; inventing them here would create false assurance.

## 18. Suggested Document 08 text

### Data, file, storage, and local-security requirements

PERTAMINA OTO SHALL preserve the Local-First boundary. A local-only operation SHALL NOT upload document content. Source files, drafts, working state, undo/redo, cache, thumbnails, temporary files, checkpoints, clipboard content, PDF passwords, visual signatures, and local history SHALL NOT be automatically synchronized. “Amankan ke Akun” SHALL transfer only the file or result explicitly selected by the user and SHALL maintain no more than three active/addressable secured versions.

All document payloads and derivatives—including OCR text, thumbnails, previews, checkpoints, scanner frames, clipboard staging, and redaction intermediates—SHALL inherit the source confidentiality classification. Official Pertamina classification and retention policy SHALL replace engineering working labels before production.

The local workspace SHALL be isolated per OS profile/device and Pertamina subject using restrictive operating-system permissions. SQLite, WAL/SHM, local artifacts, caches, migration backups, and crash residue SHALL share the same protected boundary and SHALL be excluded from automatic cloud/device backup where supported. A missing enforceable exclusion for sensitive local-only data SHALL be a release-blocking risk unless explicitly accepted.

Internal filenames SHALL be opaque and unpredictable. File creation SHALL be exclusive. All paths SHALL be normalized, contained under an approved root, and protected against traversal, absolute/device paths, symlinks, junctions, mount points, and reparse points using final-object/handle verification where available. Cleanup SHALL operate only on manifest-owned artifacts and SHALL never follow a user-controlled link or recursively delete a user-supplied path.

File output SHALL use private staging, close/flush, format and digest validation, and atomic publication. The default SHALL be a new result/Save As. The application SHALL NOT overwrite a source silently; any replacement SHALL require explicit confirmation and expected-file identity validation, and failure SHALL leave the previous file intact.

SQLite SHALL contain local metadata and recovery state, not file payloads or secrets. The selected protection SHALL cover the database, WAL, SHM, migration copies, crash copies, and backups and SHALL store its key separately in an approved OS-protected facility. Workers SHALL use typed IPC rather than arbitrary SQLite access. Corruption or migration failure SHALL NOT cause a silent reset.

SQL Server SHALL store metadata, authorization relationships, job/transfer state, secured-version state, retention state, and audit metadata. File payloads SHALL reside in private secure object/file storage by default. Desktop/mobile clients SHALL NOT connect directly to SQL Server. Runtime and migration identities SHALL be separate and least-privileged, and all database transport and backups SHALL use approved protection.

Storage references SHALL be internal, opaque, and never proof of access. The server SHALL authorize the actor, object, purpose, operation, version, and retention state before resolving a storage reference for upload, download, HEAD, range, retry, export, or deletion. Raw paths, bucket keys, shares, and durable unrestricted URLs SHALL NOT be exposed to clients or logs. Objects SHALL carry validated type, size, strong digest, state, owner/purpose, retention, and encryption-key reference metadata.

Uploads SHALL be accepted only after authentication, policy/capacity preflight, and—where applicable—explicit user choice. Bytes SHALL stream to quarantine under actual-byte and resource limits. Extension, declared MIME, magic bytes, structural parsing, corruption/password state, decompressed size, page/object/pixel/frame/entry limits, embedded/active content, and approved malware/security inspection SHALL be evaluated before an artifact becomes job input or secured content. Generated output SHALL be revalidated.

PDF, Office, image, archive, OCR, and scanner content SHALL be treated as untrusted even after validation. Parsers and converters SHALL run patched, isolated, least-privileged, network-restricted where appropriate, and subject to CPU, RAM, time, input, expanded-size, and output quotas. Inconclusive validation or scanner timeout SHALL fail closed and produce an actionable status.

Downloads SHALL reauthorize every request and range, use safe content headers, and write to a new private local staging file. The client SHALL verify expected size and digest before Save As and SHALL NOT overwrite the source automatically. An expired result SHALL be reported explicitly and SHALL require a new user-initiated retry/rerun; a missing unexpired object SHALL be treated as an integrity/reconciliation incident.

PDF passwords SHALL exist only in volatile memory for the operation that needs them. They SHALL NOT be logged, telemetered, persisted in SQLite/SQL/object metadata, placed in audit/events/queues/checkpoints/backups/chat, or included in diagnostics or issue reports. After restart or resume, the user SHALL be asked again. Protect SHALL create a validated new protected result. Unlock SHALL require a valid password and SHALL create a new result; it SHALL NOT be presented as DRM bypass.

Visual signature/paraf SHALL be treated as a sensitive non-PKI user asset. Saving it for reuse SHALL be opt-in and local to the protected profile. It SHALL NOT be automatically uploaded, synchronized, telemetered, included in diagnostics, or stored as audit payload. Audit SHALL record action metadata only. The product SHALL make no cryptographic-signature, certificate, identity-proof, or non-repudiation claim.

PDF redaction SHALL remove or irreversibly transform targeted underlying content; an opaque overlay alone SHALL NOT qualify. The final result SHALL pass approved rendering, structural, extraction/search/copy, and derivative-content checks. Pre-redaction previews, thumbnails, OCR caches, undo artifacts, and checkpoints SHALL be invalidated or cleaned according to policy. The original source SHALL remain unchanged.

OCR text, scanner input, clipboard content, and issue screenshots SHALL be restricted and untrusted. OCR SHALL be local-first, SHALL NOT use public AI, and SHALL NOT enter logs, telemetry, or generic indexes. Scanner/camera capture and clipboard read SHALL require foreground user action. Clipboard polling/history SHALL be prohibited. Issue screenshots SHALL require preview and explicit Send, SHALL support removal/crop/redaction, and SHALL use case-bound authorization and retention if uploaded.

Temporary artifacts, caches, checkpoints, failed uploads, job intermediates, results, issue attachments, and secured versions SHALL have explicit owners, states, leases, retention, and idempotent cleanup. Cleanup SHALL tombstone before removal, respect active leases/legal hold, retry bounded failures, and expose safe backlog metrics. The system SHALL NOT claim absolute secure deletion on modern storage; confidentiality SHALL rely on minimizing copies and retention, encryption and key lifecycle, access control, protected backup, and OS-supported deletion.

All local, server, object-storage, transfer, IPC, and backup encryption SHALL use approved standards-based providers and enterprise key management. Keys SHALL be separate from data, least-privileged, versioned, rotatable, recoverable under authorized disaster recovery, and absent from code/configuration/logs/telemetry/diagnostics. Encryption SHALL NOT replace authorization, validation, integrity checking, isolation, or retention controls.

Backup and restore SHALL protect confidentiality and integrity, restrict operator access, and be tested. A server restore SHALL reconcile SQL metadata with object existence, digest, key reference, version/tombstone, retention, and authorization before content becomes addressable. A local restore SHALL validate the SQLite/artifact manifest and SHALL NOT overwrite an external source. No corrupt store SHALL be silently replaced with an empty store.
