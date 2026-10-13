# API-DEC-006 — Hybrid List Query Standard

> **Status:** LOCKED  
> **Decision owner:** User  
> **Decision date:** 24 Agustus 2026

- Ordinary bounded metadata list memakai page; large/changing/sensitive list memakai opaque keyset cursor.
- Default umum 25 dan maximum 100; history/job default 25 dan maximum 50.
- Invalid/excessive value ditolak, tidak di-clamp diam-diam.
- Cursor integrity-protected, expiring, scope-bound, dan selalu di-authorize ulang.
- Filter, search, dan sort memakai allowlist serta stable unique tie-breaker.
- Authorization diterapkan sebelum pagination/count.
- Wide snapshot/report memakai permission-gated asynchronous export.

**Traceability:** `FR-HIST-001..003`; `FR-AUDIT-001..004`; `FR-SUP-001..007`; `NFR-PERF-007..013`; `SR-SEC-001..023`.
