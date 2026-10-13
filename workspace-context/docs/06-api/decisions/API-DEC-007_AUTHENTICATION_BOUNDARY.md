# API-DEC-007 — Pertamina Identity and Server Authorization Boundary

> **Status:** LOCKED  
> **Decision owner:** User  
> **Decision date:** 24 Agustus 2026

Client authenticates through approved Pertamina Identity. API validates identity token/session and then enforces deny-by-default permission, ownership, enterprise policy, and application invariants.

Role is not the final authorization decision. Offline session only permits narrow local capability within time-bounded policy. Server and sensitive operations require current online authorization. Worker/provider use separate scoped service identity; end-user token is not forwarded.

Exact identity protocol/claims follow approved Pertamina Identity and Security Design without reopening this boundary.

**Traceability:** `FR-AUTH-001..005`; `FR-ROLE-001..003`; `SR-SEC-001..023`; `IR-API-002`; Solution Design sections 19, 22.6, dan 26.
