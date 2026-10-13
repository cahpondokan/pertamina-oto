# CHANGE CONTROL REGISTER
## PERTAMINA OTO — Documentation Baseline

> Versi: 1.0  
> Tanggal: 2 September 2026  
> Status: **ACTIVE — PRE-IMPLEMENTATION**

---

# 1. Aturan

1. Dokumen 01–19 FINAL tidak boleh diubah tanpa controlled revision.
2. Perubahan substansi memerlukan owner, reviewer, approver, reason, impact, evidence, versi baru, dan hash baru.
3. Perbaikan metadata/bahasa tidak boleh mengubah requirement, Feature ID, contract, role, permission, schema, atau behavior.
4. Agent 0 dapat menyusun usulan dan evidence, tetapi bukan approval authority.
5. File lama tidak dihapus sebelum canonical/superseded disposition disetujui manusia.
6. `MASTER_PROJECT_SUMMARY.md` adalah mutable index; snapshot immutable harus dibuat pada acceptance gate.

---

# 2. Register

| Change ID | Scope | Perubahan / kebutuhan | Kelas | Status | Authority / owner | Exit evidence |
|---|---|---|---|---|---|---|
| `CCR-001` | Baseline audit | Buat audit, manifest, matrix, traceability, readiness, roadmap, PoC, bootstrap, dan register | New governance artifacts | `RESOLVED` | Agent 0; human review pending | 10 artifact tersedia dan tervalidasi |
| `CCR-002` | D01–D08 duplicates | Tetapkan approval authority, canonical path, dan superseded marker untuk snapshot/handoff/duplicate | Configuration | `OPEN` | Human Documentation Integrator | Approved manifest + archive index; tanpa silent delete |
| `CCR-003` | Master snapshots | Pulihkan/rekonstruksi v3.4/v4.0/v4.1 bila diwajibkan dan buat immutable snapshot v4.3 | Configuration | `OPEN` | Repository/Documentation Owner | Versioned snapshot + SHA-256 + parent chain |
| `CCR-004` | D01–D19 metadata | Tambah Document ID, content revision, parent hashes, owner/reviewer/approver, dan changelog terkontrol | Metadata | `DEFERRED_CONTROLLED_REVISION` | Documentation Owner | Approved metadata-only revision |
| `CCR-005` | Bahasa/status | Audit ulang ID-1.0; normalisasi istilah dan lifecycle vocabulary | Editorial/governance | `OPEN` | Documentation Owner | Reproducible lint/review record; no semantic diff |
| `CCR-006` | D01 standard label | Kualifikasi `Selaras` sebagai baseline internal sampai official Pertamina compliance sign-off | Governance | `OPEN` | Pertamina IT C&T / Business Owner | Official interpretation/sign-off; controlled D01 revision bila diperlukan |
| `CCR-007` | D02/D03 | Disposition `FR-FILE-006..009` tanpa membuat ID baru diam-diam | Requirement | `OPEN` | Requirement/Business Owner | Add/replace/remove decision + trace impact + approved revision |
| `CCR-007A` | D02 | Disposition undefined reference `UIR-UI-015..016` dan revalidate downstream | Requirement | `OPEN` | Requirement/UX Owner | Canonical definition/range correction + approved revision |
| `CCR-008` | D03 | Selaraskan risk `FEAT-CONV-017` dari LOW terhadap MEDIUM controls downstream | Security/risk | `OPEN` | Security + Feature Owner | Approved risk disposition dan controlled revision |
| `CCR-009` | D06/D07/D09/D15 | Lengkapi typed Admin user/session/role/permission contracts dan direct-assignment persistence | API/Data | `OPEN` | API + Security Owner | Versioned contract, mapping, OpenAPI diff, contract/security tests |
| `CCR-010` | D06/D07/D10 | Lengkapi Audit batch/database event type/version/stable ID/sequence/provenance/per-item results/dedupe/index serta detail projection | API/Audit/Data | `OPEN` | API + Audit Owner | Compatible addendum + schema/index/authorization tests |
| `CCR-011` | D06/D07/D10 | Lengkapi Activity/monitoring projection dan export-result parent binding | API/Data/Audit | `OPEN` | API + Data + Audit Owner | Contract/schema/index/tests |
| `CCR-012` | D18 | Buat versioned Local IPC contract dan internal-event catalog | Architecture | `OPEN` | Architecture + Security Owner | Executable schema, compatibility/fault/security tests |
| `CCR-012A` | D04 | Rekonsiliasi conceptual tree dengan locked ContentIntelligence/Local Worker atau nyatakan tree non-exhaustive | Architecture presentation | `OPEN` | Architecture Owner | Approved controlled D04 revision/interpretation |
| `CCR-013` | Traceability | Generate overlay 550 Feature ke module/interface/data/permission/Audit/test/manual/artifact | Traceability | `IMPLEMENTATION_PENDING` | Technical Integrator | Generated matrix bound to source commit/artifact digests |
| `CCR-014` | Master | Perbarui status audit dan implementation readiness ke v4.3 | Summary index | `RESOLVED` | Agent 0; human acceptance pending | Master v4.3 current hash pada manifest |
| `CCR-015` | Phase 0 execution | Gunakan provisional local Git repository dan local CI/evidence untuk offline-first bootstrap; Hostinger non-Production lalu Azure setelah IT approval | Temporary execution/governance exception | `APPROVED_LOCAL_ONLY` | Workspace User — Project/Product Owner dan approval authority sementara | Phase 0 `COMPLETE` local-only: `docs/project-management/PHASE_00_FINAL_REPORT_P00-20260905-1155.md`; final `d497ec68ea8fa3e21ca311a580a44111045cfb20`; remote/IT/Azure gates tetap terbuka; no Production claim |

---

# 3. Controlled Revision Minimum Record

```text
ChangeId
DocumentId
OldVersion / OldSha256
NewVersion / NewSha256
Reason
SemanticImpact
TraceabilityImpact
SecurityImpact
MigrationOrCompatibilityImpact
Owner
Reviewer
Approver
ApprovalEvidence
EffectiveDate
Supersedes
```

---

# 4. Status

```text
Resolved audit artifacts             : 2
Approved local-only exceptions        : 1
Open controlled changes              : 12
Deferred controlled revisions        : 1
Implementation-pending overlay        : 1
Silent changes to Documents 01–19     : 0
Application code changes              : 0
```

Jumlah status adalah snapshot register dan tidak menyatakan seluruh open item proyek selesai.
