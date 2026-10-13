# 07 — DATABASE DESIGN — WORKING INTEGRATION RECORD
## PERTAMINA OTO — Office Tools Organizer

> **Version:** 0.1  
> **Date:** 24 Agustus 2026  
> **Status:** SUPERSEDED BY FINAL v1.0  
> **Purpose:** Preserve the specialist-integration phase and audit lineage  
> **Canonical feature baseline:** 550

---

# 1. Inputs Integrated

```text
AGENT_A_SQL_SERVER_REPORT.md              READ
AGENT_B_LOCAL_SQLITE_SYNC_REPORT.md       READ
AGENT_C_DATABASE_PERFORMANCE_REPORT.md    READ
AGENT_D_DATA_GOVERNANCE_REPORT.md         READ
```

Parent baseline at integration time:

```text
Documents 01–06 FINAL
06 API decisions 9/9 LOCKED
550 canonical features
33 Primary PDF Tool Cards
```

---

# 2. Integration Result

```text
AGREED                 15 groups
CONFLICT RESOLVED      10 groups
AUTO-LOCKED            12 decisions
USER APPROVAL REQUIRED 0
MATERIAL CONTRADICTION 0
```

Resolved items include:

- Document 06 specialist baseline note superseded by official FINAL v1.0;
- schema naming normalized;
- local and server Personal History separated;
- upload ownership assigned to `jobs`;
- idempotency/outbox remain domain-owned;
- generic workspace sync removed from baseline;
- secured version 4 becomes atomic tombstone + policy cleanup;
- SQL Blob exception cannot bypass Hybrid Storage;
- visual signature remains protected local-only by default;
- RCSI remains evidence-gated, not an assumed environment setting.

---

# 3. Working Coverage

| Required area | Integrated outcome |
|---|---|
| Principles and ownership | One owner/module; no cross-module write |
| SQL Server logical design | 11 logical schemas and server entity catalog |
| SQLite local design | Per-profile/device metadata/state entity catalog |
| Keys/constraints/naming/time | Internal/public key split; explicit constraints and UTC rules |
| Concurrency/transactions | Row version, sequence, lease, short transaction |
| EF Core/migrations | Per-module DbContext and migration ownership |
| Index/query/search | Keyset, allowlist, query-to-index and over-indexing guards |
| Jobs/recovery | Job/attempt/checkpoint/upload/idempotency/outbox |
| History/Audit/Support | Separate data planes and lifecycles |
| Secured Files | SQL metadata + object payload + max three active versions |
| Account/Admin/Policy | Identity, permissions, configuration, typed preference |
| Assistant/Intelligence | Minimal operational metadata; no content by default |
| Workspace/autosave | Local operation log, snapshot, artifact manifest |
| Selective sync/conflict | Explicit secured transfer, audit outbox, REST cache reconciliation |
| Retention/cleanup/delete | Typed policy, tombstone, bounded resumable workers |
| Classification/encryption | Working controls with Document 08 governed handoff |
| Performance/backup/restore | Capacity gates, cross-store recovery, load tests |
| Traceability | Requirement, Feature, and API mappings |
| Acceptance | Architecture/security/performance/recovery checklist |

---

# 4. Superseding Artifacts

The complete approved baseline is:

```text
docs/07-database/07_DATABASE_DESIGN_FINAL_v1.0_PERTAMINA_OTO.md
```

Decision and audit evidence:

```text
docs/07-database/DATABASE_DECISION_REGISTER.md
docs/07-database/DATABASE_INTEGRATION_AUDIT.md
```

This working file is retained for lineage only and must not be used instead of FINAL v1.0.
