# AGENT A — BUSINESS PROCESS & CORE FEATURE UAT REPORT

> **Task ID:** `DOC13-AGENT-A`  
> **Peran:** Senior System Analyst, Business Process Analyst, Senior UAT Analyst, Senior Application Architect  
> **Status:** Analisis dan rekomendasi untuk Document 13; belum merupakan hasil/sign-off UAT  
> **Baseline:** Documents 01–12 FINAL dan Master Project Summary v3.5  
> **Ruang lingkup utama:** Triple Entry Flow, 33 Primary PDF Tool Cards, Merge PDF, dan tindakan berisiko tinggi  
> **Batas:** Tidak mengubah Feature/Requirement, tidak menulis Unit/API test, tidak coding, dan tidak mengedit master Document 13

Dokumen ini mendefinisikan penerimaan dari sudut pandang pekerjaan nyata pengguna. Pertanyaan utamanya selalu:

> **Apakah pengguna yang berwenang dapat menyelesaikan pekerjaannya dengan hasil yang benar, dapat dipahami, aman, dan dapat digunakan?**

Build hijau, status teknis sukses, record tersimpan, atau response transport berhasil tidak cukup untuk meluluskan UAT bila hasil bisnis tidak tercapai.

---

# 1. Business UAT strategy

## 1.1 Tujuan UAT

UAT PERTAMINA OTO membuktikan bahwa release candidate:

1. menyatukan pekerjaan PDF dalam workflow yang mudah dipahami;
2. memungkinkan user mencapai hasil bisnis yang sama melalui File-First, Tool-First, atau Assistant-First yang sesuai;
3. melindungi file sumber dan tidak menganggap preview/draft/temp sebagai hasil final;
4. tetap berguna secara lokal ketika offline untuk capability yang memang eligible;
5. memberi progress, cancel, error, dan recovery yang dapat ditindaklanjuti;
6. tidak meminta user memilih engine/provider atau memahami arsitektur teknis;
7. menunjukkan batas capability/format/platform secara jujur;
8. menjaga aksi sensitif di bawah permission, confirmation, reauthentication, ownership, dan policy yang berlaku;
9. menghasilkan file/output yang dapat dibuka, dibaca, diedit, dibagikan, atau digunakan kembali sesuai tujuan tool;
10. menyediakan History dan evidence bisnis yang tepat tanpa mengekspos konten sensitif.

## 1.2 Batas UAT

UAT menguji public workflow, business outcome, kejelasan UX, kesesuaian platform, dan penerimaan user. UAT tidak mengulang:

- 50 Unit cases Merge pada Document 11;
- 36 system scenarios Merge pada Document 12 secara teknis;
- detail HTTP, database, queue, IPC, thread, class, atau storage internals;
- load/stress/malicious corpus/VA/PT;
- pemilihan vendor/engine/provider;
- Production smoke atau Production approval.

Technical, Security, Performance, Recovery, Compatibility, dan Regression gates dari Document 12 harus sudah lulus sebelum UAT. UAT tidak boleh digunakan untuk menutupi technical gate yang gagal.

## 1.3 Model kedalaman berbasis risiko

| Kedalaman | Berlaku untuk | Minimum UAT |
|---|---|---|
| `D1 — Standard` | Core Low-risk yang memakai family rule stabil | Satu primary workflow, satu distinctive outcome, UX utama, hasil dapat dibuka, source tetap aman |
| `D2 — Enhanced` | Medium-risk, optional-server, PoC/fidelity, device/platform dependency | Primary + alternate/limitation + error/recovery + honest capability message |
| `D3 — Critical` | High-risk atau tindakan irreversible/content-sensitive | Primary + denial/cancel + confirmation/reauth bila berlaku + source/result verification + recovery + explicit business sign-off |

Protect, Unlock, Redact, Visual Sign, Amankan ke Akun, Permanent Delete, AI Summarizer, dan Translate PDF menggunakan sekurang-kurangnya `D2`; Unlock, Redact, dan Permanent Delete menggunakan `D3`.

## 1.4 Prasyarat umum UAT

UAT baru dimulai setelah seluruh entry package Document 12 tersedia:

```text
immutable release candidate/version/digest
controlled UAT lane dan konfigurasi yang diketahui
deployment/configuration/migration/health smoke PASS
applicable technical/security/regression/performance/recovery/platform gates PASS
no open BLOCKER/CRITICAL
known issues, limitation, workaround, owner, dan disposition tersedia
UAT accounts/permissions/platforms/data siap
synthetic/sanitized approved data tersedia
Feature/platform release scope dan CapabilityVersion dibekukan
user guidance, support/escalation, reset, defect, dan retest path siap
```

`BLOCKED`, `NOT_RUN`, atau `INCONCLUSIVE` bukan `PASS`.

## 1.5 Konvensi skenario

| Item | Aturan |
|---|---|
| UAT ID | `UAT-<AREA>-<NNN>`; tidak menggantikan Feature ID/Requirement ID |
| Steps | Bahasa user; tidak menyebut controller, table, method, atau internal component |
| Expected Result | Hasil pekerjaan yang dapat dilihat/digunakan user |
| Critical UX | Kejelasan, feedback, langkah, confirmation, platform-natural interaction |
| Acceptance | Kondisi `PASS` yang menjawab business goal dan forbidden outcome |
| Evidence | Catatan hasil, output sintetis, screenshot tersanitasi bila perlu, build/platform/data ID |

## 1.6 Data UAT

- Default memakai dokumen sintetis dengan isi yang mudah diverifikasi oleh business user.
- Sanitized representative document hanya digunakan dengan approval dan provenance yang jelas.
- Dataset tidak memuat identitas pegawai nyata, secret, token, tanda tangan nyata, atau dokumen Production.
- Untuk fidelity, corpus memiliki jawaban pembanding: urutan halaman, teks sumber, istilah yang harus ada, layout utama, area redact, dan expected output property.
- Output UAT dapat memuat isi sintetis, tetapi akses dan retention evidence tetap dibatasi.
- Password dan visual-signature asset UAT bersifat sintetis serta tidak digunakan ulang di luar lane.

---

# 2. UAT actor mapping

## 2.1 Actor aplikasi dan peserta UAT

| Actor/peserta | Tujuan bisnis | Scope UAT | Batas yang harus dibuktikan |
|---|---|---|---|
| **User — pekerja Desktop** | Mengolah dokumen kantor, file besar, batch, dan workflow kompleks | Mayoritas 33 Core, File/Tool/Assistant entry, offline/local, History | Tidak dapat mengubah enterprise policy atau membuka resource user lain |
| **User — pekerja Mobile/Tablet** | Scan, crop/rotate, OCR ringan, visual sign, merge/compress sederhana, share, secured access | Hanya Feature yang resmi exposed pada device aktual | Curated capability; bukan salinan Desktop; unavailable action harus jelas |
| **Administrator** | Mengelola account/configuration/policy/feature/limit sesuai permission | Prasyarat/visibility policy dan negative boundary terkait UAT | Tidak otomatis boleh membuka isi dokumen, hasil, OCR, atau tanda tangan user |
| **Auditor** | Memeriksa evidence aktivitas/kepatuhan secara read-only | Supporting acceptance untuk audit-visible high-risk action bila scope UAT mencakupnya | Tidak mengubah/menghapus Audit dan tidak mendapat content access otomatis |
| **Support/Superadmin permission profile** | Menangani issue yang dikirim user | Supporting workflow bila defect UAT dilaporkan melalui aplikasi | Bukan role wildcard; tidak otomatis membuka dokumen di luar attachment yang dikirim |
| **Assistant on behalf of User** | Membantu user menemukan/menyusun tool/workflow | Assistant-First dan parity | User tetap Actor; Assistant hanya Origin, tidak mendapat permission tambahan |
| **System/Service/Worker** | Menjalankan pekerjaan yang diprakarsai user | Diamati melalui progress/result/recovery, bukan peserta UAT manusia | Executor tidak menggantikan Actor dan tidak memberi hak baru |
| **Perwakilan bisnis/UAT tester** | Menilai apakah pekerjaan nyata dapat diselesaikan | Menjalankan skenario sesuai actor account yang diberikan | Bukan role aplikasi baru; akses tetap mengikuti account/permission resmi |
| **UAT Lead/pemilik sign-off yang ditunjuk Pertamina** | Mengonsolidasikan hasil, defect, risiko, dan keputusan | Governance, result review, sign-off | Nama/authority final perlu ditetapkan Pertamina; tidak boleh mengubah expected behavior agar pass |

## 2.2 Aturan actor

1. Persona Desktop dan Mobile/Tablet tetap formal role `User`; keduanya bukan role baru.
2. Actor aktual tidak berubah ketika Assistant atau Worker terlibat.
3. Administrator, Auditor, dan Support hanya menjalankan UAT sesuai permission yang memang dirilis.
4. Account UAT tidak boleh mempunyai wildcard permission tersembunyi.
5. Cross-user negative scenario memakai dua account sintetis berbeda dan tidak memakai data nyata.
6. Sensitive business acceptance dilakukan oleh user yang memahami konteks dan berwenang atas dokumen sintetis tersebut.

---

# 3. Triple Entry Flow UAT

## 3.1 Prinsip parity

```text
Entry berbeda
→ cara menemukan tool boleh berbeda
→ Feature ID dan aturan kerja tetap canonical
→ izin, capability, confirmation, processing, dan finality tetap sama
→ hasil bisnis setara
```

User tidak harus melihat Feature ID, schema version, atau engine. UAT Lead menggunakannya hanya untuk traceability.

## 3.2 Skenario entry flow

| UAT ID | Entry | Business goal | Precondition | Steps user | Expected result | Critical UX | Acceptance |
|---|---|---|---|---|---|---|---|
| `UAT-ENTRY-001` | File-First | Dari file yang sudah dimiliki, user cepat menemukan tindakan yang relevan | User berwenang; dua PDF valid; Merge exposed | Masukkan/drop file → lihat deteksi dan rekomendasi → pilih Merge → preview → proses → simpan | Sistem mengenali file, menawarkan aksi yang masuk akal, dan menghasilkan satu PDF berurutan | Rekomendasi ringkas 3–5 aksi, tidak memaksa proses, validasi/status mudah dipahami | `PASS` bila user menyelesaikan Merge tanpa mencari menu teknis dan hasil dapat digunakan |
| `UAT-ENTRY-002` | Tool-First | User yang sudah tahu kebutuhannya dapat langsung menemukan tool | User berwenang; dua PDF valid | Cari “gabung PDF” → pilih Merge → masukkan file → atur urutan → proses → simpan | Tool yang benar ditemukan melalui istilah natural dan menghasilkan outcome yang sama | Search relevan, nama/icon konsisten, tidak ada menu/parameter teknis yang membingungkan | `PASS` bila user menemukan tool dan menyelesaikan pekerjaan tanpa jalur alternatif tersembunyi |
| `UAT-ENTRY-003` | Assistant-First | User menyatakan hasil, bukan memilih tool satu per satu | User berwenang; Assistant dan Merge exposed | Tulis “Gabungkan A lalu B” → review plan → koreksi bila perlu → jalankan → buka hasil | Assistant memilih Merge canonical, mempertahankan order, dan hanya menyatakan sukses setelah hasil tersedia | Plan jelas menyebut file, urutan, output, risiko/kebutuhan koneksi; ambiguity ditanya | `PASS` bila user memahami plan dan memperoleh hasil yang sama tanpa Assistant mengarang aksi/hasil |
| `UAT-ENTRY-004` | Parity | Membuktikan tiga entry tidak menciptakan business rule berbeda | Dataset A,B yang sama; parameter dan account sama | Jalankan `001`–`003`; bandingkan urutan, hasil, source, feedback, History | Ketiga output secara semantik setara; hanya origin/discovery berbeda | Istilah, progress, error, dan Save As konsisten | `PASS` bila hasil, permission, source protection, dan completion meaning setara |
| `UAT-ENTRY-005` | Assistant multi-step | Menyelesaikan pekerjaan gabung → nomor halaman → kompres | Seluruh Feature exposed dan user permitted | Nyatakan tujuan → review urutan step → ubah parameter nomor bila perlu → konfirmasi → proses → buka output | Satu output final mengikuti seluruh langkah; intermediate tidak disajikan sebagai final palsu | Cumulative plan, kebutuhan resource/server, progress per tahap, recovery jelas | `PASS` bila output memenuhi tiga tujuan dan user dapat mengetahui tahap gagal bila tidak selesai |
| `UAT-ENTRY-006` | Capability unavailable | User tidak diarahkan ke jalan buntu | Satu capability tidak tersedia pada platform/device | Cari/minta Feature dari File/Tool/Assistant entry | Fitur disembunyikan atau dijelaskan dengan alternatif yang disetujui; tidak muncul tombol mati/false success | Bahasa actionable dan konsisten ant entry | `PASS` bila user memahami mengapa tidak tersedia dan apa opsi amannya |
| `UAT-ENTRY-007` | Permission revoked/stale | Entry UI tidak menjadi authority | Screen/plan sudah terbuka lalu permission/flag berubah | Coba menjalankan dari ketiga entry | Semua entry menolak secara konsisten tanpa memproses atau mengungkap resource | Pesan aman, tidak menuduh/mengekspos detail, navigation recovery jelas | `PASS` bila tidak ada entry yang menjadi bypass |
| `UAT-ENTRY-008` | Offline parity | Capability lokal tetap berguna tanpa server traffic | Offline grant valid; Merge local-capable | Jalankan Merge dari File/Tool/Assistant saat offline | Ketiganya tetap lokal; History lokal tersedia; sync metadata menunggu dengan aman | Online-required dan local-capable dibedakan jelas | `PASS` bila pekerjaan selesai lokal dan tidak ada consent/upload tersembunyi |

## 3.3 Parity acceptance

Triple Entry Flow lulus hanya jika:

- Feature dan business meaning sama;
- input yang sama menghasilkan order/selection yang sama;
- permission/policy/capability decision sama;
- confirmation tidak hilang pada Assistant-First;
- local-only tetap lokal pada ketiga entry;
- hasil tervalidasi dan dapat dibuka;
- source tetap aman;
- error/recovery tidak bertentangan;
- Assistant tidak menjadi satu-satunya cara mengakses tool.

---

# 4. 33 Core coverage matrix

## 4.1 Precondition shorthand

| Kode | Precondition |
|---|---|
| `BASE` | RC/lane UAT siap; user punya permission; Feature exposed; data sintetis valid; output location tersedia |
| `POC` | `BASE` + approved engine/provider/device outcome dan limitation release sudah diketahui |
| `DEVICE` | `BASE` + real device/camera/scanner permission dan platform exposure |
| `SEC` | `BASE` + authorized synthetic content/password/signature asset dan high-risk guidance |
| `AI` | `POC` + approved local/enterprise-internal provider, answer-key corpus, privacy notice/policy siap |

Semua row juga mewarisi source protection, validated final result, privacy-safe evidence, dan History/Audit rule yang berlaku.

## 4.2 Matrix

| UAT ID | Feature ID | Business Goal | Actor | Precondition | Steps | Expected Result | Critical UX | Risk | Acceptance |
|---|---|---|---|---|---|---|---|---|---|
| `UAT-CORE-001` | `FEAT-PDF-005` | Menggabungkan beberapa PDF menjadi satu dokumen berurutan | User | `BASE`; ≥2 PDF | Pilih/drop → atur order → Merge → Save As → buka | Satu PDF valid dengan seluruh halaman sesuai order | Order/preview/progress/cancel/Save As jelas | Low / D1+detail | `PASS` bila hasil dapat digunakan dan semua source tetap utuh; lihat Section 5 |
| `UAT-CORE-002` | `FEAT-PDF-006/007` | Membagi satu PDF menjadi beberapa dokumen yang tepat | User | `BASE`; PDF multi-page | Pilih mode/range → preview pembagian → proses → buka outputs | Set output sesuai rentang tanpa halaman hilang/tertukar | Range validation dan output naming mudah dipahami | Low / D1 | `PASS` bila user memperoleh semua bagian yang dimaksud dan dapat membukanya |
| `UAT-CORE-003` | `FEAT-PDF-013` | Menghapus halaman yang tidak dibutuhkan dari hasil baru | User | `BASE`; PDF dengan halaman penanda | Pilih halaman → remove → Undo/redo bila perlu → finalisasi | Halaman target tidak ada pada output; original tetap lengkap | Thumbnail selection, Undo, minimum-page warning | Low / D1 | `PASS` bila output tepat dan user dapat memperbaiki pilihan sebelum final |
| `UAT-CORE-004` | `FEAT-PDF-014` | Membuat PDF baru hanya dari halaman terpilih | User | `BASE`; PDF multi-page | Pilih halaman/order → extract → Save As → buka | PDF baru berisi selection tepat sesuai order | Selection summary dan output location jelas | Low / D1 | `PASS` bila selected pages lengkap dan source tidak berubah |
| `UAT-CORE-005` | `FEAT-PDF-010..016` | Menata halaman menjadi dokumen siap digunakan | User | `BASE`; PDF multi-page | Reorder/move/remove/duplicate/add blank sesuai skenario → preview → finalisasi | Output mencerminkan working arrangement dan tidak mengubah source | Multi-page preview, Undo/Redo, no unnecessary confirm | Low / D1 | `PASS` bila user dapat menyusun target tanpa kehilangan orientasi/halaman |
| `UAT-CORE-006` | `FEAT-SCAN-014` | Mengubah dokumen fisik menjadi PDF digital | User Mobile/Tablet atau Desktop sesuai exposure | `DEVICE/POC`; dokumen fisik sintetis | Capture/scan → retake bila perlu → crop/perspective → reorder → selesai | PDF dapat dibaca, halaman lengkap dan berurutan | Touch-natural capture, edge/crop, retake, device error | Low / D2 | `PASS` bila user mendigitalkan dokumen tanpa memerlukan workflow Desktop yang dipaksakan |
| `UAT-CORE-007` | `FEAT-PDF-023` | Mengecilkan ukuran PDF agar lebih mudah dikirim/disimpan | User | `BASE`; PDF representative | Pilih kualitas → lihat trade-off → compress → buka hasil | File lebih sesuai tujuan ukuran dan tetap terbaca; limitation jujur bila reduction minim | Preset sederhana, quality/size feedback, no misleading promise | Low / D1 | `PASS` bila hasil usable dan user memahami kualitas/ukuran akhir |
| `UAT-CORE-008` | `FEAT-PDF-025/026` | Memulihkan PDF bermasalah menjadi file yang bisa dibuka/diproses | User | `BASE`; repairable dan non-repairable sample | Pilih file → repair → review/open hasil atau guidance | Repairable file menghasilkan copy usable; non-repairable tidak dinyatakan sukses | Tidak ada false success; original aman; recovery guidance | Low / D2 | `PASS` bila hasil benar-benar dapat dibuka atau user mendapat kegagalan jujur |
| `UAT-CORE-009` | `FEAT-OCR-004` | Membuat PDF scan dapat dicari/disalin secara wajar | User | `POC`; scan ID/EN dengan answer text | Pilih language/pages → OCR → search/copy known terms → save | Known terms dapat ditemukan; visual pages tetap usable; limitation terlihat | Progress per halaman, language, scanned/low-quality warning | Medium / D2 | `PASS` bila user dapat menemukan informasi kunci dan tidak dijanjikan akurasi palsu |
| `UAT-CORE-010` | `FEAT-CONV-007` | Menggabungkan gambar menjadi satu PDF berurutan | User | `BASE`; beberapa JPG | Pilih images → order/orientation/size → convert → buka | PDF memiliki halaman gambar sesuai order dan orientasi | Thumbnail/order/page-fit sederhana | Low / D1 | `PASS` bila hasil siap dibaca/dibagikan tanpa gambar terpotong tak terduga |
| `UAT-CORE-011` | `FEAT-CONV-001` | Membuat PDF dari Word dengan isi/layout yang layak | User | `POC`; DOCX representative | Pilih DOCX → review option/limitation → convert → compare/open | PDF membuka dan mempertahankan content/layout utama sesuai approved capability | Font/macro/unsupported warning dan progress jelas | Low / D2 | `PASS` bila dokumen siap dibaca/dibagikan dan limitation tidak disembunyikan |
| `UAT-CORE-012` | `FEAT-CONV-003` | Membuat PDF dari presentasi | User | `POC`; PPTX representative | Pilih PPTX/slide scope → convert → review pages | Slide lengkap, berurutan, dan visual utama tidak rusak di luar limitation | Slide selection dan fidelity warning | Low / D2 | `PASS` bila PDF presentasi layak dibagikan/print sesuai scope |
| `UAT-CORE-013` | `FEAT-CONV-002` | Membuat PDF dari workbook/sheet terpilih | User | `POC`; XLSX dengan sheet/print area | Pilih sheets/orientation/paper → convert → review | Sheet terpilih muncul dengan pagination/print area yang dapat digunakan | Preview page break, sheet selection, formula safety notice bila berlaku | Low / D2 | `PASS` bila isi bisnis tidak hilang/terpotong tanpa warning |
| `UAT-CORE-014` | `FEAT-CONV-017` | Menyimpan HTML yang diizinkan sebagai PDF | User | `POC`; approved HTML sample | Pilih/input sumber yang diizinkan → review → convert → open | PDF mencerminkan content/layout yang didukung tanpa menjalankan input berbahaya | Source restriction dan unsupported external content jelas | Medium / D2 | `PASS` bila hasil usable dan user tidak diarahkan memasukkan arbitrary public URL |
| `UAT-CORE-015` | `FEAT-CONV-012` | Mengubah halaman PDF menjadi gambar | User | `BASE`; PDF multi-page | Pilih pages/resolution → convert → buka images | Satu image per selection dengan urutan/resolusi yang dipahami | Estimate count/quality/output location | Low / D1 | `PASS` bila image lengkap dan bisa digunakan untuk kebutuhan target |
| `UAT-CORE-016` | `FEAT-CONV-004` | Mendapat dokumen Word yang dapat diedit dari PDF | User | `POC`; text/layout representative | Pilih PDF → convert → buka/edit sample paragraph | Word output dapat dibuka dan bagian target dapat diedit sesuai limitation | Editability/layout limitation dinyatakan sebelum proses | Low / D2 | `PASS` bila output memenuhi penggunaan yang dijanjikan, bukan sekadar extension benar |
| `UAT-CORE-017` | `FEAT-CONV-006` | Mengubah PDF menjadi presentasi yang dapat disunting | User | `POC`; PDF slide-like | Convert → buka presentation → cek slide order/editability | Slides terbentuk sesuai urutan dan dapat digunakan dalam approved limitation | Segmentation limitation dan preview | Low / D2 | `PASS` bila user dapat melanjutkan pekerjaan presentasi secara wajar |
| `UAT-CORE-018` | `FEAT-CONV-005` | Mengambil tabel PDF ke workbook yang dapat digunakan | User | `POC`; table corpus | Pilih pages/tables → convert → buka spreadsheet → cek cells | Data tabel utama masuk cell yang tepat sesuai limitation; output aman dibuka | Table selection, limitation, no formula surprise | Low / D2 | `PASS` bila user dapat memakai data tanpa salah representasi material |
| `UAT-CORE-019` | `FEAT-PDF-075` | Menghasilkan PDF/A sesuai kebutuhan arsip yang didukung | User | `POC`; supported input | Pilih conformance option yang dirilis → convert → review report/output | Output lolos conformance evidence yang disetujui atau gagal jujur | Standard/limitation dijelaskan tanpa jargon berlebih | Medium / D2 | `PASS` bila hasil benar-benar sesuai profile yang diklaim |
| `UAT-CORE-020` | `FEAT-PDF-012` | Memperbaiki orientasi halaman | User | `BASE`; mixed orientation PDF | Pilih pages → rotate → preview → save | Hanya halaman terpilih berubah ke orientasi target | Direct manipulation, Undo, angle state visible | Low / D1 | `PASS` bila output mudah dibaca dan source tetap utuh |
| `UAT-CORE-021` | `FEAT-PDF-037` | Menambahkan nomor halaman yang konsisten | User | `BASE`; multi-page PDF | Pilih range/start/position → preview → save | Nomor muncul pada halaman/posisi yang dipilih tanpa menutupi content utama | Live preview dan range/start clarity | Low / D1 | `PASS` bila numbering siap digunakan tanpa manual correction material |
| `UAT-CORE-022` | `FEAT-PDF-036` | Memberi watermark yang jelas dan terkendali | User | `BASE`; PDF representative | Masukkan text/image sintetis → position/opacity/pages → preview → save | Watermark muncul sesuai setting pada pages target | Preview/opacity/repeat, easy remove before final | Low / D1 | `PASS` bila user mendapat watermark yang terbaca tanpa merusak penggunaan dokumen |
| `UAT-CORE-023` | `FEAT-PDF-019` | Memotong area halaman yang tidak diperlukan | User | `BASE`; PDF dengan crop markers | Atur crop per page/all → preview → save → reopen | Area hasil sesuai crop dan content target tidak hilang tak terlihat | Handles/bounds/Apply-to clarity, Undo | Low / D1 | `PASS` bila output menampilkan area yang dimaksud secara konsisten |
| `UAT-CORE-024` | `FEAT-PDF-031..041` | Mengedit elemen PDF yang didukung | User | `POC`; supported/unsupported samples | Pilih edit → ubah supported content → preview → save; coba unsupported case | Supported edit tersimpan; unsupported content mendapat limitation/fallback jujur | Perbedaan edit vs annotation jelas; no silent layout damage | Medium / D2 | `PASS` bila user dapat menyelesaikan edit yang diklaim dan tidak tertipu pada case unsupported |
| `UAT-CORE-025` | `FEAT-PDF-053..060` | Mengisi/mengelola form PDF dan membuka ulang nilainya | User | `BASE`; form sample | Isi field/check/select → save → close/reopen → verify | Field identity/value/required state benar dan output usable | Tab/focus/label, validation, save/reopen clarity | Low / D1 | `PASS` bila user dapat menyelesaikan form tanpa nilai tertukar/hilang |
| `UAT-CORE-026` | `FEAT-PDF-062` | Membuka proteksi PDF hanya dengan otorisasi/password sah | User | `SEC`; authorized protected PDF | Masukkan password benar → unlock copy → reopen; coba salah/no password | Authorized copy usable; wrong/no password tidak bypass; source tetap terlindungi | Password masking, clear authority warning, no persistence | High / D3 | `PASS` bila hanya tindakan berwenang berhasil dan tidak ada kesan “crack/bypass” |
| `UAT-CORE-027` | `FEAT-PDF-061` | Melindungi PDF agar akses berikutnya memerlukan credential yang dipilih | User | `SEC`; unprotected sample | Atur protection → confirm → save → reopen benar/salah password | Output terlindungi sesuai setting; source tidak berubah; password tidak ditampilkan ulang | Password confirmation/strength guidance, final summary | Medium / D2+ | `PASS` bila proteksi terbukti saat reopen dan user memahami dampaknya |
| `UAT-CORE-028` | `FEAT-SIGN-013` | Menempatkan tanda tangan/paraf visual pada PDF final | User | `SEC/DEVICE`; synthetic asset | Draw/upload/paste → place/resize/move → preview → confirm export | Mark visual tepat pada output; source utuh; tidak diklaim PKI | Touch/stylus/Desktop-natural, clear/undo, asset-save opt-in | Medium / D2+ | `PASS` bila output visual sesuai maksud dan UI jelas menyatakan non-PKI |
| `UAT-CORE-029` | `FEAT-PDF-069` | Menghilangkan informasi sensitif secara permanen pada output | User | `SEC`; redaction answer-key PDF | Mark areas → preview → final confirm → reopen/search/copy | Informasi target tidak dapat dilihat/dicari/disalin pada output; source utuh | Irreversible warning, exact scope summary, cancel before final | High / D3 | `PASS` bila redaction benar-benar menghapus/meratakan content, bukan overlay |
| `UAT-CORE-030` | `FEAT-PDF-071/072` | Memahami perbedaan antara dua PDF | User | `BASE`; known-difference pair | Pilih original/compare file → process → review differences | User dapat menemukan perubahan yang ditanam dan mengetahui source masing-masing | Difference navigation, provenance, no ambiguous colors only | Low / D1 | `PASS` bila semua perubahan penting yang diharapkan dapat dipahami |
| `UAT-CORE-031` | `FEAT-INTEL-001` | Mendapat ringkasan yang membantu memahami PDF lebih cepat | User | `AI`; answer-key corpus | Pilih document/pages → start explicitly → review summary/source | Ringkasan memuat key points, tidak menambah fakta material, diberi label AI-assisted | Scope/provenance/limitation/provider notice bila internal server | Medium / D2+ | `PASS` bila ringkasan berguna dan grounded; tidak mengganti source |
| `UAT-CORE-032` | `FEAT-INTEL-002` | Mendapat terjemahan yang dapat dipahami ke bahasa target | User | `AI`; bilingual reviewed corpus | Pilih source/target language → translate → preview → review terms/layout → export | Makna utama dan istilah kritis sesuai reviewed expectation; limitation layout terlihat | Language control, preview, fidelity notice, explicit export | Medium / D2+ | `PASS` bila hasil usable untuk tujuan yang disetujui tanpa fidelity claim palsu |
| `UAT-CORE-033` | `FEAT-CONV-036` | Mengubah struktur teks PDF menjadi Markdown yang dapat diedit | User | `POC`; text and scan samples | Convert → preview headings/lists/tables → save `.md` → open | Markdown valid dan struktur utama dipertahankan sejauh capability; OCR fallback jelas | Preview/source mode, heading/table limitation, no AI when unnecessary | Low / D2 | `PASS` bila file `.md` dapat dipakai dan struktur penting tidak hilang diam-diam |

## 4.3 Rule coverage 33 Core

- Seluruh 33 row harus mempunyai hasil `PASS`, `FAIL`, `BLOCKED`, `NOT_RUN`, atau `N/A` dengan alasan.
- Feature PoC-gated tidak boleh `PASS` menggunakan fake/generic output; approved implementation dan limitation harus sudah ada.
- Platform yang tidak exposed diberi `N/A` dengan release capability evidence, bukan dipaksa melalui deep link/Assistant.
- Low-risk tidak berarti source/finality/privacy dapat diabaikan.
- High-risk lulus hanya setelah skenario tambahan Section 6 yang berlaku.

---

# 5. Merge PDF detailed UAT

## 5.1 Business goal dan baseline

**Business goal:** user dapat menggabungkan dua atau lebih PDF menjadi satu file berurutan, melihat progress, mengoreksi pilihan, menyimpan hasil baru, dan tetap yakin bahwa semua file sumber aman—termasuk ketika offline, dibatalkan, gagal, atau dipulihkan.

Baseline:

```text
Feature       : FEAT-PDF-005
Actor         : User
Mode bisnis   : lokal; user tidak memilih engine
Input         : dua atau lebih PDF berurutan
Output        : satu PDF final atomik
Save behavior : Save As/hasil baru; source tidak ditimpa otomatis
History       : outcome pribadi dapat dilihat/dibuka ulang
Offline       : tetap berjalan jika grant/capability/resource valid
```

## 5.2 Dataset UAT

| Dataset | Isi |
|---|---|
| `UAT-MRG-DATA-A` | PDF A dua halaman dengan marker `A1`, `A2` |
| `UAT-MRG-DATA-B` | PDF B satu halaman dengan marker `B1` |
| `UAT-MRG-DATA-C` | PDF C tiga halaman dengan marker `C1..C3` |
| `UAT-MRG-DATA-BAD` | File berextension `.pdf` tetapi tidak valid |
| `UAT-MRG-DATA-ENC` | PDF terlindungi password sintetis |
| `UAT-MRG-DATA-LARGE` | Corpus approved besar/banyak file untuk progress/cancel/recovery |

Marker membuat business user dapat memeriksa urutan tanpa memahami parser/engine.

## 5.3 Primary end-to-end UAT

### `UAT-MRG-001` — Merge melalui file picker

**Precondition:** `BASE`; A, B, C tersedia; output folder dapat ditulis.

**Steps:**

1. Buka PERTAMINA OTO dan pilih jalur File-First.
2. Pilih A, B, dan C melalui file picker.
3. Tunggu validasi dan pilih rekomendasi **Merge PDF**.
4. Pastikan urutan awal A, B, C terlihat.
5. Ubah urutan menjadi C, A, B.
6. Hapus B, lalu gunakan Undo untuk mengembalikannya.
7. Mulai proses.
8. Amati progress dan tahap sampai selesai.
9. Review ringkasan/result.
10. Pilih **Save As** ke nama baru.
11. Tutup hasil lalu buka kembali dari History.
12. Buka A, B, dan C dari lokasi asal.

**Expected business result:** satu PDF baru berisi `C1,C2,C3,A1,A2,B1`; hasil dapat dibuka kembali; A, B, C tetap dapat dibuka dan tidak berubah; History menunjukkan Merge berhasil dan menyediakan aksi yang benar.

**Acceptance:** `PASS` hanya jika user menyelesaikan seluruh tujuan tanpa dialog teknis, tanpa kehilangan order, tanpa overwrite source, dan tanpa status sukses sebelum hasil benar-benar tersedia.

## 5.4 Detailed scenario matrix

| UAT ID | Fokus | Precondition | Steps user | Expected user outcome | Acceptance |
|---|---|---|---|---|---|
| `UAT-MRG-002` | Drag-and-drop | `BASE`; A,B | Drag A dan B ke area input → pilih Merge → save | Kedua file diterima dan outcome sama dengan picker | `PASS` bila drop target jelas, tidak duplikat tak sengaja, dan hasil A lalu B |
| `UAT-MRG-003` | Validasi zero/one file | `BASE`; A | Buka Merge tanpa file lalu dengan A saja | Sistem menjelaskan minimal dua PDF dan tidak memulai proses | `PASS` bila user tahu cara memperbaiki tanpa error teknis |
| `UAT-MRG-004` | File invalid | `BASE`; A + BAD | Masukkan keduanya → review status → coba process | BAD ditandai jelas; Merge atomik tidak menghasilkan final dengan input wajib invalid; user dapat remove BAD dan melanjutkan bila dua input valid tersedia | `PASS` bila tidak ada false/partial success dan guidance actionable |
| `UAT-MRG-005` | Password validation | `BASE`; A + ENC | Masukkan → wrong password → correct password sintetis | Wrong password aman; correct membuat file eligible; password tidak muncul lagi di History/recent UI | `PASS` bila user dapat lanjut secara sah tanpa password tersimpan/terlihat |
| `UAT-MRG-006` | Initial order | `BASE`; A,B,C | Tambahkan dalam order A,B,C → preview marker/list | Input order terlihat dan tidak disortir nama/waktu secara tersembunyi | `PASS` bila user dapat memprediksi hasil sebelum process |
| `UAT-MRG-007` | Reorder | `BASE`; A,B,C | Drag C ke awal → process → open output | Output C,A,B | `PASS` bila list/preview dan result konsisten |
| `UAT-MRG-008` | Remove dan Undo | `BASE`; A,B,C | Remove B → Undo → remove A → process | Working state mengikuti tindakan; source file tidak dihapus | `PASS` bila Undo mudah ditemukan dan output sesuai final list |
| `UAT-MRG-009` | Explicit duplicate | `BASE`; A,B | Tambahkan A dua kali secara sengaja + B → process | Output memuat dua occurrence A sesuai order; tidak silent dedupe | `PASS` bila duplicate terlihat sebelum process dan hasil sesuai intent |
| `UAT-MRG-010` | Proses normal | `BASE`; A,B | Mulai Merge | UI tetap dapat digunakan sesuai desain dan proses tidak berpindah ke server secara terlihat/diam-diam | `PASS` bila pekerjaan selesai lokal tanpa meminta consent upload |
| `UAT-MRG-011` | Progress | `BASE`; LARGE | Mulai Merge besar → amati progress/tahap/item/estimasi jika tersedia | Tidak ada silent wait; status bermakna dan tidak meloncat menjadi sukses palsu | `PASS` bila user mengetahui pekerjaan masih berjalan dan dapat memilih cancel saat aman |
| `UAT-MRG-012` | Cancel sebelum mulai | `BASE`; queued LARGE | Tekan Cancel sebelum pekerjaan berjalan | Status cancelled; tidak ada final output | `PASS` bila pembatalan jelas dan user dapat memulai ulang |
| `UAT-MRG-013` | Cancel saat berjalan | `BASE`; LARGE | Cancel saat progress aktif → tunggu safe stop | Proses berhenti aman; source tetap; temp tidak ditawarkan sebagai final | `PASS` bila tidak muncul completion terlambat/menyesatkan |
| `UAT-MRG-014` | Result validation | `BASE`; A,B,C | Selesaikan → buka result → periksa semua marker | Semua pages lengkap, urut, dapat dibaca | `PASS` bila file bukan hanya “ada” tetapi benar-benar usable |
| `UAT-MRG-015` | Save As | `BASE`; completed result | Pilih folder/nama baru → save → reopen | Satu hasil final berada di lokasi pilihan | `PASS` bila lokasi/nama jelas dan final dapat dibuka |
| `UAT-MRG-016` | Existing target conflict | `BASE`; target name sudah ada | Save As ke target existing → pilih batal/nama lain | Existing file tetap; user diberi pilihan aman | `PASS` bila tidak ada overwrite diam-diam |
| `UAT-MRG-017` | Target sama dengan source | `BASE`; A,B | Pilih path/nama salah satu source sebagai target | Sistem menolak baseline Save As dan meminta target baru | `PASS` bila digest/isi source tetap sama |
| `UAT-MRG-018` | Source protection pada failure | `BASE`; controlled engine failure data | Mulai → terjadi error → buka A/B | A/B tetap dapat dibuka dan tidak berubah; no half-final | `PASS` bila user dapat mencoba ulang/menutup tanpa data loss |
| `UAT-MRG-019` | History dan reopen | `BASE`; successful Merge | Buka History → pilih hasil → reopen → delete History item | Result dapat dibuka; delete History tidak menghapus source/result/Audit yang diwajibkan | `PASS` bila History membantu melanjutkan pekerjaan dan lifecycle jelas |
| `UAT-MRG-020` | Offline | Valid protected offline session; network off | Masukkan A/B → Merge → Save As → lihat History | Merge selesai lokal; tidak ada prompt upload; metadata sync menunggu aman | `PASS` bila business task selesai tanpa koneksi |
| `UAT-MRG-021` | Offline grant invalid | Grant expired/tampered/wrong profile | Coba Merge | Sistem fail closed/online required sebelum process, tanpa memindahkan file | `PASS` bila pesan jelas dan tidak ada queue/profile leakage |
| `UAT-MRG-022` | Engine failure | `BASE`; fault setup approved oleh QA | Mulai Merge → error → pilih guidance/retry bila tersedia | Error aman dan actionable; tidak ada result palsu; retry tidak loop | `PASS` bila user tahu apakah dapat mencoba lagi dan source aman |
| `UAT-MRG-023` | Low storage | Quota-limited UAT volume | Mulai LARGE ketika ruang tidak cukup | Peringatan sebelum pekerjaan berisiko; user dapat memilih lokasi/bersihkan ruang | `PASS` bila tidak ada source/target rusak atau silent disk exhaustion |
| `UAT-MRG-024` | Recovery app/Worker | `BASE`; LARGE; interruption dikendalikan QA | Mulai → QA menutup app/Worker sesuai charter → buka kembali | Sistem menawarkan Lanjutkan/Coba Lagi/Batalkan sesuai capability; tidak mengarang completed | `PASS` bila user dapat pulih tanpa mengulang edit/order yang seharusnya tersimpan atau kehilangan source |
| `UAT-MRG-025` | Many/large files | `BASE`; LARGE within approved limit | Masukkan corpus → review order → process/cancel | UI tetap responsif; result tepat atau limit dijelaskan sebelum resource habis | `PASS` bila user tidak terjebak silent wait/crash dan outcome akurat |
| `UAT-MRG-026` | Triple-entry parity | Preconditions Section 3 | Jalankan same A,B melalui File/Tool/Assistant | Hasil A,B setara; Assistant origin saja yang berbeda | `PASS` bila tidak ada entry yang mengubah rule/permission/confirmation |
| `UAT-MRG-027` | Accessibility/localization | Desktop target; ID dan EN resources | Keyboard-only, screen reader labels, high contrast, resize/DPI, long text | Input/order/remove/process/cancel/save dapat dipakai; action tidak terpotong | `PASS` bila user dapat menyelesaikan primary task pada acceptance setup |

## 5.5 Merge exit criteria

```text
[ ] UAT-MRG-001..027 applicable selesai dengan status sah
[ ] picker dan drag/drop memberi outcome setara
[ ] validasi, order, reorder, remove, Undo, process, progress, cancel lulus
[ ] result benar, dapat dibuka ulang, dan Save As jelas
[ ] source/target aman pada success/failure/cancel/recovery
[ ] History membantu user dan tidak mencampur lifecycle lain
[ ] offline local-capable lulus; invalid grant fail closed
[ ] error/low storage/recovery memberi tindakan yang dapat dipahami
[ ] File/Tool/Assistant parity lulus
[ ] tidak ada unresolved Blocker/Critical/Major pada Merge business outcome
```

---

# 6. High-risk feature UAT

## 6.1 Protect PDF — `FEAT-PDF-061`

| UAT ID | Skenario | Steps | Expected business result | Acceptance |
|---|---|---|---|---|
| `UAT-HR-PROT-001` | Proteksi normal | Pilih PDF → masukkan/konfirmasi password sintetis + setting yang dirilis → review → finalisasi → reopen | Output meminta credential yang benar dan dapat dibuka dengannya | `PASS` bila proteksi terbukti saat reopen dan source tetap tidak berubah |
| `UAT-HR-PROT-002` | Password salah/kosong saat reopen | Buka output dengan password salah/kosong | Akses ditolak tanpa membocorkan password/isi | `PASS` bila tidak ada bypass dan feedback aman |
| `UAT-HR-PROT-003` | Cancel sebelum final | Atur proteksi → cancel pada confirmation | Tidak ada protected final baru dan source tidak berubah | `PASS` bila cancel benar-benar menghentikan finalisasi |
| `UAT-HR-PROT-004` | Save conflict/recovery | Pilih target existing atau ganggu finalisasi secara terkontrol | Existing target/source aman; user dapat retry dengan tindakan jelas | `PASS` bila tidak ada half-protected/false-success output |

Critical UX: password masked, confirm-password/setting summary, warning dampak, tidak menyimpan password, dan tidak mengklaim capability di luar yang dirilis.

## 6.2 Unlock PDF — `FEAT-PDF-062`

| UAT ID | Skenario | Steps | Expected business result | Acceptance |
|---|---|---|---|---|
| `UAT-HR-UNLOCK-001` | Authorized unlock | Gunakan test PDF yang dimiliki → masukkan password benar → unlock copy → reopen | Copy dapat dibuka sesuai intended output | `PASS` bila user berwenang menyelesaikan pekerjaan dan source tetap protected |
| `UAT-HR-UNLOCK-002` | Wrong/no password | Coba password salah/kosong | Tidak ada output unlocked dan tidak ada guidance bypass | `PASS` bila kegagalan jelas tanpa mengungkap credential/detail proteksi |
| `UAT-HR-UNLOCK-003` | Authority statement | Buka tool dan review guidance; coba account/resource tidak berwenang | UI menegaskan password/otorisasi sah; unauthorized resource tidak diproses | `PASS` bila produk tidak dipersepsikan sebagai cracking tool |
| `UAT-HR-UNLOCK-004` | Error/cancel/privacy | Cancel atau picu error setelah password entry | Tidak ada result palsu; password tidak muncul pada UI history/evidence; retry aman | `PASS` bila no bypass, no secret persistence, source aman |

## 6.3 Redact PDF — `FEAT-PDF-069`

| UAT ID | Skenario | Steps | Expected business result | Acceptance |
|---|---|---|---|---|
| `UAT-HR-REDACT-001` | Preview dan cancel | Mark area/text sintetis → preview → cancel | Working state dapat dibatalkan; source tetap lengkap | `PASS` bila user dapat review scope dan tidak terjadi final removal sebelum confirmation |
| `UAT-HR-REDACT-002` | Final redact | Mark known secret text/image → review summary → confirm final → Save As | Output tidak menampilkan target | `PASS` bila output baru dibuat dan source tidak ditimpa |
| `UAT-HR-REDACT-003` | Removal verification | Reopen output → search/copy/select/extract text di area target → zoom/review | Redacted information tidak dapat dipulihkan melalui penggunaan biasa; bukan sekadar kotak overlay | `PASS` hanya dengan independent redaction evidence dari QA plus user-visible checks |
| `UAT-HR-REDACT-004` | Partial failure/recovery | Ganggu finalization secara terkontrol | Tidak ada partial final yang diberi label sukses; user dapat retry/review kembali | `PASS` bila tidak ada false assurance tentang data sensitif |

Critical UX: irreversible warning, exact page/area count, final confirmation, visual distinction antara mark-for-redaction dan applied redaction, serta no accidental overwrite.

## 6.4 Visual Sign PDF — `FEAT-SIGN-013`

| UAT ID | Skenario | Steps | Expected business result | Acceptance |
|---|---|---|---|---|
| `UAT-HR-SIGN-001` | Mobile/Tablet drawing | Draw dengan touch/stylus → clear/undo → place/resize/move → preview → final export | Visual signature tepat pada lokasi/ukuran target | `PASS` bila interaction natural dan output sesuai preview |
| `UAT-HR-SIGN-002` | Desktop asset | Upload/paste synthetic transparent asset → place → final | Output berisi asset tepat dan source aman | `PASS` bila user dapat menyelesaikan tanpa drawing mouse wajib |
| `UAT-HR-SIGN-003` | Asset save choice | Gunakan asset → pilih/tolak penyimpanan sesuai UI → buka sesi baru | Asset hanya tersedia kembali bila user memilih dan policy mengizinkan | `PASS` bila tidak ada silent save/sync |
| `UAT-HR-SIGN-004` | Scope meaning | Review label/help/output | UI menyatakan tanda tangan/paraf visual non-PKI, bukan certificate digital signature | `PASS` bila tidak ada klaim legal/cryptographic yang tidak ada |

## 6.5 Amankan ke Akun — `FEAT-BACKUP-001` (dengan lifecycle `FEAT-BACKUP-002..010`)

| UAT ID | Skenario | Steps | Expected business result | Acceptance |
|---|---|---|---|---|
| `UAT-HR-SECURE-001` | Explicit secure action | Selesaikan hasil → pilih **Amankan ke Akun** → review notice → confirm → tunggu completion | Hanya hasil yang dipilih muncul pada daftar akun | `PASS` bila tindakan eksplisit, progress jelas, dan tidak terjadi generic workspace sync |
| `UAT-HR-SECURE-002` | Cross-device same account | Login account yang sama pada perangkat kedua → buka/download file | File yang diamankan dapat digunakan dari perangkat lain | `PASS` bila ownership sama dan file benar |
| `UAT-HR-SECURE-003` | Cross-user denial | Login User B → cari/coba buka ID/resource User A | File tidak terlihat/tidak dapat dibuka | `PASS` bila tidak ada existence/content leak |
| `UAT-HR-SECURE-004` | Data not auto-synced | Buat draft/cache/source/password state tanpa menekan secure | Data tersebut tidak muncul pada perangkat lain | `PASS` bila hanya selected result/allowlisted metadata tersinkron |
| `UAT-HR-SECURE-005` | Version/connection recovery | Simpan versi berikutnya, putuskan koneksi, lanjutkan sesuai UI, cek max-three behavior yang dirilis | User melihat status/version yang benar tanpa silent overwrite/duplicate | `PASS` bila transfer dapat dipulihkan dan version lifecycle dipahami |

## 6.6 Permanent Delete

Konteks harus dipisahkan:

- local permanent delete: `FEAT-TRASH-004`;
- secured-file deletion/account lifecycle: `FEAT-BACKUP-007` dan resource Account Recycle/retention yang berlaku.

| UAT ID | Skenario | Steps | Expected business result | Acceptance |
|---|---|---|---|---|
| `UAT-HR-DELETE-001` | Local cancel | Pilih item local Recycle Bin → Permanent Delete → pilih Cancel | Item tetap ada dan dapat direstore | `PASS` bila confirmation menyebut target/dampak secara jelas |
| `UAT-HR-DELETE-002` | Local confirmed delete | Pilih satu item sintetis → confirm/reauth bila policy meminta | Hanya target dihapus permanen; item lain/source lain tetap | `PASS` bila item tidak dapat direstore dan tidak ada bulk ambiguity |
| `UAT-HR-DELETE-003` | Account secured delete | Dari Account Recycle/lifecycle, pilih secured file → confirm/reauth bila berlaku | Resource mengikuti server retention/deletion policy dan state terlihat jelas | `PASS` bila user tidak mengira ini sama dengan local Recycle Bin |
| `UAT-HR-DELETE-004` | Offline/permission/recovery | Coba account permanent delete offline atau setelah permission revoked; ganggu acknowledgement | Aksi menunggu/ditolak aman; tidak ada duplicate/destructive uncertainty; authoritative status dipulihkan saat online | `PASS` bila user mengetahui apakah item masih ada atau benar-benar terhapus |

Durasi retention dan exact reauthentication trigger berasal dari policy resmi, bukan ditentukan UAT report.

## 6.7 AI Summarizer — `FEAT-INTEL-001`

| UAT ID | Skenario | Steps | Expected business result | Acceptance |
|---|---|---|---|---|
| `UAT-HR-SUM-001` | Text PDF summary | Pilih document/pages → mulai eksplisit → review key points vs answer key | Ringkasan ringkas, relevan, dan mencakup fakta utama | `PASS` bila tidak ada fakta material yang tidak bersumber dan source tetap authority |
| `UAT-HR-SUM-002` | Scanned PDF | Pilih scan → sistem menjalankan/menawarkan OCR sesuai capability → summarize | Ringkasan grounded pada extracted content; limitation OCR terlihat | `PASS` bila user memahami confidence/limitation dan hasil tetap berguna |
| `UAT-HR-SUM-003` | Provider/network unavailable | Jalankan pada mode yang memerlukan approved internal provider saat offline/unavailable | Local eligible route berjalan atau user mendapat online-required/retry yang jujur | `PASS` bila tidak ada public fallback/false success |
| `UAT-HR-SUM-004` | Scope/privacy/Assistant | Minta Assistant meringkas selected pages; ubah scope; review notice | Hanya scope yang dipilih dipakai dan Assistant menampilkan actual result | `PASS` bila user mengendalikan content scope dan output berlabel AI-assisted |

## 6.8 Translate PDF — `FEAT-INTEL-002`

| UAT ID | Skenario | Steps | Expected business result | Acceptance |
|---|---|---|---|---|
| `UAT-HR-TRANS-001` | ID → EN | Pilih source/target → translate → preview → review glossary/key sentences | Makna utama dan istilah kritis sesuai approved answer key | `PASS` bila hasil dapat dipahami dan kesalahan material tidak lolos tanpa indication |
| `UAT-HR-TRANS-002` | EN → ID | Ulangi pada corpus kedua → export | Hasil Bahasa Indonesia dapat digunakan sesuai business purpose | `PASS` bila translation dan punctuation/number/date context tidak menyesatkan |
| `UAT-HR-TRANS-003` | Layout limitation | Gunakan PDF kompleks → preview hasil dan limitation | UI menyatakan bagian/layout yang mungkin berubah; user dapat cancel sebelum export | `PASS` bila tidak ada fidelity promise palsu |
| `UAT-HR-TRANS-004` | Sensitive/offline/provider unavailable | Gunakan classification fixture yang tidak mengizinkan transfer atau putuskan koneksi | Sistem memakai approved eligible path atau menolak/menunggu dengan aman | `PASS` bila tidak ada public provider/fallback tersembunyi dan source tetap aman |

---

# 7. Error/recovery business cases

| UAT ID | Business situation | What user does | Expected business behavior | Acceptance |
|---|---|---|---|---|
| `UAT-ERR-001` | File rusak/tidak didukung | Masukkan file dan review status | User mengetahui file mana bermasalah dan cara mengganti/remove | Tidak ada output palsu atau pesan teknis mentah |
| `UAT-ERR-002` | Password diperlukan/salah | Masukkan password sintetis salah lalu benar | Retry aman; password tidak terlihat kembali | User dapat lanjut secara sah tanpa secret leakage |
| `UAT-ERR-003` | Ruang penyimpanan kurang | Mulai pekerjaan besar | Peringatan sebelum data berisiko; pilihan lokasi/cleanup/cancel | Source/target aman dan pesan actionable |
| `UAT-ERR-004` | Capability/engine tidak tersedia | Pilih tool pada platform/capability terbatas | Tool hidden/explained atau approved alternative | Tidak ada dead button/false promise |
| `UAT-ERR-005` | Fitur server diperlukan saat offline | Mulai optional-server-only effective path | Online-required/wait state jelas; tidak auto-upload | User tahu kapan/apa yang dapat dilanjutkan |
| `UAT-ERR-006` | Koneksi putus saat transfer/job | Putuskan/reconnect sesuai charter | Progress/state dipulihkan; tidak membuat pekerjaan ganda | User dapat melanjutkan atau membatalkan tanpa ambiguity |
| `UAT-ERR-007` | User membatalkan | Cancel queued/running operation | Stop pada safe point; no final palsu; source aman | Status cancelled stabil dan restart jelas |
| `UAT-ERR-008` | Aplikasi/Worker berhenti | Restart aplikasi | Draft/job terdeteksi; Lanjutkan/Coba Lagi/Batalkan sesuai capability | Tidak ada data source hilang atau false completed |
| `UAT-ERR-009` | Engine/provider gagal | Ikuti error guidance | Error aman; retry hanya bila wajar; limitation jujur | User tidak masuk retry loop dan tahu next action |
| `UAT-ERR-010` | Target Save As sudah ada | Save ke nama existing | Conflict dan pilihan safe rename/cancel | Existing target tidak berubah diam-diam |
| `UAT-ERR-011` | Permission/account berubah | Coba action dari screen/plan lama | Ditolak konsisten; no content/resource leakage | UI stale tidak menjadi bypass |
| `UAT-ERR-012` | Assistant salah/ambigu | Beri instruksi multi-tafsir/destructive | Assistant meminta klarifikasi atau memberi pilihan aman | Tidak menebak action sensitif |
| `UAT-ERR-013` | Result tidak valid/parsial | Picu approved negative fixture | Result tidak ditawarkan sebagai final; guidance tersedia | Technical “success” tidak menipu user |
| `UAT-ERR-014` | History result pindah/hilang | Buka item History dengan result unavailable | Status/guidance akurat; tidak crash; source lifecycle tidak dikarang | User memahami bahwa History metadata bukan content backup |
| `UAT-ERR-015` | Amankan ke Akun acknowledgement hilang | Putuskan setelah action diterima | Setelah reconnect, state authoritative menunjukkan satu outcome | Tidak ada duplicate file/version dari retry user |
| `UAT-ERR-016` | Permanent delete outcome tidak pasti | Ganggu connection/acknowledgement | UI merekonsiliasi sebelum menawarkan retry destructive | User tidak dapat memicu double effect atau salah percaya item hilang/ada |
| `UAT-ERR-017` | AI/OCR limitation | Gunakan scan buruk/format kompleks | Output menampilkan limitation dan tidak mengarang certainty | User dapat memutuskan review/manual correction |
| `UAT-ERR-018` | Account switch | Buat pending local work lalu ganti account | Work/queue profile lama tidak muncul/terkirim sebagai account baru | Tidak ada cross-account content/evidence mix |

## 7.1 Recovery acceptance rule

Recovery dianggap berhasil bukan karena aplikasi kembali terbuka, tetapi karena:

- user tahu status pekerjaan sebenarnya;
- source dan existing target tetap aman;
- edit/order yang memang checkpointed tidak hilang;
- partial/temp tidak diberi label final;
- retry/resume tidak menggandakan hasil atau aksi;
- permission/policy/capability diperiksa kembali bila diperlukan;
- user mempunyai pilihan tindakan yang dapat dipahami.

---

# 8. Acceptance criteria

## 8.1 Scenario status

| Status | Makna UAT |
|---|---|
| `PASS` | Dijalankan oleh actor yang tepat dan seluruh business result, critical UX, serta forbidden outcome terpenuhi |
| `FAIL` | Minimal satu business result/UX/safety acceptance tidak terpenuhi |
| `BLOCKED` | Tidak dapat dijalankan karena environment/data/account/PoC/build/decision blocker yang diketahui |
| `NOT_RUN` | Direncanakan tetapi belum dijalankan; alasan wajib |
| `INCONCLUSIVE` | Bukti, data, setup, atau expected oracle tidak valid |
| `N/A` | Terbukti tidak berlaku pada release/platform; alasan dan capability scope wajib |

Diagnostic rerun tidak menghapus hasil gagal awal.

## 8.2 Pass rule per scenario

Skenario hanya `PASS` bila:

```text
actor yang tepat dapat memulai
→ langkah utama dapat ditemukan/dipahami
→ permission/capability/confirmation sesuai konteks
→ feedback/progress/error/recovery dapat ditindaklanjuti
→ expected output benar dan dapat digunakan
→ source/target/resource lain tetap aman
→ limitation dinyatakan jujur
→ tidak ada forbidden side effect
```

“File ada”, “proses selesai”, atau “status sukses” tidak cukup.

## 8.3 Overall UAT exit recommendation

```text
[ ] seluruh Must/Core scenario yang released dan applicable mempunyai hasil final
[ ] seluruh D3/high-risk scenario applicable PASS
[ ] seluruh triple-entry parity critical scenario PASS
[ ] Merge detailed UAT PASS
[ ] no open BLOCKER/CRITICAL
[ ] no open MAJOR yang menggagalkan Must/Core business outcome, source safety, privacy, atau critical UX
[ ] known lower-risk issue memiliki scope, workaround, owner, risk, dan approved disposition
[ ] platform/exposure combinations yang diklaim release memiliki evidence
[ ] PoC-gated Feature memiliki approved engine/limitation dan UAT outcome; BLOCKED bukan PASS
[ ] Requirement → Feature → UAT → result/defect/evidence traceability lengkap
[ ] UAT result summary dan sign-off authority lengkap
```

Tidak ada pass-rate percentage rekaan. Semua critical applicable acceptance wajib lulus.

## 8.4 Business acceptance dimensions

| Dimensi | Pertanyaan penerimaan |
|---|---|
| Effectiveness | Apakah user memperoleh output yang dibutuhkan? |
| Correctness | Apakah isi, order, selection, protection, redaction, translation, atau structure tepat? |
| Usability | Apakah user dapat menyelesaikan pekerjaan tanpa bantuan teknis berlebihan? |
| Transparency | Apakah progress, limitation, server need, AI assistance, dan error dijelaskan jujur? |
| Control | Apakah preview, edit, Undo, cancel, confirmation, dan Save As tersedia sesuai risiko? |
| Safety | Apakah source, target, ownership, password, content, dan sensitive action aman? |
| Continuity | Apakah History, offline behavior, recovery, dan cross-device flow membantu user melanjutkan pekerjaan? |
| Consistency | Apakah entry flow/platform yang exposed mempertahankan business rule yang sama? |

## 8.5 Defect dan retest dalam UAT

- Gunakan severity/status Document 12; UAT tidak membuat skala baru.
- Defect mencatat UAT ID, Requirement/Feature ID, build, platform, dataset, steps user, expected vs actual, business impact, dan safe evidence.
- Setelah fix, exact UAT scenario diulang pada artifact yang ditetapkan, lalu regression evidence relevan harus tetap lulus.
- `AS_DESIGNED`, `DEFERRED`, atau `ACCEPTED_RISK` adalah disposition, bukan `PASS`.
- Perubahan expected behavior memerlukan perubahan source document terkontrol; tester tidak menurunkan acceptance agar build lulus.

## 8.6 Sign-off recommendation

Sign-off minimum melibatkan UAT Lead/pemilik bisnis yang ditunjuk Pertamina, Product/Application owner, dan risk/security owner bila high-risk scope berlaku. Nama, delegasi, dan authority resmi harus dikonfirmasi sebelum eksekusi. Sign-off mencatat:

```text
release candidate/build digest
UAT scope dan excluded/limited platform/features
scenario results dan open defects
known limitations/workarounds/risks
high-risk acceptance outcome
decision: ACCEPTED / ACCEPTED_WITH_APPROVED_LIMITATION / REJECTED / BLOCKED
approver identity, role/authority, date, dan evidence reference
```

---

# 9. Requirement/Feature traceability

## 9.1 Trace chain

```text
Business goal/process
→ Requirement ID
→ canonical Feature ID/resource
→ UAT actor + real workflow
→ UAT ID
→ PASS/FAIL/BLOCKED/NOT_RUN/INCONCLUSIVE/N/A
→ defect/evidence
→ business sign-off
→ release/handover
```

## 9.2 Traceability matrix

| Business area | Requirement/source | Feature/resource | UAT coverage |
|---|---|---|---|
| Product usefulness/internal office tools | Document 01 Sections 3–5 | Released PERTAMINA OTO tools | Sections 1, 4, 8 |
| Triple Entry Flow | `DEC-REQ-024..025`, `UIR-UI-005`, `FR-ASSIST-001..014` | All exposed canonical Features | `UAT-ENTRY-001..008`, `UAT-MRG-026` |
| File input | `FR-FILE-001..005` | File-consuming Core | `UAT-MRG-001..002`, Core matrix |
| File validation | `FR-FILE-010..015` | File-consuming Core | `UAT-MRG-003..005`, `UAT-ERR-001..002` |
| Local/server/offline | `FR-PROC-001..005`, `UIR-UI-006..010` | 19 local-only + 14 optional-server Core paths | Core matrix, `UAT-ENTRY-006..008`, `UAT-ERR-004..006` |
| Source/working/finality | `FR-STOR-001..006`, `NFR-REC-001..003` | All transforming Core | `UAT-MRG-006..018`, Core Acceptance |
| Progress/cancel/recovery | `FR-REC-001..006`, `UIR-UI-003..004` | Long-running/Core workflows | `UAT-MRG-010..025`, Section 7 |
| Personal History | `FR-HIST-001..003` | `FEAT-HIST-001..015` + Core outcomes | `UAT-MRG-019`, `UAT-ERR-014` |
| Enterprise Audit | `FR-AUDIT-001..004` | 33 Core + high-risk evidence | UAT entry gate + high-risk acceptance; technical evidence remains Document 12 |
| 33 Core mapping | Document 03 Section 4.1; Document 12 Sections 16–17 | Exact Feature IDs in Section 4 | `UAT-CORE-001..033` |
| Merge PDF | `FR-FILE-001..015`, `FR-PROC-001..005`, `FR-STOR-001..006`, `FR-HIST-001..003`, `FR-REC-001..006` | `FEAT-PDF-005` | `UAT-CORE-001`, `UAT-MRG-001..027` |
| Visual Sign | `FR-SIGN-001..003`, `BR-SIGN-001`, `SR-SEC-011..012` | `FEAT-SIGN-013` | `UAT-CORE-028`, `UAT-HR-SIGN-001..004` |
| Unlock | `SR-SEC-022` + Core processing/storage rules | `FEAT-PDF-062` | `UAT-CORE-026`, `UAT-HR-UNLOCK-001..004` |
| Redact | `SR-SEC-023` + Core processing/storage rules | `FEAT-PDF-069` | `UAT-CORE-029`, `UAT-HR-REDACT-001..004` |
| Protect | Core processing/storage/security rules | `FEAT-PDF-061` | `UAT-CORE-027`, `UAT-HR-PROT-001..004` |
| Amankan ke Akun | `FR-BACKUP-001..010` | `FEAT-BACKUP-001..010` | `UAT-HR-SECURE-001..005`, `UAT-ERR-015` |
| Recycle/Permanent Delete | `FR-TRASH-001..009` | `FEAT-TRASH-004`; account lifecycle `FEAT-BACKUP-007` | `UAT-HR-DELETE-001..004`, `UAT-ERR-016` |
| AI Summarizer | `FR-INTEL-001`, `SR-SEC-021` | `FEAT-INTEL-001` | `UAT-CORE-031`, `UAT-HR-SUM-001..004` |
| Translate PDF | `FR-INTEL-002`, `SR-SEC-021` | `FEAT-INTEL-002` | `UAT-CORE-032`, `UAT-HR-TRANS-001..004` |
| PDF to Markdown | `FR-INTEL-003` | `FEAT-CONV-036` | `UAT-CORE-033` |
| UAT readiness | Document 12 Sections 54–56 | Immutable RC/UAT package | Section 1.4 dan 8 |

Feature-level traceability untuk 33 Core berada pada Section 4 dan tidak boleh diganti dengan alias UI.

---

# 10. Risks/gaps

## 10.1 Open risks dan input yang diperlukan

| ID | Risk/gap | UAT impact | Recommendation |
|---|---|---|---|
| `UAT-R01` | UAT physical topology belum dikonfirmasi | Isolation/reset/evidence dapat berbeda | Konfirmasi dedicated stack atau isolated logical lane; pertahankan same immutable RC |
| `UAT-R02` | UAT participants dan formal sign-off authority belum bernama | Hasil tidak mempunyai business authority | Tetapkan business owner/UAT Lead/delegation sebelum execution |
| `UAT-R03` | Exact release Feature/platform/device/browser matrix belum freeze | 33 Core dapat over/under-scope | Generate matrix dari registry + `CapabilityVersion`; approve included/limited/excluded combinations |
| `UAT-R04` | Banyak conversion/OCR/PDF/A/edit/scanner/AI Feature masih PoC-gated | Expected fidelity/limitation belum final | Hanya masuk execution setelah `APPROVED_ENGINE`/`APPROVED_WITH_LIMITATION`; selain itu `BLOCKED` |
| `UAT-R05` | Approved engine/provider/device build belum lengkap | Output UAT tidak repeatable/representative | Catat exact build/corpus/limitation dalam UAT package |
| `UAT-R06` | Pertamina Identity UAT accounts/lifecycle belum confirmed | Revocation/reauth/cross-user/offline scenario tidak valid | Siapkan synthetic accounts dengan least privilege; no test-only bypass |
| `UAT-R07` | High-risk policy values/reauth trigger/retention belum final | Protect/delete/secured-file steps dapat ambigu | Gunakan current approved policy fixture; jangan hardcode nilai dalam UAT script |
| `UAT-R08` | Representative sanitized corpus dan business answer key belum approved | Fidelity/AI/translation acceptance menjadi subjektif | Buat reviewed corpus manifest dan expected key points/terms/layout properties |
| `UAT-R09` | AI/translation quality bersifat probabilistik/provider-dependent | Hasil bisa tidak stabil dan sign-off tidak konsisten | Gunakan task-specific rubric, fixed corpus/provider version, factuality/terminology red lines, dan reviewer ganda bila perlu |
| `UAT-R10` | Accessibility participant/device/assistive technology inventory belum fixed | Claim aksesibilitas dapat terlalu luas | Tetapkan target devices/AT/zoom/input dan acceptance participant plan |
| `UAT-R11` | User guide/release notes masih entry input | Tester dapat salah memahami intended limitation | Freeze draft guidance bersama RC sebelum UAT |
| `UAT-R12` | Numeric performance target bukan domain keputusan UAT | User dapat merasa lambat walau technical target belum approved | Catat usability observation; technical performance gate tetap Document 12 dan approval resmi |
| `UAT-R13` | 33 Core masih `PLANNED`/gated pada pre-coding baseline | Report ini bukan bukti PASS | Treat sebagai executable specification; jangan menyatakan completion sebelum run evidence |
| `UAT-R14` | Local History dan Account secured storage mudah disalahartikan | User dapat mengira History adalah backup atau delete lokal menghapus cloud | Uji dan label lifecycle terpisah secara eksplisit |

## 10.2 Ambiguitas/konflik yang ditemukan

1. **“Upload/Drop” pada File-First bukan selalu server upload.** Dalam business flow kata upload dipakai sebagai cara memasukkan file, tetapi Local-First melarang server transfer untuk local-only processing. UAT menggunakan istilah **masukkan/pilih/drop** dan memerlukan notice/consent hanya saat content benar-benar dikirim ke server.
2. **Batch validation vs atomic Merge.** `FR-FILE-014` memungkinkan file valid dalam batch tetap diproses ketika file lain bermasalah, tetapi Merge adalah operasi atomik atas ordered required inputs. Interpretasi UAT: invalid item ditandai dan dapat dikeluarkan; Merge baru berjalan bila daftar final memiliki minimal dua input valid. Sistem tidak boleh menghasilkan partial Merge diam-diam.
3. **Permanent Delete mempunyai dua lifecycle.** Local Recycle Bin (`FEAT-TRASH-004`) berbeda dari secured account deletion (`FEAT-BACKUP-007`/Account Recycle-retention). Keduanya tidak boleh digabung menjadi satu skenario/label yang menyembunyikan otoritas dan recovery berbeda.
4. **Sign PDF bukan PKI.** Istilah “sign” dapat dipersepsikan sebagai digital signature berbasis sertifikat, tetapi source mengunci visual signature/paraf non-PKI. UAT wajib menilai clarity label/help dan tidak menerima klaim cryptographic/legal yang tidak ada.
5. **Protect dikategorikan Medium pada Catalog tetapi diminta high-risk depth oleh prompt.** Tidak ada konflik material: risk canonical tetap Medium, sementara UAT memakai enhanced depth karena password/finality sensitif.
6. **AI Core masih feasibility-gated.** Kehadiran Tool Card tidak berarti provider/fidelity sudah accepted. UAT tidak boleh memakai simulated/generic success untuk meluluskan Summarizer/Translate.

Tidak ditemukan alasan untuk mengubah architecture, Feature ID, Requirement, role, processing mode, atau source document.

## 10.3 Decisions/confirmations required before execution

1. Siapa UAT Lead, business sign-off authority, dan alternate approver resmi?
2. Apa exact released Feature × platform × device/browser matrix?
3. Apakah UAT mempunyai dedicated environment atau isolated logical QA lane, dan bagaimana reset window-nya?
4. Account/permission/offline/reauth fixtures mana yang disediakan Pertamina Identity untuk UAT?
5. Approved engine/provider/device version dan limitation apa yang dibawa ke UAT?
6. Dataset sintetis/sanitized, glossary, factual answer key, dan output evidence mana yang disetujui?
7. Policy/reauth/retention/config values mana yang berlaku untuk secured storage dan permanent delete?
8. Known MAJOR/MINOR issue mana yang boleh masuk UAT beserta workaround dan decision authority?

---

# 11. Suggested Document 13 text

## 11.1 Purpose and principle

> **User Acceptance Testing PERTAMINA OTO** membuktikan bahwa user yang berwenang dapat menyelesaikan pekerjaan office-document nyata melalui workflow yang mudah dipahami dan hasil yang dapat digunakan. UAT menilai business outcome, usability, clarity, safety, continuity, dan consistency. Technical status, file existence, atau transport success tidak cukup bila user tidak memperoleh hasil yang benar.

## 11.2 Scope and readiness

> UAT memakai immutable release candidate yang sama dengan QA evidence dan hanya dimulai setelah UAT readiness gate Document 12 lulus. UAT lane, accounts, permissions, data, platform, capability, known issues, user guidance, defect/retest path, dan sign-off authority harus siap. UAT tidak menjalankan load, fuzz, malicious corpus, VA/PT, direct database manipulation, atau test-only authorization bypass.

## 11.3 Actor model

> Formal application roles tetap User, Administrator, dan Auditor. Support/Superadmin adalah permission profile, bukan wildcard role. Desktop dan Mobile/Tablet UAT personas tetap role User. Assistant bertindak on behalf of User; User tetap Actor. System/Service/Worker adalah Executor, bukan actor bisnis dan tidak memberi permission tambahan.

## 11.4 Triple Entry acceptance

> File-First, Tool-First, dan Assistant-First boleh berbeda pada discovery/presentation, tetapi harus menggunakan canonical Feature dan business rule yang sama. Input/order, permission/policy/capability, confirmation, processing mode, source/finality, result, error, History, dan recovery harus setara. Assistant tidak boleh mengarang Feature, parameter, confirmation, atau success.

## 11.5 Core coverage

> Seluruh 33 Primary PDF Tool Cards dipetakan ke satu atau beberapa Feature ID canonical. Kedalaman UAT berbasis risiko: Standard untuk Low-risk family yang stabil, Enhanced untuk Medium/PoC/device/fidelity dependency, dan Critical untuk High-risk/irreversible/content-sensitive action. PoC-gated Feature tidak dapat PASS sebelum approved implementation dan limitation tersedia.

## 11.6 Merge PDF acceptance

> `FEAT-PDF-005 — Merge PDF` menjadi UAT vertical slice paling detail. User harus dapat memilih atau drag/drop dua atau lebih PDF, melihat validation/order, reorder/remove/Undo, memulai proses lokal, memahami progress, cancel dengan aman, memeriksa result, Save As ke output baru, membuka ulang dari History, bekerja offline ketika eligible, dan pulih dari error/interruption tanpa merusak source. Hasil atomik harus memuat seluruh halaman sesuai final order. Merge tidak memakai server processing/API/SignalR.

## 11.7 High-risk acceptance

> Protect, Unlock, Redact, Visual Sign, Amankan ke Akun, Permanent Delete, AI Summarizer, dan Translate PDF mempunyai UAT tambahan. Unlock hanya berhasil dengan password/otorisasi sah. Redact harus menghilangkan content secara nyata, bukan overlay. Sign PDF tetap visual non-PKI. Secured storage hanya untuk selected result melalui tindakan explicit. Permanent Delete membedakan local dan account lifecycle. AI/translation memakai approved local/enterprise-internal provider, menampilkan limitation, dan tetap grounded pada source.

## 11.8 Business error and recovery

> Recovery PASS hanya jika user mengetahui state sebenarnya, source/target aman, partial/temp tidak dianggap final, retry/resume tidak menggandakan effect, permission/policy/capability direvalidasi bila perlu, dan user memperoleh pilihan Lanjutkan/Coba Lagi/Batalkan yang sesuai. Aplikasi yang kembali terbuka tanpa business state yang benar bukan recovery success.

## 11.9 Scenario acceptance

> Setiap UAT scenario memiliki UAT ID, Requirement ID, Feature ID/resource, Actor, business goal, precondition, steps user, expected result, Critical UX, risk/depth, forbidden outcome, evidence, dan status. Status yang valid adalah PASS, FAIL, BLOCKED, NOT_RUN, INCONCLUSIVE, dan N/A. Hanya PASS yang berarti scenario telah dijalankan dan seluruh business acceptance terpenuhi.

## 11.10 Exit and sign-off

> UAT selesai ketika seluruh released/applicable Must/Core scenario memiliki hasil final; seluruh D3/high-risk, Triple Entry critical parity, dan Merge detailed UAT PASS; tidak ada open Blocker/Critical atau Major yang menggagalkan business outcome/safety; known issue mempunyai governed disposition; traceability/evidence lengkap; dan approver resmi menandatangani scope, limitation, defects, risk, serta outcome. Tidak ada pass-rate percentage yang menggantikan critical acceptance.

## 11.11 Suggested checklist

```text
[ ] immutable RC dan UAT entry package terverifikasi
[ ] actor/accounts/permissions/platform/data sesuai scope
[ ] File-First, Tool-First, Assistant-First parity PASS
[ ] 33/33 Core mempunyai result/disposition yang sah
[ ] Merge detailed UAT PASS
[ ] high-risk applicable UAT PASS
[ ] source/target/result/finality acceptance PASS
[ ] error/cancel/offline/recovery acceptance PASS
[ ] platform-natural UX dan critical accessibility scope PASS
[ ] PoC limitation dinyatakan dan diterima secara eksplisit
[ ] no open Blocker/Critical/disallowed Major
[ ] defects/retests/known issues terdokumentasi
[ ] Requirement → Feature → UAT → evidence traceability lengkap
[ ] business/UAT sign-off authority menandatangani final outcome
```

---

**Kesimpulan Agent A:** UAT yang direkomendasikan berpusat pada keberhasilan kerja user, bukan implementation detail. Breadth dicapai melalui satu matrix 33 Core yang canonical; depth diberikan pada Merge dan delapan area berisiko tinggi. Hasil laporan ini adalah spesifikasi UAT pre-execution—bukan klaim bahwa Feature, UAT, atau Production telah lulus.
