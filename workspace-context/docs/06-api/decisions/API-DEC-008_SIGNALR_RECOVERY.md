# API-DEC-008 — SignalR Envelope and REST Recovery

> **Status:** LOCKED  
> **Decision owner:** User  
> **Decision date:** 24 Agustus 2026

Persisted REST state is authoritative. SignalR is versioned at-least-once notification after persistence.

Envelope minimum memuat event identity/type/version, stream identity, per-stream sequence, applicable resource/Feature ID, correlation ID, UTC timestamp, persisted status, safe progress, step/count, dan message code. Duplicate/stale events are ignored; gaps and unknown versions trigger REST reconciliation.

One authenticated Activity Hub serves user-scoped activity. Hub carries no business command or document content. Disconnect never stops jobs. No PAUSED state exists; `JobWaiting` reports `WAITING_CONNECTION` or `WAITING_RESOURCE`. Local jobs use typed local IPC.

**Traceability:** `IR-API-004..008`; progress/activity/recovery/monitoring Feature IDs; Solution Design sections 11–12, 21–24, dan 28.
