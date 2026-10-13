# API-DEC-004 — Hybrid File Transfer

> **Status:** LOCKED  
> **Decision owner:** User  
> **Decision date:** 24 Agustus 2026

- Local-only processing tidak mengunggah file.
- File kecil/menengah memakai streamed multipart one-shot.
- File besar/tidak stabil memakai resumable sequential upload session dengan range, digest, committed offset, expiry, dan abort.
- Whole-file RAM buffering dilarang.
- Hanya validated READY upload boleh menjadi execution input.
- Download memakai authorized streaming, manifest, ETag, dan Range.
- Temporary result tidak menjadi secured file tanpa aksi eksplisit **Amankan ke Akun**.
- Exact limit dan TTL mengikuti typed policy serta capacity/security evidence.

**Traceability:** `FR-FILE-010..015`; `FR-PROC-001..005`; `FR-STOR-001..006`; `FR-BACKUP-001..010`; `IR-API-003..008`; Solution Design sections 9, 11–13, 17, 24, 26–28.
