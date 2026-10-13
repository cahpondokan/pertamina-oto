# PERTAMINA OTO — Project Execution Framework Phase 0–11

**Versi:** 1.0  
**Tanggal baseline:** 2026-09-05  
**Pemilik operasional:** Agent 0 / Human Project Owner  
**Status proyek saat kerangka dibuat:** dokumentasi dan rencana implementasi tersedia; coding formal masih menunggu entry gate Phase 0.

## 1. Tujuan

Kerangka ini mengubah dokumen kebutuhan, desain, pengujian, deployment, dan handover menjadi proses eksekusi yang dapat dipanggil dengan satu perintah pendek. Setiap phase mempunyai entry gate, pembagian Agent 0–7, paket kerja, verification, exit evidence, dan kondisi berhenti yang tetap.

Kerangka ini tidak mengubah isi kebutuhan 01–19. File prompt di `docs/implementation-prompts/` tetap menjadi instruksi rinci tiap agen; dokumen ini mengatur lifecycle proyek dan hubungan antar-phase.

### Profil delivery yang dipilih pengguna

1. Rilis pertama memprioritaskan **Windows Desktop offline-first**.
2. Core PDF, conversion/OCR yang feasible, edit/security, dan intelligence yang mempunyai approved local runtime dimaksimalkan tanpa ketergantungan server.
3. Source document untuk local processing tidak di-upload; local capability tetap berjalan ketika jaringan tidak tersedia.
4. Hostinger KVM 4 dapat menjadi environment sementara untuk Git/CI, distribusi internal, Dev/QA/Pilot, atau server capability opsional, tetapi tidak menjadi dependency Core offline.
5. Android/Web hanya menerima curated capability setelah Core offline stabil.
6. iOS tidak dibuat dalam release scope pertama dan berstatus `DEFERRED` sampai controlled scope change disetujui.
7. Azure tetap target migrasi setelah pihak IT menyetujui topology dan governance; portability dijaga melalui contracts/adapters, externalized configuration, reproducible artifacts, dan migration evidence.

## 2. Definisi selesai

Kemajuan proyek dihitung berdasarkan gate, bukan perkiraan persentase subjektif:

- Sebuah phase selesai hanya bila seluruh exit gate mandatory berstatus PASS dan buktinya tercatat.
- Proyek belum 100% hanya karena seluruh fitur telah dikodekan.
- **100% berarti Phase 0–11 semuanya `COMPLETE`, exact approved release sudah live, verifikasi Production lulus, handover diterima, dan closure ditandatangani manusia yang berwenang.**
- Phase 10 berarti siap dipertimbangkan untuk Production, bukan sudah diluncurkan.
- Phase 11 adalah satu-satunya phase yang mencakup launch Production dan closure.

## 3. Model organisasi

| Identitas | Peran tetap | Akuntabilitas utama |
|---|---|---|
| Agent 0 | Project Manager, Principal Analyst/Programmer, Master Integrator | Gate, scope, task card, dispatch, audit, integrasi, verification, status, laporan |
| Agent 1 | Platform/Architecture/Build Specialist | Repository, .NET platform, composition, shared runtime, reproducible build |
| Agent 2 | Domain/PDF/Application Specialist | Domain rules, PDF engine/use case, product correctness |
| Agent 3 | Desktop UI/UX/Accessibility Specialist | WinUI shell/workspace, design system, responsive UX, accessibility, manuals |
| Agent 4 | Data/Storage/API/Processing Specialist | Local storage, jobs, conversion/OCR, API, data, migration, recovery |
| Agent 5 | Mobile/Web/Device Specialist | Android/Web exposure dan device/browser; iOS hanya setelah scope dibuka kembali |
| Agent 6 | Independent QA/Security/Performance Specialist | Test, threat review, security, accessibility, performance, reliability |
| Agent 7 | CI/CD/Governance/Evidence/SRE Specialist | Pipeline, license/evidence, artifacts, operations, rollout/handover |

Agent 0 ditambah Agent 1–7 berarti delapan identitas. Istilah “tujuh agen pelaksana” selalu merujuk Agent 1–7, dengan Agent 0 sebagai integrator. Concurrency aktual dapat lebih kecil; Agent 0 wajib menggunakan wave dan tidak boleh mengklaim agen bekerja paralel bila mereka tidak benar-benar didispatch.

## 4. Lifecycle satu perintah phase

| Tahap | Aksi Agent 0 | Output minimum |
|---|---|---|
| 1. Resolve | Petakan `phase N` ke file prompt, status, dan keputusan | Nomor/nama phase terkonfirmasi |
| 2. Preflight | Audit entry gate, urutan, repo, toolchain, access, baseline | Laporan GO/BLOCKED dan bukti |
| 3. Plan | Turunkan exact scope menjadi task card dan dependency wave | Daftar task, owner, path, acceptance |
| 4. Dispatch | Jalankan hanya agen aktif sesuai kapasitas aktual | Task/agent ID atau bukti dispatch |
| 5. Supervise | Pantau blocker, cegah overlap, jawab temuan berbasis sumber | Checkpoint dan keputusan |
| 6. Integrate | Tunggu seluruh handoff, audit, gabungkan menurut dependensi | Integrated candidate yang dapat diuji |
| 7. Verify | Jalankan checks phase dan kumpulkan evidence | Test/build/audit result |
| 8. Close | Nilai exit gate dan update catatan pusat | `COMPLETE`, `BLOCKED`, `FAILED`, atau `PAUSED` |
| 9. Stop | Berikan laporan dan perintah phase berikutnya | Tidak ada auto-start Phase N+1 |

## 5. Aturan wave dan ownership

1. Agent 0 membuat dependency graph sebelum dispatch.
2. Pekerjaan read-only dan path terpisah boleh paralel.
3. Shared contracts, root build files, schema, lockfile, dan catatan pusat hanya mempunyai satu writer pada satu waktu.
4. Wave berikutnya tidak dimulai bila input dari wave sebelumnya belum lulus acceptance.
5. Semua agen aktif harus ditunggu sebelum integrasi final. Handoff tanpa evidence ditolak.
6. Bila slot hanya memungkinkan tiga agen spesialis bersamaan, Agent 0 menjalankan contoh urutan: Wave A tiga agen, audit/integrasi checkpoint, Wave B tiga agen, Wave C agen terakhir. Identitas peran tetap sama.
7. Perubahan besar tidak boleh digabung “secara mental”; integrasi harus terlihat pada diff/artifact yang sama dan diuji sesudah penggabungan.

## 6. Kerangka rinci Phase 0–11

### Phase 0 — Repository Bootstrap

**Tujuan:** membentuk repository resmi, toolchain terkunci, solution/project graph minimum, test harness, CI pull-request, dan workflow multi-agent yang dapat direproduksi.  
**Agen aktif:** Agent 1, 2, 6, 7; Agent 0 mengintegrasikan.  
**Entry gate:** baseline/roadmap disetujui manusia; repo/project/default branch resmi diketahui; Product Owner, Technical Owner, dan Repository Integrator manusia ditetapkan; akses developer/reviewer tersedia; exact .NET SDK/build agent dan NuGet sources/dependencies disetujui; kewenangan branch protection/PR/pipeline tersedia.

Paket kerja:

- BOOT-001: repository, ignore/attributes/editor config, ownership, branch/worktree convention.
- BOOT-002: exact SDK, package sources, restore policy, dependency admission.
- BOOT-003: root solution/build, warnings/analyzers, deterministic baseline.
- BOOT-004: minimal approved project graph dan boundary canary tanpa Feature produk.
- BOOT-005: Master Test Plan skeleton dan test projects.
- BOOT-006: architecture, contract, security, secret, dependency, dan static-analysis gates.
- BOOT-007: PR-only CI Restore → Build → Unit → Architecture → Contract → Audit/Scan/Analysis.
- BOOT-008: task-card, isolated branch/worktree, handoff, review, dan evidence workflow.

**Exit evidence:** clean restore/build/test dari clone/worktree baru; exact SDK/dependency manifest; project graph sesuai desain; PR CI hijau; security/secret/dependency gates aktif; workflow Agent 0–7 terdokumentasi; tidak ada fitur atau Production deployment.  
**Stop:** setelah bootstrap PASS. Perintah berikutnya: `kerjakan phase 1 sesuai rencana`.

### Phase 1 — Architecture Foundation

**Tujuan:** mengimplementasikan fondasi arsitektur dan contract yang dapat dieksekusi, termasuk vertical skeleton Merge dengan fake processing boundary.  
**Agen aktif:** Agent 1, 2, 3, 4, 6, 7.  
**Entry gate:** Phase 0 COMPLETE; baseline build/CI bersih; project graph dan ownership disetujui.

Paket kerja:

- BuildingBlocks: Contracts, Security, Observability, Storage.Abstractions, Processing.Abstractions.
- Domain/application skeleton FEAT-PDF-005 tanpa real engine.
- Minimal Desktop shell yang hanya mengonsumsi application contracts.
- Opaque file references, atomic publication port, workspace metadata, local job boundaries.
- Executable architecture, contract, security, dan failure-path tests.
- CI/evidence diperluas untuk project graph Phase 1.

**Exit evidence:** dependency direction dan boundary tests PASS; fake Merge flow dapat dijalankan; UI tidak mengandung business rule; storage/output contract aman; full CI hijau.  
**Stop:** jangan memasukkan real PDF provider atau mobile/web. Berikutnya Phase 2.

### Phase 2 — Proof of Capability

**Tujuan:** membuktikan kelayakan engine/provider berisiko sebelum feature implementation.  
**Agen aktif:** Agent 1–7.  
**Entry gate:** Phase 1 COMPLETE; daftar PoC, corpus non-sensitif, evaluation rubric, license/security owner, dan candidate/version tersedia.

Paket kerja:

- Wave 0 POC-PDF-001 sebagai blocker real Merge adapter.
- PDF engine, page/render/edit/repair feasibility.
- Conversion Office/HTML/image/PDF dan OCR Indonesia/Inggris.
- Visual/structural/semantic fidelity oracle.
- Scanner/device, cross-platform, dan local-AI feasibility yang disetujui.
- Security, malformed-file, resource, concurrency, timeout, privacy, dan performance tests.
- Provenance, license, redistribution/runtime/support/cost review.

**Exit evidence:** setiap candidate mempunyai reproducible harness, version/hash, corpus manifest, measurement, limitation, risk, license disposition, dan keputusan `APPROVE`, `APPROVE_WITH_LIMITS`, `REJECT`, atau `DEFER` oleh owner berwenang.  
**Stop:** jangan memilih vendor/provider secara sepihak. Berikutnya Phase 3 setelah POC-PDF-001 membuka real adapter.

### Phase 3 — Merge PDF Vertical Slice

**Tujuan:** menghasilkan satu vertical slice desktop offline yang lengkap untuk FEAT-PDF-005 Merge.  
**Agen aktif:** Agent 1, 2, 3, 4, 6, 7.  
**Entry gate:** Phase 2 COMPLETE untuk POC-PDF-001 dan dependency yang dipilih; exact Feature/Requirement/Test mapping; design foundation tersedia.

Paket kerja:

- Dependency admission, registry/resolver, dan build composition.
- Merge domain/application dan approved local PDF adapter.
- WinUI workspace: picker/drag-drop, reorder, validation, progress, cancel, recovery, Save As, result.
- Atomic Save As, temp lifecycle, opaque refs, SQLite history minimum.
- Unit/System/UAT evidence, malformed/encrypted/large input, cancellation, source integrity.
- Development artifact dan traceability di CI.

**Exit evidence:** real end-to-end Merge lulus golden/corpus test; source tidak berubah; output atomic dan dapat dibuka; failure/cancel/recovery aman; UI/accessibility lulus; full regression hijau.  
**Stop:** jangan memperluas ke seluruh katalog PDF. Berikutnya Phase 4.

### Phase 4 — Core Local PDF Operations

**Tujuan:** memperluas pola vertical slice ke subset Core local PDF yang telah disetujui.  
**Agen aktif:** Agent 1, 2, 3, 4, 6, 7.  
**Entry gate:** Phase 3 COMPLETE; exact ID mapping dan operation semantics disetujui; resource budget tersedia.

Paket kerja:

- Split, remove/extract pages, organize, compress, repair, rotate, dan compare.
- Capability registry/composition dan bounded concurrency.
- Workspace UI konsisten dengan design system Merge.
- Metadata/history/temp/atomic output/recovery per operasi.
- Test matrix exact Feature ID, fidelity, performance, source safety, dan regression Merge.
- CI artifacts dan trace overlay.

**Exit evidence:** semua exact Core IDs yang discope mempunyai implementation, UI, test, trace, limitation, dan result safety; tidak ada klaim seluruh 550 Feature selesai.  
**Stop:** berikutnya Phase 5.

### Phase 5 — Core Conversion, Scan, and OCR

**Tujuan:** mengimplementasikan format family yang telah lulus PoC dan governance.  
**Agen aktif:** Agent 1–7.  
**Entry gate:** Phase 4 COMPLETE; PoC/provider/license/security disposition per format family; corpus dan quality thresholds disetujui.

Paket kerja:

- Isolated engine runtime, resolver, timeout, packaging, resource measurement.
- Image/PDF boundaries dan PDF/A conformance.
- Conversion/OCR/scan workspace dengan preview, options, confidence, limitations, cancel/recovery.
- Approved Office→PDF, PDF→Office, local HTML→PDF, image/PDF, OCR ID/EN adapters.
- Scanner/device boundary dan exposure feasibility tanpa duplikasi business rule.
- Fidelity, semantic, OCR accuracy, security, performance, dan dependency/license gates.

**Exit evidence:** setiap family yang dibuka memenuhi threshold, safety, provenance, license, packaging, fallback, dan exact trace; family yang belum approved tetap diblokir tanpa menghambat family independen.  
**Stop:** tidak ada macro/script/remote URL/hidden upload. Berikutnya Phase 6.

### Phase 6 — Core Edit and PDF Security

**Tujuan:** menyediakan operasi edit dan keamanan PDF berisiko tinggi dengan preview, authorization, dan audit yang benar.  
**Agen aktif:** Agent 1, 2, 3, 4, 6, 7.  
**Entry gate:** Phase 5 COMPLETE; controlled disposition untuk requirement/security trace; threat model dan semantics disetujui.

Paket kerja:

- Edit/forms/watermark/crop/page-number/unlock/protect/visual-sign/redact.
- Authorization context, reauthentication ports, protected-secret seams.
- Preview, confirmation, irreversible warning, recovery, dan limitation disclosure.
- Audit intent, secure metadata, atomic publication.
- Threat-led tests untuk permission, password/secret handling, true redaction, bypass, logging, dan source safety.
- High-risk CI gates dengan sanitized evidence.

**Exit evidence:** security semantics dan exact trace PASS; redaction terbukti menghapus informasi sesuai requirement; secret tidak bocor; destructive/high-risk action mendapat confirmation/reauth sesuai desain. Visual signing tidak diklaim sebagai PKI/digital signature.  
**Stop:** berikutnya Phase 7.

### Phase 7 — Core PDF Intelligence

**Tujuan:** menyediakan intelligence yang grounded, terukur, dan sesuai data boundary.  
**Agen aktif:** Agent 1, 2, 3, 4, 6, 7.  
**Entry gate:** Phase 6 COMPLETE; provider/model/version/license/privacy/retention/egress approved; corpus dan eval threshold tersedia.

Paket kerja:

- Provider abstraction, local/internal composition, package integrity, resource limits.
- Deterministic extraction, chunk/source mapping, grounded summary/translation/Markdown contracts.
- Consent/review UI, sources, comparison, warning, progress/cancel, edit/copy/export, recovery.
- Routing, retention/privacy enforcement, job metadata, audit intent.
- Factuality, omission, hallucination, translation, structure, prompt-injection, privacy, egress, resource tests.
- Model/provider governance dan CI eval gates.

**Exit evidence:** outputs dapat dilacak ke sumber, threshold eval lulus, data boundary terbukti, limitation jelas, dan provider mempunyai exit path. Jika provider belum approved, hanya deterministic extraction/fake provider yang boleh selesai.  
**Stop:** berikutnya Phase 8.

### Phase 8 — Android and Web Curated Capability; iOS Deferred

**Tujuan:** mengekspos subset capability yang disetujui ke Android/Web tanpa mengklaim full parity; iOS tidak termasuk rilis pertama.  
**Agen aktif:** Agent 1–7.  
**Entry gate:** Phase 7 COMPLETE; exact platform capability matrix; API/offline/sync/privacy decisions; device/browser matrix; signing/account/hosting authority sesuai target.

Paket kerja:

- Shared contracts/SDK/build targets tanpa memindahkan domain rules.
- Exact feature parity mapping dan platform adapters.
- Design tokens, responsive behavior, accessibility, desktop regression.
- Approved API/session/sync/conflict/cache/upload boundaries.
- Android/Web implementation, permissions, offline behavior, responsive layouts. iOS hanya mempertahankan contract compatibility dan tidak dibangun/dipublish.
- Device/browser/accessibility/security/privacy/performance/regression matrices.
- Multi-platform CI dan development artifacts, tanpa Production publishing.

**Exit evidence:** platform matrix dan limitations transparan; approved Android/Web capabilities lulus pada target devices/browsers; offline/sync/permissions aman; desktop tidak regress; iOS ditandai `DEFERRED/NOT_IN_RELEASE_SCOPE`, bukan `FAILED` atau `COMPLETE`.  
**Stop:** berikutnya Phase 9.

### Phase 9 — Hardening, UAT, and Pilot

**Tujuan:** membekukan scope, membangun immutable release candidate, menghilangkan blocker defect, melaksanakan UAT/pilot, dan membuktikan kesiapan release.  
**Agen aktif:** Agent 1–7.  
**Entry gate:** Phase 8 COMPLETE untuk release scope; scope/dependency freeze; QA/UAT/pilot plan, data, environment, user, approver, support, monitoring, dan rollback tersedia.

Paket kerja:

- Deterministic RC build, dependency manifest, SBOM.
- Defect-only fixes di frozen product scope.
- UI/accessibility/responsive labels dan user/admin manual alignment.
- API/data/offline/job/audit/recovery/performance hardening.
- Real-device/browser regression.
- Full functional, security, abuse, performance, recovery, dan UAT evidence.
- Immutable RC pipeline, known issues, pilot monitoring/support/rollback package.

**Exit evidence:** no unresolved Blocker/Critical; affected dan broad regression PASS; immutable RC digest tercatat; UAT/pilot acceptance manusia tersedia; known issues/residual risks mempunyai disposition.  
**Stop:** feature baru membutuhkan controlled change. Berikutnya Phase 10.

### Phase 10 — Production Readiness

**Tujuan:** membuktikan semua prasyarat Production terhadap immutable RC tanpa deploy live.  
**Agen aktif:** Agent 1–7.  
**Entry gate:** Phase 9 COMPLETE; exact immutable RC; Production topology, SSO/network/secrets/certificates/signing/data/migration/ops/support/change inputs dan owner diketahui.

Paket kerja:

- Reproducible build, configuration schema, install/upgrade/rollback prerequisites.
- Functional smoke catalog, failure/degraded modes, diagnostics.
- Desktop/mobile/web packaging, signing, distribution, staged rollout dan withdrawal rehearsal.
- Topology, network, identity, data/storage/queue, migration, backup/restore/DR rehearsal pada target-like environment.
- VA/PT/security, performance/capacity, recovery, residual risk assessment.
- Monitoring/alerts/dashboard/log/on-call/Service Desk/runbook/change/KT evidence.

**Exit evidence:** readiness checklist PASS, dry-run dan rollback rehearsal PASS, immutable RC tidak berubah, production inputs lengkap, residual risks disetujui manusia, Go/No-Go packet siap.  
**Stop:** Phase 10 tidak mengotorisasi deployment. Berikutnya Phase 11 hanya setelah explicit launch authorization.

### Phase 11 — Production Launch, Handover, and Closure

**Tujuan:** mempromosikan exact immutable RC melalui approved runbook, memvalidasi Production, menyerahkan operasi, dan menutup proyek.  
**Agen aktif:** Agent 1–7.  
**Entry gate:** Phase 10 COMPLETE; exact RC digest/target; approved change ticket/window; named deploy, rollback, business, security, data, operations, UAT, dan closure authority; communication/escalation/runbook tersedia.

Paket kerja:

- Artifact/digest/signature validation di setiap promotion checkpoint.
- Approved Production deployment dan staged channel rollout.
- Read-only/minimally mutating functional smoke menggunakan data approved.
- Desktop/mobile/web install/update/channel validation.
- Approved migration, reconciliation, backup checkpoint, rollback readiness.
- Security/reliability/privacy/monitoring observation dan incident routing.
- Knowledge transfer, access transfer, CMDB/cartography/manuals, support activation, handover, closure.

**Exit evidence:** exact RC live; Production smoke/reconciliation PASS; monitoring, alert, support, backup/rollback aktif; UAT/Production acceptance dan residual-risk sign-off diterima; KT/access/CMDB/cartography/manuals diserahkan; open items mempunyai accepted disposition; closure authority menandatangani penutupan.  
**Stop/rollback:** hentikan atau rollback saat threshold runbook tercapai. Kegagalan kode kembali ke Phase 9. Tidak ada agen yang boleh memalsukan sign-off manusia.

## 7. Gate keputusan manusia

Keputusan manusia disimpan di `PHASE_DECISION_REGISTER.md`. Agent 0 boleh menyiapkan evidence dan rekomendasi, tetapi status `APPROVED` hanya boleh diisi bila nama/role approver, tanggal, ruang lingkup, dan referensi bukti tersedia. Persetujuan lisan yang tidak dapat dilacak ditandai `UNVERIFIED`, bukan `APPROVED`.

## 8. Kebijakan blocker dan recovery

- **Sequence blocker:** phase sebelumnya belum COMPLETE; jangan lompat.
- **Authority blocker:** approval/credential/owner/change window tidak tersedia; lanjutkan analisis aman, jangan melakukan mutation yang membutuhkan authority.
- **Technical blocker:** toolchain/dependency/test gagal; isolasi cause, simpan diagnostics, dan jangan menutupi failure.
- **Contract blocker:** requirement/desain/implementasi bertentangan; hentikan downstream lane dan ajukan exact decision.
- **Capacity blocker:** slot agen terbatas; gunakan wave. Ini bukan alasan mengklaim pekerjaan semua agen telah berjalan.
- **External blocker:** provider/account/device/environment tidak tersedia; selesaikan fake/deterministic/independent work jika diizinkan prompt.

Setelah blocker diselesaikan, pengguna cukup memberi `lanjutkan phase N sesuai rencana`. Agent 0 memvalidasi ulang bukti, melanjutkan dari checkpoint, dan tidak mengulang pekerjaan yang sudah terbukti lulus.

## 9. Handoff dan audit minimum

Setiap handoff agen wajib memuat objective, exact scope/IDs, changed paths, commands/tests beserta hasil, evidence path, assumptions, decisions, risks, blockers, dan rekomendasi integration order. Agent 0 melakukan audit terhadap:

- scope dan do-not-touch compliance;
- overlap/conflict dan dependency direction;
- source/data/secret safety;
- license/provenance/package admission;
- test adequacy dan negative/failure paths;
- traceability Requirement → Feature → Design/Control → Code → Test → Evidence;
- dokumentasi/user-facing limitation;
- reproducibility dari clean environment.

## 10. Laporan penutupan phase

Agent 0 menerbitkan satu laporan ringkas yang dapat diaudit: status, entry gate, workstreams, agen/wave aktual, perubahan, verification, exit-gate matrix, evidence links, blocker/residual risk, keputusan manusia, serta perintah berikutnya. Jika exit gate PASS, perintah berikutnya selalu ditulis eksplisit, misalnya `kerjakan phase 1 sesuai rencana`.
