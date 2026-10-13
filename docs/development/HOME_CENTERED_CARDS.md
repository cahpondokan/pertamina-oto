# Beranda kartu Ikon terpusat

10 Oktober 2026 (Asia/Jakarta). Scope implementasi lokal UI; bukan eksekusi phase. Satu writer, tanpa subagent.

## Baseline dan ownership

Checkout `worktrees/core-functional-first`, branch `task/core-functional-first`, HEAD `84cbb0f538020003bcfb6583e94c2fcf6524f87c`. Versi produk tetap **0.2.4 · Pengembangan**. Parent chat terlihat idle ketika diperiksa; tidak ditemukan writer lain yang aktif pada source ini. Dirty work existing dipertahankan. Baseline tracked diff, status, HEAD, dan salinan file yang disentuh berada di `artifacts/home-centered-01a12669/baseline/`.

Acuan tetap: `C:/Users/cahpo/.codex/visualizations/2026/09/07/01a07c8d-3c57-7963-8500-daed7ddb07ee/oto-card-hover.html`, hanya Ikon terpusat. SHA-256 `DA504DE9122A2D8E49D95CEA0EDE76CE4353E7DCC4B5FA71FDABDF50812BCA24`. File prototipe tidak ditulis. Dibaca sebagai sumber ukuran/warna/teks dan dibuka di panel Codex.

## Implementasi

- `CenteredHome.cs`: komponen kartu bersama untuk 16 capability aktif, ikon vector native, tinggi kartu 160 DIP termasuk border, jarak 12 DIP, warna/teks/ukuran mengikuti CSS prototipe. Grid 4/3/2/1 kolom mengikuti lebar yang tersedia.
- Pencarian memakai `FindTools`/alias existing; kategori Halaman/Konversi/Edit/Keamanan dan favorit memakai ID alat asli. Tidak ada favorit contoh yang ditambahkan ke data pengguna. Favorit disimpan oleh `SaveLibrary` existing; error penyimpanan tetap ditampilkan.
- Hover/focus memakai transform -3 DIP selama 180 ms, sorotan biru, shadow, dan gerakan/skala ikon. Menghormati `UISettings.AnimationsEnabled`. Tidak mengubah ukuran layout kartu. Kontrol tetap keyboard-focusable dan memiliki automation names.
- Shell tanpa sidebar, dengan Beranda, kembali/maju, Riwayat, Menu. `Buka & baca PDF` memakai picker reader existing. Menu menyimpan Pengaturan/Bantuan/Assistant/Administrasi, command file/edit/zoom existing, dan Dokumen sesi agar pemilihan sumber lama tetap dapat diakses. File Diamankan tidak ada di navigasi baru; data dan route existing tetap ada.
- `MainWindow.cs`, `DesignedHome.cs`, `DesktopMenu.cs`: hanya hookup beranda dan wadah shell; catalog entry diarahkan ke beranda. `NavigationHistory.cs`: mempertahankan kembali ke halaman Dokumen sesi. `Program.cs` dan `CenteredHomeAudit.cs`: entry audit DEBUG terisolasi.
- Tidak ada tab contoh, label PROTOTIPE, dialog simulasi atau Tweak pada produk. Tidak membangun model multi-document baru.

## Bukti dan batas verifikasi

Evidence khusus task: `artifacts/home-centered-01a12669/`. Diff khusus perubahan ini: `own-diff.patch`; hash source: `source-manifest.json`. Output development: `app/`; log MSBuild: `build-native.log`; laporan native dan screenshot: `native/`.

Build CLI awal gagal pada resource WinUI karena task PRI tidak ada di SDK CLI. Build memakai MSBuild Visual Studio yang sudah tersedia dan dipakai skrip proyek. Tidak ada dependency baru/restore jaringan atau perubahan toolchain permanen.

Native audit memakai synthetic PDF, isolated-library.json, dan hasil salinan sintetis. Hasil container bukan bukti pipeline Save As/processing ulang. Pemeriksaan native mencakup lebar 1480/800/minimum yang tetap diberlakukan Windows, pencarian alias dan empty state, kategori, favorit persisten, reader preview, tombol zoom, back navigation, sesi editor, alat, hasil, dan menu destinations. Source hash harus tetap sama. Timing event TextChanged diselesaikan sebelum assertion. Screenshot ditulis dari RenderTargetBitmap aplikasi WinUI nyata melalui stream memori.

Browser menolak URL file lokal prototipe; tidak dicoba bypass. Screenshot native diperiksa, tetapi perbandingan screenshot prototipe-versus-native pada ukuran sama **belum selesai**. Computer Use tersedia dan dibaca; peluncuran interaktif mengalami approval timeout. Hover pointer fisik/flicker, physical keyboard end-to-end, screen reader, physical pinch/print, seluruh DPI, half-screen snap Windows, dan network-disabled end-to-end tidak diklaim lulus. Reduced-motion diverifikasi dari implementasi, tanpa mengubah setting OS. Tidak ada klaim Production/UAT/phase completion.

## Area terlindungi dan pengiriman

Pipeline worker/PDF, pencarian isi/highlight, renderer, editor, print/spooler, zoom implementation, Save As/result validation, installer/updater, dependencies, release feed dan nomor versi tidak diubah. Central phase state/register/log tidak berubah karena ini UI scope lokal.

Status pengiriman awal: paket belum dibuat pada task UI. Pada 11 Oktober 2026 pengguna meminta paket update lokal; hasil pengiriman tercatat di bawah. Instalasi dan feed aktif tidak diubah.

## Hasil final

Status **LOCAL_IMPLEMENTATION_VERIFIED_VISUAL_COMPARISON_PENDING**. MSBuild final exit 0; native audit final exit 0: **30 PASS, 0 FAIL**. `git diff --check` exit 0; protected tracked diff check tidak menemukan perubahan di luar scope dibanding baseline.

Identitas build development `home-centered-01a12669`, produk 0.2.4, SHA-256 DLL `208D0F7B6A9598FABAC44F6B1A6C0089BA6EDC294BCD3AFFC585705DF68248C5`. Ringkasan machine-readable: `artifacts/home-centered-01a12669/verification.json`. Screenshot native diperiksa: `home-final.png`, `home-640.png`, `keyboard-focus.png`, `reader.png`, dan `result.png` dalam folder native. Fokus keyboard terlihat jelas; gambar sintetis tampil pada reader/hasil. Physical UAT dan screenshot comparison tetap pending sebagaimana dijelaskan di atas.

## Paket kandidat lokal — 11 Oktober 2026

Status **LOCAL_CANDIDATE_PACKAGED_VERIFIED**. Produk tetap **0.2.4 · Pengembangan**, build `20261011-054806-66eec1fb`. Feed kandidat terisolasi: parent workspace `releases/windows-local-candidates/home-centered-01a12669/`; ZIP `OTO-local-candidate-0.2.4-20261011-054806-66eec1fb.zip`. Seluruh **1.333 file** dan isi ZIP diverifikasi hash/length terhadap manifest. Manifest SHA-256 `2CE3709610681451EEF7CDB8463EF7E133B94E88BCFEEC4BC0E45AA4943EB9C6`; ZIP SHA-256 `74763D22D800D44B8CC0C0985986593ECE638F9405E50C11AA1A4D34C721BF0D` (222.142.822 bytes).

Build pertama berhenti aman pada gate Word karena Office diperbarui. Pengguna secara eksplisit mengizinkan re-admission Word 16.0.20430.20146; hanya pin exact provider berubah, lihat WORD_TO_PDF.md dan `word-readmission.json`. Host mengompilasi pin yang sama, sehingga host/worker dibangun ulang. DLL final yang diuji dan dikemas: `E4D11BEFF2A3AA6764166A0DB323EC300AA22702EA104060125C86D69C77BC30`. Audit beranda final **30 PASS / 0 FAIL**; standard Build-LocalUpdate exit 0 termasuk worker PDF/editor/search, Word worker, Word UI **39 PASS**, quality **39 PASS**, PDF opener, release notes, product identity, dan distribution. Bukti lengkap: `artifacts/home-centered-01a12669/local-package-verification.json`, `local-package-build-admitted.log`, `local-package-delivery-admitted.log`, dan `local-update/`.

Runtime privat .NET 10.0.12 diverifikasi untuk lima komponen. Root feed aktif dan installed current.json diverifikasi tetap sama dengan baseline (0.2.3). Kandidat ini sengaja tidak dipromosikan ke updater lama: migrasi 0.2.3 memerlukan installer lengkap satu kali sesuai LOCAL_UPDATES.md. Tidak memasang aplikasi, mengubah shortcut, menerbitkan online, atau mengklaim UAT/Production. Batas physical UAT dan visual comparison di atas tetap berlaku.

## Instalasi lokal — 11 Oktober 2026

Atas perintah eksplisit pengguna untuk memasang dan menampilkan shortcut Desktop, installer migrasi lengkap dibuat dari kandidat exact di atas melalui Build-DownloadPackage.ps1, diverifikasi, lalu dijalankan melalui jalur console PASANG. Installer exit **0**. Installed current.json kini **0.2.4 / 20261011-054806-66eec1fb**; seluruh **1.333 file** installed cocok hash/length manifest. Launcher stabil dan runtime privat diverifikasi. Versi lama `20261009-035333-e218f12d` retained dan previous.json menunjuknya.

Shortcut existing `C:/Users/cahpo/OneDrive/Desktop/PERTAMINA OTO.lnk` terverifikasi mengarah ke `C:/Users/cahpo/AppData/Local/Programs/PertaminaOTO/PERTAMINA-OTO.exe`; Explorer dibuka dengan shortcut terpilih. Launcher berhasil membuka proses aplikasi dari exact installed version, dengan native window handle tersedia. Bukti: `install-package-build.log`, `install-execution.log`, `install-exit.txt`, `install-before.json`, `install-verification.json`, `installed-startup.json`. Instalasi ini menggantikan status belum dipasang pada laporan packaging terdahulu. Root active feed tetap tidak dipromosikan; online publication dan Production/UAT tidak dilakukan.
