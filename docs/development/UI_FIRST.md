# UI-first — 8 September 2026

Status ACTIVE / PARTIAL, bukan seluruh UI selesai atau phase COMPLETE.

## Scope / task card Agent 0

Permintaan pengguna: semua tampilan aplikasi nyata mengikuti prototipe. Baseline clean `task/core-functional-first` @ `84cbb0f538020003bcfb6583e94c2fcf6524f87c`, continuation worktree yang sama. Referensi: `worktrees/uiux-integrator/uiux/components/shell/`, `screens/home/`, `screens/settings/`, `screens/phase2/` dan katalog 33 alat. Ini adaptasi WinUI, bukan rebuild prototipe web.

Allowed: Desktop.App UI, konfigurasi output Desktop, dokumentasi development/koordinasi. Do not touch: Worker/WorkerClient/protocol, package admission, main/deployment/data pengguna. Acceptance: navigasi nyata, glyph bukan inisial, label capability jujur, worker wiring dipertahankan, build offline dan visual review. Stop pada biaya/dependency/engine baru atau konflik data. Satu Agent0; tidak ada agen paralel.

## Hasil

- Shell biru gelap, header pencarian, badge LOCAL, ikon berwarna dan kartu; beranda tiga entry; katalog 33 alat; favorit sesi nyata.
- Halaman kerja 13 operasi dasar mempertahankan picker, ordering, opsi, Save As, progress/cancel/result. Layout file/pengaturan dua panel, bertumpuk pada jendela kecil.
- 20 alat belum aktif mendapat area input/opsi/hasil nonaktif, bukan processing simulasi.
- Riwayat/Aktivitas memakai hasil sesi. Assistant/File Diamankan/Admin mengungkap layanan belum terhubung, tanpa data contoh/role palsu.
- Pengaturan folder memakai picker nyata; bantuan berisi formulir laporan yang disalin atas klik pengguna, tanpa kirim tiket atau screenshot otomatis.
- Tidak mengubah worker, mengunduh package, menambah biaya, menjalankan Sandbox atau memproses dokumen.

## Verification dan batas

Build: Visual Studio MSBuild `Pertamina.Oto.Development.slnx /t:Build /p:Configuration=Debug /p:DesktopUiOutputPath=<worktree>/artifacts/desktop-ui-final/ /v:minimal /nologo`, tanpa restore. Build terpisah PASS exit 0. `git diff --check` PASS.

Build awal gagal izin sandbox lalu executable terkunci oleh aplikasi lama pengguna. Folder output terpisah dipakai; sesi pengguna tidak ditutup. Computer Use berhasil membuka build UI awal dan mengamati screenshot/accessibility Beranda serta Merge: ikon dan entry tampak. Helper kemudian timeout pada close/list_windows, tetap gagal setelah retry/reset. Build UI pertama mungkin masih terbuka. Layout dua panel final/responsive/navigasi ulang belum terverifikasi visual. Test engine baseline 23/21 tidak dijalankan ulang; bukan bukti baru. Tidak mengklaim UAT/complete.

Remaining: detail editor/canvas/thumbnail dan parameter spesifik seluruh prototipe; command palette Ctrl+K; file-first umum/drag-drop; persistence; recovery/queue; akun/admin nyata; verifikasi resize, keyboard, screen reader dan visual final. Mobile/Web kemudian, iOS deferred. Halaman nonaktif bukan fitur selesai.

Executable terbaru: `artifacts/desktop-ui-final/Pertamina.Oto.Desktop.App.exe`. Tidak ada commit/main promotion/rilis pada checkpoint ini.
