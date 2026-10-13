# API-DEC-009 — Layered Admission Control

> **Status:** LOCKED  
> **Decision owner:** User  
> **Decision date:** 24 Agustus 2026

Rate limit, quota, concurrency, and backpressure use the strictest effective bound from system safety, enterprise policy, caller scope, workload category, environment, worker, storage, and provider capacity.

Quota/reservation is checked before acceptance. Queue is bounded, fair, persistent, and workload-separated. `429` handles caller limits, `413` hard payload limits, `409` storage/resource conflicts, and `503` unsafe shared saturation. Retry guidance uses `Retry-After`, bounded backoff, and jitter.

Exact numeric limits are typed policy values approved after PoC, benchmark, load/security/license review, and safety headroom. Local safe capability remains available during server pressure; public AI fallback is prohibited.

**Traceability:** `NFR-PERF-007..013`; `SR-SEC-001..023`; `IR-API-003..009`; Solution Design sections 11.4, 26, dan 27.
