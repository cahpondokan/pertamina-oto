# MASTER PROJECT SUMMARY
## PERTAMINA OTO — Office Tools Organizer

> **Version:** 2.2  
> **Snapshot:** 18 Agustus 2026  
> **Status:** Documents 01–05 FINAL / PRE-CODING CONTINUITY BASELINE

---

# Current Status

```text
01 BUSINESS PROCESS          FINAL v3.4
02 REQUIREMENT SPECIFICATION FINAL v1.5
03 FEATURE CATALOG           FINAL v1.3 — 550 canonical records
04 SOLUTION DESIGN           FINAL v1.2 — 18/18 decisions locked
05 TECHNICAL REFERENCE       FINAL v1.2 — 15/15 decisions locked

Completed documents          5 / 19
Overall document progress    26.3%
Next document                06_API_DOCUMENTATION.md
```

---

# Project Identity

- Product: **PERTAMINA OTO**
- Subtitle: **Office Tools Organizer**
- Formal: **PERTAMINA OTO — Office Tools Organizer**
- Repository: `pertamina-oto`
- Namespace: `Pertamina.Oto.*`
- Development context: **one human developer using multiple scoped Codex sessions/tasks**.

---

# Core Product Principles

- Desktop-First; Windows Desktop is the most complete productivity client.
- Mobile/Tablet is curated, not Desktop parity.
- Responsive Web is enterprise/browser channel.
- Offline-First behavior.
- Capability-aware Local-First processing.
- Server-efficient design.
- Non-destructive editing.
- No Silent Wait.
- Undo/Redo for reversible work.
- Selective sync through **Amankan ke Akun** only.
- Maximum 3 versions for secured account files.
- Feature completeness does not mean hundreds of primary buttons.
- **33 Primary PDF Tool Cards** form the Core Feature Set and are implemented before secondary user tools.
- Digital Signature certificate/PKI remains out of scope.
- Visual signature/paraf non-PKI remains in scope.
- Assistant orchestrates official canonical tools; it is not a processing engine.
- 8-layer Definition of Done remains mandatory.

---

# Architecture

**Modular Monolith + Clean Architecture + Separate Deployables**

Hosts:
- Web — ASP.NET Core MVC
- API — ASP.NET Core Web API
- Worker — C#/.NET Worker Service
- Desktop — WinUI 3
- Mobile — .NET MAUI curated companion

Data:
- SQL Server + EF Core for enterprise/server metadata
- Local Workspace filesystem for user working files
- SQLite for local app metadata/state
- Secure File/Object Storage for approved server-stored file payloads

Communication:
- REST for commands/data
- SignalR for realtime events
- REST refresh as fallback
- Named Pipe/local IPC for Windows Desktop ↔ isolated Local Worker

---

# Technical Reference Final — 15 Decisions

1. .NET 10 LTS baseline + controlled upgrade
2. Desktop UI = WinUI 3
3. Mobile/Tablet = .NET MAUI
4. Structured Monorepo + branch/worktree per Codex task
5. Central Package Management + explicit dependency approval
6. PDF = primary engine + specialized fallback
7. OCR = local-first primary + specialized/server fallback
8. Office conversion = hybrid provider
9. Image = deterministic primary engine + specialized ML provider
10. Local Worker IPC = Named Pipe/local IPC behind abstraction
11. Local metadata/state = SQLite
12. Logging = Microsoft.Extensions.Logging + OpenTelemetry + Application Insights
13. Testing = MSTest + Microsoft.Testing.Platform
14. Automated layered security/dependency scanning
15. Codex = short-lived scoped task + dedicated worktree + human merge gate

Engine/vendor names remain subject to Proof-of-Capability and license/security review.

---

# New Implementation Priority Principle

Initial user-facing development must prioritize:

> **33 Primary PDF Tool Cards first; Common-Simple-First determines sequencing inside that Core Set.**

This is implementation sequencing, not removal of advanced catalog capabilities.

**First recommended user-facing vertical slice:**

> `FEAT-PDF-005 — Merge PDF`

Reason:
- common office task;
- low risk;
- local preferred;
- MVP;
- Primary Tool;
- validates file input, validation, PDF provider abstraction, local processing, progress, result handling, storage metadata, logging, and testing without requiring OCR/AI/server-heavy complexity.

Recommended early order:
1. Merge PDF
2. Split PDF / page range
3. Rotate PDF page
4. Compress PDF
5. Simple image resize/crop/rotate/compress
6. Image → PDF
7. Batch Rename
8. File Checksum

---


# Primary PDF Tool Surface — Core Backlog

33 Core Tool Cards across:
- Organize PDF
- Optimize PDF
- Convert to PDF
- Convert from PDF
- Edit PDF
- PDF Security
- PDF Intelligence

New canonical capabilities from the latest audit:
- `FEAT-SIGN-013` — Sign PDF with Visual Signature (non-PKI)
- `FEAT-INTEL-001` — AI Summarizer
- `FEAT-INTEL-002` — Translate PDF
- `FEAT-CONV-036` — PDF to Markdown

Canonical feature count: **550**.

First vertical slice remains `FEAT-PDF-005 — Merge PDF`.
Secondary user-tool development waits until Core items are completed or have approved feasibility/exception decisions.

---

# Proof-of-Capability Gate

Still required for:
- edit existing PDF text;
- Office → PDF;
- PDF → Word/Excel/PowerPoint;
- complex table OCR;
- background removal;
- HTML/Web → PDF;
- PDF/A;
- physical scanner;
- optional local AI.

---

# Documentation Plan

01 `01_BUSINESS_PROCESS.md` — FINAL  
02 `02_REQUIREMENT_SPECIFICATION.md` — FINAL  
03 `03_FEATURE_CATALOG.md` — FINAL  
04 `04_SOLUTION_DESIGN.md` — FINAL  
05 `05_TECHNICAL_REFERENCE.md` — FINAL  
06 `06_API_DOCUMENTATION.md` — NEXT  
07 `07_DATABASE_DESIGN.md`  
08 `08_SECURITY_DESIGN.md`  
09 `09_ROLE_MATRIX.md`  
10 `10_AUDIT_TRAIL_DESIGN.md`  
11 `11_UNIT_TEST_DOCUMENT.md`  
12 `12_TEST_PLAN.md`  
13 `13_UAT_DOCUMENT.md`  
14 `14_USER_MANUAL.md`  
15 `15_ADMIN_MANUAL.md`  
16 `16_INCIDENT_SUPPORT_GUIDE.md`  
17 `17_DEPLOYMENT_GUIDE.md`  
18 `18_APPLICATION_CARTOGRAPHY.md`  
19 `19_HANDOVER_CHECKLIST.md`

---

# Next Work

Documentation continues with **06_API_DOCUMENTATION.md**.

When implementation planning begins, do not start 550 canonical features at once. Create a first end-to-end **Merge PDF vertical slice** with the minimum reusable foundation necessary for that feature.

Every Codex implementation task must state:
- Task ID
- Requirement IDs
- Feature IDs
- Allowed Paths
- Do-Not-Touch Paths
- Dependencies
- Acceptance Criteria
- Tests
- Security/Performance constraints
- Expected output

Human developer remains final reviewer and integrator.
