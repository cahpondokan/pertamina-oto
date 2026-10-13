# AGENT B — 33 CORE PDF TOOLS MANUAL REPORT

**Task ID:** `DOC14-AGENT-B`  
**Peran:** Senior System Analyst; PDF Workflow Specialist; Senior Technical Writer; UX Instruction Designer; Senior Document-Processing Architect  
**Status:** Draft specialist untuk integrasi Document 14; bukan klaim bahwa tool telah dirilis  
**Sumber:** Document 01–13 FINAL

> **Kesenjangan keputusan material:** Document 13 FINAL telah direkonsiliasi. Dokumen tersebut menegaskan bahwa `FULL/PRIMARY/SUPPORTED`, `LIMITED`, dan `NOT_EXPOSED` harus dibuktikan oleh release-capability evidence, tetapi tidak menetapkan exposure final per 33 tool. Document 03/12 juga hanya memberi posture umum serta kandidat (`W*`, `P/T*`) dan menyatakan exposure aktual berasal dari registry dan release `CapabilityVersion`. Karena itu laporan ini tidak mengarang status platform final; sel yang belum diputuskan ditandai `UNRESOLVED` sampai manifest resmi tersedia.

Panduan ditulis dari sudut pandang pengguna. Nama komponen teknis, library, dan detail kontrak layanan sengaja tidak digunakan. Semua tool tetap mengikuti aturan global: file sumber tidak ditimpa otomatis, isi file tidak dikirim tanpa pemberitahuan, hasil sementara bukan hasil final, proses panjang harus terlihat, dan **Amankan ke Akun** hanya terjadi setelah tindakan eksplisit pengguna.

---

## 1. Tool-page template

Setiap halaman tool menggunakan urutan tetap berikut.

| Bagian | Isi yang wajib |
|---|---|
| **Nama** | Nama card resmi dalam Feature Catalog |
| **Tujuan** | Outcome yang ingin dicapai user, bukan cara teknis |
| **Feature ID** | Canonical Feature ID; workflow card dapat menyebut rentang/multiple ID resmi |
| **Tersedia pada** | Desktop/Web/Tablet/Phone berdasarkan release capability manifest; status gated/limited harus terlihat |
| **Memerlukan koneksi** | Tidak, Ya, atau Kondisional; jika server diperlukan, user diberi tahu sebelum transfer |
| **Format input** | Format yang benar-benar diterima build; jangan menyimpulkan hanya dari ekstensi |
| **Format hasil** | Format final/preview yang benar-benar dihasilkan build |
| **Sebelum memulai** | Kepemilikan file, password sah, storage, kualitas scan, pilihan halaman/bahasa, atau precondition lain |
| **Langkah** | Alur ringkas dari pilih file sampai Save As/open result |
| **Opsi utama** | Hanya opsi yang tersedia pada build; gunakan “bila tersedia” untuk opsi capability-dependent |
| **Hasil** | Apa yang berubah, apa yang tetap aman, dan cara memeriksa hasil |
| **Peringatan** | Risiko kualitas, finality, password, transfer, redaction, atau fidelity |
| **Masalah umum** | Gejala user-facing dan tindakan pemulihan yang aman |
| **Batas kemampuan** | Struktur/format yang sulit, fitur gated, perbedaan platform, dan hal yang tidak dijanjikan |

### 1.1 Pola umum penggunaan

```text
Buka tool
→ pilih/masukkan file
→ periksa status validasi
→ atur pilihan yang tersedia
→ preview/ringkasan
→ mulai
→ pantau progress
→ periksa hasil
→ Save As / download / share
→ Amankan ke Akun hanya bila diperlukan
```

### 1.2 Aturan editorial

- Gunakan kata “mencoba memperbaiki”, bukan “pasti memperbaiki”.
- Gunakan “dapat berubah”, bukan menjanjikan fidelity identik.
- Bedakan PDF berbasis teks dari PDF scan.
- Bedakan **Add Text** dari **Edit Existing Text**.
- Bedakan visual signature/paraf non-PKI dari certificate/PKI signature.
- Jelaskan bahwa final redaction dapat permanen dan wajib dipreview.
- Jangan tampilkan pilihan cara pemrosesan teknis. Aplikasi menentukan jalur dan memberi notice bila koneksi/transfer diperlukan.
- Jika tool tidak tersedia pada platform/build, sembunyikan atau jelaskan keterbatasan/fallback; jangan tampilkan tombol buntu.

---

## 2. Core Tool availability matrix

### 2.1 Legenda dan batas kewenangan

Status yang diminta manual:

- Desktop: `FULL` atau `SUPPORTED`.
- Web: `SUPPORTED`, `LIMITED`, atau `NOT EXPOSED`.
- Tablet: `SUPPORTED` atau `LIMITED`.
- Phone: `SUPPORTED`, `LIMITED`, atau `NOT EXPOSED`.

Tambahan aman dalam draft ini:

- `UNRESOLVED` berarti sumber final belum cukup untuk menetapkan salah satu status di atas.
- `*` berarti hanya jika registry/release exposure mengizinkan.
- `GATED` berarti jangan dipublikasikan sebagai tersedia sebelum PoC/feasibility/security/fidelity gate lulus.

Document 12 menyatakan `W*` hanya bila resmi browser-safe/server-capable dan `P/T*` hanya bila registry expose. Tabel Document 12 **tidak mengotorisasi exposure baru**. Document 13 FINAL juga mensyaratkan release-capability evidence untuk status availability. Oleh sebab itu, `UNRESOLVED` harus diganti dari manifest registry + `CapabilityVersion` sebelum publication.

### 2.2 Matriks aman sementara

| No | Tool | Desktop | Web | Tablet | Phone | Dasar/limit utama |
|---:|---|---|---|---|---|---|
| 1 | Merge PDF | `FULL` | `UNRESOLVED` | `LIMITED*` | `LIMITED*` | Local; mobile hanya simple bila exposed |
| 2 | Split PDF | `FULL` | `UNRESOLVED` | `UNRESOLVED (P/T*)` | `UNRESOLVED (P/T*)` | Page/range flow; P/T hanya jika exposed |
| 3 | Remove pages | `FULL` | `UNRESOLVED` | `UNRESOLVED (P/T*)` | `UNRESOLVED (P/T*)` | Curated page editing candidate |
| 4 | Extract pages | `FULL` | `UNRESOLVED` | `UNRESOLVED (P/T*)` | `UNRESOLVED (P/T*)` | Curated page selection candidate |
| 5 | Organize PDF | `FULL` | `UNRESOLVED` | `UNRESOLVED (P/T*)` | `UNRESOLVED (P/T*)` | Workflow bundle; subset exposure belum final |
| 6 | Scan to PDF | `SUPPORTED` | `UNRESOLVED` | `SUPPORTED` | `SUPPORTED` | Mobile-first; physical scanner Desktop `GATED` |
| 7 | Compress PDF | `FULL` | `UNRESOLVED` | `LIMITED*` | `LIMITED*` | Simple mobile compression bila exposed |
| 8 | Repair PDF | `FULL` | `UNRESOLVED (W*)` | `UNRESOLVED` | `UNRESOLVED` | Advanced repair capability-dependent |
| 9 | OCR PDF | `SUPPORTED, GATED` | `UNRESOLVED (W*)` | `LIMITED*` | `LIMITED*` | OCR ringan vs kompleks; fidelity/resource gate |
| 10 | JPG to PDF | `FULL` | `UNRESOLVED` | `UNRESOLVED (P/T*)` | `UNRESOLVED (P/T*)` | Image order/page dimensions |
| 11 | Word to PDF | `SUPPORTED, GATED` | `UNRESOLVED (W*)` | `UNRESOLVED` | `UNRESOLVED` | Office fidelity/font gate |
| 12 | PowerPoint to PDF | `SUPPORTED, GATED` | `UNRESOLVED (W*)` | `UNRESOLVED` | `UNRESOLVED` | Slide/layout fidelity gate |
| 13 | Excel to PDF | `SUPPORTED, GATED` | `UNRESOLVED (W*)` | `UNRESOLVED` | `UNRESOLVED` | Sheet/print-area gate |
| 14 | HTML to PDF | `SUPPORTED, GATED` | `UNRESOLVED (W*)` | `UNRESOLVED` | `UNRESOLVED` | Active content/remote resource gate |
| 15 | PDF to JPG | `FULL` | `UNRESOLVED` | `UNRESOLVED (P/T*)` | `UNRESOLVED (P/T*)` | Page selection/resolution |
| 16 | PDF to Word | `SUPPORTED, GATED` | `UNRESOLVED (W*)` | `UNRESOLVED` | `UNRESOLVED` | Editability/layout gate |
| 17 | PDF to PowerPoint | `SUPPORTED, GATED` | `UNRESOLVED (W*)` | `UNRESOLVED` | `UNRESOLVED` | Slide segmentation gate |
| 18 | PDF to Excel | `SUPPORTED, GATED` | `UNRESOLVED (W*)` | `UNRESOLVED` | `UNRESOLVED` | Table/cell fidelity gate |
| 19 | PDF to PDF/A | `SUPPORTED, GATED` | `UNRESOLVED (W*)` | `UNRESOLVED` | `UNRESOLVED` | Independent conformance validation |
| 20 | Rotate PDF | `FULL` | `UNRESOLVED` | `LIMITED*` | `LIMITED*` | Explicit curated mobile example |
| 21 | Add page numbers | `FULL` | `UNRESOLVED` | `UNRESOLVED (P/T*)` | `UNRESOLVED (P/T*)` | Range/start/position |
| 22 | Add watermark | `FULL` | `UNRESOLVED` | `UNRESOLVED (P/T*)` | `UNRESOLVED (P/T*)` | Text/image/opacity if exposed |
| 23 | Crop PDF | `FULL` | `UNRESOLVED` | `LIMITED*` | `LIMITED*` | Explicit curated mobile example |
| 24 | Edit PDF | `SUPPORTED, GATED subset` | `UNRESOLVED (W*)` | `LIMITED*` | `LIMITED*` | Existing-text editing remains gated |
| 25 | PDF Forms | `SUPPORTED` | `UNRESOLVED` | `UNRESOLVED (P/T*)` | `UNRESOLVED (P/T*)` | Fill/create exposure by build |
| 26 | Unlock PDF | `FULL` | `UNRESOLVED` | `UNRESOLVED (P/T*)` | `UNRESOLVED (P/T*)` | Valid password/authority only |
| 27 | Protect PDF | `FULL` | `UNRESOLVED` | `UNRESOLVED (P/T*)` | `UNRESOLVED (P/T*)` | Password is not recoverable by app |
| 28 | Sign PDF | `FULL` | `UNRESOLVED` | `SUPPORTED` | `SUPPORTED` | Visual non-PKI; mobile-first drawing |
| 29 | Redact PDF | `FULL` | `UNRESOLVED` | `UNRESOLVED (P/T*)` | `UNRESOLVED (P/T*)` | Permanent finalization; high risk |
| 30 | Compare PDF | `FULL` | `UNRESOLVED` | `UNRESOLVED` | `UNRESOLVED` | Document 12 minimum only Desktop |
| 31 | AI Summarizer | `SUPPORTED, GATED` | `UNRESOLVED (W*)` | `UNRESOLVED*` | `UNRESOLVED*` | Approved local/internal provider only |
| 32 | Translate PDF | `SUPPORTED, GATED` | `UNRESOLVED (W*)` | `UNRESOLVED*` | `UNRESOLVED*` | Approved provider/fidelity gate |
| 33 | PDF to Markdown | `SUPPORTED, GATED complex` | `UNRESOLVED (W*)` | `UNRESOLVED` | `UNRESOLVED` | Deterministic-first; complex layout gate |

**Blocking integration note:** publication of “Tersedia pada” in the final user manual is blocked until the release exposure manifest resolves every `UNRESOLVED` cell. The table above is a safe editorial draft, not a release promise.

---

## 3. Organize PDF guides

### 3.1 Merge PDF

- **Nama:** Merge PDF.
- **Tujuan:** Menggabungkan dua atau lebih PDF menjadi satu PDF dengan urutan yang dipilih.
- **Feature ID:** `FEAT-PDF-005`.
- **Tersedia pada:** Desktop `FULL`; Tablet/Phone `LIMITED*` untuk simple Merge; Web `UNRESOLVED`.
- **Memerlukan koneksi:** Tidak untuk pemrosesan lokal yang eligible. **Amankan ke Akun** selalu memerlukan koneksi.
- **Format input:** Dua atau lebih file PDF yang valid; PDF berpassword memerlukan password sah.
- **Format hasil:** Satu file PDF baru.
- **Sebelum memulai:** Pastikan urutan yang diinginkan, ruang penyimpanan cukup, dan password tersedia untuk file terlindungi.
- **Langkah:** Buka Merge PDF → tambahkan file → periksa status → ubah urutan/hapus input salah → pilih pengaturan hasil yang tersedia → mulai → pantau progress → preview bila tersedia → **Save As** → buka hasil.
- **Opsi utama:** Urutan file, remove/Undo, nama/lokasi hasil, dan pilihan hasil lain yang benar-benar tersedia pada build.
- **Hasil:** Satu PDF final yang memuat seluruh halaman sesuai urutan input akhir; semua file sumber tetap tidak berubah.
- **Peringatan:** Merge bersifat atomik; input wajib yang invalid tidak boleh menghasilkan “sebagian berhasil” sebagai final. Jangan memilih file sumber sebagai target Save As.
- **Masalah umum:** Kurang dari dua file; PDF rusak; password salah; storage tidak cukup; cancel; Save As conflict. Perbaiki input/ruang/target lalu retry secara eksplisit.
- **Batas kemampuan:** File sangat besar atau banyak dapat berjalan di background lokal. Batas jumlah/ukuran mengikuti policy build dan tidak boleh ditebak dari manual.

### 3.2 Split PDF

- **Nama:** Split PDF.
- **Tujuan:** Memisahkan satu PDF menjadi beberapa PDF.
- **Feature ID:** `FEAT-PDF-006 / FEAT-PDF-007`.
- **Tersedia pada:** Desktop `FULL`; Tablet/Phone `UNRESOLVED (P/T*)`; Web `UNRESOLVED`.
- **Memerlukan koneksi:** Tidak untuk path lokal yang eligible.
- **Format input:** Satu PDF valid.
- **Format hasil:** Beberapa file PDF sesuai pembagian.
- **Sebelum memulai:** Periksa jumlah halaman, password, lokasi output, dan aturan penamaan hasil.
- **Langkah:** Pilih PDF → periksa preview/page count → pilih cara pembagian atau page range yang tersedia → tinjau daftar hasil → mulai → pantau → buka beberapa hasil → Save As/export.
- **Opsi utama:** Page range dan mode split yang benar-benar didukung build; penamaan/lokasi hasil.
- **Hasil:** Manifest/daftar PDF terpisah dengan halaman sesuai pilihan; sumber tetap utuh.
- **Peringatan:** Range tumpang tindih atau halaman yang tidak tercakup harus terlihat sebelum proses; jangan menganggap semua mode split tersedia.
- **Masalah umum:** Range kosong/tidak valid, password diperlukan, output name conflict, atau storage tidak cukup. Koreksi range/target lalu coba lagi.
- **Batas kemampuan:** Bookmark, struktur kompleks, atau pembagian khusus hanya dipertahankan sejauh capability build mendukung.

### 3.3 Remove pages

- **Nama:** Remove pages.
- **Tujuan:** Menghapus halaman yang dipilih dari working state dan menghasilkan PDF baru.
- **Feature ID:** `FEAT-PDF-013`.
- **Tersedia pada:** Desktop `FULL`; Tablet/Phone `UNRESOLVED (P/T*)`; Web `UNRESOLVED`.
- **Memerlukan koneksi:** Tidak untuk path lokal yang eligible.
- **Format input:** Satu PDF valid.
- **Format hasil:** Satu PDF baru tanpa halaman yang dipilih.
- **Sebelum memulai:** Pastikan thumbnail/page number yang dipilih benar dan minimal halaman tersisa memenuhi aturan build.
- **Langkah:** Buka PDF → pilih halaman → Remove → periksa snackbar/Undo → preview urutan akhir → finalisasi → Save As.
- **Opsi utama:** Single/multiple page selection, range bila tersedia, Undo/Redo.
- **Hasil:** PDF baru tanpa halaman pilihan; file sumber dan halaman asal tidak dihapus.
- **Peringatan:** Remove sebelum finalisasi masih working-state action; final output harus dipreview dan disimpan sebagai hasil baru.
- **Masalah umum:** Semua halaman dipilih, selection hilang, atau page number tidak sesuai preview. Batalkan/Undo dan pilih ulang.
- **Batas kemampuan:** Isi halaman tidak dianalisis untuk menentukan apakah “aman dihapus”; keputusan tetap milik user.

### 3.4 Extract pages

- **Nama:** Extract pages.
- **Tujuan:** Mengambil halaman tertentu menjadi PDF baru.
- **Feature ID:** `FEAT-PDF-014`.
- **Tersedia pada:** Desktop `FULL`; Tablet/Phone `UNRESOLVED (P/T*)`; Web `UNRESOLVED`.
- **Memerlukan koneksi:** Tidak untuk path lokal yang eligible.
- **Format input:** Satu PDF valid.
- **Format hasil:** Satu atau beberapa PDF baru sesuai mode yang tersedia.
- **Sebelum memulai:** Tentukan halaman dan urutan yang ingin diambil; siapkan password jika perlu.
- **Langkah:** Pilih PDF → pilih halaman/range → tinjau urutan → pilih bentuk hasil yang tersedia → mulai → preview/open result → Save As.
- **Opsi utama:** Selection/range, urutan, satu/lebih hasil hanya jika build menawarkannya.
- **Hasil:** Hasil berisi halaman yang dipilih; halaman tetap ada pada sumber.
- **Peringatan:** Extract berbeda dari Remove: Extract membuat salinan halaman, bukan menghapusnya dari sumber.
- **Masalah umum:** Range invalid, page selection salah, atau hasil tidak dapat disimpan. Koreksi selection/target dan ulangi.
- **Batas kemampuan:** Form, attachment, bookmark, atau metadata halaman dapat berubah sesuai kemampuan build; periksa hasil.

### 3.5 Organize PDF

- **Nama:** Organize PDF.
- **Tujuan:** Mengelola susunan halaman dalam satu workspace sebelum membuat PDF final.
- **Feature ID:** `FEAT-PDF-010..016`.
- **Tersedia pada:** Desktop `FULL`; Tablet/Phone `UNRESOLVED (P/T*)`; Web `UNRESOLVED`.
- **Memerlukan koneksi:** Tidak untuk operasi lokal yang eligible.
- **Format input:** Satu atau beberapa PDF valid sesuai capability workspace.
- **Format hasil:** Satu PDF final atau hasil lain yang secara eksplisit ditawarkan workspace.
- **Sebelum memulai:** Periksa status input, jumlah halaman, ruang temp, dan urutan awal.
- **Langkah:** Tambahkan file → buka workspace → reorder/move/rotate/remove/duplicate/add blank sesuai build → gunakan Undo/Redo → preview → lihat change summary → finalisasi → Save As.
- **Opsi utama:** Reorder, Move Page, Rotate, Remove, Extract, Duplicate Page, Add Blank Page sesuai capability yang dirilis.
- **Hasil:** PDF baru yang merefleksikan working state final.
- **Peringatan:** Organize PDF adalah workspace yang menggunakan capability canonical; bukan aturan atau pemroses terpisah.
- **Masalah umum:** Selection halaman membingungkan, operasi tidak didukung untuk struktur tertentu, atau temp storage tidak cukup. Gunakan preview/Undo atau pisahkan pekerjaan.
- **Batas kemampuan:** Tidak semua operasi dalam bundle harus tersedia pada setiap platform/build; UI wajib menjelaskan subset yang tersedia.

### 3.6 Scan to PDF

- **Nama:** Scan to PDF.
- **Tujuan:** Mengubah halaman fisik atau gambar scan menjadi PDF.
- **Feature ID:** `FEAT-SCAN-014`.
- **Tersedia pada:** Tablet/Phone `SUPPORTED`; Desktop `SUPPORTED` untuk import scan, sedangkan integrasi scanner fisik `GATED`; Web `UNRESOLVED`.
- **Memerlukan koneksi:** Tidak untuk capture/import lokal; dapat kondisional jika pemeriksaan/proses yang dipilih memerlukan layanan internal.
- **Format input:** Camera capture, scanner/image, atau PDF scan yang didukung.
- **Format hasil:** PDF.
- **Sebelum memulai:** Bersihkan lensa, gunakan cahaya cukup, siapkan seluruh halaman, dan berikan izin camera/scanner hanya pada alur foreground.
- **Langkah:** Capture/import → periksa edge → retake bila perlu → crop/perspective correction → tambah/reorder halaman → preview → pilih orientation/page size bila tersedia → simpan/proses lebih lanjut.
- **Opsi utama:** Retake, crop, perspective correction, reorder, rotate, orientation/page size, dan OCR lanjutan bila ditawarkan.
- **Hasil:** PDF yang dapat dibuka dengan urutan halaman sesuai preview.
- **Peringatan:** Hasil buram, miring, bayangan, atau halaman terpotong menurunkan keterbacaan dan kualitas OCR. Scan tidak diunggah otomatis.
- **Masalah umum:** Izin camera ditolak, fokus buruk, edge salah, halaman hilang, atau scanner Desktop tidak tersedia. Berikan izin, retake/import, atau gunakan fallback yang dijelaskan aplikasi.
- **Batas kemampuan:** Integrasi scanner fisik bergantung perangkat/driver/approval dan tidak boleh dijanjikan sebelum PoC lulus.

---

## 4. Optimize PDF guides

### 4.1 Compress PDF

- **Nama:** Compress PDF.
- **Tujuan:** Mengurangi ukuran PDF dengan kompromi kualitas yang dapat dipahami.
- **Feature ID:** `FEAT-PDF-023`.
- **Tersedia pada:** Desktop `FULL`; Tablet/Phone `LIMITED*`; Web `UNRESOLVED`.
- **Memerlukan koneksi:** Tidak untuk path lokal yang eligible.
- **Format input:** PDF valid.
- **Format hasil:** PDF baru yang dioptimalkan ukurannya.
- **Sebelum memulai:** Periksa ukuran awal, kualitas gambar/teks yang harus dipertahankan, dan ruang output.
- **Langkah:** Pilih PDF → periksa status → pilih preset/tingkat kualitas yang tersedia → preview/estimasi bila tersedia → mulai → bandingkan hasil dengan sumber → Save As.
- **Opsi utama:** Preset kualitas/ukuran dan pengaturan gambar yang benar-benar tersedia pada build.
- **Hasil:** PDF yang umumnya lebih kecil; file sumber tidak berubah.
- **Peringatan:** Ukuran lebih kecil dapat menurunkan ketajaman gambar, hasil scan, atau kualitas cetak. Jangan hanya memeriksa ukuran; buka dan zoom hasil.
- **Masalah umum:** Ukuran hampir tidak berubah, kualitas terlalu rendah, atau proses kehabisan ruang. Pilih preset lain atau pertahankan sumber.
- **Batas kemampuan:** PDF yang sudah efisien mungkin tidak mengecil berarti. Tidak ada jaminan target ukuran tertentu tanpa policy/capability yang mendukung.

### 4.2 Repair PDF

- **Nama:** Repair PDF.
- **Tujuan:** Mencoba membuat PDF rusak/tidak konsisten dapat dibuka atau diproses kembali.
- **Feature ID:** `FEAT-PDF-025 / FEAT-PDF-026`.
- **Tersedia pada:** Desktop `FULL` untuk repair utama; advanced fallback capability-dependent; Web `UNRESOLVED (W*)`; Tablet/Phone `UNRESOLVED`.
- **Memerlukan koneksi:** Tidak untuk repair lokal utama; kondisional bila advanced fallback yang disetujui diperlukan.
- **Format input:** PDF yang gagal dibuka, rusak, atau tidak konsisten tetapi lolos batas keamanan untuk diperiksa.
- **Format hasil:** PDF hasil perbaikan bila berhasil.
- **Sebelum memulai:** Simpan file sumber, pastikan storage cukup, dan jangan menganggap file dapat dipulihkan sepenuhnya.
- **Langkah:** Pilih PDF → baca status kerusakan aman → mulai repair utama → bila ditawarkan, setujui fallback yang sesuai → periksa laporan/preview → buka hasil → Save As.
- **Opsi utama:** Repair standard; advanced repair hanya jika tersedia dan dijelaskan.
- **Hasil:** PDF baru yang telah divalidasi jika repair berhasil; jika tidak, status gagal tetap jelas tanpa result palsu.
- **Peringatan:** Tidak semua file rusak dapat dipulihkan. Halaman, form, attachment, font, atau struktur tertentu dapat tetap hilang/berubah.
- **Masalah umum:** File terlalu rusak, password diperlukan, pemeriksaan keamanan tidak selesai, atau hasil tetap tidak dapat dibuka. Gunakan salinan lain/backup atau minta sumber baru.
- **Batas kemampuan:** Repair bukan recovery forensik dan tidak boleh melewati proteksi/password atau mengaktifkan konten berbahaya.

### 4.3 OCR PDF

- **Nama:** OCR PDF.
- **Tujuan:** Menambahkan text layer agar PDF scan dapat dicari/disalin sejauh hasil pengenalan memungkinkan.
- **Feature ID:** `FEAT-OCR-004`.
- **Tersedia pada:** Desktop `SUPPORTED, GATED`; Tablet/Phone `LIMITED*`; Web `UNRESOLVED (W*)`.
- **Memerlukan koneksi:** Kondisional. OCR ringan dapat lokal; dokumen kompleks/besar dapat memerlukan layanan internal yang disetujui dengan notice.
- **Format input:** PDF scan/gambar dalam PDF; PDF berbasis teks dapat tidak memerlukan OCR.
- **Format hasil:** Searchable PDF dengan text layer.
- **Sebelum memulai:** Pilih bahasa bila tersedia, periksa kualitas scan, orientation, halaman, dan storage.
- **Langkah:** Pilih PDF → sistem menilai text layer → pilih bahasa/page range bila tersedia → mulai → pantau per halaman → preview/search sample → Save As.
- **Opsi utama:** Bahasa, page range, orientation correction, dan tingkat pemrosesan sesuai build.
- **Hasil:** PDF yang dapat dicari sejauh teks berhasil dikenali; tampilan sumber dipertahankan sejauh capability.
- **Peringatan:** Hasil OCR harus diperiksa, terutama pada scan buruk, tulisan kecil, tabel, banyak kolom, stempel, tulisan tangan, atau halaman miring.
- **Masalah umum:** Bahasa salah, teks acak, halaman tidak terbaca, proses lama, atau storage kurang. Perbaiki scan/rotation/language dan ulangi halaman yang perlu.
- **Batas kemampuan:** OCR tidak menjamin teks 100% benar dan tidak mengganti verifikasi manusia untuk data penting.

---

## 5. Convert to PDF guides

### 5.1 JPG to PDF

- **Nama:** JPG to PDF.
- **Tujuan:** Mengubah satu atau beberapa gambar JPG menjadi PDF berurutan.
- **Feature ID:** `FEAT-CONV-007`.
- **Tersedia pada:** Desktop `FULL`; Tablet/Phone `UNRESOLVED (P/T*)`; Web `UNRESOLVED`.
- **Memerlukan koneksi:** Tidak untuk path lokal yang eligible.
- **Format input:** JPG/JPEG valid yang didukung build.
- **Format hasil:** PDF.
- **Sebelum memulai:** Periksa orientation, urutan gambar, resolusi, dan apakah setiap gambar harus menjadi halaman terpisah.
- **Langkah:** Tambahkan gambar → ubah urutan → pilih orientation/page size/fit bila tersedia → preview → mulai → buka PDF → Save As.
- **Opsi utama:** Image order, orientation, page size, fit/margin, dan quality bila tersedia.
- **Hasil:** PDF dengan halaman gambar sesuai urutan akhir.
- **Peringatan:** Gambar beresolusi rendah tetap tampak buram; crop/fit dapat menambah margin atau memotong area bila dipilih.
- **Masalah umum:** Gambar berputar, urutan salah, terlalu besar, atau tidak valid. Rotate/reorder/resize secara aman lalu coba lagi.
- **Batas kemampuan:** Metadata gambar, transparansi non-JPG, dan color profile kompleks dapat berubah; periksa preview.

### 5.2 Word to PDF

- **Nama:** Word to PDF.
- **Tujuan:** Mengonversi dokumen Word menjadi PDF untuk dibaca/dibagikan sebagai layout tetap.
- **Feature ID:** `FEAT-CONV-001`.
- **Tersedia pada:** Desktop `SUPPORTED, GATED`; Web `UNRESOLVED (W*)`; Tablet/Phone `UNRESOLVED`.
- **Memerlukan koneksi:** Kondisional menurut capability/policy; aplikasi memberi notice sebelum transfer server.
- **Format input:** DOCX dan format Word lain hanya jika tercantum pada build.
- **Format hasil:** PDF.
- **Sebelum memulai:** Simpan perubahan Word, periksa font, page break, comments/track changes, active content, dan halaman yang ingin disertakan.
- **Langkah:** Pilih dokumen → periksa status → pilih halaman, orientation, page size, dan output setting bila tersedia → preview → mulai → bandingkan hasil → Save As.
- **Opsi utama:** Page selection, orientation, page size, print/output range, dan preview sesuai build.
- **Hasil:** PDF baru yang merepresentasikan dokumen Word sejauh capability konversi.
- **Peringatan:** Font yang tidak tersedia, field dinamis, object tertanam, macro, template, atau layout kompleks dapat berubah/tidak dijalankan. Hasil tidak dijanjikan selalu identik.
- **Masalah umum:** Font berganti, tabel/shape bergeser, halaman bertambah, atau format ditolak. Gunakan font yang didukung, sederhanakan layout, dan periksa preview.
- **Batas kemampuan:** Feature tetap feasibility-gated; hanya format dan fidelity yang telah disetujui boleh dipublikasikan.

### 5.3 PowerPoint to PDF

- **Nama:** PowerPoint to PDF.
- **Tujuan:** Mengonversi slide PowerPoint menjadi halaman PDF.
- **Feature ID:** `FEAT-CONV-003`.
- **Tersedia pada:** Desktop `SUPPORTED, GATED`; Web `UNRESOLVED (W*)`; Tablet/Phone `UNRESOLVED`.
- **Memerlukan koneksi:** Kondisional menurut capability/policy.
- **Format input:** PPTX dan format presentasi lain hanya jika didukung build.
- **Format hasil:** PDF.
- **Sebelum memulai:** Periksa slide order, font, notes/hidden slide, media, animation, dan slide yang ingin disertakan.
- **Langkah:** Pilih file → pilih slide/range bila tersedia → pilih orientation/page size/layout yang tersedia → preview → mulai → periksa urutan dan clipping → Save As.
- **Opsi utama:** Slide selection, orientation, page size, satu-slide/print layout hanya bila build menyediakannya.
- **Hasil:** PDF dengan halaman berdasarkan slide yang dipilih.
- **Peringatan:** Animation, transition, video, audio, atau interaksi tidak menjadi gerakan aktif di PDF. Font dan layout dapat berubah.
- **Masalah umum:** Slide terpotong, font berbeda, hidden slide ikut/tidak ikut, atau object tidak muncul. Koreksi selection/layout dan ulangi.
- **Batas kemampuan:** Fidelity kompleks dan active content tetap feasibility-gated; preview wajib diperiksa.

### 5.4 Excel to PDF

- **Nama:** Excel to PDF.
- **Tujuan:** Mengonversi workbook/sheet Excel menjadi PDF.
- **Feature ID:** `FEAT-CONV-002`.
- **Tersedia pada:** Desktop `SUPPORTED, GATED`; Web `UNRESOLVED (W*)`; Tablet/Phone `UNRESOLVED`.
- **Memerlukan koneksi:** Kondisional menurut capability/policy.
- **Format input:** XLSX dan format spreadsheet lain hanya jika didukung build.
- **Format hasil:** PDF.
- **Sebelum memulai:** Periksa sheet, print area, page breaks, orientation, page size, formula result, hidden rows/columns, dan scaling.
- **Langkah:** Pilih workbook → pilih sheet/range bila tersedia → atur orientation/page size/scaling yang ditawarkan → preview tiap halaman → mulai → periksa tabel → Save As.
- **Opsi utama:** Sheet selection, print area, orientation, page size, scaling/fit, dan page range sesuai build.
- **Hasil:** PDF yang berisi tampilan cetak sheet terpilih.
- **Peringatan:** Kolom lebar, merged cells, chart, formula dinamis, atau print area yang salah dapat menghasilkan banyak halaman/terpotong. Hasil tidak selalu identik dengan tampilan layar.
- **Masalah umum:** Sheet tidak ikut, kolom terpotong, halaman kosong, atau formula/display berbeda. Koreksi print area/scaling dan preview ulang.
- **Batas kemampuan:** Macro/active content tidak dijalankan sebagai baseline; fidelity workbook kompleks tetap gated.

### 5.5 HTML to PDF

- **Nama:** HTML to PDF.
- **Tujuan:** Mengubah konten HTML yang diizinkan menjadi PDF.
- **Feature ID:** `FEAT-CONV-017`.
- **Tersedia pada:** Desktop `SUPPORTED, GATED`; Web `UNRESOLVED (W*)`; Tablet/Phone `UNRESOLVED`.
- **Memerlukan koneksi:** Kondisional; input remote URL bukan baseline kontrak saat ini. Notice/consent berlaku jika jalur server resmi digunakan.
- **Format input:** File/konten HTML yang didukung dan diizinkan build; bukan URL arbitrer.
- **Format hasil:** PDF.
- **Sebelum memulai:** Pastikan content legal/aman, resource yang diperlukan tersedia secara diizinkan, dan layout dapat dipreview tanpa active content berbahaya.
- **Langkah:** Pilih/masukkan HTML melalui cara resmi → periksa resource/validation → pilih orientation/page size/page setting bila tersedia → preview → mulai → periksa layout/link → Save As.
- **Opsi utama:** Orientation, page size, margin/header/footer, dan page range hanya bila benar-benar tersedia.
- **Hasil:** PDF dari render HTML yang telah divalidasi.
- **Peringatan:** Script, active content, remote resource, custom/local scheme, download, dan credential tidak dijalankan/diambil secara bebas. Font/CSS/layout dapat berbeda.
- **Masalah umum:** Gambar/font tidak muncul, layout responsif berubah, halaman terpotong, atau resource diblokir. Gunakan input self-contained yang disetujui dan periksa preview.
- **Batas kemampuan:** Remote webpage-to-PDF memerlukan contract/PoC terpisah; jangan menjanjikan konversi URL umum.

---

## 6. Convert from PDF guides

### 6.1 PDF to JPG

- **Nama:** PDF to JPG.
- **Tujuan:** Mengubah halaman PDF menjadi gambar JPG.
- **Feature ID:** `FEAT-CONV-012`.
- **Tersedia pada:** Desktop `FULL`; Tablet/Phone `UNRESOLVED (P/T*)`; Web `UNRESOLVED`.
- **Memerlukan koneksi:** Tidak untuk path lokal yang eligible.
- **Format input:** PDF valid.
- **Format hasil:** Satu file JPG per halaman/selection sesuai build.
- **Sebelum memulai:** Tentukan halaman, resolution/quality, dan folder hasil.
- **Langkah:** Pilih PDF → pilih halaman/range → pilih resolution/quality bila tersedia → preview sample → mulai → buka beberapa JPG → simpan hasil.
- **Opsi utama:** Page selection, resolution, quality, color setting bila tersedia.
- **Hasil:** Gambar JPG sesuai halaman yang dipilih.
- **Peringatan:** Teks menjadi bagian gambar dan tidak lagi selectable; resolusi rendah mengurangi keterbacaan.
- **Masalah umum:** Hasil buram, warna berbeda, page selection salah, atau terlalu banyak output. Sesuaikan range/resolution.
- **Batas kemampuan:** Transparansi, vector detail, annotation, atau color profile dapat berubah pada rasterization.

### 6.2 PDF to Word

- **Nama:** PDF to Word.
- **Tujuan:** Membuat dokumen Word yang dapat diedit sejauh struktur PDF dapat dikenali.
- **Feature ID:** `FEAT-CONV-004`.
- **Tersedia pada:** Desktop `SUPPORTED, GATED`; Web `UNRESOLVED (W*)`; Tablet/Phone `UNRESOLVED`.
- **Memerlukan koneksi:** Kondisional menurut capability/policy.
- **Format input:** PDF berbasis teks atau PDF scan; scan dapat memerlukan OCR.
- **Format hasil:** Dokumen Word/DOCX jika didukung build.
- **Sebelum memulai:** Tentukan apakah PDF memiliki text layer, pilih halaman, dan siapkan OCR untuk scan bila ditawarkan.
- **Langkah:** Pilih PDF → lihat status text/scan → pilih page range/OCR bila tersedia → mulai → preview/buka Word → periksa paragraf, tabel, gambar, dan urutan → simpan.
- **Opsi utama:** Page selection, OCR language, layout/editability mode hanya bila tersedia.
- **Hasil:** Dokumen Word yang dapat diedit sebagian/seluruhnya sesuai struktur yang berhasil dikenali.
- **Peringatan:** PDF bukan format sumber editing. Kolom, font, equation, table, textbox, header/footer, dan posisi dapat berubah. Jangan menjanjikan hasil identik.
- **Masalah umum:** Teks menjadi gambar, paragraf terpecah, tabel berantakan, atau scan kosong. Aktifkan OCR bila sah/tersedia dan koreksi hasil manual.
- **Batas kemampuan:** Editability/fidelity tetap feasibility-gated; dokumen kompleks mungkin memerlukan rekonstruksi manual.

### 6.3 PDF to PowerPoint

- **Nama:** PDF to PowerPoint.
- **Tujuan:** Mengubah halaman PDF menjadi presentasi yang dapat diedit sejauh struktur memungkinkan.
- **Feature ID:** `FEAT-CONV-006`.
- **Tersedia pada:** Desktop `SUPPORTED, GATED`; Web `UNRESOLVED (W*)`; Tablet/Phone `UNRESOLVED`.
- **Memerlukan koneksi:** Kondisional menurut capability/policy.
- **Format input:** PDF berbasis teks atau scan; OCR dapat diperlukan.
- **Format hasil:** PPTX/presentasi yang didukung build.
- **Sebelum memulai:** Pilih halaman, tentukan apakah setiap halaman menjadi slide, dan periksa kebutuhan OCR.
- **Langkah:** Pilih PDF → pilih page range → aktifkan OCR bila ditawarkan/diperlukan → mulai → buka hasil → periksa slide order, text, image, dan object → simpan.
- **Opsi utama:** Page selection, slide mapping/layout, OCR language sesuai build.
- **Hasil:** Presentasi dengan slide berdasarkan halaman/struktur PDF.
- **Peringatan:** Object mungkin menjadi gambar atau kelompok elemen; animation/transition asli tidak dapat direkonstruksi dari PDF.
- **Masalah umum:** Slide tidak editable, element bergeser, teks hilang, atau segmentation salah. Koreksi manual atau gunakan output visual sesuai kebutuhan.
- **Batas kemampuan:** Editability dan slide segmentation tetap gated; hasil bukan jaminan rekonstruksi file PowerPoint asli.

### 6.4 PDF to Excel

- **Nama:** PDF to Excel.
- **Tujuan:** Mengekstrak tabel/data PDF menjadi workbook Excel sejauh struktur dapat dikenali.
- **Feature ID:** `FEAT-CONV-005`.
- **Tersedia pada:** Desktop `SUPPORTED, GATED`; Web `UNRESOLVED (W*)`; Tablet/Phone `UNRESOLVED`.
- **Memerlukan koneksi:** Kondisional menurut capability/policy.
- **Format input:** PDF berbasis teks atau scan; scan dapat memerlukan OCR.
- **Format hasil:** XLSX/workbook yang didukung build.
- **Sebelum memulai:** Pilih halaman/tabel, periksa text layer, bahasa/angka, merged cells, dan kebutuhan OCR.
- **Langkah:** Pilih PDF → pilih page/table area bila tersedia → pilih OCR/language bila perlu → mulai → preview table mapping → buka Excel → periksa cell/angka/header → simpan.
- **Opsi utama:** Page/table selection, OCR, table detection, sheet split sesuai build.
- **Hasil:** Workbook dengan data yang berhasil dipetakan ke cell.
- **Peringatan:** Formula asli tidak dapat dipulihkan dari tampilan PDF; tabel kompleks, border tidak jelas, multi-line cell, dan scan buruk dapat salah. Verifikasi angka penting.
- **Masalah umum:** Kolom bergeser, angka/tanggal salah, tabel menjadi teks, atau tidak ada table terdeteksi. Pilih area lebih sempit/perbaiki scan lalu koreksi manual.
- **Batas kemampuan:** Table/cell fidelity dan keamanan data hasil tetap feasibility-gated; output editable bukan replika workbook sumber.

### 6.5 PDF to PDF/A

- **Nama:** PDF to PDF/A.
- **Tujuan:** Mengonversi PDF ke bentuk PDF/A yang didukung untuk kebutuhan arsip jangka panjang.
- **Feature ID:** `FEAT-PDF-075`.
- **Tersedia pada:** Desktop `SUPPORTED, GATED`; Web `UNRESOLVED (W*)`; Tablet/Phone `UNRESOLVED`.
- **Memerlukan koneksi:** Kondisional menurut capability/policy.
- **Format input:** PDF valid.
- **Format hasil:** PDF/A pada profile yang benar-benar didukung build.
- **Sebelum memulai:** Ketahui kebutuhan profile/aturan arsip resmi, periksa font, encryption, attachment, active content, dan metadata.
- **Langkah:** Pilih PDF → pilih profile bila tersedia/diwajibkan → lihat isu kompatibilitas → mulai → tunggu validation → review laporan/preview → Save As.
- **Opsi utama:** Profile/conformance target dan penanganan elemen tidak kompatibel hanya sesuai build/policy.
- **Hasil:** File hanya disebut PDF/A bila validasi conformance yang disetujui berhasil.
- **Peringatan:** Mengubah ekstensi atau sekadar menyimpan ulang tidak membuktikan PDF/A. Elemen tertentu dapat dihapus/diubah dan konversi dapat gagal.
- **Masalah umum:** Font tidak tertanam, encryption/attachment tidak sesuai, metadata kurang, atau validation gagal. Perbaiki sumber atau pilih profile resmi yang tepat.
- **Batas kemampuan:** Feature berstatus feasibility gate; manual tidak boleh menjanjikan semua PDF dapat menjadi semua profile PDF/A.

---

## 7. Edit PDF guides

### 7.1 Rotate PDF

- **Nama:** Rotate PDF.
- **Tujuan:** Memutar satu atau beberapa halaman PDF ke orientation yang benar.
- **Feature ID:** `FEAT-PDF-012`.
- **Tersedia pada:** Desktop `FULL`; Tablet/Phone `LIMITED*`; Web `UNRESOLVED`.
- **Memerlukan koneksi:** Tidak untuk path lokal yang eligible.
- **Format input:** PDF valid.
- **Format hasil:** PDF baru dengan rotasi halaman diterapkan.
- **Sebelum memulai:** Pilih halaman dan arah rotasi; pastikan preview page number benar.
- **Langkah:** Buka PDF → pilih halaman/range → Rotate left/right sesuai control → preview semua halaman terkait → Undo bila salah → finalisasi → Save As.
- **Opsi utama:** Page selection/range dan arah/sudut yang tersedia.
- **Hasil:** Halaman tampil dalam orientation baru; sumber tetap tidak berubah.
- **Peringatan:** Rotasi tampilan sementara berbeda dari rotasi yang difinalisasi ke hasil; periksa preview.
- **Masalah umum:** Hanya satu halaman berubah, selection salah, atau hasil kembali miring. Pilih ulang page range dan finalisasi.
- **Batas kemampuan:** Rotasi tidak memperbaiki perspective/skew isi scan; gunakan Scan/OCR correction bila tersedia.

### 7.2 Add page numbers

- **Nama:** Add page numbers.
- **Tujuan:** Menambahkan nomor halaman pada PDF.
- **Feature ID:** `FEAT-PDF-037`.
- **Tersedia pada:** Desktop `FULL`; Tablet/Phone `UNRESOLVED (P/T*)`; Web `UNRESOLVED`.
- **Memerlukan koneksi:** Tidak untuk path lokal yang eligible.
- **Format input:** PDF valid.
- **Format hasil:** PDF baru dengan nomor halaman.
- **Sebelum memulai:** Tentukan page range, angka awal, posisi, dan ruang margin agar nomor tidak menutup content.
- **Langkah:** Pilih PDF → pilih halaman → atur start/position/format yang tersedia → preview halaman awal/tengah/akhir → mulai → Save As.
- **Opsi utama:** Range, start number, position, format/style bila tersedia.
- **Hasil:** Nomor halaman tampil sesuai setting pada hasil final.
- **Peringatan:** Nomor logis pada dokumen dapat berbeda dari index halaman PDF; verifikasi sample dan halaman yang dikecualikan.
- **Masalah umum:** Nomor tertutup, terpotong, salah mulai, atau mengenai footer. Ubah position/margin/range.
- **Batas kemampuan:** Penomoran kompleks per section/chapter hanya tersedia jika build menawarkannya.

### 7.3 Add watermark

- **Nama:** Add watermark.
- **Tujuan:** Menambahkan watermark teks atau gambar pada halaman PDF.
- **Feature ID:** `FEAT-PDF-036`.
- **Tersedia pada:** Desktop `FULL`; Tablet/Phone `UNRESOLVED (P/T*)`; Web `UNRESOLVED`.
- **Memerlukan koneksi:** Tidak untuk path lokal yang eligible.
- **Format input:** PDF; teks atau gambar watermark sesuai opsi build.
- **Format hasil:** PDF baru dengan watermark.
- **Sebelum memulai:** Siapkan teks/gambar, page range, opacity, posisi, dan pastikan tidak menutupi informasi penting.
- **Langkah:** Pilih PDF → pilih watermark type → atur position/opacity/scale/repeat bila tersedia → preview beberapa halaman → mulai → Save As.
- **Opsi utama:** Text/image, position, rotation, opacity, scale, page range, repeat sesuai build.
- **Hasil:** Watermark terlihat pada halaman yang dipilih.
- **Peringatan:** Watermark bukan redaction dan bukan DRM; content di bawahnya mungkin tetap dapat dibaca/diekstrak.
- **Masalah umum:** Watermark terlalu gelap/kecil, gambar tidak transparan, atau hanya muncul di sebagian halaman. Koreksi opsi/range.
- **Batas kemampuan:** Efek watermark pada layer/print/viewer berbeda harus diperiksa; tidak menjamin pencegahan penyalinan.

### 7.4 Crop PDF

- **Nama:** Crop PDF.
- **Tujuan:** Mengubah area halaman yang terlihat pada PDF.
- **Feature ID:** `FEAT-PDF-019`.
- **Tersedia pada:** Desktop `FULL`; Tablet/Phone `LIMITED*`; Web `UNRESOLVED`.
- **Memerlukan koneksi:** Tidak untuk path lokal yang eligible.
- **Format input:** PDF valid.
- **Format hasil:** PDF baru dengan crop diterapkan.
- **Sebelum memulai:** Pilih halaman, zoom untuk melihat batas, dan pastikan tidak memotong header/footer/tanda penting.
- **Langkah:** Pilih PDF → pilih halaman → tarik/masukkan crop bounds → pilih apply scope bila tersedia → preview → Undo/koreksi → finalisasi → Save As.
- **Opsi utama:** Crop rectangle, page range/apply-to pages, margin/box setting bila tersedia.
- **Hasil:** Area halaman pada hasil mengikuti crop yang dipilih.
- **Peringatan:** Crop tidak sama dengan Redact; content di luar area terlihat dapat tetap ada dalam struktur PDF.
- **Masalah umum:** Content terpotong, crop tidak konsisten pada ukuran halaman berbeda, atau selection bergeser saat rotation. Terapkan per kelompok ukuran/orientation dan preview.
- **Batas kemampuan:** Crop tidak menghapus informasi sensitif secara aman dan tidak otomatis menyeragamkan ukuran halaman.

### 7.5 Edit PDF

- **Nama:** Edit PDF.
- **Tujuan:** Mengedit PDF melalui workspace capability yang tersedia tanpa mengubah sumber langsung.
- **Feature ID:** `FEAT-PDF-031..041`.
- **Tersedia pada:** Desktop `SUPPORTED` dengan existing-text subset `GATED`; Tablet/Phone `LIMITED*`; Web `UNRESOLVED (W*)`.
- **Memerlukan koneksi:** Tidak untuk capability lokal; kondisional untuk subset/fallback resmi yang membutuhkan server.
- **Format input:** PDF valid; asset gambar/PDF tambahan sesuai action yang tersedia.
- **Format hasil:** PDF baru.
- **Sebelum memulai:** Tentukan apakah perlu **Add Text** atau **Edit Existing Text**, siapkan font/asset, dan periksa struktur PDF.
- **Langkah:** Buka workspace → pilih halaman → tambah/edit objek yang tersedia → atur position/size/style → gunakan Undo/Redo → preview → lihat change summary → finalisasi → Save As.
- **Opsi utama:** Add Text, Edit Existing Text, Add Image, Add Shape, Add Line/Arrow, Header, Footer, Overlay, Underlay sesuai build.
- **Hasil:** PDF baru dengan perubahan yang telah dipreview.
- **Peringatan:** **Add Text** menambahkan objek teks baru. **Edit Existing Text** mencoba mengubah teks yang sudah tertanam dan hanya cocok untuk struktur/font/layout tertentu. Keduanya bukan hal yang sama.
- **Masalah umum:** Existing text tidak dapat dipilih, font berubah, text reflow buruk, object tertutup layer, atau asset tidak didukung. Gunakan Add Text/overlay yang jelas atau workflow lain; jangan memaksa hasil palsu.
- **Batas kemampuan:** Existing-text editing tetap feasibility-gated. PDF hasil scan membutuhkan OCR untuk mengenali teks, tetapi OCR tidak otomatis membuat semua teks aman diedit dengan layout asli.

### 7.6 PDF Forms

- **Nama:** PDF Forms.
- **Tujuan:** Mengisi atau mengelola field form PDF sesuai capability build.
- **Feature ID:** `FEAT-PDF-053..060`.
- **Tersedia pada:** Desktop `SUPPORTED`; Tablet/Phone `UNRESOLVED (P/T*)`; Web `UNRESOLVED`.
- **Memerlukan koneksi:** Tidak untuk baseline lokal yang eligible.
- **Format input:** PDF form atau PDF yang akan diberi field.
- **Format hasil:** PDF dengan nilai/field form; PDF flattened bila dipilih.
- **Sebelum memulai:** Tentukan apakah hanya mengisi, membuat/edit field, atau flatten; simpan salinan editable sebelum flatten.
- **Langkah:** Buka PDF → pilih Fill/Create mode yang tersedia → isi/tambah/edit field → tab/review seluruh field → validasi → preview → pilih flatten hanya bila diperlukan → Save As → reopen.
- **Opsi utama:** Fill Form; Create Form; text field, checkbox, radio, dropdown, visual signature field; edit field; flatten sesuai build.
- **Hasil:** PDF dengan field/nilai yang dapat dibuka ulang; flatten mengubah field menjadi content tetap sejauh didukung.
- **Peringatan:** Flatten dapat menghilangkan kemampuan mengedit field. Visual signature field tetap non-PKI dan tidak menjadi certificate signature.
- **Masalah umum:** Field tidak dapat diisi, nilai hilang saat reopen, tab order membingungkan, atau flatten terlalu awal. Simpan salinan editable dan ulangi pada field yang didukung.
- **Batas kemampuan:** Create/advanced form adalah capability build-dependent; script/action form aktif tidak dijalankan bebas dan beberapa form dinamis mungkin tidak didukung.

---

## 8. PDF Security guides

### 8.1 Unlock PDF

- **Nama:** Unlock PDF.
- **Tujuan:** Membuat salinan PDF yang dapat dibuka tanpa proteksi yang dipilih, hanya dengan password dan hak yang sah.
- **Feature ID:** `FEAT-PDF-062`.
- **Tersedia pada:** Desktop `FULL`; Tablet/Phone `UNRESOLVED (P/T*)`; Web `UNRESOLVED`.
- **Memerlukan koneksi:** Tidak untuk path lokal yang eligible; reauthorization/policy dapat memerlukan online sesuai konteks.
- **Format input:** PDF terlindungi yang didukung.
- **Format hasil:** PDF baru yang proteksinya dibuka sesuai capability/otorisasi sah.
- **Sebelum memulai:** Pastikan Anda berhak memproses file dan memiliki password yang benar.
- **Langkah:** Pilih PDF → masukkan password → baca confirmation/authority notice → mulai → preview/reopen hasil → Save As.
- **Opsi utama:** Hanya opsi unlock yang sah dan tersedia pada build; tidak ada mode bypass/crack.
- **Hasil:** Salinan baru dapat dibuka sesuai outcome; sumber tetap terlindungi/tidak berubah.
- **Peringatan:** Produk tidak menyediakan cara melewati password/encryption tanpa otorisasi. Password hanya digunakan selama operasi dan tidak disimpan/log/audit.
- **Masalah umum:** Password salah, format proteksi tidak didukung, permission/policy menolak, atau hasil gagal divalidasi. Dapatkan password/otorisasi yang benar; jangan mencari bypass.
- **Batas kemampuan:** Proteksi/DRM/certificate yang tidak didukung tidak dapat dihapus. Unlock bukan pemulihan password.

### 8.2 Protect PDF

- **Nama:** Protect PDF.
- **Tujuan:** Membuat salinan PDF dengan password/permission yang didukung.
- **Feature ID:** `FEAT-PDF-061`.
- **Tersedia pada:** Desktop `FULL`; Tablet/Phone `UNRESOLVED (P/T*)`; Web `UNRESOLVED`.
- **Memerlukan koneksi:** Tidak untuk path lokal yang eligible; policy/reauth dapat bersifat kondisional.
- **Format input:** PDF valid.
- **Format hasil:** PDF baru yang terlindungi.
- **Sebelum memulai:** Pilih password kuat sesuai policy, konfirmasi dengan benar, dan tentukan permission yang benar-benar didukung.
- **Langkah:** Pilih PDF → masukkan/konfirmasi password → pilih permission bila tersedia → review dampak → final confirmation → proses → reopen dengan password benar/salah untuk memeriksa → Save As.
- **Opsi utama:** Open password dan permission print/copy/edit hanya sejauh didukung build/policy.
- **Hasil:** PDF baru meminta password/menerapkan permission yang dipilih sejauh mekanisme PDF mendukung.
- **Peringatan:** Aplikasi tidak menyimpan password sebagai recovery. Jika lupa, aplikasi tidak dapat mengembalikannya. Permission PDF bukan DRM absolut.
- **Masalah umum:** Password confirmation berbeda, output masih terbuka karena viewer cache, atau permission tidak didukung viewer. Tutup/reopen pada pemeriksaan yang benar dan review setting.
- **Batas kemampuan:** Tidak menjamin semua viewer menghormati seluruh permission non-encryption; tidak menggantikan klasifikasi/access control enterprise.

### 8.3 Sign PDF

- **Nama:** Sign PDF.
- **Tujuan:** Menempatkan tanda tangan visual/paraf/stamp pada PDF.
- **Feature ID:** `FEAT-SIGN-013`.
- **Tersedia pada:** Desktop `FULL`; Tablet/Phone `SUPPORTED`; Web `UNRESOLVED`.
- **Memerlukan koneksi:** Tidak untuk path lokal yang eligible.
- **Format input:** PDF + signature asset/drawing yang dipilih user.
- **Format hasil:** PDF baru dengan visual signature.
- **Sebelum memulai:** Pastikan aset memang milik/diizinkan, pilih metode drawing/upload/paste/saved asset yang tersedia, dan periksa halaman target.
- **Langkah:** Pilih PDF → buat/pilih signature → preview → tempatkan → move/resize → clear/undo bila perlu → review seluruh halaman → final confirmation → Save As.
- **Opsi utama:** Touch/stylus drawing pada Mobile/Tablet; upload/paste transparent image, saved asset terlindungi, mouse/stylus pada Desktop sesuai capability.
- **Hasil:** PDF baru menampilkan visual signature pada posisi/ukuran yang dipilih.
- **Peringatan:** Ini **visual signature/paraf non-PKI**, bukan certificate/digital signature dan tidak memberi verifikasi cryptographic. Asset tidak auto-upload/sync/telemetry/audit payload.
- **Masalah umum:** Background tidak transparan, posisi salah, ukuran pecah, canvas hilang saat rotate, atau asset tidak dapat dibuka. Gunakan preview/Undo dan pilih asset yang valid.
- **Batas kemampuan:** Tidak membuat sertifikat, trust chain, timestamp, atau legal PKI assurance. Penyimpanan saved signature hanya setelah pilihan user dan kontrol aman.

### 8.4 Redact PDF

- **Nama:** Redact PDF.
- **Tujuan:** Menghilangkan informasi sensitif secara permanen pada hasil final sejauh capability yang didukung.
- **Feature ID:** `FEAT-PDF-069`.
- **Tersedia pada:** Desktop `FULL`; Tablet/Phone `UNRESOLVED (P/T*)`; Web `UNRESOLVED`.
- **Memerlukan koneksi:** Tidak untuk path lokal yang eligible; policy/reauth dapat bersifat kondisional.
- **Format input:** PDF valid; scan dapat mempunyai image/OCR content.
- **Format hasil:** PDF baru dengan redaction final.
- **Sebelum memulai:** Gunakan salinan sumber, identifikasi semua teks/gambar/annotation/layer/hasil OCR terkait, dan pastikan target benar.
- **Langkah:** Pilih PDF → tandai area/content → preview setiap halaman → gunakan search/review tambahan bila tersedia → lihat summary → final confirmation → proses → buka hasil → uji search/copy/extract yang tersedia → Save As.
- **Opsi utama:** Area/text selection, page range, reason/appearance hanya bila tersedia; finalization selalu terpisah dari preview.
- **Hasil:** Content target dihapus/diubah secara permanen pada hasil final yang tervalidasi; sumber tetap tidak berubah.
- **Peringatan:** Kotak hitam/overlay saja **bukan redaction aman**. Final redaction dapat permanen dan tidak dapat di-Undo setelah hasil difinalisasi; preview sangat penting.
- **Masalah umum:** Teks masih dapat dicari/copy, layer tersembunyi tetap ada, OCR mengandung content, atau format tidak didukung. Jangan gunakan hasil; koreksi dan finalisasi ulang setelah capability memastikan penghapusan nyata.
- **Batas kemampuan:** Content yang tidak dapat diproses aman harus memblokir finalization atau menampilkan limitation; produk tidak boleh memberi jaminan palsu.

### 8.5 Compare PDF

- **Nama:** Compare PDF.
- **Tujuan:** Membandingkan dua PDF atau halaman tertentu dan menunjukkan perbedaan yang dapat ditinjau.
- **Feature ID:** `FEAT-PDF-071 / FEAT-PDF-072`.
- **Tersedia pada:** Desktop `FULL`; Web/Tablet/Phone `UNRESOLVED`.
- **Memerlukan koneksi:** Tidak untuk path lokal yang eligible.
- **Format input:** Dua PDF valid atau halaman dari dua PDF.
- **Format hasil:** Tampilan/data perbedaan; file report hanya jika build secara eksplisit menyediakan export.
- **Sebelum memulai:** Pastikan versi A/B tidak tertukar, pilih halaman/alignment yang sesuai, dan ketahui apakah dokumen berbasis teks atau scan.
- **Langkah:** Pilih PDF A dan B → pilih all/specific pages → atur alignment/compare mode bila tersedia → mulai → tinjau perbedaan → buka source side-by-side → simpan/export hanya bila ditawarkan.
- **Opsi utama:** Page selection, visual/structure comparison mode, sensitivity/alignment sesuai build.
- **Hasil:** Daftar/overlay/tampilan perbedaan yang merujuk jelas ke source A/B.
- **Peringatan:** Perbedaan visual dan struktur bergantung jenis dokumen. Reflow, scan noise, font, atau posisi kecil dapat menghasilkan banyak perbedaan yang perlu interpretasi manusia.
- **Masalah umum:** Halaman tidak sejajar, semua area ditandai berubah, atau perbedaan text tidak terdeteksi pada scan. Pilih page mapping yang benar atau gunakan OCR bila tersedia.
- **Batas kemampuan:** Compare bukan bukti hukum/semantik bahwa dokumen setara dan tidak menjamin menemukan setiap perubahan tersembunyi.

---

## 9. PDF Intelligence guides

### 9.1 AI Summarizer

- **Nama:** AI Summarizer.
- **Tujuan:** Membuat ringkasan isi PDF untuk membantu review, tanpa menggantikan dokumen sumber.
- **Feature ID:** `FEAT-INTEL-001`.
- **Tersedia pada:** Desktop `SUPPORTED, GATED`; Web/Tablet/Phone hanya jika exposure final menyatakan tersedia (`UNRESOLVED*`).
- **Memerlukan koneksi:** Kondisional. Dapat lokal bila capability disetujui; layanan enterprise-internal memerlukan koneksi, policy check, minimisasi, dan notice. Public AI dilarang.
- **Format input:** PDF berbasis teks atau scan; scan dapat memerlukan OCR.
- **Format hasil:** Ringkasan text/data yang ditandai sebagai hasil berbantuan AI.
- **Sebelum memulai:** Pilih dokumen/halaman yang tepat, periksa klasifikasi, text layer/OCR, dan jenis ringkasan bila tersedia.
- **Langkah:** Pilih PDF → pilih scope/jenis ringkasan bila tersedia → review kebutuhan koneksi/transfer → mulai → pantau → baca hasil → bandingkan poin penting dengan sumber → simpan/copy hanya bila diizinkan.
- **Opsi utama:** Scope halaman, panjang/jenis ringkasan, bahasa output sesuai provider/build.
- **Hasil:** Ringkasan yang grounded pada content terpilih dan dapat direview terhadap sumber.
- **Peringatan:** Ringkasan dapat melewatkan konteks, angka, pengecualian, atau salah menekankan informasi. Selalu bandingkan dengan sumber untuk keputusan penting.
- **Masalah umum:** Tidak ada text layer, OCR buruk, provider tidak tersedia, content diblok policy, atau ringkasan terlalu umum. Perbaiki OCR/scope atau review manual.
- **Batas kemampuan:** Feature/layanan masih feasibility-gated; bukan sumber kebenaran, bukan approval otomatis, dan tidak mengirim content ke layanan publik.

### 9.2 Translate PDF

- **Nama:** Translate PDF.
- **Tujuan:** Menerjemahkan content PDF ke bahasa target yang didukung.
- **Feature ID:** `FEAT-INTEL-002`.
- **Tersedia pada:** Desktop `SUPPORTED, GATED`; Web/Tablet/Phone hanya jika exposure final menyatakan tersedia (`UNRESOLVED*`).
- **Memerlukan koneksi:** Kondisional; layanan enterprise-internal dapat memerlukan koneksi, policy, minimisasi, dan notice. Layanan terjemahan publik dilarang.
- **Format input:** PDF berbasis teks atau scan; scan dapat memerlukan OCR.
- **Format hasil:** PDF/text/data terjemahan sesuai capability build.
- **Sebelum memulai:** Pilih bahasa sumber/target, scope halaman, istilah penting, klasifikasi, dan kebutuhan OCR.
- **Langkah:** Pilih PDF → pilih/deteksi bahasa sumber dan pilih target → review scope/koneksi → mulai → preview → periksa nama, angka, istilah teknis, dan layout → final export/Save As.
- **Opsi utama:** Source/target language, page scope, output form, glossary hanya bila resmi tersedia.
- **Hasil:** Terjemahan yang dapat dibandingkan dengan source dan dipreview sebelum final export.
- **Peringatan:** Istilah, angka, konteks, tabel, dan layout dapat salah/berubah. Hasil tidak menggantikan review penerjemah untuk content penting.
- **Masalah umum:** Bahasa salah terdeteksi, layout berubah, text tidak diterjemahkan karena scan, atau provider diblok/tidak tersedia. Pilih bahasa/OCR/scope yang benar atau gunakan review manual.
- **Batas kemampuan:** Language pair, fidelity, provider, data handling, dan capacity tetap gated; jangan menjanjikan layout identik.

### 9.3 PDF to Markdown

- **Nama:** PDF to Markdown.
- **Tujuan:** Mengekstrak struktur teks PDF menjadi file Markdown.
- **Feature ID:** `FEAT-CONV-036`.
- **Tersedia pada:** Desktop `SUPPORTED` dengan complex layout `GATED`; Web `UNRESOLVED (W*)`; Tablet/Phone `UNRESOLVED`.
- **Memerlukan koneksi:** Tidak untuk deterministic text extraction lokal; kondisional jika OCR/complex approved path diperlukan.
- **Format input:** PDF berbasis teks atau scan.
- **Format hasil:** `.md`/Markdown.
- **Sebelum memulai:** Periksa text layer, bahasa OCR bila scan, page scope, dan kompleksitas heading/list/table.
- **Langkah:** Pilih PDF → sistem mendeteksi text/scan → pilih page range/OCR bila tersedia → mulai → preview Markdown → periksa heading/list/table/order → koreksi bila perlu → Save As `.md`.
- **Opsi utama:** Page selection, OCR language, handling heading/list/table sesuai build.
- **Hasil:** Markdown yang mempertahankan struktur sejauh dapat dikenali.
- **Peringatan:** Heading, list, table, multi-column order, footnote, equation, dan image reference dapat memerlukan koreksi manual.
- **Masalah umum:** Urutan text salah, table pecah, heading datar, atau scan kosong. Gunakan OCR yang tepat dan edit Markdown setelah preview.
- **Batas kemampuan:** AI bukan dependency default. Complex layout/table/OCR fidelity tetap gated; output bukan representasi visual identik dari PDF.

---

## 10. Merge PDF complete guide

### 10.1 Ringkasan

**Merge PDF** menggabungkan dua atau lebih PDF menjadi satu hasil baru. Proses baseline berjalan lokal ketika device/capability/policy mengizinkan. Anda tidak perlu memilih cara teknis pemrosesan. File sumber tidak diubah dan tidak dikirim ke server untuk baseline Merge lokal.

```text
Input  : minimal dua PDF berurutan
Output : satu PDF final
Save   : Save As / file baru
Offline: dapat digunakan jika capability dan akses offline masih valid
```

### 10.2 Sebelum memulai

1. Pastikan sedikitnya dua PDF tersedia dan dapat dibaca.
2. Pastikan Anda memiliki password yang sah untuk PDF terlindungi.
3. Tentukan urutan dokumen yang diinginkan.
4. Pastikan lokasi hasil mempunyai ruang yang cukup.
5. Jika file sangat besar/banyak, siapkan waktu dan jangan menutup aplikasi sebelum status pekerjaan jelas.
6. Gunakan salinan data penting pada UAT/latihan; aplikasi tetap tidak boleh menimpa sumber secara otomatis.

### 10.3 Langkah lengkap

#### 1. Buka Merge PDF

Dari Home, pilih **Merge PDF**, cari melalui Feature Search, pilih file lalu rekomendasi Merge (File-First), atau minta Assistant menggabungkan file. Semua jalur harus menuju `FEAT-PDF-005` dengan aturan yang sama.

#### 2. Tambahkan file

Gunakan **Pilih File**. Pada Desktop, drag-and-drop dapat digunakan. Jika Mobile/Tablet mengekspos simple Merge, gunakan file picker/share input yang disediakan platform. Tambahkan minimal dua PDF.

#### 3. Periksa status file

Setiap input harus menunjukkan keadaan yang dapat dipahami, misalnya siap, memerlukan password, warning, unsupported, corrupted, blocked, atau storage tidak cukup. Jangan mulai sebelum semua input wajib eligible.

#### 4. Ubah urutan

Susun file sesuai urutan halaman yang diinginkan. Urutan pada daftar/preview adalah urutan hasil. Aplikasi tidak boleh mengurutkan ulang berdasarkan nama atau waktu tanpa terlihat.

#### 5. Hapus file yang salah

Hapus input yang tidak diperlukan dari working list. Tindakan ini tidak menghapus file sumber dari penyimpanan. Gunakan **Undo/Urungkan** bila salah.

#### 6. Pilih pengaturan hasil

Pilih nama/lokasi hasil dan pengaturan lain yang benar-benar disediakan build. Jangan memilih salah satu file sumber sebagai target. Jika nama target sudah ada, pilih nama baru atau tindakan overwrite resmi bila tersedia dan memang Anda maksudkan.

#### 7. Mulai

Pilih **Mulai/Gabungkan**. Untuk file besar, pekerjaan dapat berpindah ke background lokal. Status diterima/queued tidak sama dengan selesai.

#### 8. Pantau progress

Pantau persentase bila akurat, tahap, jumlah item, dan estimasi bila tersedia. Jika total belum diketahui, aplikasi menggunakan indikator tanpa persentase palsu. Anda dapat membuka Activity Center. **Batalkan** hanya ditawarkan bila aman.

#### 9. Tangani password atau error

- Jika password diminta, masukkan password yang sah. Password salah tidak menghasilkan output.
- Jika file rusak/unsupported, hapus atau ganti file tersebut.
- Jika storage kurang, pilih lokasi lain atau sediakan ruang.
- Jika proses gagal, baca apakah aman mencoba lagi. Jangan gunakan temp/partial file sebagai hasil.

#### 10. Preview hasil bila tersedia

Periksa jumlah halaman dan urutan awal/akhir. Untuk pemeriksaan yang mudah, gunakan dokumen dengan marker/judul halaman yang dikenali. Keberadaan file saja tidak membuktikan hasil benar.

#### 11. Save As

Simpan sebagai file baru di lokasi yang dipilih. Hasil harus divalidasi sebelum dipublikasikan. Jika penulisan/rename gagal, file target lama harus tetap aman.

#### 12. Buka hasil

Buka hasil setelah disimpan. Periksa bahwa PDF dapat dibaca, semua halaman ada, urutan benar, tidak ada halaman tambahan/hilang, dan History menunjuk ke outcome yang tepat.

#### 13. Amankan ke Akun bila diperlukan

Pilih **Amankan ke Akun** hanya jika ingin menyimpan hasil terpilih untuk akses lintas perangkat. Tindakan ini memerlukan koneksi, permission, quota/policy, dan confirmation. Ini bukan auto-sync workspace. Maksimum tiga versi file yang diamankan berlaku sesuai policy; sumber, draft, undo, cache, temp, checkpoint, password, dan signature asset tidak ikut otomatis.

### 10.4 Skenario pengguna

#### A. Dua file

1. Tambahkan PDF A lalu PDF B.
2. Pastikan keduanya `READY`/siap.
3. Periksa urutan A, B.
4. Mulai dan Save As.
5. Buka hasil dan pastikan seluruh halaman A muncul sebelum B.

**Berhasil bila:** satu PDF baru benar, A/B tidak berubah, dan tidak ada upload pemrosesan.

#### B. Banyak file

1. Tambahkan seluruh file dalam batas yang diizinkan.
2. Periksa duplicate dan status masing-masing.
3. Reorder secara bertahap; gunakan preview/list yang jelas.
4. Mulai, pantau Activity Center, lalu validasi page count/order.

**Berhasil bila:** UI tetap dapat digunakan, urutan dapat diprediksi, hasil atomik, dan limit dijelaskan sebelum resource habis. Duplicate yang sengaja ditambahkan tidak boleh dihapus diam-diam.

#### C. File rusak

1. Tambahkan dua PDF valid dan satu PDF rusak.
2. Periksa status file rusak.
3. Hapus/ganti file rusak atau batalkan.
4. Lanjutkan hanya ketika minimal dua input valid tersedia.

**Berhasil bila:** tidak ada hasil parsial palsu; pesan menjelaskan input yang harus diperbaiki; sumber tetap aman.

#### D. File berpassword

1. Tambahkan PDF terlindungi.
2. Masukkan password salah untuk memastikan penolakan aman.
3. Masukkan password benar yang sah.
4. Lanjutkan Merge dan buka hasil.

**Berhasil bila:** password tidak muncul di History, log, issue, atau layar setelah tidak diperlukan; password salah tidak memberi bypass.

#### E. Cancel

- **Sebelum mulai:** Cancel menghasilkan state cancelled dan tidak memulai pekerjaan.
- **Saat berjalan:** Cancel menjadi permintaan berhenti pada safe point. Tunggu sampai status `CANCELLED`; jangan menganggap klik tombol berarti proses telah berhenti seketika.

**Berhasil bila:** tidak ada completion terlambat/menyesatkan, tidak ada hasil final, temp tidak ditawarkan, dan source tetap aman.

#### F. Storage penuh/kurang

1. Mulai dari preflight dengan lokasi yang tidak cukup.
2. Baca kebutuhan/tindakan yang diperbolehkan.
3. Pilih lokasi lain atau sediakan ruang, lalu retry.

**Berhasil bila:** proses berisiko dihentikan sebelum output rusak; existing target/source tidak berubah; user tidak terjebak silent wait.

#### G. Offline

1. Putuskan koneksi pada account/offline grant yang masih valid.
2. Tambahkan PDF lokal dan jalankan Merge.
3. Save As dan buka hasil.
4. Amati History/Audit metadata yang menunggu sinkronisasi aman.

**Berhasil bila:** Merge selesai lokal tanpa upload/content sync. **Amankan ke Akun** tetap online-required. Jika offline grant tidak valid, aplikasi fail closed/online-required tanpa memindahkan file.

### 10.5 Recovery

Jika aplikasi atau pekerjaan background lokal berhenti:

1. Buka kembali PERTAMINA OTO.
2. Periksa recovery candidate di startup/Activity Center.
3. Pilih **Lanjutkan** hanya bila checkpoint compatible ditawarkan.
4. Pilih **Coba Lagi** bila resume tidak didukung tetapi input/working order aman.
5. Pilih **Batalkan** untuk membuang pekerjaan sementara, bukan file sumber.
6. Masukkan kembali password bila diperlukan; password tidak dipulihkan dari database.

Jangan gunakan file temp/setengah jadi. Result hanya final setelah validation dan Save As berhasil.

### 10.6 Checklist hasil Merge

```text
[ ] minimal dua input yang benar telah digunakan
[ ] urutan hasil sama dengan daftar akhir
[ ] jumlah halaman sesuai
[ ] tidak ada halaman hilang/tambahan tanpa intent
[ ] result dapat dibuka ulang
[ ] semua source tetap dapat dibuka dan tidak berubah
[ ] existing target tidak tertimpa tanpa confirmation resmi
[ ] status success baru muncul setelah result valid
[ ] History/Activity menunjukkan outcome yang benar
[ ] tidak ada pengiriman file ke server untuk baseline Merge lokal
```

---

## 11. Warning catalog

Katalog berikut adalah pesan/konsep warning untuk konsistensi manual. Ini bukan Error Code baru.

| Ref manual | Berlaku pada | Warning user-facing yang direkomendasikan |
|---|---|---|
| `WARN-MAN-001` | Semua tool | “File sumber tidak akan ditimpa otomatis. Simpan hasil sebagai file baru atau konfirmasi overwrite resmi bila tersedia.” |
| `WARN-MAN-002` | Tool server-conditional | “Koneksi diperlukan. Dokumen hanya dikirim setelah pemberitahuan, pemeriksaan policy, dan persetujuan yang berlaku.” |
| `WARN-MAN-003` | Password PDF | “Password hanya digunakan selama operasi dan tidak disimpan sebagai recovery.” |
| `WARN-MAN-004` | Compress | “Ukuran lebih kecil dapat mengurangi kualitas gambar/scan. Periksa hasil sebelum digunakan.” |
| `WARN-MAN-005` | Repair | “Tidak semua file rusak dapat dipulihkan. Hasil hanya tersedia bila validation berhasil.” |
| `WARN-MAN-006` | OCR | “Hasil pengenalan dapat salah, terutama pada scan buruk, tabel, banyak kolom, stempel, atau tulisan tangan.” |
| `WARN-MAN-007` | Office/HTML → PDF | “Font, layout, page break, object, atau active content dapat berubah/tidak didukung. Preview hasil.” |
| `WARN-MAN-008` | PDF → Office | “Hasil editable bukan rekonstruksi file sumber asli. Layout, table, font, dan object dapat memerlukan koreksi.” |
| `WARN-MAN-009` | PDF/A | “File hanya disebut PDF/A setelah conformance validation berhasil.” |
| `WARN-MAN-010` | Crop/Watermark | “Crop/watermark bukan redaction dan tidak menghapus content sensitif dari struktur file.” |
| `WARN-MAN-011` | Protect | “Aplikasi tidak dapat memulihkan password yang terlupa. PDF permission bukan DRM absolut.” |
| `WARN-MAN-012` | Unlock | “Gunakan hanya jika Anda mempunyai password dan hak yang sah. Tool ini bukan mekanisme bypass.” |
| `WARN-MAN-013` | Sign | “Ini adalah visual signature/paraf non-PKI, bukan certificate/digital signature.” |
| `WARN-MAN-014` | Redact | “Final redaction dapat permanen. Periksa setiap target; overlay saja bukan redaction aman.” |
| `WARN-MAN-015` | Compare | “Perbedaan visual/struktur bergantung jenis dokumen dan memerlukan interpretasi pengguna.” |
| `WARN-MAN-016` | Summarize/Translate | “Hasil berbantuan AI/translation dapat salah atau kehilangan konteks. Bandingkan dengan sumber.” |
| `WARN-MAN-017` | PDF to Markdown | “Heading, list, table, dan reading order dapat perlu koreksi manual.” |
| `WARN-MAN-018` | Save/finalization | “Hasil sementara belum final. Tunggu validation dan Save As selesai.” |
| `WARN-MAN-019` | Amankan ke Akun | “Hanya hasil yang dipilih yang diamankan. Draft/source/cache/temp tidak ikut otomatis; maksimum tiga versi berlaku.” |
| `WARN-MAN-020` | Gated/limited tool | “Capability ini bergantung build, platform, policy, dan hasil feasibility gate. Opsi yang tidak tersedia tidak boleh dijanjikan.” |

---

## 12. Common-error catalog

Katalog ini menggunakan kategori user-facing. UI tetap menampilkan Error Code canonical yang sebenarnya pada detail aman/Support bila tersedia; manual tidak membuat code baru.

| Gejala | Kemungkinan aman | Tindakan pengguna | Jangan dilakukan |
|---|---|---|---|
| File ditolak saat dipilih | Format/type/signature/size/permission tidak sesuai | Pilih file yang didukung dan dapat dibaca; periksa extension bukan satu-satunya bukti | Mengganti extension untuk memaksa file |
| Password diperlukan/salah | PDF terlindungi | Masukkan password sah; minta pemilik file bila tidak tahu | Mencari bypass atau memasukkan password ke Issue/chat |
| File rusak | Struktur tidak dapat divalidasi | Coba Repair pada salinan, cari backup, atau minta source baru | Menggunakan output temp sebagai final |
| Capability tidak tersedia | Platform/build/flag/policy/device tidak mendukung | Gunakan fallback resmi atau platform lain yang dijelaskan | Memaksa deep link/Assistant/direct route |
| Koneksi diperlukan | Path yang dipilih memerlukan layanan internal | Hubungkan ke jaringan resmi, review notice, lalu lanjutkan bila disetujui | Menganggap reconnect otomatis memberi consent baru |
| Menunggu koneksi | Job/intent belum dapat melanjutkan | Buka Activity Center; reconnect; review intent/policy | Mengirim perintah berulang kali |
| Menunggu resource/layanan sibuk | Kapasitas/antrean/layanan belum aman | Ikuti saran waktu mencoba lagi atau coba nanti; cancel bila tersedia | Retry cepat berulang atau fallback publik |
| Storage tidak cukup | Temp/output space di bawah kebutuhan | Pilih lokasi lain, bersihkan ruang aman, lalu retry | Menghapus folder sistem/temp secara manual tanpa panduan |
| Process cancelled | User meminta cancel atau policy stop | Mulai lagi bila diperlukan; periksa tidak ada final | Menggunakan partial file |
| Result validation failed | Output tidak lengkap/format salah | Retry bila guidance menyatakan aman atau laporkan masalah | Menganggap file “ada” berarti benar |
| Save As conflict | Target sudah ada/sama dengan source | Pilih nama/lokasi baru atau governed overwrite action | Menimpa sumber tanpa confirmation |
| Result expired | Temporary server result melewati retention | Pilih rerun/reprocess secara eksplisit | Mengulang download tanpa akhir |
| OCR buruk | Scan/language/orientation tidak cocok | Retake/perbaiki scan, pilih bahasa, rotate, ulangi page scope | Memakai angka/nama penting tanpa review |
| Layout/font berbeda | Font/structure tidak dapat direproduksi | Preview, gunakan font/layout lebih sederhana, koreksi hasil | Menjanjikan hasil identik |
| Table tidak terdeteksi | Struktur PDF/scan terlalu kompleks | Pilih area lebih sempit, OCR, atau koreksi Excel manual | Menganggap formula asli dapat dipulihkan |
| Existing text tidak dapat diedit | Object/font/layout tidak didukung | Gunakan Add Text/overlay yang jelas atau workflow lain | Mengklaim Add Text sama dengan Edit Existing Text |
| Signature asset gagal | Type/size/transparency/access tidak sesuai | Pilih asset sah, preview, atau gambar ulang | Menaruh asset dalam Issue/telemetry |
| Scanner/camera tidak bekerja | Izin/device/driver/capability tidak tersedia | Berikan izin foreground, pilih camera lain, atau import scan | Menjanjikan physical scanner sebelum PoC |
| Redacted content masih dapat dicari/copy | Hanya overlay atau struktur tidak didukung | Jangan gunakan hasil; ulangi Redact dan verifikasi final | Membagikan hasil yang belum lolos pemeriksaan |
| Summarize/Translate unavailable | Provider/policy/capacity/connection tidak tersedia | Tunggu/ubah scope atau review sumber manual | Mengirim ke public AI/translation service |
| Aplikasi/pekerjaan background berhenti | Crash/interruption | Buka kembali; pilih Lanjutkan/Coba Lagi/Batalkan sesuai recovery | Menganggap status lama “masih berjalan” sebagai success |

---

## 13. Screenshot plan

Screenshot hanya dibuat dari release UI yang telah disetujui dan menggunakan file sintetis/sanitized. Jangan menampilkan password, isi sensitif, path penuh, token, signature asset nyata, atau data Produksi. Setiap screenshot harus mempunyai alt text/caption yang menjelaskan tujuan, bukan sekadar nama layar.

### 13.1 Organize PDF

| Prioritas | Screenshot minimum | Manfaat |
|---:|---|---|
| 1 | Halaman awal kelompok + enam Core cards | Menunjukkan cara menemukan Merge/Split/Remove/Extract/Organize/Scan |
| 2 | Merge input state dengan validation dan order | Menjelaskan status, reorder, remove, dan minimal dua file |
| 3 | Organize workspace dengan page selection + Undo | Menjelaskan working state multi-operasi tanpa screenshot per aksi |
| 4 | Scan capture/crop/reorder pada Mobile/Tablet | Menjelaskan alur platform-natural yang berbeda dari Desktop |
| 5 | Progress/Activity + result/Save As | Menjelaskan no silent wait dan finalization |

### 13.2 Optimize PDF

| Prioritas | Screenshot minimum | Manfaat |
|---:|---|---|
| 1 | Halaman awal Compress/Repair/OCR | Orientasi kelompok |
| 2 | Compress key option + preview/size-quality note | Menjelaskan kompromi kualitas |
| 3 | Repair status/warning dan success/failure outcome | Mencegah janji pasti pulih |
| 4 | OCR language/page option + searchable-result check | Menjelaskan setup dan review |
| 5 | Shared progress/result | Satu gambar dapat dipakai lintas tiga tool bila UI sama |

### 13.3 Convert to PDF

| Prioritas | Screenshot minimum | Manfaat |
|---:|---|---|
| 1 | Kelompok/card selection | Menunjukkan lima converter resmi |
| 2 | Input state representatif (Word/Excel atau JPG) | Menjelaskan format/status file |
| 3 | Key options: page/sheet/slide, orientation, page size | Menjelaskan pilihan tanpa satu gambar per control |
| 4 | Output preview dengan fidelity warning | Menekankan pemeriksaan font/layout |
| 5 | Result/Save As atau conditional server notice | Menjelaskan final output/koneksi bila berlaku |

### 13.4 Convert from PDF

| Prioritas | Screenshot minimum | Manfaat |
|---:|---|---|
| 1 | Kelompok/card selection | Orientasi lima tool |
| 2 | Input status “text layer” vs “scan/OCR needed” | Menjelaskan dependency OCR |
| 3 | Page/table/output option representatif | Menjelaskan scope konversi |
| 4 | Preview editable output limitation | Menjelaskan Word/PPT/Excel tidak selalu identik |
| 5 | PDF/A validation outcome / general result | Menjelaskan conformance dan finality |

### 13.5 Edit PDF

| Prioritas | Screenshot minimum | Manfaat |
|---:|---|---|
| 1 | Edit/Organize workspace dengan page pane | Menjelaskan struktur editing |
| 2 | Add Text dibanding Edit Existing Text state | Menjelaskan perbedaan paling penting |
| 3 | Page numbers/watermark/crop key option panel | Satu composite/representative view bila konsisten |
| 4 | PDF Forms fill/create/flatten state yang tersedia | Menjelaskan capability build |
| 5 | Change summary + preview/result | Menjelaskan non-destructive finalization |

### 13.6 PDF Security

| Prioritas | Screenshot minimum | Manfaat |
|---:|---|---|
| 1 | Security group landing + warnings | Orientasi dan risiko |
| 2 | Protect/Unlock password state dengan data sintetis/masked | Menjelaskan privacy dan hak sah tanpa menunjukkan password |
| 3 | Sign placement/resize + label non-PKI | Mencegah salah pengertian certificate signature |
| 4 | Redact preview + final confirmation | Menekankan permanensi dan overlay limitation |
| 5 | Compare A/B result view | Menjelaskan provenance/perbedaan |
| 6 | Security result/Save As | Menjelaskan source tetap aman |

### 13.7 PDF Intelligence

| Prioritas | Screenshot minimum | Manfaat |
|---:|---|---|
| 1 | Intelligence group landing | Menunjukkan tiga tool resmi, bukan menu Assistant tersembunyi |
| 2 | Input + text/OCR/preprocessing state | Menjelaskan text layer dan scan |
| 3 | Summary type / target language / Markdown option representatif | Menjelaskan key setup |
| 4 | Conditional provider/connection notice | Menjelaskan approved internal processing tanpa detail teknis |
| 5 | Result preview dengan review/compare-to-source warning | Menekankan human verification |

### 13.8 Merge PDF screenshot set paling lengkap

1. Merge landing/empty state dengan minimal input guidance.
2. Input state A/B/C dengan status, order, remove, dan Undo.
3. Progress/Activity Center untuk file besar, termasuk Cancel bila aman.
4. Result preview/change summary dengan page order.
5. Save As dan open result.
6. Optional **Amankan ke Akun** confirmation—hanya jika panduan selective sync memang dibahas pada halaman yang sama.
7. Satu error example yang paling actionable (invalid file atau insufficient storage), bukan screenshot untuk setiap error.

---

## 14. Feature traceability

| No | Tool | Canonical Feature ID | Requirement/gate utama | Manual section |
|---:|---|---|---|---|
| 1 | Merge PDF | `FEAT-PDF-005` | `FR-PROC-001..005`; `UIR-UI-005..010`; local first slice | 3.1; 10 |
| 2 | Split PDF | `FEAT-PDF-006 / 007` | `FR-PROC-001..005`; page/range outcome | 3.2 |
| 3 | Remove pages | `FEAT-PDF-013` | Non-destructive working state | 3.3 |
| 4 | Extract pages | `FEAT-PDF-014` | Page identity/order | 3.4 |
| 5 | Organize PDF | `FEAT-PDF-010..016` | Workflow bundle; tidak membuat aturan/pemroses paralel | 3.5 |
| 6 | Scan to PDF | `FEAT-SCAN-014` | `FR-SCAN-001..002`; physical scanner PoC | 3.6 |
| 7 | Compress PDF | `FEAT-PDF-023` | Size-quality acceptance | 4.1 |
| 8 | Repair PDF | `FEAT-PDF-025 / 026` | No false repair success; advanced fallback | 4.2 |
| 9 | OCR PDF | `FEAT-OCR-004` | `FR-SCAN-001..002`; OCR PoC/fidelity | 4.3 |
| 10 | JPG to PDF | `FEAT-CONV-007` | `FR-PROC-001..005`; image order/dimension | 5.1 |
| 11 | Word to PDF | `FEAT-CONV-001` | `BR-GOV-006..007`; feasibility/font/layout | 5.2 |
| 12 | PowerPoint to PDF | `FEAT-CONV-003` | `BR-GOV-006..007`; feasibility/slide fidelity | 5.3 |
| 13 | Excel to PDF | `FEAT-CONV-002` | `BR-GOV-006..007`; feasibility/sheet/print area | 5.4 |
| 14 | HTML to PDF | `FEAT-CONV-017` | `BR-GOV-006..007`; remote/active-content gate | 5.5 |
| 15 | PDF to JPG | `FEAT-CONV-012` | Page/resolution/color outcome | 6.1 |
| 16 | PDF to Word | `FEAT-CONV-004` | `BR-GOV-006..007`; editability fidelity gate | 6.2 |
| 17 | PDF to PowerPoint | `FEAT-CONV-006` | `BR-GOV-006..007`; slide segmentation gate | 6.3 |
| 18 | PDF to Excel | `FEAT-CONV-005` | `BR-GOV-006..007`; table/cell fidelity gate | 6.4 |
| 19 | PDF to PDF/A | `FEAT-PDF-075` | PDF/A feasibility + independent conformance | 6.5 |
| 20 | Rotate PDF | `FEAT-PDF-012` | Page rotation normalization | 7.1 |
| 21 | Add page numbers | `FEAT-PDF-037` | Range/start/position | 7.2 |
| 22 | Add watermark | `FEAT-PDF-036` | Opacity/position/repeat; not redaction | 7.3 |
| 23 | Crop PDF | `FEAT-PDF-019` | Bounds/clipping; not redaction | 7.4 |
| 24 | Edit PDF | `FEAT-PDF-031..041` | Existing-text `FEASIBILITY_GATE`; subset exposure | 7.5 |
| 25 | PDF Forms | `FEAT-PDF-053..060` | Fill/create/field/flatten by build | 7.6 |
| 26 | Unlock PDF | `FEAT-PDF-062` | `SR-SEC-003`, `SR-SEC-022`; valid authority only | 8.1 |
| 27 | Protect PDF | `FEAT-PDF-061` | `SR-SEC-003`; password privacy | 8.2 |
| 28 | Sign PDF | `FEAT-SIGN-013` | `FR-SIGN-003`; `BR-SIGN-001`; `SR-SEC-011..012` | 8.3 |
| 29 | Redact PDF | `FEAT-PDF-069` | `SR-SEC-023`; real removal + confirmation | 8.4 |
| 30 | Compare PDF | `FEAT-PDF-071 / 072` | Visual/structure comparison limitation | 8.5 |
| 31 | AI Summarizer | `FEAT-INTEL-001` | `FR-INTEL-001`; `SR-SEC-021`; provider gate | 9.1 |
| 32 | Translate PDF | `FEAT-INTEL-002` | `FR-INTEL-002`; `SR-SEC-021`; fidelity/provider gate | 9.2 |
| 33 | PDF to Markdown | `FEAT-CONV-036` | `FR-INTEL-003`; deterministic first/OCR fallback | 9.3 |

Canonical IDs di atas tidak boleh diganti oleh label terjemahan, screenshot title, alias search, atau nama workflow. Organize/Edit/Forms tetap workflow card/bundle yang memakai capability canonical terkait, bukan Feature atau pemroses baru.

---

## 15. Risks/gaps

| ID | Risiko/gap | Dampak pada manual | Tindakan yang diperlukan |
|---|---|---|---|
| `D14-B-001` | Document 13 FINAL tidak memuat exact Feature × platform exposure per 33 tool | Availability final tetap tidak dapat diturunkan dari UAT posture umum | **BLOCKED_BY_DECISION: RELEASE_FEATURE_PLATFORM_EXPOSURE_MANIFEST** — integrator harus memasok registry-derived manifest + `CapabilityVersion` |
| `D14-B-002` | Exact release Feature × platform exposure belum tersedia | Status Web/Tablet/Phone dapat menjadi janji palsu | Freeze registry-derived manifest + `CapabilityVersion`; ganti seluruh `UNRESOLVED` sebelum publication |
| `D14-B-003` | Document 03 mencantumkan Desktop/Tablet/Phone exposure fields tetapi tidak memberi nilai final per record pada tabel | Tidak dapat mengisi status hanya dari schema field | Jangan infer `NOT EXPOSED`; gunakan actual registry/release export |
| `D14-B-004` | Web exposure tidak menjadi field eksplisit pada schema list Document 03 | Browser-safe surface per tool ambigu | Integrator menetapkan Web exposure/capability projection resmi |
| `D14-B-005` | Semua 33 Core masih `PLANNED`/sebagian `FEASIBILITY_GATE` pada baseline pre-coding | Manual dapat terlihat sebagai bukti implementasi | Beri label “tersedia bila muncul pada build”; publish hanya pages untuk released Feature |
| `D14-B-006` | Exact format extension/profile/preset/options belum dibekukan | Manual dapat mengarang `.doc`, PDF/A profile, quality preset, dll. | Generate supported-format/options table dari released descriptor; wording laporan sengaja “bila tersedia” |
| `D14-B-007` | Office/PDF/HTML/PDF-A/edit-text/Intelligence fidelity belum lulus PoC final | Hasil dapat jauh dari ekspektasi | Masukkan limitation yang terukur dari PoC/corpus ke halaman final; jangan klaim identik |
| `D14-B-008` | Scanner fisik Desktop tergantung device/driver/approval | Instruksi scan dapat gagal pada sebagian device | Pisahkan camera scan, import scan, dan physical scanner; publish hanya adapter supported |
| `D14-B-009` | Exact UI labels/menu/control belum tersedia | Steps dan screenshot tidak dapat menunjuk control final | Content review terhadap final UI copy; jangan membuat tombol yang belum ada |
| `D14-B-010` | Screenshot release belum tersedia | Screenshot plan belum dapat dieksekusi | Ambil setelah UI stabil dengan synthetic data; accessibility/security review sebelum publish |
| `D14-B-011` | Approved browser/device/OS matrix belum final | “Tersedia pada” terlalu luas | Tautkan halaman ke supported platform matrix per release |
| `D14-B-012` | Numeric upload/file/page/temp limits belum disetujui/bersifat policy | Angka manual cepat usang | Tampilkan effective value dari aplikasi/help runtime; jangan hard-code pada manual |
| `D14-B-013` | PDF/A profile dan institutional archive requirement belum ditetapkan | Pengguna dapat memilih conformance yang salah | Konfirmasi kebutuhan Records/Compliance dan approved validation before publication |
| `D14-B-014` | Password/permission terminology dapat disalahartikan sebagai DRM | Ekspektasi keamanan berlebihan | Pertahankan warning bahwa permission PDF bukan DRM absolut dan password tidak recoverable |
| `D14-B-015` | Visual Sign dapat disalahartikan sebagai digital certificate signature | Risiko proses/legal | Gunakan label non-PKI pada landing, step, result, warning, screenshot, dan Help |
| `D14-B-016` | AI/translation language, provider, retention, quality, capacity belum disetujui penuh | Privacy/accuracy promise salah | Publish only approved provider/language capability; selalu tampilkan review-to-source warning |
| `D14-B-017` | Result/secured retention copy dan versi keempat bergantung policy | User bingung antara temp result dan secured version | Sinkronkan wording dengan Document 06/retention policy final; jangan mengarang tanggal |
| `D14-B-018` | Localization glossary Indonesia/English belum final | Nama opsi/error tidak konsisten | Freeze glossary; Feature/Error/Job/Correlation IDs tidak diterjemahkan |

Tidak ada gap yang mengizinkan memilih library, membuat Feature ID/capability, mengubah scope, memindahkan local-only processing ke server, memakai public AI, atau mengklaim tool gated pasti tersedia.

---

## 16. Suggested Document 14 text

### 16.x Cara menggunakan manual

> PERTAMINA OTO menyediakan 33 Primary PDF Tool Cards dalam tujuh kelompok: Organize PDF, Optimize PDF, Convert to PDF, Convert from PDF, Edit PDF, PDF Security, dan PDF Intelligence. Pilih tool dari Home/Feature Search, masukkan file, periksa status, atur pilihan yang tersedia, preview, mulai, pantau progress, lalu periksa dan simpan hasil. File sumber tidak ditimpa otomatis.

> Ketersediaan tool berbeda menurut platform, build, capability perangkat, permission, policy, resource, dan koneksi. Desktop adalah client paling lengkap. Phone/Tablet menyediakan pengalaman terkurasi. Web hanya menampilkan tool yang resmi browser-safe/server-capable. Jika capability tidak tersedia, aplikasi menyembunyikan atau menjelaskan alternatif; tampilan antaraplikasi tidak harus identik.

### 16.x Koneksi dan file

> Tool local-capable berjalan tanpa upload pemrosesan ketika perangkat dan policy mengizinkan. Jika layanan internal diperlukan, PERTAMINA OTO memberi tahu sebelum content ditransfer. User tidak memilih cara pemrosesan teknis. **Amankan ke Akun** adalah tindakan online terpisah untuk file/result yang dipilih dan bukan auto-sync source, draft, undo, cache, thumbnail, temp, password, atau signature asset.

> Selalu periksa status file sebelum memulai. Extension saja tidak membuktikan format. PDF terlindungi memerlukan password yang sah. Password hanya digunakan selama operasi dan tidak disimpan sebagai recovery. File sementara atau output parsial bukan hasil final.

### 16.x Hasil dan progress

> Proses panjang menampilkan progress atau aktivitas tanpa persentase palsu. Background work dapat dilihat di Activity Center. `Accepted`, queued, waiting, atau retrying belum berarti selesai. Gunakan hasil hanya setelah status selesai, validation berhasil, preview diperiksa, dan Save As/download selesai.

> Gunakan Save As untuk membuat hasil baru. Overwrite, share, permanent delete, server transfer, final redaction, visual signature, Protect/Unlock, dan tindakan sensitif lain mengikuti confirmation/policy yang berlaku. Jika pekerjaan gagal, source tetap aman; ikuti guidance Lanjutkan, Coba Lagi, Batalkan, atau pilih target lain.

### 16.x Kualitas dan fidelity

> Compression dapat mengurangi kualitas. Repair tidak dapat memulihkan semua file. OCR dapat salah. Office/HTML/PDF conversion dapat mengubah font, layout, table, page break, slide, atau editability. PDF/A hanya dinyatakan berhasil setelah conformance validation. AI summary/translation dan Markdown extraction harus dibandingkan dengan sumber. Manual tidak menjanjikan hasil selalu identik.

### 16.x Security tools

> Unlock PDF hanya digunakan dengan password dan hak sah; bukan bypass. Protect PDF tidak menyimpan password untuk recovery dan permission PDF bukan DRM absolut. Sign PDF menggunakan visual signature/paraf non-PKI, bukan certificate signature. Crop/watermark tidak menghapus content sensitif. Redact PDF harus menghapus content pada final result, bukan hanya menutupnya dengan overlay; preview dan final confirmation wajib.

### 16.x Isi halaman 33 Core

> Gunakan isi Sections 3–9 laporan Agent B sebagai draft halaman tool, Section 10 sebagai halaman Merge PDF lengkap, Sections 11–12 sebagai warning/error guidance, dan Section 13 sebagai screenshot brief. Sebelum publication, integrator wajib mengganti semua `UNRESOLVED` berdasarkan release registry/capability manifest, memasukkan supported format/options yang sebenarnya, menghapus halaman Feature yang belum dirilis, serta memverifikasi label dan screenshot terhadap immutable release candidate.

### 16.x Gate publication

```text
[ ] Document 13 FINAL telah dibaca dan direkonsiliasi
[ ] exact 33-tool release/platform exposure manifest tersedia
[ ] seluruh UNRESOLVED diganti dengan status resmi
[ ] gated Feature hanya disebut tersedia setelah approved outcome
[ ] supported input/output/options berasal dari released descriptor
[ ] UI label dan steps cocok dengan release build
[ ] screenshot memakai synthetic/sanitized data dan lolos review
[ ] Indonesia/English glossary konsisten
[ ] tidak ada detail komponen/library/kontrak layanan pada user pages
[ ] Feature ID canonical tidak berubah
[ ] source safety, no silent upload, non-PKI, real-redaction, dan AI/provider limits tetap jelas
```
