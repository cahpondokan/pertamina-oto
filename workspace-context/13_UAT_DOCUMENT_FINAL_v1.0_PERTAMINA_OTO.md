# 13 — DOKUMEN USER ACCEPTANCE TEST
## PERTAMINA OTO — Office Tools Organizer

> **Versi:** FINAL v1.0  
> **Tanggal:** 1 September 2026  
> **Status dokumen:** FINAL — PRE-CODING UAT BASELINE  
> **Status eksekusi:** Belum direkam; memerlukan release candidate dan sign-off manusia  
> **Prioritas:** 33 Primary PDF Tool Cards  
> **Vertical slice pertama:** `FEAT-PDF-005 — Merge PDF`  
> **Baseline:** 550 fitur canonical  
> **Bahasa:** Bahasa Indonesia — `ID-1.0`

---

# 1. Tujuan

Dokumen ini menetapkan cara membuktikan bahwa PERTAMINA OTO dapat diterima oleh pengguna dan proses bisnis. Keberhasilan bukan sekadar program tidak error; pengguna harus memperoleh hasil yang benar, mudah dipahami, aman, sesuai izin, dan dapat dipulihkan.

Status FINAL berarti **spesifikasi UAT telah final**, bukan klaim bahwa aplikasi, fitur, build, atau release candidate telah lulus UAT.

---

# 2. Ruang Lingkup

UAT mencakup proses bisnis, Triple Entry Flow, 33 Core, Merge PDF, UX, platform, offline/Local-First, selective sync, security user-facing, role, Assistant, Issue Reporting, error/recovery, evidence, defect, retest, risiko, dan sign-off.

UAT tidak mengulang Unit Test, API/contract test, SQL test, static analysis, penetration test, load test, atau detail fault injection. Evidence teknis Dokumen 11–12 menjadi entry gate.

Dokumen 01–12 FINAL adalah sumber kebenaran. Laporan Agent A–D adalah input integrasi, bukan sumber yang lebih tinggi.

---

# 3. Sasaran UAT

Setiap skenario menjawab tujuan user, kondisi awal, langkah user, expected result, forbidden outcome, kemudahan, kebenaran hasil, recovery, permission, dan penerimaan user.

Dimensi penerimaan: efektivitas, correctness, usability, transparency, control, safety, continuity, dan consistency.

---

# 4. Prinsip UAT

1. Business outcome lebih utama daripada technical green.
2. UAT dimulai setelah gate Dokumen 12 terpenuhi.
3. Evidence terikat immutable release candidate.
4. Kedalaman berbasis risiko: `D1-STANDARD`, `D2-ENHANCED`, `D3-CRITICAL`.
5. Tidak ada false green atau status sukses sebelum hasil final tervalidasi.
6. Source aman; default menghasilkan file baru.
7. No Silent Wait; status dan next action selalu jelas.
8. Local-only capability tidak dipaksa memakai server.
9. File/Tool/Assistant memakai aturan canonical yang sama.
10. Business acceptance hanya diberikan manusia berwenang.

| Delapan lapisan DoD | Perlakuan |
|---|---|
| Technical Correctness | Entry evidence Dokumen 11–12 |
| Security & Privacy | Entry evidence + perilaku user-facing |
| Performance | Entry evidence + persepsi pengguna |
| Test & Documentation | Entry evidence + traceability |
| Business Process Correctness | Fokus UAT |
| UI/UX Acceptance | Fokus UAT |
| Cross-Platform Behavior | Fokus UAT |
| Error & Recovery | Fokus UAT |

---

# 5. Peran UAT

| Actor/fungsi | Tanggung jawab | Batas |
|---|---|---|
| User | Own tool/file/workspace/History/secured file/Issue/Assistant | Tidak mengakses user lain/Audit enterprise |
| Administrator | Administrasi dalam delegated scope | Tidak otomatis membaca document content |
| Auditor | Audit read/query/export bila berizin | Tidak mutate event/content plane |
| Support/Superadmin | Issue/attachment/triage dalam exact scope | Permission profile, bukan role wildcard |
| UAT Decision Authority | Menetapkan hasil bisnis | Nama/delegasi harus dikonfirmasi |
| Coordinator/Tester | Menyiapkan dan menjalankan UAT | Fungsi proyek, bukan role aplikasi |
| QA/Developer/Reviewer | RC, evidence, fix, retest, risk review | Tidak self-approve tanpa authority |
| Codex | Draft/traceability/consistency | Bukan tester manusia atau approver |

Assistant bertindak atas nama User. User tetap Actor; Worker/Service adalah Executor.

---

# 6. Lingkungan

UAT memakai logical lane terkontrol dan terisolasi dari Production. Manifest mencatat lane, build/version/commit/pipeline/digest, deployment, config/policy/schema/capability, OS/device/browser/runtime/engine/provider, topology, identity/permission fixture, Dataset ID, owner, reset, recovery, dan support path.

Dilarang: auth bypass, shared credential, raw Production data, direct DB editing, manual binary patch, public AI, test pada Production, destructive fault pada system drive/workspace, atau secret pada evidence.

Topology fisik dan exact inventory mengikuti `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

---

# 7. Entry Criteria

```text
[ ] immutable RC mempunyai build/version/commit/pipeline/digest
[ ] artifact yang sama telah melewati controlled pipeline dan QA
[ ] deployment/configuration/migration/health smoke PASS
[ ] applicable gate Dokumen 12 PASS
[ ] critical security/recovery/compatibility/platform evidence tersedia
[ ] tidak ada Blocker atau Critical terbuka
[ ] known Major/Minor mempunyai scope, owner, target, disposition
[ ] scope, traceability, capability matrix, dan guidance siap
[ ] account/permission/policy/reauth/offline fixture siap
[ ] synthetic/sanitized data versioned dan resettable
[ ] evidence/defect/retest/support/escalation path siap
```

Mandatory item yang belum siap membuat skenario terkait `BLOCKED`. Exploratory review tidak boleh dilaporkan sebagai formal PASS.

---

# 8. Exit Criteria

```text
[ ] seluruh released/applicable business-critical dan Must/Core dieksekusi
[ ] D3, Triple Entry critical, dan Merge detailed scenarios PASS
[ ] 33/33 Core mempunyai hasil sah
[ ] tidak ada Blocker atau Critical terbuka
[ ] tidak ada Major yang merusak outcome/source/privacy/critical UX
[ ] governed known issues lengkap
[ ] exact retest dan affected regression PASS
[ ] platform/capability limitation eksplisit
[ ] Requirement → Feature → UAT → Evidence/Defect lengkap
[ ] evidence aman dan counts rekonsiliasi
[ ] UAT Summary dan human sign-off tercatat
[ ] Production approval terpisah
```

Tidak ada pass-rate persentase yang menggantikan mandatory outcome.

---

# 9. Data UAT

Urutan preferensi: synthetic/generated deterministic; sanitized representative yang disetujui; controlled real-like sample hanya jika synthetic tidak cukup dan governance mengizinkan.

Manifest mencatat Dataset ID/version, purpose, Requirement/Feature/UAT, provenance, classification, format/count/size/digest, expected property, owner/access, expiry, reset, dan disposal.

Password, token, secret, raw Production document/log/database/backup, real signature, clipboard, unrestricted OCR/AI/prompt, dan data pribadi tidak boleh menjadi evidence biasa.

---

# 10. Template UAT Case

```text
UAT ID dan Judul
Business Goal
Requirement ID
Canonical Feature ID atau governed resource
Actor; Platform/capability; Risk/depth
Release/build/digest; Environment/config version
Precondition; Test Data/Dataset ID
Langkah user
Expected Result dan Expected UX
Expected Security/forbidden side effect
Expected Recovery; Audit Expectation bila relevan
Acceptance Result: PASS / FAIL / BLOCKED / NOT_APPLICABLE
Evidence; Defect ID; Executor/reviewer/timestamp; Notes
```

Pola ID: `UAT-<AREA>-NNN`. UAT ID bukan Feature ID baru.

---

# 11. Status Penerimaan

| Status | Arti |
|---|---|
| `PASS` | Semua business/UX/security/recovery/forbidden assertions terpenuhi |
| `FAIL` | Minimal satu acceptance tidak terpenuhi |
| `BLOCKED` | Tidak dapat dijalankan karena dependency/decision/environment |
| `NOT_APPLICABLE` | Tidak berlaku pada approved scope dan mempunyai bukti |

Skenario belum dieksekusi tidak diberi hasil. `PASS WITH BUG` tidak dipakai. Minor issue boleh ditautkan pada PASS hanya jika acceptance tetap terpenuhi dan Known Issue Reference tercatat.

---

# 12. Keterkaitan Defect

Setiap FAIL menunjuk satu Defect ID yang memuat UAT/Requirement/Feature, build/digest, environment/platform/data, expected/actual, safe reproduction/evidence, severity, `DEF-P0..DEF-P3`, owner, fix version, retest, regression, dan disposition.

UAT defect adalah release-validation record. In-App Issue adalah Support workflow; satu Issue ID dapat ditautkan ke satu Defect ID setelah triage, tetapi enum/status keduanya tetap terpisah.

---

# 13. Skenario Proses Bisnis

| ID | Tujuan | Acceptance |
|---|---|---|
| `UAT-BIZ-001` | Menemukan tool utama | Tool yang diekspos ditemukan maksimal dua interaksi |
| `UAT-BIZ-002` | Memproses file lokal | Hasil usable; source aman; no server processing/upload |
| `UAT-BIZ-003` | Pekerjaan background | `202 Accepted` bukan Completed; Activity/result jelas |
| `UAT-BIZ-004` | Melanjutkan pekerjaan | History/Activity/retry memakai current state/permission |
| `UAT-BIZ-005` | Amankan hasil | Exact result, explicit transfer, maksimum tiga versi |
| `UAT-BIZ-006` | Laporkan masalah | Pesan mudah, screenshot opsional/explicit, status terlihat |
| `UAT-BIZ-007` | Bekerja offline | Local eligible jalan; server-only dijelaskan |
| `UAT-BIZ-008` | Pulih dari kegagalan | No silent loss/duplicate/false success |
| `UAT-BIZ-009` | Tindakan sensitif | Target/impact/confirmation/reauth jelas |
| `UAT-BIZ-010` | Lintas perangkat | Hanya explicitly secured result tersedia |

---

# 14. Skenario File-First

| ID | Skenario | Acceptance |
|---|---|---|
| `UAT-ENTRY-001` | File valid → recommendation → tool → result | Feature canonical dan rekomendasi relevan |
| `UAT-ENTRY-002` | Mixed valid/invalid | Item bermasalah jelas; aturan batch/atomic benar |
| `UAT-ENTRY-003` | File berpassword | Password masked, operation-only, tidak masuk History/evidence |
| `UAT-ENTRY-004` | Capability unavailable | Explain/fallback/jalan kembali; no dead-end |

Kata masukkan/pilih/drop bukan server upload. Content transfer memerlukan notice, consent, permission, dan explicit action.

---

# 15. Skenario Tool-First

| ID | Skenario | Acceptance |
|---|---|---|
| `UAT-ENTRY-005` | Search/select → input → parameter → process → result | Label/parameter/limit/result jelas |
| `UAT-ENTRY-006` | Local-only offline | Official local service/Worker; zero processing upload/API |
| `UAT-ENTRY-007` | Optional-server | Resolver/policy menentukan mode; transfer dijelaskan |
| `UAT-ENTRY-008` | Permission/exposure berubah | Final recheck; stale UI bukan authority; draft aman |

---

# 16. Skenario Assistant-First

| ID | Situasi | Acceptance |
|---|---|---|
| `UAT-AST-001` | Permintaan jelas | Feature canonical dan plan benar |
| `UAT-AST-002` | Ambigu | Klarifikasi; belum execute |
| `UAT-AST-003` | Destructive | Target/impact; confirmation/reauth tetap |
| `UAT-AST-004` | Unsupported | Unavailable/alternatif resmi; no invention |
| `UAT-AST-005` | Offline local-only | Official local path; no silent reroute/upload |
| `UAT-AST-006` | Offline mixed/server | Online-required/waiting; safe intent bila didukung |
| `UAT-AST-007` | Permission denied | Safe denial; no bypass |
| `UAT-AST-008` | Tool/provider failure | Actual status/recovery; no public AI fallback |
| `UAT-AST-009` | User correction | Plan diperbaiki; no duplicate job |
| `UAT-AST-010` | Grounding | Success hanya dari validated result/persisted state |
| `UAT-AST-011` | Instruksi di document/OCR | Diperlakukan sebagai data, bukan instruction |
| `UAT-AST-012` | Partial multi-step denial | Per-step authorization; no false full success |

File/Tool/Assistant wajib sama pada Feature, normalized input/order/parameter, permission/policy, confirmation, resolver, source/finality, result, History/Audit, dan error semantics.

---

# 17. Coverage 33 Core

| No | UAT ID | Tool / Feature ID | Depth | Acceptance utama |
|---:|---|---|---|---|
| 1 | `UAT-CORE-001` | Merge — `FEAT-PDF-005` | D1+detail | Semua page sesuai order; source utuh |
| 2 | `UAT-CORE-002` | Split — `FEAT-PDF-006/007` | D1 | Semua part sesuai range |
| 3 | `UAT-CORE-003` | Remove — `FEAT-PDF-013` | D1 | Target hilang; Undo jelas |
| 4 | `UAT-CORE-004` | Extract — `FEAT-PDF-014` | D1 | Hanya selection/order yang dipilih |
| 5 | `UAT-CORE-005` | Organize — `FEAT-PDF-010..016` | D1 | Working arrangement = final |
| 6 | `UAT-CORE-006` | Scan — `FEAT-SCAN-014` | D2 | Capture/retake/crop/reorder usable |
| 7 | `UAT-CORE-007` | Compress — `FEAT-PDF-023` | D1 | Size/quality jujur |
| 8 | `UAT-CORE-008` | Repair — `FEAT-PDF-025/026` | D2 | Usable result atau honest failure |
| 9 | `UAT-CORE-009` | OCR — `FEAT-OCR-004` | D2 | Known terms searchable/copyable |
| 10 | `UAT-CORE-010` | JPG→PDF — `FEAT-CONV-007` | D1 | Page/order/orientation benar |
| 11 | `UAT-CORE-011` | Word→PDF — `FEAT-CONV-001` | D2 | Main content/layout usable |
| 12 | `UAT-CORE-012` | PPT→PDF — `FEAT-CONV-003` | D2 | Slides lengkap/urut |
| 13 | `UAT-CORE-013` | Excel→PDF — `FEAT-CONV-002` | D2 | Sheet/print area benar |
| 14 | `UAT-CORE-014` | HTML→PDF — `FEAT-CONV-017` | D2 | Safe input; usable output |
| 15 | `UAT-CORE-015` | PDF→JPG — `FEAT-CONV-012` | D1 | Image count/order/resolution benar |
| 16 | `UAT-CORE-016` | PDF→Word — `FEAT-CONV-004` | D2 | Target content editable |
| 17 | `UAT-CORE-017` | PDF→PPT — `FEAT-CONV-006` | D2 | Slides ordered/usable |
| 18 | `UAT-CORE-018` | PDF→Excel — `FEAT-CONV-005` | D2 | Tables/cells benar, no formula surprise |
| 19 | `UAT-CORE-019` | PDF/A — `FEAT-PDF-075` | D2 | Claimed profile terbukti |
| 20 | `UAT-CORE-020` | Rotate — `FEAT-PDF-012` | D1 | Hanya selected pages berubah |
| 21 | `UAT-CORE-021` | Page Numbers — `FEAT-PDF-037` | D1 | Range/position/number tepat |
| 22 | `UAT-CORE-022` | Watermark — `FEAT-PDF-036` | D1 | Page/position/opacity tepat |
| 23 | `UAT-CORE-023` | Crop — `FEAT-PDF-019` | D1 | Crop tepat tanpa hidden loss |
| 24 | `UAT-CORE-024` | Edit — `FEAT-PDF-031..041` | D2 | Supported edit saved; limitation jelas |
| 25 | `UAT-CORE-025` | Forms — `FEAT-PDF-053..060` | D1 | Field values tetap setelah reopen |
| 26 | `UAT-CORE-026` | Unlock — `FEAT-PDF-062` | D3 | Hanya authority/password sah |
| 27 | `UAT-CORE-027` | Protect — `FEAT-PDF-061` | D2 | Protection terbukti saat reopen |
| 28 | `UAT-CORE-028` | Sign — `FEAT-SIGN-013` | D2 | Mark visual tepat; jelas non-PKI |
| 29 | `UAT-CORE-029` | Redact — `FEAT-PDF-069` | D3 | Content hilang nyata, bukan overlay |
| 30 | `UAT-CORE-030` | Compare — `FEAT-PDF-071/072` | D1 | Difference/provenance dipahami |
| 31 | `UAT-CORE-031` | Summarizer — `FEAT-INTEL-001` | D2 | Grounded; no material invention |
| 32 | `UAT-CORE-032` | Translate — `FEAT-INTEL-002` | D2 | Meaning/critical terms benar |
| 33 | `UAT-CORE-033` | PDF→Markdown — `FEAT-CONV-036` | D2 | Valid Markdown; structure jelas |

Seluruh row mewarisi source safety, final validation, privacy-safe evidence, dan History/Audit rule. PoC-gated Feature tidak dapat PASS dengan fake/generic output.

---

# 18. UAT Detail Merge PDF

**Feature:** `FEAT-PDF-005` — mode lokal; tanpa server processing API/SignalR.  
**Goal:** menggabungkan ≥2 PDF menjadi output berurutan, Save As, source tetap aman saat normal/offline/cancel/failure/recovery.

Dataset: A=`A1,A2`; B=`B1`; C=`C1,C2,C3`; BAD=invalid PDF; ENC=password sintetis; LARGE=approved large corpus.

| ID | Fokus | Acceptance |
|---|---|---|
| `UAT-MRG-001` | Picker A,B,C; reorder C,A,B; remove/Undo; process; Save As; History | Output `C1,C2,C3,A1,A2,B1`; source utuh; reopen berhasil |
| `UAT-MRG-002` | Drag A,B | Setara picker; no unintended duplicate |
| `UAT-MRG-003` | Zero/one input | Minimal dua dijelaskan; no process |
| `UAT-MRG-004` | Invalid item | Item jelas; no partial Merge; final list ≥2 valid |
| `UAT-MRG-005` | Wrong/correct password | Safe deny/allow; password tidak persist |
| `UAT-MRG-006` | Initial order | Visible; no silent sort |
| `UAT-MRG-007` | Reorder | Preview/list/result konsisten |
| `UAT-MRG-008` | Remove/Undo | Working state tepat; source tidak delete |
| `UAT-MRG-009` | Explicit duplicate | Visible dan dipertahankan; no silent dedupe |
| `UAT-MRG-010` | Normal local process | No processing upload/API |
| `UAT-MRG-011` | Progress LARGE | No silent wait/fake ETA; cancel bila aman |
| `UAT-MRG-012` | Cancel queued | Cancelled; no final output |
| `UAT-MRG-013` | Cancel running | Safe point; temp bukan final |
| `UAT-MRG-014` | Validate result | Semua marker lengkap/urut/terbaca |
| `UAT-MRG-015` | Save As | Exact selected location/name; reopen |
| `UAT-MRG-016` | Existing target | No silent overwrite; safe choice |
| `UAT-MRG-017` | Target = source | Baseline reject; source unchanged |
| `UAT-MRG-018` | Engine failure | Source/old target safe; no half-final |
| `UAT-MRG-019` | History/reopen/delete entry | File remains; Audit unaffected |
| `UAT-MRG-020` | Offline valid grant | Complete locally; no upload |
| `UAT-MRG-021` | Invalid offline grant | Fail closed/online-required; no transfer |
| `UAT-MRG-022` | Engine unavailable | Safe error; bounded retry only |
| `UAT-MRG-023` | Low storage | Safe preflight/failure; source/target safe |
| `UAT-MRG-024` | App/Worker interruption | Continue/retry/cancel per valid checkpoint |
| `UAT-MRG-025` | Many/large | UI usable; limit early; accurate outcome |
| `UAT-MRG-026` | File/Tool/Assistant parity | Same Feature/rule/order/result; Origin only differs |
| `UAT-MRG-027` | Keyboard/AT/scaling/ID-EN | Complete primary task without clipped actions |

Merge exit memerlukan semua applicable scenarios berstatus sah; primary flow, source safety, offline, recovery, parity, localization, dan basic accessibility PASS; no Blocker/Critical atau outcome-breaking Major.

# 19. UAT Organize PDF

Split, remove, extract, reorder, rotate, crop, page number, watermark, dan compare harus menghasilkan working state/final output yang sama. Range/order/selection/position terlihat; Undo tersedia untuk tindakan rutin; source tetap utuh. Confirmation tidak diulang untuk working-state action yang reversible.

---

# 20. UAT Optimize PDF

Compress menguji trade-off size/quality dan no false reduction. Repair memakai repairable/non-repairable samples dan hanya lulus bila hasil benar-benar usable. OCR memakai ID/EN answer key dan known-term search/copy. PDF/A hanya lulus dengan approved conformance evidence. Limitation engine/PoC wajib jujur.

---

# 21. UAT Convert to PDF

| ID | Feature | Acceptance |
|---|---|---|
| `UAT-TO-001` | JPG→PDF `FEAT-CONV-007` | Page/order/orientation benar; no unexpected crop |
| `UAT-TO-002` | Word→PDF `FEAT-CONV-001` | Main content/layout usable; unsupported item jelas |
| `UAT-TO-003` | PPT→PDF `FEAT-CONV-003` | Slides lengkap/urut/fidelity layak |
| `UAT-TO-004` | Excel→PDF `FEAT-CONV-002` | Sheet/print area/pagination benar |
| `UAT-TO-005` | HTML→PDF `FEAT-CONV-017` | Approved source; unsafe/external content tidak dijalankan |
| `UAT-TO-006` | Scan→PDF `FEAT-SCAN-014` | Capture/retake/crop/reorder/preview natural |

Macro/script tidak dijalankan demi fidelity. Approved engine/build/limitation wajib dibekukan.

---

# 22. UAT Convert from PDF

| ID | Feature | Acceptance |
|---|---|---|
| `UAT-FROM-001` | PDF→JPG `FEAT-CONV-012` | Count/order/resolution sesuai selection |
| `UAT-FROM-002` | PDF→Word `FEAT-CONV-004` | Target content editable sesuai limitation |
| `UAT-FROM-003` | PDF→PPT `FEAT-CONV-006` | Slides ordered/usable |
| `UAT-FROM-004` | PDF→Excel `FEAT-CONV-005` | Tables/cells benar; no formula/macro surprise |
| `UAT-FROM-005` | PDF→Markdown `FEAT-CONV-036` | `.md` valid; main structure retained |
| `UAT-FROM-006` | Scanned input | OCR fallback/limitation jelas |

Extension yang benar tidak cukup; output harus usable untuk tujuan bisnis.

---

# 23. UAT Edit PDF

UAT mencakup supported existing-content edit, add text/image/shape/annotation, Forms, Undo/Redo, finalization/Save As, dan large workspace. Supported edit harus bertahan setelah reopen; unsupported content dijelaskan; field identity/value tidak tertukar; final target jelas; source tidak auto-overwrite.

---

# 24. UAT Keamanan PDF

| ID | Tindakan | Acceptance |
|---|---|---|
| `UAT-HR-PROT-001` | Protect/reopen | Password diperlukan; source tetap; no password evidence |
| `UAT-HR-UNLOCK-001` | Unlock authorized/wrong password | Hanya authority/password sah; bukan crack/bypass |
| `UAT-HR-REDACT-001` | Mark/preview/confirm/reopen/search/copy | Content hilang nyata; bukan overlay |
| `UAT-HR-SIGN-001` | Draw/upload/paste/place/export | Mark tepat; non-PKI jelas; reusable asset opt-in lokal |
| `UAT-HR-DELETE-001` | Permanent local delete | Exact target/impact/confirmation; no collateral delete |
| `UAT-HR-DELETE-002` | Secured purge | Fresh reauth; retention/legal hold; final state |
| `UAT-HR-SHARE-001` | Share sensitive result | Exact result/target/classification/current authorization |
| `UAT-HR-OVERWRITE-001` | Overwrite exact target | Version/conflict/summary/confirmation; atomic publication |

Password, redaction content, signature payload, page content, recipient secret, path, dan storage key tidak masuk Audit/log/telemetry/evidence biasa.

---

# 25. UAT PDF Intelligence

Summarizer menguji key points, factual grounding, limitation, provider failure, dan policy deny. Translate menguji reviewed bilingual corpus, critical terminology, layout limitation, user correction, serta offline/provider unavailable. Hanya approved local/enterprise-internal provider; no public AI fallback; output assisted tidak menggantikan source.

---

# 26. UAT Offline

| ID | Kondisi | Acceptance |
|---|---|---|
| `UAT-OFF-001` | Valid protected grant + local Feature | Berjalan lokal; zero processing upload |
| `UAT-OFF-002` | Draft/preview/Undo/local History/recovery | Own protected profile tersedia |
| `UAT-OFF-003` | Server/secured action | Online-required/waiting; no false success |
| `UAT-OFF-004` | Admin/Support/Audit offline | Ditolak; cached role/page bukan authority |
| `UAT-OFF-005` | Grant expired/tampered/wrong profile/clock rollback | Fail closed; no lifetime extension |
| `UAT-OFF-006` | Eligible high-risk local action | Permission/policy/confirmation tetap |
| `UAT-OFF-007` | Fresh reauth required | Menunggu online authentication |
| `UAT-OFF-008` | Sleep/restart during work | Compatible checkpoint recovery; no duplicate/loss |

Reconnect me-refresh identity, account, session, permission, policy, capability, dan compatibility sebelum dispatch.

---

# 27. UAT Local-First

Local-only memakai official local service/Worker tanpa processing API. Optional-server memakai Resolver/policy; exact content transfer dijelaskan. Local Worker failure tidak mematikan UI atau merusak source. Resource rendah menghasilkan preflight/failure aman. Draft/cache/temp/checkpoint tetap lokal. Offline Audit hanya safe metadata outbox dan tidak mengunggah dokumen.

---

# 28. UAT Selective Sync

| ID | Skenario | Acceptance |
|---|---|---|
| `UAT-SYNC-001` | Hasil tidak diamankan | Tidak muncul lintas device; no content upload |
| `UAT-SYNC-002` | Explicit Amankan | Exact validated result + notice/confirm |
| `UAT-SYNC-003` | Success | Setelah durable server state |
| `UAT-SYNC-004` | Disconnect during transfer | Waiting/retry; partial bukan final |
| `UAT-SYNC-005` | Lost acknowledgement/retry | Idempotent; satu logical effect |
| `UAT-SYNC-006` | Version 2/3 | Ordered list; max three |
| `UAT-SYNC-007` | Version 4 | Tetap tiga versi sesuai policy |
| `UAT-SYNC-008` | Restore | Exact version/target; no silent overwrite |
| `UAT-SYNC-009` | Same account device lain | Current authorization; available sesuai scope |
| `UAT-SYNC-010` | Cross-user reference | Deny/conceal; no content leak |
| `UAT-SYNC-011` | Delete | Account Recycle; local source/History/Audit unaffected |
| `UAT-SYNC-012` | Purge | Fresh reauth, retention/legal hold, final state |

Source tak dipilih, draft, undo, cache, thumbnail, temp, checkpoint, local History, password/token/clipboard, signature asset, full OCR, prompt/chat, dan unrelated Issue tidak auto-sync.

---

# 29. UAT Account dan SSO

Login/logout, expiry, reauthentication, account disable/revoke, account switch, dan client compatibility diuji. PERTAMINA Identity mengautentikasi; OTO mengotorisasi setiap action/resource. Reauth terikat principal/session/action/resource dan bukan permission elevation. Profile/queue/draft tidak berpindah saat account switch.

---

# 30. UAT Role dan Permission

Allow/deny, cross-user opaque ID, stale/revoked permission, policy deny, direct link/API, hidden Feature, dan Assistant invocation diuji. Denial terjadi sebelum engine/upload/job/storage side effect dan tidak membocorkan owner/filename/size/count. Otorisasi mempertimbangkan permission, scope, policy, state/version, capability, compatibility, consent, connectivity, dan reauth.

---

# 31. UAT Administrator

Admin menguji safe account metadata, eligibility/session revoke, scoped permission assignment, typed configuration/policy, Feature exposure, limit/maintenance/minimum-version, serta safe operational metadata. Mutasi high-impact memakai reauth, exact target, concurrency, SoD, dan Audit. Admin tidak membuat Feature baru, mematikan hard safety/Audit/public-AI controls, atau otomatis membaca document content.

---

# 32. UAT Auditor

Auditor menguji scoped read-only Audit Console, authorization-first filter/query/detail, optional bounded export dengan permission terpisah, expiry/reauth, dan negative mutation. Event reference bukan content grant. Auditor tidak edit/delete event, mutate policy/user data, atau memperoleh document/Issue/Assistant payload.

---

# 33. UAT Support

Support/Superadmin diuji sebagai scoped permission profile: exact Issue list/detail/attachment, typed triage/status, restricted internal note, bounded PDF/Excel export, dan negative cross-scope pivot memakai filename/Job/Correlation/Attachment/Export ID. Offline/stale assignment tidak memberi authority. Full user-document access tetap tidak didukung baseline.

---

# 34. UAT Issue Reporting

```text
Bantuan/error → Laporkan Masalah → pesan
→ screenshot opsional → markup opsional → preview/remove
→ explicit Kirim → status sederhana
```

UAT menguji report dari Help/error, attachment failure, duplicate-safe retry, safe auto-context, own status, dan Support triage. Screenshot tidak auto-capture/upload. Password/token/clipboard/unrelated content/raw stack/path/storage key tidak otomatis masuk Issue.

---

# 35. UAT Error dan Recovery

| ID | Gangguan | Acceptance |
|---|---|---|
| `UAT-REC-001` | Invalid/password | Safe actionable message; no process |
| `UAT-REC-002` | Network disconnect | `WAITING_CONNECTION`; accepted job findable |
| `UAT-REC-003` | SignalR disconnect | REST reconciliation; no replay |
| `UAT-REC-004` | Worker crash | UI/source safe; valid retry/resume |
| `UAT-REC-005` | App crash/restart | Supported state recovery; no silent loss |
| `UAT-REC-006` | Quota storage full | Safe preflight/failure/cleanup |
| `UAT-REC-007` | Permanent job failure | No unsafe retry/raw internals |
| `UAT-REC-008` | Transient failure | Bounded retry; one result |
| `UAT-REC-009` | Provider unavailable/busy | Waiting/backpressure; no public fallback |
| `UAT-REC-010` | Session expired | Reauth + current state/permission |
| `UAT-REC-011` | Server/storage unavailable | Local eligible remains; no false Completed |
| `UAT-REC-012` | Response lost after commit | Same resource/job; no duplicate effect |
| `UAT-REC-013` | Finalization failure | Source/old target safe; staged not final |
| `UAT-REC-014` | Bad checkpoint | Safe restart/discard; no silent resume |
| `UAT-REC-015` | Cancel | Request vs terminal state clear |
| `UAT-REC-016` | Result expired | Expiry/rerun clear; no download loop |

Semua case memerlukan clear state, supported recovery, bounded/current-authorized/idempotent retry, source/target safety, dan no silent data loss.

---

# 36. UAT Lintas Platform

| Area | Desktop | Web | Phone | Tablet |
|---|---|---|---|---|
| Posisi | Primary full feature | Enterprise/browser | Curated companion | Curated richer companion |
| Input | Picker/drop/paste/keyboard/mouse | Browser picker/upload | Touch/picker/share/camera | Touch/picker/share/camera/stylus |
| Local | Paling lengkap/Worker | No offline business authority | Curated | Curated |
| Layout | Resize/DPI/multi-panel | Responsive/zoom/reflow | Portrait/landscape/safe area | Larger adaptive layout |
| Signature | Mouse/draw/upload/paste | If exposed | Touch/stylus if supported | Touch/stylus if supported |

Camera, stylus, provider, share, notification, credential store, lifecycle, dan assistive-technology claims memerlukan real-device evidence.

---

# 37. UAT Adaptive UX

`FULL/PRIMARY/SUPPORTED` harus tersedia; `LIMITED` menjelaskan batas; `NOT_EXPOSED` tidak bocor atau membuat dead-end. Missing device capability memberi safe alternative/jalan kembali. Offline mempertahankan local eligible. Busy/resource memberi `WAITING_RESOURCE` atau safe reject. Update state mengikuti compatibility. Resize/rotate/input switch mempertahankan state/focus/selection/progress.

`NOT_APPLICABLE` memerlukan release-capability evidence. Promised-but-broken adalah FAIL; dependency/PoC belum siap adalah BLOCKED.

---

# 38. UAT Localization

Bahasa Indonesia dan English diuji pada navigation, search, 33 Core, form/editor, progress/error/confirmation, History/Activity/Secured, Help/Issue/Assistant, dan privileged surface. Unsupported locale fallback ke Bahasa Indonesia. Language switch tidak menghilangkan state atau re-execute action. Text expansion/reflow, locale presentation, localized `messageCode`, dan accessibility text diuji.

Feature/Requirement/Job/Correlation/Issue/Error/permission/schema/event/state IDs/codes, filename, dan user content tidak diterjemahkan. Raw key/enum atau campuran bahasa yang membingungkan adalah defect.

---

# 39. Accessibility dan Basic Usability

UAT memeriksa label/ikon, keyboard/focus/restore, readable state/contrast, no color-only information, zoom/scaling/reflow, practical touch targets, orientation/safe area, semantic name/role/state, understandable error, serta discoverable critical action. Hasil ini adalah basic usability acceptance, bukan certification/conformance claim.

---

# 40. Persepsi Kinerja

Fast task selesai tanpa dialog berlebihan; noticeable task memakai indicator; unknown total memakai indeterminate state tanpa fake percentage/ETA; long/background task memakai accurate progress/Activity; connection/resource/retry/terminal states dibedakan. Observation ditautkan ke approved Dokumen 12 baseline/target; UAT tidak membuat SLA angka baru.

---

# 41. Evidence

Evidence dapat berupa sanitized screenshot, test result, safe log reference, Correlation ID, Job ID, sanitized result sample, dan video hanya bila temporal behavior membutuhkannya. Manifest mencatat UAT/Defect/Requirement/Feature, build/digest, environment/config/platform/data, executor/time/result, artifact location/digest, access/classification, retention/disposal, sanitization, dan limitation.

Password/token/secret, unrestricted content/OCR/AI/prompt, real signature, clipboard, raw SQL/stack/body/claim, storage key/SAS URL, dan unnecessary attachment dilarang. UAT Evidence, Audit, History, Observability, dan Support tetap plane terpisah.

---

# 42. Penanganan Defect

Severity: `BLOCKER`, `CRITICAL`, `MAJOR`, `MINOR`; priority terpisah `DEF-P0..DEF-P3`.

```text
NEW → CONFIRMED → IN_PROGRESS → READY_FOR_RETEST → VERIFIED → CLOSED
Disposition: AS_DESIGNED / DUPLICATE
```

`FOUND`, `IN PROGRESS`, `READY FOR RETEST`, dan `NOT A BUG` hanya label UI untuk istilah canonical Dokumen 12, bukan enum baru. Blocker/Critical menghentikan scope dan menghasilkan REJECTED sampai fixed/retested.

---

# 43. Retest

Fix masuk pipeline artifact baru. Exact original reproduction dijalankan lebih dahulu, forbidden side effect diperiksa, lalu affected regression. Retest tanpa regression atau regression tanpa exact reproduction tidak cukup untuk VERIFIED. Failure evidence awal tidak ditimpa. Perubahan expected behavior memerlukan controlled revision.

---

# 44. Dependency Regression

Shared registry/contract memicu Core dan Triple Entry regression; validation/storage memicu input/Save As/source/History/recovery; job/realtime memicu Activity/cancel/retry/state; authorization memicu all-role/direct/Assistant/offline; Worker/engine memicu family/checkpoint/resource/privacy; sync memicu idempotency/max-three/cross-user; UI/localization memicu platforms/input/reflow; Audit/Issue memicu plane/scope/field minimization.

UAT regression melengkapi, bukan menggantikan, technical regression Dokumen 12.

---

# 45. Known Issues

Record memuat Defect/UAT, severity/priority, Requirement/Feature/platform/scope, impact, reason, workaround/control, owner, target, review/expiry, authority/reference, dan follow-up. Known issue bukan PASS/VERIFIED/CLOSED. Blocker/Critical tidak dapat diterima. Major memerlukan explicit decision; Minor tetap tracked; cumulative Minor dapat menjadi Major.

---

# 46. Penerimaan Risiko

Hanya governed Major/Minor yang tidak melanggar mandatory acceptance dapat dipertimbangkan. Tidak boleh diterima: Blocker/Critical, security/privacy exception, data loss/corruption, unauthorized access/disclosure, secret/content leak, destructive/false success, evidence/Audit-integrity failure, atau perubahan business/role/platform/security/SLA.

Business acceptance tidak menyiratkan Production/security risk approval.

---

# 47. Sign-Off

| Result | Kondisi |
|---|---|
| `ACCEPTED` | Exit lengkap; no Blocker/Critical; required outcome PASS |
| `ACCEPTED_WITH_KNOWN_ISSUES` | No Blocker/Critical; governed Major/Minor record lengkap |
| `REJECTED` | Gate/outcome/evidence/sign-off gagal atau defect terlarang |

Sign-off terikat product/release/scope, build/commit/pipeline/digest, environment/config/schema/policy/capability, period, summary/count/evidence/defect/known issue, result/rationale, authority/timestamp/reference/condition, dan `Production approval: SEPARATE / NOT IMPLIED`.

Approver/delegation/quorum/repository/retention menunggu konfirmasi resmi. Visual Sign non-PKI bukan mekanisme legal/PKI sign-off.

---

# 48. Traceability Requirement

| Area | Requirement/source | Coverage |
|---|---|---|
| Triple Entry | `DEC-REQ-024..025`, `UIR-UI-005`, `FR-ASSIST-001..014` | Bagian 14–16 |
| File | `FR-FILE-001..005`, `FR-FILE-010..015` | 14, 17–23 |
| Local/offline | `FR-PROC-001..005`, `UIR-UI-006..010` | 15–16, 26–28 |
| Storage/finality | `FR-STOR-001..006`, `NFR-REC-001..003` | 18, 23–24, 35 |
| Recovery | `FR-REC-001..006`, `UIR-UI-003..004` | 18, 35, 40 |
| History | `FR-HIST-001..003` | 13, 18, 26–28 |
| Audit | `FR-AUDIT-001..004` | 24, 29–33, 41 |
| Sign | `FR-SIGN-001..003`, `BR-SIGN-001`, `SR-SEC-011..012` | Core-028, 24 |
| Unlock/Redact | `SR-SEC-022..023` | Core-026/029, 24 |
| Secured/Trash | `FR-BACKUP-001..010`, `FR-TRASH-001..009` | 24, 28 |
| Intelligence | `FR-INTEL-001..003`, `SR-SEC-021` | Core-031..033, 25 |
| Role | `FR-ROLE-001..003`, Dokumen 09 | 29–33 |

Rantai: Business → Requirement → Feature/resource → Actor/platform → UAT → Result → Evidence/Defect → Risk/Sign-Off.

---

# 49. Traceability Feature

Bagian 17 memetakan 33/33 Core. Coverage tambahan: `FEAT-BACKUP-001..010`→`UAT-SYNC-001..012`; `FEAT-TRASH-004`→Bagian 24; `FEAT-HIST-001..015`→Bagian 13/18/26; `FEAT-SUPPORT-001..008`→Bagian 33–34; governed account/admin/audit resources→Bagian 29/31/32; Assistant→underlying canonical Feature + Bagian 16.

Tidak ada Feature ID baru. Baseline tetap 550.

---

# 50. Final Acceptance Checklist

## 50.1 Finalisasi spesifikasi

```text
[x] Dokumen 12 FINAL dan sumber 01–12 diperiksa
[x] Agent A–D tersedia dan diaudit silang
[x] 50/50 topik wajib tercakup
[x] 25/25 keputusan aman AUTO-LOCKED
[x] 550 baseline; 33/33 Core mapped
[x] Merge mempunyai 27 scenarios
[x] Triple Entry/offline/local/sync/role/Assistant/platform/Issue/recovery lengkap
[x] evidence/defect/retest/regression/risk/sign-off lengkap
[x] no edits pada 01–12/specialist; no app code/new ID/role/SLA
[x] audit integrasi PASS
```

## 50.2 Execution gate

```text
[ ] immutable RC/entry package verified
[ ] authority/account/permission/platform/data ready
[ ] Triple Entry critical parity PASS
[ ] 33/33 Core memiliki hasil sah
[ ] Merge/D3/high-risk critical PASS
[ ] source/offline/recovery/platform critical PASS
[ ] no Blocker/Critical/disallowed Major
[ ] retest/regression/evidence/traceability complete
[ ] known issue/risk governed
[ ] UAT Summary dan human sign-off recorded
[ ] Production approval tetap separate
```

## 50.3 Boundary

Usage findings menjadi input Dokumen 14; administration findings ke Dokumen 15; support/incident/recovery findings ke Dokumen 16. Dokumen 13 tidak menulis manual/runbook penuh.

---

# Lampiran — UAT Summary Minimum

```text
UAT Summary ID; Release/Scope; Build/Commit/Pipeline/Digest
Environment/Config/Policy/Schema/Capability; Period; Authorities
Cases Planned/Executed/Passed/Failed/Blocked/Not Applicable
Defects by Severity; Core/Requirement/Platform Coverage
Known Risks/Issues; Technical Evidence References
Final Recommendation: ACCEPTED / ACCEPTED_WITH_KNOWN_ISSUES / REJECTED
Rationale; Unmet Criteria; Sign-Off PENDING/RECORDED
Production Approval: SEPARATE / NOT IMPLIED
```

```text
Laporan Agent A–D        : 4 / 4
Keputusan UAT            : 25 / 25 AUTO-LOCKED
Topik wajib              : 50 / 50
Core coverage            : 33 / 33
Merge scenarios          : 27
Konflik material terbuka : 0
Dokumen 13               : FINAL v1.0
Execution UAT            : belum diklaim
Next                     : 14_USER_MANUAL.md
```
