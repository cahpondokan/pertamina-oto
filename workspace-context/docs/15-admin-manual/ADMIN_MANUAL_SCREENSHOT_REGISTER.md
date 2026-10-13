# REGISTER SCREENSHOT ADMIN MANUAL
## PERTAMINA OTO — Office Tools Organizer

> **Versi:** 1.0  
> **Tanggal:** 1 September 2026  
> **Status:** PLANNED — ACTUAL SCREENSHOT 0  
> **Aplikasi/build:** `RC-TBD`

---

# 1. Aturan

- Placeholder bukan screenshot.
- Capture hanya dari exact approved RC pada QA/UAT terkendali.
- Gunakan account, Issue, Job, file metadata, dan Correlation ID dummy/sanitized.
- Jangan tampilkan password, token, secret, raw claim, pegawai nyata, document/OCR content, signature, clipboard, storage key/path, connection string, production topology, atau raw diagnostic.
- Catat build, platform, locale, environment, role/profile, permission fixture, tanggal, owner, caption, alt text, dan stale trigger.
- Status lifecycle: `PLANNED → CAPTURED → REVIEWED → SECURITY_REVIEWED → APPROVED`; screenshot lama menjadi `OUTDATED`.

---

# 2. Register

| ID | Placeholder | Area/state | Data aman minimum | Status |
|---|---|---|---|---|
| `ADM-SCR-001` | `[SCREENSHOT-ADMIN-DASHBOARD-001]` | Dashboard + environment/build | Aggregate dummy | `PLANNED` |
| `ADM-SCR-002` | `[SCREENSHOT-USER-MANAGEMENT-001]` | Account search scoped | Dummy principal | `PLANNED` |
| `ADM-SCR-003` | `[SCREENSHOT-ACCOUNT-SUMMARY-001]` | Eligibility/session/role summary | Masked synthetic ID | `PLANNED` |
| `ADM-SCR-004` | `[SCREENSHOT-ACCOUNT-DISABLE-001]` | Disable confirmation | Exact dummy target + reason code | `PLANNED` |
| `ADM-SCR-005` | `[SCREENSHOT-SESSION-REVOKE-001]` | Session revoke | Synthetic session ref | `PLANNED` |
| `ADM-SCR-006` | `[SCREENSHOT-ROLE-ASSIGN-001]` | Role assignment preview | Dummy user + scope | `PLANNED` |
| `ADM-SCR-007` | `[SCREENSHOT-PERMISSION-001]` | Permission assignment | Canonical code + synthetic scope | `PLANNED` |
| `ADM-SCR-008` | `[SCREENSHOT-PERMISSION-REVOKE-001]` | Permission revoke | Exact assignment + impact | `PLANNED` |
| `ADM-SCR-009` | `[SCREENSHOT-EFFECTIVE-ACCESS-001]` | Effective allow/deny explanation | Safe reason code | `PLANNED` |
| `ADM-SCR-010` | `[SCREENSHOT-REAUTH-001]` | Reauthentication notice | No credential visible | `PLANNED` |
| `ADM-SCR-011` | `[SCREENSHOT-FEATURE-CONFIG-001]` | Feature Registry list/detail | Feature ID + status | `PLANNED` |
| `ADM-SCR-012` | `[SCREENSHOT-FEATURE-IMPACT-001]` | Enable/disable impact | Dummy environment/scope | `PLANNED` |
| `ADM-SCR-013` | `[SCREENSHOT-FEATURE-FLAG-001]` | Flag scope/version/expiry | Synthetic audience | `PLANNED` |
| `ADM-SCR-014` | `[SCREENSHOT-PLATFORM-EXPOSURE-001]` | Deployed maximum vs effective | Four platform codes | `PLANNED` |
| `ADM-SCR-015` | `[SCREENSHOT-POLICY-DIFF-001]` | Typed old/new policy | Non-Production example | `PLANNED` |
| `ADM-SCR-016` | `[SCREENSHOT-LIMIT-QUOTA-001]` | Configured/effective/hard bound | Non-official dummy values | `PLANNED` |
| `ADM-SCR-017` | `[SCREENSHOT-CORE-ADMIN-001]` | One Core card administration | Feature ID + posture | `PLANNED` |
| `ADM-SCR-018` | `[SCREENSHOT-PROVIDER-STATUS-001]` | Safe provider/capability state | Approved profile code only | `PLANNED` |
| `ADM-SCR-019` | `[SCREENSHOT-JOB-QUEUE-001]` | Job/queue aggregate | Synthetic counts/IDs | `PLANNED` |
| `ADM-SCR-020` | `[SCREENSHOT-JOB-DETAIL-001]` | Safe Job detail | Feature/Job/Correlation/status | `PLANNED` |
| `ADM-SCR-021` | `[SCREENSHOT-JOB-ACTION-001]` | Cancel/retry impact | Capture only after contract release | `PLANNED` |
| `ADM-SCR-022` | `[SCREENSHOT-SECURED-METADATA-001]` | Aggregate secured metadata | Masked owner + version count | `PLANNED` |
| `ADM-SCR-023` | `[SCREENSHOT-SECURED-VERSIONS-001]` | Version metadata/max-three | Synthetic versions | `PLANNED` |
| `ADM-SCR-024` | `[SCREENSHOT-SUPPORT-CONSOLE-001]` | Scoped Issue list | Synthetic Issue data | `PLANNED` |
| `ADM-SCR-025` | `[SCREENSHOT-SUPPORT-DETAIL-001]` | Issue + safe diagnostics | Dummy message/reference | `PLANNED` |
| `ADM-SCR-026` | `[SCREENSHOT-SUPPORT-TRIAGE-001]` | Typed status/category/priority | Synthetic Issue | `PLANNED` |
| `ADM-SCR-027` | `[SCREENSHOT-SUPPORT-ATTACHMENT-001]` | Exact child attachment | Sanitized app screenshot | `PLANNED` |
| `ADM-SCR-028` | `[SCREENSHOT-SUPPORT-EXPORT-001]` | Bounded PDF/Excel export | Synthetic filter/scope | `PLANNED` |
| `ADM-SCR-029` | `[SCREENSHOT-AUDIT-SEARCH-001]` | Scoped Audit query | Synthetic events | `PLANNED` |
| `ADM-SCR-030` | `[SCREENSHOT-AUDIT-DETAIL-001]` | Read-only safe event | Opaque refs | `PLANNED` |
| `ADM-SCR-031` | `[SCREENSHOT-AUDIT-EXPORT-001]` | Bounded export confirmation | Synthetic date/scope | `PLANNED` |
| `ADM-SCR-032` | `[SCREENSHOT-SYSTEM-HEALTH-001]` | Health + freshness/status | Aggregate dummy telemetry | `PLANNED` |
| `ADM-SCR-033` | `[SCREENSHOT-ALERT-ACTION-001]` | Alert safe checks/actions | No topology/secret | `PLANNED` |
| `ADM-SCR-034` | `[SCREENSHOT-VERSION-COMPATIBILITY-001]` | Current/minimum/contract/capability | Synthetic versions | `PLANNED` |
| `ADM-SCR-035` | `[SCREENSHOT-CONFIG-HIERARCHY-001]` | Effective source/provenance | Safe typed values | `PLANNED` |
| `ADM-SCR-036` | `[SCREENSHOT-BACKUP-STATUS-001]` | Last backup/verification state | No location/key details | `PLANNED` |
| `ADM-SCR-037` | `[SCREENSHOT-ACCESS-DENIED-001]` | Safe denial/next action | Synthetic target | `PLANNED` |
| `ADM-SCR-038` | `[SCREENSHOT-ERROR-CODE-001]` | Safe code + Correlation ID | Synthetic identifiers | `PLANNED` |

---

# 3. Capture Gate

```text
[ ] exact RC/build/digest/environment available
[ ] UI label inventory final
[ ] permission fixtures approved
[ ] dummy dataset reviewed
[ ] capture owner assigned
[ ] privacy/security checklist passed
[ ] caption and alt text reviewed
[ ] screenshot tied to procedure and build
[ ] no real Production data
[ ] final approval recorded
```

```text
Register entries  : 38
Actual screenshot : 0
Approved          : 0
False screenshot  : 0
```
