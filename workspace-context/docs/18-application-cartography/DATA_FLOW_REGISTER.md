# DATA FLOW REGISTER
## PERTAMINA OTO — Document 18

> Versi: FINAL v1.0
> Status: PRE-IMPLEMENTATION DESIGN REGISTER
> Data classification: working label; official value requires Pertamina confirmation

| DataFlowId | Name / Trigger | Source → Destination / Protocol | Purpose / Data / Sensitive | Persistence / Retention / SOT | Permission / Validation / Audit | Failure / Retry / Owner |
|---|---|---|---|---|---|---|
| CART-FLOW-001 | Local PDF / user confirms | Desktop→Pdf service→local engine→Workspace; in-process | Process local PDF; restricted content | Local artifacts+SQLite; policy retention; local state SOT | Feature permission; input/output validation; L2/L3 by action | Typed fail; safe retry; Workspace/Pdf owner TBD |
| CART-FLOW-002 | Local Worker / heavy local job | Desktop→Named Pipe→Local Worker→engine | Isolation; job refs/content local; restricted | local_job/checkpoint/manifest SOT | Same Feature permission; IPC/job binding; L2 | heartbeat/cancel/resume; owner TBD |
| CART-FLOW-003 | Optional server / resolver allows | Client→preflight/upload→execute API | Approved server processing; restricted | Upload/Job SQL+temp/result; Job SOT | process+server+Feature, consent/policy; L2 | same key; no silent server; Jobs owner TBD |
| CART-FLOW-004 | Server Job / durable acceptance | API→Job/queue→Worker→provider→result | Background work; restricted | Job/attempt/checkpoint+object; policy expiry | workload+resource permission; validate result; accepted/terminal Audit | lease/retry/reconcile; Jobs owner TBD |
| CART-FLOW-005 | Amankan ke Akun / explicit | Client/result→API→secured metadata+storage | Selective save; restricted | secured aggregate/object; max three accessible versions | secured permissions+OWN/quota/reauth; lifecycle Audit | idempotent reconcile; Secured owner TBD |
| CART-FLOW-006 | Cross-device retrieval / user chooses | Device→secured REST→local copy | Retrieve owned version; restricted | Server secured SOT until local copy | ownership each request; digest/ETag; download Audit by policy | Range resume/expiry; Secured owner TBD |
| CART-FLOW-007 | Issue report / explicit submit | Client→Issue API→Support store/attachment | Support intake; message + optional screenshot | support aggregate/private object; policy retention | reporter scope; safe context/preview; Audit action | preserve message if attachment fails; Support owner TBD |
| CART-FLOW-008 | Audit / registered action | Owner outbox→Audit ingestion→Audit Store | Enterprise evidence; safe metadata | append-oriented Audit SOT; policy retention/hold | workload ingest; schema allowlist; mandatory action evidence | stable EventId/batch replay; Audit owner TBD |
| CART-FLOW-009 | Personal History / terminal action | Owner event→History projection→UI/API | User recall/reprocess; personal metadata | local/server History projection SOT | OWN; safe refs; history event policy | dedupe/retry; no rollback business action; owner TBD |
| CART-FLOW-010 | Telemetry / runtime signal | Hosts/adapters→OTel/APM | Health/performance; safe metadata only | telemetry backend, not business SOT | workload emit; closed fields; no Audit content | bounded buffer/drop; Observability owner TBD |
| CART-FLOW-011 | Assistant / user intent | Assistant→Registry→Authz→Application service | Canonical orchestration; minimized intent | actual Feature stores remain SOT | underlying permission/policy/confirm; Origin Assistant | clarify/deny; underlying retry; Assistant owner TBD |
| CART-FLOW-012 | AI Summarizer | ContentIntelligence→approved local/internal provider | Summarize selected content; restricted | local result default or Job result; no payload SQL/log | intelligence permission+consent/minimization; safe Audit | no public fallback; timeout/circuit; owner TBD |
| CART-FLOW-013 | Translate PDF | Extraction/OCR→ContentIntelligence provider | Translate selected text/layout; restricted | local/server result according to resolver | intelligence+server if used; validation; safe Audit | typed partial/unsupported; no public fallback |
| CART-FLOW-014 | PDF to Markdown | Conversion→extraction→OCR fallback | Deterministic extraction first; restricted | local/approved server artifact SOT | process+server if used; structure validation; L2 | typed failure; preserve source; Conversion owner TBD |
| CART-FLOW-015 | Visual Sign PDF | Protected local asset→SignatureAnnotation→Pdf | Apply non-PKI visual signature; sensitive | protected local asset + local result | signature permission+confirm; placement/output validation; L3 | clear transient copy; owner TBD |
| CART-FLOW-016 | Redact PDF / finalization | Workspace→Pdf redaction→forensic validator | Irreversible removal; restricted | working state + validated new result | redact permission+confirm/reauth; forensic validation; L3 | block false success; retry from working state |
| CART-FLOW-017 | Admin config change | Admin UI→existing typed API→config owner | Policy/flag/limit update; internal control metadata | versioned SQL config SOT | update permission+ADMIN_SCOPE+If-Match/reauth; Audit | 412/staged rollback; Config owner TBD |
| CART-FLOW-018 | Audit export | Auditor→Audit API→Job/Worker→restricted result | Bounded evidence export; controlled metadata | Audit SOT + derived expiring artifact | export create/read+AUDIT_SCOPE; filter/authz; Audit | same key; expiry; retrieval binding gap |
| CART-FLOW-019 | Incident diagnosis | Support/Ops→Issue/Telemetry/Audit/Job refs | Diagnose via safe evidence; sensitivity varies | Each plane remains its own SOT | separate permissions; no direct DB/storage; access Audit | unknown remains unknown; runbook handoff; owner TBD |

## Global prohibited payload

Password, token, secret/key, full document/OCR text, visual signature bytes, clipboard content, dan AI prompt/document/provider payload tidak boleh masuk SignalR, ordinary Audit, telemetry, History, atau automatic Issue diagnostics.

