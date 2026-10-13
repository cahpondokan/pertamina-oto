# 14 — USER MANUAL
## PERTAMINA OTO — Office Tools Organizer

> **Versi manual:** FINAL v1.0  
> **Tanggal:** 1 September 2026  
> **Status:** FINAL — PRE-IMPLEMENTATION USER MANUAL BASELINE  
> **Release readiness:** BELUM RELEASE_READY  
> **Aplikasi/RC:** `RC-TBD`  
> **Bahasa:** Bahasa Indonesia — `ID-1.0`  
> **Baseline:** 550 fitur canonical; 33 Primary PDF Tool Cards

---

# 1. Informasi Dokumen

Manual ini adalah panduan pra-implementasi. Struktur, langkah, warning, dan traceability sudah dikunci. Tombol, menu, ketersediaan platform, dan screenshot harus diperiksa kembali terhadap Release Candidate sebelum dipakai sebagai manual rilis.

Daftar isi mengikuti Bagian 1–57. Gunakan pencarian nama tool, alias pada Bagian 50, FAQ, atau matriks availability untuk menemukan tugas.

---

# 2. Tujuan Manual

Manual membantu Anda memahami aplikasi, menemukan tool, menjalankan pekerjaan, memantau proses, menjaga file, mengatasi masalah sederhana, memakai Assistant, bekerja offline, mengamankan hasil, dan melaporkan masalah.

---

# 3. Target Pembaca

Pembaca utama adalah pengguna biasa: pengguna baru/non-teknis, Desktop, Web, Phone/Tablet, online, atau offline. Instruksi administrasi, incident response, deployment, database, API, dan coding tidak dibahas.

---

# 4. Scope

Manual mencakup penggunaan umum, 33 Core, Merge detail, Assistant, platform, penyimpanan/sync, error, keamanan/privasi, FAQ, glossary, screenshot, availability, limitation, versioning, dan checklist.

Untuk administrasi lihat Dokumen 15. Untuk incident support lihat Dokumen 16.

---

# 5. Konvensi Penulisan

- Tombol/menu ditulis **tebal**.
- File contoh ditulis `contoh.pdf`.
- `TBD_UI_LABEL` berarti label belum dapat dibuktikan dari RC.
- **PENTING** muncul sebelum tindakan berisiko.
- **HASIL** menjelaskan outcome setelah langkah.
- Satu langkah berisi satu aksi.
- Screenshot placeholder seperti `[SCREENSHOT-HOME-001]` bukan screenshot aplikasi.

---

# 6. Status Fitur dan Capability

| Status | Arti |
|---|---|
| **Tersedia** | Terbukti tersedia pada exact release/platform |
| **Terbatas** | Hanya subset/use case tertentu tersedia |
| **Memerlukan koneksi** | Tindakan membutuhkan layanan online |
| **Memerlukan provider yang disetujui** | Hanya bekerja dengan provider local/internal yang approved |
| **Belum tersedia pada versi ini** | Belum dirilis atau belum dibuktikan oleh RC |

Semua fitur dalam baseline manual ini berstatus **Belum tersedia pada versi ini** sampai implementation, release exposure, dan UAT execution selesai. Panduan fitur menjelaskan perilaku yang diharapkan, bukan janji release.

---

# 7. Persyaratan Perangkat Dasar

Anda memerlukan akun Pertamina aktif, permission yang sesuai, file yang berhak diproses, ruang penyimpanan cukup, dan koneksi untuk capability online. Exact OS, browser, storage, limit, dan version menunggu release matrix.

---

# 8. Platform yang Didukung

| Platform | Posisi produk | Catatan pra-implementasi |
|---|---|---|
| Windows Desktop | Aplikasi paling lengkap | Exact released tools menunggu RC |
| Responsive Web | Account/secured/history/browser-safe/privileged channel | Tidak sama dengan Desktop |
| Phone | Curated companion | Scan, touch, share, simple tools bila dirilis |
| Tablet | Curated companion lebih kaya | Touch/stylus; dapat lebih lengkap dari Phone |

Tidak ada parity penuh. Feature yang tidak terlihat dapat dipengaruhi release, platform, perangkat, permission, policy, koneksi, atau capability.

---

# 9. Login

1. Buka PERTAMINA OTO.
2. Pilih **TBD_UI_LABEL: Login Pertamina** jika diminta.
3. Selesaikan autentikasi pada halaman Pertamina.
4. Kembali ke aplikasi.
5. Periksa akun aktif sebelum membuka file sensitif.

`[SCREENSHOT-LOGIN-001]`

---

# 10. Session

Aplikasi dapat meminta login kembali ketika session berakhir, akses berubah, akun dinonaktifkan, atau tindakan sensitif memerlukan verifikasi ulang.

1. Simpan pekerjaan yang dapat disimpan.
2. Pilih **TBD_UI_LABEL: Login Kembali**.
3. Selesaikan verifikasi.
4. Buka kembali pekerjaan jika recovery ditawarkan.

Logout bukan cara menyimpan. Sebelum meninggalkan perangkat bersama, periksa Activity Center, simpan pekerjaan, lalu logout.

---

# 11. Offline Access

Offline dapat digunakan jika Anda pernah login di perangkat, session offline masih valid, permission/policy mengizinkan, dan tool dapat bekerja lokal.

- Local-capable tool tetap bekerja.
- Server/account action menunjukkan **Memerlukan koneksi** atau **Menunggu koneksi**.
- File tidak auto-upload.
- Reauthentication tidak digantikan oleh session offline.
- Setelah reconnect, account/permission/policy/capability diperiksa ulang.

Jangan mengulang pekerjaan yang menunggu sebelum memeriksa Activity Center.

---

# 12. Quick Start — Mulai dalam 5 Menit

1. Buka aplikasi.
2. Login bila diminta.
3. Pilih file, tool, atau Assistant.
4. Periksa input dan pilihan.
5. Jalankan proses.
6. Pantau Activity Center jika proses lama.
7. Buka dan periksa hasil.
8. Pilih **Simpan sebagai File Baru**.
9. Pilih **Amankan ke Akun** hanya jika hasil perlu diakses lintas perangkat.

**HASIL —** File hasil dapat dibuka dan file asli tetap tersedia.

---

# 13. Tampilan Utama

Struktur konseptual: Home, tambah file, pencarian tool, kelompok tool, Assistant, Activity Center, Riwayat, Secured Files, Bantuan/Laporkan Masalah, dan Profil. Nama/posisi final memakai `TBD_UI_LABEL` sampai RC tersedia.

`[SCREENSHOT-HOME-001]`

---

# 14. Navigasi

- Gunakan pencarian untuk nama/alias tool.
- Buka Activity Center tanpa menghentikan background work.
- Baca warning sebelum meninggalkan unsaved working state.
- Pada layar kecil, menu dapat berada dalam menu tambahan.
- Control unavailable harus menjelaskan alasan/jalan berikutnya.

---

# 15. Triple Entry Flow

**File-First:** pilih file → periksa rekomendasi → pilih tool → proses.  
**Tool-First:** pilih/cari tool → masukkan file → proses.  
**Assistant-First:** tulis tujuan → periksa rencana → jalankan.

Pilih cara yang paling nyaman. Ketiganya memakai aturan, izin, konfirmasi, progress, dan hasil yang sama.

---

# 16. Memasukkan File

| Cara | Platform umum | Catatan |
|---|---|---|
| File Picker | Desktop/Web/Phone/Tablet | Sesuai permission perangkat |
| Drag-and-Drop | Desktop/Web bila didukung | Folder tidak selalu didukung |
| Paste file/screenshot | Platform yang mendukung | Input sementara, bukan otomatis tersimpan |
| Share/Open With | Phone/Tablet/OS integration | Bergantung release |
| Scan camera | Utama Phone/Tablet | Periksa crop/orientation |
| Recent Files | Platform yang menyediakan | File dapat perlu dipilih ulang |

Menambahkan file bukan selalu mengunggahnya. Aplikasi harus menjelaskan sebelum content dikirim ke server.

---

# 17. Validasi File

| Status | Tindakan Anda |
|---|---|
| Siap | Lanjutkan |
| Memerlukan password | Masukkan password sah |
| Peringatan | Baca dampak sebelum lanjut |
| Sedang diperiksa | Tunggu hasil validasi |
| Tidak didukung | Pilih format/tool lain |
| Rusak | Gunakan file sehat/cadangan |
| Diblokir | Jangan melewati pembatasan |
| Penyimpanan tidak cukup | Sediakan ruang/pilih lokasi lain |

Menghapus input dari daftar tidak menghapus file asli.

---

# 18. Working State

Working state adalah perubahan sementara sebelum hasil final disimpan. Preview menunjukkan perubahan yang direncanakan. File partial/temp bukan hasil final. Periksa selection, order, page, option, dan target sebelum finalisasi.

---

# 19. Autosave

Autosave menyimpan working state sementara jika capability mendukung. Autosave bukan Save As, bukan backup, dan bukan Secured File. Setelah recovery, periksa kembali input, urutan, option, dan target.

---

# 20. Undo/Redo

Gunakan **Urungkan** untuk membatalkan perubahan reversible dan **Ulangi** untuk mengembalikannya. Undo/Redo tidak berlaku setelah tindakan final/irreversible dan tidak sama dengan version history.

---

# 21. Save dan Save As

1. Periksa hasil.
2. Pilih **Simpan sebagai File Baru**.
3. Tentukan nama dan lokasi.
4. Periksa conflict target.
5. Simpan dan buka ulang hasil.

File asli tidak ditimpa otomatis. Overwrite hanya jika control resmi tersedia dan Anda memilihnya secara eksplisit. Amankan ke Akun berbeda dari Save/Save As.

---

# 22. Recovery

Jika aplikasi/proses berhenti:

1. Buka kembali aplikasi.
2. Periksa recovery candidate/Activity Center.
3. Pilih **Lanjutkan** bila checkpoint valid tersedia.
4. Pilih **Coba Lagi** setelah penyebab diperbaiki.
5. Pilih **Batalkan** untuk membuang pekerjaan sementara, bukan source.
6. Masukkan kembali password bila diperlukan.

Jangan gunakan file temp/setengah jadi.

---

# 23. Progress

| Status user-friendly | Arti |
|---|---|
| Menunggu | Sudah diterima, belum mulai |
| Sedang diproses | Pekerjaan berjalan |
| Menunggu koneksi | Membutuhkan jaringan |
| Menunggu resource | Menunggu ruang/capability |
| Mencoba kembali | Retry aman sedang berlangsung |
| Selesai | Hasil final tersedia |
| Gagal | Tidak ada hasil final valid |
| Dibatalkan | Pekerjaan dihentikan |

Persentase/ETA hanya ditampilkan bila akurat. No Silent Wait berlaku.

---

# 24. Activity Center

1. Buka **Activity Center**.
2. Pilih pekerjaan.
3. Baca status terbaru.
4. Pilih **Batalkan**, **Coba Lagi**, **Lanjutkan**, atau **Buka Hasil** hanya bila tersedia.
5. Jangan mengulang pekerjaan yang sama saat status belum jelas.

Realtime notification dapat terputus sementara; pekerjaan dapat tetap berjalan. Activity Center harus memulihkan status terbaru.

`[SCREENSHOT-ACTIVITY-001]`

---

# 25. History

Riwayat membantu menemukan aktivitas dan hasil sebelumnya. Riwayat bukan backup, Secured Files, atau Enterprise Audit. Jika file dipindah/dihapus, hasil mungkin tidak dapat dibuka. Menghapus entry Riwayat tidak otomatis menghapus file atau Audit.

---

# 26. Amankan ke Akun

**MEMERLUKAN KONEKSI.**

1. Selesaikan dan periksa hasil.
2. Pilih hasil yang benar.
3. Pilih **Amankan ke Akun**.
4. Baca notice dan konfirmasi.
5. Tunggu progress sampai berhasil.
6. Buka Secured Files.

Hanya hasil terpilih yang di-upload. Draft/source/cache/temp/checkpoint/password/clipboard/signature asset tidak ikut otomatis.

---

# 27. Secured Files

Login dengan akun yang sama, buka Secured Files, pilih file/version, lalu buka/download/restore bila tersedia dan berizin. Maksimum tiga versi terbaru berlaku. Versi keempat mengikuti retention policy. Delete lokal berbeda dari delete akun; permanent delete dapat meminta reauthentication dan tunduk legal hold.

---

# 28. Overview 33 Core Tools

| Kelompok | Tools |
|---|---|
| Organize PDF | Merge, Split, Remove, Extract, Organize, Scan |
| Optimize PDF | Compress, Repair, OCR |
| Convert to PDF | JPG, Word, PowerPoint, Excel, HTML to PDF |
| Convert from PDF | PDF to JPG, Word, PowerPoint, Excel, PDF/A |
| Edit PDF | Rotate, Page Numbers, Watermark, Crop, Edit, Forms |
| PDF Security | Unlock, Protect, Sign, Redact, Compare |
| PDF Intelligence | AI Summarizer, Translate, PDF to Markdown |

Current release status seluruh tool: **Belum tersedia pada versi ini**. Guide berikut adalah baseline untuk implementasi dan validasi RC.

# 29. Organize PDF

Semua guide berstatus **Belum tersedia pada versi ini** sampai RC. Desktop adalah target utama; exposure platform lain harus dibuktikan.

## Merge PDF — `FEAT-PDF-005`

Tujuan: gabungkan ≥2 PDF menjadi satu. Koneksi: tidak untuk baseline lokal. Sebelum mulai: siapkan file valid, password sah, order, storage. Langkah: tambah → validasi → reorder/remove → proses → pantau → Save As. Hasil: satu PDF baru; source utuh. Peringatan: Merge atomik; invalid required input tidak menghasilkan partial final. Masalah: kurang dari dua file, password, file rusak, storage, conflict. Lihat Bagian 36.

## Split PDF — `FEAT-PDF-006/007`

Tujuan: pecah satu PDF berdasarkan range/mode yang tersedia. Koneksi: tidak untuk local eligible. Langkah: pilih file → preview page count → pilih range → tinjau output → proses → buka hasil. Hasil: beberapa PDF sesuai range. Peringatan: range kosong/tumpang tindih harus diperiksa. Masalah: invalid range, password, target conflict.

## Remove pages — `FEAT-PDF-013`

Tujuan: hapus halaman dari working state. Langkah: pilih page → Remove → gunakan Undo bila salah → preview → Save As. Hasil: PDF baru tanpa page target; source tetap. Peringatan: keputusan isi tetap milik Anda. Masalah: semua page terpilih atau selection salah.

## Extract pages — `FEAT-PDF-014`

Tujuan: salin selected pages ke hasil baru. Langkah: pilih file/page/range/order → preview → proses → Save As. Hasil: output hanya berisi selection. Peringatan: Extract tidak menghapus page pada source. Masalah: invalid range atau unsupported structure.

## Organize PDF — `FEAT-PDF-010..016`

Tujuan: susun page dalam workspace. Langkah: tambah file → reorder/move/rotate/remove/duplicate/add blank bila tersedia → Undo/Redo → preview → finalize → Save As. Hasil: PDF baru mengikuti working state. Peringatan: subset berbeda per platform/build. Masalah: selection, temp storage, unsupported operation.

## Scan to PDF — `FEAT-SCAN-014`

Tujuan: buat PDF dari camera/import scan. Target: Phone/Tablet; Desktop import; physical scanner gated. Langkah: capture/import → retake → crop/perspective → reorder → preview → save. Hasil: PDF terbaca. Peringatan: scan tidak auto-upload; kualitas memengaruhi OCR. Masalah: permission camera, blur, edge, missing page.

---

# 30. Optimize PDF

## Compress PDF — `FEAT-PDF-023`

Tujuan: kurangi ukuran dengan trade-off kualitas. Langkah: pilih PDF → pilih preset bila tersedia → preview/estimate → process → bandingkan → Save As. Hasil: umumnya lebih kecil. Peringatan: gambar/scan dapat kurang tajam; tidak ada jaminan target size. Masalah: size tidak turun atau quality terlalu rendah.

## Repair PDF — `FEAT-PDF-025/026`

Tujuan: mencoba membuat PDF rusak dapat dibuka. Langkah: pilih salinan → baca status → repair → validasi/open → Save As jika berhasil. Hasil hanya tersedia bila valid. Peringatan: tidak semua file dapat dipulihkan. Masalah: damage permanen atau fallback tidak tersedia.

## OCR PDF — `FEAT-OCR-004`

Tujuan: kenali teks pada scan. Langkah: pilih file/page/language → rotate/correct scan → OCR → search/copy known terms → save. Hasil: text searchable/copyable sejauh dikenali. Peringatan: scan buruk, table, multi-column, stamp, handwriting dapat salah. Masalah: wrong language/orientation atau low quality.

---

# 31. Convert to PDF

## JPG to PDF — `FEAT-CONV-007`

Pilih images → atur order/orientation/page fit → preview → convert → Save As. Hasil: satu PDF berurutan. Peringatan: periksa crop/rotation. Masalah: image unsupported atau order salah.

## Word to PDF — `FEAT-CONV-001`

Pilih DOCX → periksa limitation/font/layout → pilih option bila tersedia → convert → preview → Save As. Hasil: PDF usable. Peringatan: font/layout/page break/object dapat berubah. Memerlukan approved engine; koneksi dapat kondisional.

## PowerPoint to PDF — `FEAT-CONV-003`

Pilih PPTX/slide scope → convert → periksa slide order/visual → Save As. Peringatan: animation/media tidak selalu dipertahankan; fidelity gated.

## Excel to PDF — `FEAT-CONV-002`

Pilih XLSX/sheet/print area/orientation → preview pagination → convert → Save As. Peringatan: page break, chart, formula display dapat berubah; periksa semua sheet.

## HTML to PDF — `FEAT-CONV-017`

Pilih approved HTML source → review external/active-content limitation → convert → preview → save. Peringatan: jangan masukkan arbitrary public URL; script/remote resource tidak dijalankan bebas. Memerlukan approved engine/provider path.

---

# 32. Convert from PDF

## PDF to JPG — `FEAT-CONV-012`

Pilih PDF/page/resolution → convert → periksa count/order/image → save. Peringatan: resolution/quality memengaruhi size.

## PDF to Word — `FEAT-CONV-004`

Pilih PDF → periksa text/scan status → convert → buka/edit sample → save. Peringatan: editable output bukan reconstruction original; layout/font/table perlu koreksi.

## PDF to PowerPoint — `FEAT-CONV-006`

Pilih PDF/page scope → convert → periksa slide order/editability → save. Peringatan: segmentation/layout dapat berubah.

## PDF to Excel — `FEAT-CONV-005`

Pilih PDF/page/table → convert → periksa cells/headers/values → save. Peringatan: formula asli tidak dipulihkan otomatis; koreksi table manual mungkin perlu.

## PDF to PDF/A — `FEAT-PDF-075`

Pilih PDF/profile yang dirilis → convert → tunggu conformance validation → save. Hasil hanya disebut PDF/A jika validation berhasil. Peringatan: profile/archival need harus sesuai kebijakan.

---

# 33. Edit PDF

## Rotate PDF — `FEAT-PDF-012`

Pilih page → rotate → preview → Undo bila perlu → Save As. Hasil: hanya selected pages berubah.

## Add page numbers — `FEAT-PDF-037`

Pilih range/start/position/style bila tersedia → preview sample/all → process → save. Peringatan: logical document number dapat berbeda dari PDF index.

## Add watermark — `FEAT-PDF-036`

Pilih text/image, page, position, opacity → preview → save. Peringatan: watermark bukan redaction/DRM; content di bawahnya dapat tetap diekstrak.

## Crop PDF — `FEAT-PDF-019`

Pilih page/bounds/apply scope → preview → Undo/koreksi → save. Peringatan: crop bukan redaction; content dapat tetap ada dalam struktur.

## Edit PDF — `FEAT-PDF-031..041`

Pilih page → add/edit supported object → position/style → Undo/Redo → preview/change summary → Save As. Peringatan: Add Text berbeda dari Edit Existing Text; existing-content editing gated dan tidak selalu mampu mempertahankan layout.

## PDF Forms — `FEAT-PDF-053..060`

Pilih Fill/Create mode bila tersedia → isi/tambah field → validate → preview → flatten hanya jika perlu → Save As → reopen. Peringatan: flatten dapat menghilangkan editability; active/dynamic form tidak selalu didukung.

---

# 34. PDF Security

## Unlock PDF — `FEAT-PDF-062`

**PENTING —** Gunakan hanya jika Anda mempunyai password dan hak sah. Pilih PDF → masukkan password → baca authority notice → process → reopen → Save As. Source tetap terlindungi. Tool bukan bypass/crack dan tidak memulihkan password.

## Protect PDF — `FEAT-PDF-061`

**PENTING —** Simpan password dengan aman; aplikasi tidak menyimpannya sebagai recovery. Pilih PDF → password/confirmation → permission bila tersedia → review → process → test reopen → Save As. PDF permission bukan DRM absolut.

## Sign PDF — `FEAT-SIGN-013`

**PENTING —** Ini tanda tangan/paraf visual non-PKI, bukan certificate/digital signature. Pilih/create synthetic/authorized mark → preview → page/place/resize → confirm → Save As. Asset reuse opt-in lokal; tidak auto-sync.

## Redact PDF — `FEAT-PDF-069`

**PENTING —** Redaksi final dapat permanen. Pilih target → preview semua page/layer → final confirmation → process → reopen/search/copy check → Save As. Kotak/overlay bukan redaction aman. Jangan gunakan hasil jika content masih ditemukan.

## Compare PDF — `FEAT-PDF-071/072`

Pilih PDF A/B → page/alignment/mode bila tersedia → compare → review difference/provenance. Peringatan: scan noise/reflow/font dapat membuat banyak difference; Compare bukan bukti legal/semantic equivalence.

---

# 35. PDF Intelligence

## AI Summarizer — `FEAT-INTEL-001`

Status tambahan: **Memerlukan provider yang disetujui** dan dapat **Memerlukan koneksi**. Pilih PDF/scope → baca notice → process → bandingkan summary dengan source. **PENTING —** Ringkasan dapat salah/melewatkan konteks; periksa angka, nama, tanggal, syarat, dan keputusan.

## Translate PDF — `FEAT-INTEL-002`

Status tambahan: **Memerlukan provider yang disetujui**. Pilih source/target language/scope → process → preview → periksa technical/legal terms, number, table, layout → export. Hasil tidak selalu identik; review manusia diperlukan.

## PDF to Markdown — `FEAT-CONV-036`

Pilih PDF/page → OCR bila scan → convert → preview heading/list/table/reading order → koreksi → Save As `.md`. Peringatan: complex layout, equations, tables, footnotes dapat perlu koreksi; AI bukan dependency default.

Public AI/translation endpoint tidak digunakan.

---

# 36. Merge PDF — Panduan Lengkap

Status: **Belum tersedia pada versi ini**. Target baseline: Desktop full; simple curated exposure lain menunggu RC. Koneksi: tidak untuk local Merge; Amankan ke Akun memerlukan koneksi.

## Sebelum memulai

1. Siapkan minimal dua PDF valid.
2. Siapkan password sah bila diperlukan.
3. Tentukan urutan.
4. Pastikan storage cukup.
5. Gunakan source yang berhak Anda proses.

## Langkah

1. Buka **Merge PDF** melalui File, Tool, atau Assistant.
2. Tambahkan minimal dua PDF.
3. Periksa status setiap file.
4. Masukkan password sah jika diminta.
5. Ubah urutan file.
6. Hapus input yang salah; gunakan Undo bila tersedia.
7. Pilih nama/lokasi hasil.
8. Pilih **TBD_UI_LABEL: Gabungkan**.
9. Pantau progress/Activity Center.
10. Pilih Batalkan hanya bila tersedia dan aman.
11. Periksa page count/order/result.
12. Pilih **Simpan sebagai File Baru**.
13. Buka ulang hasil.
14. Periksa Riwayat.
15. Pilih Amankan ke Akun hanya jika perlu lintas perangkat.

## Error dan recovery

- Kurang dari dua file: tambah input.
- File rusak/unsupported: ganti/hapus; jangan gunakan partial result.
- Password salah: gunakan password sah; jangan menebak terus.
- Storage penuh: pilih lokasi lain/sediakan ruang.
- Cancel: tunggu status Dibatalkan; klik bukan berarti langsung berhenti.
- App/process berhenti: buka ulang → Lanjutkan/Coba Lagi/Batalkan sesuai recovery.
- Offline valid: Merge tetap lokal; Amankan ke Akun menunggu online.

**HASIL —** Satu PDF final sesuai order; source tidak berubah; no server processing upload.

```text
[ ] minimal dua input valid
[ ] order/page count benar
[ ] result dapat dibuka ulang
[ ] source/old target aman
[ ] success hanya setelah validation/Save As
[ ] Activity/History benar
[ ] no processing upload untuk baseline Merge lokal
```

Screenshot: `[SCREENSHOT-MERGE-001..007]`.

---

# 37. Assistant

1. Buka **Assistant** (`TBD_UI_LABEL` location).
2. Tulis tujuan, file, order, dan option penting.
3. Periksa Plan Preview.
4. Koreksi file/step/parameter/output/koneksi.
5. Pilih **TBD_UI_LABEL: Jalankan**.
6. Baca confirmation untuk overwrite/delete/share/upload/high-risk action.
7. Pantau Activity Center.
8. Buka dan periksa actual result.

Contoh: “Gabungkan tiga PDF ini, lalu kompres hasilnya.”

Assistant hanya memakai tool resmi dan permission Anda. Plan bukan hasil. Chat bukan confirmation. Document content diperlakukan sebagai data, bukan instruction. Jangan masukkan password/token/secret. AI dapat salah dan tidak memakai public endpoint.

---

# 38. Panduan Desktop

Desktop adalah target pekerjaan paling lengkap: multi-file, drag/drop, keyboard/mouse, batch, large file, preview/workspace kompleks, background local work. Periksa order/preview; pantau Activity; Save As; gunakan Amankan hanya untuk selected result. Exact release availability menunggu RC.

---

# 39. Panduan Web

Web ditujukan untuk account, History, Secured Files, browser-safe tools, Issue, serta Admin/Audit/Support bila permission. Tidak semua Desktop tools tersedia. Web cached page bukan offline authority. Upload memerlukan exact file, purpose, notice, confirmation, progress, dan final result.

---

# 40. Panduan Phone

Phone adalah curated companion: scan, read, crop/rotate, simple Merge/Compress, light OCR, visual signature, share, Activity, Amankan, Issue—hanya bila dirilis. Gunakan touch; periksa portrait/landscape/safe area; jangan harapkan large batch/multi-panel/full Desktop parity.

---

# 41. Panduan Tablet

Tablet dapat lebih kaya dari Phone: large preview, touch/stylus, scan, annotation, signature, curated edit. Periksa orientation dan state. Stylus bukan satu-satunya cara. Exact capability mengikuti device/release.

---

# 42. Issue Reporting

1. Pilih **Bantuan**.
2. Pilih **Laporkan Masalah**.
3. Tulis pesan singkat.
4. Tambahkan screenshot hanya bila perlu.
5. Gunakan markup bila tersedia.
6. Periksa preview.
7. Pilih **Kirim**.
8. Lihat status bila tersedia.

Screenshot tidak auto-upload. Safe diagnostics ditambahkan otomatis. Jangan masukkan password/token/secret/document content yang tidak perlu. Anda tidak perlu mengisi form teknis panjang.

---

# 43. Pesan Error

| Pesan/gejala | Tindakan aman |
|---|---|
| File tidak didukung | Pilih format/tool lain; jangan ubah extension untuk memaksa |
| File rusak | Gunakan backup/file sehat atau Repair pada salinan |
| Password salah | Gunakan password sah; jangan kirim ke Issue/Assistant |
| Storage tidak cukup | Pilih lokasi lain/sediakan ruang |
| Koneksi putus | Periksa Activity; reconnect; jangan duplicate action |
| Layanan tidak tersedia | Simpan draft; coba nanti; no public fallback |
| Permission denied | Periksa akun; jangan bypass |
| Result validation failed | Jangan gunakan file; retry/lapor sesuai guidance |
| Save conflict | Pilih nama/lokasi baru atau explicit overwrite |
| Result expired | Rerun/reprocess secara explicit |

---

# 44. Troubleshooting

1. Baca pesan dan next action.
2. Pastikan source aman.
3. Perbaiki input, koneksi, permission, atau storage.
4. Periksa Activity Center.
5. Pilih Coba Lagi hanya jika tersedia.
6. Buka/review hasil sebelum digunakan.
7. Laporkan Masalah jika berulang.

Jangan edit temp folder, direct DB, security control, atau file internal aplikasi.

---

# 45. Panduan Keamanan Pengguna

- Proses hanya file yang berhak Anda gunakan.
- Periksa file sebelum upload/share.
- Jangan bagikan password.
- Periksa redaction final dengan search/copy.
- Logout dari perangkat bersama.
- Periksa nama/lokasi/akun tujuan.
- Jangan bypass permission/policy.
- Laporkan upload tak dipilih, wrong-account file, atau false success.

---

# 46. Panduan Privasi

Minimalkan content yang diproses/dibagikan. Periksa screenshot dan preview. Jangan masukkan secret/password/clipboard/signature nyata ke Assistant/Issue. File tidak auto-upload; server transfer harus dijelaskan. Signature asset, draft, cache, dan full OCR tidak auto-sync. Gunakan kanal resmi.

---

# 47. Localization

Bahasa utama/fallback adalah Indonesia; English memakai sumber/struktur setara. UI label manual harus cocok exact locale/build. Feature/Requirement/UAT/Job/Correlation/Error IDs, filename/user content, PERTAMINA OTO, PDF/PDF-A/OCR/JPG/HTML/Markdown tidak diterjemahkan.

---

# 48. Accessibility dan Basic Usability

Manual memakai heading berurutan, numbered steps, plain language, meaningful links, readable tables, text alternative, dan caption. Informasi tidak hanya color/icon/screenshot. Screenshot harus punya alt text. Ini bukan klaim sertifikasi accessibility.

---

# 49. FAQ

**Mengapa fitur tidak muncul?** Release/platform/device/permission/policy/koneksi dapat membatasi. Jangan paksa direct route; laporkan bila seharusnya tersedia.

**Apakah file dikirim ke server?** Tidak selalu. Local-capable process tetap lokal; server transfer memerlukan notice/approval.

**Bagaimana bekerja offline?** Gunakan local eligible tool selama session valid; online action menunggu koneksi.

**Bagaimana membatalkan?** Buka progress/Activity dan pilih Batalkan bila tersedia; tunggu terminal status.

**Apa itu Amankan ke Akun?** Explicit selected-result transfer, bukan full workspace sync.

**Mengapa tiga versi?** Baseline menyimpan maksimal tiga versi; versi keempat mengikuti retention policy.

**Apakah Sign PDF merupakan digital certificate signature?** Tidak. Hanya visual signature/paraf non-PKI.

**Apakah AI selalu benar?** Tidak. Bandingkan dengan source; provider harus approved local/internal.

**Bagaimana melapor?** Gunakan Laporkan Masalah; pesan singkat; screenshot optional/previewed.

---

# 50. Glossary

| Istilah | Arti |
|---|---|
| Activity Center | Tempat memantau pekerjaan/status/action |
| Assistant | Jalur percakapan untuk tool resmi; no elevation |
| Working State | Draft/edit aktif sebelum final |
| Autosave | Simpan temporary working state |
| Preview | Tinjau perubahan sebelum final |
| History | Riwayat pribadi; bukan backup/Audit |
| Amankan ke Akun | Explicit selected-result sync |
| Secured File | Hasil yang diamankan pada account scope |
| OCR | Pengenalan teks dari image/scan |
| PDF/A | PDF untuk archival/conformance yang tervalidasi |
| Visual Signature | Mark visual non-PKI |
| Redaction | Permanent removal pada final result |
| Job ID/Correlation ID | Identifier support, bukan credential |
| Retry/Resume | Coba kembali/Lanjutkan pada safe state |

---

# 51. Screenshot Register

Tidak ada screenshot aktual. Gunakan placeholder:

```text
[SCREENSHOT-LOGIN-001]
[SCREENSHOT-HOME-001]
[SCREENSHOT-MERGE-001..007]
[SCREENSHOT-ACTIVITY-001]
[SCREENSHOT-SECURED-001]
[SCREENSHOT-ISSUE-001]
```

Setiap placeholder dijelaskan pada `USER_MANUAL_SCREENSHOT_REGISTER.md`. Screenshot final hanya dari exact RC, dummy data, approved status, alt text/caption, dan privacy/security review. Mockup tidak disebut screenshot.

---

# 52. Feature Availability Matrix

| No | Tool | Current status | Target posture | Koneksi/provider |
|---:|---|---|---|---|
| 1 | Merge PDF | Belum tersedia | Desktop utama; others TBC | Local baseline |
| 2 | Split PDF | Belum tersedia | Desktop utama; others TBC | Local eligible |
| 3 | Remove pages | Belum tersedia | Desktop utama; others TBC | Local eligible |
| 4 | Extract pages | Belum tersedia | Desktop utama; others TBC | Local eligible |
| 5 | Organize PDF | Belum tersedia | Desktop utama; curated TBC | Local eligible |
| 6 | Scan to PDF | Belum tersedia | Phone/Tablet; Desktop import | Device/conditional |
| 7 | Compress PDF | Belum tersedia | Desktop; curated TBC | Local eligible |
| 8 | Repair PDF | Belum tersedia | Desktop | Conditional advanced |
| 9 | OCR PDF | Belum tersedia | Desktop; light curated TBC | Approved engine |
| 10 | JPG to PDF | Belum tersedia | Desktop; others TBC | Local eligible |
| 11 | Word to PDF | Belum tersedia | Desktop | Approved engine/conditional |
| 12 | PowerPoint to PDF | Belum tersedia | Desktop | Approved engine/conditional |
| 13 | Excel to PDF | Belum tersedia | Desktop | Approved engine/conditional |
| 14 | HTML to PDF | Belum tersedia | TBC | Approved engine/conditional |
| 15 | PDF to JPG | Belum tersedia | Desktop; others TBC | Local eligible |
| 16 | PDF to Word | Belum tersedia | Desktop | Approved engine/conditional |
| 17 | PDF to PowerPoint | Belum tersedia | Desktop | Approved engine/conditional |
| 18 | PDF to Excel | Belum tersedia | Desktop | Approved engine/conditional |
| 19 | PDF to PDF/A | Belum tersedia | Desktop | Approved validator |
| 20 | Rotate PDF | Belum tersedia | Desktop; curated TBC | Local eligible |
| 21 | Add page numbers | Belum tersedia | Desktop; others TBC | Local eligible |
| 22 | Add watermark | Belum tersedia | Desktop; others TBC | Local eligible |
| 23 | Crop PDF | Belum tersedia | Desktop; curated TBC | Local eligible |
| 24 | Edit PDF | Belum tersedia | Desktop; curated subset TBC | Gated/conditional |
| 25 | PDF Forms | Belum tersedia | Desktop; others TBC | Local eligible |
| 26 | Unlock PDF | Belum tersedia | Desktop; others TBC | Local eligible/reauth conditional |
| 27 | Protect PDF | Belum tersedia | Desktop; others TBC | Local eligible/reauth conditional |
| 28 | Sign PDF | Belum tersedia | Desktop/Phone/Tablet target | Local eligible |
| 29 | Redact PDF | Belum tersedia | Desktop; others TBC | Local eligible/reauth conditional |
| 30 | Compare PDF | Belum tersedia | Desktop | Local eligible |
| 31 | AI Summarizer | Belum tersedia | TBC | Approved provider; conditional connection |
| 32 | Translate PDF | Belum tersedia | TBC | Approved provider; conditional connection |
| 33 | PDF to Markdown | Belum tersedia | Desktop | Local deterministic/conditional OCR |

Matrix ini tidak mengotorisasi release. Ganti current status/target hanya dari registry/capability manifest + RC evidence.

---

# 53. Known Limitations

- Actual UI/menu/control/screenshots belum tersedia.
- Exact release/platform/browser/device matrix belum tersedia.
- Supported format/options/limits belum final.
- Office/HTML/PDF-A/edit/OCR/AI fidelity masih gated.
- Physical scanner support masih capability/approval-dependent.
- Exact SSO/offline duration/retention wording mengikuti policy.
- UAT specification final, tetapi UAT execution belum diklaim.
- Publication format, contact route, owner, accessibility target, dan screenshot retention belum dikonfirmasi.

Limitation tidak mengizinkan false claim, public AI, server-forced local tool, full workspace sync, PKI claim, atau security reduction.

---

# 54. Support dan Contact Route

Gunakan **Bantuan › Laporkan Masalah** (`TBD_UI_LABEL`) sebagai jalur utama. External email/phone/URL belum ditetapkan dan tidak dibuat oleh manual. Sertakan Job ID/Correlation ID hanya jika aplikasi menampilkannya; jangan kirim secret/content yang tidak perlu.

Untuk admin lihat Dokumen 15. Untuk incident support lihat Dokumen 16.

---

# 55. Versioning Manual

```text
ManualVersion: 1.0
Status: PRE_IMPLEMENTATION_BASELINE
AppliesToAppVersion: RC-TBD
Platforms: release matrix TBC
Locales: ID source; EN pending validation
ValidatedBuild: none
ContentOwner/Reviewer: TBC
```

Guide lifecycle: `DRAFT_CONTENT → WAITING_FOR_UI → WAITING_FOR_SCREENSHOT → UAT_VALIDATED → RELEASE_READY`.

Feature/UI/platform/error/sync/Assistant/security/localization change memicu documentation impact review. Screenshot stale menjadi OUTDATED dan diganti dari RC baru.

---

# 56. Requirement dan Feature Traceability

| Area | Requirement/source | Manual |
|---|---|---|
| Triple Entry | `DEC-REQ-024..025`, `FR-ASSIST-001..014` | Bagian 15, 37 |
| File/validation | `FR-FILE-001..015` | Bagian 16–17 |
| Processing/offline | `FR-PROC-001..005`, `UIR-UI-006..010` | 11, 18–24 |
| Storage/recovery | `FR-STOR-001..006`, `FR-REC-001..006` | 18–27, 44 |
| History | `FR-HIST-001..003` | 25 |
| Secured files | `FR-BACKUP-001..010` | 26–27 |
| Sign | `FR-SIGN-001..003`, `BR-SIGN-001` | 34 |
| Intelligence | `FR-INTEL-001..003`, `SR-SEC-021` | 35 |
| Issue | `FEAT-SUPPORT-001..008` | 42, 54 |
| 33 Core | Document 03/13 | 28–36, 52 |

Feature mapping 33/33 berada pada Bagian 29–35 dan 52. Baseline tetap 550; tidak ada Feature ID baru.

---

# 57. User Acceptance Checklist

## Baseline documentation

```text
[x] 57/57 topik tercakup
[x] 33/33 Core guide + Feature ID
[x] Merge detail/offline/error/source protection
[x] Assistant/platform/sync/Issue/security/privacy
[x] FAQ/glossary/screenshot/availability/traceability
[x] no false screenshot/availability/PKI/AI/fidelity claim
[x] Bahasa Indonesia sederhana
[x] boundary Admin/Incident dijaga
```

## Release validation gate

```text
[ ] exact RC/version/build/digest recorded
[ ] Feature × platform exposure resolved
[ ] all TBD_UI_LABEL replaced with actual RC labels
[ ] actual screenshots APPROVED, safe, captioned, with alt text
[ ] all 33 Core Tools verified on their released platforms
[ ] all released guide steps task-tested without verbal help
[ ] UAT execution PASS and known issues reconciled
[ ] links/search/TOC/offline help validated
[ ] localization/accessibility/security reviews pass
[ ] owner/maintenance/contact/publication path confirmed
[ ] no documentation Blocker/Critical
[ ] status changed to RELEASE_READY only after all above
```

Gate ini dikunci oleh User melalui `MAN-USER-001`. Kegagalan satu butir mempertahankan status **BELUM RELEASE_READY**.

```text
Dokumen 14 baseline : FINAL v1.0
Release-ready       : BELUM
Core guides         : 33 / 33
Screenshot actual   : 0
Screenshot planned  : 33 Core + common set
Next document       : 15_ADMIN_MANUAL.md
```
