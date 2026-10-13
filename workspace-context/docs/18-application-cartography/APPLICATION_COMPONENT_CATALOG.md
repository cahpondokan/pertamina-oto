# APPLICATION COMPONENT CATALOG
## PERTAMINA OTO — Document 18

> Versi: FINAL v1.0
> Status: PRE-IMPLEMENTATION LOGICAL CATALOG
> Owner resmi: REQUIRES_PERTAMINA_OWNER_CONFIRMATION

Catalog ini memetakan logical component, bukan class/microservice/deployable aktual. Semua actual path, package, artifact digest, instance, version, dan health endpoint menunggu implementasi.

## Catalog

| ComponentId | Name | Type / Module | Responsibility / Business capability | Host / Processing | Owned data | Interfaces provided / consumed | Zone / Permission / Audit | Health / Monitoring | Artifact / Version / Criticality | Owner / Runbook / Test / Status |
|---|---|---|---|---|---|---|---|---|---|---|
| CART-CMP-001 | Feature Registry | Application / AccountAdministration anchor TBD | 550 Feature, schema, exposure, capability | API + client projection / hybrid system | Registry/config projection | Provides registry query; consumes config | API/Data; filtered auth; config Audit | version/freshness/count | API/client; TBD; TBD_OFFICIAL | TBD; Config/Data Integrity; contract tests; DESIGNED |
| CART-CMP-002 | Processing Coordinator | Application / Workspace anchor TBD | Preflight, local/server route, Job coordination | Desktop/API / hybrid | Operation coordination refs | Provides processing; consumes registry/auth/jobs/resolver | Client/API; Feature permission; L2/L3 | admission/state/correlation | Desktop/API; TBD; TBD_OFFICIAL | TBD; Worker/Queue; state tests; DESIGNED |
| CART-CMP-003 | Engine Resolver | Domain service / Processing.Abstractions | Approved capability/provider selection | Desktop/API/Workers / local-hybrid-server | No business data | Provides resolver; consumes profiles/capability/policy | Processing zones; deny unsafe route | selection/error/provider profile | Host cohort; version TBD | TBD; Engine family; resolver tests; DESIGNED |
| CART-CMP-004 | PDF Application Service | Application / Pdf | Page/edit/forms/security/compare/conformance | Desktop/API/Workers / local-first | PDF working operation metadata | Provides Pdf contract; consumes Workspace/OCR/Signature/Resolver | Local/API; document permissions; L2/L3 | result validation/duration | Desktop/Workers; TBD; TBD_OFFICIAL | TBD; Data Integrity/Redaction; Pdf tests; DESIGNED |
| CART-CMP-005 | Conversion Application Service | Application / Conversion | Typed format conversion | Desktop/API/Workers / hybrid | Conversion manifest/parameters | Provides conversion; consumes Workspace/OCR/Resolver | Local/API; process+server; L2 | fidelity/error/resource | Desktop/Workers; TBD | TBD; Conversion; contract/fidelity tests; POC_REQUIRED |
| CART-CMP-006 | Image Application Service | Application / Image | Image processing | Desktop/Mobile/Local Worker | Image working metadata | Provides image; consumes Workspace/Resolver | Client/Local; base permission; L2 | output validation/resource | Client cohort; TBD | TBD; family runbook; image tests; PLANNED |
| CART-CMP-007 | Scanner/OCR Service | Application / ScannerOcr | Capture, OCR, searchable output | Mobile/Desktop/Workers / hybrid | Capture/OCR working metadata | Provides Scan/OCR; consumes device/Pdf/Workspace/Resolver | Device/Processing; consent+base/server; L2 | device/engine/fidelity | Client/Workers; TBD | TBD; OCR/AI; OCR tests; POC_REQUIRED |
| CART-CMP-008 | Signature/Annotation Service | Application / SignatureAnnotation | Visual non-PKI asset/placement/annotation | Desktop/Mobile / local | Protected local asset refs | Provides signature/annotation; consumes Pdf/Image/Workspace | Local sensitive; signature permission; L2/L3 | protected store/output | Client; TBD | TBD; Data Integrity; security tests; DESIGNED |
| CART-CMP-009 | Workspace Service | Application / Workspace | Draft, operation log, recovery, result refs | Desktop/Mobile + API subset / hybrid | SQLite/workspace state | Provides workspace/recovery; consumes storage/processing | Client/Data; owner; L1/L2 | integrity/capacity/recovery | Client/API; TBD | TBD; Local/Offline; state tests; DESIGNED |
| CART-CMP-010 | Local Job Coordinator | Application / Workspace | Authoritative local Job and IPC | Desktop + Local Worker / local | local_job/checkpoint refs | Provides local job; consumes IPC/engine | Local Processing; job scope; L2 | heartbeat/version/crash | Desktop cohort; IPC TBD | TBD; Local Worker; IPC/state tests; POC_REQUIRED |
| CART-CMP-011 | Server Job Service | Application / Jobs owner TBD | Durable Job/admission/idempotency/lease | API + Server Worker / server | jobs schema/object refs | Provides Job REST/internal work; consumes SQL/storage/queue | API/Server; job permissions; L2 | queue age/lease/state | API/Worker; TBD | TBD; Queue/Worker; state tests; DESIGNED |
| CART-CMP-012 | Secured File Service | Application / Secured owner TBD | Explicit save/version/recycle/retrieve | API/Worker / server selective | secured schema + object refs | Provides secured REST; consumes storage/authz | API/Secure Storage; OWN/reauth; L2/L3 | versions/quota/digest | API/Worker; TBD | TBD; Storage/Sync; lifecycle tests; DESIGNED |
| CART-CMP-013 | Authorization Service | Building Block + AccountAdministration | Subject/permission/owner/scope/policy/reauth | All boundaries | Policy/session projections | Provides authz; consumes SSO/config/resource facts | Every zone; deny-default; L3 for sensitive | denial/revision/freshness | Shared/API; TBD | TBD; SSO/Cross-user; negative tests; DESIGNED |
| CART-CMP-014 | Audit Service | Application / Audit | Ingest/append/query/export evidence | Client outbox/API/Worker | audit schema | Provides Audit contracts; consumes safe intents/jobs | Audit/Data; scoped permission; mandatory | backlog/dedupe/export | API/Worker; TBD | TBD; Audit Failure; Audit tests; DESIGNED |
| CART-CMP-015 | Issue Reporting Service | Application / Support | Submit/attachment/triage/status/export | Clients/API/Web/Worker | support schema + attachments | Provides Issue/Support REST; consumes storage/audit | Support zone; reporter/support scope; L2/L3 | outbox/status/attachment | API/Web/Worker; TBD | TBD; Issue; Support tests; DESIGNED |
| CART-CMP-016 | Assistant Orchestrator | Application / Assistant | Intent→canonical Feature orchestration | Desktop/Mobile/Web/API / curated | Minimal workflow/conversation metadata | Provides Assistant plan; consumes Registry/Authz/Application services | Client/API; underlying permission; L2/L3 by Feature | plan/denial/correlation | Client/API; TBD | TBD; AI/API; Assistant tests; DESIGNED |
| CART-CMP-017 | Content Intelligence Resolver | Application / ContentIntelligence | Summarize/translate via approved provider | Local/Server Worker / hybrid | Safe operation/provenance metadata | Provides intelligence; consumes extraction/OCR/provider/authz | Processing/Provider; intelligence permission; L2 | provider circuit/fidelity | Workers; TBD | TBD; AI Provider; PoC/security tests; POC_REQUIRED |
| CART-CMP-018 | Result Validation | Pattern / producing module | Format/security/fidelity validation + atomic promote | Client/Workers | Result manifest/digest | Provides validation primitives; consumes module rules/storage | Processing/Data; inherited permission; L2/L3 | false-success/reject rate | Producing artifact; TBD | Per-module owner; Data Integrity; oracle tests; DESIGNED |
| CART-CMP-019 | Progress/Activity Projection | Application / Workspace anchor TBD | Aggregate persisted state for UI | API/clients; SignalR hint | Derived projection/cache | Provides activity view; consumes Job/Issue resources/events | API/Client; resource scope; no progress Audit | gap/reconcile/freshness | API/client; TBD | TBD; SignalR; recovery tests; DESIGNED |

## Quality gates

- Tidak ada component tanpa fungsi.
- Tidak ada engine/provider sebagai Feature owner.
- Tidak ada client direct SQL.
- Tidak ada Local-only component yang membutuhkan server.
- Tidak ada shared mutable business state pada Building Blocks.
- Satu data/write owner harus dikonfirmasi sebelum implementasi.

