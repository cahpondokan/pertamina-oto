# IMPLEMENTATION ROADMAP
## PERTAMINA OTO

> Versi: 1.0  
> Tanggal: 2 September 2026  
> Status: CANDIDATE — HUMAN APPROVAL REQUIRED  
> Aturan: milestone berbasis evidence, bukan tanggal atau persentase rekaan.

---

# Roadmap

| Phase | Fokus | Entry minimum | Exit evidence | Tidak berarti |
|---:|---|---|---|---|
| 0 | Repository Bootstrap | Human accepts audit/manifest/roadmap; repo/access/owner/SDK/feed approved | Clean restore/build; non-zero test discovery; PR CI green; no product Feature | Feature implemented |
| 1 | Architecture Foundation | Phase 0 green | Minimum Contracts/Security/Observability/Storage/Processing abstractions; Merge Domain/Application contract tests; dependency rules pass | Provider selected |
| 2 | Proof-of-Capability | PoC charter/corpus/security/license/reviewer approved | Governed dispositions for required PoCs; limitation/evidence recorded | Permanent vendor/Production readiness |
| 3 | Merge PDF Vertical Slice | Phase 0–1; `POC-PDF-001` gates real adapter | `FEAT-PDF-005` local E2E; applicable 50 Unit/36 system evidence; 27 UAT package; 8-layer DoD evidence | Production ready |
| 4 | Core Local PDF Operations | Merge foundation stable | Split/remove/extract/organize/compress/repair/rotate/compare with source safety/local proof | All 550 implemented |
| 5 | Core Conversion/OCR | Relevant PoCs approved/limited | Scan/OCR/image/Office/HTML/PDF-A conversions with fidelity/security/resource evidence | Universal format fidelity |
| 6 | Core Edit/Security | Edit PoC/security controls ready | Page number/watermark/crop/edit/forms/unlock/protect/visual sign/redact with high-risk evidence | PKI signature included |
| 7 | Core PDF Intelligence | Approved local/internal provider or governed local route | Summarizer/Translate/Markdown with grounding/privacy/public-egress denial | Public AI allowed |
| 8 | Mobile/Web Curated Capability | Core contracts stable; exact platform exposure approved | Selected capability parity/limitations, real-device/browser evidence, no duplicate business rule | Full platform parity |
| 9 | Hardening, UAT & Pilot | Implemented release scope, QA environment, owners | Immutable RC; test/security/performance/recovery evidence; actual UAT; manuals; Pilot package | Production approval |
| 10 | Production Readiness | Pilot exit + Pertamina confirmations | Approved topology/SSO/network/config/secret/signing; restore/DR/monitoring/support; Production-readiness sign-off | Deployment/closure otomatis |

---

# Critical Path

```text
Audit & Human Acceptance
→ BOOT-001 Repository Authority
→ Phase 0 Build/Pipeline
→ Phase 1 Merge Contracts/Foundation
→ POC-PDF-001
→ Phase 3 Merge PDF
→ Core Local PDF
→ PoC-gated Conversion/Edit/Intelligence
→ Curated Platform Exposure
→ QA/UAT/Pilot
→ Production Readiness
```

---

# Milestones

| Milestone | Outcome | Exit evidence |
|---|---|---|
| `M0` | Documentation audited | 10 audit/readiness artifacts + Master update + human review pending |
| `M1` | Repository builds | Clean controlled clone restore/build; non-zero tests |
| `M2` | PR pipeline green | Restore→Build→Unit→Architecture→Contract→scans→reports |
| `M3` | PDF Engine PoC approved | `POC-PDF-001` decision/limitations/license/security/resource evidence |
| `M4` | Merge alpha | Picker/order/merge/validated Save As local happy path |
| `M5` | Merge vertical slice done | 8-layer applicable evidence; no Blocker/Critical |
| `M6` | Core local complete | Local PDF Core subset evidence/limitations |
| `M7` | Conversion/OCR complete | Relevant PoC/fidelity/security/performance evidence |
| `M8` | Edit/Security/Intelligence complete | High-risk/AI/data-boundary evidence |
| `M9` | QA/UAT candidate | Immutable RC, trace, results, known issues |
| `M10` | Pilot candidate | Actual UAT, support/monitoring/rollback readiness |
| `M11` | Production readiness | All Production confirmation/evidence/sign-off gates |

---

# Phase 0–3 Workstreams

```text
Human Owner / Final Approver / Merge Authorizer
└── Agent 0 — AI Master Integrator
    ├── Lane A — Repository/Foundation
    ├── Lane B — PDF Domain/Application/Adapter
    ├── Lane C — Desktop/UI/Workspace
    └── Lane D — Test/Security/Pipeline
```

Human dan Agent 0 bukan entitas yang sama. Agent 0 tidak memberi approval, tidak menjadi Product Owner, dan tidak melakukan merge tanpa otorisasi manusia.

---

# Phase Guard

1. Secondary user tools tidak masuk active implementation queue sebelum 33 Core gate terpenuhi atau ada controlled reprioritization.
2. Jangan memulai seluruh 550 Feature.
3. PoC dapat paralel, tetapi real adapter tetap gated oleh PoC masing-masing.
4. Setiap task wajib memiliki exact IDs, allowed paths, tests, security constraints, budget method, branch, dan worktree.
5. Local-only tetap membuktikan zero processing upload/API/SignalR/server Job.
6. Production confirmation tidak memblokir pure local foundation, tetapi wajib sebelum area yang terpengaruh.
7. Tidak ada milestone yang otomatis mengubah UAT, Production, handover, atau closure menjadi selesai.

---

# Current Gate

```text
Current milestone : M0 — DOCUMENTATION AUDITED
Next task         : BOOT-001
Authorization     : PLANNING ONLY
Formal coding     : NO-GO pending Phase-0 confirmations
```
