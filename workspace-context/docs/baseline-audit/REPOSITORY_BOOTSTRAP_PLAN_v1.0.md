# REPOSITORY BOOTSTRAP PLAN
## PERTAMINA OTO

> Versi: 1.0  
> Tanggal: 2 September 2026  
> Status: PLAN ONLY — FORMAL BOOTSTRAP BLOCKED  
> Tidak ada file aplikasi/repository yang dibuat oleh audit ini.

---

# 1. Entry Gate

Phase 0 hanya boleh dimulai setelah:

```text
[x] final baseline audit tersedia
[x] standalone baseline manifest tersedia
[x] cross-document/open-item/change-control registers tersedia
[ ] human menerima manifest dan roadmap
[ ] official Azure DevOps repo/project/default branch tersedia
[ ] human Product/Technical/Repository Integrator ditetapkan
[ ] developer/reviewer access disetujui
[ ] exact .NET 10 SDK patch dan build agent disetujui
[ ] NuGet sources/source mapping dan initial dependencies disetujui
[ ] branch protection/PR/pipeline authority tersedia
```

Current first task: `BOOT-001`. Formal coding tetap NO-GO.

---

# 2. Proposed Minimal Tree

```text
pertamina-oto/
├── global.json
├── Directory.Build.props
├── Directory.Packages.props
├── Pertamina.Oto.sln
├── README.md
├── src/
│   ├── BuildingBlocks/
│   │   ├── Pertamina.Oto.Contracts/
│   │   ├── Pertamina.Oto.Security/
│   │   ├── Pertamina.Oto.Observability/
│   │   ├── Pertamina.Oto.Storage.Abstractions/
│   │   └── Pertamina.Oto.Processing.Abstractions/
│   ├── Modules/
│   │   ├── Pdf/
│   │   ├── FileProductivity/
│   │   └── Workspace/
│   └── Hosts/
│       ├── Pertamina.Oto.Desktop/
│       └── Pertamina.Oto.LocalWorker/
├── tests/
│   ├── Unit/
│   ├── Integration/
│   ├── Architecture/
│   └── Contract/
├── docs/
│   └── implementation/
├── eng/
│   ├── baseline/
│   ├── build/
│   ├── test/
│   └── security/
└── pipelines/
    └── azure-pipelines-ci.yml
```

Tree adalah proposal. Project hanya dibuat ketika mempunyai first tested behavior. Jangan membuat semua 12 modules/hosts sebagai empty shell.

`main` adalah proposed protected branch; official default branch tetap `TBC`.

---

# 3. Root File Responsibility

| File/area | Minimum responsibility | Guard |
|---|---|---|
| `global.json` | Exact approved .NET SDK patch/roll-forward | Tidak boleh mengarang value |
| `Directory.Build.props` | Nullable, language, analyzer, warning, deterministic build metadata | Satu foundation writer |
| `Directory.Packages.props` | Central approved package versions | Dependency/license/security review |
| Solution | Active projects only | Tidak ada duplicate solution authority |
| `README.md` | Scope, build/test entry, current non-Production status, contribution link | Tidak menyimpan secret/claim palsu |
| `eng/baseline` | D01–D19 manifest/hash/decision references | Audit/Integrator review |
| `eng/build` | Local/CI reproducible entry scripts | No Production command |
| `eng/test` | MTP settings/filter/result sanitization/corpus tooling | Zero tests = failure |
| `eng/security` | Approved scans/allowlist/waiver schema | No disabled gate to obtain green |
| `pipelines` | PR CI only initially | No Production deploy stage |

---

# 4. Bootstrap Work Packages

| Task | Deliverable | Exit evidence | Status |
|---|---|---|---|
| `BOOT-001` | Repository authority/access/ownership + accepted baseline manifest | Repo reference, access test, owners, accepted D01–D19/Master baseline | `BLOCKED` |
| `BOOT-002` | SDK/feed/package admission | Approved SDK patch, sources, first dependency record | `BLOCKED` |
| `BOOT-003` | Root build files/README/solution | Clean restore/build from controlled clone | `PENDING` |
| `BOOT-004` | Minimal active projects | Reference graph; no unrelated empty modules | `PENDING` |
| `BOOT-005` | MSTest/MTP discovery | Meaningful smoke test discovered/reported | `PENDING` |
| `BOOT-006` | Architecture/Contract test skeleton | Forbidden reference and schema/registry tests executable | `PENDING` |
| `BOOT-007` | PR CI | Green pipeline plus negative canary proof | `PENDING` |
| `BOOT-008` | Multi-agent task/worktree workflow | Approved task-card schema, path ownership, stop protocol | `PENDING` |

---

# 5. Initial Project Creation Sequence

| Project | Create when | First responsibility | Forbidden content |
|---|---|---|---|
| Contracts | Phase 0/1 | Canonical IDs and typed/versioned primitives | UI/provider/ORM/workflow |
| Security | Phase 1 | Authorization context/decision ports | Scattered role branching |
| Observability | Phase 1 | Correlation/safe logging primitives | Content/password/raw path |
| Storage Abstractions | Phase 1 | Opaque refs/atomic publication ports | Physical path as authority |
| Processing Abstractions | Phase 1 | Engine/progress/cancel/capability ports | Vendor SDK/Feature branching |
| Pdf Domain/Application/Contracts | Phase 1 | Merge rules/use case/contracts | UI/provider/ORM leaks |
| Pdf Infrastructure | After `POC-PDF-001` | Approved adapter | Provider selection policy |
| Desktop | Phase 1/3 | WinUI shell/input/state adapters | Business rule/SQL/provider SDK |
| Workspace | Merge working/local job/history need | Local state/persistence boundary | PDF transform logic |
| Local Worker | After IPC/threshold decision | Job-scoped isolated execution | Public endpoint/user token |

---

# 6. Task Card Schema

Setiap task multi-agent wajib memuat:

```text
Task ID
Requirement IDs
Feature IDs
Objective
ProcessingMode
ServerDependency
StorageImpact
NetworkImpact
ConcurrencyRisk
Allowed Paths
Do-Not-Touch Paths
Dependencies
Acceptance Criteria
Tests
Security Constraints
Performance Budget / Measurement Method
Expected Output
Branch
Worktree
Owner Lane
Reviewer
Stop Conditions
Handoff Evidence
```

Missing field berarti task belum siap dispatch.

---

# 7. Multi-Agent Ownership

| Actor/Lane | Scope | Authority boundary |
|---|---|---|
| Human Owner/Integrator | Final review, approval, merge authorization, dependency/provider decision | Satu-satunya approval/merge authority |
| Agent 0 — AI Master Integrator | Task slicing, coordination, audit, evidence synthesis | Tidak approve, tidak merge tanpa authority, tidak menjadi owner Pertamina |
| Lane A — Repository/Foundation | Root build/package/test/pipeline + BuildingBlocks | No Feature/UI/provider work |
| Lane B — PDF | Pdf contracts/domain/application/approved adapter | No root package/UI/server endpoint |
| Lane C — Desktop/Workspace | WinUI/input/state/Save As/local metadata | No PDF business/provider rule |
| Lane D — Test/Security/Pipeline | Cross-cutting tests/scans/gates | No expected-behavior weakening |

Root files/contracts/registries mempunyai satu writer pada satu waktu.

---

# 8. Proposed Git/Worktree Strategy

```text
Default protected branch : TBC (proposed: main)
Task branch              : task/<task-id>-<short-name>
PoC branch               : poc/<poc-id>-<short-name>
Documentation evidence   : docs/<task-id>-<short-name>
Worktree                 : one per active lane/task
```

Do not force-push/rebase another lane. Contract conflict menghentikan downstream work dan meminta Human Integrator decision.

---

# 9. Initial PR Pipeline

```text
Restore
→ Build
→ Unit
→ Architecture
→ Contract
→ Dependency Audit
→ Secret Scan
→ Static Analysis
→ Safe Artifact / Report
```

Failure rules:

- unapproved package/source fails;
- zero tests fails;
- ignored critical test fails;
- architecture/contract incompatibility fails;
- real secret detection blocks and triggers rotation process;
- unsafe/mutable/incomplete evidence artifact is rejected.

Exact command, threshold, agent image, retention, and signing policy remain `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

---

# 10. Phase 0 Exit

```text
[ ] controlled clone restores/builds reproducibly
[ ] non-zero meaningful MTP tests discovered
[ ] architecture/contract canary tests execute
[ ] PR pipeline green and negative canary proves blocking
[ ] dependency/source record approved
[ ] no product Feature implementation
[ ] no secret/Production data/config
[ ] evidence linked to commit/build/task IDs
```
