# Dokumentasi visual Windows — 8 Oktober 2026

Checkpoint 9 Oktober: slot maksimal telah diberikan kepada task05 dengan `MaxCaptureReady=true`. DLL kandidat 0.2.3 terverifikasi `053781FA261C1DD8873748AE8CE0390CA1E92BC640AB94AB1BB699E3F833E9EF`, source manifest `02D94048DFF0EFD9EAA5AC49DE66918DE024EA6E0F05F430E8B3731B720D4A6B`. Namun computer-use gagal saat initialization, `failed to write kernel assets` / `os error 3`, termasuk satu reset/retry. Tidak ada launch, PID milik task05, input, atau screenshot baru. `capture-session.json` berstatus `released` agar task04 melanjutkan regresi. Bukti: `capture-runtime-failure.json`. Ini blocker runtime, bukan menunggu minimum untuk batch maksimal. Minimum/Snap tetap pending; kandidat belum diklaim release. `Build-Gallery.ps1` merupakan generator checkpoint baseline awal; jangan menjalankannya untuk menimpa manifest checkpoint terbaru.

Target dokumentasi adalah aplikasi pengembangan **versi terakhir setelah audit dan penerapan ukuran minimum selesai**, sesuai klarifikasi manusia di chat ini. Versi yang diunggah di website bukan acuan. Upload berikutnya dilakukan pengguna setelah ukuran minimum selesai; task dokumentasi tidak melakukan upload.

Galeri persiapan: `../../../artifacts/user-guide-20261008/gallery.html`. Katalog aktual mempunyai 16 alat aktif dan 17 alat belum tersedia. Inventaris source menentukan daftar dokumentasi, bukan bukti seluruh fitur telah lulus UAT.

## Status dan urutan

1. Inventaris, naskah, matriks cakupan dan kerangka galeri dapat dikerjakan sekarang.
2. Task04 memiliki source/build dan native UI. Jangan capture atau mengambil fokus sampai `NativeUiOwner=task05` atau handoff eksplisit tersedia.
3. Seluruh screenshot final maksimal, Snap kiri, Snap kanan, dan minimum exact memakai satu DLL final terverifikasi dari task04. Catat path, build, hash dan fixture setiap gambar. Angka minimum menunggu `DocsMinimumReady=true`, alasan ukuran dan bukti uji/visual.
4. Snapshot 0.2.2 build `20261008-084354-201e4764` hanya baseline historis untuk perbandingan. Jangan memasukkannya sebagai ilustrasi versi final. Aplikasi terpasang dan versi website tidak diasumsikan sama dengan target.
5. Mode Snap yang terpotong atau tidak layak diberi `NOT_READY` dengan bukti, bukan contoh UI baik. Screenshot client dari audit tidak dianggap bukti native frame atau Snap.
6. Review galeri mendahului PPT. Rencana PPT: maksimal 12 gambar per slide (4 kolom × 3 baris), dikelompokkan per alur OTO. Judul/frame editable dan screenshot individual pada resolusi asli. Belum membuat PPT final.

## Naskah singkat dan pemilihan gambar

| Alur | Langkah dan hasil yang dijelaskan | Target gambar ringkas |
|---|---|---|
| Navigasi | Mulai dari Beranda, Pembaca PDF atau Semua Alat. Nama dan status kartu mengikuti aplikasi final. | Beranda dan katalog; satu state menu bila perlu |
| Input | Pilih file contoh yang sesuai alat. Periksa daftar, urutan dan halaman sebelum proses. Paste/drop hanya dicantumkan setelah label dan perilaku final terverifikasi. | Intake kosong reusable dan intake berisi contoh |
| Pembaca PDF | Buka PDF contoh, gulir antarahalaman, atur zoom, cari teks dan pindah kemunculan. | Reader loaded + search; empty reader bila instruksi perlu |
| Organize / Merge | Susun file atau halaman, periksa urutan, lalu proses salinan hasil. | Merge loaded dan organizer loaded |
| Split / Extract / Remove | Tentukan halaman/rentang sesuai label, periksa preview sebelum menyimpan hasil. | Satu state bermakna per fitur, hindari ulang intake identik |
| Rotate / Crop | Putar halaman atau batasi area tampil. Crop bukan penghapusan permanen/redaksi. | Preview perubahan dengan PDF contoh |
| Compress | Kompresi stream lossless. Ukuran dapat tetap atau bertambah; tidak menjanjikan pengurangan kualitas gambar. | Opsi dan hasil yang relevan |
| JPG to PDF / PDF to JPG | Susun JPG/PNG menjadi PDF A4; ekspor halaman PDF sebagai JPG sesuai preset kualitas. | Input gambar dan hasil multipage/folder |
| Word to PDF | DOCX A4 portrait memerlukan Microsoft Word lokal berlisensi. Format/ukuran lain mengikuti batas katalog aktual. | Intake dan keterangan syarat; hasil hanya bila environment sah tersedia |
| Editor | Edit teks asli yang didukung atau tambahkan teks/gambar/bentuk; simpan salinan PDF baru. Jelaskan keterbatasan font/scan. | Editor loaded, alat edit/properti, hasil simpan |
| Watermark / Page numbers | Atur teks/nomor dan halaman sasaran, periksa hasil. | Opsi dan preview hasil |
| Protect / Unlock | Lindungi PDF dengan sandi baru; unlock membutuhkan sandi pemilik yang sah. Tidak menampilkan sandi/credential pada gambar. | Intake/opsi aman dan status, tanpa transaksi keamanan pengguna |
| Hasil | Verifikasi nama hasil dan lokasi. `Buka hasil`, `Cetak hasil`, `Tampilkan di folder` dijelaskan dari label aktual. Sumber dipertahankan. | Hasil PDF tunggal, hasil banyak file bila berbeda, preview |
| Print | Buka dialog, periksa preview/opsi/printer. Tidak kirim pekerjaan cetak. | Dialog yang aman; keterbatasan hardware dicatat |
| Riwayat / Favorit / File Diamankan | Tinjau metadata lokal dan akses yang tersedia pada profil contoh. | Empty/loaded secukupnya; tanpa dokumen pribadi |
| Pengaturan / update | Tinjau label final, status koneksi dan catatan rilis. Jangan unduh, instal, restart, atau ubah default pengguna. | Pengaturan aman, catatan rilis, dialog hanya bila tanpa transaksi |
| Bantuan / Assistant / Administrasi / Tentang | Jelaskan status aktual. Jangan mengklaim AI/administrasi tersedia dari keberadaan menu saja. Tentang mencatat versi final. | Satu halaman per fungsi, state belum tersedia jujur |

## Kontrak capture dan review

Gunakan PDF/gambar sintetis bertanda contoh dan profil terisolasi yang didukung. Tidak membaca dokumen pribadi untuk demonstrasi. Jangan mengubah clipboard, shortcut, default settings, installed pointer, release feed, website atau log phase. Tidak rebuild dari task dokumentasi.

Manifest wajib membedakan resolusi monitor physical/work area, Windows scale, RasterizationScale, outer px dan client DIP; mode maximal/left/right/minimum, PID, binary/build/hash, route/state, fixture, ukuran asli dan hash gambar, native frame, temuan clipping dan hasil review. Nilai belum diamati tetap null, bukan perkiraan.

Gambar asli terpisah, tidak retouch state UI. Frame putih/navy dan label ditempatkan di luar screenshot; crop privasi harus berlabel dan raw dipertahankan lokal. Periksa image dimensions, link, overflow galeri, caption aktual dan konsistensi build. Tidak mengklaim seluruh DPI/perangkat atau hardware UAT.

Referensi read-only: alur GateLog Desktop Security/Super Admin dan PPT 12 gambar, serta OTO `ToolCatalog.cs`, `RESULT_ACTIONS_FIX.md`, `PRINT_PATCH_0_2_1.md`, `RESPONSIVE_LAYOUT.md`, `WEBSITE_GUIDE.md`. Guide website lama menyebut 18 aktif; katalog desktop aktual 16 menjadi inventaris task ini.

## Final handoff task04 — 9 Oktober 2026

Gate minimum tersedia:640×760clientDIP pada aktual1920×1080/100%, work1920×1032. Native regressions3890 PASS/0FAIL dan dedicated107 PASS dilaporkan task04; task05 tidak menjalankan ulang tes aplikasi. Rilis lokal0.2.3 `20261009-035333-e218f12d` manifest76065EE2DD5BE1B7AFD8CB9396968B5148BCBB923D5B229E9ED26CB726F874D0 dan DLL053781FA261C1DD8873748AE8CE0390CA1E92BC640AB94AB1BB699E3F833E9EF diverifikasi langsung. Source manifest cocok dengan handoff. Field CaptureLimitation/AuditProgress pada handoff masih menyebut gate pending; bukti DocsMinimumReady, native-proof dan build immutable yang lebih baru menjadi checkpoint ini.

Recovery sesi task05 melalui reset/init tetap gagal oserror3; tidak menyatakan runtime global gagal. Slot kembali released tanpa launch/input. Dua gambar native reader dari task04 (minimum dan Snap kanan) diperiksa lalu disalin byte-for-byte ke raw; hash dan ukuran asli tercatat. Gambar maksimal/editor serta Snap kiri tidak dimasukkan karena footer memuat folder pengguna. Bukti asal tetap milik task04, tanpa retouch atau penyembunyian temuan. Galeri mempunyai dua contoh final, bukan dokumentasi seluruh fitur selesai. Maksimal, kiri, intake/tools/result/settings dan cakupan lain pending. Mode minimum reader tidak membuktikan semua fitur/DPI/perangkat. Review browser render masih terhalang kebijakan file protocol; seluruh link/image path diperiksa secara statis. PPT belum dibuat dan belum ada approval galeri.

Clean reader maximum supplement: final-reader-maximized-clean-native.jpg visually reviewed, 1920×1032 origin0,0 scale1, synthetic3page, no private folder visible. Hash CED67524923090C6082AF7066A3FBBAE6806AB803710C7A2C4C2A38E5FD41E45 matches provenance and imported bytes. Gallery now3 final native reader images (maximum, minimum, rightSnap). Full feature inventory remains pending; no native input or build repeated.

PPT draft requested:14 views/slide using 4+4+3+3 row layout. Current reviewed coverage3 screenshots only. Draft one slide, individual images and editable labels/frame. Original image byte hashes checked in PPT media. Remaining11 views required before one full14-view slide. Raster zoom limit remains original pixels. No new source/build/UI input. PPT finalization passes structural/layout/import checks, native PowerPoint not inspected.

Delivered PPT revision16:48 views in3slides, exactly16/slide. All33catalog tool pages represented including17unavailable states;12navigation/result states plus3native readers.45client audit renders1920×930 exceptSettings privacycrop1920×700. This completes page inventory illustration, not all loaded workflows/modals/native-window size coverage.48embeddedimage hashes match supplied assets. No resampling inPPT. Quality receipt ppt-build/quality-16.json; finalizationreceipt validation-16-r2.json. All three slide renders inspected, footer clearance repaired. Old14draft retained historical only.

Lifecycle extension request: all active features before/during/after. Current verified real lifecycle coverage during0/16 after0/16. New feature-lifecycle-coverage.csv distinguishes before empty, configured loaded, real processing, real feature-specific results. Generic synthetic result and DEBUG mock Print are not actual processing evidence. Runtime reset/init still fails oserror3; UIreleased without input. Existing page PPT retained, not claimed lifecycle complete.
