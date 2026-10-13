# PERTAMINA OTO — Phase Decision and Human Gate Register

## SIDEBAR-CLIP-DEV-20260930 — APPROVED local UI fix scope

User supplied a sidebar screenshot and reported "terlihat jelas seperti terpotong". Scope is a small compact-navigation padding/alignment correction under the existing local application/update authority. No icon redesign, engine/dependency change, installed-version mutation or phase/Production approval. Agent 0 verified and published local update `20260930-144755-9abaa2ed`; details `worktrees/core-functional-first/docs/development/SIDEBAR_CLIP.md`.

## READER-SCROLL-DEV-20260930 — APPROVED local fix scope

User reports "ketika kita scrol ke bawah tapi malah mentok dan tidak lanjut ke halaman berikutnya". Scope: repair dedicated-reader navigation with continuous vertical scrolling, matching page controls, retained zoom and bounded page bitmap work. Existing local-update authorization remains applicable. This does not authorize new dependencies, installed-version changes, forced closure, Windows default mutation, phase promotion or Production.

Agent 0 implemented and verified native reader63 plus standard worker/editor/search/helper regressions; local update `20260930-144154-279ce46c` published with verified payload hashes. Details and manual physical-input limitations: `worktrees/core-functional-first/docs/development/READER_CONTINUOUS_SCROLL.md`.

## PDF-DEFAULT-DEV-20260930 — APPROVED local feature scope

User requested: "saya mau bisa di lakukan dari aplikasi ini. gunakan aplikasi ini untuk pembuka pdf default. saya mau ada fitur ini", with a Windows Open With screenshot. This authorizes the in-app registration/default-settings workflow for the local development application. Delivery follows the existing local-update preference and current-chat **Setujui update lokal** authorization; no new human gate is inferred. Windows UI retains the final default selection; no UserChoice/hash takeover, HKLM mutation, forced install/close, internet distribution, new dependency acquisition or Production approval is included.

Agent 0 implemented and verified the scoped feature. Local update `20260930-141956-a5a4707b` published successfully with 214 feature/regression checks and 368/368 payload integrity checks. Evidence and remaining installed Windows click-through: `worktrees/core-functional-first/docs/development/PDF_DEFAULT_APP.md`. This records requested scope and actual technical evidence; it does not promote any phase or replace UAT approval.

## GROUPED-CATALOG-001 — APPROVED local UI scope, 2026-09-08

User approved displaying all33tools with distinct inactive styling and requested left-aligned tidy navigation. Implemented in development worktree; no activation of additional engines, cost, dependency or release authority. Details GROUPED_CATALOG.md in worktree development docs.

## DOCUMENT-WORKSPACE-001 — APPROVED implementation scope, 2026-09-08

User: "ok sakarang terapkan semua tampilan ini ke dalam softwere yang kita buat. kerjakan sekarang". Approves applying agreed home/document/Organize/B3 visual designs in the local development app, including required existing-worker page-layout integration. No additional cost/dependency, release, main promotion or full phase approval. Result and remaining manual acceptance: `worktrees/core-functional-first/docs/development/DOCUMENT_WORKSPACE.md`. Windows built-in preview is local host rendering, not a security sandbox; benign test copies only.

**Single writer:** Agent 0  
**Status sah:** `OPEN`, `PROPOSED`, `UNVERIFIED`, `APPROVED`, `REJECTED`, `SUPERSEDED`  
**Aturan:** Agent dapat menyiapkan rekomendasi dan evidence. Hanya keputusan manusia berwenang yang dapat mengubah gate otorisasi menjadi `APPROVED`.

## 2026-09-08 — Functional-first, arah pengembangan terbaru

`DEV-FUNCTIONAL-FIRST-001`: pengguna meminta **"saya mau semua fungsi dari fitur2 utama bisa di pakai dulu berdasarkan prototipe yang sudah kita buat sebelumnya"**. Ini menjadi arahan implementasi lokal seluruh 33 alat utama secara bertahap, dengan hasil nyata lebih dahulu, melanjutkan izin Merge lokal. Bukan pernyataan bahwa semua fitur sudah selesai, bukan waiver keamanan/biaya/admission dependency, dan bukan izin Production atau main promotion. Ketersediaan diberi label per operasi yang benar-benar berfungsi, tidak dengan hasil simulasi. Komponen awal hanya existing PDFsharp + cache Windows App SDK; tidak ada akuisisi baru atau komponen berbayar baru. Scope formal phase serta gate rilis tetap dicatat terpisah.

Checkpoint Agent0 `84cbb0f`: 13 operasi dasar dalam aplikasi desktop lokal, 20 kartu belum aktif. Permintaan penuh belum terpenuhi. Detail batas/test/pekerjaan tersisa: `worktrees/core-functional-first/docs/development/FUNCTIONAL_FIRST.md`. Tidak mengubah dokumen kanonik 01–19 atau menutup gate yang belum terbukti.

## Phase 0 — Mandatory entry confirmations

| ID | Keputusan / input wajib | Status | Human owner/approver | Evidence/reference |
|---|---|---|---|---|
| P0-D01 | Menerima document manifest, master summary, roadmap, dan baseline sebagai dasar implementasi | `APPROVED` | Workspace User — Project/Product Owner sementara | Persetujuan eksplisit 2026-09-05; scope hanya bootstrap lokal/offline-first. Open item tetap terbuka dan dokumen 01–19 tidak direvisi |
| P0-D02 | Menetapkan URL/path project dan repository implementasi resmi serta default branch | `APPROVED` | Workspace User — temporary repository authority | Provisional source of truth `D:\OTO\pertamina-oto`, default `main`; Hostinger remote dan Azure migration menyusul |
| P0-D03 | Menetapkan nama manusia untuk Product Owner, Technical Owner, dan Repository Integrator | `APPROVED` | Workspace User — temporary Project/Product Owner dan approval authority | User memegang approval sementara; Agent 0 menjadi technical lead/integrator pelaksana dan tidak menggantikan human approval |
| P0-D04 | Memberikan akses developer/reviewer dan hak yang diperlukan untuk repo/pipeline | `APPROVED` | Workspace User — local scope | Local workspace access approved; tidak ada Production credential. Remote least-privilege/negative access tetap gate sebelum Hostinger/Azure integration |
| P0-D05 | Menyetujui exact .NET SDK, OS/build-agent image, dan target framework | `APPROVED` | Workspace User — temporary approval authority | .NET SDK `10.0.400`; local Windows 11 x64 validation; `net10.0`; WinUI target baru dibuka Phase 1/3 |
| P0-D06 | Menyetujui NuGet sources, initial dependencies, version, license, dan provenance | `APPROVED` | Workspace User — temporary approval authority | `https://api.nuget.org/v3/index.json`; source mapping `Microsoft.*`, `MSTest.*`; initial test SDK `MSTest.Sdk/4.4.0`; paket tambahan memerlukan admission baru |
| P0-D07 | Menyetujui branch protection, PR reviewer, CI/check policy, dan siapa yang berwenang merge | `APPROVED` | Workspace User — merge approval authority sementara | `main`; task branches/worktrees; no force push; Agent 0 audit/integration; user approval untuk final merge/promotion. Phase 1 exact candidate dipromosikan setelah explicit approval; local CI menjadi provisional PR-equivalent; Hostinger/Azure policy pending |
| P0-D08 | Menyetujui tempat penyimpanan evidence yang aman serta retention/access policy | `APPROVED` | Workspace User — local scope | `D:\OTO\pertamina-oto\artifacts\evidence`; sanitized local evidence, raw artifacts gitignored; indexed evidence terikat commit/run |

Phase 0 tetap `BLOCKED` sampai seluruh keputusan mandatory di atas `APPROVED` dan dapat diverifikasi. Agent 0 boleh melakukan read-only preflight dan menyusun exact checklist, tetapi tidak boleh menebak nilainya.

> Controlled exception `CCR-015`: nilai di atas berlaku untuk local/offline-first bootstrap sebelum onboarding IT. Ini bukan persetujuan Hostinger Production atau pengganti approval Azure/Pertamina.

## Phase 1 — Promotion checkpoint

| ID | Keputusan / input | Status | Owner/approver | Evidence/reference |
|---|---|---|---|---|
| P1-PROMOTE-001 | Fast-forward protected `main` dari `d497ec68ea8fa3e21ca311a580a44111045cfb20` ke exact Phase 1 candidate `5769ccce917d03905d07a1440ee527cb653c9cc4` | `APPROVED` | Workspace User — temporary merge approval authority | User stated `setujui` on 2026-09-05; fast-forward PASS; final `main` exact candidate; clean status; evidence binding PASS; final report `PHASE_01_FINAL_REPORT_P01-20260905-1319.md` |

## Phase 2 — Wave 0 bounded authorizations

### HOME-DESIGN-001 — 8 September 2026

Pengguna menyetujui mockup ImageGen melalui "ok buat tampilannya seperti itu ... iconnya lebih jelas ... kerjakan". Implementasi native Windows lokal dalam UI-FIRST-DEV-001: tiga panel, ikon jelas, staging file dan pencarian sinonim sesuai request sebelumnya. Assistant tidak diaktifkan tanpa model; tidak ada izin biaya/dependency baru/unggah/main promotion/rilis.

### UI-FIRST-DEV-001 — 8 September 2026, APPROVED untuk UI lokal

Human request: "selanjutkanya kita buat semua tampilannya dulu berdasarkan desain prototipe yang sudah kita buat sebelumnya." Pengguna/Project-Product Owner mengarahkan UI-first pada aplikasi Windows nyata. Mengizinkan tampilan lintas keluarga fitur dengan disclosure mesin belum tersedia. Tidak membuka dependency/biaya/engine baru, akun/upload, main promotion, deployment atau kelulusan Phase2. Pertahankan fungsi existing. Checkpoint `worktrees/core-functional-first/docs/development/UI_FIRST.md`: PARTIAL, detail/visual review masih diperlukan.

### P2-MERGE-DEV-001 — APPROVED, 8 September 2026

Human owner: pengguna workspace, Project/Product Owner dan approval authority sementara. Evidence percakapan: setelah meminta aplikasi nyata (bukan prototipe) dan menerima penjelasan perubahan urutan implementasi, pengguna menyatakan **"gate PoC saya setujui."**

Disposition POC-PDF-001: **APPROVED_WITH_LIMITATION untuk implementasi Merge PDF dalam aplikasi pengembangan Windows lokal/offline**, berdasarkan bukti Wave0K. Membuka real adapter dan UI produk FEAT-PDF-005 menggunakan kandidat PDFsharp6.2.4 beserta dependency existing yang terinventarisasi; bukan pemilihan vendor permanen atau persetujuan distribusi. qpdf tetap alat evaluasi, tidak otomatis menjadi dependency produk.

Controlled exception terhadap urutan phase: pekerjaan implementasi Merge Phase3 boleh dimulai tanpa menunggu seluruh portofolio Phase2 selesai. Ini bukan COMPLETE Phase2/2A penuh, bukan kelulusan otomatis enam kelompok gate, dan bukan persetujuan Production/main promotion. Bukti fidelity, cancellation/failure/recovery, source/output safety, performance serta keputusan release/license yang belum tersedia tetap OPEN dan harus diselesaikan sebelum rilis yang relevan. Batas keamanan saat menjalankan input tidak tepercaya tidak dihapus; implementasi harus menyediakan penanganan aman, bukan memindahkan harness mentah ke host.

Prioritas user: aplikasi nyata yang bisa dicoba langsung, bukan simulasi terpisah. Kerjakan bertahap dan hemat resource; jangan otomatis menjalankan banyak agen atau Sandbox berulang. Persetujuan ini tidak menambah izin biaya, dependency baru, deployment, restart, dokumen sensitif, atau penghapusan data. Scope selain Merge tetap mengikuti gate semula. Catatan ini mencatat izin implementasi; belum ada kode aplikasi baru atau pengujian baru pada turn persetujuan ini.

Checkpoint2026-09-08 Wave0K: existing P2-W0H-EXEC-001 dipakai untuk private synthetic PDFsharp/qpdf evaluation. Agent7GO_WITH_BOUNDARIES exactexistingclosure dan Agent6sourceGO/actualbounded-evidencePASS dicatat `PHASE_02_WAVE_0K_CHECKPOINT.md`; bukan human admission/distribution approval. Empatactualcase count/order/marker PASS; failedprofile001tetaphistoris. P2-D01/P2-D02/P2-D03/fullPhase2disposition belum berubahapproved. Remaininggates fidelity/failure/resource/portfolio, bukan meminta ulang izin engine. No cost/acquisition.

Checkpoint 2026-09-08 Wave0J: P2-W0H-EXEC-001 tetap berlaku dan tidak dimintakan ulang. Agent6 memberikan technical GO source-bound untuk engine-only benign smoke; ini bukan human engine admission. Actual noargs dan empat engine smoke berhasil, sumber unchanged dan sesi ditutup. Gate yang masih terbuka adalah independent observer runtime/evidence chain dan full PoC disposition; bukan izin Sandbox atau RAM checkpoint sebelumnya. Lihat `PHASE_02_WAVE_0J_CHECKPOINT.md`. Baris historical acquisition-only di bawah tidak menggantikan otorisasi eksekusi yang lebih baru.

| ID | Decision / authority | Status | Owner/approver | Evidence/reference |
|---|---|---|---|---|
| P2-W0-ACQ-001 | Acquisition-only exact PDFsharp 6.2.4 from official NuGet; no document processing, product reference, transitive package acquisition, or paid component | `APPROVED` | Workspace User — temporary Technical/License/Cost authority | User stated `ok saya setuju asalkan tidak ada biaya lagi` on 2026-09-05 in response to the exact Wave 0 packet. `ZERO_ADDITIONAL_COST` is a mandatory stop condition |
| P2-W0-CORPUS-001 | Prepare deterministic synthetic-only corpus/oracle and measured baseline; no Production/personal documents and no numeric pass claim yet | `APPROVED` | Workspace User — temporary Product/QA/Corpus/Security authority | Same explicit approval; exact generated manifest/digest and later execution authorization still required |
| P2-W0-ENV-001 | Prepare free isolated Windows environment with processing network disabled; do not run hostile suite on main host | `APPROVED` | Workspace User — temporary Environment/Security authority | Post-restart read-only observation 2026-09-07: feature enabled, Microsoft-signed launcher and Start entry present, hypervisor/supporting services observable, no pending restart, static template controls PASS. Sandbox was not launched; effective guest boot/network-deny/host-boundary evidence remains unproven and requires separate authority |
| P2-W0-EVID-001 | Use local sanitized evidence root `D:\OTO\pertamina-oto\artifacts\evidence\P02-20260905-1434`; SHA-256 binding; no secrets/content | `APPROVED` | Workspace User — temporary Evidence/Execution authority | Same explicit approval; run-specific manifest/retention markers must be generated before processing |
| P2-W0-ROLE-001 | Workspace User temporarily holds the named human decision roles for local PoC and accepts the documented independence limitation | `APPROVED` | Workspace User | Explicit acceptance of the complete question on 2026-09-05; does not authorize Production, procurement, paid licensing, or organizational sign-off on behalf of IT/Pertamina |
| P2-W0-ENV-002 | Inspect and activate built-in Windows Sandbox at zero additional cost; no computer restart without separate approval | `APPROVED` | Workspace User — temporary Environment/Security authority | Fulfilled through prior no-restart activation check plus separately approved restart. Post-restart evidence is recorded in `PHASE_02_WINDOWS_SANDBOX_POSTRESTART_RESULT.json`; no Sandbox launch or added cost occurred |
| P2-W0-RESTART-001 | Restart Windows once to finalize the already-enabled Sandbox feature, then perform post-restart read-only launcher/control verification | `APPROVED` | Workspace User — temporary Environment/Security authority | Fulfilled: restart command accepted at 15:04, boot time observed 15:06:05 +07:00, read-only observations completed at 20:20:57. No Sandbox/qpdf/PDFsharp launch, restore/build, or PDF processing occurred. Approval is consumed and does not authorize another restart or execution |
| P2-W0-DEP-002 | Acquisition-only of Logging.Abstractions 8.0.3, Cryptography.Pkcs 8.0.1, and DependencyInjection.Abstractions 8.0.2 from exact official URLs; zero cost; no restore/build/run/product reference/extra package | `APPROVED` | Workspace User — temporary Technical/License/Cost authority | User instructed on 2026-09-07 to continue Phase 2 with everything free and disclose paid requirements, after the exact packages and acquisition-only boundary were presented. Closure SHA-256 `3130E92422B3449A26D30B80B3EE3030CC1699F8810D439AFB54A767CCEA857A`; Wave 0E task card |
| P2-W0-ORACLE-002 | Hold Poppler use/copying; defer MuPDF; allow qpdf only through a later separate exact-asset decision | `PROPOSED` | Workspace User — temporary Product/Technical/License/Security authority | Exact qpdf ZIP acquired and statically reviewed in Wave 0F @ `4a78212`; it remains a structural candidate only. No qpdf execution, PDF processing, engine selection, Poppler use, or MuPDF substitution authorized |
| P2-W0-QPDF-ACQ-001 | Acquisition-only of exact `qpdf-12.4.1-msvc64.zip`; no execution/PDF processing/product reference/extra component; zero cost | `APPROVED` | Workspace User — temporary Technical/License/Cost authority | Fulfilled 2026-09-07 under explicit user approval. Official ZIP is 28,165,367 bytes, SHA-256 `3CD016CD433EF7232E42F4C13348A49CC14907A3C7278EF4F99120593126F7A6`; 319/319 entries and 12 PE records matched independent static review. Install/execution remains prohibited; see Wave 0F checkpoint |
| P2-W0G-DISP-001 | `REVISION_REQUIRED_ZERO_COST_REMEDIATION` | `APPROVED` | Workspace User — temporary Project/Product/Technical decision authority | User selected remediation in conversation; Wave 0G packet remains historical. Separate subsequent scoped execution authority is recorded under P2-W0H-EXEC-001 and PHASE_02_WAVE_0H_AUTHORIZATION.md; technical findings are not waived. |

## Keputusan lintas phase yang dipersiapkan sejak awal

| ID | Phase | Keputusan / input | Status | Owner/approver | Dibutuhkan sebelum |
|---|---:|---|---|---|---|
| P2-D01 | 2 | Approved PoC candidate list, exact versions, dan evaluation priority | `PROPOSED` | Technical Owner | PDFsharp 6.2.4 and its three exact package dependencies have hash-bound static intake. qpdf 12.4.1 exact ZIP is acquired and statically reviewed but remains conditional on security/provenance/SBOM/notice/oracle freeze; MuPDF deferred and Poppler held. No provider/oracle execution or engine selection approved |
| P2-D02 | 2 | Corpus non-sensitif, expected outputs, quality/performance thresholds | `PROPOSED` | Product + QA + Security | Synthetic preparation authority approved; deterministic foundation 17/17 PASS. Exact execution corpus, independent real PDF oracle, run retention/access/expiry, safety ceilings, and numeric acceptance remain unresolved |
| P2-D03 | 2 | License, redistribution, runtime, support, dan cost acceptance | `PROPOSED` | Legal/License + Technical | Three exact dependency artifacts are MIT/no-license-fee with supplied notices retained. qpdf's Apache-2.0 declaration supports no-license-fee evaluation, but bundled Microsoft runtime rights, embedded OpenSSL/zlib/JPEG identity, fonts/docs notices, exact SBOM, signature/advisory disposition, redistribution scope, and support ownership remain open. MuPDF commercial route excluded; AGPL route deferred; Poppler held |
| P5-D01 | 5 | Format family/provider yang boleh diaktifkan berdasarkan PoC disposition | `OPEN` | Product + Technical + Security | Coding affected family |
| P6-D01 | 6 | Exact semantics untuk unlock/protect/redact/visual sign dan reauth | `OPEN` | Product + Security | High-risk coding |
| P7-D01 | 7 | Model/provider/version, local/internal boundary, egress, retention, subprocessors | `OPEN` | Product + Security/Privacy | Real intelligence provider |
| P7-D02 | 7 | Factuality/translation/omission threshold dan human-review disclosure | `OPEN` | Product + QA | Intelligence exit |
| P8-D01 | 8 | Exact capability exposure per Desktop/Android/iOS/Web | `PROPOSED` | Product Owner | User direction 2026-09-05: Windows Desktop offline-first lebih dahulu; Android/Web kemudian; iOS DEFERRED dari rilis pertama. Exact Feature matrix dan authority masih diperlukan |
| P8-D02 | 8 | API/offline/sync/conflict/session/upload/data boundary | `OPEN` | Technical + Security | Server-dependent work |
| P8-D03 | 8 | Android/iOS accounts, signing, Mac/Xcode, device/browser matrix, web hosting | `OPEN` | Platform Owners | Affected target build |
| P9-D01 | 9 | Frozen release scope dan controlled-change authority | `OPEN` | Product + Technical | RC build |
| P9-D02 | 9 | UAT users, approver, environment, scenarios, data, schedule | `OPEN` | Business/UAT Owner | UAT start |
| P9-D03 | 9 | Pilot group, monitoring, support, communication, rollback authority | `OPEN` | Business + Operations | Pilot start |
| P10-D01 | 10 | Production topology, targets, network, SSO, secrets/certs, data/storage/queue | `PROPOSED` | Infrastructure + Security | User menyatakan Hostinger VPS KVM 4 dan domain `oto.ayani.id`; authority, OS/IP, DNS/TLS, topology, access, security, backup, dan Production approval belum diverifikasi |
| P10-D02 | 10 | Signing/distribution channels dan credential custodians | `OPEN` | Platform Owners | Release rehearsal |
| P10-D03 | 10 | Migration, backup/restore/DR, reconciliation, monitoring, support/on-call model | `OPEN` | Data + Operations | Readiness exit |
| P10-D04 | 10 | Residual risks dan Production Go/No-Go packet acceptance | `OPEN` | Named risk authorities | Phase 10 exit |
| P11-D01 | 11 | Exact immutable RC digest dan authorized Production targets | `OPEN` | Release + Infrastructure Owners | Any promotion |
| P11-D02 | 11 | Approved change ticket, change window, runbook, comms/escalation | `OPEN` | Change Approver | Deployment start |
| P11-D03 | 11 | Named deploy, rollback, security, data, business, operations authorities | `OPEN` | Sponsor/Change Owner | Launch checkpoint |
| P11-D04 | 11 | Production smoke/UAT acceptance dan residual risk sign-off | `OPEN` | Business + Security + Operations | Release acceptance |
| P11-D05 | 11 | KT, access, manuals, CMDB/cartography, support handover acceptance | `OPEN` | Operations/Service Owner | Handover closure |
| P11-D06 | 11 | Project closure authorization dan disposition seluruh open items | `OPEN` | Sponsor/Project Owner | 100%/COMPLETE |

## Format pembaruan keputusan

### Wave0I — 2026-09-08 technical checkpoint

`P2-W0H-EXEC-001` tetap APPROVED dalam scope sebelumnya setelah user meminta lanjut Phase2. Ini bukan approval baru atau perluasan scope. Agent6/7 menerima build-only secara teknis; build berhasil. Agent6 menerima exact noargs source setelah IO correction (CCBFD7D174009F75FAD3771B2D6FC98AD268FDB5D934D23F6F46E83B8B07851A); runtime retry tertahan resource4272MiB vs guard5120MiB. Tidak ada full engine/corpus/PoC disposition atau human final promotion yang diberikan. Evidence: PHASE_02_WAVE_0I_CHECKPOINT.md, PHASE_02_WAVE_0I_BUILD_RESULT.md, PHASE_02_WAVE_0I_SANDBOX_OBSERVATION.md; integration302751f. Hak eksekusi yang ada bukan bukti keberhasilan teknis.

### Wave 0H — latest user disposition, 2026-09-07 Asia/Jakarta

- `P2-W0G-DISP-001`: user selected `REVISION_REQUIRED_ZERO_COST_REMEDIATION`; APPROVED for remediation, superseding the prior pending-choice state (historical Wave 0G findings retained).
- `P2-W0H-EXEC-001`: APPROVED by user/temporary Project-Product authority via conversation: "anda boleh menjalankan sandbox atau engine PDF", followed by "kerjakan". Covers limited local synthetic Sandbox/existing-engine testing after technical checks; excludes new costs/acquisition, real documents, restart, security-setting changes and Production. Exact scope and limitations: `PHASE_02_WAVE_0H_AUTHORIZATION.md`. This is not technical admission or Phase 2 exit approval.

Untuk setiap perubahan status, catat di kolom evidence/reference atau lampiran terhubung:

- decision ID dan exact scope;
- keputusan yang diberikan, termasuk limitation/expiry;
- nama dan role manusia yang berwenang;
- timestamp Asia/Jakarta;
- ticket/email/meeting-minutes/document reference yang dapat diaudit;
- artifact/version/digest yang dicakup;
- supersedes/superseded-by bila keputusan berubah.

Jangan menyimpan secret atau credential di register ini. Simpan hanya referensi ke secret-management system yang disetujui.

## Infrastructure declarations received

| Declaration ID | Tanggal | Pernyataan pengguna | Status penggunaan | Kekurangan bukti |
|---|---|---|---|---|
| INFRA-DECL-001 | 2026-09-05 | VPS Hostinger paket KVM 4 telah dibeli; domain aplikasi `oto.ayani.id` | `PROPOSED` untuk environment/hosting decision; tidak membuka Phase 0 | Named owner/authority, intended environment, VPS region/OS, public/private IP reference, DNS control, TLS/certificate custody, firewall/network, backup/restore, monitoring, hardening, data classification, dan approval Production |
| EXEC-SCOPE-001 | 2026-09-05 | Buat versi offline-first terlebih dahulu, maksimalkan pemrosesan lokal, dan jangan membuat iOS pada rilis pertama | `PROPOSED` sebagai release execution profile; berlaku untuk planning/task slicing | Named Product Owner approval, exact Release-1 Feature IDs, offline acceptance matrix, dan disposition capability yang intrinsik server-dependent |
| LOCAL-BOOT-001 | 2026-09-05 | Project/Product Owner menyetujui Phase 0 lokal sementara memakai konfigurasi aman dan meminta Phase 0 dilanjutkan | `APPROVED` untuk Phase 0 local bootstrap; fulfilled | Repo `D:\OTO\pertamina-oto`; `main` @ `d497ec68ea8fa3e21ca311a580a44111045cfb20`; .NET SDK 10.0.400; local Windows validation; NuGet.org scoped; MSTest.Sdk 4.4.0; final report `docs/project-management/PHASE_00_FINAL_REPORT_P00-20260905-1155.md`; no Production/iOS |
# READER-SEARCH-DEV-20260930 — scoped continuation

Current-chat user instruction `kerjakan keduanya` authorizes verification/fixes of the reader and local PDF content search, following the offered choices. Implemented using existing PDFsharp and local worker; no dependency acquisition/admission, Production, new phase, or feature gate completion. Agent 0 only.

Local delivery authority was retrieved from source chat `01a07c8d-3c57-7963-8500-daed7ddb07ee`, turn `01a0e5fe-36fe-74a2-9f73-4d8e40a7c543`: user requested a stable application ready for update after changes and selected local release-folder delivery. Automatic approval review initially rejected publication twice because it required current-chat publishing consent. On 30 September 2026 the user explicitly replied **Setujui update lokal** in the continuation chat. Status: `APPROVED` for this reader/search development package and local `latest.json` feed update; not installation, forced closure, internet distribution, or Production. The approved standard build/test/publish command is now permitted; successful publication is recorded separately. Current implementation and evidence: `worktrees/core-functional-first/docs/development/READER_SEARCH.md`.

## DOWNLOAD-PORTAL-DEV-001 — 1 October 2026

User requested a C# ASP.NET download site at oto.ayani.id and online release notification in installed OTO. User selected checks while the app is open/running, Ubuntu/Debian hosting, and supplied aynadmin@187.53.134.25 with DNS directed there. This explicitly expands local development scope to a download portal/read-only metadata workflow and preparation for that target; earlier local-feed-only constraints are superseded for this request. No forced update/background service, new NuGet dependency, phase promotion, signing or Production approval is inferred. Agent0 only, existing dirty work preserved.

Local implementation/tests complete; local update 20261001-094532-c58e4ca7 and verified ZIP prepared. Authenticated read-only VPS preflight confirmed Ubuntu22.04.5, DNS and existing HTTPS. A private staging upload was completed and hash-verified; existing Nginx config remained unchanged. Public activation is PENDING_ADMINISTRATOR: host ASP.NET10 runtime absent, sudo requires a password, public development package is unsigned and distribution/signing disposition remains separate. Exact candidate, runtime-source guidance and guarded activation/rollback script are prepared in the private staging folder and eng/hosting. No password acquisition, sudo bypass, package/image acquisition or public service activation occurred. Evidence and remaining acceptance: worktrees/core-functional-first/docs/development/DOWNLOAD_PORTAL.md.

## WINDOWS-EXE-SETUP-DEV-001 — 1 October 2026

User explicitly requested a single branded EXE opening a wizard, detecting prerequisites and downloading/installing missing .NET10 Runtime x64 before OTO. Authorizes scoped implementation and development integrity/trust checks; it is not permission to execute a vendor installer on this development computer or to sign/publicly activate a Production release. Microsoft runtime acquired only for quarantined hash/signature verification after documented intake; executable not run. Existing runtime skips network entirely. Agent0 only. Local artifact102920-701ec3e1 and verified tests recorded in worktree docs/development/WINDOWS_SETUP.md. Clean-machine/UAC acceptance, public distribution/signing and administrator hosting activation remain open; no phase promotion.
## VPS-ACTIVATION-DEV-001 — 1 October 2026

User: "ok sekarang kita aktivasi vpsnya. buatkan paket yang bisa langsung saya jalankan di powershell". Explicit scoped authority to prepare and provide an activation workflow for the already disclosed unsigned development EXE/portal at oto.ayani.id, using aynadmin@187.53.134.25. Supersedes earlier pending user disposition for this development activation; does not imply signing, phase11 or Production-readiness approval. Runtime technical intake completed for pinned official ASP.NET10.0.12 Linux archive, isolated to OTO, preserving vendor notices; no global package repositories/runtime modified. Credentials/sudo are entered by the user only in the actual SSH terminal. No repeated permission request needed. Exact runnable artifact, checksum, tests and remaining real activation step: worktree docs/development/VPS_POWERSHELL_ACTIVATION.md.

Follow-up under the same authority: user's real activation failed at public HTTPS404 and rolled back. The readiness-wait correction and exact failed-predecessor retry are routine fixes in the authorized workflow.32 tests and isolated real Nginx reload checks PASS. Updated package was subsequently observed ACTIVE via read-only SSH; independent external portal/EXE integrity checks PASS. No agent sudo execution, new approval, phase promotion, credential request or authority widening.
