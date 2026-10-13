# Reader verification and local search — task card

- Agent: Agent 0 only, following the existing resource preference; no specialist dispatch.
- Objective: user requested both verification/fixes of the existing reader and content search on 30 September 2026.
- Baseline: `worktrees/core-functional-first`, `task/core-functional-first` at `84cbb0f` plus existing uncommitted work; previous local update `20260930-130204-c6ca5caf`. Preserve all existing work.
- Inputs: shared contract; project state and decisions; reader implementation/audit; existing PDFsharp worker; `PDF_READER_WORKSPACE.md`, `LOCAL_UPDATES.md`, PDFium intake (not admitted).
- Allowed paths: reader/search and audit files, local worker protocol/client/dispatch and dedicated search tests, local update build gate, development and Agent 0 coordination records, task-specific artifacts and verified local update feed.
- Do not touch: editor extraction/mutation semantics, canonical requirements, protected branches, installed application, user documents, PDFium candidate admission, external services.
- Acceptance: search selectable PDF text locally, show page/snippet results and navigate to matching pages; accessible controls and cancellation; clear limits for scans/unsupported structures; preserve source bytes and stale-document safety; existing reader regression passes.
- Design: bounded read-only text extraction using existing PDFsharp content parser in the existing child process. Independent from restrictive editable-text inspection. No engine/dependency acquisition. Support simple fonts and supported ToUnicode maps; never treat partial extraction as proof the whole PDF has no match. Search returns at most 100 snippets; no plaintext cache or logging.
- Verification: native reader baseline and integrated UI audit/screenshots; worker search synthetic tests covering fonts, encoding, partial extraction, source safety and bad inputs; relevant worker/editor/history regressions; local package build gate. Physical gestures and network-denial execution remain separately disclosed if not performed.
- Dependencies: existing local toolchain/packages. PDFium remains outside product scope.
- Stop conditions: new dependency/admission required, unsafe mutation, unexpected concurrent changes, or failing verification. No Production or phase promotion.
- Handoff: exact paths, test results, limitations, update ID and evidence.
