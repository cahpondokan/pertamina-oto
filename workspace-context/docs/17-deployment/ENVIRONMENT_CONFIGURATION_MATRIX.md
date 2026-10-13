# MATRIKS ENVIRONMENT DAN KONFIGURASI
## PERTAMINA OTO — Office Tools Organizer

> **Versi:** 1.0  
> **Tanggal:** 1 September 2026  
> **Status:** FINAL REGISTER — PRE-IMPLEMENTATION

# 1. Environment Matrix

| Area | Local Development | Shared Development | QA/Testing | Pilot | Production |
|---|---|---|---|---|---|
| Tujuan | feedback/unit/component | shared integration | system/security/recovery/performance | operational validation terbatas | operasi resmi |
| Pengguna | developer/test identity | authorized team | tester/reviewer/UAT prep | approved participants | official users |
| Data | synthetic | synthetic | synthetic/sanitized-approved | approved Pilot data | governed operational data |
| Access | local/restricted | restricted | isolated test access | participant allowlist | Pertamina-approved route |
| Source deploy | local/task build untuk feedback | pipeline artifact | same pipeline artifact | exact RC artifact | approved same RC artifact |
| Config | developer-safe | typed Dev | typed QA | frozen/versioned Pilot | controlled Production |
| Secret | approved developer store | non-Prod source | separate QA source | separate Pilot source | approved mechanism TBD |
| SQL/storage | isolated local/Dev | Dev-only | QA-only | Pilot-only | topology TBD |
| Logging | safe local | Dev stream | QA evidence stream | Pilot operational stream | approved production sink |
| Monitoring | local/basic | component health | full test observability | alert rehearsal | active before rollout |
| Backup | fixture/recreate | change recovery point | restore rehearsal | verified set/test | policy TBD |
| Allowed test | Unit/component | integration/smoke | E2E/security/recovery/bounded load | business/canary/drill | bounded health/smoke only |
| Dilarang | Production data/secret | unbounded load | outside-window/destructive root | fuzz/pen-test/stress | attack/load/debug/manual patch |
| Approval | task/team | env owner | QA/security owner | Pilot authorities | official Pertamina approval |
| Readiness | DRAFT/QA local | QA_VALIDATED candidate | QA_VALIDATED | PILOT_VALIDATED | confirmation required |

# 2. Configuration Matrix

| Kelas | Contoh | Source | Secret? | Versioned | Validation/rollback |
|---|---|---|---:|---:|---|
| Default | safe application defaults | artifact template | No | Yes | schema + build test |
| Environment | environment code, approved endpoints | controlled deployment config | No | Yes | preflight + previous version |
| Enterprise Policy | limits, retention class, minimum client | governed control plane | No | Yes | authorization/reauth/Audit |
| Feature Flag | exposure/ring/provider enablement | governed typed config | No | Yes | owner/scope/expiry/disable |
| Runtime Capability | engine/resource/connection state | runtime resolver | No | observed | health/compatibility refresh |
| Secret Reference | DB/storage/provider credential reference | approved secret mechanism | Reference only | controlled | access/rotation/revocation test |

Configuration tidak boleh mengubah business meaning, Feature ID, authorization, Audit, Local-First, public-AI block, max-three, atau non-PKI scope.

# 3. Logical Separation pada Satu Node

| Deployable | Process | Identity | Config | Data/Folder | Health | Resource budget |
|---|---|---|---|---|---|---|
| Web | separate | separate | separate | read-only artifact/cache | own liveness/readiness | bounded |
| API | separate | separate | separate | no shared writable root | own liveness/readiness | bounded |
| Worker | separate | separate | separate | job-scoped temp/work | own heartbeat/readiness | strict concurrency |
| SQL Server | DB service | DB identities | DB config | dedicated data/log | internal health | protected headroom |
| Storage | private boundary | scoped identity | zone config | purpose-separated zones | availability/capacity | quota/lifecycle |
| Monitoring | collector/sink | monitoring identity | safe exporter | separate safe store | freshness | bounded |

# 4. Hostinger KVM 2 Boundary

Hostinger KVM 2 hanya `SINGLE_NODE_TEMPORARY` untuk Development, QA terbatas, atau Pilot terbatas setelah approval. Actual resource plan diverifikasi saat provisioning. Tidak boleh menjadi Production baseline, single backup location, atau dependency arsitektur permanen.

# 5. Confirmation Status

Exact hosting, OS, VM/container/PaaS, DNS/TLS, firewall, SSO, secret store, object storage, capacity, backup/DR, monitoring retention, dan Production topology tetap memerlukan tag konfirmasi resmi.
