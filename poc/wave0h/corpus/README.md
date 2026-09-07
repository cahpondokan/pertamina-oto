# Wave 0H synthetic corpus

Prepared for Agent 0 execution after technical review. No generator or PDF parser was run during preparation.

Scope: POC-PDF-001 / FEAT-PDF-005. Python 3.12+ standard library only; no dependency acquisition. Fixed US Letter pages, Helvetica, one ASCII marker per page, no images, forms, attachments, scripts, encryption, external references or hostile payloads. A has 2 pages, B 3, C 1. Six source pages total. Each merge has at most 6 pages. This is a small technical fixture, not a product capacity threshold.

`spec.json` contains exact input occurrence lists, expected page markers, and UT-MERGE/TP-MRG IDs. Expected counts: AB=5, CAB=6, AA=4, AC=3. AA explicitly repeats A twice; never deduplicate it. AC is the final list after removing B; CLI input selection alone does not prove a UI removal action. CAB alone does not prove UI reorder behavior. TP-MRG-008 is covered only for three inputs and explicit duplication, not load testing.

## Execution handoff (Agent 0 only, after review)

1. In the admitted isolated run environment with the existing compatible Python runtime, run `python generate.py NEW_DIRECTORY` once. The parent must already exist. A failed run may leave incomplete files; abandon that run directory and choose a fresh one. Do not reuse or overwrite it. No Python availability in the guest is asserted by this lane.
2. Independently validate all three sources before merging. Archive `sources.json`, its SHA-256 and source hashes in the operator's pre-run evidence before invoking the engine. Preserve that manifest read-only through the run. A mutable manifest alone cannot prove source immutability. Use distinct output paths outside the source directory. Never target A.pdf/B.pdf/C.pdf or an existing user file.
3. Use the admitted candidate commands from Agent 2 with exactly the input occurrence lists in `spec.json`. Record engine/version, command, exit status, output hash and source hashes after each case. No network/server calls are needed by this corpus tooling; network isolation evidence belongs to Agent 1/operator.
4. An observer independent of the merge engine must reopen each output, count pages, and read the visible marker on EVERY page in order, including both occurrences for AA. Archive raw observer output or render evidence. If qpdf merges, qpdf observing itself is insufficient for an independent verdict. If an independent observer is unavailable, record that specific limitation; do not invent observations or claim a pass. Render/fidelity verification is separate and remains outstanding.
5. Write an observation JSON with fields `case`, `outputSha256`, `rawEvidenceSha256`, `pageCount` (integer), `pageMarkers` (ordered string array), `reopened` (boolean), `observerTool`, `observerVersion`, `independentOfMergeEngine` (boolean). Use measured values, not a copied expected list. Run `python oracle.py AB SOURCE_DIR OUTPUT.pdf OBSERVATION.json RAW_EVIDENCE`, substituting each case. Files are bounded to 1 MiB and JSON to 64 KiB for this smoke. Capture exit code and stdout/stderr. The script hashes bytes but never parses a PDF.

The comparator binds observations to the measured output and raw evidence and checks unchanged source digests against the frozen pre-run manifest. It cannot authenticate a human-entered observation or establish observer independence: Agent 0 and independent reviewers must audit raw evidence and provenance. A successful comparison is only STRUCTURAL_OBSERVATION_MATCH. It is not a fidelity, security, publication or Phase 2 completion verdict.

## Coverage boundaries

The four structural cases support the exact IDs in spec.json, subject to actual execution. Successful independent reopen contributes to TP-MRG-029. Not exercised here: UT-MERGE-008 (one input rejection), UT-MERGE-010 (missing ordered ID), UT-MERGE-033/034/036/037/038 (validation/publication and source/existing-target protection), TP-MRG-028 (bad output rejection). No corrupt PDF is generated. Source hash preservation supplies evidence for TP-MRG-001 only after an actual pre/post run; no-server evidence and application behavior require operator/harness evidence. No requirement is declared passed by this preparation.

Baseline: d472b85e179d715b9795773bcfda01dc628e9d42. Allowed path: poc/wave0h/corpus/**. Historical Wave 0G authorization snapshots are unchanged; current human authorization is recorded by Agent 0. This lane does not request repeated approval or execute the authorized operator work.
