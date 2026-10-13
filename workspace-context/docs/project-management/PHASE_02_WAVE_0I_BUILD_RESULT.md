# Hasil build offline — 8 September 2026

Agent0 menjalankan Build-Offline.ps1 -Mode Build setelah review independen Agent6 BUILD_ONLY_ADDENDUM dan Agent7 HANDOFF. Izin P2-W0H-EXEC-001 tetap berlaku. Scope hanya throwaway PoC, bukan product adapter.

- Input kode kandidat pada c9dec05a204e79a4420d556f691e4ea744df2ca2; source tidak berubah oleh integrasi review berikutnya.
- Verifikasi awal365file installed SDK/runtime/reference/compiler,4archive paket,12DLL terpilih: PASS. SDK10.0.400, runtime10.0.11.
- Percobaan pertama ditolak akses tulis sebelum compiler dimulai; tidak dianggap build gagal karena kode.
- Retry dengan izin filesystem sesuai scope: exit0, status BUILT_NOT_EXECUTED,215file guest. Tanpa NuGet restore, network acquisition, MSBuild, analyzer/package targets atau runner execution oleh build.
- Compiler terukur pada log: Visual C# Compiler5.9.0-1.26379.115. Log hanya banner, tanpa diagnosis error.
- Output lokal: worktrees/phase2-20260905/agent-0-integration/artifacts/evidence/P02-W0I/build-001.
- bundle.manifest.json SHA256 C5F2CE96F35B5790A4D4D862ACA457AD65411A4143AE90EB011502928B59543D.
- guest/app/MergeSmoke.dll SHA2567EF30795356FE694D23C39DAB671119EDB07295CCA4D36B83911CDA6CEC0BB15.
- RuangD yang diobservasi sebelum build sekitar109.92GiB bebas; tidak membeli kapasitas.

Staging untuk loader terpisah: folder baru TEMP/OTO-W0I-Noargs-001, hanya salinan guest bundle. Folder baru dibatasi ACL pemilik/SYSTEM/administrator; tidak mengubah ACL sumber atau pengaturan Windows. Seluruh215file diperiksa ulang terhadap manifest: PASS. Tidak ada dokumen pengguna. Salinan asli D memiliki inherited Authenticated Users Modify, sehingga tidak dipakai langsung sebagai mapped input.

Operator Invoke-Noargs.ps1 default verification (tanpa -Execute) lulus dengan satu read-only mapping dan nol writable mapping. Manifest/source/config wajib tetap sama pada review/run. Hasil runtime akan dicatat terpisah, bukan disimpulkan dari build.

Pemeriksaan independen/fixture gabungan awal8+8+28=44 lulus. Setelah koreksi Agent1, environment12+candidate8+oracle28=48 pemeriksaan; hanya environment12 yang rerun sesudah perbaikan, dua lane lain tidak berubah. Tidak ada test canonical Merge atau visual-fidelity yang diberi PASS oleh angka tersebut.
