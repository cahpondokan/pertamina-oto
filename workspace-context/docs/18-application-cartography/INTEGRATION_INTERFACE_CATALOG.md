# INTEGRATION INTERFACE CATALOG
## PERTAMINA OTO — Document 18

> Versi: FINAL v1.0
> Baseline authority: Dokumen 06 FINAL untuk REST/SignalR
> Status: SOURCE_VALIDATED WITH PRE-IMPLEMENTATION GAPS

Kolom Auth mencakup authentication/authorization. Timeout, RateLimit, dan owner numerik/resmi tetap POLICY_CONFIGURABLE atau TBD.

| InterfaceId | Name / Producer → Consumer | Purpose / Protocol / Direction | Endpoint/Channel / Sync | Auth / Sensitive data | Contract/schema / Input→Output | Retry / Timeout / Idempotency / RateLimit | Error / SOT / Monitoring | Owner / Status |
|---|---|---|---|---|---|---|---|---|
| CART-INT-001 | Client → API | Resource/command; HTTPS REST; outbound | /api/v1; sync/202 async | Pertamina auth + permission/owner/policy; content conditional | API v1 + typed schemas; request/ref→resource/result/Job | GET bounded; mutation same key; timeout/rate policy | ProblemDetails; persisted owner store; route/status/duration | API/module owner TBD; DESIGNED |
| CART-INT-002 | Client → Feature Execute | Canonical server processing; REST | POST /api/v1/features/{featureId}/execute; sync/async | process+server+Feature permissions; restricted bytes via refs | ContractVersion + ParameterSchemaVersion; FeatureExecution→result/Job | same Idempotency-Key/fingerprint; bounded admission | typed error; Job/result SOT; Feature/Job correlation | Feature+Jobs owner TBD; DESIGNED |
| CART-INT-003 | Client → File Transfer | Explicit server transfer; streaming REST | preflight/uploads/upload-sessions; sync | underlying action + purpose/consent; restricted content | API v1; metadata/chunk/digest→session/READY | uploadId+range+digest; session expiry; byte quota | typed transfer error; upload SQL+temp object; bytes/digest safe | Upload/Jobs owner TBD; DESIGNED |
| CART-INT-004 | Client → Job/Result | Query/control/download; REST | /api/v1/jobs/...; sync/202/stream | job/result permissions + OWN/scope | API v1; Job action/query→state/manifest/content | GET retry; action same key; Range/ETag | persisted Job is SOT; state/duration/expiry | Jobs owner TBD; DESIGNED |
| CART-INT-005 | Client → Resource APIs | Account/history/secured/Issue/Audit/admin | Existing Dokumen 06 families; sync/async | resource permission/owner/scope/reauth; sensitive varies | API v1 typed resource schemas | exact Idempotency/ETag/pagination per D06 | owning aggregate SOT; safe resource metrics | Respective module owner TBD; DESIGNED |
| CART-INT-006 | Post-commit publisher → Client | Realtime notification; SignalR; outbound | /hubs/activity; async | authenticated user groups/job scope; no content | EventVersion envelope; event→hint | reconnect/dedupe/sequence; no command retry | gap→REST reconcile; persisted resource SOT | Realtime/API owner TBD; DESIGNED |
| CART-INT-007 | Desktop → Local Worker | Isolated local execution; Named Pipe; bidirectional | ILocalWorkerTransport; async | ACL/profile/instance/job binding; restricted local refs | IpcContractVersion TBD; job request→progress/result | heartbeat/timeout/cancel; request identity; local admission | typed IPC error; SQLite/manifest SOT; crash/resource metrics | Desktop/Processing owner TBD; POC_REQUIRED |
| CART-INT-008 | API → Server Worker | Durable background dispatch; persisted Job/queue; async | Job Store/Queue abstraction; async | workload identity; no user token; job scope | Job/attempt/lease/checkpoint version TBD | claim/lease/retry/dedupe; bounded concurrency | persisted Job SOT; queue age/lease/crash | Jobs/Worker owner TBD; DESIGNED |
| CART-INT-009 | Module → Module | Application contract; in-process typed | Application interface; sync/async only where defined | verified actor/context; sensitivity contract-specific | Contract/schema version TBD | no blind retry; operation-specific idempotency | callee aggregate SOT; contract outcome | Callee module owner TBD; PARTIAL |
| CART-INT-010 | Owner module → Consumers | Post-commit internal event; outbox; async | Event catalog TBD | workload identity/allowlist; metadata only | EventVersion/type/payload TBD | consumer dedupe/retry/retention TBD | owner aggregate SOT; backlog/poison metrics | Event owner/consumer TBD; BLOCKED_BY_DECISION |
| CART-INT-011 | Application → Engine | Processing adapter; typed in-process/worker | Engine abstraction | job-scoped refs; restricted content only when needed | Engine/Capability/provider profile version TBD | allowlisted bounded retry/timeout/circuit | output untrusted; application Job SOT; provider metrics | Feature module owner TBD; POC_REQUIRED |
| CART-INT-012 | Application → Storage | Opaque streaming storage abstraction | Local/private object operations | authorized job/resource scope; restricted bytes | object manifest/digest/ETag version TBD | same operation/key; finalize/reconcile | metadata aggregate + exact object; capacity/digest | Data owner + storage owner TBD; PARTIAL |
| CART-INT-013 | Application → Audit | Safe evidence ingestion; outbox/REST batch | AuditIntent/batch/query/export | workload ingest or Auditor scope; no content | Audit EventVersion; safe intent→ack/event/export Job | EventId/batch key dedupe; bounded retry | audit.AuditEvents; backlog/reject/duplicate | Audit owner TBD; DESIGNED |
| CART-INT-014 | Runtime → Observability | Technical signal; OTel/APM; outbound | Approved collector/backend TBD; async | workload identity; safe allowlist only | Telemetry schema/config version TBD | bounded buffer/backoff/drop policy | Observability only, not business SOT; freshness/drop | Observability owner TBD; PARTIAL |
| CART-INT-015 | Application/API → Pertamina Identity | Authentication/session; approved protocol | Endpoint/profile TBD; sync | issuer/audience/signature/time; credentials sensitive | Claims/profile TBD; assertion→Principal/session | allowlisted transient retry only | Identity assertion SOT; auth outcome safe metrics | IAM owner TBD; PERTAMINA_CONFIRMATION_REQUIRED |
| CART-INT-016 | ContentIntelligence → Internal Provider | Purpose-scoped intelligence; typed provider | Approved internal route TBD; async/sync profile | workload identity, minimization, consent, no public AI | Provider profile/schema TBD; selected content→untrusted result | timeout/circuit/idempotency if supported | Job SOT; latency/error/profile safe metrics | Provider/Intelligence owner TBD; POC_REQUIRED |

## Gap tanpa route invention

| Gap | Status |
|---|---|
| Admin user/session/role/permission REST surface | Tidak ada pada Dokumen 06; controlled API decision diperlukan |
| Monitoring aggregate API | Tidak ada; minimal health tetap berlaku |
| Activity REST read model | Belum diputuskan; reconcile dari existing resources sementara |
| Audit/Support export retrieval parent binding | Belum lengkap; tidak boleh membuka cross-scope result |
| Local IPC wire/auth/compatibility | PoC + contract register diperlukan |
| Internal event names/owners/consumers | Catalog belum lengkap; tidak dibuat di Dokumen 18 |

