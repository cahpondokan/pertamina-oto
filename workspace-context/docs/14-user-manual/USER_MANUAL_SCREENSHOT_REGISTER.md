# REGISTER SCREENSHOT USER MANUAL
## PERTAMINA OTO — Office Tools Organizer

> **Versi:** 1.0  
> **Tanggal:** 1 September 2026  
> **Status:** PLANNED — ACTUAL RELEASE CANDIDATE BELUM TERSEDIA  
> **Aturan:** Placeholder bukan screenshot aplikasi

---

# 1. Lifecycle

```text
PLANNED → CAPTURED → REVIEWED → APPROVED
APPROVED → OUTDATED → replacement PLANNED
```

Hanya screenshot `APPROVED` dari exact supported RC yang boleh masuk manual release aktif.

---

# 2. Field Wajib

```text
Screenshot ID; Chapter/Guide ID; Platform; Language; Build Version
Screen; Purpose; Required Elements; Data Type; Masking Required
Status; Owner; Capture Date; Artifact Path/Digest
Alt Text; Caption; Last Reviewed Build; Replacement ID
```

---

# 3. Screenshot Umum

| ID | Layar/tujuan | Platform | Elemen wajib | Data | Status |
|---|---|---|---|---|---|
| `SS-LOGIN-001-ID` | Login | Desktop | Login Pertamina, bantuan | Dummy account | PLANNED |
| `SS-HOME-001-ID` | Home/Quick Start | Desktop | file, tool, Assistant, Activity | Dummy files | PLANNED |
| `SS-ENTRY-001-ID` | File-First | Desktop | file status, recommendation | Dummy PDF | PLANNED |
| `SS-ENTRY-002-ID` | Tool-First | Desktop | tool, input, requirement | Dummy PDF | PLANNED |
| `SS-ENTRY-003-ID` | Assistant plan | Desktop | plan, file, output, warning | Dummy intent | PLANNED |
| `SS-BASIC-001-ID` | Working state | Desktop | preview, Undo/Redo | Dummy PDF | PLANNED |
| `SS-SAVE-001-ID` | Save As | Desktop | name, location, save | Dummy path | PLANNED |
| `SS-ACTIVITY-001-ID` | Activity Center | Desktop | waiting/running/fail/done/actions | Dummy job | PLANNED |
| `SS-RECOVERY-001-ID` | Recovery | Desktop | Continue/Retry/Cancel | Dummy draft | PLANNED |
| `SS-HISTORY-001-ID` | History | Desktop | activity/result/open | Dummy history | PLANNED |
| `SS-SECURED-001-ID` | Amankan ke Akun | Desktop | notice, target, progress | Dummy result/account | PLANNED |
| `SS-SECURED-002-ID` | Secured Files | Web | file, max-three versions | Dummy files/account | PLANNED |
| `SS-PLATFORM-001-ID` | Camera scan | Phone | capture/crop/reorder | Dummy document | PLANNED |
| `SS-PLATFORM-002-ID` | Visual signature | Tablet | touch/stylus preview | Synthetic mark | PLANNED |
| `SS-WEB-001-ID` | Server transfer notice | Web | file, purpose, consent/progress | Dummy PDF | PLANNED |
| `SS-ISSUE-001-ID` | Report a Problem | Desktop | message, optional screenshot | Dummy Issue | PLANNED |
| `SS-ISSUE-002-ID` | Screenshot preview | Desktop | preview/markup/remove/send | Dummy Issue | PLANNED |
| `SS-ERROR-001-ID` | Safe error | Desktop | message, next action, safe ID | Dummy IDs | PLANNED |
| `SS-L10N-001-EN` | English Home | Desktop | EN labels | Dummy files | PLANNED |
| `SS-ACC-001-ID` | Zoom/reflow | Web | readable layout | Dummy data | PLANNED |

---

# 4. Screenshot 33 Core

| No | ID | Tool / Feature ID | Platform target | Tujuan | Status |
|---:|---|---|---|---|---|
| 1 | `SS-CORE-001-ID` | Merge `FEAT-PDF-005` | Desktop | order/validation | PLANNED |
| 2 | `SS-CORE-002-ID` | Split `FEAT-PDF-006/007` | Desktop | range | PLANNED |
| 3 | `SS-CORE-003-ID` | Remove `FEAT-PDF-013` | Desktop | selection/Undo | PLANNED |
| 4 | `SS-CORE-004-ID` | Extract `FEAT-PDF-014` | Desktop | page output | PLANNED |
| 5 | `SS-CORE-005-ID` | Organize `FEAT-PDF-010..016` | Desktop | reorder/preview | PLANNED |
| 6 | `SS-CORE-006-ID` | Scan `FEAT-SCAN-014` | Phone | capture/crop | PLANNED |
| 7 | `SS-CORE-007-ID` | Compress `FEAT-PDF-023` | Desktop | size/quality | PLANNED |
| 8 | `SS-CORE-008-ID` | Repair `FEAT-PDF-025/026` | Desktop | outcome | PLANNED |
| 9 | `SS-CORE-009-ID` | OCR `FEAT-OCR-004` | Desktop | language/preview | PLANNED |
| 10 | `SS-CORE-010-ID` | JPG→PDF `FEAT-CONV-007` | Desktop | image order | PLANNED |
| 11 | `SS-CORE-011-ID` | Word→PDF `FEAT-CONV-001` | Desktop | limitation | PLANNED |
| 12 | `SS-CORE-012-ID` | PPT→PDF `FEAT-CONV-003` | Desktop | slide scope | PLANNED |
| 13 | `SS-CORE-013-ID` | Excel→PDF `FEAT-CONV-002` | Desktop | sheet/range | PLANNED |
| 14 | `SS-CORE-014-ID` | HTML→PDF `FEAT-CONV-017` | TBC | approved input | PLANNED |
| 15 | `SS-CORE-015-ID` | PDF→JPG `FEAT-CONV-012` | Desktop | page/image | PLANNED |
| 16 | `SS-CORE-016-ID` | PDF→Word `FEAT-CONV-004` | Desktop | conversion warning | PLANNED |
| 17 | `SS-CORE-017-ID` | PDF→PPT `FEAT-CONV-006` | Desktop | slide result | PLANNED |
| 18 | `SS-CORE-018-ID` | PDF→Excel `FEAT-CONV-005` | Desktop | table result | PLANNED |
| 19 | `SS-CORE-019-ID` | PDF/A `FEAT-PDF-075` | Desktop | conformance | PLANNED |
| 20 | `SS-CORE-020-ID` | Rotate `FEAT-PDF-012` | Desktop | page preview | PLANNED |
| 21 | `SS-CORE-021-ID` | Page numbers `FEAT-PDF-037` | Desktop | range/position | PLANNED |
| 22 | `SS-CORE-022-ID` | Watermark `FEAT-PDF-036` | Desktop | opacity/preview | PLANNED |
| 23 | `SS-CORE-023-ID` | Crop `FEAT-PDF-019` | Desktop | bounds | PLANNED |
| 24 | `SS-CORE-024-ID` | Edit `FEAT-PDF-031..041` | Desktop | supported edit | PLANNED |
| 25 | `SS-CORE-025-ID` | Forms `FEAT-PDF-053..060` | Desktop | field/validation | PLANNED |
| 26 | `SS-CORE-026-ID` | Unlock `FEAT-PDF-062` | Desktop | masked password | PLANNED |
| 27 | `SS-CORE-027-ID` | Protect `FEAT-PDF-061` | Desktop | warning/options | PLANNED |
| 28 | `SS-CORE-028-ID` | Sign `FEAT-SIGN-013` | Phone/Tablet | non-PKI preview | PLANNED |
| 29 | `SS-CORE-029-ID` | Redact `FEAT-PDF-069` | Desktop | preview/final warning | PLANNED |
| 30 | `SS-CORE-030-ID` | Compare `FEAT-PDF-071/072` | Desktop | A/B difference | PLANNED |
| 31 | `SS-CORE-031-ID` | Summarizer `FEAT-INTEL-001` | TBC | result/review | PLANNED |
| 32 | `SS-CORE-032-ID` | Translate `FEAT-INTEL-002` | TBC | language/review | PLANNED |
| 33 | `SS-CORE-033-ID` | Markdown `FEAT-CONV-036` | Desktop | structure preview | PLANNED |

---

# 5. Set Merge PDF

```text
SS-MERGE-001-ID  empty state + minimal two inputs
SS-MERGE-002-ID  A/B/C validation, order, remove, Undo
SS-MERGE-003-ID  progress/Activity/Cancel
SS-MERGE-004-ID  result preview/page order
SS-MERGE-005-ID  Save As/open result
SS-MERGE-006-ID  optional Amankan ke Akun notice
SS-MERGE-007-ID  invalid file or insufficient storage
```

Semua berstatus `PLANNED`, build `RC-TBD`, owner `Documentation Owner — TBC`.

---

# 6. Aturan Keamanan

Gunakan data dummy/synthetic. Dilarang menampilkan password/token/secret, real user/account, internal document, real signature, restricted Issue, raw log/stack/SQL/path/storage key, unrelated tab/notification/taskbar/recent item, atau Production data.

Capture workflow: verifikasi RC/platform/locale → muat dataset dummy → tutup layar lain → capture area minimum → periksa content/metadata → catat register → documentation/security/privacy review → approve/store by controlled reference.

Setiap screenshot wajib mempunyai alt text dan caption. Mockup atau gambar edit tidak boleh disebut screenshot aplikasi.
