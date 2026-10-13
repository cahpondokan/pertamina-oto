# API-DEC-003 — URI Major Versioning

> **Status:** LOCKED  
> **Decision owner:** User  
> **Decision date:** 24 Agustus 2026

Functional API memakai `/api/v1/...`; major berikutnya memakai `/api/v2/...`. Bootstrap `GET /api/compatibility` tetap stabil tanpa major route.

Major naik hanya untuk breaking HTTP change. App, contract, parameter, workflow, configuration, dan capability version dikelola terpisah. Supported majors dapat berjalan berdampingan selama compatibility window.

**Traceability:** compatibility requirements; Solution Design sections 23–24; `API-DEC-002`.
