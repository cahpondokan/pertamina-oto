# REGISTER RUNBOOK INCIDENT
## PERTAMINA OTO — Office Tools Organizer

> **Versi:** 1.0  
> **Tanggal:** 1 September 2026  
> **Status:** FINAL REGISTER — PRE-IMPLEMENTATION  
> **Catatan:** Tidak memuat command Produksi

---

# 1. Aturan Runbook

Setiap runbook wajib memuat scope, environment/version, symptoms, safe checks, evidence, containment, workaround, recovery class, validation, escalation, forbidden actions, stop condition, related Requirement/Feature/Test, serta handoff Dokumen 17 bila ada perubahan Produksi.

Status readiness yang sah: `DRAFT`, `REQUIRES_POLICY_CONFIRMATION`, `REQUIRES_SECURITY_CONFIRMATION`, `REQUIRES_UI_VALIDATION`, `TABLETOP_VALIDATED`, dan `RELEASE_READY`.

# 2. Register

| ID | Skenario/komponen | Related Feature | Safe monitoring/evidence | Status |
|---|---|---|---|---|
| `RB-API-OUTAGE-001` | Web/API outage | Server-dependent canonical Features | readiness, error/latency, Correlation ID, contract/build | `REQUIRES_POLICY_CONFIRMATION` |
| `RB-LOCAL-WORKER-001` | Local Worker crash/stuck | `FEAT-PROG-001..009`; affected processing Feature | process/IPC/job/checkpoint/version | `REQUIRES_UI_VALIDATION` |
| `RB-QUEUE-OVERLOAD-001` | Queue overload/retry storm | Server/background Features | depth, age, drain, retry, resource trend | `REQUIRES_POLICY_CONFIRMATION` |
| `RB-STORAGE-FAIL-001` | SQL/object/local storage unavailable | `FEAT-BACKUP-001..013`; `FEAT-VER-001..006` | readiness, capacity, metadata/object/digest state | `REQUIRES_POLICY_CONFIRMATION` |
| `RB-SSO-FAIL-001` | Pertamina SSO/session failure | `FEAT-AUTH-001..012` | safe auth outcome, session/policy version, time | `REQUIRES_POLICY_CONFIRMATION` |
| `RB-CROSS-USER-001` | Cross-user/cross-scope exposure | All protected resources | safe AuthZ/Audit references; no content browsing | `REQUIRES_SECURITY_CONFIRMATION` |
| `RB-MALICIOUS-DOC-001` | Malicious document/parser event | PDF-consuming Feature families | opaque file ref, digest if approved, engine/SBOM/build | `REQUIRES_SECURITY_CONFIRMATION` |
| `RB-AUDIT-FAIL-001` | Audit pipeline failure | `FEAT-AUDIT-001..008` | outbox/backlog/schema/sequence/freshness | `REQUIRES_POLICY_CONFIRMATION` |
| `RB-AI-PROVIDER-001` | Approved AI provider failure | `FEAT-INTEL-001..002` | provider profile, circuit, timeout, policy outcome | `REQUIRES_POLICY_CONFIRMATION` |
| `RB-REDACTION-001` | Redaction failure/exposure | `FEAT-PDF-069` | output/version/distribution refs; no redacted content | `REQUIRES_SECURITY_CONFIRMATION` |
| `RB-SIGNALR-001` | SignalR disconnect/event gap | `FEAT-PROG-001..009` | connection/revision/current REST state | `REQUIRES_UI_VALIDATION` |
| `RB-ISSUE-001` | Issue submission/attachment/outbox failure | `FEAT-SUPPORT-001..008` | Issue/error/correlation/state/outbox refs | `REQUIRES_UI_VALIDATION` |
| `RB-DATA-INTEGRITY-001` | corruption/false success/duplicate effect | Affected canonical Feature | state/version/object/digest/Job/Audit reconciliation | `REQUIRES_SECURITY_CONFIRMATION` |
| `RB-BAD-RELEASE-001` | bad release/rollback need | Affected release Feature set | artifact/config/schema/change references | `REQUIRES_POLICY_CONFIRMATION` |

# 3. Tabletop Review Minimum

| Skenario wajib | Design review | Operational exercise | Gap utama |
|---|---|---|---|
| Server/API outage | PASS | BELUM | route/contact/target resmi |
| Local Worker crash | PASS | BELUM | actual RC, UI, checkpoint evidence |
| Queue overload | PASS | BELUM | threshold/capacity policy |
| Storage failure | PASS | BELUM | restore authority, RPO/RTO |
| SSO failure | PASS | BELUM | SSO route dan session policy |
| Cross-user data exposure | PASS | BELUM | Security/Privacy process |
| Malicious document | PASS | BELUM | quarantine/evidence procedure |
| Audit pipeline failure | PASS | BELUM | degraded-mode policy |
| AI provider failure | PASS | BELUM | provider escalation/contract |
| Redaction failure | PASS | BELUM | notification/exposure process |

`PASS` hanya berarti alur keputusan, forbidden action, evidence boundary, dan handoff telah direview di tingkat desain. Tidak berarti `TABLETOP_VALIDATED` atau `RELEASE_READY`.

# 4. Release Gate

Runbook dapat menjadi `RELEASE_READY` hanya setelah exact RC/version, UI/error catalog, policy, owner/contact, permission/API, monitoring, UAT, security review, controlled drill, dan publication approval tersedia.
