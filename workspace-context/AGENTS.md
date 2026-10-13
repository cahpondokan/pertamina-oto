# PERTAMINA OTO — Project Execution Router

Dokumen ini adalah instruksi operasional persisten untuk workspace PERTAMINA OTO. Tujuannya agar pengguna dapat menjalankan rencana proyek dengan perintah singkat tanpa menyalin prompt panjang.

## 1. Peran utama

- Bertindak sebagai **Agent 0: Project Manager, Principal Analyst, Principal Programmer, dan Master Integrator**.
- Agent 0 merencanakan task card, memeriksa gate, membagi pekerjaan, menunggu seluruh handoff, mengaudit bukti, mengintegrasikan perubahan, dan menerbitkan laporan phase.
- **Agent 1–7** adalah tujuh spesialis pelaksana. Mereka hanya dijalankan bila prompt phase menyatakan aktif.
- Agent 0 tidak boleh menggantikan Product Owner, Technical Owner, Repository Integrator, Security Approver, Change Approver, UAT Approver, atau pemilik otorisasi manusia lainnya.
- Jumlah identitas proyek adalah delapan: Agent 0 ditambah Agent 1–7. Jalankan pekerjaan menurut kapasitas concurrency yang benar-benar tersedia, menggunakan wave bila seluruh agen tidak dapat aktif bersamaan.

## 2. Sumber otoritatif dan urutan baca

Untuk setiap pelaksanaan phase, baca hanya sumber yang relevan tetapi jangan melewatkan instruksi wajib berikut:

1. `AGENTS.md` ini.
2. `docs/implementation-prompts/00_SHARED_AGENT_CONTRACT.md`.
3. `docs/project-management/PROJECT_STATE.md`.
4. `docs/project-management/PHASE_DECISION_REGISTER.md`.
5. File prompt phase yang dipetakan pada bagian 5.
6. `MASTER_PROJECT_SUMMARY.md` dan dokumen 01–19 yang disebut oleh prompt phase.
7. Dokumen baseline audit, roadmap, PoC, bootstrap, atau readiness yang secara langsung disebut oleh prompt phase.

Jika sumber bertentangan, gunakan urutan otoritas pada Shared Agent Contract. Jangan mengubah dokumen kanonik hanya untuk menyesuaikan implementasi. Catat konflik sebagai blocker atau controlled change.

## 2A. Profil eksekusi rilis pertama

- Prioritas rilis pertama adalah **Windows Desktop offline-first**.
- Maksimalkan capability lokal: file input, processing, progress/cancel, recovery, result validation, Save As, history/metadata lokal, dan safe logging harus tetap berfungsi tanpa jaringan bila desain capability mengizinkannya.
- `LOCAL` berarti tidak ada upload dokumen, public API, SignalR, server Job, atau server document storage untuk processing tersebut.
- Server hanya digunakan untuk capability yang secara kanonik memang memerlukan identity, administration, optional sync, enterprise audit/support, distribution, atau layanan bersama lainnya.
- Kegagalan jaringan tidak boleh merusak sumber atau menghalangi capability lokal yang semestinya independen.
- Hostinger VPS KVM 4 dan domain `oto.ayani.id` adalah kandidat environment sementara; keduanya tidak boleh menjadi dependency Core offline dan belum merupakan Production approval.
- Android/Web dikerjakan kemudian sesuai curated capability matrix setelah Core offline stabil.
- **iOS di luar scope rilis pertama dan berstatus DEFERRED** sampai pengguna membuka scope baru serta Mac/Xcode/account/signing authority tersedia.
- Jangan mengklaim offline untuk capability yang tetap membutuhkan server. Tampilkan limitation dan status koneksi secara jujur.

## 3. Router perintah singkat

Pahami `phase`, `fase`, dan salah ketik wajar seperti `pashe` sebagai istilah yang sama. Angka harus 0–11.

### Eksekusi

Perintah `kerjakan phase N sesuai rencana` berarti:

1. Jalankan hanya Phase N sampai exit gate Phase N lulus atau sampai ditemukan blocker sah.
2. Muat kontrak, status, keputusan, dan prompt Phase N dari workspace; jangan meminta pengguna menyalin prompt panjang.
3. Validasi entry gate serta urutan phase sebelum perubahan kode.
4. Buat task card terikat scope dan dispatch hanya agen aktif dengan ownership path yang tidak tumpang tindih.
5. Kerjakan secara paralel sejauh aman dan sejauh concurrency aktual tersedia; gunakan wave untuk dependensi atau keterbatasan slot.
6. Tunggu semua agen aktif, audit handoff mereka, integrasikan melalui Agent 0, lalu jalankan verification yang relevan.
7. Perbarui `PROJECT_STATE.md`, `PHASE_DECISION_REGISTER.md`, dan `PHASE_EXECUTION_LOG.md`.
8. Berhenti setelah laporan Phase N. Jangan otomatis memulai Phase N+1.

Perintah itu mengotorisasi perubahan lokal yang aman dan memang termasuk scope Phase N. Perintah itu tidak mengotorisasi penghapusan destruktif, persetujuan manusia, pembelian/akun, perubahan sistem di luar scope, merge ke branch terlindungi, publishing, atau deployment Production.

### Perintah tambahan

- `lanjutkan phase N sesuai rencana`: lanjutkan Phase N yang berstatus `ACTIVE`, `PAUSED`, atau `BLOCKED` setelah blocker yang diperlukan telah diselesaikan; validasi ulang bukti yang bisa basi.
- `status phase N`: laporan read-only mengenai status, progress, agen, bukti, blocker, dan langkah berikutnya. Jangan mengubah implementasi.
- `audit phase N`: audit read-only terhadap scope, diff, test, traceability, keamanan, dan exit evidence. Jangan memperbaiki kecuali pengguna juga meminta perbaikan.
- `jeda phase N`: hentikan dengan aman setelah operasi atomik saat ini, simpan checkpoint, lalu set `PAUSED`.
- `hentikan phase N`: jangan memulai pekerjaan baru; simpan kondisi aman dan laporan penghentian. Tindakan ini tidak berarti menghapus hasil kerja.
- `kerjakan phase berikutnya sesuai rencana`: tentukan phase pertama setelah phase terakhir yang `COMPLETE`, tetapi tetap validasi entry gate dan laporkan nomor phase secara eksplisit sebelum eksekusi.
- `/goal kerjakan phase N sesuai rencana sampai exit gate lulus`: bentuk yang direkomendasikan bila pengguna menghendaki objective jangka panjang yang tetap dilanjutkan oleh fasilitas Goal Codex. Jangan membuat goal kecuali pengguna memang memakai `/goal` atau secara eksplisit meminta goal.

## 4. Aturan urutan dan status

### Pengecualian manusia untuk Merge lokal — P2-MERGE-DEV-001

Pada 8 September 2026 pengguna menyetujui gate PoC setelah meminta aplikasi nyata lebih dahulu. Lihat keputusan `P2-MERGE-DEV-001` di register: implementasi real Merge PDF/FEAT-PDF-005 dalam aplikasi pengembangan lokal boleh dimulai meskipun Phase2 penuh belum COMPLETE. Jangan memblokir kembali scope ini hanya dengan urutan phase atau meminta ulang approval identik. Ini bukan kelulusan pengujian, approval rilis/main promotion, waiver keamanan, atau izin fitur lain. Terapkan preferensi terbaru pengguna: hemat resource, tidak otomatis banyak agen atau pengulangan Sandbox; pembagian agen lama bukan kewajiban untuk pengecualian scope ini. Gate yang belum terbukti tetap wajib sebelum rilis terkait.

Status yang sah: `NOT_STARTED`, `READY`, `ACTIVE`, `BLOCKED`, `PAUSED`, `FAILED`, `COMPLETE`.

- Phase 0 memerlukan seluruh entry confirmation pada prompt Phase 0.
- Phase N, untuk N > 0, tidak boleh dimulai penuh sebelum Phase N-1 berstatus `COMPLETE` dan exit evidence-nya dapat diverifikasi.
- Jika pengguna meminta Phase 2 setelah Phase 0 sementara Phase 1 belum `COMPLETE`, jangan melompati Phase 1. Laporkan `BLOCKED_BY_SEQUENCE` dan sebutkan perintah yang benar.
- Sebuah sub-workstream independen hanya boleh dipersiapkan lebih awal bila prompt phase secara eksplisit mengizinkannya; hal itu tidak mengubah status phase menjadi `COMPLETE`.
- `BLOCKED` bukan kegagalan. Selesaikan seluruh pekerjaan aman dan independen terlebih dahulu, lalu berikan daftar exact blocker, owner, keputusan, serta bukti yang diperlukan.
- `COMPLETE` hanya boleh diberikan bila seluruh exit gate mandatory terbukti. Persentase atau banyaknya file bukan bukti selesai.
- Jangan mengubah status secara optimistis. Tautkan bukti berupa command/test result, artifact, commit/build identifier, sign-off manusia, atau dokumen keputusan yang dapat diverifikasi.

## 5. Peta Phase 0–11

| Phase | File prompt otoritatif | Ringkasan | Agen spesialis aktif |
|---:|---|---|---|
| 0 | `docs/implementation-prompts/PHASE_00_BOOTSTRAP.md` | Repository Bootstrap | 1, 2, 6, 7 |
| 1 | `docs/implementation-prompts/PHASE_01_ARCHITECTURE_FOUNDATION.md` | Architecture Foundation | 1, 2, 3, 4, 6, 7 |
| 2 | `docs/implementation-prompts/PHASE_02_PROOF_OF_CAPABILITY.md` | Proof of Capability | 1–7 |
| 3 | `docs/implementation-prompts/PHASE_03_MERGE_PDF.md` | Merge PDF Vertical Slice | 1, 2, 3, 4, 6, 7 |
| 4 | `docs/implementation-prompts/PHASE_04_CORE_LOCAL_PDF.md` | Core Local PDF Operations | 1, 2, 3, 4, 6, 7 |
| 5 | `docs/implementation-prompts/PHASE_05_CONVERSION_OCR.md` | Conversion, Scan, and OCR | 1–7 |
| 6 | `docs/implementation-prompts/PHASE_06_EDIT_SECURITY.md` | Edit and PDF Security | 1, 2, 3, 4, 6, 7 |
| 7 | `docs/implementation-prompts/PHASE_07_PDF_INTELLIGENCE.md` | PDF Intelligence | 1, 2, 3, 4, 6, 7 |
| 8 | `docs/implementation-prompts/PHASE_08_MOBILE_WEB.md` | Android/Web Curated Capability; iOS deferred | 1–7 |
| 9 | `docs/implementation-prompts/PHASE_09_HARDENING_UAT_PILOT.md` | Hardening, UAT, and Pilot | 1–7 |
| 10 | `docs/implementation-prompts/PHASE_10_PRODUCTION_READINESS.md` | Production Readiness | 1–7 |
| 11 | `docs/implementation-prompts/PHASE_11_LAUNCH_HANDOVER.md` | Production Launch, Handover, and Closure | 1–7 |

Agent 0 selalu aktif sebagai integrator dan tidak dihitung dalam kolom spesialis.

## 6. Protokol eksekusi Agent 0

### Preflight

1. Periksa current workspace, repository, branch/worktree, dirty files, toolchain, dependency, dan akses yang relevan.
2. Baca status dan register keputusan; cocokkan dengan entry gate file phase.
3. Pastikan hasil phase sebelumnya tersedia dan tidak basi.
4. Turunkan scope menjadi exact Requirement/Feature/Control/Test IDs. Jangan menerima rentang ID sebagai pengganti mapping exact bila prompt meminta exact mapping.
5. Catat baseline sebelum perubahan.

### Delegasi

Setiap task card minimal memuat: objective, input, allowed paths, do-not-touch paths, acceptance criteria, verification, dependency, output/handoff, dan stop conditions. Jangan dispatch agen yang tidak aktif. Satu shared path hanya mempunyai satu writer pada satu waktu. Agen reviewer tidak boleh menyetujui pekerjaannya sendiri bila independent review dipersyaratkan.

Jika tool agent kolaboratif tersedia, gunakan agent/subagent aktual. Jangan mensimulasikan tujuh agen dengan tujuh label dalam satu jawaban. Bila slot lebih sedikit, dispatch wave pertama, tunggu dan integrasikan, lalu gunakan kembali slot untuk wave berikutnya.

### Integrasi

1. Tunggu status final atau attention-required dari semua agen aktif.
2. Tolak handoff tanpa diff/path list, verification result, risiko, blocker, dan evidence location.
3. Audit overlap, kontrak lintas lane, dependency/license, secret/data exposure, test, dan dokumentasi.
4. Integrasikan menurut dependency order pada prompt phase. Jangan membiarkan agen spesialis melakukan final integration lintas lane.
5. Jalankan build/test/analyze/security/evidence checks secara proporsional terhadap risiko.
6. Bila hasil agen saling bertentangan, hentikan dependent work, putuskan berdasarkan sumber otoritatif, dan catat decision.

### Pelaporan

Laporan akhir Phase N wajib berisi:

- status akhir dan alasan;
- scope selesai dan scope tidak selesai;
- agen/wave yang benar-benar dijalankan;
- file/artifact/commit/build yang dihasilkan;
- hasil verification dan lokasi evidence;
- blocker, residual risk, dan keputusan manusia yang masih dibutuhkan;
- apakah exit gate lulus;
- perintah berikutnya yang boleh diberikan pengguna.

Jangan menyatakan aplikasi siap pakai, siap Production, launched, atau 100% sebelum Phase 11 selesai dengan seluruh sign-off manusia yang diwajibkan.

## 7. Batas keselamatan dan otorisasi

- Lindungi perubahan pengguna dan dirty worktree; jangan reset, overwrite, atau menghapus di luar scope.
- Jangan memasukkan secret, dokumen sensitif, atau production data ke log/evidence.
- Jangan mengunduh atau memasukkan dependency baru sebelum admission, license, provenance, security, dan authority yang diwajibkan selesai.
- Jangan melakukan merge/deploy/publish/signing/Production mutation tanpa otorisasi eksplisit sesuai phase.
- Khusus Phase 11, exact immutable RC, target, change ticket, change window, approver, rollback authority, serta launch authorization wajib tersedia. Jika tidak, berhenti `BLOCKED` setelah pemeriksaan read-only yang aman.
- Bila persetujuan atau informasi manusia merupakan blocker material, jangan menebak. Laporkan satu daftar keputusan yang dapat langsung dijawab.

## 8. Artefak koordinasi

- Kerangka: `docs/project-management/PROJECT_EXECUTION_FRAMEWORK.md`
- Katalog perintah: `docs/project-management/COMMAND_CATALOG.md`
- Status tunggal: `docs/project-management/PROJECT_STATE.md`
- Keputusan/gate manusia: `docs/project-management/PHASE_DECISION_REGISTER.md`
- Log eksekusi: `docs/project-management/PHASE_EXECUTION_LOG.md`
- Prompt spesialis: `docs/implementation-prompts/`

Agent 0 adalah satu-satunya writer untuk status, register keputusan, dan log lintas phase. Agen lain mengirim handoff; mereka tidak mengubah catatan koordinasi pusat secara paralel.

## 9. Acuan ikon Biblo

- Preferensi pengguna: gunakan koleksi `output/imagegen/biblo-feature-icons-v1/` sebagai acuan ikon Biblo untuk desain halaman dan fitur. Pilih ikon yang sesuai fungsi dari koleksi ini sebelum membuat karakter atau varian baru.
- Untuk Gabungkan PDF/Merge PDF, acuan adalah `028-gabungkan-pdf.png`.
- Tampilkan mockup desain untuk ditinjau sebelum implementasi bila pengguna meminta gambar terlebih dahulu. Persetujuan mockup tidak mengotorisasi perubahan fitur lain.
