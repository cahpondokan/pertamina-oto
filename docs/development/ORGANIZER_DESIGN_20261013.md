# Atur halaman PDF — implementasi desain 13 Oktober 2026

Scope ini adalah implementasi pengembangan lokal dari mockup `03-atur-halaman-pdf-sumber-kanan-hover.png`, catatan revisi hover, dan standar lintas fitur yang disetujui. Ini bukan eksekusi penuh phase, approval rilis, atau integrasi ke main. Baseline task: `db287ba9b59d81cc43cdd89c9eb2f042ba5a50a0`, branch `task/organizer-ui-20261013`.

## Perilaku

- Workspace loaded memakai grid susunan di kiri, daftar sumber dan Hasil susunan di kanan. Pada lebar sempit kedua panel ditumpuk dalam scroll halaman; grid tetap memiliki scroll sendiri. Toolbar loaded berisi Tambah PDF, Undo, Redo, dengan petunjuk paste/drop terpisah. Landing mengikuti komponen input Merge dan tetap memiliki Tempel file.
- Asset Biblo031 dipakai kembali byte-identical untuk organizer, termasuk home, hasil, proses, dan dialog. Mapping alat lain dipertahankan.
- Sumber diberi kode append-only A, B, …, AA. Label `B · Hal. 2` tetap menunjukkan provenance setelah perpindahan, sedangkan `Urutan N` dihitung dari susunan hasil. Duplikat memiliki ID instance berbeda. Halaman kosong diberi label `Kosong · A4`.
- Hover atau fokus keyboard memperlihatkan preview, putar, keluarkan halaman; gap insert terlihat di dekat halaman yang mendapat fokus. Tooltip/nama aksesibel, context-menu duplicate/move, mekanisme drag, bitmap cache, Undo/Redo dan zoom yang ada digunakan kembali.
- Paste global pada textbox/password organizer diserahkan ke input teks sebelum membaca clipboard. Adapter file tetap menjalankan batas input dan processing lokal yang ada.
- Simpan menggunakan WorkerClient dan validasi output yang ada, menghasilkan PDF baru dengan collision policy yang ada. Hasil memakai komponen saved-result Merge, reader/print/exact-file reveal yang sama, dengan ikon dan copy organizer.
- Tutup dokumen menggunakan modal Merge dengan copy organizer, fokus awal Kembali, Enter/Escape aman pada key-up, dan pilihan simpan/buang/kembali. Gagal atau cancel save mempertahankan susunan dan kata sandi dalam memori. Sukses close-save membuka landing dengan notice dan path hasil yang tepat. Tidak ada password di artefak evidence.
- Kembali edit memulihkan breadcrumb serta subscription resize. Reentry dari alat lain melepaskan kontrol dari parent aktual. Properti visual shared control yang diubah organizer dipulihkan sebelum alat lain dibangun.

## Verifikasi developer dan bukti

Lokasi evidence di root workspace: `artifacts/organizer-ui-20261013/implementation/`. `HANDOFF.md`, `candidate-ready.json`, `source-file-map.json`, `desktop-source-manifest.json` dan `candidate-files.json` mengikat source, baseline, build dan kandidat final. Hanya marker `READY_FOR_QA` beserta handoff lengkap yang menyatakan kandidat telah dibekukan.

Native audit `--audit-organizer-design <absolute-output>` menguji input dua PDF sintetis, provenance/instance, perubahan urutan, blank, rotasi, Undo/Redo, fokus/gap/context menu, nama panjang, ukuran minimum/normal/maximized, 100 halaman, zoom thumbnail, actual worker save, output count/dimensions, pembaca hasil, print preview tanpa printer, close failure/cancel/success/discard, source hash, reentry Merge dan callback Back/resize. Hasil berada di luar folder executable. Uji lama organizer dipanggil dengan direktori evidence eksplisit.

Regresi terkait menggunakan `--audit-merge-close-flow`, `--audit-organizer-transfer`, `--audit-saved-result-fit`, `--audit-merge-panel`, `--audit-merge-empty` dan `--audit-centered-home`. Audit clipboard melewati operasi clipboard global. Handoff membedakan uji pada DLL final dan uji pendukung pada build sebelumnya.

## Batas bukti

- Audit output developer membedakan source A/B melalui ukuran halaman; halaman berukuran sama di dalam satu sumber belum merupakan oracle urutan berbasis konten. QA independen perlu memakai fixture dengan label unik.
- Paste textbox diperiksa melalui source guard dan pengenalan target; clipboard OS tidak diubah. Pengujian fisik clipboard memerlukan lingkungan atau consent tersendiri.
- Enter/Escape/hold/repeat handler organizer belum dibuktikan dengan input fisik end-to-end. Native modal assertions membuktikan default focus dan pilihan/retensi; jangan menyamakan keduanya.
- Tidak ada actual print job, assertion seleksi Explorer, penggantian DPI/high contrast/multimonitor, screen reader, hardware pinch, atau human UAT dengan jaringan dimatikan. Local-only processing didasarkan pada jalur implementasi dan worker yang digunakan; bukan bukti network-isolated runtime.
- Pointer timing/animation smoothness memerlukan penilaian visual pengguna. Ukuran mockup adalah arah desain, bukan kontrak pixel-perfect.
- Build memakai dependency cache dan restore assets baseline yang sudah tersedia, tanpa download dependency baru. Refresh vulnerability online tidak berhasil pada percobaan restore awal dan tidak diklaim lulus. Lockfiles final dibandingkan dengan baseline.

## Audit kandidat tanpa mengubahnya

Kandidat adalah snapshot payload terpisah dari live bin/obj. Verifikasi manifest sebelum dan sesudah audit. Untuk menjalankan UI, QA dapat menyalin payload yang sudah diverifikasi ke `artifacts/organizer-ui-20261013/qa/runtime/app`, memverifikasi salinan, mengatur `OTO_ISOLATED_NATIVE_AUDIT=1` hanya untuk child process dan memakai output absolut di lane QA. Dengan demikian fallback data/log berada di sibling `NativeAuditData` di lane QA, bukan profile pengguna atau kandidat immutable. Jangan menjalankan audit lama yang menulis ke AppContext.BaseDirectory langsung dari kandidat.

Integrasi dilakukan Agent0 setelah handoff implementasi dan audit independen. Gunakan hanya delta dalam source-file-map; jangan mengintegrasikan seluruh backup tree atau dirty checkout sumber. Versi produk, dependency, engine/worker/print, website, installer/updater, registri phase dan current/latest tidak diubah oleh task ini.


## Putaran 2 — QA-01

Temuan P3 label aksesibilitas ditutup dengan membatasi inferensi `Salinan halaman` ke halaman sumber PDF (`Source >= 0`). Blank A4 menggunakan sentinel bersama dan tidak menyimpan lineage duplikasi; karena itu semua blank diberi label kosong A4 tanpa menyimpulkan provenance salinan. ID instance, payload layout, mekanisme duplicate/Undo dan worker tidak berubah. Label salinan untuk halaman sumber yang berulang tetap dipertahankan.

Regression tambahan memeriksa dua blank hasil insert independen, label/ID blank duplikat, source duplicate, kemurnian refresh label terhadap state layout, dan Undo kembali ke fixture save sebelumnya. Evidence putaran ini berada di `artifacts/organizer-ui-20261013/implementation/round2`; marker dan payload build15 tetap immutable. QA independen menangani gap OUI-04/OUI-06/OUI-07. Implementer tidak memakai clipboard atau input fisik.

## Round3 — OUI-07 integration seam

Callback down/up organizer diekstrak per instance dan dipakai bersama oleh native PreviewKeyDown/PreviewKeyUp dan audit DEBUG. Observer DEBUG menangkap dialog setelah Opened/fokus Kembali; referensi callback dibersihkan pada Closed. Cabang Merge, UI, safe default dan semantik closure/queue dipertahankan.

Audit melalui flag organizer existing memakai CloseDocumentsAsync/ContentDialog.ShowAsync nyata, DispatcherQueue nyata dan tombol AutomationPeer nyata. Kasus K01–K10 (13 skenario) mencakup repeat Enter/Escape, kedua release order, Save/Discard ketika satu/dua tombol held, re-press sama/lain sebelum queued close, unknown key, up tanpa down, duplicate up/idempotence dan dialog baru. Fixture memuat Undo/Redo nonkosong, rotasi, password sintetis, hasil PDF dan checkpoint. Retensi dibandingkan saat held dan setelah None: seluruh layout, isi kedua stack, source, password hanya di memori, selected tool/output/checkpoint dan file set/hash PDF. Trace tidak menyimpan password.

Kelas bukti adalah native programmatic event-sequence integration. Tidak membuktikan physical hold/OS auto-repeat maupun seluruh routed-event path; keputusan handled callback diuji, adapter ke e.Handled diperiksa pada source. Drag Explorer manual masih gate manusia terpisah. Status build/test/freeze yang otoritatif ada di implementation/round3/HANDOFF.md dan candidate-ready.json; build15/round2 tetap immutable.
# Round4 user correction — save color and input rejection

On 13 October 2026 the user directly requested this correction in the QA chat. This scope changes that chat's role to implementation; its new tests are developer verification and do not replace independent acceptance. Frozen round3 and its QA-02 human finding remain intact.

The shared run button lost its constructor colors after a generic tool reset. Organizer now applies a scoped native AccentButtonStyle with template-blue normal, darker hover/pressed, white enabled text and a distinct disabled state. Previous resources/style/local colors are restored when leaving Organizer. High Contrast keeps the system resource fallback; live High Contrast has not been tested.

Rejection feedback now uses a visible warning InfoBar next to the add/drop area. Subscription belongs to the always-present canvas instead of the initially collapsed message, and reconnects when returning from saved results. This removes the hidden-message subscription dependency. The old physical-drop symptom was human-reported; its exact original routed-event sequence was not reproduced. Import filtering and processing are unchanged.

Regression uses actual local StorageItems through DataPackage and the production import adapter: invalid-only retention including Undo/Redo, mixed valid/invalid import, notification after refresh/collapse/result Back, source-byte preservation, minimum-width visibility, primary-button interaction-state renders and resource restoration. This is native programmatic evidence, not a physical Explorer drag. Existing Organizer/close/transfer suites also ran on the exact same DLL. Evidence and candidate identity: `artifacts/organizer-ui-20261013/implementation/round4-user-fix/` in the parent workspace. No installation, feed publication or phase status change.

