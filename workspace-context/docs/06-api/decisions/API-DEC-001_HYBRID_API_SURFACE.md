# API-DEC-001 — Hybrid API Surface

> **Status:** LOCKED  
> **Decision owner:** User  
> **Decision date:** 24 Agustus 2026  
> **Selected option:** C — Hybrid API Surface

## Decision

Server processing menggunakan canonical route:

```http
POST /api/{major}/features/{featureId}/execute
```

Resource/domain API tetap menggunakan route khusus untuk account, jobs, history, issues, secured files, audit, administration, configuration, compatibility, dan health.

Local-only tools tidak mempunyai server-processing API. UI card, workflow, dan Assistant tidak membuat endpoint atau processing engine duplikat.

## Guard

- Closed typed handler registry per canonical Feature ID.
- Typed/versioned parameter dan result schema.
- Per-feature authorization, policy, capability, capacity, dan consent checks.
- No arbitrary RPC, engine name, SQL, path, URL, or command from client.
- REST remains source of truth.
- SignalR remains notification only.

## Technical derivations

- `uploadId` adalah transfer resource identity.
- `fileRef` adalah typed execution reference; upload input membawa `kind=UPLOAD` dan READY `uploadId` sebagai ID.
- Tidak ada state `PAUSED`; waiting memakai `WAITING_CONNECTION` atau `WAITING_RESOURCE` dan event `JobWaiting`.

## Traceability

`IR-API-001..012`; `FR-PROC-001..005`; `FR-ASSIST-003..008`; `NFR-ARCH-002..004`; `FEAT-PROC-002..005`; `FEAT-ASSIST-009`; Feature Catalog v1.3; Solution Design sections 14, 24, dan 25.
