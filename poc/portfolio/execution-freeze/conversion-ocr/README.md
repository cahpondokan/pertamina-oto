# Conversion/OCR/PDF-A No-Execution Disposition

Packet `P2-W0G-B-CONVERSION-OCR-DISPOSITION-001` is a proposed, static-only
scope/dependency/gate matrix for run `P02-20260905-1434`. It covers exactly:

- `POC-CONV-001`, `POC-CONV-002`, `POC-CONV-003`, and `POC-CONV-004`;
- `POC-OCR-001` and `POC-OCR-002`;
- `POC-PDF-003`.

Every record is `NOT_STARTED`, `NOT_APPROVED`, `NOT_AUTHORIZED`, and has
`NO_GOVERNED_DISPOSITION`. The only eventual disposition vocabulary is
`APPROVED_ENGINE`, `APPROVED_WITH_LIMITATION`,
`BLOCKED_ALTERNATIVE_REQUIRED`, or `DEFERRED_BY_GOVERNED_DECISION`, and none
of those results is claimed here.

## Approval isolation

`POC-PDF-001` is scoped to Merge PDF. Its candidate, authority, execution,
corpus/oracle, or later result cannot be inherited by the seven records in
this packet. Each future PoC requires its own exact scope, candidate/version,
license/cost, security review, corpus, independent oracle, isolated
environment, measurement plan, evidence policy, reviewers, and explicit human
execution authorization.

The proposal preserves these mandatory differentiators:

- Office conversion denies macro/VBA/OLE/DDE/embedded executable execution and
  external link/template/network fetch;
- PDF-to-Office and table OCR neutralize or reject spreadsheet formula
  injection and do not emit active macros or external links;
- HTML conversion is limited to validated local/captured artifacts and denies
  JavaScript, active content, arbitrary remote URLs, SSRF, all rendering
  network egress, local/custom schemes, downloads, WebSocket, ambient
  credentials/storage, and uncontrolled navigation;
- OCR output is restricted untrusted DATA, excluded from ordinary logs,
  telemetry, generic indexing, commands, and public AI/upload paths;
- PDF-to-Markdown is deterministic-first, uses only separately governed OCR
  fallback, and keeps raw HTML, auto-fetch, formula, link, and document
  instructions inert;
- PDF/A conformance requires an exact independent validator. The conversion
  candidate cannot be its sole oracle, and only explicitly approved PDF/A
  levels may be claimed.

## Feature mapping

| PoC | Direct canonical Feature IDs | Separately gated dependencies / exclusions |
|---|---|---|
| `POC-CONV-001` | `FEAT-CONV-001`, `FEAT-CONV-002`, `FEAT-CONV-003` | Does not prove PDF-to-Office, HTML, Markdown, or PDF/A |
| `POC-CONV-002` | `FEAT-CONV-004`, `FEAT-CONV-005`, `FEAT-CONV-006` | Does not prove Office-to-PDF, HTML, Markdown, or PDF/A |
| `POC-OCR-001` | `FEAT-OCR-001`, `FEAT-OCR-002`, `FEAT-OCR-003`, `FEAT-OCR-004`, `FEAT-OCR-005`, `FEAT-OCR-009`, `FEAT-OCR-010`, `FEAT-OCR-011` | Complex table/layout IDs remain outside this PoC |
| `POC-OCR-002` | `FEAT-OCR-006`, `FEAT-OCR-012`, `FEAT-OCR-013` | Excel/CSV/Markdown routes `FEAT-CONV-005/021/022/025/036` remain separately gated |
| `POC-CONV-003` | `FEAT-CONV-017`, `FEAT-CONV-018` | `FEAT-CONV-018` is limited to a captured/local artifact; remote URL fetch remains unavailable |
| `POC-PDF-003` | `FEAT-PDF-075` | Does not prove Merge or undeclared PDF/A levels |
| `POC-CONV-004` | `FEAT-CONV-036` | OCR fallback `FEAT-OCR-005/006/012/013` requires its own approved disposition; no AI is implied |

## Gate and evidence rule

`P2-D01`, `P2-D02`, and `P2-D03` remain `PROPOSED`. Separate security,
environment/measurement/evidence, and human execution gates also remain open.
No numeric quality, accuracy, resource, file-size, page-count, duration, or
concurrency threshold is invented. A future run must preserve initial failures
and all measurements, distinguish unsupported behavior from failure, keep
sources immutable, publish only independently validated results, fail closed
on denied/inconclusive security behavior, and sanitize evidence without storing
document/OCR/Markdown/formula content.

The machine-readable proposal contains the full prerequisites, owners, oracle,
resource, cancel/crash/timeout/failure/cleanup requirements, and exact reopen
conditions per PoC.

## Static verification

From this directory, run only the text/JSON verifier:

```powershell
python -B .\verify_static.py
```

The verifier parses this packet and schema only. It must not be repurposed to
open Office/HTML/PDF/image input, load a renderer/parser/OCR/conversion engine,
launch Windows Sandbox, restore/build a project, call a network endpoint, or
acquire a package/model/binary.

Expected result:

```text
STATIC_CONVERSION_OCR_DISPOSITION_VERIFICATION=PASS
POCS=7;SCHEMA=1;NEGATIVE_FIXTURES=8;EXECUTION_AUTHORIZATIONS=0;VENDOR_RECOMMENDATIONS=0
DOCUMENTS_OPENED=0;PDF_PROCESSING=0;ENGINE_LOADS=0;NETWORK_ACQUISITIONS=0
```
