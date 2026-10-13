# PDFium original-text probe - 18 September 2026

Outcome: BASIC_PASS_SUBSET_LIMITATION. This is a small engine probe, not a complete editor or admission for application distribution.

Used existing Codex bundled Python 3.12 tooling: pypdfium2 5.13.0 and PDFium 153.0.7999.0. No downloads, installation, app references or user documents. DLL SHA256: 958e5342ed7e2e20fb914adde238bbae0ac8ad4a3267aa49d0b9dd266c7667f2. Bundled version.json reports pdfium-binaries origin and null source hash. Upstream revision/provenance and security admission are therefore still incomplete for product use. Bundled notices inspected: PDFium BSD-style terms plus additional notices, binary packaging MIT, dependency license inventory. No commercial distribution clearance is implied.

Execution: run probe.py using the bundled Python executable, with artifacts/pdfium-text-edit-probe as argument. Each run creates a unique folder. Only self-created fixtures are opened. A separate process was used; that does not constitute a security sandbox. No network-isolation test was performed, although the script makes no network requests.

Latest evidence: artifacts/pdfium-text-edit-probe/20260918-094143-541289/results.json and PNG/PDF fixtures in the same directory.

## Findings

- Direct FPDFText_SetText changed the existing Helvetica object from 'Status pompa: perlu perbaikan' to 'Status pompa: sudah diperbaiki'.
- Object count remained 5: no new overlay/white rectangle. Content was regenerated and saved to a new file.
- Independent pypdf extraction found the new string and not the old string.
- Independent Poppler rendering found changes only on the target line (pixel bounds 217,193,371,214 at 96 DPI). The second/control page was pixel-identical.
- Source hashes unchanged. Neighbor text retained. Basic rendered output inspected visually.
- Embedded subset case: replacing ABBA with Omega + CDEF returned API success, but extraction returned y-diaeresis + CDEF rather than the requested character. The rendered result was inspected. This is a fidelity failure, not a successful edit. Application must detect unsupported glyphs and reject or offer explicit font replacement, then validate again.
- 12 operational assertions passed; the separate subset fidelity probe intentionally reported a limitation. Do not summarize this as all editing tests passed.

## Still required

Exact product binary admission, native C# binding test, more font/encoding/rotation/crop/nested-object cases, long-text overlap checks, lifecycle/cancel/crash checks, signed/encrypted safeguards, and full native UI/UX audit. Do not enable the catalog Edit feature based on this probe alone. No OCR, redaction or secure deletion proof is provided.

Earlier attempts failed on folder permissions and the tooling DLL path before edits; those evidence folders were retained. An earlier ASCII-only subset test passed but did not establish new-glyph support. The latest Omega test is authoritative for this limitation.
