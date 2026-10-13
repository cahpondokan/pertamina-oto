# ENVIRONMENT DEPLOYMENT MATRIX
## PERTAMINA OTO — Document 18

> Versi: FINAL v1.0
> Status: LOGICAL PRE-IMPLEMENTATION MATRIX

Legend: L local/client; R required; O optional; A approved conditional; P Production logical placeholder; — tidak normal.

| Deployable/dependency | Local Dev | Shared Dev | QA/Test | Pilot | Production |
|---|:---:|:---:|:---:|:---:|:---:|
| Desktop | L | O | R | A | P setelah distribution approval |
| Mobile/Tablet | O | O | R untuk released scope | A | P setelah distribution approval |
| Web | O | R | R | R | P |
| API | O integration | R | R | R | P |
| Server Worker | O integration | R bila server Feature | R | R | P |
| Local Worker | L dengan Desktop | L | R untuk IPC/Desktop | A | P dengan Desktop cohort |
| SQL Server | O isolated | R Dev-only | R QA-only | R Pilot-only | P internal |
| SQLite | L/profile | L/client | L/test profile | L/participant | P per client/profile |
| Local Workspace | L | L | L | L | P user device |
| Secure Storage | O isolated | R Dev zones | R QA zones | R Pilot zones | P private |
| Monitoring | L basic | R health | R full test | R alert rehearsal | P sebelum rollout |
| Approved Provider | A non-Prod | A Dev profile | A QA profile | A Pilot profile | P setelah approval |
| Azure DevOps Artifact | O feedback | R | R exact artifact | R exact RC | P approved RC |
| Backup/Recovery location | O test | R change recovery | R restore rehearsal | R verified | P separate failure domain |

## Environment control fields

| Field | Rule |
|---|---|
| Deployed? | Sesuai matrix dan approved release scope |
| Logical/Physical | Logical selalu terpisah; physical placement environment-specific |
| Artifact | Build once, immutable, digest + Release Manifest |
| Configuration | Typed/versioned/environment-bound |
| Secret Source | Approved secret/key boundary; tidak embedded |
| Data Type | Synthetic-first non-Prod; Production data hanya approved scope |
| Network Zone | REQUIRES_PERTAMINA_NETWORK_CONFIRMATION |
| Monitoring | Environment-isolated, safe fields, freshness/owner |
| Backup | Environment-isolated; Production separate failure domain |
| Scaling | Berdasarkan measurement/approval; tidak ada angka final di sini |
| Owner | REQUIRES_PERTAMINA_OWNER_CONFIRMATION |
| Status | Pre-implementation; Production physical = TBD |

## Hostinger KVM 2

- hanya temporary Shared Development, limited QA, atau approved limited Pilot;
- Web/API/Worker/SQL/storage/monitoring tetap logical separate;
- satu node berarti satu failure/security/resource domain;
- backup di active node saja tidak diterima;
- dilarang menjadi Production baseline.

## Production

Seluruh cell P dipetakan ke PERTAMINA_APPROVED_PRODUCTION_INFRASTRUCTURE. Hosting, OS, process/container, DNS/IP/port/firewall, ingress, HA, capacity, storage, monitoring, backup/DR, authority, dan exact topology masih confirmation gap.

