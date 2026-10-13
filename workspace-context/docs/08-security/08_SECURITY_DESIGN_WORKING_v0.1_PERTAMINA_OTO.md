# 08 — SECURITY DESIGN — WORKING INTEGRATION RECORD
## PERTAMINA OTO — Office Tools Organizer

> **Version:** 0.1  
> **Date:** 24 Agustus 2026  
> **Status:** SUPERSEDED BY FINAL v1.0  
> **Purpose:** Preserve specialist-integration and audit lineage  
> **Canonical feature baseline:** 550

---

# 1. Inputs Integrated

```text
AGENT_A_IDENTITY_ACCESS_SECURITY_REPORT.md       READ
AGENT_B_DATA_FILE_LOCAL_SECURITY_REPORT.md       READ
AGENT_C_APP_API_AI_THREAT_SECURITY_REPORT.md     READ
AGENT_D_SECURE_SDLC_OPERATIONS_REPORT.md         READ
```

Parent baseline at integration time:

```text
Documents 01–07 FINAL
06 API decisions       9/9 LOCKED
07 Database decisions 12/12 AUTO-LOCKED
550 canonical features
33 Primary PDF Tool Cards
```

---

# 2. Integration Result

```text
AGREED                  16 findings
CONFLICT RESOLVED       10 findings
AUTO-LOCKED             20 decisions
USER APPROVAL REQUIRED   0
MATERIAL CONTRADICTION   0
```

Important resolutions:

- no remote URL input is added to the current generic feature API;
- browser SignalR uses the protected same-site Web session, never a URL token;
- disabled local OTO access returns safe `403`; invalid authentication remains `401`;
- revocation stops control and result access, with safe-boundary job revalidation;
- logout locks the profile and clears credentials without silently deleting work;
- engineering data labels do not claim to be official Pertamina classifications;
- required encryption properties are locked without inventing a provider;
- workload identity and security-tool products remain governed confirmations;
- build-artifact integrity is separate from visual Sign PDF non-PKI.

---

# 3. Working Coverage

| Area | Integrated outcome |
|---|---|
| Identity/session/offline | Pertamina SSO, server authorization, secure Web/native sessions, bounded offline grant |
| Authorization | Deny-by-default permission + object scope + policy/state; explicit reauthentication gates |
| Privileged roles | Admin, Support, and Auditor have no implicit content access |
| API/SignalR | REST authority, authenticated notification only, no token in URL |
| Worker/IPC | Least privilege, typed/versioned IPC, isolation, limits, crash containment |
| Database/local data | API-only server boundary and protected per-profile local boundary |
| File lifecycle | Stream, quarantine, validate, isolate, validate output, controlled cleanup |
| Document threats | PDF/Office/image/OCR/HTML/URL risks have explicit controls and feasibility gates |
| Assistant/AI | Closed orchestrator, no public AI, prompt-injection and data-leakage defenses |
| Secrets/crypto | Approved stores, encryption/key requirements, rotation and fail-closed rules |
| Logs/audit/privacy | Allowlisted metadata, redaction, separated history/audit/support/telemetry planes |
| Abuse/resources | Layered admission control, bounded queues, reservations, backpressure |
| Secure SDLC | Coding, dependency, scan, SBOM, artifact, CI/CD, VA/PT, and release evidence gates |
| Operations | Environment separation, backup/restore, vulnerability and incident principles |
| Traceability | Requirement, Feature, API, and Database mappings |

All 59 requested security areas were covered.

---

# 4. Governed Confirmations

Unknown Pertamina-specific values and products remain marked `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` in FINAL v1.0. They do not authorize a weaker fallback or reopen the logical security decisions.

These include IAM details, official classification, Role Matrix, audit policy, approved cryptography/key/storage/AI/scanning platforms, hardening standards, vulnerability/VA-PT/incident rules, backup/DR values, and numeric limits.

---

# 5. Superseding Artifacts

The authoritative security baseline is:

```text
docs/08-security/08_SECURITY_DESIGN_FINAL_v1.0_PERTAMINA_OTO.md
```

Decision and audit evidence:

```text
docs/08-security/SECURITY_DECISION_REGISTER.md
docs/08-security/SECURITY_INTEGRATION_AUDIT.md
```

This working record is retained for lineage only and must not be used instead of FINAL v1.0.
