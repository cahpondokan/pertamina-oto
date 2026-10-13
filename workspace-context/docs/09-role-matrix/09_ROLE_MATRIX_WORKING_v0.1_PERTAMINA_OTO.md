# 09 — ROLE MATRIX — WORKING INTEGRATION RECORD
## PERTAMINA OTO — Office Tools Organizer

> **Version:** 0.1  
> **Date:** 26 Agustus 2026  
> **Status:** SUPERSEDED BY FINAL v1.0  
> **Purpose:** Preserve specialist-integration and audit lineage  
> **Canonical feature baseline:** 550

---

# 1. Inputs Integrated

```text
AGENT_A_ROLE_PERMISSION_MODEL_REPORT.md          READ
AGENT_B_FEATURE_ACTION_MATRIX_REPORT.md          READ
AGENT_C_PRIVILEGED_ACCESS_REPORT.md              READ
AGENT_D_PERMISSION_ENFORCEMENT_TEST_REPORT.md    READ
```

Parent baseline:

```text
Documents 01–08 FINAL
08 Security decisions 20/20 AUTO-LOCKED
550 canonical features
33 Primary PDF Tool Cards
```

---

# 2. Integration Result

```text
AGREED                  18 findings
CONFLICT RESOLVED       14 findings
AUTO-LOCKED             18 decisions
USER APPROVAL REQUIRED   0
MATERIAL CONTRADICTION   0
```

Important resolutions:

- formal roles remain User, Administrator, and Auditor;
- Superadmin remains a Support permission profile, not a role;
- compact action permissions replace per-Feature permission sprawl;
- canonical names use `domain.resource.action`;
- Administrator/Auditor/Support never inherit document content;
- Support attachment means only the item explicitly sent with an authorized Issue;
- Audit read/export are separated and Auditor receives no update/delete;
- local processing does not imply server transfer;
- offline grants never include privileged server permissions;
- Assistant, SignalR, API, UI, and Worker share the same authorization boundaries;
- missing API/persistence contracts are controlled handoffs, not silent edits.

---

# 3. Working Coverage

| Area | Integrated outcome |
|---|---|
| Role model | Three formal roles, no hierarchy, multiple duties explicitly assigned |
| Permission model | Compact action catalog with stable three-segment names |
| Policy/resource/context | Independent gates; deny wins; typed scopes |
| User | Own resources and approved tools |
| Administrator | OTO control plane and safe operational metadata |
| Auditor | Scoped read-only evidence; separate bounded export |
| Support | Granular Issue profile, no unrelated content |
| Service identities | Non-human, audience/purpose/job scoped |
| Feature mapping | 550/550 through families and sensitive exceptions |
| Core tools | 33/33 mapped |
| Local/offline | Protected local-only absolute-expiry grant |
| Sensitive actions | Confirmation, online, reauth, SoD, and Audit flags |
| Enforcement | UI UX only; API/application final; SignalR notify; Worker bounded |
| Assistant | No elevation; every step authorized identically |
| Matrices | Role, resource, platform, sensitive action |
| Tests | Cross-user, stale/revoked, direct API, Assistant, Hub, Worker negatives |
| Traceability | Requirement, Feature, API, Database, and Security |

All 48 requested Role Matrix areas are covered.

---

# 4. Superseding Artifacts

The authoritative baseline is:

```text
docs/09-role-matrix/09_ROLE_MATRIX_FINAL_v1.0_PERTAMINA_OTO.md
```

Decision and audit evidence:

```text
docs/09-role-matrix/ROLE_PERMISSION_DECISION_REGISTER.md
docs/09-role-matrix/ROLE_MATRIX_INTEGRATION_AUDIT.md
```

This working record is retained for lineage only and must not be used instead of FINAL v1.0.
