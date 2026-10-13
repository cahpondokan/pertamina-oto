# Document 06 — Specialist Integration Audit

> **Tanggal:** 24 Agustus 2026  
> **Reports:** Agent A, B, C, dan D  
> **Baseline:** Documents 01–05 FINAL; 550 canonical features

## Agreed

- Hybrid API Surface direkomendasikan seluruh specialist dan telah disetujui user.
- Local-only processing tidak memakai server API.
- Generic feature execution wajib closed, typed, versioned, dan per-feature authorized.
- Resource/domain lifecycle tidak dipaksa ke generic feature endpoint.
- REST adalah source of truth; SignalR notification only; REST recovery wajib.
- Assistant memakai canonical contract yang sama dengan UI.
- Tidak ditemukan Feature ID mismatch pada 33 Core Tool mappings.
- Core mapping: 19 local-only baseline, 14 optional-server, dan 0 universally required-server.

## Resolved

- Status API-DEC-001 disahkan user sebagai LOCKED — C.
- Baseline feature count ditetapkan 550. Legacy count tidak dipakai pada Document 06 atau Master Summary.
- `fileRef` dan `uploadId` dipisahkan berdasarkan lifecycle dan contract layer.
- State `PAUSED` tidak ditambahkan. Waiting memakai state upstream yang sudah ada dan event `JobWaiting`.
- `201 Created` dibatasi untuk addressable resource creation dengan `Location`; immediate command memakai `200`.
- API-DEC-002 disahkan user sebagai Governed Hybrid OpenAPI.
- Typed/versioned C# contract menjadi sumber implementasi; generated OpenAPI menjadi versioned contract artifact.
- CI diff dan contract test menjadi mandatory breaking-change guard.
- API-DEC-003 sampai API-DEC-009 dikunci berdasarkan persetujuan user atas seluruh rekomendasi specialist.
- Operational values yang memang membutuhkan PoC, benchmark, capacity test, Security Design, Role Matrix, atau approved Pertamina Identity tetap configuration/dependent detail; contract strategy-nya sudah locked.

## Deferred implementation parameters — tidak membuka ulang keputusan API

- File-transfer thresholds, TTL, dan retention: typed policy + capacity/security test.
- Idempotency retention dan retry budget: typed policy + recovery test.
- Exact identity protocol/claim mapping: approved Pertamina Identity + Security Design.
- Exact physical SignalR hub route: API route inventory, tanpa mengubah event/recovery contract.
- Exact quota and concurrency numbers: typed policy + benchmark/load test.

## Result

```text
Fatal contradiction       : NONE after user confirmation
Duplicate endpoint         : NONE; overlapping proposals are one shared route family
Local-First violation      : NONE in locked API-DEC-001
Feature ID mismatch        : NONE for 33 Core mappings
Public AI endpoint         : PROHIBITED
PKI digital signature      : OUT OF SCOPE
Document 06 decisions      : 9 / 9
Decision contradiction     : NONE
Finalization readiness     : CROSS-DOCUMENT AUDIT REQUIRED
```
