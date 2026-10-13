# Word to PDF — local development handoff

Status: IMPLEMENTED_AND_VERIFIED for the bounded DOCX A4 portrait profile, 1 October 2026. This is a local development capability, not Phase 5 completion or Production approval. Baseline HEAD: 84cbb0f538020003bcfb6583e94c2fcf6524f87c, branch task/core-functional-first. Existing dirty changes were preserved. No subagents, new provider download, installation, purchase, Word binary redistribution, deployment or central phase-record mutation.

## Authority and traceability

The user directly answered “izinkan” in this chat after the exact installed-Word development/synthetic-test admission and parent-report permission request. This permits the bounded local evaluation described here. The canonical P5-D01 register remains OPEN; this handoff does not forge Product/Technical/Security sign-off or waive release gates. Exact mapping: FEAT-CONV-001, UAT-CORE-011, UAT-TO-002. Evidence below is technical development evidence; human UAT sign-off remains pending.

The installed provider updated during evaluation. Current admitted executable: C:/Program Files/Microsoft Office/Root/Office16/WINWORD.EXE, version 16.0.20430.20092, SHA256 EEE6DEFCCCF12743FC44B5118AC6511359E851029D9E1C1F085DFEF6E4A8FA99, Authenticode Valid Microsoft Corporation. Runtime checks the exact hash and registered path, failing closed when absent or changed. A subsequent Office update needs review and explicit admission before updating the pin. Existing Word activation/licensing remains an installation prerequisite; executable presence and successful export do not certify entitlement. No Word binaries are packaged.

## Implementation and owned paths

New src/Workers/Pertamina.Oto.WordPdf.Worker: native fixed-format export through task-owned Word, bounded OOXML preflight, hash-pinned provider, Windows job isolation, fixed safe protocol/stage messages and synthetic self-tests. No package dependency added.

New desktop WordPdfClient.cs, WordWorkspace.cs and DEBUG WordPdfAudit.cs: supervised worker, single local DOCX picker/paste/drop, progress/cancel, safe result publication, history and internal PDF preview. Minimal Word routing edits to MainWindow.cs, SourceWorkspace.cs, EmptyToolScreens.cs, WorkspaceFlow.cs, AutomaticOutputName.cs, ToolCatalog.cs, Program.cs and desktop csproj. Shared reader implementation remains owned by its earlier task. eng/Build-LocalUpdate.ps1 now requires 27 Word worker checks and 39 native UI checks before publishing the local update. eng/WordPdfProbe is historical development ownership exploration, not shipped provider code.

Source stays read-only and locked, copied into unique local staging, with no overwrite. PDF is checked for page count and every page is reopened/rendered by the independent Windows PDF renderer before atomic publication. Existing output conflicts use unique names. Failure/cancel/timeout removes owned staging and leaves source/final output intact; cleanup failure is disclosed. One task-owned /a Word process inhibits global templates/add-ins and locks setting files. Ownership is proven from PID and native Word window before COM interaction. Never attach to a user's active COM instance. Job close kills owned Word only, including stuck COM; supervisor and Word have separate memory limits and a two-minute limit. Macros, links/fields and automatic spelling/grammar/link updates are disabled before opening; no saving of source, Normal template, user documents or recent-file entries.

OOXML preflight rejects remote relationships, traversal, duplicates, DTD/entities, macros, ActiveX, OLE, altChunk, embedded fonts, executable content, unknown fields, protection, disguised images and unsupported MIME. ZIP/XML/image/page/size limits fail closed. The accepted profile is intentionally narrower than all DOCX files.

## Verified evidence

Worker self-test: 27 PASS, nonzero discovery; corpus and checks.txt at C:/Users/cahpo/AppData/Local/Temp/oto-word-tests-28d49308839a44f09aece422e2670d00. Checks include native two-page export, immutable source hash, actual PDF signature, unsafe OOXML rejection, missing/changed provider rejection, and a separate Word document whose content and unsaved state remain unchanged while conversion runs. No leaked task PID.

Native UI audit: 39 PASS, exit 0, artifacts/word-pdf/word-audit-db001b519787447da51526ca4dc0e461/audit.txt. Screenshots: empty-intake.png, docx-workspace.png, result-panel.png, internal-result-preview.png. Independent page-1.png/page-2.png visually checked: Indonesian text, table, embedded image, repeated header and changing PAGE footer retained. Searchable original text and A4 geometry confirmed. Tests cover intake/accessibility, real ExecuteAsync, automatic output naming, history, result preview navigation/zoom, existing output protection, locked/malformed source, pre-start and in-flight cancel, timeout, missing helper, source hash and no staging or PID residue.

Header/footer text extraction in the existing search engine can be partial. Independent page rendering and visual inspection, rather than fabricated full search coverage, are the fidelity oracle. Exact crop pixels may differ from antialiasing; the audit requires nonempty corresponding header bands within a bounded pixel tolerance and different page-number footer bands.

## Limitations and pending gates

Only one local DOCX, A4 portrait with explicit page size. DOC/DOCM, A3, Letter and landscape are rejected before Word because their export was not reliably verified in this evaluation. Complex unsupported OOXML, encrypted documents, active fields and external hyperlinks/resources are rejected. Fonts can substitute depending on installed fonts; check the final PDF. No whole-document rasterization or artificial quality toggle, no PDF/A claim.

Processing uses local files/native Word and uploads no document. A completely disconnected-network run has not been verified; Office activation may periodically require connection. Do not present this evidence as universal zero-egress or unlimited offline licensing proof. Release needs approved provider/entitlement disposition, disconnected-run evidence, broader representative fidelity/security review and human UAT. No canonical phase status was promoted.

## Provider references

- https://learn.microsoft.com/en-us/office/vba/api/word.document.exportasfixedformat
- https://learn.microsoft.com/en-us/office/vba/api/word.application.automationsecurity
- https://support.microsoft.com/en-us/office/command-line-switches-for-microsoft-office-products-079164cd-4ef5-4178-b235-441737deb3a6
- https://support.microsoft.com/en-us/visio/considerations-for-server-side-automation-of-office

## Delivered local update

Build-LocalUpdate.ps1 exited 0. Product 0.1.0, Pengembangan; immutable local package 20261001-153307-07f0be4a (380 files), manifest SHA256 2A195972778937DE5FC4E91BB84E3E9F6C809C15FC5946828091069F4334FD99. Feed: D:/AFT Ahmad Yani Semarang/Teknik/Dokumentasi Pertamina OTO/releases/windows-local/latest.json. Use Periksa update in the existing app; no automatic installation or shortcut mutation was performed.

Build evidence: artifacts/update-build-56b35b6209b149d9b1baff246423500e. Word UI audit word-audit-bd9b2a0905834bd996bee7615e7c59cd: 39 PASS, zero FAIL. Quality audit quality-audit-c5e9da9e785c46d5a9bd8af6d8c29d43: 39 PASS, zero FAIL. Final packaged worker corpus C:/Users/cahpo/AppData/Local/Temp/oto-word-tests-56540c46bc22478a82bc69b56913e6c5: 27 PASS. Other gates: PDF worker 41, editor 63, search 25, distribution 62 plus updater 13, product-version 25 plus legacy updater 13, inert product-package fixture 8, PDF association exit 0. Build exited 0; no new provider package installed. Final source-only cleanup removes trailing blank lines and does not change the built behavior.

## Installed Word re-admission — 11 October 2026

During local centered-home update packaging, the mandatory Word self-test failed closed because Office updated. The user explicitly authorized evaluation of the new Word binary and continuation of local packaging in this chat. Registered path (both registry views): `C:/Program Files/Microsoft Office/Root/Office16/WINWORD.EXE`; version `16.0.20430.20146`; SHA-256 `A02EC8E28E1210A13F1C8D1874C9F10D6ED987A18365CBAAE72D8403BA2D906D`; Authenticode `Valid`, signer Microsoft Corporation. This supersedes the earlier development hash admission above. Only the exact provider pin changes; no Word binaries are redistributed. Canonical release/UAT approvals remain separate. Build and synthetic regression evidence: `artifacts/home-centered-01a12669/local-package-*.log` and `local-update/`.
