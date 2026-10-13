# API-DEC-005 — Persisted Idempotency + Request Fingerprint

> **Status:** LOCKED  
> **Decision owner:** User  
> **Decision date:** 24 Agustus 2026

Mutating and expensive command memakai scoped `Idempotency-Key`. Persisted record menyimpan request fingerprint, logical correlation, resource ID, safe response, status, dan expiry.

Key/fingerprint sama me-replay resource yang sama. Key sama dengan fingerprint berbeda menghasilkan `409 IDEMPOTENCY_KEY_REUSED`. Retry/resume mempertahankan logical Job ID dan menambah append-only attempt history. Retry bounded hanya untuk transient allowlist; resume hanya dari compatible verified checkpoint.

**Traceability:** `FR-REC-001..006`; `NFR-REC-001..005`; `IR-API-004..008`; recovery/progress Feature IDs; Solution Design sections 11–13, 21, 24, 27–28.
