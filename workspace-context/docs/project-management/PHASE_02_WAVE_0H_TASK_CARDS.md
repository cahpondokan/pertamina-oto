# Phase 2 Wave 0H — Remediasi dan uji lokal terbatas

User memilih REVISION_REQUIRED_ZERO_COST_REMEDIATION, kemudian secara eksplisit mengizinkan Sandbox/engine PDF dan memerintahkan `kerjakan`. Izin manusia tersedia; readiness teknis harus dibuktikan. Tidak perlu meminta ulang izin yang sama. Biaya tambahan, data nyata, Production, instalasi sistem/restart, serta engine baru tidak termasuk.

Baseline integrasi: d472b85e179d715b9795773bcfda01dc628e9d42. Repo D:\OTO\pertamina-oto. Koordinasi di root workspace. Main tetap dilindungi. Scope utama POC-PDF-001 / FEAT-PDF-005. Historical Wave 0G adalah snapshot larangan lama; jangan mengubahnya untuk membuat hasil hijau.

## Wave A — tiga task sidebar paralel

Setiap agen membuat satu worktree baru dari baseline di worktrees/phase2-20260905/agent-N-wave0h, branch task/P2-AN-wave0h. Jika sudah ada, periksa sebelum menggunakan. Gunakan apply_patch. Baca AGENTS, Shared Contract, Phase 2, status/register, charter dan sumber relevan. Semua handoff wajib commit, path list, verification, risiko, evidence, dan status bersih.

- Agent 1: allowed poc/wave0h/environment/**. Siapkan konfigurasi Sandbox konkret dan script observasi ringan dengan jaringan/clipboard/printer/audio/video redirection nonaktif, tanpa mapped host drive secara default. Tentukan alur ekspor evidence yang tidak membuka data host. Read-only host readiness boleh. Jangan launch dulu: Agent 0 mengaudit lalu menjadi satu-satunya operator launch. Acceptance: konfigurasi valid, command/timeout/close plan konkret, no credential/data host, batas observasi dan bukti yang belum tersedia jujur.
- Agent 2: allowed poc/wave0h/candidate/**. Periksa official reports PDFsharp issue 361, qpdf 1739/1688, relevance exact versi dan mitigasi untuk synthetic smoke. Lengkapi lisensi/notice untuk evaluation-only dengan file yang sudah tersedia; jangan mengarang waiver atau mewajibkan redistribution untuk evaluation. Siapkan rencana minimal runnable kandidat/hash/command/asset yang sudah ada. Tidak acquire/install/run engine/build/restore dalam lane ini. Acceptance: rekomendasi konkret go/hold terbatas dengan bukti primer dan blocker tersisa yang spesifik.
- Agent 3: allowed poc/wave0h/corpus/**. Lengkapi spesifikasi synthetic benign smoke A/B/C, exact expected page/order/duplicate markers dan source invariants; siapkan generator/oracle yang dapat direview, hanya standard library jika diperlukan. Jangan jalankan generator/PDF parser dulu. Tidak hostile/bomb/real docs. Acceptance: minimal runnable spec, batas dataset kecil, pemetaan UT-MERGE/TP-MRG exact, tidak membuat klaim fidelity tanpa oracle independen.

## Wave B — independent review

Agent 0 mengintegrasikan Wave A. Agent 6 (poc/wave0h/security/**) dan Agent 7 (poc/wave0h/governance/**) memakai fresh worktree dari hasil integrasi, mengaudit scope konkret dan izin baru. Bedakan benign Sandbox observation dari candidate processing. Review tidak menggantikan human authority, tetapi jangan mengulang permintaan persetujuan yang sudah ada. Agent 4–5 tidak aktif pada remediasi terbatas ini: portfolio tidak berubah.

## Agent 0

Audit/integrasikan, catat izin manusia baru dan status aktual. Lakukan benign Sandbox observation bila konfigurasi layak, lalu engine smoke hanya bila technical admission scope terbatas terpenuhi. Jika blocker nyata, selesaikan pekerjaan independen, laporkan bukti dan exact next action. Tidak mengklaim Phase 2 COMPLETE tanpa exit evidence. Jangan test validator lama yang hardcodes izin false sebagai gate izin baru; pertahankan sebagai bukti historis.
