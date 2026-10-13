# REGISTER RUNBOOK DEPLOYMENT
## PERTAMINA OTO — Office Tools Organizer

> **Versi:** 1.0  
> **Tanggal:** 1 September 2026  
> **Status:** FINAL REGISTER — PRE-IMPLEMENTATION  
> **Larangan:** Tidak memuat command, credential, endpoint, atau topology Produksi

# 1. Aturan

Setiap runbook wajib memuat environment, exact artifact/config/version, authority/approval, prerequisites, data/security/capacity impact, backup, stop condition, rollback, smoke, evidence, monitoring, Incident handoff, dan acceptance. Status: `DRAFT`, `REQUIRES_INFRA_CONFIRMATION`, `REQUIRES_SECURITY_CONFIRMATION`, `REQUIRES_SSO_CONFIRMATION`, `REQUIRES_DISTRIBUTION_CONFIRMATION`, `QA_VALIDATED`, `PILOT_VALIDATED`, `PRODUCTION_READY`.

# 2. Register

| ID | Runbook | Scope | Status |
|---|---|---|---|
| `DRB-QA-DEPLOY-001` | Normal QA deployment | same artifact + QA config + smoke | `REQUIRES_INFRA_CONFIRMATION` |
| `DRB-API-FAIL-001` | Failed API deployment | hold traffic/admission, validate config/dependency, rollback/forward-fix | `REQUIRES_INFRA_CONFIRMATION` |
| `DRB-WORKER-FAIL-001` | Failed Worker deployment | stop claims, drain/lease/checkpoint, compatible artifact | `REQUIRES_INFRA_CONFIRMATION` |
| `DRB-DB-MIG-FAIL-001` | Failed SQL migration | stop, preserve evidence, exact-state forward-fix/restore decision | `REQUIRES_INFRA_CONFIRMATION` |
| `DRB-CLIENT-INCOMPAT-001` | Incompatible client | compatibility status, preserve local work, approved update/path | `REQUIRES_DISTRIBUTION_CONFIRMATION` |
| `DRB-STORAGE-FAIL-001` | Storage unavailable | stop false final, preserve staging/intent, reconcile | `REQUIRES_INFRA_CONFIRMATION` |
| `DRB-SECRET-MISSING-001` | Secret/reference missing | fail safely; restore approved reference; no hardcode | `REQUIRES_SECURITY_CONFIRMATION` |
| `DRB-CONFIG-BAD-001` | Bad configuration | validate/revert typed version; no manual Production edit | `REQUIRES_SECURITY_CONFIRMATION` |
| `DRB-QUEUE-HIGH-001` | High queue after release | admission/backpressure, isolate workload/provider, observe drain | `REQUIRES_INFRA_CONFIRMATION` |
| `DRB-ROLLBACK-001` | Emergency application rollback | retained compatible artifact/config + approval + smoke | `REQUIRES_INFRA_CONFIRMATION` |
| `DRB-RESTORE-001` | Restore from backup | isolated restore + full SQL/storage/key/app/AuthZ/Audit validation | `REQUIRES_INFRA_CONFIRMATION` |
| `DRB-HOSTINGER-EXIT-001` | Hostinger Pilot migration | build-once promotion + protected data migration + validation | `REQUIRES_INFRA_CONFIRMATION` |
| `DRB-SQLITE-001` | Client SQLite migration failure | preserve profile/workspace; journal/recover; no silent reset | `REQUIRES_DISTRIBUTION_CONFIRMATION` |
| `DRB-HOTFIX-001` | Hotfix/emergency release | Incident → branch → tests/scans/review → artifact → approval | `REQUIRES_SECURITY_CONFIRMATION` |
| `DRB-DESKTOP-UPDATE-001` | Desktop + Local Worker update | signed compatibility cohort + state preservation | `REQUIRES_DISTRIBUTION_CONFIRMATION` |
| `DRB-MOBILE-UPDATE-001` | Mobile staged update | approved signing/channel/ring + compatibility | `REQUIRES_DISTRIBUTION_CONFIRMATION` |

# 3. Tabletop Design Review

| Skenario wajib | Design review | Exercise nyata | Gap |
|---|---|---|---|
| Normal QA deployment | PASS | BELUM | exact environment/pipeline |
| Failed API deployment | PASS | BELUM | topology/traffic switch |
| Failed Worker deployment | PASS | BELUM | drain/lease targets |
| Failed DB migration | PASS | BELUM | recovery point/authority |
| Incompatible client | PASS | BELUM | distribution/minimum version |
| Storage unavailable | PASS | BELUM | product/topology/restore |
| Secret missing | PASS | BELUM | secret-management product |
| Bad configuration | PASS | BELUM | config store/approval |
| High queue after release | PASS | BELUM | thresholds/capacity |
| Emergency rollback | PASS | BELUM | release authority/window |
| Restore from backup | PASS | BELUM | RPO/RTO/backup topology |
| Hostinger Pilot migration | PASS | BELUM | target infrastructure |

`PASS` hanya design tabletop review. Tidak berarti `QA_VALIDATED`, `PILOT_VALIDATED`, atau `PRODUCTION_READY`.
