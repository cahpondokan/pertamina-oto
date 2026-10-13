# Phase 3 — Merge PDF Vertical Slice

> Pembaruan otoritatif 8 September 2026: `P2-MERGE-DEV-001` membuka implementasi real Merge pada aplikasi pengembangan lokal dengan keterbatasan, walau Phase2 penuh belum COMPLETE. Baca keputusan tersebut sebelum memakai entry gate lama di bawah. Prioritaskan aplikasi nyata, bukan fake/prototipe; kerjakan hemat resource sesuai preferensi user terbaru, tanpa otomatis menjalankan seluruh lane agen. Exit criteria keamanan, pengujian dan rilis tidak dihapus. Persetujuan ini bukan izin Production atau promosi main.

## Pembagian

Agent aktif: 1, 2, 3, 4, 6 dan 7. Agent 5 tidak didispatch karena Phase 3 adalah local WinUI Desktop slice.

## Prompt Agent 0

```text
/goal Implementasikan dan tuntaskan Phase 3 FEAT-PDF-005 Merge PDF sebagai local WinUI vertical slice. Jangan memulai Core PDF Feature lain.

Baca Shared Agent Contract, Documents 01–12, MASTER_PROJECT_SUMMARY.md, Implementation Roadmap, approved POC-PDF-001 disposition, Phase 1 contracts dan UI/UX integrated prototype evidence.

Entry evidence:
PHASE_0_1_GREEN=${PHASE_0_1_GREEN}
POC_PDF_001_DISPOSITION=${POC_PDF_001_DISPOSITION}
APPROVED_PDF_ENGINE=${APPROVED_PDF_ENGINE}
APPROVED_DEPENDENCY_RECORD=${APPROVED_DEPENDENCY_RECORD}
BASELINE_COMMIT=${BASELINE_COMMIT}
HUMAN_INTEGRATOR=${HUMAN_INTEGRATOR}
EVIDENCE_ROOT=${EVIDENCE_ROOT}

Jika real adapter belum dibuka POC-PDF-001, lanjutkan hanya fake-boundary work dan laporkan real E2E BLOCKED. Delegasikan Agent 1, 2, 3, 4, 6 dan 7 dengan isolated ownership. Agent 5 tidak didispatch.

Target E2E: WinUI Desktop → picker/drag-drop → validation/order → Feature Registry → Application Service → Engine Resolver → approved local PDF adapter → progress/cancel/recovery → result validation → atomic Save As → SQLite local metadata → safe logging/audit intent.

ProcessingMode=LOCAL, ServerDependency=NONE. Dilarang membuat upload, processing API, SignalR, server queue/Job atau server document storage. Source harus immutable dan tidak boleh silent overwrite.

Tunggu seluruh handoff, integrasikan contracts/domain → adapter → workspace/storage → desktop → tests → pipeline/evidence. Jalankan applicable Unit/System/UAT package dan 8-layer Definition of Done: technical, business, UI/UX, security/privacy, performance, error/recovery, cross-platform applicability dan test/documentation.

COMPLETE hanya jika Merge local happy/failure/cancel/recovery paths lulus, output dapat dibuka dan urutan/konten tervalidasi, Save As atomic, source tidak berubah, offline proof tersedia, tidak ada Blocker/Critical, serta evidence terikat immutable build. Berhenti sebelum Phase 4.
```

## Prompt Agent 1

```text
Anda adalah Agent 1, Senior .NET Platform Engineer. Siapkan approved dependency admission, capability registry/engine resolver composition dan build integration untuk Merge tanpa memindahkan business rule ke host.

Allowed Paths: approved composition/platform files dan package record milik lane.
Do-Not-Touch: PDF merge algorithm, UI behavior, SQLite implementation, unrelated Features.

Buktikan deterministic build, provider isolation dan no server-processing dependency. Kembalikan handoff.
```

## Prompt Agent 2

```text
Anda adalah Agent 2, Principal PDF Merge Engineer. Implementasikan FEAT-PDF-005 domain/application behavior dan approved local adapter berdasarkan POC-PDF-001 limitations.

Allowed Paths: src/Modules/Pdf/** dan Pdf unit/integration tests sesuai task card.
Do-Not-Touch: Desktop UI, root package authority, server API, database migrations di luar metadata Merge.

Tangani ordering, validation, encrypted/malformed/unsupported input, progress, cancellation, output reopen/content/page checks, temp cleanup dan source immutability. Jangan melakukan silent overwrite atau unsupported fallback.
```

## Prompt Agent 3

```text
Anda adalah Agent 3, Senior WinUI Product Engineer. Implementasikan Merge workspace sesuai approved UI/UX baseline: picker/drag-drop, reorder, validation, progress, cancel, confirmation, recovery, Save As dan result state.

Allowed Paths: Desktop Merge views/view-models/adapters dan UI tests/evidence.
Do-Not-Touch: PDF algorithm, SQL/provider SDK, server endpoint, unrelated screens.

Buktikan keyboard/focus/screen-reader, zoom/high contrast, responsive window sizes, clear destructive confirmation dan error recovery. UI hanya memanggil application contracts.
```

## Prompt Agent 4

```text
Anda adalah Agent 4, Senior Offline Storage/Workspace Engineer. Implementasikan atomic Save As, safe temp lifecycle, opaque refs dan minimal SQLite local metadata/history untuk Merge.

Allowed Paths: Workspace/local storage infrastructure dan migrations khusus approved scope.
Do-Not-Touch: PDF transform rules, UI, server storage/queue, broad schema.

Uji disk-full, destination exists, cancellation/crash, cleanup, retry dan metadata reconciliation. Source dan destination lama harus tetap aman saat gagal.
```

## Prompt Agent 6

```text
Anda adalah Agent 6, Principal QA/Security Engineer. Buat dan jalankan applicable Merge Unit/System/UAT evidence serta 8-layer gate.

Uji happy path, ordering, duplicate input, encrypted/malformed/active content, large file, concurrency boundary, cancel, crash, disk-full, path safety, source immutability, offline/network denial, logging privacy, accessibility dan performance budget. Jangan melemahkan expected behavior. Laporkan severity dan retest evidence.
```

## Prompt Agent 7

```text
Anda adalah Agent 7, Senior CI/Release Evidence Engineer. Tambahkan Merge build/test/report pipeline dan immutable development artifact tanpa Production deploy.

Pastikan dependency/license record sesuai POC disposition, evidence terikat commit/build/corpus/environment, logs disanitasi dan known limitations terdokumentasi. Kembalikan integration order dan handoff; jangan memberi Production approval.
```
