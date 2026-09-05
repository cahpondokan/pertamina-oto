# Validasi Phase 1

Entry point: `pipelines/Invoke-Phase1Validation.ps1 -RunId <unique-run> -EvidenceRoot <new-directory>`.
Jalankan dari worktree bersih pada integrated commit setelah Agent 0 memasukkan seluruh project Agent 3 dan Agent 6 ke solution. Semua project di tests harus terdaftar di solution dan dikelompokkan Unit, Architecture, Contract, Security, atau UI. Kelima lane wajib tersedia; setiap project dijalankan sendiri dengan minimum satu test. Restore locked dan build tanpa incremental menggunakan SDK 10.0.400. Pipeline Phase 0 tetap historis dan tidak dipakai untuk scope Phase 1.

Evidence menggunakan allowlist: log hanya stage/status/exit, TRX hanya counter numerik, tanpa nama test, path fisik, output atau exception. Output tool dibuang, bukan dipublikasikan. Metadata run/task/decision/commit ada di summary dan manifest SHA-256. Decision IDs merujuk otorisasi SDK/dependency/local baseline, bukan sign-off Phase 1. Laporan deterministik untuk input yang sama; tidak mengandung wall-clock timestamp. Manifest bukan tanda tangan digital dan tidak tahan pemalsuan serentak seluruh evidence oleh penulis berwenang; simpan hash manifest di catatan integrator.

Self-test: `pwsh -NoProfile -File pipelines/Test-Phase1Validation.ps1 -EvidenceRoot <new-directory>`. Canary engine menghasilkan CANARY_ONLY, bukan bukti build produk. Fixture tamper sengaja tidak valid. Scope gate menggunakan allowlist source, larangan artifact deployment/Production, reference binary/package dan indikator server/platform; review independen tetap diperlukan untuk kode provider yang disamarkan. Dependency admission Phase 0 tidak diperluas.

Tidak ada deployment. Agent 0 wajib menjalankan final pipeline pada integrated commit; checkpoint Wave A belum mencakup Security/UI dan tidak dapat lulus final gate.
