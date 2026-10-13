# PROOF-OF-CAPABILITY MASTER PLAN
## PERTAMINA OTO

> Versi: 1.0  
> Tanggal: 2 September 2026  
> Status: CANDIDATE — HUMAN/DEPENDENCY/SECURITY APPROVAL REQUIRED  
> Batas: PoC bukan pemilihan vendor final dan bukan bukti Production readiness.

---

# 1. Decision Model

Status keluaran:

```text
APPROVED_ENGINE
APPROVED_WITH_LIMITATION
BLOCKED_ALTERNATIVE_REQUIRED
DEFERRED_BY_GOVERNED_DECISION
```

Setiap keputusan wajib menyebut candidate/version, corpus, environment, license, security, quality, resource measurement, limitation, owner/reviewer, dan evidence hash.

---

# 2. Minimum PoC Portfolio

| PoC ID | Objective | Feature/capability impact | Priority | Critical gate |
|---|---|---|---|---|
| `POC-PDF-001` | Primary local PDF engine untuk Merge dan operasi PDF dasar | Merge + PDF page/optimize/security/edit families sesuai generated capability mapping | P0 | Real Merge adapter dan broad local PDF Core |
| `POC-PDF-002` | Edit existing PDF text | Edit PDF existing-text subset | P0 | Phase 6 claim boundary |
| `POC-CONV-001` | Office → PDF | Word/PPT/Excel → PDF | P0 | Phase 5 Office-to-PDF |
| `POC-CONV-002` | PDF → Office | PDF → Word/PPT/Excel | P0 | Phase 5 PDF-to-Office |
| `POC-OCR-001` | OCR Indonesia/English | OCR PDF/scanner/searchable text | P0 | OCR Core |
| `POC-OCR-002` | Complex Table OCR | Complex tables/forms/mixed-layout extraction | P1 | Table-heavy OCR/Markdown/Excel routes |
| `POC-CONV-003` | HTML/Web → PDF | Captured/local HTML rendering | P1 | HTML-to-PDF; no arbitrary remote URL |
| `POC-PDF-003` | PDF/A conversion/conformance | PDF → PDF/A | P1 | PDF/A Core |
| `POC-SCAN-001` | Physical scanner integration | Scan to PDF | P1/device | Scan Core |
| `POC-IMG-001` | Background removal | Secondary image capability | P2 | Does not block 33 Core gate |
| `POC-INTEL-001` | AI Summarizer | Summarizer | P0 | Phase 7 Summarizer |
| `POC-INTEL-002` | Translate PDF | Translation | P0 | Phase 7 Translate |
| `POC-CONV-004` | PDF → Markdown | Deterministic extraction + governed OCR/AI fallback | P1 | Phase 7 Markdown |
| `POC-AI-LOCAL-001` | Optional local AI runtime | Optional local provider path for intelligence | P1 | Only if local AI candidate is scheduled |

Candidate/product/version semuanya `TBD_APPROVED_CANDIDATE`; plan tidak memilih vendor.

---

# 3. Standard PoC Record

| Field | Wajib |
|---|---|
| Objective | Capability yang dibuktikan dan yang tidak dibuktikan |
| Candidate / Version | Exact package/product/model/runtime/build |
| Feature Mapping | Generated exact Feature IDs; no unexplained range |
| Test Samples | Synthetic/approved corpus manifest + digest/classification |
| Quality | Structural/visual/semantic/conformance oracle |
| Security | Malformed/active content/path/egress/prompt/secret/data controls |
| License/Cost | License, redistribution, runtime, support, cost model; no procurement assumption |
| Platform | OS/architecture/runtime/device/browser yang diuji |
| CPU/RAM/Storage | Measured peak/average on declared environment |
| Large File | Representative/boundary workloads; no universal limit invented |
| Concurrency | Measured isolated and bounded parallel cases |
| Offline | Network-disabled behavior and dependency proof |
| Cancellation/Failure | Cancel/crash/timeout/retry/temp cleanup/source safety |
| Fallback | Safe failure, limitation, alternative evaluation path |
| Decision | One governed status + owner/reviewer/date |
| Evidence | Report, logs sanitized, result digests, environment/corpus manifest |

---

# 4. Per-PoC Minimum Evidence

| PoC | Samples/oracle | Security/offline | Resource/failure | Decision focus |
|---|---|---|---|---|
| `POC-PDF-001` | Ordered multi-PDF corpus; page/order/content/reopen oracle | Encrypted/malformed/active-content safe; no network | large input, cancel/crash/temp/source immutability | Fidelity, license, local adapter viability |
| `POC-PDF-002` | Simple/complex/font/subset/rotated text objects; before-after render/structure | No unsafe content execution; source preserved | resource, cancel, unsupported object safe failure | Supported edit subset and fallback |
| `POC-CONV-001` | DOCX/PPTX/XLSX with fonts/tables/charts/formulas; visual/structural oracle | Macro/link/network denial; headless isolation | timeout/crash/large docs/concurrency | Fidelity/runtime/license matrix |
| `POC-CONV-002` | Native/scanned/mixed PDF; semantic/layout/editability oracle | Malicious PDF safe; no hidden upload | large/complex docs, failure/recovery | Output limitations per Office target |
| `POC-OCR-001` | ID/EN/mixed, rotation/noise/scan; character/word/field metric | OCR content excluded from ordinary logs | model size, pages, CPU/RAM, cancel | Accuracy/confidence/language support |
| `POC-OCR-002` | Complex table, merged cells, headers, multi-page, forms | Formula-injection safe output | page/table complexity measurement | Table structure fidelity/limitations |
| `POC-CONV-003` | Captured/local HTML/CSS/fonts/layout | Script/network/SSRF/active content denied | timeout/resource/large page | Offline renderer fidelity and safe boundary |
| `POC-PDF-003` | Declared PDF/A levels; independent validator | Font/profile/attachment/signature handling | large file/cancel/invalid input | Conformance level and limitations |
| `POC-SCAN-001` | Representative scanner/device; feeder/duplex/DPI/page cases | OS device consent; no silent capture/upload | cancel/device loss/large batch | Approved interface/device matrix |
| `POC-IMG-001` | Background types/subjects/resolutions; segmentation oracle | Local-only/no public inference | CPU/RAM/model/package/large image | Quality threshold and platform viability |
| `POC-INTEL-001` | Long/short/structured/multilingual documents; factual/coverage oracle | Prompt injection, scope, public-egress denial, no training/reuse assumption | context/resource/latency/concurrency | Grounding, omission/hallucination limits |
| `POC-INTEL-002` | ID↔EN, layout/table/terminology corpus; bilingual review | Same provider/data boundary; untrusted output | pages/context/resources/failure | Fidelity, layout and review requirements |
| `POC-CONV-004` | Native/scanned/table/multi-column PDF; structure Markdown oracle | Formula/link/content sanitization | large file/OCR fallback/resources | Deterministic-first capability and fallback |
| `POC-AI-LOCAL-001` | Representative intelligence corpus on target devices | Fully offline, model/license integrity, no public endpoint | package size, CPU/RAM/storage/thermal/concurrency | Optional local runtime feasibility |

---

# 5. Sequencing

```text
Wave 0 critical : POC-PDF-001
Wave 1 parallel : PDF-002, CONV-001, CONV-002, OCR-001,
                  OCR-002, CONV-003, PDF-003
Wave 2 governed : SCAN-001, INTEL-001, INTEL-002, CONV-004,
                  AI-LOCAL-001
Later secondary : IMG-001 when scheduled
```

PoCs boleh paralel setelah isolated environment, corpus authority, dependency/license review, dan reviewer tersedia. `POC-PDF-001` adalah critical path untuk real Merge adapter; pure contracts/domain/UI/fake tests tidak menunggu provider decision setelah Phase 1 gate.

---

# 6. Stop Rules

Stop dan lapor ke Human Owner/Agent 0 bila:

- license/data authority tidak dapat diverifikasi;
- candidate membutuhkan unapproved public endpoint;
- secret/document/content terekspos;
- source dapat rusak atau overwrite;
- candidate memerlukan architecture/security boundary baru;
- acceptance bertentangan dengan canonical requirements;
- evidence tidak dapat disanitasi;
- result hanya demo tanpa repeatable oracle.

Failed PoC tidak mengizinkan silent scope reduction atau vendor selection.

---

# 7. Current Status

Semua 14 PoC: `NOT_STARTED`. Tidak ada provider/vendor final yang dipilih.
