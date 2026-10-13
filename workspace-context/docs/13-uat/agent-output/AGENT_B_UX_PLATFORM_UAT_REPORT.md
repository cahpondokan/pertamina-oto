# AGENT B — UX, PLATFORM & ACCESSIBILITY UAT REPORT

**Task ID:** `DOC13-AGENT-B`  
**Peran:** Senior UX System Analyst; Cross-Platform UX Architect; UAT Specialist; Senior Desktop/Web/Mobile Architect  
**Status:** Rekomendasi specialist untuk integrasi ke Document 13; bukan hasil UAT dan bukan perubahan scope produk  
**Sumber berwenang:** Document 01–12 FINAL

Dokumen ini merancang UAT berdasarkan hasil yang dipahami pengguna pada capability yang memang dirilis untuk setiap platform. Kesamaan aturan bisnis, Feature ID, otorisasi, keamanan sumber, dan finality wajib dibuktikan; kesamaan bentuk UI tidak diwajibkan. Perbedaan antarmuka yang sesuai perangkat adalah benar, sedangkan capability yang hilang tanpa penjelasan, tindakan buntu, fallback diam-diam, atau hasil yang menyesatkan adalah kegagalan.

---

## 1. UX acceptance strategy

### 1.1 Sasaran penerimaan

UAT harus membuktikan bahwa pengguna nyata dapat:

1. menemukan fungsi penting melalui **File-First**, **Tool-First**, atau **Assistant-First** sesuai exposure platform;
2. memahami input, parameter, perubahan, lokasi eksekusi yang relevan, risiko, dan langkah berikutnya;
3. menyelesaikan outcome tanpa jalur membingungkan, dead-end, atau dialog yang tidak perlu;
4. membedakan preview/working state, hasil sementara, hasil final, dan file sumber;
5. memahami keterbatasan platform serta alternatif yang tersedia;
6. melihat progress, waiting state, keberhasilan, kegagalan, dan recovery tanpa silent wait;
7. bekerja dengan pola input yang natural untuk Desktop, Web, Phone, atau Tablet;
8. menyelesaikan alur dalam Bahasa Indonesia dan English tanpa perubahan identifier teknis atau business rule;
9. menggunakan fungsi penting dengan accessibility/usability dasar yang sesuai platform;
10. gagal dengan aman: sumber tidak rusak, tidak ada upload/overwrite/share/delete diam-diam, dan tindakan berikutnya jelas.

Indikator bisnis Document 01 bahwa tool utama dapat ditemukan maksimal dalam dua interaksi digunakan untuk tool utama pada surface yang memang diekspos. Ini tidak berarti setiap fungsi sekunder harus selalu berada dua interaksi dari Home.

### 1.2 Prinsip penilaian adaptive

```text
Canonical Feature dan business outcome sama
                +
platform exposure yang dirilis
                +
device capability nyata
                +
permission/policy/connectivity/resource saat ini
                ↓
UI platform-natural: execute / limited / fallback / hide / explain
```

- Desktop dinilai sebagai primary full-feature productivity client.
- Phone dan Tablet dinilai sebagai companion terkurasi; Tablet boleh lebih kaya daripada Phone.
- Web dinilai sebagai kanal enterprise/browser untuk account, history, selective sync, administration/audit/support sesuai permission, browser-safe tools, dan server-required flow.
- Variasi layout, control, urutan gestur, atau adapter input tidak menyebabkan FAIL jika outcome, keamanan, kejelasan, dan capability contract tetap benar.
- `N/A` hanya sah bila release scope/platform exposure membuktikan capability memang tidak berlaku dan jalur yang terlihat tidak berakhir buntu.
- Capability yang dijanjikan oleh release tetapi tidak dapat diselesaikan adalah `FAIL`, bukan `N/A`.
- PoC/approval yang belum selesai adalah `BLOCKED`, bukan PASS dan bukan alasan menghapus Feature.

### 1.3 Unit penerimaan dan cakupan sesi

Setiap case UAT minimal mencatat:

| Dimensi | Isi minimum |
|---|---|
| Outcome | Tujuan pengguna dalam bahasa bisnis |
| Traceability | Requirement ID, canonical Feature ID/resource, UAT Case ID |
| Persona | User, Administrator, Auditor, atau profil Support berizin; tidak ada role Superadmin tanpa batas |
| Platform | Desktop, Web, Phone, atau Tablet beserta versi OS/browser/app yang diuji |
| Kondisi | Exposure, capability perangkat, permission, policy, koneksi, locale, orientation/window |
| Data | Dataset sintetis/sanitized yang disetujui; ukuran/jenis hanya merujuk manifest |
| Langkah | Tindakan yang terlihat oleh pengguna, bukan detail implementasi |
| Harapan | Outcome, feedback, fallback/recovery, dan forbidden side effect |
| Evidence | Catatan dan screenshot/video aman hanya bila dibutuhkan |
| Hasil | `PASS`, `FAIL`, `BLOCKED`, `NOT_RUN`, `INCONCLUSIVE`, atau `N/A` sesuai Document 12 |
| Defect | Dampak pengguna, scope platform/capability, workaround aman, dan traceability |

Satu sesi tidak dianggap representatif untuk seluruh platform. Kombinasi yang dirilis harus mencakup kondisi daring/luring yang relevan, permission allow/deny, locale Indonesia/English, ukuran viewport/orientation yang didukung, serta adapter perangkat nyata untuk klaim camera, scanner, share, stylus, credential store, lifecycle, dan accessibility.

### 1.4 Entry, evidence, dan exit

UAT dimulai hanya setelah paket entry Document 12 tersedia: immutable release candidate, UAT lane terkontrol, smoke/gate yang berlaku lulus, account/permission/data/platform siap, known issue terdokumentasi, serta jalur defect/retest tersedia. UAT bukan tempat menjalankan load, malicious corpus, penetration, debug, atau direct database testing.

Evidence tidak boleh memuat password, token, secret, isi dokumen produksi, OCR lengkap, clipboard, aset tanda tangan, atau screenshot yang tidak disetujui. Screenshot UAT harus memakai data sintetis/sanitized, mengambil area minimum, dan mengikuti akses serta retensi evidence yang disetujui.

Exit UX/platform hanya dapat PASS apabila seluruh skenario critical yang berlaku selesai, tidak ada dead-end atau misleading success, platform limitation memiliki penjelasan/fallback, seluruh Blocker/Critical ditutup, serta Major yang tersisa memiliki disposition resmi. Hasil teknis Document 12 tidak menggantikan sign-off pengguna/business pada Document 13.

---

## 2. Navigation scenarios

| Surface | Tugas UAT | Kriteria penerimaan |
|---|---|---|
| **Home** | User baru mencari satu tool utama, melanjutkan recovery/activity yang relevan, dan berpindah ke tiga entry flow | Tool utama pada surface yang dirilis ditemukan maksimal dua interaksi; entry penting berlabel jelas; state account/offline/recovery terlihat; tidak ada card mati atau akses yang melampaui permission |
| **Feature Search** | Cari dengan nama resmi dan istilah natural dalam Indonesia/English, misalnya “kecilkan PDF”/“compress PDF” | Hasil mengarah ke Feature canonical yang benar; label boleh dilokalkan tetapi Feature ID tidak berubah; unsupported/hidden Feature tidak bocor atau membentuk dead-end; input/parameter selanjutnya jelas |
| **File-First** | Masukkan file melalui adapter yang didukung lalu pilih tindakan relevan | Sistem mengenali jenis/jumlah file, memvalidasi, dan menawarkan 3–5 tindakan relevan bila rekomendasi tersedia; action yang dipilih menuju contract/permission yang sama; file invalid tidak membuat seluruh batch valid gagal tanpa penjelasan |
| **Tool-First** | Temukan tool melalui kelompok/card/search, pilih input, atur parameter, preview, proses | Tujuh kelompok Core dan card yang dirilis mudah dipahami; input dan parameter yang diperlukan terlihat; tidak ada implementasi bisnis paralel; working state dibedakan dari final result |
| **Assistant** | Nyatakan outcome sederhana dan multi-step; uji intent ambigu, offline, serta langkah berisiko | Assistant adalah jalur alternatif; plan menampilkan langkah, file, output, kebutuhan server, dan risiko; ambiguitas meminta klarifikasi; setiap langkah memakai permission/capability yang sama; success hanya berasal dari hasil tool aktual |
| **History** | Lihat aktivitas pribadi, buka hasil/lokasi bila tersedia, reprocess, dan hapus entry | Hanya data milik user yang terlihat; initial loading/lazy loading sesuai contract; reprocess mengecek keadaan saat ini; hapus History tidak menghapus source/result/Audit; action yang tak berlaku pada platform dijelaskan |
| **Activity Center** | Pantau local job, server job, sync, recovery, waiting, failed, completed, dan cancel/retry/resume | Status lintas entry flow konsisten; Job ID/Correlation ID tersedia pada detail aman bila perlu; state tidak mundur; event realtime yang hilang dipulihkan melalui state berwenang; next action selalu jelas |
| **Secured Files** | Buka daftar, unduh/buka, Amankan ke Akun, buat versi, restore, recycle/delete sesuai izin | Daring dan ownership diverifikasi; Amankan selalu explicit; tidak ada auto-sync draft/source/cache/temp; maksimum tiga versi terlihat dan dipahami; aksi destructive mengikuti confirmation/reauth/policy |
| **Settings** | Ubah preferensi user yang tersedia; akun privileged membuka konfigurasi sesuai permission | Preferensi lokal/allowlisted account tidak berubah menjadi full workspace sync; item privileged hanya tampil sesuai projection; denial/update-required dijelaskan; perubahan tidak mengubah Feature ID/business meaning |
| **Help** | Cari bantuan fungsi, error, recovery, capability limitation, dan cara melapor masalah | Bantuan kontekstual menggunakan istilah user; tidak menjanjikan capability yang tidak tersedia; memberi jalur kembali ke pekerjaan atau recovery; detail teknis sensitif tidak dipaparkan |
| **Issue Reporting** | Dari menu dan dari error, tulis pesan, ambil screenshot bila didukung, preview/markup/remove, lalu kirim | Hanya pesan singkat wajib; screenshot opsional dan tidak auto-upload; safe context ditambahkan otomatis; kegagalan attachment tidak menghilangkan pesan; status user sederhana dan halaman tidak menjadi form ticket teknis |

### 2.1 Parity antarjalur

Untuk satu Feature representatif yang diekspos, UAT menjalankan outcome yang sama melalui File-First, Tool-First, dan Assistant-First. Yang harus setara adalah Feature canonical, normalized input/order/parameter, permission/policy, resolver, confirmation, source safety, final result, History/Audit outcome, dan error semantics. Perbedaan susunan layar atau jumlah gestur yang wajar untuk platform boleh diterima.

### 2.2 Direct route dan jalan kembali

Deep link, browser refresh/back, resume aplikasi, recent item, notification, dan link dari Activity Center harus berakhir pada salah satu keadaan aman: halaman/action berwenang, penjelasan unavailable/online-required/update-required, atau navigasi kembali yang jelas. Route/menu yang tidak berwenang tidak boleh membocorkan resource, count, atau capability tersembunyi.

---

## 3. Desktop UAT

| ID | Skenario pengalaman | Acceptance yang diharapkan |
|---|---|---|
| `UAT-DESK-001` | Drag-and-drop satu file, multi-file, dan folder yang diizinkan | Target drop terlihat; urutan/input role dapat dipahami; file melewati validasi yang sama dengan picker; folder watcher tetap opt-in dan tidak memproses otomatis |
| `UAT-DESK-002` | File picker dan multi-file selection untuk Merge/batch | Pemilihan, urutan, duplicate/warning, dan file bermasalah terlihat; item valid dapat dilanjutkan sesuai mixed-batch rule |
| `UAT-DESK-003` | Keyboard-only pada Home, Search, Tool, editor, dialog, Activity Center, Settings, Help | Focus order/indicator/restore benar; seluruh aksi kritis dapat dicapai; shortcut yang dirilis tidak mengubah business rule dan hasilnya setara dengan mouse |
| `UAT-DESK-004` | Mouse interaction pada selection, reorder, crop, preview, resize signature | Target, hover/selected state, drag handle, dan undo terlihat; salah klik yang dapat dipulihkan tidak memicu dialog berlebihan |
| `UAT-DESK-005` | Explicit paste file/gambar/screenshot | Clipboard hanya dibaca setelah gesture Paste; preview dan nama sementara muncul; content tetap lokal sampai user memilih proses/transfer/save; tidak ada auto-upload atau clipboard history aplikasi |
| `UAT-DESK-006` | Large workspace, banyak halaman/file, multi-panel, resize window, DPI/text scaling | UI tetap dapat digunakan; lazy loading/virtualization tidak menyembunyikan selection/progress; tidak overflow/clipping; action penting tetap terlihat; target kinerja memakai baseline/target Document 12, bukan penilaian angka rekaan |
| `UAT-DESK-007` | Proses berat melalui Local Worker | UI tetap responsif secara praktis; progress muncul; source tetap aman; crash Worker tidak menutup UI; user mendapat retry/resume/cancel yang benar sesuai checkpoint |
| `UAT-DESK-008` | App close/crash/restart saat draft/job aktif | Startup menawarkan Lanjutkan/Coba Lagi/Batalkan sesuai state; output parsial tidak dianggap final; edit tidak hilang diam-diam; password diminta kembali bila diperlukan |
| `UAT-DESK-009` | Save As/final export dan overwrite target yang sudah ada | Ringkasan perubahan terlihat; default menghasilkan output baru; overwrite hanya pada exact selected target dengan confirmation; staged/failed output tidak merusak target lama |
| `UAT-DESK-010` | Undo/Redo selama edit | Perubahan rutin dapat di-undo/redo, feedback ringan terlihat, dan mekanisme tidak dipresentasikan sebagai version history file |
| `UAT-DESK-011` | Visual Sign PDF | Upload/paste gambar transparan dan saved signature yang terlindungi dapat dipilih; drawing mouse tersedia, stylus hanya bila device mendukung; preview, move, resize, final confirmation berfungsi; non-PKI dijelaskan; aset tidak auto-sync |
| `UAT-DESK-012` | Offline local-capable lalu server-required | Local-capable tetap berjalan tanpa processing API; server-required menampilkan Online/Koneksi Diperlukan atau waiting yang actionable; tidak ada perpindahan server/upload diam-diam |
| `UAT-DESK-013` | Progress/Activity Center dari tool biasa dan Assistant | Local IPC dan server activity terlihat dalam bahasa user; fast/indeterminate/background behavior tepat; detail Job/Correlation dapat disalin dari detail aman jika dibutuhkan untuk support |
| `UAT-DESK-014` | Paste screenshot ke Issue Reporting | Capture menargetkan surface aplikasi bila capability mendukung; preview/markup/remove mendahului explicit Send; user dapat mengirim pesan tanpa attachment setelah kegagalan/pilihan |

Desktop tidak lulus hanya karena tool dapat dieksekusi. Alur dianggap gagal bila keyboard kehilangan focus, workspace besar membuat aksi final tidak terjangkau, Local Worker crash mematikan UI, Save As menimpa source, atau paste/signature menyebabkan transfer yang tidak disetujui.

---

## 4. Web UAT

| ID | Skenario pengalaman | Acceptance yang diharapkan |
|---|---|---|
| `UAT-WEB-001` | Viewport lebar/sempit, zoom/reflow, browser text scaling | Layout responsive tanpa horizontal overflow yang memutus tugas; label/action penting tetap terbaca; tabel/list/detail memakai pola yang layak browser |
| `UAT-WEB-002` | Keyboard, focus, browser back/forward/refresh | Navigation dan dialog dapat digunakan keyboard; focus dipulihkan; refresh/back tidak mengulang mutasi/upload/job dan tidak menghasilkan false success |
| `UAT-WEB-003` | Feature discovery browser-safe | Hanya tool yang resmi diekspos dan capability/policy-compatible muncul; Feature lokal/native yang tidak tersedia tidak ditiru sebagai control mati; alternatif platform/server dijelaskan bila disetujui |
| `UAT-WEB-004` | Upload file untuk server-required/browser-safe flow | User melihat tujuan transfer, size/policy validation, consent, progress, serta safe error; transfer streaming tidak terlihat “membeku”; cancel/retry tidak membuat upload/job ganda |
| `UAT-WEB-005` | Response `202 Accepted` | UI menyatakan pekerjaan **diterima untuk background**, bukan selesai; Activity/Job link tersedia; status dapat dipulihkan setelah tab ditutup/dibuka bila masih berwenang; Job ID dan Correlation ID tetap konsisten |
| `UAT-WEB-006` | SignalR disconnect/gap/reconnect | Realtime ditandai degraded tanpa menyatakan job gagal; REST mengambil state terbaru; terminal state tidak mundur; perintah execute tidak dikirim ulang saat reconnect |
| `UAT-WEB-007` | Result manifest, download, range/resume bila didukung | Result `AVAILABLE` dapat diunduh setelah reauthorization; nama/type/size aman; browser menawarkan download/Save As; local source tidak ditimpa; result yang belum siap dan kedaluwarsa dibedakan |
| `UAT-WEB-008` | Result expired | `RESULT_EXPIRED`/HTTP 410 diterjemahkan menjadi pesan mudah dipahami, tanggal expiry bila tersedia, dan opsi rerun/reprocess yang eksplisit; UI tidak mengulang download tanpa akhir atau mengklaim file hilang lokal |
| `UAT-WEB-009` | Account, History, Secured Files | Ownership dan state terkini benar; selective sync terlihat sebagai Amankan ke Akun, bukan cloud workspace; maksimal tiga versi dipresentasikan tanpa membingungkan Save vs Save as New File |
| `UAT-WEB-010` | Administrator, Audit, dan Support | Halaman/action hanya sesuai permission terpisah; Administrator/Support/Auditor tidak mendapat isi dokumen; list/filter/export ber-scope; direct URL ditolak aman dan menyediakan jalan kembali |
| `UAT-WEB-011` | Issue Reporting dan own status | Form minimal, screenshot opsional bila capability browser mendukung, upload explicit, safe error, dan status user sederhana; internal triage tidak dibebankan kepada user |
| `UAT-WEB-012` | Offline/cached page | Asset yang masih tampil tidak dianggap business authority; tindakan online menampilkan Online Required; tidak ada mutasi queued tersembunyi atau local business promise yang tidak didukung arsitektur |
| `UAT-WEB-013` | Multi-tab dan stale state | Confirmation, permission, ETag/state conflict, dan result status tidak diambil dari tab lama; konflik ditampilkan actionable tanpa silent overwrite |

Browser matrix dan versi yang didukung harus berasal dari release capability matrix. UAT tidak boleh menyatakan “semua browser” hanya karena satu browser lulus.

---

## 5. Phone UAT

| ID | Skenario pengalaman | Acceptance yang diharapkan |
|---|---|---|
| `UAT-PHONE-001` | Portrait dan landscape pada layar/scale yang didukung | Konten reflow tanpa action kritis tersembunyi, overflow, atau tertutup safe area/keyboard; perubahan orientation mempertahankan working state |
| `UAT-PHONE-002` | Touch navigation dan editing terkurasi | Target sentuh praktis, selection/state terlihat, tidak bergantung pada hover, dan gesture bukan satu-satunya cara untuk tindakan kritis |
| `UAT-PHONE-003` | Camera Scan to PDF | Foreground permission jelas; capture, retake, edge detection, crop/perspective correction, reorder multi-page, preview, dan selesai mudah dijangkau; penolakan permission memberi recovery tanpa dead-end |
| `UAT-PHONE-004` | File picker dan Share/Open With pada release yang menyertakannya | File yang dipilih/dibagikan masuk ke validasi canonical; app switch/cancel tidak menggandakan input; locator yang tidak lagi valid meminta user memilih ulang |
| `UAT-PHONE-005` | Visual signature dengan finger/stylus bila didukung | Canvas menyediakan clear/undo; hasil dipreview, dipindah, dan di-resize sebelum final; aset tidak disimpan tanpa pilihan; non-PKI dan final confirmation jelas |
| `UAT-PHONE-006` | Curated read/crop/rotate/reorder/OCR ringan/merge/compress | Parameter disederhanakan tanpa mengubah makna; source aman; fitur yang tidak diekspos tidak muncul sebagai control mati; kebutuhan Desktop/server dijelaskan |
| `UAT-PHONE-007` | Background/resume, lock/unlock, app termination, network switch | Draft/checkpoint yang layak dipulihkan; tidak ada duplicate upload/job; stale permission direfresh; output sementara tidak disebut final |
| `UAT-PHONE-008` | Offline curated local vs online-required | Local eligible dapat berjalan sesuai protected grant; Amankan ke Akun/server/Issue submit menunggu atau meminta online; tidak ada auto-upload saat koneksi kembali tanpa intent/consent yang masih valid |
| `UAT-PHONE-009` | Activity Center/status | Job lokal/server yang relevan dapat ditemukan setelah berpindah layar; waiting connection/resource, retrying, failed, cancelled, dan completed dibedakan; next action dapat dijalankan dengan touch |
| `UAT-PHONE-010` | Share/export hasil | Hanya hasil tervalidasi yang dibagikan; target share membutuhkan explicit action/confirmation sesuai policy; cancel kembali ke hasil tanpa kehilangan file |
| `UAT-PHONE-011` | Issue Reporting | Tulis pesan dengan keyboard layar, screenshot aplikasi bila didukung, preview/markup/remove, dan explicit Send; form tidak meminta data teknis yang sudah dapat ditambahkan aman |
| `UAT-PHONE-012` | Language switch dan text expansion | Indonesia/English, dynamic text, error/progress panjang, dan keyboard layar tidak menutup input, confirmation, cancel, atau privacy notice |

UAT Phone tidak menuntut batch/file besar/multi-panel setara Desktop. UAT tetap FAIL bila keterbatasan itu tidak dijelaskan, tindakan yang dijanjikan tidak selesai, atau fallback memindahkan content ke server tanpa persetujuan.

---

## 6. Tablet UAT

| ID | Skenario pengalaman | Acceptance yang diharapkan |
|---|---|---|
| `UAT-TAB-001` | Portrait/landscape dan ukuran layar Tablet yang didukung | Layout memanfaatkan ruang lebih besar tanpa memaksa desktop UI; state/selection tetap konsisten saat rotation; tidak overflow atau menyembunyikan action |
| `UAT-TAB-002` | Touch dan stylus | Touch tetap lengkap; stylus memperbaiki precision untuk signature/annotation bila capability ada, tetapi tidak mengubah permission/business outcome |
| `UAT-TAB-003` | Camera scanner multi-page | Capture/retake/crop/perspective/reorder/preview natural pada layar besar; split/detail layout tidak memutus urutan halaman atau finalization |
| `UAT-TAB-004` | Visual Sign PDF | Drawing stylus/touch, clear/undo, preview, move, resize, rotation/orientation, dan final confirmation konsisten; aset sensitif tetap lokal kecuali user memilih penyimpanan yang diizinkan |
| `UAT-TAB-005` | Editing terkurasi yang lebih kaya daripada Phone | Fitur/parameter tambahan hanya muncul sesuai exposure/capability; selected page/tool context tidak hilang ketika pane berubah; tidak ada asumsi semua fitur Desktop tersedia |
| `UAT-TAB-006` | File picker, Share/Open With, dan external keyboard bila didukung | Adapter platform masuk ke validasi yang sama; keyboard focus/action tetap benar; cancel/app switch tidak membuat duplicate intent |
| `UAT-TAB-007` | Split view/multi-window atau resize bila OS/release mendukung | App menjaga working state dan progress; critical confirmation tidak muncul di pane yang tidak terlihat; unsupported lifecycle dinilai `N/A` dengan bukti scope |
| `UAT-TAB-008` | Offline, background/resume, dan Activity Center | Curated local work tetap tersedia sesuai grant; online resource tidak dieksekusi diam-diam; progress/recovery dapat ditemukan setelah resume |
| `UAT-TAB-009` | Secured Files lintas perangkat | File yang diamankan secara explicit dapat ditemukan setelah login; download/open membuat working copy sesuai aturan; versi dan konflik tidak menyebabkan silent overwrite |
| `UAT-TAB-010` | Accessibility/readability pada jarak dan mode input berbeda | Label, focus, scaling, contrast/readability, touch target, dan stylus target praktis; informasi penting tidak hanya mengandalkan warna |

Tablet bukan sekadar Phone yang diperbesar maupun Desktop yang diperkecil. Richness tambahan harus berasal dari release exposure, bukan diasumsikan oleh UAT.

---

## 7. Adaptive-capability scenarios

| Kondisi | Stimulus UAT | Perilaku yang diterima | Gagal bila |
|---|---|---|---|
| Exposure `FULL`/`PRIMARY`/`SUPPORTED` | Buka Feature dari surface yang dirilis | Action tersedia dengan UX yang sesuai platform dan outcome canonical | Feature hilang, contract berbeda, atau UI mengklaim unsupported tanpa dasar |
| Exposure `LIMITED` | Jalankan parameter/use case dalam dan di luar subset | Batas yang tersedia dijelaskan sebelum kerja hilang; subset tetap menghasilkan outcome valid | Parameter diam-diam diabaikan atau hasil disajikan setara versi penuh |
| Exposure `NOT_EXPOSED` | Cari melalui menu, search, deep link, Assistant | Tidak ada leak/bypass; bila user mencapai intent dari konteks sah, UI memberi penjelasan/alternatif/jalan kembali | Control mati, blank page, atau Assistant menjanjikan Feature |
| Capability device hilang | Camera/stylus/scanner/file-provider/notification tidak ada atau permission OS ditolak | UI menawarkan retry settings/picker/import/alternatif yang disetujui atau kembali aman | Crash, loop permission, atau transfer server diam-diam |
| Permission ditolak/dicabut | Menu lama, deep link, stale tab, Assistant, reconnect | Projection diperbarui; server denial konsisten; working data aman; tidak membocorkan resource | Cached visibility dianggap otorisasi atau denial tanpa recovery/context yang aman |
| Policy/feature flag berubah | Tindakan sudah dibuka lalu policy berubah sebelum final action | Final action memeriksa ulang; UI menjelaskan state baru dan mempertahankan draft bila aman | Mutasi tetap berjalan memakai keputusan lama |
| Offline | Local-eligible dan server-only action | Local-eligible dapat berjalan; server-only jelas `ONLINE_REQUIRED`/waiting; intent aman dapat dipertahankan sesuai contract | Auto-upload, false success, atau seluruh aplikasi mati padahal fungsi lokal tersedia |
| Koneksi kembali | Pending intent/job dan permission lama | Refresh identity/account/permission/policy/capability/compatibility lebih dahulu; dispatch explicit dan idempotent | Duplicate upload/job atau intent user lama dieksekusi dengan identity baru |
| Resource/storage rendah | Proses lokal/server dipilih | Preflight/peringatan/wait/reject aman dan actionable; source tetap aman | Freeze, silent wait, output parsial dianggap final |
| Server/provider sibuk | Queue/capacity/backpressure | `WAITING_RESOURCE`, retry guidance, atau safe rejection terlihat; `Retry-After` diterjemahkan bila tersedia | Spinner tanpa status, retry storm, atau fallback provider tidak disetujui |
| Version incompatible | `UPDATE_AVAILABLE`, `UPDATE_REQUIRED`, `UNSUPPORTED` | Non-blocking notice atau safe block sesuai status; local data tidak rusak; next action jelas | Forced update tanpa alasan contract/security atau crash/data loss |
| Entry-flow parity | Intent sama melalui File/Tool/Assistant | Feature, permission, parameter meaning, confirmation, result, dan evidence setara | Assistant atau surface tertentu memperoleh kemampuan lebih tinggi |
| Layout/input berubah | Resize, rotate, zoom, keyboard muncul, switch mouse/touch/stylus | State, focus, selection, progress, dan action kritis tetap dapat ditemukan | Context hilang, dialog terpotong, action tersembunyi |

### 7.1 Aturan `N/A` versus `FAIL`

`N/A` memerlukan bukti release scope atau platform exposure serta alasan dalam hasil UAT. Contoh: physical scanner Desktop yang masih PoC-gated dapat `BLOCKED`/`N/A` sesuai release disposition; scanner camera pada Phone yang dirilis tidak dapat dinyatakan `N/A` hanya karena perangkat uji salah. External stylus dapat `N/A` pada perangkat tanpa stylus, tetapi visual signature touch yang dirilis tetap harus diuji.

Perbedaan UX diterima bila pengguna memahami capability dan masih mempunyai jalur aman. Blank state tanpa penjelasan, tombol selalu disabled, error generik, navigasi berputar, atau “gunakan perangkat lain” tanpa menyimpan/menjelaskan langkah adalah dead-end dan `FAIL`.

---

## 8. Error/progress communication

### 8.1 Tingkat feedback

| Situasi | Feedback UAT yang diharapkan |
|---|---|
| Sangat cepat | Selesai langsung tanpa dialog/progress yang mengganggu; hasil/status akhir tetap jelas |
| Mulai terasa | Indikator ringan dengan konteks tindakan |
| Total belum diketahui | Indeterminate activity; tidak menampilkan persentase/ETA palsu |
| Proses panjang | Persentase bila benar, current step, completed/total item, ETA hanya bila cukup akurat, cancel bila aman |
| Background | Masuk Activity Center dan notification sesuai capability/permission OS; meninggalkan layar tidak menghilangkan pekerjaan |
| Menunggu koneksi | `WAITING_CONNECTION` dengan alasan dan langkah online/retry/cancel yang benar |
| Menunggu sumber daya | `WAITING_RESOURCE` dengan status tetap terlihat; tidak menyamakan dengan network failure |
| Retry/resume | `RETRYING`, attempt/recovery state yang mudah dipahami, checkpoint availability tanpa mengekspos token/path internal |
| Terminal | `COMPLETED`, `FAILED`, atau `CANCELLED` stabil dan tidak mundur karena event terlambat |

### 8.2 Kontrak komunikasi yang harus diuji

- HTTP `202 Accepted` berarti background work sudah diterima secara durable, bukan file selesai.
- SignalR hanya mempercepat feedback. Disconnect menandai realtime degraded; REST/persisted state memulihkan Activity Center.
- Success hanya boleh muncul setelah result divalidasi. Output parsial/staged tidak boleh tampil sebagai final.
- Cancel pada proses berjalan dapat menunggu safe point; UI membedakan “permintaan cancel diterima” dari “sudah cancelled”.
- Retry dengan intent yang sama tidak boleh membuat logical job/resource ganda. User tidak dibebani Idempotency-Key, tetapi efek duplikat harus dapat diamati tidak terjadi.
- Resume hanya ditawarkan bila checkpoint compatible dan valid. Jika tidak, UI menjelaskan retry dari safe point tanpa kehilangan edit.
- Mixed batch/partial failure menampilkan item berhasil dan gagal secara jelas, tanpa menyebut seluruh batch sukses atau membuang item berhasil.
- Result belum siap, expired, tidak berwenang, dan hilang karena insiden integritas tidak boleh memakai pesan yang sama.
- Error menampilkan bahasa user, langkah perbaikan, dan Correlation ID aman pada detail/support path bila tersedia; stack trace, path, provider internals, atau content tidak ditampilkan.

### 8.3 Smart communication

| Jenis | Acceptance |
|---|---|
| Snackbar | Untuk informasi ringan; tidak menjadi satu-satunya tempat bagi error/confirmation/action kritis; durasi tidak membuat pengguna kehilangan satu-satunya recovery |
| Undo/Redo | Tindakan working-state rutin dapat dipulihkan tanpa dialog berlebihan; feedback menyebut perubahan yang terjadi |
| Warning | Menjelaskan risiko/batasan dan pilihan; tidak menyamar sebagai success |
| Error | Menyebut apa yang gagal, apakah source/result aman, dan apa yang dapat dilakukan berikutnya |
| Confirmation | Hanya pada upload/content transfer, overwrite, permanent delete, share, final export/download, tindakan sensitif/irreversible, atau policy-driven action |
| Change summary | Sebelum final action, menampilkan file/target, langkah/perubahan penting, output, kebutuhan server, dan dampak yang relevan |
| Success | Menunjukkan hasil, lokasi/action berikutnya, serta membedakan saved local, downloaded, shared, atau secured |

Scenario wajib mencakup upload ditolak limit, server busy, SignalR putus, storage penuh, Worker crash, permission dicabut, result expired, attachment Issue gagal, dan finalization gagal. Semua harus mempertahankan source/draft yang aman serta menyediakan jalan keluar.

---

## 9. Accessibility/basic usability

UAT ini memvalidasi usability/accessibility dasar dan **tidak mengklaim sertifikasi** atau conformance level yang belum ditetapkan.

| Area | Pemeriksaan manual/real-platform | Kriteria penerimaan |
|---|---|---|
| Label | Baca control tanpa konteks developer | Label menggunakan istilah user; placeholder bukan satu-satunya label; ikon pada tindakan penting disertai teks/nama yang dapat dipahami |
| Keyboard Desktop/Web | Selesaikan navigation, form, picker trigger, editor action, dialog, Activity, download | Urutan focus logis, indicator terlihat, tidak ada keyboard trap, focus kembali ke pemicu/next logical item setelah dialog |
| Focus saat perubahan state | Validation error, panel terbuka, snackbar, progress selesai, reconnect | Focus tidak meloncat tanpa alasan; error/status dapat ditemukan; background update tidak mencuri focus |
| Readability/contrast | State normal, selected, disabled, warning, error, progress | Teks dan state terbaca praktis pada theme/build yang dirilis; informasi tidak hanya melalui warna |
| Text scaling/zoom/reflow | Scaling/zoom yang didukung, locale panjang, OS keyboard | Tidak clipping/overlap/overflow; confirmation, cancel, privacy notice, dan error tetap dapat dijangkau |
| Touch target | Phone/Tablet serta touch device yang dirilis | Target praktis dan terpisah; destructive action tidak mudah terpicu karena target bertumpuk; stylus bukan syarat untuk touch action |
| Orientation/safe area | Phone/Tablet portrait/landscape | Konten dan action penting tidak tertutup notch, system bar, keyboard, atau pane; state tidak hilang |
| Screen reader/semantic exposure | Sesuai kemampuan platform dan release matrix | Nama, role, state, validation, progress, selected/disabled, dan dialog title tersedia secara bermakna; urutan baca mendukung task |
| Error comprehension | Minta pengguna menjelaskan error dan next action dengan kata sendiri | Makna, dampak terhadap source/result, dan recovery dipahami tanpa membuka log teknis |
| Critical action visibility | Finalize, cancel, overwrite, share, secure, permanent delete, report issue | Tidak tersembunyi hanya di hover/gesture/menu yang tidak dapat ditemukan; consequence dan target jelas |
| Data presentation | History, Activity, Secured, Audit/Support list sesuai permission | Mode/list/detail tidak menghilangkan status/ownership/action penting; loading/pagination tidak terasa seperti data hilang |

Accessibility claim untuk camera, stylus, file picker, share intent, app lifecycle, dan assistive technology memerlukan evidence perangkat nyata. Emulator/snapshot dapat membantu regression, tetapi tidak cukup untuk sign-off pengalaman tersebut.

---

## 10. Localization

### 10.1 Scope Bahasa Indonesia dan English

UAT kedua bahasa mencakup Home, navigation, Feature Search, 33 Core labels yang dirilis, form/validation, preview/editor action, progress/waiting, success/warning/error, confirmation/change summary, History, Activity Center, Secured Files, Settings, Help, Issue Reporting, Assistant plan/recovery, serta halaman Admin/Audit/Support sesuai permission.

Fallback yang diharapkan adalah Bahasa Indonesia. Missing resource key, raw key, raw enum, atau campuran bahasa yang membuat instruksi tidak dipahami adalah defect.

### 10.2 Identifier yang tidak boleh diterjemahkan

```text
Feature ID
Job ID
Correlation ID
Process/Resource/Issue ID
Error Code dan Reason Code
permission/policy code pada detail teknis berwenang
API/contract/schema/event version
state/event code dalam persistence/wire contract
filename dan content buatan user
```

UI boleh menampilkan label lokal untuk state/error, tetapi pemetaan ke code canonical harus tetap satu-ke-satu. Perubahan bahasa tidak boleh mengubah request, permission, parameter meaning, sort/filter semantics, atau hasil.

### 10.3 Skenario localization

1. Perangkat menggunakan bahasa yang didukung saat first launch; bahasa aplikasi mengikuti aturan dan dapat diubah manual.
2. Perangkat menggunakan bahasa yang tidak didukung; aplikasi fallback ke Bahasa Indonesia tanpa menampilkan key mentah.
3. Ubah bahasa saat draft/Activity aktif; state dan file tidak hilang, label baru konsisten, dan action tidak dieksekusi ulang.
4. Cari tool dengan istilah resmi dan natural dalam kedua bahasa; hasil menuju Feature canonical yang sama.
5. Jalankan Assistant intent ekuivalen dalam kedua bahasa; plan, confirmation, parameter, dan result grounding setara.
6. Tampilkan teks panjang pada Phone, Tablet, Web sempit, Desktop scaling tinggi, dialog, snackbar, tabel, dan button; tidak overflow/truncation yang menghilangkan arti.
7. Verifikasi tanggal/waktu/timezone, angka, ukuran file, durasi, page count, dan persentase mengikuti presentation locale tanpa mengubah nilai contract.
8. Verifikasi error/progress yang berasal dari stable `messageCode` dilokalkan client; tidak ada raw provider/engine text.
9. Verifikasi accessibility name/help/error juga dilokalkan.
10. Verifikasi nama file, nama preset user, isi dokumen, dan identifier tidak diterjemahkan atau dinormalisasi secara merusak.

Glossary bisnis Indonesia/English, owner copy, dan aturan istilah resmi Pertamina perlu dibekukan sebelum final UAT agar perbedaan terjemahan tidak ditangani secara ad hoc.

---

## 11. Cross-platform acceptance matrix

Legenda:

- `W` — wajib untuk release/platform baseline tersebut.
- `C` — diuji bila capability dan release exposure mengizinkan.
- `O` — memerlukan koneksi dan otorisasi online.
- `N` — bukan baseline platform; `N/A` hanya dengan bukti scope.
- Kombinasi seperti `C/O` berarti capability-dependent dan online saat dieksekusi.

| Capability/experience | Windows Desktop | Responsive Web | Phone | Tablet | Acceptance lintas platform |
|---|---:|---:|---:|---:|---|
| Responsive/adaptive layout | `W` | `W` | `W` | `W` | No overflow; pola boleh berbeda |
| Home + Feature Search | `W` | `W` | `W` | `W` | Hanya Feature visible/authorized/capable |
| File-First | `W` | `W` untuk browser-safe flow | `W` untuk curated flow | `W` untuk curated flow | Input adapter berbeda, validation sama |
| Tool-First | `W` | `C/O` | `C` curated | `C` curated lebih kaya | Canonical card/Feature tetap sama |
| Assistant-First | `W/C` | `C/O`; tanpa authority offline | `C` curated | `C` curated | Tidak lebih berwenang daripada user/tool |
| File picker | `W` | `W` untuk upload/tool yang dirilis | `W` | `W` | Actual type/size/security validation sama |
| Drag-and-drop | `W` | `C` browser | `N/C` platform | `N/C` platform | Tidak dipaksakan pada touch-only |
| Paste file/screenshot | `W` | `C` browser | `C` platform | `C` platform | Explicit gesture; preview; no auto-upload |
| Camera Scan to PDF | `N/C` physical scanner PoC/import | `C` hanya bila exposure resmi | `W` mobile-first | `W` mobile-first | Capture sampai preview natural pada device |
| Physical scanner | `C` dan PoC-gated | `N` baseline | `N` | `N` | Tidak dianggap tersedia tanpa approved adapter |
| Share/Open With input | `C` sesuai release | `C` browser/OS | `C` sesuai release | `C` sesuai release | App lifecycle dan duplicate intent diuji |
| Large file/batch/multi-panel | `W` | `C/O` browser-safe | `N/C` limited | `C` limited | Desktop menjadi target paling lengkap |
| Eligible local processing | `W/C` | `N` sebagai offline authority | `C` curated | `C` curated | Local-only menghasilkan zero processing upload/API |
| Local Worker isolated | `W` untuk proses berat yang memakai worker | `N` | `N/C` platform background boundary | `N/C` | Crash tidak merusak UI/source |
| Server-required processing | `O` | `O` | `O` | `O` | Notice/consent, `202`, Activity, result |
| Activity Center | `W` | `W/O` | `W` | `W` | State lokal/server direkonsiliasi tanpa false status |
| Local workspace/recovery | `W` | `N` offline business authority | `C` | `C` | Draft/checkpoint tidak auto-sync |
| Save As/final export | `W` | `W` download untuk flow yang dirilis | `C` platform save/share | `C` | Source tidak auto-overwrite; summary lebih dulu |
| Undo/Redo editing | `W` | `C` browser-safe editor | `C` curated | `C` curated | Working-state only, bukan file versioning |
| Visual Sign non-PKI | `W` | `C/O` bila exposed | `W` curated | `W` curated | Input method berbeda; preview/final rule sama |
| Personal History | `W` | `W/O` | `C/O` plus local where allowed | `C/O` | Own scope; delete bukan delete Audit |
| Secured Files/max-three | `O` | `O` | `O` | `O` | Explicit Amankan; ownership/version/expiry |
| Settings/preferences | `W` | `O` untuk account settings | `W/C` | `W/C` | Allowlisted preference, bukan workspace sync |
| Admin/configuration | `O` jika exposed | `O` | `O` hanya jika explicitly exposed | `O` hanya jika explicitly exposed | Permission/scope, no user-content elevation |
| Audit query/export | `O` jika exposed | `O` | `O` hanya jika explicitly exposed | `O` hanya jika explicitly exposed | Auditor scope dan field projection |
| Support Console | `O` jika exposed | `O` | `O` hanya jika explicitly exposed | `O` hanya jika explicitly exposed | Issue scope, bukan document authority |
| Issue Reporting | `W`; submit `O` | `W/O` | `W`; submit `O` | `W`; submit `O` | Minimal form; screenshot optional/explicit |
| Keyboard navigation | `W` | `W` | `C` external keyboard | `C` external keyboard | Semua action kritis pada input mode yang dirilis |
| Touch/stylus | `C` hardware | `C` device | `W` touch; stylus `C` | `W` touch; stylus `C` | Stylus tidak menjadi satu-satunya jalur |
| Orientation | `N/C` window adaptation | `C` viewport/device | `W` | `W` | State dan critical action tetap tersedia |
| Localization Indonesia/English | `W` | `W` | `W` | `W` | Identifier dan meaning tidak berubah |
| Accessibility/basic usability | `W` | `W` | `W` | `W` | Evidence sesuai kemampuan platform, tanpa klaim sertifikasi |

Matriks ini merupakan baseline UAT, bukan pengganti `DesktopExposure`, `TabletExposure`, `PhoneExposure`, dan release Web exposure per Feature. Release package harus menghasilkan matriks final yang lebih spesifik untuk Feature yang benar-benar masuk scope.

---

## 12. Risks/gaps

| Risiko/gap | Dampak UAT | Rekomendasi/disposition |
|---|---|---|
| Exact Feature × Desktop/Web/Phone/Tablet exposure per release belum dibekukan dalam paket UAT | `N/A`, FAIL, dan missing capability dapat salah klasifikasi | **REQUIRES_PERTAMINA_STANDARD_CONFIRMATION:** freeze capability/exposure manifest per RC, termasuk limitation dan fallback |
| Exact device, OS, browser, version, screen class, orientation, DPI/text scale matrix belum disetujui | Klaim lintas platform tidak representatif | **REQUIRES_PERTAMINA_STANDARD_CONFIRMATION:** tetapkan supported matrix dan minimum real-device evidence |
| Framework native tetap tunduk approval arsitektur/security/distribusi | Scenario Phone/Tablet/Desktop tertentu belum dapat dieksekusi | Catat `BLOCKED` per platform, jangan PASS/N/A tanpa release disposition |
| Physical scanner integration masih PoC-gated | Desktop Scan dapat berbeda antara import hasil scan dan direct scanner | Pisahkan acceptance import/OCR dari scanner-driver PoC; jangan menjanjikan direct scanner sebelum gate lulus |
| Daftar browser-safe tools dan Web Assistant exposure belum rinci per Feature | Web dapat dinilai seolah full Desktop | Freeze Web release surface dari registry/capability projection sebelum UAT |
| Share/Open With adalah capability/release-dependent | Input dari aplikasi lain tidak seragam per OS/release | Uji hanya target release yang menyertakan; dokumentasikan fallback picker dan app lifecycle |
| Accessibility standard/certification, assistive technology, dan supported combination belum menjadi requirement resmi | “Accessible” dapat ditafsirkan terlalu luas atau terlalu sempit | **REQUIRES_PERTAMINA_STANDARD_CONFIRMATION:** tetapkan target standar dan test matrix; sementara gunakan basic usability checks tanpa klaim sertifikasi |
| Target peserta, persona bisnis, task-success threshold, time-on-task, dan sign-off authority belum ditetapkan | Hasil usability tidak mempunyai decision rule resmi | **REQUIRES_PERTAMINA_STANDARD_CONFIRMATION:** bisnis/UAT owner menetapkan participant profile dan approval rule tanpa mengarang angka di laporan ini |
| Bahasa/glossary/copy owner Pertamina belum dibekukan | Defect localization menjadi subjektif | Tetapkan glossary Indonesia/English, protected terms, dan copy approval sebelum final cycle |
| Design baseline/mock serta supported UI modes belum terikat ke RC | Cosmetic variance dapat disalahartikan sebagai functional failure atau sebaliknya | Pakai release UI specification sebagai oracle; fokus laporan ini pada outcome, clarity, no-overflow, dan safety |
| UAT lane/accounts/permissions/data belum tersedia | Admin/Auditor/Support/ownership denial tidak dapat divalidasi | Gunakan entry package Document 12; tidak boleh membuat bypass atau memakai Production data |
| Numeric UX/performance thresholds belum disetujui | “Responsif/cepat” rentan menjadi opini | Laporkan measured observation dan tautkan ke approved performance targets Document 12; jangan membuat SLA UAT |
| Camera, stylus, file-provider, notification, share, credential store, lifecycle sering tidak akurat di emulator | False PASS pada pengalaman perangkat | Real-device evidence wajib untuk claim tersebut; emulator hanya pendukung |
| Hidden versus disabled behavior dapat berbenturan dengan security concealment | Penjelasan terlalu rinci dapat membocorkan Feature/resource | Gunakan safe capability projection Document 09: explain state yang boleh terlihat, conceal resource yang tidak boleh diketahui |
| System notification dapat ditolak OS/user | Background work terlihat hilang bila hanya mengandalkan notification | Activity Center tetap jalur in-app; uji denial notification dan recovery saat app dibuka |
| Result/secured/Issue retention wording belum memiliki copy final | User tidak memahami expiry atau konsekuensi delete/version keempat | Validasi copy terhadap policy/contract final; jangan menampilkan angka retensi yang belum disetujui |
| Screenshot UAT/Issue dapat menangkap data sensitif | Evidence menjadi kanal kebocoran | Gunakan synthetic/sanitized data, preview/crop/remove, explicit capture, restricted evidence handling |

Gap di atas tidak membolehkan penghapusan Feature, pengurangan keamanan, pemindahan local-only processing ke server, public AI fallback, atau PASS palsu. Gap yang memblokir execution diberi status `BLOCKED` dan dibawa ke decision owner yang tepat.

---

## 13. Suggested Document 13 text

### 13.x Prinsip UX dan platform UAT

PERTAMINA OTO menggunakan outcome-based, capability-aware UAT. Adaptive UX tidak berarti UI identik. Windows Desktop adalah primary full-feature productivity client; Phone dan Tablet adalah companion terkurasi; Responsive Web adalah kanal enterprise/browser. UAT membuktikan business outcome, canonical Feature, permission, source safety, finality, feedback, dan recovery yang sama, sedangkan layout dan input pattern mengikuti platform.

Perbedaan platform tidak menjadi defect jika sesuai release exposure, dijelaskan kepada user, tidak menciptakan dead-end, dan mempunyai fallback/jalan kembali yang benar. `N/A` hanya digunakan dengan bukti release scope. Capability yang dijanjikan tetapi tidak selesai adalah `FAIL`; PoC/approval/environment yang belum tersedia adalah `BLOCKED`.

### 13.x Entry dan traceability

UAT hanya dimulai setelah UAT entry package Document 12 lengkap: immutable RC, lane terkontrol, smoke dan gate yang berlaku lulus, no open Blocker/Critical, known issue terdokumentasi, account/permission/data/platform siap, serta defect/retest path tersedia. Setiap case mencatat Requirement ID, canonical Feature ID/resource, persona, platform/capability, connectivity, locale, dataset, expected outcome, forbidden side effect, result, evidence aman, dan defect.

UAT memakai data synthetic/sanitized yang disetujui. Password, token, secret, dokumen Production, OCR lengkap, clipboard, aset tanda tangan, dan screenshot tidak disalin ke evidence tanpa kebutuhan dan kontrol resmi. UAT tidak menjalankan load, malicious corpus, penetration, debug, atau direct DB test.

### 13.x Navigation acceptance

Home, Feature Search, File-First, Tool-First, Assistant, History, Activity Center, Secured Files, Settings, Help, dan Issue Reporting diuji pada platform tempat surface tersebut dirilis. Tool utama harus dapat ditemukan maksimal dua interaksi. File-First, Tool-First, dan Assistant-First untuk Feature yang sama wajib menghasilkan Feature canonical, normalized input/parameter, authorization, confirmation, result, dan evidence yang setara.

Assistant adalah jalur alternatif. Assistant menampilkan plan multi-step, file/output, kebutuhan server, dan aksi berisiko; meminta klarifikasi saat ambigu; tidak membuat Feature/parameter; dan tidak mengklaim success tanpa hasil tool aktual. Deep link, refresh, resume, recent item, dan Activity link harus berakhir pada action yang berwenang atau explanation/jalan kembali yang aman.

### 13.x Platform acceptance

Desktop UAT mencakup drag/drop, picker dan multi-file, keyboard/mouse, explicit paste screenshot, large workspace/multi-panel, Local Worker, progress/Activity, crash recovery, Undo/Redo, Save As/overwrite protection, serta metode visual signature Desktop.

Web UAT mencakup responsive/zoom/reflow, keyboard/focus/browser navigation, browser-safe exposure, upload consent/progress, durable `202 Accepted`, SignalR degradation + REST recovery, result download/expiry, account/history/secured files, serta Admin/Audit/Support sesuai permission. Cached Web page tidak menjadi offline business authority.

Phone UAT mencakup touch, portrait/landscape, safe area/keyboard, camera scanner capture-to-preview, picker/share input, visual signature touch, curated editing, background/resume/network switch, Activity, share result, dan capability limitation. Tablet menjalankan baseline mobile yang sama dengan UX lebih kaya hanya bila exposure mengizinkan, termasuk stylus, larger layout, dan lifecycle/multi-window yang didukung.

Camera, stylus, share, picker, credential store, lifecycle, dan assistive technology claim memerlukan perangkat nyata. UAT tidak menuntut Phone/Tablet/Web setara Desktop dan tidak menerima fallback server tanpa notice, policy, permission, connectivity, dan consent.

### 13.x Progress, error, dan recovery acceptance

Sistem menerapkan No Silent Wait. Proses sangat cepat selesai langsung; proses mulai terasa menggunakan indikator ringan; proses panjang menampilkan progress detail yang benar; total yang tidak diketahui memakai indeterminate state; background work masuk Activity Center. State `WAITING_CONNECTION`, `WAITING_RESOURCE`, `RETRYING`, `COMPLETED`, `FAILED`, dan `CANCELLED` dibedakan dan mempunyai next action yang jelas.

`202 Accepted` berarti pekerjaan background diterima secara durable, bukan selesai. SignalR bukan source of truth; disconnect tidak menggagalkan Job dan REST memulihkan state. Success hanya muncul setelah output divalidasi. Cancel, retry, resume, checkpoint mismatch, partial failure, storage/capacity error, permission change, server busy, Worker crash, result expired, dan Issue attachment failure diuji tanpa source corruption, duplicate logical effect, atau false final output.

Tindakan rutin memakai snackbar/Undo tanpa dialog berlebihan. Confirmation digunakan untuk upload/content transfer, overwrite, permanent delete, share, final export/download, dan tindakan sensitif/irreversible. Sebelum final action, UI menampilkan ringkasan perubahan, target/output, kebutuhan server, dan dampak yang relevan.

### 13.x Accessibility dan basic usability

UAT minimum memeriksa readable labels, keyboard navigation Desktop/Web, focus order/indicator/restore, understandable errors, practical contrast/readability, informasi yang tidak hanya mengandalkan warna, text scaling/zoom/reflow, touch target, orientation/safe area, critical action visibility, dan no overflow. Screen reader/semantic name-role-state diuji sesuai kemampuan platform dan release matrix. Hasil ini tidak disebut sertifikasi accessibility tanpa requirement dan approval resmi.

### 13.x Localization

Bahasa Indonesia dan English diuji untuk navigation, Feature Search, tools, form/validation, progress, notification, error, confirmation, History, Activity, Secured Files, Settings, Help, Issue, Assistant, dan privileged surface yang dirilis. Fallback adalah Bahasa Indonesia dan teks menggunakan key-based resources.

Feature ID, Job ID, Correlation ID, Process/Resource/Issue ID, Error/Reason Code, permission/policy code, contract/schema/event version, wire state, filename, dan user content tidak diterjemahkan. Label lokal boleh berbeda, tetapi mapping code, parameter meaning, permission, sort/filter, dan result tidak berubah. Text expansion, date/time/number/file-size presentation, language switch, accessibility text, dan natural search/Assistant intent diuji tanpa overflow atau kehilangan state.

### 13.x Result dan sign-off

Status UAT adalah `PASS`, `FAIL`, `BLOCKED`, `NOT_RUN`, `INCONCLUSIVE`, atau `N/A` sesuai definisi Document 12. Diagnostic rerun tidak menghapus failure awal. Sign-off memerlukan seluruh critical applicable scenario PASS, no open Blocker/Critical, platform limitation dan known issue terdokumentasi, Major memiliki disposition resmi, serta evidence/traceability lengkap. Exact platform/device/browser matrix, participant profile, success threshold, accessibility target, glossary, dan sign-off authority berstatus `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` sebelum final UAT cycle.

