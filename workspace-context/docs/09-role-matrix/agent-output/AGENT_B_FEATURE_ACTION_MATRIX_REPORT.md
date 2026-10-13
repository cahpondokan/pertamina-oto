# AGENT B — Feature / Action Permission Mapping Report

**Task ID:** DOC09-AGENT-B  
**Scope:** permission grouping, role defaults, resource authorization, reauthentication, online/offline behavior, and audit mapping  
**Sources reviewed:** Documents 01–08 FINAL, especially Document 03 Feature Catalog v1.3, the 33 Primary PDF Tool Cards, Document 06 API Documentation, and Document 08 Security Design  
**Status:** recommendation for Document 09; no Feature ID is added, changed, or removed, and the Document 09 master is not modified

## 1. Permission grouping strategy

### 1.1 Recommended authorization model

PERTAMINA OTO has 550 canonical Feature records, but Feature ID and permission serve different purposes:

- **Feature ID** identifies a stable product capability, schema, execution profile, exposure, and result contract.
- **Permission** grants a business/security action over a resource class and scope.
- **Role** is a maintainable bundle of permissions, not the final authorization check.
- **Policy** applies classification, platform, capacity, network, time, and organization restrictions.
- **Resource authorization** proves that the caller may act on this particular file, job, issue, secured file, user, policy, or audit set.
- **Confirmation** proves current user intent; it is not permission or authentication.
- **Reauthentication** proves recent authentication for a scoped sensitive action; it is not a temporary super-role.

```text
Allow = AuthenticatedOrValidOfflineGrant
    AND AccountActiveAsLastReliablyKnown
    AND SessionOrOfflineGrantValid
    AND FeatureVisibleAndActive
    AND BusinessActionPermission
    AND ResourceOwnershipOrExplicitScope
    AND EnterprisePolicyAndClassification
    AND ResourceStateAndVersion
    AND PlatformCapabilityAndApprovedExecutionRoute
    AND RequiredUserConfirmation
    AND FreshReauthenticationWhenRequired
```

Feature discovery and button visibility are UX filtering only. Desktop/mobile application services and ASP.NET Core API policies perform the real decision. An opaque ID, file path, Job ID, Correlation ID, ETag, object reference, possession of a local file, or previous authorization result is never sufficient by itself.

### 1.2 Proposed action-permission vocabulary

The following **33 proposed action permissions** cover the current surface without creating 550 permissions. The number 33 is incidental and is not tied one-to-one to the 33 Core cards. Exact naming remains a Document 09 governance decision; the grouping and separation are the recommendation.

Role abbreviations: `U` = User, `A` = Administrator, `Au` = Auditor, `S` = explicit Support bundle. Role bundles may be additive, but an administrative/audit/support bundle never implies a User’s document-content permission.

| Permission | Business action and resource | Default role grant | Reauth | Offline | Audit |
|---|---|---|---|---|---|
| `document.process` | View, organize, edit, convert, scan, OCR, compare, and produce a new result from caller-authorized input | `U`; `A/Au/S` only if also granted User capability | No by default | Yes for eligible local Feature | Yes, safe process metadata |
| `document.process.server` | Permit content transfer and server execution for an otherwise permitted Feature | `U` when enterprise policy allows; never implied by local processing | Conditional by classification/policy | No | Yes |
| `document.protect` | Create a new password/permission-protected PDF | `U` | No by default; confirmation required | Yes locally | Yes, never password value |
| `document.unlock` | Create an unprotected result using a valid password/authority | `U` | No by default; policy may require | Yes locally | Yes, never password value |
| `document.visual_sign` | Create/use a visual signature/paraf/stamp and finalize a non-PKI signed result | `U` | No by default; confirmation required | Yes locally | Yes, action metadata only |
| `document.redact` | Finalize irreversible redaction into a new validated result | `U` | No by default; policy may require | Yes locally | Yes, no redacted content |
| `document.intelligence` | Summarize or translate with approved local/enterprise-internal provider | `U` only while provider/Feature is approved and enabled | Conditional by classification/provider policy | Approved local provider only | Yes, no content/prompt/output body |
| `document.share` | Disclose a selected result through an approved share/export target | `U` subject to classification policy | Conditional for high-impact disclosure | Conditional | Yes |
| `file.overwrite` | Replace an existing caller-authorized local file after explicit choice | `U` subject to platform policy | No by default | Yes | Yes |
| `local_data.delete` | Delete caller-owned local draft/result/history/recycle item | `U` | No by default; permanent action needs confirmation | Yes | Yes where enterprise audit applies |
| `secured_file.read` | List, download, or restore authorized secured files/versions | `U` for own resource | No by default | No | Yes for retrieval/restore |
| `secured_file.write` | “Amankan ke Akun” and add a version from explicitly selected result | `U` for own resource | Conditional for high-classification transfer | No | Yes |
| `secured_file.delete` | Move secured file to account recycle or permanently delete it | `U` for own resource | Required for permanent server deletion | No | Yes |
| `history.manage_own` | View, reprocess, or remove personal history without altering enterprise audit | `U` | No | Local portion yes | As applicable |
| `account.preferences.manage_own` | Read/update only allowlisted current-user preferences | `U` | No | Local preference yes; sync waits online | As applicable |
| `issue.create` | Submit own issue and explicitly selected screenshot/attachment | `U` | No; attachment send requires confirmation | No for submission | Yes |
| `issue.read_own` | Read status/detail/explicit attachment of own issue | `U` | No | No | Access metadata where required |
| `admin.user.manage` | Provision/enable/disable eligible principals within scope | `A` | Required for high-impact mutation | No | Yes |
| `admin.role.manage` | Create/update role bundles and assignments without self-escalation | `A` under separation-of-duty policy | Required | No | Yes |
| `admin.permission.manage` | Change permission assignments/delegations | Restricted `A`; separately assigned | Required | No | Yes |
| `admin.session.revoke` | Revoke another application session within scope | `A` | Required | No | Yes |
| `admin.configuration.read` | Read safe policy, Feature, limit, compatibility, and health metadata | `A` | No | No | As policy requires |
| `admin.policy.write` | Change retention, security, classification, maintenance, or governed policy | Restricted `A` | Required | No | Yes |
| `admin.feature.write` | Enable/disable or roll out an existing canonical Feature | `A` | Required for broad/high-impact change | No | Yes |
| `admin.limit.write` | Change typed limits within immutable safety ceilings | `A` | Required for broad/high-impact change | No | Yes |
| `operations.read` | View safe job, queue, sync, error, service, storage, and health metadata | `A`/Operations | No | No | Access metadata |
| `audit.read` | Query approved safe enterprise audit evidence | `Au`; `A` only by separate grant | No by default | No | Yes, access-to-audit event |
| `audit.export` | Export a bounded audit report within explicit scope | Explicit `Au`; not automatically bundled with read | Conditional/required for high-impact scope | No | Yes |
| `support.issue.read` | List/read issue message and safe diagnostics in assigned support scope | Explicit `S`; no baseline role default | No by default | No | Yes |
| `support.attachment.read` | Preview/download only explicitly sent issue attachments | Explicit `S`, separately assignable | Conditional by classification/export | No | Yes |
| `support.issue.triage` | Change category, priority, status, duplicate/not-bug/need-info state | Explicit `S` | Conditional for bulk/high impact | No | Yes |
| `support.issue.note` | Add internal support note within issue scope | Explicit `S` | No | No | Yes |
| `support.issue.export` | Export issue detail/register and authorized attachments | Explicit `S`, separately assignable | Conditional/required for high-impact disclosure | No | Yes |

Authentication mechanics, file validation, malware scanning, rate limiting, encryption, cleanup, idempotency, audit-outbox delivery, Worker execution, and other `SYSTEM_INTERNAL` Feature records are controls or workload entitlements—not permissions granted to human roles.

### 1.3 Default role bundles

| Role/bundle | Default permissions | Explicit exclusions |
|---|---|---|
| User | Own-resource user permissions above, subject to enabled Feature, capability, classification, and policy | Other users’ resources; admin; audit; support; unrestricted server route |
| Administrator | `admin.configuration.read`, approved admin writes, `operations.read` | No automatic `document.*`, `secured_file.*`, `audit.*`, `support.*`, or content access; may separately hold User role for own documents |
| Auditor | `audit.read`; `audit.export` only if explicitly assigned | No admin mutation, audit deletion, issue/document content, storage browsing, or impersonation |
| Support bundle | Only assigned `support.*` permissions and support scope | No unrelated document, job result, secured file, signature, OCR, clipboard, password, storage, admin, or audit authority |
| Workload identity | Narrow queue/object/provider/DB operation for its service and purpose | No human role, UI login, impersonation, role management, or unrestricted browsing |

## 2. Feature-to-permission mapping

This is a family/action mapping, not a new permission for every catalog record. Exceptions are explicit only when a material security reason exists.

| Canonical Feature family/action | Permission mapping | Role default | Resource/scope | Notes |
|---|---|---|---|---|
| Ordinary PDF page/view/edit/organize/optimize/compare operations in `FEAT-PDF-*` | `document.process` | User | User-selected input and own working/result | Includes Merge, Split, Rotate, Crop, forms, watermark, repair baseline |
| Ordinary `FEAT-CONV-*`, image, Office, scan, camera, and OCR tools | `document.process`; add `document.process.server` only when resolver selects server | User | Authorized input/capture and own result/job | Capability/provider/format gate and OS camera/scanner consent are not RBAC permissions |
| `FEAT-PDF-061` Protect PDF | `document.process` + `document.protect` | User | Authorized PDF and new result | Confirmation; password operation-only |
| `FEAT-PDF-062` Unlock PDF | `document.process` + `document.unlock` | User | Authorized protected PDF and new result | Valid password/authority; no bypass |
| `FEAT-PDF-069` Redact PDF | `document.process` + `document.redact` | User | Authorized PDF and irreversible redacted result | Final confirmation and verification |
| `FEAT-SIGN-001..013` visual signature/paraf/stamp | `document.process` + `document.visual_sign` | User | Own local signature asset and authorized document | Non-PKI; no auto-sync; metadata-only audit |
| `FEAT-INTEL-001..002` | `document.process` + `document.intelligence`; add server permission for enterprise-internal provider | User when approved/enabled | Minimum selected authorized content and own result | Public provider forbidden; notice/consent and minimization remain |
| `FEAT-CONV-036` PDF to Markdown | `document.process`; server modifier only for approved complex OCR/layout route | User | Authorized PDF and Markdown result | Deterministic extraction first; output untrusted |
| Save As/new result | Covered by producing Feature and output policy | User | New local result | Default behavior; no source overwrite |
| `FEAT-EDIT-012` Overwrite Original | `file.overwrite` plus producing Feature | User | Exact expected local file identity | Explicit confirmation and atomic/recoverable publication |
| `FEAT-PROD-016` Share Result | `document.share` | User subject to policy | Exact selected result and target | Confirmation and classification/target checks |
| `FEAT-BACKUP-001..002`, add version | `secured_file.write` | User | Immutable selected snapshot and own secured resource | Explicit action; max three versions |
| `FEAT-BACKUP-004..006`, `009` | `secured_file.read` | User | Own/explicitly scoped secured file/version | Reauthorize every content/range/restore action |
| `FEAT-BACKUP-007` | `secured_file.delete` | User | Own secured file and current state/ETag | Recycle and permanent deletion have different reauth rules |
| `FEAT-BACKUP-010..013` | `account.preferences.manage_own` | User | Current principal’s typed preference and ETag | Never generic table/workspace sync |
| `FEAT-HIST-001..015` | `history.manage_own` | User | Current principal’s personal projection | History delete does not delete enterprise audit |
| `FEAT-TRASH-001..005` | `local_data.delete` | User | Caller-owned app-managed local item | Permanent/empty requires confirmation; no secure-delete guarantee |
| `FEAT-AUTH-001..006`, `008..012` | Authentication/session/control, not business permission | System/current user | Current session/profile | Reauth context never grants the sensitive action |
| `FEAT-AUTH-007` | Own logout/session rule; another session uses `admin.session.revoke` | User own/Admin scoped | Exact session/principal | Other-session revocation requires reauth/audit |
| `FEAT-ADMIN-001` | `admin.user.manage` | Admin | Principal/unit scope | No content access |
| `FEAT-ADMIN-002` | `admin.role.manage` | Admin | Role/assignment scope | No self-escalation |
| `FEAT-ADMIN-003` | `admin.permission.manage` | Restricted Admin | Permission/delegation scope | Reauth and separation of duty |
| `FEAT-ADMIN-004..005` | Read through configuration-read; mutate through `admin.feature.write` | Admin | Existing Feature/environment | Cannot change Feature meaning or hard invariant |
| `FEAT-ADMIN-006..008`, `011` | Read through configuration-read; mutate through `admin.limit.write` | Admin | Typed limit and scope | Hard safety ceilings remain |
| `FEAT-ADMIN-009..010`, `012..015`; `FEAT-TRASH-011` | Read through configuration-read; mutate through `admin.policy.write` | Restricted Admin | Typed governed policy | Reauth, ETag/version, and audit |
| `FEAT-ADMIN-016..021`, `FEAT-MON-*` | `operations.read` | Admin/Operations | Safe operational metadata | Never user content |
| `FEAT-AUDIT-001`, `005..008` | Internal workload/governed lifecycle, not ordinary human permission | System | Audit pipeline/policy | No human delete/rewrite capability |
| `FEAT-AUDIT-002..003` | `audit.read` | Auditor | Approved event scope | Search/filter never widens scope |
| `FEAT-AUDIT-004` | `audit.export` | Explicit Auditor | Bounded query/export artifact | Separate from read |
| `FEAT-SUPPORT-001..004` | `issue.create`, `issue.read_own` | User | Own issue and explicitly selected attachment | Safe diagnostics exclude sensitive payloads |
| `FEAT-SUPPORT-005` | `support.issue.read`; attachment bytes additionally need `support.attachment.read` | Support | Assigned issue | No unrelated document access |
| `FEAT-SUPPORT-006` | `support.issue.triage` and/or `support.issue.note` | Support | Assigned issue and state/version | Notes may be separately delegated |
| `FEAT-SUPPORT-007..008` | `support.issue.export`; attachment permission if bytes included | Support | Bounded issue/register scope | Logged; reauth conditional |
| `FEAT-ASSIST-*` | No elevation permission; called Feature uses its normal mapping | User interface only | Same action/resource as canonical step | File-/Tool-/Assistant-First stay identical |
| `FEAT-SEC-*`, validation, cleanup, progress, platform controls | Internal control/workload authorization | System/workload | Exact service/control resource | Human roles cannot disable mandatory controls |

An enterprise may disable a single Feature ID or restrict it by classification through Feature Registry/policy without inventing `feature.execute.FEAT-PDF-005`. A Feature-specific exception is a policy rule tied to the canonical Feature ID and evaluated with the grouped action permission.

## 3. 33 Core Feature matrix

### 3.1 Legend

- `U=Allow` means a standard User is eligible when the Feature is released, visible, platform-capable, and policy-allowed. `A/Au/S=+U only` means administrative/audit/support authority does not grant document processing. `Same` in the Role-default column repeats this complete default.
- **Special** is a permission in addition to `document.process`. OS camera/scanner permission is device consent, not OTO RBAC.
- **Sensitive** means sensitivity beyond the fact that every document inherits its classification.
- **Destructive** means irreversible removal in the produced result. No Core card may silently overwrite the source.
- Audit `M` means mandatory safe metadata for start/outcome/error and local/server route; never document/OCR/password/signature/prompt/clipboard content.

| # | Core tool | Feature ID | Action | Permission | Role default / User access | Special? | Resource | Risk | Sensitive? | Destructive? | Reauth? | Online required? | Audit | Local/server effect |
|---:|---|---|---|---|---|---|---|---|---|---|---|---|:---:|---|
| 1 | Merge PDF | `FEAT-PDF-005` | Combine PDFs into new PDF | `document.process` | `U=Allow; A/Au/S=+U only` | No | Authorized inputs + own result | Low | No extra | No | No | No | M | Local; Local Worker allowed |
| 2 | Split PDF | `FEAT-PDF-006 / FEAT-PDF-007` | Split by file/page range | `document.process` | Same | No | Authorized PDF + own results | Low | No extra | No | No | No | M | Local |
| 3 | Remove Pages | `FEAT-PDF-013` | Remove pages from working state | `document.process` | Same | No | Own working copy | Low | No extra | No—reversible working state | No | No | M | Local; source unchanged |
| 4 | Extract Pages | `FEAT-PDF-014` | Create PDF from selected pages | `document.process` | Same | No | Authorized PDF/pages + own result | Low | Selected content | No | No | No | M | Local |
| 5 | Organize PDF | `FEAT-PDF-010..016` | Page organization workflow | `document.process` | Same | No | Own working copy + result | Low | No extra | No by default | No | No | M | Local workflow |
| 6 | Scan to PDF | `FEAT-SCAN-014` | Capture/import scan and create PDF | `document.process` | Same | OS camera/scanner consent only | Foreground capture/import + own result | Low catalog; capture sensitive | Yes | No | No | No for local; physical scanner gated | M | Local/device; physical scanner PoC remains |
| 7 | Compress PDF | `FEAT-PDF-023` | Produce smaller PDF | `document.process` | Same | No | Authorized PDF + own result | Low | No extra | No | No | No | M | Local/Local Worker |
| 8 | Repair PDF | `FEAT-PDF-025 / FEAT-PDF-026` | Repair malformed structure | Base; add `document.process.server` for fallback | Same | Conditional server permission | Quarantined input + own result/job | Low catalog; parser risk elevated | Malformed input | No | No default | Conditional fallback | M | Basic local; optional advanced fallback |
| 9 | OCR PDF | `FEAT-OCR-004` | Add searchable text layer | Base; server addition for approved fallback | Same | Conditional server permission | Authorized pages + restricted OCR derivative | Medium | Yes—derived text | No | Policy conditional | Conditional fallback | M | Local first; optional server |
| 10 | JPG to PDF | `FEAT-CONV-007` | Convert images to PDF | `document.process` | Same | No | Authorized images + own result | Low | Image decoder surface | No | No | No | M | Local |
| 11 | Word to PDF | `FEAT-CONV-001` | Convert Word/DOCX to PDF | Base; server addition only if selected | Same | Conditional server permission | Authorized document + result/job | Low catalog; active-content controls | Yes—Office input | No | No default | Conditional | M | Hybrid optional; feasibility-gated |
| 12 | PowerPoint to PDF | `FEAT-CONV-003` | Convert presentation to PDF | Same as #11 | Same | Conditional server permission | Authorized presentation + result/job | Low catalog; active-content controls | Yes—Office input | No | No default | Conditional | M | Hybrid optional; feasibility-gated |
| 13 | Excel to PDF | `FEAT-CONV-002` | Convert workbook/sheets to PDF | Same as #11 | Same | Conditional server permission | Authorized workbook + result/job | Low catalog; active-content controls | Yes—sheet/formula data | No | No default | Conditional | M | Hybrid optional; feasibility-gated |
| 14 | HTML to PDF | `FEAT-CONV-017` | Render supplied HTML to PDF | Base; server addition only if selected | Same | Conditional server permission | Authorized HTML/assets + result/job | Low catalog / Medium security | Yes—untrusted renderer input | No | No default | Conditional; remote URL fetch unavailable | M | Isolated renderer; feasibility-gated |
| 15 | PDF to JPG | `FEAT-CONV-012` | Render selected pages to images | `document.process` | Same | No | Authorized pages + own images | Low | Derived images | No | No | No | M | Local |
| 16 | PDF to Word | `FEAT-CONV-004` | Convert PDF structure to Word | Base; server addition only if selected | Same | Conditional server permission | Authorized PDF + result/job | Low | Extracted content | No | No default | Conditional | M | Hybrid optional; feasibility-gated |
| 17 | PDF to PowerPoint | `FEAT-CONV-006` | Convert PDF to presentation | Same as #16 | Same | Conditional server permission | Authorized PDF + result/job | Low | Extracted/rendered content | No | No default | Conditional | M | Hybrid optional; feasibility-gated |
| 18 | PDF to Excel | `FEAT-CONV-005` | Extract tables/data to workbook | Same as #16 | Same | Conditional server permission | Authorized PDF + result/job | Low | Yes—structured data | No | Policy conditional | Conditional | M | Hybrid optional; feasibility-gated |
| 19 | PDF to PDF/A | `FEAT-PDF-075` | Produce validated conformance result | Base; server addition only if selected | Same | Conditional server permission | Authorized PDF + result/job | Medium | Metadata/font/repair behavior | No | No default | Conditional | M | Optional hybrid; feasibility-gated |
| 20 | Rotate PDF | `FEAT-PDF-012` | Rotate selected pages | `document.process` | Same | No | Own working copy + result | Low | No extra | No | No | No | M | Local |
| 21 | Add Page Numbers | `FEAT-PDF-037` | Add numbering | `document.process` | Same | No | Own working copy + result | Low | No extra | No | No | No | M | Local |
| 22 | Add Watermark | `FEAT-PDF-036` | Add text/image watermark | `document.process` | Same | No | Authorized asset + own document/result | Low | Conditional asset | No | No | No | M | Local |
| 23 | Crop PDF | `FEAT-PDF-019` | Change page crop area | `document.process` | Same | No | Own working copy + result | Low | Hidden/out-of-crop awareness | No—source preserved | No | No | M | Local |
| 24 | Edit PDF | `FEAT-PDF-031..041` | Edit supported content | Base; server addition for approved gated subset | Same | Conditional server permission | Own working copy + result/job | Low catalog / Medium workflow | Content editing | No | No default | Conditional subset | M | Local baseline; gated existing-text subset |
| 25 | PDF Forms | `FEAT-PDF-053..060` | Fill/create/modify/flatten fields | `document.process` | Same | No | Authorized form + own result | Low catalog | Field data may be sensitive | Flatten irreversible in result; source preserved | No default | No baseline | M | Local workflow baseline |
| 26 | Unlock PDF | `FEAT-PDF-062` | Create unprotected result with valid password | Base + `document.unlock` | User default includes special grant | Yes | Protected PDF + new result | High | Password/protection removal | No source destruction | No default; policy conditional | No, local | M | Password never persisted/audited |
| 27 | Protect PDF | `FEAT-PDF-061` | Create protected result | Base + `document.protect` | User default includes special grant | Yes | Authorized PDF + new protected result | Medium | Password/access state | No source destruction | No default | No, local | M | Final confirmation; secret excluded |
| 28 | Sign PDF | `FEAT-SIGN-013` | Place visual signature and finalize | Base + `document.visual_sign` | User default includes special grant | Yes | Local signature asset + PDF/result | Medium | Reusable visual asset | No source destruction | No default | No, local | M | Visual non-PKI only |
| 29 | Redact PDF | `FEAT-PDF-069` | Remove selected content in new result | Base + `document.redact` | User default includes special grant | Yes | PDF, selected regions, validated result | High | Sensitive removal assurance | Yes in finalized result | No default; policy conditional | No, local | M | Preview, final confirmation, validation |
| 30 | Compare PDF | `FEAT-PDF-071 / FEAT-PDF-072` | Compare authorized PDFs/pages | `document.process` | Same | No | Two authorized inputs + result | Low | Cross-document content | No | No | No | M | Local/Local Worker |
| 31 | AI Summarizer | `FEAT-INTEL-001` | Summarize minimum selected content | Base + `document.intelligence`; server addition for internal provider | User only when provider/Feature approved | Yes | Selected pages/text + own result | Medium | Restricted content/provider/prompt injection | No | Policy/provider conditional | Local no; internal server yes | M | Approved local/internal only; public AI forbidden |
| 32 | Translate PDF | `FEAT-INTEL-002` | Translate explicit content/language scope | Same as #31 | Same | Yes | Selected content + translated result | Medium | Restricted content/provider | No | Policy/provider conditional | Local no; internal server yes | M | Approved local/internal only; public endpoint forbidden |
| 33 | PDF to Markdown | `FEAT-CONV-036` | Extract structured Markdown | Base; server addition for approved complex OCR/layout | User | Conditional server permission | PDF + restricted untrusted result | Low catalog; extracted-content sensitivity | Yes—text/OCR output | No | Policy conditional | Local no; otherwise conditional | M | Deterministic local first |

The Core-card permission never authorizes Save As overwrite, share, upload, or “Amankan ke Akun” merely because processing is authorized. Those actions are evaluated separately at the output boundary.

## 4. Sensitive action matrix

Action modes are cumulative: `NORMAL` is ordinary authorized execution; `CONFIRMATION` proves explicit intent for the exact action/resource/target; `REAUTH` supplies fresh scoped authentication; and `ONLINE_AUTHORIZATION` obtains current server identity, permission, ownership, policy, state, and version.

| Sensitive action | Canonical Feature/action | Permission | Resource | Risk | Required mode | Online? | Audit? | Key rule |
|---|---|---|---|---|---|---|---|---|
| Routine process | Invoked Core Feature | `document.process` | Authorized input + own result | Low–Medium | `NORMAL`; upload adds explicit transfer confirmation/notice | Selected server route only | Yes | Local-first; no source overwrite |
| Protect PDF | `FEAT-PDF-061` | Base + `document.protect` | Exact PDF/new protected result | Medium | `CONFIRMATION` before final export | No locally | Yes | Password never persisted |
| Unlock PDF | `FEAT-PDF-062` | Base + `document.unlock` | Protected PDF/new unprotected result | High | `CONFIRMATION`; valid password/authority | No locally | Yes | No bypass; reauth policy may elevate |
| Redact PDF | `FEAT-PDF-069` | Base + `document.redact` | Selected regions/new final result | High/irreversible | `CONFIRMATION` immediately before finalization | No locally | Yes | Removal/flatten verification |
| Visual Sign PDF | `FEAT-SIGN-013` | Base + `document.visual_sign` | Local signature asset/document/result | Medium | `CONFIRMATION` before final export | No locally | Yes, metadata only | Non-PKI; no asset upload/audit payload |
| Amankan ke Akun | `FEAT-BACKUP-001..002` | `secured_file.write` | Immutable selected snapshot | Medium/disclosure | `CONFIRMATION` + `ONLINE_AUTHORIZATION`; `REAUTH` if classification requires | Yes | Yes | Max three versions; execution upload is separate |
| Secured restore/download | `FEAT-BACKUP-004..006` | `secured_file.read` | Exact authorized version | Medium | `ONLINE_AUTHORIZATION`; confirmation if replacing output | Yes | Yes | New verified staging/Save As |
| Move secured file to Account Recycle | `FEAT-BACKUP-007` lifecycle | `secured_file.delete` | Exact secured resource/ETag | High but recoverable | `CONFIRMATION` + `ONLINE_AUTHORIZATION` | Yes | Yes | Tombstone/state transition |
| Permanent secured deletion | Permanent server lifecycle action | `secured_file.delete` | Exact deletable secured resource | High/irreversible | `CONFIRMATION` + `REAUTH` + `ONLINE_AUTHORIZATION` | Yes | Yes | Retention/legal hold checked first |
| Permanent local delete / Empty Recycle Bin | `FEAT-TRASH-004..005` | `local_data.delete` | Own app-managed items | High | `CONFIRMATION`; no default reauth | No with valid offline grant | As required | No absolute secure-delete claim |
| Overwrite | `FEAT-EDIT-012` | `file.overwrite` + producing Feature | Exact expected local file identity | High/data loss | `CONFIRMATION`; no default reauth | No locally | Yes | Atomic/recoverable; never Assistant-inferred |
| Share | `FEAT-PROD-016` | `document.share` | Exact result + approved target | Medium–High disclosure | `CONFIRMATION`; online/reauth if policy requires | Conditional | Yes | Target/payload visible |
| Submit issue screenshot | `FEAT-SUPPORT-002..003` | `issue.create` | Own issue + previewed attachment | Medium disclosure | `CONFIRMATION` on Send + `ONLINE_AUTHORIZATION` | Yes | Yes | No auto-upload; message survives attachment failure |
| User/role/permission mutation | `FEAT-ADMIN-001..003` | Corresponding admin permission | Principal/role/assignment | High privilege impact | `REAUTH` + `ONLINE_AUTHORIZATION` | Yes | Yes | No self-escalation; dual control per policy |
| Session revocation | `FEAT-AUTH-007` other session | `admin.session.revoke` | Exact session/principal | High | `REAUTH` + `ONLINE_AUTHORIZATION` | Yes | Yes | Idempotent, scoped |
| Security/classification/retention policy change | `FEAT-ADMIN-009..013` | `admin.policy.write` | Typed policy/version/scope | High systemic | `REAUTH` + `ONLINE_AUTHORIZATION` | Yes | Yes | Hard safety bounds cannot be weakened |
| Feature/maintenance/min-version change | `FEAT-ADMIN-004..005`, `014..015` | Feature/policy write | Feature/environment/version | Medium–High | Reauth for broad/high-impact + online | Yes | Yes | ETag, staged rollout, rollback |
| Limit/quota change | `FEAT-ADMIN-006..008`, `011` | `admin.limit.write` | Typed limit/scope | Medium–High | Reauth for broad/high-impact + online | Yes | Yes | Immutable ceilings |
| Audit query/detail | `FEAT-AUDIT-002..003` | `audit.read` | Approved event scope | High confidentiality/evidence | `ONLINE_AUTHORIZATION` | Yes | Yes | No content or delete implication |
| Audit export | `FEAT-AUDIT-004` | `audit.export` | Bounded query/export | High bulk disclosure | `CONFIRMATION` + online; reauth for high impact | Yes | Yes | Separate from read |
| Issue support access | `FEAT-SUPPORT-005` | `support.issue.read`; attachment separate | Assigned issue only | High | `ONLINE_AUTHORIZATION` | Yes | Yes | No unrelated content access |
| Support attachment/export | `FEAT-SUPPORT-005`, `007..008` | Attachment/export permission | Sent attachment/bounded report | High disclosure | Online; export confirmation; reauth conditional | Yes | Yes | Separate attachment grant |
| Exceptional content access | Not defined/in scope | No permission exists | User document outside explicit issue attachment | Critical | Deny | — | Denial/security event | Future capability needs product/security/legal approval |

## 5. Reauthentication matrix

| Action category | Default | Rationale |
|---|---|---|
| Permanent deletion of secured server content | **Required** | Irreversible cross-device loss |
| Role/permission/security/classification changes | **Required** | Privilege/systemic impact |
| Revocation of another session | **Required** | Immediate access/availability impact |
| Key recovery/destructive restore/emergency recovery | **Required** | Cryptographic/recovery authority |
| Exceptional user-content access, if ever approved | **Required** | Admin/support authority currently excludes content |
| High-impact audit/support export or external disclosure | **Conditional-to-required** | Bulk disclosure risk |
| High-classification secured transfer/server transfer/share | **Conditional** | Policy and transfer authority must be fresh |
| Broad feature flag, maintenance, minimum version, retention, or limit change | **Conditional-to-required** | System-wide blast radius |
| Protect/Unlock/Redact/Visual Sign | Not required by default | Own local resource plus confirmation/valid password/output assurance; policy may elevate |
| Local overwrite/permanent recycle delete | Not required by default | Own-resource confirmation and offline usability; policy may elevate |
| Routine Core processing/cancel/retry/resume | Not required | Resource permission/state checks are repeated |
| Read own secured file/issue | Not required by default | Current online authentication/resource authorization still required |

Reauthentication uses the approved Pertamina Identity method and is short-lived and scoped to principal, session, client, action, resource, policy/version, and assurance. It is invalidated by logout, disablement, revocation, permission/policy/resource-version change, or expiry; is checked before partial mutation; is unavailable offline; and never grants a missing permission. Missing freshness returns `401 REAUTHENTICATION_REQUIRED`; a valid identity lacking permission remains `403`.

Exact assurance method, age, attempt/lock behavior, dual-control actions, and classification triggers require Pertamina Identity/Security confirmation.

## 6. Online-required matrix

| Capability/action | Online requirement | Offline behavior |
|---|---|---|
| Ordinary local Core Feature | No while protected offline grant/policy is valid | Execute locally; queue safe audit metadata; never upload |
| Local Protect/Unlock/Redact/Visual Sign | No by default | Execute with cached eligible permission and confirmation; request PDF password again |
| Optional server processing | Yes only when resolver selects server | Wait or offer valid local alternative; never silently upload |
| Required server processing | Yes | Explain dependency; no execution/upload until current authorization |
| AI Summarizer/Translate | Local provider no; enterprise-internal provider yes | Approved local provider only; no public fallback |
| PDF to Markdown | Deterministic local no; approved complex route conditional | Use local route or wait |
| Amankan/add version | Yes | Preserve local file; require fresh confirmation/authorization before upload |
| Secured list/download/restore/delete | Yes | Cache may be labeled stale metadata only; no content/control from cache |
| Share | Conditional on target/classification | Only target allowed by valid cached policy; otherwise wait |
| Local Save As/overwrite/recycle/delete | No | Allowed with exact resource checks and confirmation |
| Submit/read issue | Yes | Local message draft may remain; screenshot stays local until Send |
| Admin/Operations/Support/Audit surfaces | Yes | Deny/wait; no privileged offline projection |
| Reauth-required action | Yes | Deny/wait; cached login is not fresh reauth |
| Audit metadata generation | No | Protected outbox; idempotent sync later |
| Audit query/export | Yes | Deny/wait; local outbox is not Auditor reporting storage |

Connectivity is not authorization. Reconnect refreshes identity, account/session, permission version, policy/classification, compatibility, capability, resource ownership/state/ETag, consent, and reauthentication before any queued command.

## 7. Local/offline permission behavior

1. **Minimum projection only.** The protected offline grant contains profile/principal binding, issued/last-online/absolute-expiry, offline-eligible action permissions, classification ceilings, Feature/capability/policy/contract versions, device binding where supported, integrity/rollback evidence, and last-known revoke/disable marker. It contains no token, cookie, password, service credential, unrestricted claims, server/support/admin/audit permission, or document content.
2. **Local authorization still runs.** The application service checks canonical Feature state, action permission, exact local file access, classification, capability, engine availability, resource state, confirmation, and output target. Offline never means authorization bypass.
3. **Absolute expiry.** The grant has no offline sliding renewal. Trusted last-online and monotonic elapsed time are used where available. Clock rollback, reboot, sleep, timezone, or DST cannot extend it; uncertain time fails closed for protected action.
4. **No server authority from cache.** Server-processing, secured-file, issue, admin, support, audit query/export, and reauth-dependent actions cannot execute from cached claims.
5. **Permission is not capability.** An eligible `document.process` grant does not expose every Feature. The Feature must still be visible, enabled, compatible, platform-capable, schema-valid, and locally executable.
6. **No automatic route change.** Permission to process locally does not imply permission to transfer. If server is selected, `document.process.server`, policy/classification, notice/consent, capacity, and online resource checks run before upload.
7. **Queued intent is not authorization.** An offline click may preserve a minimal intent/message draft, but reconnect performs a new decision. If permission/policy changed, the UI explains denial and preserves source/draft where safe.
8. **Revocation risk is bounded.** When disablement/revocation is learned, new actions stop, protected profile/session is locked as defined, server results are no longer exposed, and Workers stop/quarantine at safe boundaries. Before knowledge, only the short absolute-expiry local grant remains.
9. **Audit continuity.** Required local events enter a protected idempotent outbox. It contains no document name/path unless approved and necessary, content, OCR text, password, signature, clipboard data, or prompt body.
10. **Account isolation.** Switching Pertamina subject unmounts the prior profile. Cached permission from one subject/device/profile is never reusable by another.

## 8. Assistant mapping

The Assistant has no “execute anything” or “act as user” permission. `FEAT-ASSIST-*` controls the interface/orchestration capability but never replaces the called Feature’s permission.

```text
Assistant intent
→ canonical Feature ID and typed parameters
→ same action-permission mapping as File-First/Tool-First
→ same resource ownership
→ same policy/classification/capability/provider/capacity checks
→ same per-step confirmation and reauthentication
→ official application service
→ actual persisted result/status
```

- File-First, Tool-First, and Assistant-First use identical handlers.
- A multi-step plan authorizes every step at execution time. Merge permission does not authorize Share, Overwrite, Redact, Amankan ke Akun, or another later action.
- The Assistant may explain unavailability but cannot leak hidden resource/policy/role detail.
- It cannot create Feature IDs, permission codes, providers, URLs, file paths, engines, commands, or APIs at runtime.
- It cannot infer confirmation for irreversible, overwrite, permanent-delete, upload, share, final-export, visual-sign, Protect/Unlock, or Redact finalization. The UI must show exact action, resource, target, and material effect.
- It cannot satisfy reauthentication. It routes to approved reauth and resumes only with a valid scoped freshness context.
- Offline execution is allowed only if every plan step is locally executable and present in the valid offline projection. A server/freshness-required step waits or is denied; it is never silently skipped.
- Document/OCR/HTML/annotation/hidden text and model output are untrusted data, not authorization instructions.
- Audit records Feature/action, outcome, correlation/workflow ID, and a safe `invokedVia=ASSISTANT` enum—not prompt, password, document/OCR body, signature, clipboard, or model output.

Example:

```text
“Gabungkan dua PDF, redact halaman 3, lalu Amankan ke Akun”

Step 1 — FEAT-PDF-005:
  document.process

Step 2 — FEAT-PDF-069:
  document.process + document.redact
  final redaction confirmation

Step 3 — FEAT-BACKUP-001:
  secured_file.write
  explicit transfer confirmation + current online authorization
  conditional reauth under classification policy
```

No prior step’s permission or confirmation approves a later step.

## 9. Audit-required recommendations

### 9.1 Minimum event strategy

All 33 Core Tool executions produce safe audit metadata because the requirements establish minimum Feature/process audit. Local events may queue offline. Audit does not mean payload logging.

| Action/event | Audit? | Minimum safe fields | Prohibited |
|---|:---:|---|---|
| Feature accepted/started/completed/failed/cancelled | Yes | Event/principal ID, role/permission version, Feature/action, time, local/server, status, safe size/count, error code, app/platform/version, Job/Correlation ID | Content, OCR body, password, signature, clipboard, prompt/output |
| Upload/download/result retrieval | Yes | Purpose, Feature/transfer/job/result IDs, owner-scope outcome, size bucket, digest outcome, status, correlation | Payload, raw storage key/path, delegated URL, snippet |
| Protect/Unlock | Yes | Feature/action, resource/result IDs, outcome/error category, correlation | Password, encryption key, unprotected content |
| Redact | Yes | Resource/result IDs, safe counts, validation/confirmation outcome | Original/redacted content; sensitive coordinates |
| Visual Sign | Yes | Action, actor, time, document/result ID, outcome | Signature bytes/template/thumbnail |
| Amankan/add/restore/delete version | Yes | Secured/file/version IDs, action, ETag/version, digest/reauth/confirmation outcome | Content, storage reference, unapproved filename |
| Overwrite/delete/share | Yes where enterprise audit applies | Action, safe resource identity, target class, confirmation, outcome | Content, unrestricted path, share credential |
| User/role/permission/session mutation | Yes | Actor, target ID, scope, safe before/after value, reason/reauth reference, outcome | Tokens, raw claims, credentials |
| Policy/Feature/limit mutation | Yes | Actor, key/Feature ID, scope/environment, version/ETag, safe before/after value, approval, outcome | Secrets/internal exploit detail |
| Audit query/export | Yes | Actor, query scope, count bucket, export ID, reauth/decision, outcome | Returned evidence body in access log |
| Support read/attachment/note/triage/export | Yes | Actor, issue/attachment/export ID, assigned scope, action, ETag, outcome | Screenshot bytes, issue body, document/OCR/password/signature |
| Authorization denial/IDOR/reauth failure | Security policy | Principal/session/client, action, resource type/opaque ID, decision code, policy version, correlation | Existence/details where concealment applies |

### 9.2 Separation rules

- Personal history is a user projection; deleting it does not delete enterprise audit.
- Audit has no ordinary delete permission. Retention/purge is governed lifecycle, not an Auditor action.
- `audit.read` and `audit.export` are distinct. Search/filter/cursor cannot widen unit/time/row scope.
- Administrator, DBA, backup operator, monitoring operator, and Support do not automatically receive audit-read or document-content rights.
- Audit access and export are audited.
- Idempotent request replay produces one logical effect and a clear replay relationship, not duplicate business events.
- SignalR is notification only; persisted audit/job/resource state is authoritative.
- Document 10 must define the final event catalog, field allowlist, integrity, legal retention/hold, masking, export protection, alerting, and SIEM integration.

## 10. Permission simplification opportunities

1. Map Merge, Split, page organization, compression, repair, routine conversions, rotate, crop, watermark, editing, forms, compare, scan, and OCR baseline to `document.process`; do not create per-tool permissions.
2. Keep platform exposure, rollout, feasibility gate, provider availability, schema version, and Feature enable/disable in Feature Registry/policy rather than RBAC.
3. Use `document.process.server` as a conjunctive transfer/server-route modifier, never a super-permission.
4. Keep only justified sensitive exceptions: Protect, Unlock, visual Sign, Redact, and intelligence.
5. Keep disclosure separate: `document.share` and `secured_file.write` are not implied by document processing.
6. Keep `file.overwrite`, `local_data.delete`, and `secured_file.delete` separate because resources, recovery, online, and reauth differ.
7. Prefer one permission plus ownership/unit/case resource handlers over `read_own/read_team/read_all` variants for every domain.
8. Separate read/export and metadata/content only where disclosure materially changes—Audit and Support are the main cases.
9. Treat confirmation, reauth, classification, target, size, and quota as independent policy conditions; never create a composite permission such as `redact.highclassification.largefile.online`.
10. Treat camera, scanner, file picker, notifications, and clipboard gesture as OS capability/consent unless independent business restriction exists.
11. Treat validation, scanning, encryption, cleanup, audit sync, job lease, storage adapter, and provider access as purpose-bound workload authorization.
12. Use centrally versioned role templates; deny self-assignment; invalidate sessions/offline projections after changes; allow added roles without changing Feature IDs.

The target is a compact action vocabulary plus resource/policy conditions, not permission per card, button, endpoint, Feature ID, engine, or platform.

## 11. Risks/conflicts

| Risk/conflict/open decision | Impact | Recommendation/owner |
|---|---|---|
| Exact permission names/role assignments are not finalized | Teams may invent incompatible codes | Document 09 locks vocabulary, bundles, scope, delegation, and versioning |
| Feature Catalog marks Audit UI `AUDITOR/ADMIN`, while Security Design requires authority separation | UI marker may be misread as Admin entitlement | Treat marker as eligible UI only; Admin gets `audit.*` solely by separate grant; Security/Compliance confirms |
| “Superadmin” wording versus Support permission bundle | Risk of unrestricted bypass role | Keep explicit scoped Support permissions; no all-powerful role |
| User defaults for Protect/Unlock/Redact/Sign | Over-restriction harms Core usability; over-grant may breach classification policy | Standard User for own resources by default, subject to policy; Pertamina confirms exceptions |
| Reauth trigger, age, assurance, dual control, lock behavior unspecified | Inconsistent sensitive protection | Pertamina Identity/Security decision before production |
| Share may be local/platform-mediated or remote | No universal online answer | Classify targets; offline only under valid cached target policy |
| Permanent local versus secured deletion | Wrong reauth/recovery expectation | Clear location/state in UI; distinct resource handlers |
| Document 03 rates `FEAT-CONV-017` Low while Document 06 security mapping treats it Medium | Risk labels diverge | Preserve catalog value; record Medium renderer security treatment; Integrator aligns presentation |
| `FEAT-PDF-075` is Core but `Could / R3` and feasibility-gated | Permission may look like availability | Permission never overrides status/capability; align release sequencing without changing ID |
| Advanced repair/edit, Office/PDF-A/OCR/provider, scanner remain PoC-gated | False availability/server fallback | Discovery requires permission plus approved capability/provider |
| AI governance remains pending | Restricted content exposure | Default deny provider route until approved; public AI technically unreachable |
| Official classification/high-impact thresholds pending | Conditional reauth/online cannot be deterministic | Security/Data Governance supplies typed policy/test cases |
| Audit schema/retention/export belongs to Document 10 | Excess evidence disclosure | Document 10 finalizes event/scope/export; Document 09 grants actions only |
| Exceptional content access undefined | Hidden admin/support bypass may appear | Explicit deny; future design needs product/security/legal approval, purpose, reauth, time limit, evidence |

No recommendation adds or changes Feature IDs, alters Core scope, grants Administrator/Support/Auditor automatic document access, introduces PKI/full cloud, or gives the Assistant a bypass.

## 12. Suggested Document 09 text

### Feature, action, permission, and resource model

PERTAMINA OTO SHALL NOT create a permission for each of the 550 canonical Feature records. Feature ID SHALL identify the stable capability and typed contract. Permission SHALL represent a business/security action over a resource type and scope. Role SHALL be a versioned permission bundle and SHALL NOT be the final authorization decision.

Authorization SHALL require valid identity/session or eligible protected offline grant, active account, active/visible Feature, action permission, resource ownership or explicit scope, enterprise policy/classification, current resource state/version, compatible client/platform/capability, required confirmation, and fresh reauthentication when applicable. UI visibility, public ID, path, Job ID, Correlation ID, ETag, storage reference, or prior decision SHALL NOT prove access.

Ordinary PDF, conversion, image, Office, scan, OCR, form, and compare operations SHOULD use `document.process`. An approved server route SHALL additionally require `document.process.server`; local permission SHALL NOT authorize upload. Protect, Unlock, visual Sign, Redact, and approved intelligence SHOULD use separate action permissions because their credential, irreversible, sensitive-asset, or provider risks are independently governable.

Share, overwrite, local delete, secured-file read/write/delete, issue, administrative, audit, and support actions SHALL remain separate from processing. Transforming a document SHALL NOT imply authority to disclose it, overwrite the source, permanently delete it, or secure it to an account.

The User role SHOULD receive eligible own-resource document permissions subject to Feature status, capability, classification, and policy. Administrator SHALL receive only approved administrative/operational metadata permissions and SHALL NOT automatically receive document, secured-file, audit, support, OCR, password, clipboard, signature, or issue-attachment access. Auditor SHALL receive scoped audit read and only explicit audit export. “Superadmin” SHALL be an explicit Support permission bundle, not an unrestricted role.

Support SHALL separate issue read, explicitly sent attachment read, triage, internal note, and export. Audit SHALL separate read from export and SHALL imply neither audit deletion nor document access. Operations, DBA, backup, storage, and workload authority SHALL NOT imply business-content access.

All 33 Primary PDF Tool Cards SHALL use `document.process` except justified additions. `FEAT-PDF-061` SHALL additionally require Protect permission; `FEAT-PDF-062` Unlock; `FEAT-PDF-069` Redact; `FEAT-SIGN-013` visual-sign; and `FEAT-INTEL-001..002` intelligence. Optional server paths SHALL additionally require server-processing permission. `FEAT-CONV-036` SHALL use ordinary processing with deterministic local extraction first and the server modifier only for an approved complex route.

Every Core Tool SHALL be available to a standard User only when released, visible, enabled, platform-capable, schema-compatible, policy-allowed, and authorized for the exact resource. Administrator, Auditor, or Support authority alone SHALL NOT grant Core access; acting on one’s own document requires User document permission.

Confirmation SHALL be required for content transfer, overwrite, permanent deletion, share, final export, final redaction, visual-sign finalization, and other policy-defined sensitive actions. It SHALL identify exact action, resource, target, and material effect and SHALL NOT substitute for permission, ownership, online authorization, or reauthentication.

Fresh reauthentication SHALL be required for permanent secured deletion, role/permission/security changes, revocation of another session, key/destructive recovery, and any future exceptional content access. It SHALL be conditional-to-required for high-impact audit/support export, broad policy/Feature/limit change, and high-classification transfer/disclosure. Protect, Unlock, Redact, visual Sign, local overwrite, and local permanent delete SHALL not require reauth by default for the User’s own local resource, but classification policy MAY elevate them.

Reauthentication SHALL be short-lived and scoped to principal, session, client, action, resource, policy/version, and assurance; invalidated by logout, disablement, revocation, permission/policy/resource-version change, or expiry; and checked before mutation. It SHALL never grant missing permission. Missing freshness SHALL return `401 REAUTHENTICATION_REQUIRED`.

Eligible deterministic local Features MAY run offline under a protected absolute-expiry grant containing only minimum local permissions, versions, classification ceilings, binding, and integrity/time evidence. It SHALL contain no token, password, unrestricted claims, privileged server permission, or content. Upload, server processing, secured storage, issue submission, Admin/Support/Auditor surfaces, audit export, and reauth-required actions SHALL wait or fail safely until online.

Queued offline intent SHALL NOT be authorization. Reconnect SHALL refresh identity, session, permission/policy/capability/compatibility, resource ownership/state/version, consent, and reauth before dispatch. Denial SHALL preserve local source/draft where safe.

The Assistant SHALL have no bypass permission. File-First, Tool-First, and Assistant-First SHALL use identical Feature, permission, resource, policy, confirmation, reauth, and audit handlers. Every workflow step SHALL be independently authorized at execution time. Document/OCR/HTML/model content SHALL remain data and cannot grant permission, request upload/share, change policy, select arbitrary providers, or confirm action.

Every Primary Tool execution SHALL create approved safe audit metadata for start/outcome/error and local/server route. Passwords, tokens, document/OCR content, signatures, clipboard, prompts/responses, raw paths/storage references, screenshot bytes, and issue bodies SHALL NOT enter audit payloads. Local events MAY enter a protected idempotent outbox.

Administrative, session, policy, Feature, limit, audit-access/export, support-access/attachment/export, secured-version, permanent-delete, overwrite, share, and reauth decisions SHALL be audited with safe action/resource/scope/version/outcome metadata. Personal-history deletion SHALL NOT delete enterprise audit. Audit access/export SHALL themselves be auditable.

Feature rollout, platform support, feasibility gate, provider availability, schema compatibility, classification, capacity, and limits SHALL remain Feature Registry/policy conditions rather than new permissions. OS camera/scanner/filesystem/clipboard controls SHALL remain capability/consent checks unless an independent business restriction applies. Mandatory security controls and workload actions SHALL use purpose-bound service authorization and SHALL NOT be grantable to human roles as a way to disable them.
