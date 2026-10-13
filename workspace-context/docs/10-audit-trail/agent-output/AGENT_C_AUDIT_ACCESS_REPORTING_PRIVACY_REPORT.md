# AGENT C — AUDIT ACCESS, REPORTING & PRIVACY REPORT

| Atribut | Nilai |
|---|---|
| Task ID | `DOC10-AGENT-C` |
| Fokus | Audit access, reporting, audit-of-audit, search/export, dan privacy |
| Status | **REKOMENDASI — BELUM LOCKED** |
| Tanggal | 26 Agustus 2026 |
| Decision owner | User + Integrator |

## Baseline dan source note

Laporan ini diturunkan dari Document 01–09 FINAL. Document 09 FINAL v1.0 telah direkonsiliasi bersama `ROLE_PERMISSION_DECISION_REGISTER.md` (18/18 auto-locked) dan `ROLE_MATRIX_INTEGRATION_AUDIT.md` (PASS). Keputusan Role Matrix tidak dibuka ulang atau diperluas.

Traceability utama:

- `DEC-REQ-011`, `FR-AUDIT-001..004`, `FR-HIST-001..003`, `FR-SUP-001..007`;
- `SR-SEC-004`, `SR-SEC-007`, `SR-SEC-011`, `SR-SEC-015..016`, `SR-SEC-019..021`;
- `FEAT-AUDIT-001..008`, `FEAT-HIST-001..015`, `FEAT-SUPPORT-001..008`;
- API Final sections 9, 10.5–10.6, 15, 16.3, dan 18;
- Database Final sections 7.5–7.6, 8.5–8.6, 16–18;
- Security Final sections 6, 11–12, 23, dan 30;
- Role Matrix Final sections 6, 8, 10–12, 15.1, 26, 31–33, dan 39–40;
- `ROLE-DEC-002..005`, `ROLE-DEC-009..014`, dan `ROLE-DEC-018`.

Prinsip yang tidak dibuka ulang:

- User History, Enterprise Audit, Support, dan Observability adalah data plane berbeda;
- role hanya permission grouping; permission saja tetap tidak cukup tanpa scope/policy/context;
- formal role tetap User, Administrator, dan Auditor;
- Superadmin adalah Support permission profile, bukan formal role, wildcard, atau root;
- Auditor controlled-read; export terpisah; ordinary Audit update/delete tidak tersedia;
- Administrator dan Support tidak otomatis memperoleh Audit Console;
- Audit tidak memberi document-content authority;
- detail event/field/retention/reporting yang diusulkan tetap keputusan user + Integrator.

---

## 1. Audit access model

### 1.1 Separation model

```text
User History
→ convenience projection untuk user atas proses miliknya
→ dapat dihapus user sesuai policy

Enterprise Audit
→ append-oriented controlled evidence
→ permission + scope + field projection
→ tidak dapat diubah/dihapus ordinary user

Support
→ issue, explicit attachment, triage, safe diagnostics
→ bukan Enterprise Audit Console

Observability
→ health, metrics, traces, alerting
→ bukan business audit dan bukan content store
```

Satu plane tidak boleh menyalin restricted payload dari plane lain hanya agar pencarian lebih mudah. Correlation ID, Job ID, Issue ID, dan Feature ID dapat menghubungkan investigasi secara aman, tetapi tidak memberikan authorization atau content access.

### 1.2 Authorization formula

```text
ALLOW_AUDIT_ACCESS = authenticated and active principal
    AND online valid session
    AND explicit audit/support/admin-transparency permission
    AND server-derived organizational/resource scope
    AND permitted event category and event type
    AND permitted field projection
    AND permitted date/environment scope
    AND current policy/classification
    AND fresh reauthentication when required
```

Role name, UI menu, opaque ID, cursor, filter, export request, Job ID, Correlation ID, or previous access never menjadi authority.

### 1.3 Access projections

| Projection | Consumer | Isi | Tidak termasuk |
|---|---|---|---|
| `USER_SECURITY_TRANSPARENCY` | User, own account only | Selected own account/session/security events yang aman | Enterprise Audit Console, other-user events, raw device/IP, admin identity detail |
| `ADMIN_CONTROL_PLANE_EVIDENCE` | Administrator dengan explicit read permission | Admin/policy/config/operational-security changes dalam delegated scope | Document content, all-user audit, unrelated issue/audit payload |
| `AUDITOR_CONTROLLED_EVIDENCE` | Auditor dengan query/detail permission | Safe audit evidence sesuai organization/category/environment/time scope | Unlimited scope, source mutation, content, secret |
| `SUPPORT_ISSUE_EVIDENCE` | Support dengan issue permission | Issue-linked safe event projection untuk troubleshooting | General audit search/export, unrelated security/admin events, file/result content |
| `AUDIT_LIFECYCLE_EVIDENCE` | Auditor/Compliance atau authorized audit custodian | Query/detail/export/retention/archive/purge evidence | Payload yang sedang diaudit, secret, archived document content |

### 1.4 WHO can see WHAT

| Evidence category | User | Administrator | Auditor | Support/Superadmin |
|---|---|---|---|---|
| Own feature-processing activity | **User History**, bukan Audit Console | Aggregate/safe operational metadata bila permission | Scoped safe audit events | Issue-linked safe subset only |
| Own login/session/security transparency | Own selected safe projection, conditional | Only with explicit admin/security evidence permission and scope | Scoped read | Issue-linked only if relevant |
| Role/permission/account-eligibility changes | Own-effect notification/projection bila approved | Scoped change evidence with permission | Scoped read/detail | No default |
| Policy/config/feature/limit changes | No | Scoped read of relevant changes; write permission does not imply audit read | Scoped read/detail | No default |
| Authentication/security events across users | No | Explicit security-evidence permission only | Scoped read/detail | No; issue-linked safe subset only |
| Denied sensitive access | Own safe denial result only | Scoped admin/security evidence | Scoped read/detail | Own support action or issue-linked subset |
| Support issue activity | Own user-visible issue status, not internal audit | No by Administrator role alone | Scoped safe support-activity evidence | Own scoped case actions/detail |
| Audit query/detail/export/lifecycle | No | Explicit Audit grant only | Scoped read; export separately granted | No default |
| Document/OCR/issue screenshot/prompt content | No via Enterprise Audit | **NO** | **NO** | **NO** via Audit; issue attachment follows separate Support permission |

### 1.5 Enforcement

- scope diterapkan sebelum count, filter, search, sort, cursor, detail, projection, atau export;
- list dan detail memakai field projection berbeda;
- exact identifiers di-authorize sebelum digunakan sebagai filter;
- every cursor continuation, detail view, export generation, artifact download, archive retrieval, dan purge action di-authorize ulang;
- permissions/policy/scope changes menginvalidasi cursor dan pending access sesuai revision policy;
- Audit, Admin, Support, dan Auditor surfaces online-only;
- denied discovery menggunakan safe `403` atau consistent concealed `404` sesuai resource policy;
- raw SQL/query builder, direct database access, storage browsing, dan unrestricted report builder dilarang.

---

## 2. User boundary

### 2.1 User History remains primary

User tidak otomatis membutuhkan Enterprise Audit Console. Aktivitas tool sehari-hari tetap ditampilkan melalui User History, yang dapat berisi nama file milik user, Feature, waktu, status, lokasi hasil, ukuran, dan durasi sesuai requirement. Menghapus User History tidak menghapus Enterprise Audit.

| Aspek | User History | Enterprise Audit |
|---|---|---|
| Tujuan | Convenience, repeat/open result, personal trace | Enterprise evidence, compliance, security accountability |
| Scope | Owner only | Permission + explicit organizational/evidence scope |
| Filename | Dapat tampil pada history milik user | Tidak disimpan secara default |
| Delete | User dapat menghapus personal projection | No ordinary user delete |
| Search | Personal history search/filter | Controlled structured filters only |
| Content | Link/result sesuai owner policy | Tidak menyimpan document content |

### 2.2 Conditional transparency events

User dapat diberikan halaman **Account Security Activity** yang sangat terbatas, bukan Audit Console. Ini **RECOMMENDED / POLICY-CONDITIONAL**, karena requirement belum mengunci UI tersebut.

| Event yang boleh ditampilkan | Safe user-facing fields | Yang disembunyikan |
|---|---|---|
| Own successful/failed sign-in yang dapat diikat aman ke account | Waktu, outcome, platform family, app/channel, safe reason code | Exact IP, hostname/device ID, raw identity claim, token, raw error |
| Own logout/session expiry/revocation | Waktu, session action, outcome, generic source | Administrator identity detail kecuali policy mengizinkan; credential/session secret |
| Own account eligibility/role/permission effect changed | Waktu, effective change summary, effective-from, support contact path | Permission internals yang sensitif, other-user data, raw before/after payload |
| Own high-risk action | Feature/action category, time, outcome, FileId/SecuredFileId masked if relevant | Filename, document content, parameters, password, signature, OCR text |
| Own content transfer or approved internal AI route | Time, purpose category, Local/Internal route, outcome | Prompt/document text, provider request/response, endpoint/account/credential |

### 2.3 User prohibitions

User tidak dapat:

- membuka Enterprise Audit Console, browse event user lain, atau melihat organization-wide count;
- menggunakan audit filter Actor/Resource untuk menemukan user/resource lain;
- melihat internal Admin, Support, Auditor, Security, archive, purge, atau export evidence;
- mengubah, menghapus, menahan, mengarsipkan, atau mengekspor enterprise audit;
- meminta filename/document content dimasukkan ke audit untuk convenience;
- menggunakan transparency page sebagai source untuk retry/reprocess—aksi tersebut tetap melalui User History/application service.

User transparency projection harus diturunkan dari server-side owner scope dan field allowlist; bukan query Audit Console dengan filter client `actor=self`.

---

## 3. Administrator boundary

Administrator tidak otomatis melihat semua audit payload. Administrator memperoleh evidence hanya melalui explicit permission, delegated scope, event-category scope, dan safe field projection.

### 3.1 Recommended Administrator evidence access

| Area | May be granted | Default field projection | Exclusions |
|---|---|---|---|
| Administrative changes | Account eligibility/session revocation, role/permission assignment/revocation, delegation and approval outcome | Actor/target safe ref, action, occurred/received time, outcome, policy/version, correlation | Credential, raw claim, document/issue content |
| Policy changes | Security/classification/retention policy version activation and bounded safe change reference | Policy key/scope, old/new version or digest, actor, outcome | Secret value, free-form payload, content |
| Configuration changes | Feature flag, limit, compatibility, maintenance, approved typed config changes | Typed key, scope, before/after safe value or version, actor, outcome | Secret/config credential, unrestricted JSON |
| Operational metadata | Safe job/queue/sync/error lifecycle evidence needed for administration | Event type, counts/category, Feature/Job/Correlation ID as restricted fields, status, duration/size bucket | Input/result, filename/path, raw exception, storage key |
| Security-relevant metadata | Denied privileged actions, self-assignment attempts, invalid policy changes, session revocation outcomes | Stable reason/error code, actor/target masked as allowed, event type/outcome | Exploit payload, token, IP by default, raw authorization internals |

### 3.2 Guard Administrator

- Admin write permission does not imply read access to audit evidence of the same domain.
- Admin may view evidence of own/current-scope changes for operational confirmation, but cannot menjadi sole independent reviewer when SoD policy requires Auditor review.
- Admin does not receive general authentication/security-event access across the enterprise unless separately assigned a security/audit read permission.
- Admin does not receive `audit.export` merely because it can change policy/configuration.
- Retention-policy administration does not grant archive/purge execution or legal-hold override.
- Admin cannot edit/delete source events, rewrite actor/time/outcome, or remove evidence of its own action.
- Monitoring metrics remain Observability; only accountability-relevant state/action belongs in Audit.
- Document content, filename/path, OCR, Assistant content, issue screenshot/message/note, signature, clipboard, password, token, secret, raw exception, SQL, and storage key remain excluded.

---

## 4. Auditor boundary

Auditor adalah primary consumer Enterprise Audit Console, tetapi access tetap controlled—not unlimited.

### 4.1 Auditor capabilities

Document 09 mengunci `audit.event.list` dan `audit.event.read` sebagai permission default Auditor di dalam `AUDIT_SCOPE`; `audit.export.create` dan `audit.export.read` bersifat optional dan harus diberikan terpisah.

| Capability | Default behavior | Additional guard |
|---|---|---|
| Query/list | Cursor-based safe evidence within assigned scope | Scope before count/filter; no exact total required |
| Structured search/filter | Allowlisted fields dan exact identifiers | No free-text payload/document search |
| Detail | Safe event detail within category/field entitlement | Privileged/sensitive detail view itself audited |
| Administrative/security evidence | Read within organizational/environment/category scope | Restricted actor/resource identifiers only when need-to-know |
| Audit-of-audit | Read query/detail/export/lifecycle evidence | Cannot alter source or lifecycle policy |
| Export | Separate permission; bounded asynchronous job | Reauth/approval when high impact; export action audited |

### 4.2 Auditor scope dimensions

An Auditor grant should be bounded by applicable combinations of:

- organization/unit/region or other confirmed enterprise scope;
- environment;
- event category/type;
- time range/retention tier;
- field sensitivity/classification;
- read/detail/export action;
- legal purpose/case where policy requires.

Exact organizational vocabulary, disclosure threshold, and Auditor grant source remain `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 4.3 Auditor prohibitions

Auditor cannot:

- edit/delete/correct an audit event in place; correction uses a new linked evidence event if approved;
- change policy/configuration, role/permission, user data, issue, job, secured file, or retention rule;
- remove legal hold, run purge, or retrieve archive unless a separate governed custodian permission exists;
- read document/result/OCR/prompt/chat/signature/clipboard/issue screenshot/message/note content;
- access raw token/password/secret/key/claim/error/SQL/network packet/provider request/response;
- run arbitrary SQL, regex, raw expression, dynamic field path, full-text document search, or unbounded export;
- infer unlimited scope from Auditor role, Event ID, FileId, Correlation ID, or export possession.

---

## 5. Support boundary

Support/Superadmin membutuhkan evidence hanya untuk troubleshooting issue yang sedang berada dalam scope-nya. Support tidak otomatis memperoleh Enterprise Audit Console, general audit query, atau audit export.

### 5.1 Issue-linked projection

| Field/evidence | Support visibility | Guard |
|---|---|---|
| Issue ID | Full within scoped case | Not usable to browse unrelated cases |
| Event time | Relevant event window | Window server-derived/configured around issue/job; not arbitrary enterprise range |
| Feature ID | Full canonical ID | No Feature parameters/document name |
| Job ID | Exact for linked issue when needed | No job input/result/content access |
| Correlation ID | Exact for linked issue when needed | Correlation only; no unrestricted trace/log browsing |
| Process ID | Exact if present and necessary | Same issue/correlation scope |
| Outcome/status | Safe enum | No hidden payload |
| Error Code | Stable safe code and approved guidance | No raw exception, stack, SQL, internal secret |
| Platform/App version | Family/version needed for reproduction | No hostname, serial, device account, exact IP |
| Execution location/source | Local/server/component category | No network topology or credential |
| Safe size/duration category | Only if needed for reproduction/capacity triage | Prefer bucket, not exact sensitive metadata |
| Issue-related access evidence | Support actions on that issue | No unrelated admin/security events |

### 5.2 Support enforcement

- Projection is accessed through Support/Issue application boundary, not by silently granting Auditor role.
- Support list/detail/attachment/triage/note/export permissions remain separate.
- General Actor, Resource, Environment, or security-category audit filters are unavailable to Support by default.
- Issue PDF/Excel export remains a Support export, not Enterprise Audit export; it contains only authorized issue fields and explicitly sent attachments as policy permits.
- Support cannot pivot Job/Correlation/File/Event ID to source/result/secured content.
- Support cannot access audit events outside issue-linked scope, including unrelated authentication, role, policy, audit-export, archive, or purge events.
- If a Support principal also requires Auditor duties, that is a separate explicit grant/profile combination subject to SoD policy; it is not inherited from Superadmin label.

Document 06 currently exposes general audit query only through Auditor permission. If issue-linked evidence is not already covered by the Support Issue safe-context projection, implementation requires a controlled API/application-contract addendum—not a direct audit endpoint bypass.

---

## 6. Audit-of-audit

Aksi manusia atau privileged service terhadap Audit system dapat menjadi evidence. Audit-of-audit harus cukup untuk accountability tanpa membuat recursion tak terbatas atau merekam raw query/export payload.

### 6.1 Recommended events

| Event family | Trigger | Safe evidence | Guard |
|---|---|---|---|
| `AUDIT_SEARCH_SENSITIVE` | Query memakai cross-user actor, exact resource, security category, archive tier, atau wide-range threshold | Actor, scope/category codes, date-span category, filter fingerprint, result-count band, outcome, correlation | Tidak merekam raw free text, filename, SQL, secret, atau seluruh returned IDs |
| `AUDIT_DETAIL_VIEWED` | Privileged detail/identity/resource field dibuka | Viewer, target AuditEventId, field-projection class, purpose/case ref if required, outcome | Tidak menyalin event payload ke audit-of-audit |
| `AUDIT_EXPORT_REQUESTED` | Export request accepted/denied | Requester, normalized filter/scope version, field set, format, policy version, outcome | No exported rows/content |
| `AUDIT_EXPORT_COMPLETED` | Artifact generated/failed/cancelled/expired | ExportRequestId, count/size band, digest, expiry, outcome, Job/Correlation ID | No artifact bytes/path/storage key |
| `AUDIT_EXPORT_DOWNLOADED` | Authorized artifact download | Actor, ExportRequestId, time, outcome | No link/token/content |
| `AUDIT_RETENTION_POLICY_CHANGED` | Retention/legal-tier policy version changes | Actor/approver, scope, old/new version/digest, effective time, outcome | No secret/free-form config dump |
| `AUDIT_HOLD_CHANGED` | Legal/investigation hold applied/released | Actor/approver, case/purpose ref, bounded scope, effective/expiry, outcome | Exact legal narrative stays in approved case system |
| `AUDIT_ARCHIVE_ACTION` | Archive create/verify/restore/failure | Actor/workload, range/tier, count, manifest/digest ref, outcome | Opaque archive ref only; no archive path/content |
| `AUDIT_PURGE_ACTION` | Purge request/approval/execution/failure/hold-block | Actor/approver/workload, policy version, bounded range/count, hold check, outcome | No deleted payload; purge evidence not deleted by same action |
| `AUDIT_PERMISSION_CHANGED` | Audit query/detail/export/lifecycle grant changes | Actor/approver, target, scope/profile version, outcome | No raw claim/credential |
| `AUDIT_ACCESS_DENIED` | Privileged access denied or enumeration pattern detected | Actor ref when known, action/category, safe reason code, target type, correlation | No attack payload/raw request |

### 6.2 Recursion and integrity guards

1. Reading an audit-of-audit event may generate one `AUDIT_DETAIL_VIEWED` only when the view is privileged by policy; system ingestion of that new event does not recursively audit itself.
2. Cursor pagination within one already-authorized query should not generate one event per page. Record the sensitive query session/request once, plus abnormal/denied continuations.
3. Export lifecycle uses one ExportRequestId and state events, not duplicated row-level audit events.
4. Source events are append-oriented. Corrections, late context, or lifecycle outcomes are new linked events; no in-place rewrite.
5. Archive/purge action evidence is committed after/independently from the target lifecycle operation and is outside the exact purge set of that operation.
6. Failed, denied, cancelled, partial, hold-blocked, and retry outcomes remain visible.
7. Workload identity and human approver are distinguished; a service identity never substitutes for the accountable human request/approval.

---

## 7. Search/filter model

### 7.1 Query rules

- Audit lists use opaque keyset cursor, default 50 and maximum 100 items per API Final.
- Default stable order: `OccurredAtUtc DESC` plus unique Audit Event tie-breaker.
- `ReceivedAtUtc` is visible and can be the selected time dimension for late/offline-ingestion investigation.
- UTC RFC 3339 half-open ranges `[from,to)` apply.
- A configured recent range is the default; maximum interactive range is policy/capacity controlled.
- Wide/old/archive-range queries become asynchronous export or require narrower filters.
- Authorization/scope is applied before count/filter/search/sort/cursor; exact total is not required.
- Cursor is integrity-protected, expiring, and bound to principal, permission revision, scope, filters, time dimension, sort, and field projection.
- No arbitrary sort, raw expression, regex, dynamic field path, SQL, free-text payload search, leading-wildcard large-table search, or client-defined authorization filter.

### 7.2 Business-use filters

| Filter | Semantics | Business use | Privacy/implementation guard |
|---|---|---|---|
| Date range | Required/explicit UTC `[from,to)` on Occurred or Received time | Incident window, compliance period, late-event review | Bounded; selected time dimension shown in UI/export |
| Actor | Exact internal PrincipalId or approved directory selector within scope | Review activity of a known subject | No broad email/name text scan; masked list display; full identity field permission |
| Event Category | Exact/multi-select controlled category | Authentication, authorization, admin, policy, feature, support, audit lifecycle | Category catalog versioned; not free text |
| Event Type | Exact/multi-select controlled event code | Investigate a specific action | Only types visible in caller category scope |
| Feature | Exact canonical Feature ID | High-risk feature/control review | No parameter/document search |
| Outcome | Exact controlled outcome/status | Success, denied, failed, cancelled, partial | Stable code; no raw reason payload |
| Resource | Exact `ResourceType + opaque ResourceId` | Investigate a known account/job/file/issue/export | No filename/path/storage key; permission before lookup |
| Correlation ID | Exact | Trace one cross-component operation | Restricted identifier; cannot expand content/scope |
| Job ID | Exact | Job lifecycle/security troubleshooting | Restricted identifier; no input/result pivot |
| Platform | Exact controlled platform family | Cross-platform behavior/security issue | App/platform code only; no device fingerprint |
| Source | Exact controlled event-source/component code | Distinguish client/API/Worker/system-originated evidence | No hostname/network topology; source code must be authenticated/server-derived |
| Environment | Exact controlled environment code, or fixed by deployment | Separate Production/Pilot/QA evidence | If storage is physically/environment scoped, omit user-selectable filter and bind environment server-side |

Filters yang sengaja tidak dibuat: filename, document/OCR text, issue message/note/screenshot content, Assistant prompt/chat, provider request/response, password/token/secret, raw exception/SQL, arbitrary JSON property, device hostname/serial, exact IP, geolocation, dan free-form “search everything”.

### 7.3 Field/schema implication

Database Final minimum fields already support Actor, Event Type, Outcome, Feature, Process, Job, Correlation, execution location, Error Code, Platform, App Version, and timestamps. Requested structured filters for `EventCategory`, `ResourceType/ResourceId`, `Source`, and possibly `Environment` require stable typed/indexable semantics. They must not be buried as unbounded free-form `SafeMetadataJson` if used routinely.

If those typed fields are adopted by Document 10, implementation requires a controlled Document 07 data-dictionary/index addendum and matching Document 06 contract update. This report does not edit either FINAL document.

---

## 8. Export model

### 8.1 Recommended baseline

All Enterprise Audit exports should use the existing bounded asynchronous export contract, including small exports. One consistent path avoids a second synchronous disclosure mechanism.

```text
Authorized filtered query
→ explicit Export request
→ permission/scope/field/reauth/admission checks
→ durable ExportRequest + background job
→ point-in-time/as-of bounded extraction
→ safe artifact + manifest/digest/expiry
→ reauthorized download
→ expiry/tombstone/cleanup
```

### 8.2 Format recommendation

| Format | Recommendation | Reason |
|---|---|---|
| CSV UTF-8 | **Primary detailed audit export** | Simple, streamable, low server memory, widely usable, easy schema/version header/manifest |
| PDF | Optional only for fixed human-readable report templates in section 11 | Suitable for management/compliance summary, not raw high-volume evidence dump |
| XLSX | Not baseline for Enterprise Audit; retain for Support issue register | Adds package/memory/row complexity; adopt only if explicit business requirement |
| JSON/raw database dump | Not provided to ordinary users | Too implementation-specific and easy to misuse; API contracts remain separate |

CSV cells remain allowlisted and neutralize spreadsheet formula prefixes even though audit fields should not contain user content. Encoding, delimiter, schema version, UTC time semantics, and null representation must be declared in the manifest.

### 8.3 Mandatory export controls

- separate export permission; view/detail permission does not imply export;
- explicit date range, event/category scope, organizational/environment scope, field set, and format;
- same or stricter field masking as interactive view; export never “unmasks everything” by default;
- fresh reauthentication for high-impact export; optional independent approval based on classification/volume policy;
- authorization repeated at request, generation, artifact publication, and each download;
- request fingerprint/idempotency prevents duplicate jobs/effects;
- per-user/category/global concurrency, row/byte/date-span/runtime limits, rate limit, queue, backpressure, and storage quota;
- requests beyond configured safe bounds are narrowed/rejected safely—not silently truncated as a complete report;
- consistent `asOf`/snapshot boundary and disclosure of late/offline event limitations;
- encrypted/private temporary artifact, opaque ID, no public/durable URL, no email attachment, visible expiry;
- manifest includes ExportRequestId, requester/scope, normalized filter summary/fingerprint, field schema/version, generated/as-of time, record count, digest, classification label, and expiry;
- download uses safe headers, range rules if supported, and current authorization;
- completed/failed/cancelled/downloaded/expired export lifecycle is audited;
- artifact cleanup is idempotent and legal-hold/policy aware;
- no document content, filename/path, issue screenshot/message, prompt/provider content, credential, raw exception, or storage key.

Large export remains background. Numeric thresholds are configuration/policy/benchmark owned and must not be hardcoded in Document 10 without evidence.

---

## 9. Privacy classification

### 9.1 Labels

| Label | Meaning |
|---|---|
| `SAFE` | Approved audit metadata that may be stored and shown within authorized scope; **not public data** |
| `MASKED` | Only a minimized/derived representation is shown by default; full value is not needed for normal list/report |
| `RESTRICTED` | Stored only for justified evidence need and visible only with explicit field/category scope |
| `NOT STORED` | Prohibited from Enterprise Audit; keep in its proper source system or do not persist |

### 9.2 Field classification

| Field/data | Classification | Recommended treatment |
|---|---|---|
| AuditEventId | `SAFE` | Opaque stable event reference; still subject to event scope |
| OccurredAtUtc / ReceivedAtUtc | `SAFE` | Store both; show late-received indicator where relevant |
| EventCategoryCode / EventTypeCode | `SAFE` | Controlled/versioned codes |
| OutcomeStatus / safe denial reason code | `SAFE` | Stable enum/code; no raw explanation payload |
| FeatureId | `SAFE` | Canonical ID only; no parameters/content |
| PlatformCode / app version | `SAFE` | Platform family and approved app version |
| ExecutionLocation / SourceCode / EnvironmentCode | `SAFE` | Controlled codes, server-derived; no topology/hostname |
| ErrorCode | `SAFE` | Stable safe code only |
| Size/duration category | `SAFE` | Prefer bucket for reports and broad views |
| Exact input/output size/duration | `RESTRICTED` | Store only where required by baseline/analysis; minimize export |
| ActorPrincipalId | `RESTRICTED` | Required enterprise identity reference; masked in broad list/report by default |
| Role/unit snapshot codes | `RESTRICTED` | Approved code snapshot for historical evidence; scoped display |
| Display name/email/employee number | `MASKED` | Resolve/display only when need-to-know; avoid duplicating into event row when stable ID suffices |
| ProcessId / JobId / CorrelationId | `RESTRICTED` | Exact search allowed in scope; masked in broad list/export |
| ResourceTypeCode | `SAFE` | Controlled logical type |
| FileId / SecuredFileId / IssueId / AttachmentId / ExportRequestId | `RESTRICTED` | Opaque reference only; no content authority; mask in list |
| Original filename | `NOT STORED` | Use FileId/SecuredFileId or safe logical reference; filename remains in User History/source system if needed |
| Local path / UNC / URI / storage key / bucket/share path | `NOT STORED` | No direct-object or path disclosure |
| Document bytes/text/thumbnail/preview | `NOT STORED` | Never Audit payload |
| Full OCR/extracted/redacted/translated/Markdown text | `NOT STORED` | Never Audit payload |
| Issue message/internal note | `NOT STORED` | Remains in permission-gated Support plane |
| Issue screenshot/attachment bytes or raw object reference | `NOT STORED` | Audit may store restricted IssueId/AttachmentId only for an access/action event |
| Password/token/cookie/secret/private or encryption key | `NOT STORED` | Never Audit/log/telemetry/export |
| Clipboard/signature/paraf payload | `NOT STORED` | Safe action metadata only if required |
| Assistant prompt/chat/tool narrative | `NOT STORED` | Store only intent category/Feature/outcome/correlation if required |
| AI/provider request/response/document context | `NOT STORED` | Never Audit payload |
| Approved AI provider profile/model-policy code | `RESTRICTED` | Only for provider/security/governance event; no endpoint/account/credential |
| AI route `LOCAL` / `INTERNAL_APPROVED` and policy outcome | `SAFE` | Useful for compliance without naming sensitive infrastructure |
| Exact IP address/network header | `NOT STORED` baseline | If Pertamina mandates for security events, use separate confirmed restricted profile with masking/retention; do not infer now |
| Network zone/channel code | `SAFE` | Controlled coarse code if business/security use exists |
| Device family | `SAFE` | Desktop/Mobile/Tablet/Web only |
| OS family/major version | `MASKED` | Keep coarse version only where support/security use exists |
| Hostname/device name/serial/IMEI/MAC/advertising ID | `NOT STORED` | Avoid device fingerprint and privacy leakage |
| Exact geolocation | `NOT STORED` | No approved audit business use |
| Raw User-Agent | `NOT STORED` | Use platform/app/OS family fields instead |
| Raw exception/stack/SQL/claims/request body | `NOT STORED` | Stable Error/Event code only |
| Policy/config key and version | `SAFE` | Controlled key/version within category scope |
| Safe typed before/after value | `RESTRICTED` | Only allowlisted non-secret setting; otherwise version/digest only |
| Secret config value or unrestricted JSON | `NOT STORED` | Never copy into evidence |
| `SafeMetadataJson` | `RESTRICTED` | Versioned, size-limited, event-type allowlist; not arbitrary property bag |

Official mapping to Pertamina classification, handling, export, retention, and masking standards remains `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

---

## 10. Metadata masking

Masking reduces incidental disclosure but never replaces authorization.

| Metadata | List/report default | Privileged detail/search | Storage rule |
|---|---|---|---|
| Actor identity | Display role/unit category plus masked principal, e.g. suffix only | Full internal PrincipalId or approved directory-resolved identity only with field entitlement | Store stable PrincipalId; avoid duplicate name/email |
| Job/Correlation/Process ID | Short prefix/suffix; copy disabled unless permitted | Exact ID for scoped lookup/troubleshooting | Store opaque ID; no semantic payload |
| File/SecuredFile resource | Resource type + masked opaque ID | Exact ID only after resource/evidence authorization | Never filename/path/storage key |
| Issue/Attachment | Masked ID in broad Auditor report | Exact ID for scoped Support/Auditor detail | No screenshot object ref/bytes |
| File size/duration | Bucket/category | Exact value only where evidence need is approved | Prefer safe numeric/bucket field, no content |
| Device | Platform family and coarse OS/app version | More exact supported version only for justified security/support scope | No hostname/serial/fingerprint |
| IP/network | No exact IP in baseline view | Not available in baseline | If later mandated, restricted storage and policy-defined prefix masking/retention |
| Provider | `LOCAL` or `INTERNAL_APPROVED`; provider profile hidden | Approved profile code for security/governance permission | No URL/account/tenant/credential/request/response |
| Admin policy/config change | Key/scope + version change | Allowlisted safe typed value if permission and non-secret | Secret values excluded; digest/version preferred |
| Filter/audit search | Category/date/scope summary | Restricted normalized filter details if investigation requires | Store fingerprint and allowlisted summary, not raw query/SQL |

Additional masking rules:

1. Masked values tetap stabil hanya selama kebutuhan view/report; jangan membuat global cross-system tracking identifier baru.
2. Full identifier exact search boleh diterima sebagai input, tetapi hasil tetap scope-filtered dan list projection tetap masked.
3. Export memakai projection/masking caller pada waktu generation dan download; tidak otomatis membuka field penuh.
4. A copied masked value, Event ID, atau hash tidak boleh dapat digunakan sebagai storage path, token, atau authorization proof.
5. Control characters, bidi ambiguity, formula prefix, dan newline dinormalisasi/escaped pada UI/CSV/PDF.
6. Masking failures fail closed by omitting field or report—not by returning raw value.

### 10.1 Filename decision

Original filename **tidak diperlukan** dalam Enterprise Audit baseline dan berisiko memuat nama orang, unit, proyek, kontrak, klasifikasi, atau isi dokumen. Rekomendasi:

```text
Audit resource identity:
ResourceType + FileId / SecuredFileId / JobId / IssueId

Not:
Original filename, local path, storage path, URL, or object key
```

User tetap dapat melihat filename pada User History atau resource miliknya sendiri. Auditor/Administrator/Support tidak memperoleh filename melalui Audit. Exceptional need harus dibuktikan dan disetujui sebagai restricted source-system lookup, bukan dengan menyalin filename ke semua event.

---

## 11. Reporting recommendations

Gunakan tujuh report template minimum yang berupa scoped saved query/export definition. Tidak diperlukan dashboard baru pada baseline. Report dapat dijalankan on-demand; scheduling, subscription, atau dashboard hanya ditambahkan bila ada business requirement.

| Report | Primary audience | Business purpose | Minimum filters | Safe output |
|---|---|---|---|---|
| Privileged Changes | Auditor; scoped Administrator read if granted | Review account eligibility, session revocation, role/permission/delegation changes | Date, actor/target scope, event type, outcome, environment | Actor/target refs per field permission, action, version, outcome, time, correlation |
| Authentication & Security Events | Auditor/Security-authorized reader | Review sign-in, revocation, authorization anomalies, security-control outcomes | Date, actor scope, category/type, outcome, platform, source, environment | Safe reason/error codes, coarse device/platform, no exact IP/raw payload |
| Policy & Configuration Changes | Auditor; relevant Administrator | Review policy, classification, retention, feature flag, limit, compatibility, maintenance changes | Date, actor, key/category, outcome, environment | Key/scope, version/digest, safe typed value when allowed, approver/outcome |
| Denied Sensitive Access | Auditor/Security-authorized reader | Detect cross-user, self-elevation, content, export, storage, or privilege attempts | Date, actor scope, event/resource type, outcome, source, environment | Safe denial code, masked target ref, action, correlation; no attack payload |
| High-Risk Feature Actions | Auditor/Compliance scope | Review Protect/Unlock/Redact/visual Sign, server content transfer, approved AI/translation, permanent deletion, high-classification action | Date, Feature ID, outcome, route/source, actor scope | Feature/action, Local/Internal route, consent/reauth outcome code, masked resource ID; no parameters/content |
| Support Activity | Auditor; Support manager within Support scope | Review issue detail/attachment access, triage/status/note action, issue export | Date, Support actor, issue/attachment ref, action, outcome | Masked Issue/Attachment IDs, action/status transition code, export ref; no issue text/screenshot/note |
| Audit Access, Export & Lifecycle | Auditor/Compliance/audit custodian | Review sensitive searches, detail views, exports, retention/hold/archive/purge | Date, actor, audit action, outcome, environment | Filter fingerprint/summary, target Event/Export ref, policy version, count/size band, digest/outcome |

Report controls:

- template field/filter set is versioned and allowlisted;
- report execution uses current permission/scope and is itself audited where privileged;
- report does not silently add filename, exact IP, device ID, document/issue/AI content, or raw diagnostics;
- PDF summary uses fixed layout and aggregates; detailed row evidence uses CSV;
- aggregate small-count suppression or grouping should be applied where identity inference risk exists, subject to policy;
- “No data” distinguishes no authorized matching evidence from system failure without leaking out-of-scope counts.

---

## 12. Sensitive-field exclusions

### 12.1 Never stored in Enterprise Audit

- document/file bytes, full text, thumbnail, preview, cache, checkpoint content, or derived content;
- original filename, full local/server path, UNC/device path, URL, storage key, bucket/share name, durable download link;
- password PDF/file, SSO password, access/refresh token, cookie/session secret, API/service/provider credential, private/encryption key;
- clipboard content, visual signature/paraf/stamp payload or template;
- full OCR/extracted/redacted/translated/Markdown text;
- Assistant prompt/chat/system instruction, conversation state, model context, tool narrative, provider request/response;
- issue message, internal note, screenshot/attachment bytes, markup, or object-storage reference;
- raw exception, stack trace, SQL/query plan text, request/response body, claim dump, authorization header, IPC payload;
- user-entered Feature parameters, page text/range rationale, password value, output filename, remote URL, translation text;
- exact IP/network headers, raw User-Agent, hostname, device name/serial/IMEI/MAC/advertising ID, exact geolocation in baseline;
- malware sample bytes, exploit payload, parser content, or DLP match content;
- unrestricted `SafeMetadataJson`, arbitrary tags, or free-form “reason/details” copied from user/support/admin input.

### 12.2 Safe references instead of payload

| Sensitive source | Audit replacement |
|---|---|
| File/document | `ResourceType` + opaque `FileId`/`SecuredFileId` |
| Job/input/result | Job ID/Process ID/Correlation ID + action/outcome; no artifact path |
| Issue/screenshot | Issue ID/Attachment ID + access/action/outcome; no content/object ref |
| Assistant/AI | Intent category, canonical Feature ID, route, policy/consent/outcome, correlation |
| Admin config | Typed key/scope + old/new version or digest; safe value only if explicitly allowlisted |
| Error | Stable Error Code/event reason code |
| Device/network | Platform/app/coarse OS and approved zone/channel code |
| Export/archive | Export/manifest opaque ID + digest/count/expiry/outcome |

### 12.3 `SafeMetadataJson` rule

`SafeMetadataJson` is permitted only when every Event Type owns a versioned schema with explicit field names, types, maximum lengths/count/depth, privacy classification, masking rule, query/index policy, and retention implication. Unknown fields are rejected or dropped with safe diagnostic evidence. It is not a general bag for raw DTOs, exception objects, request bodies, provider response, filenames, or later “debug convenience”.

---

## 13. Risks/conflicts

| ID | Risk/conflict | Impact | Recommendation/status |
|---|---|---|---|
| `AUD-C-001` | Exact organizational vocabulary dan assignment authority untuk `AUDIT_SCOPE` belum dikonfirmasi Pertamina | Salah scope dapat membuka evidence lintas unit/region atau menutup kebutuhan audit sah | Default ke scope tersempit/no grant; `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` sebelum Production |
| `AUD-C-002` | Database minimum belum memiliki typed Event Category, Resource Type/ID, Source, Environment | Required filters jatuh ke JSON atau tidak efisien | Jika diadopsi, controlled Document 07 addendum + indexes berdasarkan measured queries |
| `AUD-C-003` | API general audit route hanya menyebut Auditor permission, sementara Admin/Support memerlukan narrow projections | Risiko grant Auditor terlalu luas atau bypass endpoint | Admin evidence dan Support issue-linked projection harus explicit; controlled API contract addendum jika current routes insufficient |
| `AUD-C-004` | Feature Catalog exposure `AUDITOR/ADMIN` dapat dibaca sebagai implied Admin access | Excessive audit visibility | Ikuti Role Matrix: Administrator membutuhkan separate explicit Audit/evidence grant |
| `AUD-C-005` | Business/User History menampilkan filename, sedangkan Database/Security melarang filename audit default | Filename dapat bocor ke Audit karena model dicampur | Pertahankan history/audit separation; Audit memakai FileId/SecuredFileId |
| `AUD-C-006` | User transparency view belum merupakan locked requirement | Scope/UI dapat melebar menjadi Audit Console | Implement only as own-account safe projection setelah user/Integrator decision |
| `AUD-C-007` | Exact IP/device identifiers tidak ada di baseline, tetapi sering diminta untuk security audit | Privacy/retention/fingerprinting risk | Baseline NOT STORED; enterprise confirmation required for restricted security profile |
| `AUD-C-008` | Provider profile code allowed in security metadata tetapi dapat mengungkap internal architecture | Reconnaissance/provider governance leakage | Restricted field; broad report shows `LOCAL/INTERNAL_APPROVED` only |
| `AUD-C-009` | `OccurredAtUtc` vs `ReceivedAtUtc` retention authority belum diputuskan | Late offline event dapat terhapus terlalu cepat atau disimpan terlalu lama | UI/search supports both; legal retention timestamp requires Audit/Compliance confirmation |
| `AUD-C-010` | Audit-of-audit dapat recursion/noise | Storage growth dan unusable evidence | One query-session event, stateful export events, no system-ingestion recursion |
| `AUD-C-011` | Auditor role dianggap unlimited across units/environments/categories | Excessive identity/security disclosure | Scoped grants + field/category entitlements + deny default |
| `AUD-C-012` | Support memperoleh general audit search untuk troubleshooting | Unrelated sensitive-event leakage | Issue-linked projection only; no general Actor/Resource/category filters |
| `AUD-C-013` | Admin policy writer dapat purge evidence perubahan sendiri | Evidence destruction/SoD failure | Separate retention/purge authority; legal hold; no ordinary update/delete; audit-of-audit |
| `AUD-C-014` | CSV dibuka di spreadsheet | Formula injection atau interpretation ambiguity | Controlled cells, neutralization, UTF-8/schema manifest, no user content |
| `AUD-C-015` | Large/wide export menguras server kecil | CPU/DB I/O/storage/network starvation | Async bounded export, cursor/index, concurrency/quota/backpressure, artifact expiry |
| `AUD-C-016` | Masking dianggap cukup tanpa authorization | Cross-scope inference dan linkage | Authorization before projection; masking only defense-in-depth |
| `AUD-C-017` | Filter/raw query disalin ke audit-of-audit | Identity/resource/search leakage | Store allowlisted summary + fingerprint; no raw query/SQL/free text |
| `AUD-C-018` | Audit menjadi salinan Observability | High-volume noise dan data overcollection | Only accountability/compliance events; metrics/traces remain Observability |
| `AUD-C-019` | Archive/purge event ikut dihapus oleh lifecycle yang dicatatnya | Hilang evidence atas deletion | Commit evidence outside target set; verify manifest/digest; independent authorization |
| `AUD-C-020` | Role/unit/display name berubah setelah event | Historical interpretation salah | Store stable PrincipalId + approved role/unit snapshot codes; resolve current display separately |
| `AUD-C-021` | Late/offline batch actor/role dipalsukan client | False evidence | Server derives authenticated principal/effective context; preserve Occurred/Received times and ingestion batch identity |
| `AUD-C-022` | Export format belum locked di parent documents | Multiple incompatible disclosure paths | Recommend CSV detail + optional fixed PDF; user/Integrator final decision |

No risk di atas membenarkan penyimpanan document content, filename, full issue payload, public AI data, atau unlimited privileged access.

---

## 14. Suggested Document 10 text

Teks berikut dapat dipertimbangkan Integrator untuk Document 10.

### 14.1 Audit purpose and plane separation

```text
Enterprise Audit is append-oriented controlled evidence. It is separate from
User History, Support cases, and Observability. Audit does not collect data
merely because it may be useful later. Every event and field must have an
approved accountability, security, compliance, or lifecycle purpose.

User History remains the user's personal activity projection and may contain
the user's own filename where required. Enterprise Audit does not store the
original filename by default and uses opaque FileId, SecuredFileId, JobId,
IssueId, or other safe logical references.
```

### 14.2 Access model

```text
Audit access requires online authentication, explicit permission, a
server-derived organizational/evidence scope, allowed event category/type,
allowed field projection, current policy/classification, and fresh
reauthentication when required. Role name, menu visibility, IDs, cursors, and
exports are not authorization.

User
  Has no Enterprise Audit Console by default. An optional own-account security
  transparency projection may show selected sign-in/session, access-change,
  and high-risk-action outcomes without other-user or sensitive metadata.

Administrator
  May receive scoped evidence for administrative, policy/configuration,
  operational, and security-relevant changes through separate permission.
  Administrator write authority does not imply all-audit read or export and
  never implies document content.

Auditor
  Has controlled query, search/filter, and detail access to approved safe
  evidence. Export is a separate permission. Auditor cannot edit/delete source
  events, change policy, modify user data, override hold, or access content.

Support/Superadmin
  Receives only Issue-linked safe troubleshooting evidence such as time,
  Feature ID, Job ID, Correlation ID, safe outcome/error, platform/app version,
  and source category. Support has no general Enterprise Audit Console or
  Audit export and cannot pivot identifiers to documents/results/content.
```

### 14.3 Event and privacy baseline

```text
Minimum safe evidence uses controlled event category/type/outcome, Occurred
and Received UTC times, stable ActorPrincipalId with approved role/unit
snapshot codes, Feature/Process/Job/Correlation references when applicable,
resource type plus opaque resource ID, execution/source/environment/platform
codes, app version, safe size/duration category, Error Code, schema version,
and size-limited event-specific metadata.

Original filename/path, document/OCR text or bytes, issue message/note/
screenshot bytes, Assistant prompt/chat, provider request/response, password,
token, cookie, clipboard, signature payload, secret/key, raw exception/SQL/
claim/request body, storage key, hostname/device serial, exact IP, and exact
geolocation are not stored in the baseline Enterprise Audit.

SAFE means safe only within authorized scope. Actor/resource/correlation and
provider profile identifiers are RESTRICTED and masked in broad list/report
views. Masking never replaces authorization.
```

### 14.4 Query model

```text
Audit list uses an opaque integrity-protected keyset cursor with a stable
OccurredAtUtc plus unique-event tie-breaker. ReceivedAtUtc remains available
for late/offline-ingestion investigation. UTC half-open date ranges apply.

Allowed filters are Date Range, Actor, Event Category, Event Type, canonical
Feature ID, Outcome, Resource Type + opaque Resource ID, Correlation ID,
Job ID, Platform, Source, and Environment where not already deployment-bound.
Filters use controlled exact values and documented business purpose.

No filename/document/OCR/issue/prompt/provider content search, arbitrary
field path, raw expression, regex, SQL, unbounded free text, or client-defined
authorization filter is allowed. Scope is applied before count/filter/cursor.
Wide or old-range evidence becomes a bounded asynchronous export.
```

### 14.5 Export and reporting

```text
Audit export is permission-controlled, filter/scope/field-set explicit,
asynchronous, rate/size/concurrency controlled, and audited. Authorization is
checked at request, generation, publication, and download. High-impact export
requires fresh reauthentication and approval when policy requires.

Detailed audit export uses UTF-8 CSV plus a versioned manifest containing the
filter/scope summary, field schema, UTC/as-of semantics, count, digest,
classification, and expiry. Fixed PDF is optional for approved summary report
templates. Export artifacts are private, expiring, access-controlled, and
never include excluded fields.

Minimum report templates are Privileged Changes; Authentication & Security;
Policy & Configuration Changes; Denied Sensitive Access; High-Risk Feature
Actions; Support Activity; and Audit Access/Export/Lifecycle. These are scoped
report definitions, not a requirement for a new dashboard.
```

### 14.6 Audit-of-audit and lifecycle

```text
Sensitive Audit search, privileged detail view, export request/generation/
download, Audit permission change, retention-policy change, legal hold,
archive/restore, purge, and denied Audit access create safe audit-of-audit
evidence. Evidence stores action, actor/workload, bounded scope/reference,
policy/schema version, outcome, time, and correlation—never the viewed,
exported, archived, or deleted payload.

Audit-of-audit avoids recursion: one sensitive query session is recorded, not
every cursor page; system ingestion does not audit itself; export uses one
request lifecycle; purge evidence is outside the purge target. Source events
are not edited in place. Corrections and late outcomes are linked new events.

Exact legal retention, whether OccurredAtUtc or ReceivedAtUtc controls each
tier, legal-hold authority, archive technology, purge approval, official
classification, identity/network field policy, and numeric thresholds require
Pertamina Audit/Compliance/Security confirmation.
```

---

## Closing recommendation

Document 10 sebaiknya mengunci **event purpose → audience/scope → field projection → retention/reporting** sebagai satu rantai keputusan. Event atau field yang tidak mempunyai business/compliance purpose, consumer yang sah, masking rule, dan lifecycle tidak boleh masuk audit hanya untuk kemungkinan kebutuhan masa depan.
