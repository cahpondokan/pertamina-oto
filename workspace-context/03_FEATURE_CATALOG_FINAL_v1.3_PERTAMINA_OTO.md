# 03 — KATALOG FITUR
## PERTAMINA OTO — Office Tools Organizer

> **Standar bahasa:** Bahasa Indonesia — ID-1.0

---

## 1. Informasi Dokumen

| Atribut | Nilai |
|---|---|
| Nama dokumen | `03_FEATURE_CATALOG.md` |
| Nama proyek | **PERTAMINA OTO — Office Tools Organizer** |
| Versi | **1.3** |
| Status | **FINAL — SOLUTION-ALIGNED FEATURE BASELINE** |
| Dokumen induk | `01_BUSINESS_PROCESS_FINAL_v3.4_PERTAMINA_OTO.md`; `02_REQUIREMENT_SPECIFICATION_FINAL_v1.5_PERTAMINA_OTO.md` |
| Model katalog | **Canonical Feature Catalog + Traceability** |
| Raw inventory sebelum audit | **557** capability entries |
| Tambahan hasil audit | **12** capability entries |
| Raw candidate setelah audit + Solution Design additions | **580** |
| Canonical record setelah dedup | **550** |
| Digital Signature PKI | **Out of scope** |
| Assistant | **Offline-First Hybrid Conversational Orchestrator** |
| Primary PDF Tool Surface | **33 Core Tool Cards** |
| Tambahan Primary Feature Audit | **4 canonical capabilities** |

### 1.1 Prinsip Canonicalization

- Satu capability/contract resmi mempunyai satu Feature ID canonical.
- Label/entry yang benar-benar sama digabung sebagai alias/cross-reference, bukan diimplementasikan ulang.
- Capability yang tampak mirip tetapi mempunyai input/output, risiko, lifecycle, atau UX berbeda tetap menjadi record terpisah.
- **Implementation Family** mencegah service/engine ganda walaupun beberapa capability menggunakan engine yang sama.
- Keterbatasan platform tidak menghapus fitur; availability mengikuti capability dan execution profile.
- Assistant hanya memanggil Feature ID canonical dan tidak membuat business rule paralel.

### 1.2 Riwayat Revisi

| Versi | Perubahan |
|---|---|
| 1.0 | Final pre-solution-design baseline: 539 canonical records |
| 1.1 | Menambahkan Desktop-First/Curated Mobile platform-exposure policy dan 7 capability In-App Issue Reporting/Support berdasarkan audit Solution Design; jumlah record dicatat sebagai baseline historis versi tersebut |
| 1.2 | Controlled revision: menambahkan Initial Implementation Wave/Common-Simple-First. Tidak ada Feature ID baru/dihapus pada revisi tersebut |
| 1.3 | Primary Feature Audit: menetapkan 33 Core Tool Cards, menambahkan Sign PDF visual, AI Summarizer, Translate PDF, dan PDF to Markdown; canonical count menjadi 550 |

---

## 2. Tipe Record

| Record Type | Makna |
|---|---|
| `USER_TOOL` | Tool yang secara langsung menghasilkan/manipulasi file atau data user |
| `WORKFLOW_CAPABILITY` | Capability alur kerja, state, recovery, processing, atau storage |
| `ENTERPRISE_CAPABILITY` | Account, admin, policy, audit, dan support |
| `PLATFORM_CAPABILITY` | UX, localization, progress, dan kemampuan platform |
| `SECURITY_CONTROL` | Kontrol keamanan yang wajib diverifikasi |
| `OBSERVABILITY_CAPABILITY` | Telemetry, health, metric, dan alert |
| `ASSISTANT_CAPABILITY` | Intent, planning, orchestration, dan AI routing Assistant |

## 3. Eksposur UI

| Nilai | Makna |
|---|---|
| `PRIMARY_TOOL` | Dapat muncul sebagai tool utama |
| `CONTEXT_ACTION` | Muncul sesuai file/konteks/workflow |
| `ASSISTANT` | Capability Assistant |
| `ADMIN` | UI administrator umum |
| `SUPERADMIN` | Halaman support/operasional khusus yang memerlukan permission Superadmin/Support |
| `AUDITOR/ADMIN` | Audit/compliance |
| `BACKGROUND/CONTEXT` | Feedback/platform behavior, bukan menu tool |
| `SYSTEM_INTERNAL` | Tidak diekspos sebagai tombol langsung |

## 4. Profil Eksekusi

| Profile | Processing | Server Dependency | Offline | Storage | Network | Concurrency |
|---|---|---|---|---|---|---|
| `L1` | Local preferred, server fallback bila perlu | Optional | Full* | Local temp | None/Low | Low server |
| `H1` | Hybrid/capability-aware untuk engine kompleks | Optional | Partial | Local temp / optional server temp | Low–High | Medium–High when server |
| `L-SYS` | Local/system behavior | None | Full | Local metadata/temp | None/Low | Low server |
| `SYS-H` | System hybrid | Optional | Partial | Metadata/temp | None–Low | Medium |
| `E1` | Enterprise/server metadata/file function | Required | Partial | Metadata/File by feature | Low–Medium | Medium |
| `SEC1` | Security control | Hybrid | Partial | Metadata/evidence | Low–Medium | Medium |
| `AI-H` | Local deterministic Assistant + optional internal AI | Optional | Full basic / Partial advanced | Local minimal conversation state | None–Medium | Low–Medium |
| `AI-L` | Optional local model | None | Full | Local model/cache | None | Device-bound |
| `AI-S` | Internal AI service | Required | No | Minimal approved request/content | Medium | High controlled |
| `FUTURE` | Belum menjadi implementation baseline | TBD | TBD | TBD | TBD | TBD |

> `Full*` berarti penuh selama engine lokal, policy, dan capability platform mendukung.

---



### 4.1 Permukaan Primary PDF Tool — 33 Core Tools

Bagian ini mengatur **prioritas produk dan urutan implementasi user-facing**, bukan mengganti canonicalization. Primary tool card dapat:
- menunjuk satu canonical Feature ID; atau
- menjadi workflow card yang membuka beberapa capability canonical tanpa membuat processing engine duplikat.

**Rule:** Core Feature Set dikerjakan sebelum user-tool sekunder. Common-Simple-First tetap digunakan **di dalam Core Set**. Fitur yang gated diprioritaskan Proof-of-Capability-nya dan tidak boleh dianggap selesai sebelum gate lulus.

| No | Kelompok | Primary Tool | Canonical Mapping | Catatan |
|---:|---|---|---|---|
| 1 | ORGANIZE PDF | **Merge PDF** | `FEAT-PDF-005` | Direct |
| 2 | ORGANIZE PDF | **Split PDF** | `FEAT-PDF-006 / FEAT-PDF-007` | Direct + page-range |
| 3 | ORGANIZE PDF | **Remove pages** | `FEAT-PDF-013` | Direct |
| 4 | ORGANIZE PDF | **Extract pages** | `FEAT-PDF-014` | Direct |
| 5 | ORGANIZE PDF | **Organize PDF** | `FEAT-PDF-010..016` | Workflow card / bundle |
| 6 | ORGANIZE PDF | **Scan to PDF** | `FEAT-SCAN-014` | Direct; physical scanner remains PoC-gated |
| 7 | OPTIMIZE PDF | **Compress PDF** | `FEAT-PDF-023` | Direct |
| 8 | OPTIMIZE PDF | **Repair PDF** | `FEAT-PDF-025 / FEAT-PDF-026` | Primary + advanced fallback |
| 9 | OPTIMIZE PDF | **OCR PDF** | `FEAT-OCR-004` | Searchable PDF OCR |
| 10 | CONVERT TO PDF | **JPG to PDF** | `FEAT-CONV-007` | Direct |
| 11 | CONVERT TO PDF | **Word to PDF** | `FEAT-CONV-001` | Feasibility gate |
| 12 | CONVERT TO PDF | **PowerPoint to PDF** | `FEAT-CONV-003` | Feasibility gate |
| 13 | CONVERT TO PDF | **Excel to PDF** | `FEAT-CONV-002` | Feasibility gate |
| 14 | CONVERT TO PDF | **HTML to PDF** | `FEAT-CONV-017` | Feasibility gate |
| 15 | CONVERT FROM PDF | **PDF to JPG** | `FEAT-CONV-012` | Direct |
| 16 | CONVERT FROM PDF | **PDF to Word** | `FEAT-CONV-004` | Feasibility gate |
| 17 | CONVERT FROM PDF | **PDF to PowerPoint** | `FEAT-CONV-006` | Feasibility gate |
| 18 | CONVERT FROM PDF | **PDF to Excel** | `FEAT-CONV-005` | Feasibility gate |
| 19 | CONVERT FROM PDF | **PDF to PDF/A** | `FEAT-PDF-075` | Feasibility gate |
| 20 | EDIT PDF | **Rotate PDF** | `FEAT-PDF-012` | Direct |
| 21 | EDIT PDF | **Add page numbers** | `FEAT-PDF-037` | Direct |
| 22 | EDIT PDF | **Add watermark** | `FEAT-PDF-036` | Direct |
| 23 | EDIT PDF | **Crop PDF** | `FEAT-PDF-019` | Direct |
| 24 | EDIT PDF | **Edit PDF** | `FEAT-PDF-031..041` | Workflow card; existing-text edit remains gated |
| 25 | EDIT PDF | **PDF Forms** | `FEAT-PDF-053..060` | Workflow card / bundle |
| 26 | PDF SECURITY | **Unlock PDF** | `FEAT-PDF-062` | Valid password/authority only |
| 27 | PDF SECURITY | **Protect PDF** | `FEAT-PDF-061` | Direct |
| 28 | PDF SECURITY | **Sign PDF** | `FEAT-SIGN-013` | NEW; visual signature/paraf non-PKI only |
| 29 | PDF SECURITY | **Redact PDF** | `FEAT-PDF-069` | High-risk finalization |
| 30 | PDF SECURITY | **Compare PDF** | `FEAT-PDF-071 / FEAT-PDF-072` | Direct |
| 31 | PDF INTELLIGENCE | **AI Summarizer** | `FEAT-INTEL-001` | NEW; approved local/internal AI only |
| 32 | PDF INTELLIGENCE | **Translate PDF** | `FEAT-INTEL-002` | NEW; approved local/internal provider only |
| 33 | PDF INTELLIGENCE | **PDF to Markdown** | `FEAT-CONV-036` | NEW; deterministic extraction first |

#### Sequencing Teknis di Dalam Core Set

1. **Vertical Slice pertama:** `FEAT-PDF-005 — Merge PDF`.
2. Lanjutkan Core tool low-risk/local yang reuse fondasi PDF yang sama.
3. Jalankan Proof-of-Capability sedini mungkin untuk Office conversion, PDF→Office, HTML→PDF, PDF/A, existing-text editing, physical scanner, dan PDF Intelligence provider.
4. High-risk security tools seperti Unlock/Redact harus mempunyai security acceptance khusus.
5. User-tool sekunder dimulai setelah Core Set selesai atau setiap blocker mempunyai keputusan resmi.

#### Aturan UI

Tujuh kelompok utama:
`ORGANIZE PDF`, `OPTIMIZE PDF`, `CONVERT TO PDF`, `CONVERT FROM PDF`, `EDIT PDF`, `PDF SECURITY`, `PDF INTELLIGENCE`.

Core card adalah surface utama Tool-First. File-First dan Assistant-First tetap mengarah ke canonical Feature ID yang sama.

---

## 5. Record Feature Canonical

### 5.1 PDF Tools — Organize & Viewer

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-PDF-001` | **Buka/Read PDF**<br>Open/Read PDF | Membuka dan membaca file PDF tanpa mengubah file sumber. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-002` | **Preview PDF**<br>PDF Preview | Menampilkan pratinjau halaman sebelum edit atau proses. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-003` | **Navigasi Halaman PDF**<br>PDF Page Navigation | Berpindah antarhalaman dengan cepat. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-004` | **Zoom PDF**<br>PDF Zoom | Memperbesar atau memperkecil tampilan dokumen. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-005` | **Merge PDF**<br>Merge PDF | Menggabungkan dua atau lebih PDF menjadi satu file. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-006` | **Split PDF**<br>Split PDF | Memisahkan satu PDF menjadi beberapa file. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-007` | **Split by Page Range**<br>Split by Page Range | Memisahkan PDF berdasarkan rentang halaman yang dipilih. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-008` | **Split Every N Pages**<br>Split Every N Pages | Memisahkan PDF otomatis setiap jumlah halaman tertentu. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-009` | **Split Double-Page Scan**<br>Split Double-Page Scan | Memecah hasil scan dua halaman dalam satu lembar menjadi halaman terpisah. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-010` | **Reorder Pages**<br>Reorder Pages | Mengubah urutan halaman PDF. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-011` | **Move Page**<br>Move Page | Memindahkan halaman ke posisi tertentu. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-012` | **Rotate Page**<br>Rotate Page | Memutar satu atau beberapa halaman. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-013` | **Delete/Remove Page**<br>Delete/Remove Page | Menghapus halaman dari working state tanpa merusak file asli. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-014` | **Extract Pages**<br>Extract Pages | Mengambil halaman tertentu menjadi PDF baru. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-015` | **Duplicate Page**<br>Duplicate Page | Menggandakan halaman tertentu. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-016` | **Add Blank Page**<br>Add Blank Page | Menambahkan halaman kosong. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-017` | **Normalize Page Size**<br>Normalize Page Size | Menyeragamkan ukuran halaman dalam satu dokumen. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-018` | **Resize PDF Page**<br>Resize PDF Page | Mengubah ukuran halaman PDF. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-019` | **Crop PDF Page**<br>Crop PDF Page | Memotong area halaman PDF. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-020` | **N-up / Pages per Sheet**<br>N-up / Pages per Sheet | Menggabungkan beberapa halaman PDF ke dalam satu lembar. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-021` | **Detect Blank Pages**<br>Detect Blank Pages | Mendeteksi halaman kosong secara otomatis. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-022` | **Remove Blank Pages**<br>Remove Blank Pages | Menghapus halaman kosong yang terdeteksi setelah preview. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |

### 5.2 PDF Tools — Optimize/Edit/Extract/Form/Security

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-PDF-023` | **Compress PDF**<br>Compress PDF | Mengurangi ukuran file PDF dengan menjaga kualitas sesuai preset. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-024` | **Optimize PDF**<br>Optimize PDF | Mengoptimalkan struktur dan aset PDF agar lebih efisien. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-025` | **Repair PDF**<br>Repair PDF | Mencoba memperbaiki PDF yang rusak atau tidak konsisten. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-026` | **Advanced PDF Repair**<br>Advanced PDF Repair | Menjalankan perbaikan struktur PDF lebih mendalam bila engine mendukung. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `H1` | `LOW` | Should / R2 | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-027` | **Flatten PDF**<br>Flatten PDF | Meratakan form, anotasi, atau layer menjadi konten tetap. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-028` | **Fast Web View**<br>Fast Web View | Mengoptimalkan PDF agar dapat dimuat bertahap lebih cepat. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-029` | **Reduce Embedded Image Quality**<br>Reduce Embedded Image Quality | Menurunkan kualitas gambar di PDF untuk mengurangi ukuran. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-030` | **Optimize Embedded Images**<br>Optimize Embedded Images | Mengoptimalkan gambar tertanam tanpa mengubah konten utama. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-031` | **Add Text to PDF**<br>Add Text to PDF | Menambahkan teks baru ke halaman PDF. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-032` | **Edit Existing PDF Text**<br>Edit Existing PDF Text | Mengedit teks yang sudah ada bila struktur PDF dan engine mendukung. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `H1` | `MEDIUM` | Should / R2 | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `FEASIBILITY_GATE` | — |
| `FEAT-PDF-033` | **Add Image to PDF**<br>Add Image to PDF | Menambahkan gambar ke halaman PDF. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-034` | **Add Shape to PDF**<br>Add Shape to PDF | Menambahkan bentuk seperti kotak atau lingkaran. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-035` | **Add Line/Arrow to PDF**<br>Add Line/Arrow to PDF | Menambahkan garis atau panah. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-036` | **Add Watermark**<br>Add Watermark | Menambahkan watermark teks/gambar ke PDF. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-037` | **Add Page Number**<br>Add Page Number | Menambahkan nomor halaman dengan posisi dan format yang dipilih. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-038` | **Add Header**<br>Add Header | Menambahkan header pada halaman. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-039` | **Add Footer**<br>Add Footer | Menambahkan footer pada halaman. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-040` | **Overlay PDF**<br>Overlay PDF | Menumpuk satu PDF di atas PDF lain. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-041` | **Underlay PDF**<br>Underlay PDF | Menempatkan PDF sebagai layer bawah. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-042` | **Add Index**<br>Add Index | Membuat indeks dokumen sesuai data yang tersedia. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-043` | **Create Table of Contents**<br>Create Table of Contents | Membuat daftar isi dari struktur/bookmark. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-044` | **Bookmark Manager**<br>Bookmark Manager | Membuat, mengubah, menghapus, dan mengurutkan bookmark. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-045` | **Edit PDF Metadata**<br>Edit PDF Metadata | Mengubah judul, author, subject, keyword, dan metadata lain. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-046` | **Viewer Preferences**<br>Viewer Preferences | Mengatur preferensi tampilan ketika PDF dibuka. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-047` | **Extract Text from PDF**<br>Extract Text from PDF | Mengambil teks dari PDF yang memiliki text layer. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-048` | **Extract Images from PDF**<br>Extract Images from PDF | Mengambil gambar dari PDF. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-049` | **Extract Embedded Images**<br>Extract Embedded Images | Mengambil aset gambar tertanam tanpa render ulang bila memungkinkan. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-050` | **Extract Tables from PDF**<br>Extract Tables from PDF | Mengekstrak tabel menjadi struktur data. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `H1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-051` | **Extract Attachments**<br>Extract Attachments | Mengambil lampiran tertanam di PDF. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-052` | **Attachment Manager**<br>Attachment Manager | Menambah, melihat, mengambil, atau menghapus attachment PDF. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-053` | **Fill PDF Form**<br>Fill PDF Form | Mengisi field form PDF. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-054` | **Create PDF Form**<br>Create PDF Form | Membuat form PDF dasar. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / R2 | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-055` | **Add Text Field**<br>Add Text Field | Menambahkan field teks ke form. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-056` | **Add Checkbox**<br>Add Checkbox | Menambahkan checkbox. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-057` | **Add Radio Button**<br>Add Radio Button | Menambahkan radio button. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-058` | **Add Dropdown**<br>Add Dropdown | Menambahkan dropdown. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-059` | **Add Signature Field**<br>Add Signature Field | Menambahkan area tanda tangan visual/paraf non-PKI. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-060` | **Form Flattening**<br>Form Flattening | Mengubah form yang sudah diisi menjadi konten tetap. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-061` | **Protect PDF**<br>Protect PDF | Melindungi PDF menggunakan password/permission. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `MEDIUM` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-062` | **Unlock PDF**<br>Unlock PDF | Membuka proteksi menggunakan password yang sah. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `HIGH` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-063` | **Set PDF Password**<br>Set PDF Password | Menambahkan password pada PDF. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `MEDIUM` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-064` | **Remove PDF Password**<br>Remove PDF Password | Menghapus password setelah password yang benar diberikan. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `HIGH` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-065` | **PDF Permissions**<br>PDF Permissions | Mengatur izin print, copy, dan edit sesuai mekanisme PDF yang didukung. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `MEDIUM` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-066` | **Restrict Print**<br>Restrict Print | Membatasi izin pencetakan sesuai mekanisme PDF yang didukung; bukan DRM absolut. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-067` | **Restrict Copy**<br>Restrict Copy | Membatasi izin penyalinan sesuai mekanisme PDF yang didukung; bukan DRM absolut. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-068` | **Restrict Edit**<br>Restrict Edit | Membatasi izin pengeditan sesuai mekanisme PDF yang didukung; bukan DRM absolut. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-069` | **Redact PDF**<br>Redact PDF | Menghilangkan informasi sensitif secara permanen saat finalisasi. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `HIGH` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-070` | **Security Watermark**<br>Security Watermark | Memberikan watermark keamanan/identifikasi. | `USER_TOOL` | `CONTEXT_ACTION` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |

### 5.3 PDF Tools — Advanced

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-PDF-071` | **Compare PDF**<br>Compare PDF | Membandingkan dua PDF dan menunjukkan perbedaannya. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-072` | **Compare PDF Pages**<br>Compare PDF Pages | Membandingkan halaman tertentu antar-PDF. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-073` | **Batch PDF Processing**<br>Batch PDF Processing | Menjalankan proses yang sama terhadap banyak PDF. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-074` | **Document Information**<br>Document Information | Menampilkan halaman, ukuran, metadata, proteksi, dan informasi PDF. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `PLANNED` | — |
| `FEAT-PDF-075` | **PDF/A Conversion**<br>PDF/A Conversion | Mengonversi PDF ke PDF/A bila engine dan kebijakan mendukung. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→PDF/VIEW/DATA` | `H1` | `MEDIUM` | Could / R3 | FR-PROC-001..005; UIR-UI-005..010 | `PDF_ENGINE` | `FEASIBILITY_GATE` | — |

### 5.4 Document Conversion — Office & PDF

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-CONV-001` | **Word/DOCX to PDF**<br>Word/DOCX to PDF | Mengonversi Word menjadi PDF. | `USER_TOOL` | `PRIMARY_TOOL` | `DOC/IMG/PDF/DATA→TARGET` | `H1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `FEASIBILITY_GATE` | — |
| `FEAT-CONV-002` | **Excel/XLSX to PDF**<br>Excel/XLSX to PDF | Mengonversi workbook atau sheet Excel menjadi PDF. | `USER_TOOL` | `PRIMARY_TOOL` | `DOC/IMG/PDF/DATA→TARGET` | `H1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `FEASIBILITY_GATE` | — |
| `FEAT-CONV-003` | **PowerPoint/PPTX to PDF**<br>PowerPoint/PPTX to PDF | Mengonversi presentasi PowerPoint menjadi PDF. | `USER_TOOL` | `PRIMARY_TOOL` | `DOC/IMG/PDF/DATA→TARGET` | `H1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `FEASIBILITY_GATE` | — |
| `FEAT-CONV-004` | **PDF to Word**<br>PDF to Word | Mengonversi PDF menjadi Word sejauh struktur memungkinkan. | `USER_TOOL` | `PRIMARY_TOOL` | `DOC/IMG/PDF/DATA→TARGET` | `H1` | `LOW` | Should / R2 | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `FEASIBILITY_GATE` | — |
| `FEAT-CONV-005` | **PDF to Excel**<br>PDF to Excel | Mengonversi tabel/data PDF menjadi Excel. | `USER_TOOL` | `PRIMARY_TOOL` | `DOC/IMG/PDF/DATA→TARGET` | `H1` | `LOW` | Should / R2 | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `FEASIBILITY_GATE` | — |
| `FEAT-CONV-006` | **PDF to PowerPoint**<br>PDF to PowerPoint | Mengonversi halaman PDF menjadi presentasi. | `USER_TOOL` | `PRIMARY_TOOL` | `DOC/IMG/PDF/DATA→TARGET` | `H1` | `LOW` | Should / R2 | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `FEASIBILITY_GATE` | — |

### 5.5 Document Conversion — PDF & Image

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-CONV-007` | **JPG to PDF**<br>JPG to PDF | Mengubah JPG menjadi PDF. | `USER_TOOL` | `PRIMARY_TOOL` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-008` | **PNG to PDF**<br>PNG to PDF | Mengubah PNG menjadi PDF. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-009` | **WebP to PDF**<br>WebP to PDF | Mengubah WebP menjadi PDF. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-010` | **BMP to PDF**<br>BMP to PDF | Mengubah BMP menjadi PDF. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-011` | **Multiple Images to PDF**<br>Multiple Images to PDF | Menggabungkan banyak gambar menjadi satu PDF. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-012` | **PDF to JPG**<br>PDF to JPG | Mengonversi halaman PDF menjadi JPG. | `USER_TOOL` | `PRIMARY_TOOL` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-013` | **PDF to PNG**<br>PDF to PNG | Mengonversi halaman PDF menjadi PNG. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-014` | **PDF to WebP**<br>PDF to WebP | Mengonversi halaman PDF menjadi WebP. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |

### 5.6 Document Conversion — Text & Web

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-CONV-015` | **TXT to PDF**<br>TXT to PDF | Mengubah teks menjadi PDF. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-016` | **PDF to TXT**<br>PDF to TXT | Mengambil text layer PDF ke TXT. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-017` | **HTML to PDF**<br>HTML to PDF | Mengonversi HTML menjadi PDF. | `USER_TOOL` | `PRIMARY_TOOL` | `DOC/IMG/PDF/DATA→TARGET` | `H1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `FEASIBILITY_GATE` | — |
| `FEAT-CONV-018` | **Web Page to PDF**<br>Web Page to PDF | Menyimpan halaman web sebagai PDF bila kebijakan mengizinkan. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `H1` | `LOW` | Should / R2 | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `FEASIBILITY_GATE` | — |
| `FEAT-CONV-036` | **PDF to Markdown**<br>PDF to Markdown | Mengekstrak struktur teks PDF menjadi Markdown; text layer diproses deterministik dan OCR digunakan bila diperlukan. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→MARKDOWN` | `H1` | `LOW` | Should / MVP | FR-INTEL-003; FR-PROC-001..005 | `CONVERSION_ENGINE` | `PLANNED` | Structured Text Export |

### 5.7 Document Conversion — Spreadsheet & Data

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-CONV-019` | **CSV to Excel**<br>CSV to Excel | Mengubah CSV menjadi workbook Excel. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-020` | **Excel to CSV**<br>Excel to CSV | Mengubah sheet Excel menjadi CSV. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-021` | **PDF Table to Excel**<br>PDF Table to Excel | Mengambil tabel PDF langsung ke Excel. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-022` | **Extract Table to CSV**<br>Extract Table to CSV | Mengekstrak tabel menjadi CSV. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |

### 5.8 Document Conversion — OCR Conversion

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-CONV-023` | **Image to Text**<br>Image to Text | Mengambil teks dari gambar melalui OCR. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-024` | **Scan to Word**<br>Scan to Word | Mengubah hasil scan menjadi Word. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `H1` | `LOW` | Should / R2 | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-025` | **Scan to Excel**<br>Scan to Excel | Mengubah tabel scan menjadi Excel. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `H1` | `LOW` | Should / R2 | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |

### 5.9 Document Conversion — Batch & Options

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-CONV-026` | **Multi-file Conversion**<br>Multi-file Conversion | Mengonversi banyak file dalam satu antrean. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `MEDIUM` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-027` | **Batch Word to PDF**<br>Batch Word to PDF | Mengonversi banyak Word ke PDF. | `USER_TOOL` | `PRIMARY_TOOL` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-028` | **Batch Image to PDF**<br>Batch Image to PDF | Mengonversi banyak gambar ke PDF. | `USER_TOOL` | `PRIMARY_TOOL` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-029` | **Batch PDF to Image**<br>Batch PDF to Image | Mengonversi banyak PDF ke gambar. | `USER_TOOL` | `PRIMARY_TOOL` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-030` | **Page Range Conversion**<br>Page Range Conversion | Memilih halaman tertentu untuk dikonversi. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `MEDIUM` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-031` | **Sheet Selection**<br>Sheet Selection | Memilih sheet Excel. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-032` | **Slide Selection**<br>Slide Selection | Memilih slide PowerPoint. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-033` | **Conversion Orientation**<br>Conversion Orientation | Menentukan portrait/landscape. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `MEDIUM` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-034` | **Conversion Paper Size**<br>Conversion Paper Size | Menentukan ukuran kertas output. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `MEDIUM` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |
| `FEAT-CONV-035` | **Conversion Quality**<br>Conversion Quality | Menentukan kualitas/resolusi output. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG/PDF/DATA→TARGET` | `L1` | `MEDIUM` | Should / MVP | FR-PROC-001..005; BR-GOV-006..007 | `CONVERSION_ENGINE` | `PLANNED` | — |


### 5.9A PDF Intelligence — AI & Language

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-INTEL-001` | **AI Summarizer**<br>AI Summarizer | Membuat ringkasan isi PDF menggunakan local/enterprise-internal provider yang disetujui; text extraction/OCR menjadi preprocessing bila perlu. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF→TEXT/DATA` | `AI-H` | `MEDIUM` | Should / MVP | FR-INTEL-001; SR-SEC-021 | `CONTENT_INTELLIGENCE_PROVIDER` | `FEASIBILITY_GATE` | PDF Summary |
| `FEAT-INTEL-002` | **Translate PDF**<br>Translate PDF | Menerjemahkan konten PDF ke bahasa target dengan provider yang disetujui dan data-minimization. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF+LANG→PDF/TEXT/DATA` | `AI-H` | `MEDIUM` | Should / MVP | FR-INTEL-002; SR-SEC-021 | `CONTENT_INTELLIGENCE_PROVIDER` | `FEASIBILITY_GATE` | PDF Translation |

### 5.10 Image Tools — Basic

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-IMG-001` | **Image Viewer**<br>Image Viewer | Membuka dan melihat gambar. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-002` | **Crop Image**<br>Crop Image | Memotong area gambar. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-003` | **Rotate Image**<br>Rotate Image | Memutar gambar. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-004` | **Flip Horizontal**<br>Flip Horizontal | Membalik gambar horizontal. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-005` | **Flip Vertical**<br>Flip Vertical | Membalik gambar vertikal. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-006` | **Resize Image**<br>Resize Image | Mengubah dimensi/resolusi. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-007` | **Compress Image**<br>Compress Image | Mengurangi ukuran file gambar. | `USER_TOOL` | `PRIMARY_TOOL` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-008` | **Image Quality Adjustment**<br>Image Quality Adjustment | Mengatur tingkat kualitas kompresi. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |

### 5.11 Image Tools — Conversion

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-IMG-009` | **JPG to PNG**<br>JPG to PNG | Mengubah JPG menjadi PNG. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-010` | **PNG to JPG**<br>PNG to JPG | Mengubah PNG menjadi JPG. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-011` | **JPG to WebP**<br>JPG to WebP | Mengubah JPG menjadi WebP. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-012` | **WebP to JPG**<br>WebP to JPG | Mengubah WebP menjadi JPG. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-013` | **PNG to WebP**<br>PNG to WebP | Mengubah PNG menjadi WebP. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-014` | **WebP to PNG**<br>WebP to PNG | Mengubah WebP menjadi PNG. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-015` | **BMP Image Conversion**<br>BMP Image Conversion | Mengonversi BMP ke/dari format yang didukung. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `MEDIUM` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-016` | **Batch Image Format Conversion**<br>Batch Image Format Conversion | Mengubah format banyak gambar sekaligus. | `USER_TOOL` | `PRIMARY_TOOL` | `IMAGE→IMAGE/DATA` | `L1` | `MEDIUM` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |

### 5.12 Image Tools — Adjustment

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-IMG-017` | **Brightness Adjustment**<br>Brightness Adjustment | Mengatur kecerahan. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-018` | **Contrast Adjustment**<br>Contrast Adjustment | Mengatur kontras. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-019` | **Grayscale Image**<br>Grayscale Image | Mengubah gambar menjadi grayscale. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-020` | **Basic Color Adjustment**<br>Basic Color Adjustment | Melakukan penyesuaian warna dasar. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-021` | **Image Quality Optimization**<br>Image Quality Optimization | Mengoptimalkan ukuran/kualitas gambar. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |

### 5.13 Image Tools — Background

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-IMG-022` | **Background Remover**<br>Background Remover | Menghapus background. | `USER_TOOL` | `PRIMARY_TOOL` | `IMAGE→IMAGE/DATA` | `H1` | `MEDIUM` | Should / R2 | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `FEASIBILITY_GATE` | — |
| `FEAT-IMG-023` | **Transparent Background**<br>Transparent Background | Membuat background transparan. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-024` | **Replace Background**<br>Replace Background | Mengganti background. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-025` | **Solid Color Background**<br>Solid Color Background | Menggunakan background warna polos. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |

### 5.14 Image Tools — Annotation

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-IMG-026` | **Add Text to Image**<br>Add Text to Image | Menambahkan teks. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-027` | **Add Arrow to Image**<br>Add Arrow to Image | Menambahkan panah. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-028` | **Add Line to Image**<br>Add Line to Image | Menambahkan garis. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-029` | **Add Shape to Image**<br>Add Shape to Image | Menambahkan bentuk. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-030` | **Image Watermark**<br>Image Watermark | Menambahkan watermark. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-031` | **Blur Area**<br>Blur Area | Memburamkan area tertentu. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-032` | **Censor Area**<br>Censor Area | Menutup bagian tertentu. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-033` | **Image Redaction**<br>Image Redaction | Menghilangkan area sensitif pada hasil final. | `USER_TOOL` | `PRIMARY_TOOL` | `IMAGE→IMAGE/DATA` | `L1` | `HIGH` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-034` | **Add Visual Signature to Image**<br>Add Visual Signature to Image | Menempatkan tanda tangan visual/paraf non-PKI. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |

### 5.15 Image Tools — Composition

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-IMG-035` | **Merge Images Vertically**<br>Merge Images Vertically | Menggabungkan gambar secara vertikal. | `USER_TOOL` | `PRIMARY_TOOL` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-036` | **Merge Images Horizontally**<br>Merge Images Horizontally | Menggabungkan gambar secara horizontal. | `USER_TOOL` | `PRIMARY_TOOL` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-037` | **Image Collage**<br>Image Collage | Membuat kolase. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-038` | **Long Screenshot Merge**<br>Long Screenshot Merge | Menggabungkan screenshot menjadi gambar panjang. | `USER_TOOL` | `PRIMARY_TOOL` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-039` | **Formal Photo / Pas Foto**<br>Formal Photo / ID Photo | Menyiapkan foto formal dengan ukuran/background tertentu. | `USER_TOOL` | `PRIMARY_TOOL` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |

### 5.16 Image Tools — Information

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-IMG-040` | **Image Metadata Viewer**<br>Image Metadata Viewer | Menampilkan EXIF/metadata. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-041` | **Image Metadata Remover**<br>Image Metadata Remover | Menghapus metadata untuk privasi. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-042` | **Before/After Comparison**<br>Before/After Comparison | Membandingkan hasil sebelum dan sesudah edit. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |

### 5.17 Image Tools — Batch

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-IMG-043` | **Batch Image Resize**<br>Batch Image Resize | Resize banyak gambar. | `USER_TOOL` | `PRIMARY_TOOL` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-044` | **Batch Image Compress**<br>Batch Image Compress | Mengompres banyak gambar. | `USER_TOOL` | `PRIMARY_TOOL` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-045` | **Batch Image Watermark**<br>Batch Image Watermark | Memberi watermark ke banyak gambar. | `USER_TOOL` | `PRIMARY_TOOL` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |
| `FEAT-IMG-046` | **Batch Metadata Removal**<br>Batch Metadata Removal | Menghapus metadata banyak gambar. | `USER_TOOL` | `PRIMARY_TOOL` | `IMAGE→IMAGE/DATA` | `L1` | `LOW` | Should / MVP | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `PLANNED` | — |

### 5.18 Image Tools — Future AI

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-IMG-047` | **AI Image Enhancement — Future**<br>AI Image Enhancement | Peningkatan kualitas berbasis AI jika kelak disetujui. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `FUTURE` | `LOW` | Future / Future | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `FUTURE` | — |
| `FEAT-IMG-048` | **Advanced Restoration/Upscaling — Future**<br>Advanced Restoration/Upscaling | Restorasi/upscaling lanjutan sebagai roadmap. | `USER_TOOL` | `CONTEXT_ACTION` | `IMAGE→IMAGE/DATA` | `FUTURE` | `LOW` | Future / Future | FR-PROC-001..005; UIR-UI-008..010 | `IMAGE_ENGINE` | `FUTURE` | — |

### 5.19 Scanner & OCR — Scanner

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-SCAN-001` | **Camera Document Scanner**<br>Camera Document Scanner | Memindai dokumen menggunakan kamera, terutama mobile. | `USER_TOOL` | `PRIMARY_TOOL` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `MEDIUM` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `CAMERA_SCAN_ENGINE` | `PLANNED` | — |
| `FEAT-SCAN-002` | **Automatic Edge Detection**<br>Automatic Edge Detection | Mendeteksi tepi kertas. | `USER_TOOL` | `CONTEXT_ACTION` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `LOW` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `CAMERA_SCAN_ENGINE` | `PLANNED` | — |
| `FEAT-SCAN-003` | **Auto Crop Scan**<br>Auto Crop Scan | Memotong otomatis mengikuti batas dokumen. | `USER_TOOL` | `CONTEXT_ACTION` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `LOW` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `CAMERA_SCAN_ENGINE` | `PLANNED` | — |
| `FEAT-SCAN-004` | **Perspective Correction**<br>Perspective Correction | Meluruskan dokumen yang difoto miring. | `USER_TOOL` | `CONTEXT_ACTION` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `LOW` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `CAMERA_SCAN_ENGINE` | `PLANNED` | — |
| `FEAT-SCAN-005` | **Document Enhancement**<br>Document Enhancement | Meningkatkan keterbacaan scan. | `USER_TOOL` | `CONTEXT_ACTION` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `LOW` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `CAMERA_SCAN_ENGINE` | `PLANNED` | — |
| `FEAT-SCAN-006` | **Scan Filter**<br>Scan Filter | Memberikan filter dokumen. | `USER_TOOL` | `CONTEXT_ACTION` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `LOW` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `CAMERA_SCAN_ENGINE` | `PLANNED` | — |
| `FEAT-SCAN-007` | **Grayscale Scan**<br>Grayscale Scan | Menghasilkan scan grayscale. | `USER_TOOL` | `CONTEXT_ACTION` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `LOW` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `CAMERA_SCAN_ENGINE` | `PLANNED` | — |
| `FEAT-SCAN-008` | **Black & White Scan**<br>Black & White Scan | Menghasilkan scan hitam-putih. | `USER_TOOL` | `CONTEXT_ACTION` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `LOW` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `CAMERA_SCAN_ENGINE` | `PLANNED` | — |
| `FEAT-SCAN-009` | **Multi-page Scan**<br>Multi-page Scan | Memindai banyak halaman dalam satu sesi. | `USER_TOOL` | `CONTEXT_ACTION` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `LOW` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `CAMERA_SCAN_ENGINE` | `PLANNED` | — |
| `FEAT-SCAN-010` | **Rear Camera Scan**<br>Rear Camera Scan | Menggunakan kamera belakang bila tersedia. | `USER_TOOL` | `CONTEXT_ACTION` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `LOW` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `CAMERA_SCAN_ENGINE` | `PLANNED` | — |
| `FEAT-SCAN-011` | **Reorder Scanned Pages**<br>Reorder Scanned Pages | Mengubah urutan hasil scan. | `USER_TOOL` | `CONTEXT_ACTION` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `LOW` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `CAMERA_SCAN_ENGINE` | `PLANNED` | — |
| `FEAT-SCAN-012` | **Rescan Page**<br>Rescan Page | Mengambil ulang halaman tertentu. | `USER_TOOL` | `CONTEXT_ACTION` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `LOW` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `CAMERA_SCAN_ENGINE` | `PLANNED` | — |
| `FEAT-SCAN-013` | **Delete Scanned Page**<br>Delete Scanned Page | Menghapus halaman dari working state. | `USER_TOOL` | `CONTEXT_ACTION` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `LOW` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `CAMERA_SCAN_ENGINE` | `PLANNED` | — |
| `FEAT-SCAN-014` | **Scan to PDF**<br>Scan to PDF | Menyimpan scan sebagai PDF. | `USER_TOOL` | `PRIMARY_TOOL` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `LOW` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `CAMERA_SCAN_ENGINE` | `PLANNED` | — |
| `FEAT-SCAN-015` | **Scan to Image**<br>Scan to Image | Menyimpan scan sebagai gambar. | `USER_TOOL` | `CONTEXT_ACTION` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `LOW` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `CAMERA_SCAN_ENGINE` | `PLANNED` | — |

### 5.20 Scanner & OCR — Desktop

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-SCAN-016` | **Import Scanned Image**<br>Import Scanned Image | Mengimpor gambar hasil scan. | `USER_TOOL` | `CONTEXT_ACTION` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `LOW` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `CAMERA_SCAN_ENGINE` | `PLANNED` | — |
| `FEAT-SCAN-017` | **Import Scanned PDF**<br>Import Scanned PDF | Mengimpor PDF scan. | `USER_TOOL` | `CONTEXT_ACTION` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `LOW` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `CAMERA_SCAN_ENGINE` | `PLANNED` | — |
| `FEAT-SCAN-018` | **Physical Scanner Support — Optional**<br>Physical Scanner Support | Integrasi scanner fisik bila driver/library disetujui. | `USER_TOOL` | `PRIMARY_TOOL` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `MEDIUM` | Could / R3 | FR-SCAN-001..002; FR-PROC-001..005 | `CAMERA_SCAN_ENGINE` | `FEASIBILITY_GATE` | — |
| `FEAT-OCR-011` | **Desktop OCR**<br>Desktop OCR | OCR file pada desktop. | `USER_TOOL` | `PRIMARY_TOOL` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `MEDIUM` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `OCR_ENGINE` | `PLANNED` | — |

### 5.21 Scanner & OCR — OCR

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-OCR-001` | **OCR Bahasa Indonesia**<br>OCR Indonesian | Mengenali teks Bahasa Indonesia. | `USER_TOOL` | `PRIMARY_TOOL` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `MEDIUM` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `OCR_ENGINE` | `PLANNED` | — |
| `FEAT-OCR-002` | **OCR English**<br>OCR English | Mengenali teks English. | `USER_TOOL` | `PRIMARY_TOOL` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `MEDIUM` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `OCR_ENGINE` | `PLANNED` | — |
| `FEAT-OCR-003` | **OCR Multi-language Selection**<br>OCR Multi-language Selection | Memilih bahasa OCR yang tersedia. | `USER_TOOL` | `PRIMARY_TOOL` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `MEDIUM` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `OCR_ENGINE` | `PLANNED` | — |
| `FEAT-OCR-004` | **Searchable PDF OCR**<br>Searchable PDF OCR | Menambahkan text layer pada PDF scan. | `USER_TOOL` | `PRIMARY_TOOL` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `MEDIUM` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `OCR_ENGINE` | `PLANNED` | Scan to Searchable PDF |
| `FEAT-OCR-005` | **OCR Text Extraction**<br>OCR Text Extraction | Mengambil hasil OCR sebagai teks. | `USER_TOOL` | `PRIMARY_TOOL` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `MEDIUM` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `OCR_ENGINE` | `PLANNED` | Scan to Text |
| `FEAT-OCR-006` | **OCR Table Extraction**<br>OCR Table Extraction | Mengambil tabel dari scan/gambar. | `USER_TOOL` | `PRIMARY_TOOL` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `MEDIUM` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `OCR_ENGINE` | `PLANNED` | — |
| `FEAT-OCR-007` | **OCR Result Editor**<br>OCR Result Editor | Mengoreksi hasil OCR. | `USER_TOOL` | `PRIMARY_TOOL` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `MEDIUM` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `OCR_ENGINE` | `PLANNED` | — |
| `FEAT-OCR-008` | **Copy OCR Result**<br>Copy OCR Result | Menyalin hasil OCR atas tindakan user. | `USER_TOOL` | `PRIMARY_TOOL` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `MEDIUM` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `OCR_ENGINE` | `PLANNED` | — |
| `FEAT-OCR-009` | **Batch OCR**<br>Batch OCR | Menjalankan OCR terhadap banyak file. | `USER_TOOL` | `PRIMARY_TOOL` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `MEDIUM` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `OCR_ENGINE` | `PLANNED` | — |
| `FEAT-OCR-010` | **Multi-page OCR**<br>Multi-page OCR | OCR banyak halaman. | `USER_TOOL` | `PRIMARY_TOOL` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `L1` | `MEDIUM` | Should / MVP | FR-SCAN-001..002; FR-PROC-001..005 | `OCR_ENGINE` | `PLANNED` | — |

### 5.22 Scanner & OCR — Advanced OCR

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-OCR-012` | **Document Layout Recognition**<br>Document Layout Recognition | Mengenali paragraf, kolom, dan blok dokumen. | `USER_TOOL` | `CONTEXT_ACTION` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `H1` | `LOW` | Should / R2 | FR-SCAN-001..002; FR-PROC-001..005 | `OCR_ENGINE` | `PLANNED` | — |
| `FEAT-OCR-013` | **Complex Table Recognition**<br>Complex Table Recognition | Mengenali tabel kompleks bila engine mendukung. | `USER_TOOL` | `CONTEXT_ACTION` | `CAMERA/SCAN/IMG/PDF→IMG/PDF/TEXT/DATA` | `H1` | `LOW` | Should / R2 | FR-SCAN-001..002; FR-PROC-001..005 | `OCR_ENGINE` | `FEASIBILITY_GATE` | — |

### 5.23 Signature & Annotation — Visual Signature

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-SIGN-001` | **Draw Signature with Touch**<br>Draw Signature with Touch | Membuat tanda tangan visual menggunakan jari. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; BR-SIGN-001 | `SIGNATURE_ASSET_SERVICE` | `PLANNED` | — |
| `FEAT-SIGN-002` | **Draw Signature with Stylus**<br>Draw Signature with Stylus | Membuat tanda tangan/paraf menggunakan stylus. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; BR-SIGN-001 | `SIGNATURE_ASSET_SERVICE` | `PLANNED` | — |
| `FEAT-SIGN-003` | **Create Initials**<br>Create Initials | Membuat dan menempatkan paraf/inisial. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; BR-SIGN-001 | `SIGNATURE_ASSET_SERVICE` | `PLANNED` | — |
| `FEAT-SIGN-004` | **Upload Signature Image**<br>Upload Signature Image | Mengunggah gambar tanda tangan transparan. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; BR-SIGN-001 | `SIGNATURE_ASSET_SERVICE` | `PLANNED` | — |
| `FEAT-SIGN-005` | **Paste Signature Image**<br>Paste Signature Image | Menempelkan gambar tanda tangan dari clipboard. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; BR-SIGN-001 | `SIGNATURE_ASSET_SERVICE` | `PLANNED` | — |
| `FEAT-SIGN-006` | **Saved Signature**<br>Saved Signature | Menyimpan asset tanda tangan secara aman jika user memilih. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; BR-SIGN-001 | `SIGNATURE_ASSET_SERVICE` | `PLANNED` | — |
| `FEAT-SIGN-007` | **Draw Signature with Mouse**<br>Draw Signature with Mouse | Menggambar tanda tangan menggunakan mouse. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; BR-SIGN-001 | `SIGNATURE_ASSET_SERVICE` | `PLANNED` | — |
| `FEAT-SIGN-008` | **Pen Tablet Signature**<br>Pen Tablet Signature | Menggunakan pen tablet/stylus desktop. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; BR-SIGN-001 | `SIGNATURE_ASSET_SERVICE` | `PLANNED` | — |
| `FEAT-SIGN-009` | **Resize Signature**<br>Resize Signature | Mengubah ukuran tanda tangan. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; BR-SIGN-001 | `SIGNATURE_ASSET_SERVICE` | `PLANNED` | — |
| `FEAT-SIGN-010` | **Move Signature**<br>Move Signature | Memindahkan posisi tanda tangan. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; BR-SIGN-001 | `SIGNATURE_ASSET_SERVICE` | `PLANNED` | — |
| `FEAT-SIGN-011` | **Rotate Signature**<br>Rotate Signature | Memutar tanda tangan. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; BR-SIGN-001 | `SIGNATURE_ASSET_SERVICE` | `PLANNED` | — |
| `FEAT-SIGN-012` | **Company Stamp**<br>Company Stamp | Menambahkan cap/stempel visual yang diizinkan. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; BR-SIGN-001 | `SIGNATURE_ASSET_SERVICE` | `PLANNED` | — |
| `FEAT-SIGN-013` | **Sign PDF with Visual Signature**<br>Sign PDF | Menempatkan tanda tangan visual/paraf/stamp non-PKI pada PDF, mempreview posisi/ukuran, lalu menghasilkan PDF final. | `USER_TOOL` | `PRIMARY_TOOL` | `PDF+SIGNATURE→PDF` | `L1` | `MEDIUM` | Must / MVP | FR-SIGN-003; BR-SIGN-001; SR-SEC-011..012 | `SIGNATURE_ASSET_SERVICE+PDF_ENGINE` | `PLANNED` | Visual Sign PDF; non-PKI |

### 5.24 Signature & Annotation — Annotation

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-ANN-001` | **Highlight**<br>Highlight | Menyorot teks/area. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; UIR-UI-004 | `ANNOTATION_ENGINE` | `PLANNED` | — |
| `FEAT-ANN-002` | **Underline**<br>Underline | Memberi garis bawah. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; UIR-UI-004 | `ANNOTATION_ENGINE` | `PLANNED` | — |
| `FEAT-ANN-003` | **Strikeout**<br>Strikeout | Memberi coretan pada teks. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; UIR-UI-004 | `ANNOTATION_ENGINE` | `PLANNED` | — |
| `FEAT-ANN-004` | **Comment**<br>Comment | Menambahkan komentar. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; UIR-UI-004 | `ANNOTATION_ENGINE` | `PLANNED` | — |
| `FEAT-ANN-005` | **Sticky Note**<br>Sticky Note | Menambahkan catatan tempel. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; UIR-UI-004 | `ANNOTATION_ENGINE` | `PLANNED` | — |
| `FEAT-ANN-006` | **Text Annotation**<br>Text Annotation | Menambahkan anotasi teks. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; UIR-UI-004 | `ANNOTATION_ENGINE` | `PLANNED` | — |
| `FEAT-ANN-007` | **Freehand Drawing**<br>Freehand Drawing | Menggambar bebas. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; UIR-UI-004 | `ANNOTATION_ENGINE` | `PLANNED` | — |
| `FEAT-ANN-008` | **Annotation Arrow**<br>Annotation Arrow | Menambahkan panah anotasi. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; UIR-UI-004 | `ANNOTATION_ENGINE` | `PLANNED` | — |
| `FEAT-ANN-009` | **Annotation Line**<br>Annotation Line | Menambahkan garis anotasi. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; UIR-UI-004 | `ANNOTATION_ENGINE` | `PLANNED` | — |
| `FEAT-ANN-010` | **Annotation Rectangle**<br>Annotation Rectangle | Menambahkan kotak. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; UIR-UI-004 | `ANNOTATION_ENGINE` | `PLANNED` | — |
| `FEAT-ANN-011` | **Annotation Circle**<br>Annotation Circle | Menambahkan lingkaran. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; UIR-UI-004 | `ANNOTATION_ENGINE` | `PLANNED` | — |
| `FEAT-ANN-012` | **Annotation Shapes**<br>Annotation Shapes | Menambahkan bentuk lain yang didukung. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; UIR-UI-004 | `ANNOTATION_ENGINE` | `PLANNED` | — |
| `FEAT-ANN-013` | **Stamp Annotation**<br>Stamp Annotation | Menambahkan stamp anotasi. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; UIR-UI-004 | `ANNOTATION_ENGINE` | `PLANNED` | — |
| `FEAT-ANN-014` | **Flatten Annotation**<br>Flatten Annotation | Meratakan anotasi menjadi konten final. | `USER_TOOL` | `CONTEXT_ACTION` | `DOC/IMG+ANNOTATION→WORKING_STATE` | `L1` | `LOW` | Should / MVP | FR-SIGN-001..002; UIR-UI-004 | `ANNOTATION_ENGINE` | `PLANNED` | — |

### 5.25 File & Productivity Tools — File Utilities

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-FILE-001` | **Rename File**<br>Rename File | Mengubah nama file. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..015 | `FILE_SERVICE` | `PLANNED` | — |
| `FEAT-FILE-002` | **Batch Rename**<br>Batch Rename | Mengubah nama banyak file berdasarkan pola. | `USER_TOOL` | `PRIMARY_TOOL` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..015 | `FILE_SERVICE` | `PLANNED` | — |
| `FEAT-FILE-003` | **Duplicate File**<br>Duplicate File | Membuat salinan file. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..015 | `FILE_SERVICE` | `PLANNED` | — |
| `FEAT-FILE-004` | **Duplicate Finder**<br>Duplicate Finder | Mendeteksi file yang kemungkinan duplikat. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..015 | `FILE_SERVICE` | `PLANNED` | — |
| `FEAT-FILE-005` | **Create ZIP**<br>Create ZIP | Mengompres file ke ZIP. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..015 | `FILE_SERVICE` | `PLANNED` | — |
| `FEAT-FILE-006` | **Extract ZIP**<br>Extract ZIP | Mengekstrak ZIP. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..015 | `FILE_SERVICE` | `PLANNED` | — |
| `FEAT-FILE-007` | **File Checksum**<br>File Checksum | Menghasilkan checksum untuk verifikasi integritas. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..015 | `FILE_SERVICE` | `PLANNED` | — |
| `FEAT-FILE-008` | **File Information**<br>File Information | Menampilkan informasi file. | `USER_TOOL` | `PRIMARY_TOOL` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..015 | `FILE_SERVICE` | `PLANNED` | — |
| `FEAT-FILE-009` | **Page Count**<br>Page Count | Menghitung jumlah halaman. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..015 | `FILE_SERVICE` | `PLANNED` | — |
| `FEAT-FILE-010` | **Metadata Cleaner**<br>Metadata Cleaner | Menghapus metadata yang dipilih. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..015 | `FILE_SERVICE` | `PLANNED` | — |

### 5.26 File & Productivity Tools — Productivity

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-PROD-001` | **Recent Files**<br>Recent Files | Menampilkan file yang baru digunakan. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005; UIR-UI-005 | `PRODUCTIVITY_SERVICE` | `PLANNED` | — |
| `FEAT-PROD-002` | **Recent Tools**<br>Recent Tools | Menampilkan tool yang baru digunakan. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005; UIR-UI-005 | `PRODUCTIVITY_SERVICE` | `PLANNED` | — |
| `FEAT-PROD-003` | **Favorite Tools**<br>Favorite Tools | Menyimpan tool favorit. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005; UIR-UI-005 | `PRODUCTIVITY_SERVICE` | `PLANNED` | — |
| `FEAT-PROD-004` | **Preset**<br>Preset | Menyimpan konfigurasi proses yang sering digunakan. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005; UIR-UI-005 | `PRODUCTIVITY_SERVICE` | `PLANNED` | Workflow Preset Creation |
| `FEAT-PROD-005` | **Repeat Last Action**<br>Repeat Last Action | Mengulangi proses terakhir. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005; UIR-UI-005 | `PRODUCTIVITY_SERVICE` | `PLANNED` | — |
| `FEAT-PROD-006` | **Clipboard Tools**<br>Clipboard Tools | Menangani input/aksi clipboard secara aman. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005; UIR-UI-005 | `PRODUCTIVITY_SERVICE` | `PLANNED` | — |
| `FEAT-PROD-007` | **Paste Screenshot**<br>Paste Screenshot | Menerima screenshot dan menawarkan aksi relevan. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005; UIR-UI-005 | `PRODUCTIVITY_SERVICE` | `PLANNED` | — |
| `FEAT-PROD-008` | **Copy/Paste File**<br>Copy/Paste File | Menerima file melalui clipboard. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005; UIR-UI-005 | `PRODUCTIVITY_SERVICE` | `PLANNED` | — |
| `FEAT-PROD-009` | **File Comparison**<br>File Comparison | Membandingkan file yang didukung. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005; UIR-UI-005 | `PRODUCTIVITY_SERVICE` | `PLANNED` | — |
| `FEAT-PROD-010` | **Text Cleaner**<br>Text Cleaner | Membersihkan teks dari pola/spasi yang tidak diperlukan. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005; UIR-UI-005 | `PRODUCTIVITY_SERVICE` | `PLANNED` | — |

### 5.27 File & Productivity Tools — Utility

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-PROD-011` | **QR Code Generator**<br>QR Code Generator | Membuat QR Code. | `USER_TOOL` | `PRIMARY_TOOL` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005; UIR-UI-005 | `PRODUCTIVITY_SERVICE` | `PLANNED` | — |
| `FEAT-PROD-012` | **Print**<br>Print | Mencetak hasil. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005; UIR-UI-005 | `PRODUCTIVITY_SERVICE` | `PLANNED` | — |
| `FEAT-PROD-013` | **Export**<br>Export | Mengekspor hasil. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005; UIR-UI-005 | `PRODUCTIVITY_SERVICE` | `PLANNED` | — |
| `FEAT-PROD-014` | **Auto Numbering**<br>Auto Numbering | Menambahkan penomoran otomatis pada konteks yang relevan. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005; UIR-UI-005 | `PRODUCTIVITY_SERVICE` | `PLANNED` | — |

### 5.28 File & Productivity Tools — Output Actions

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-PROD-015` | **Download Result**<br>Download Result | Mengunduh hasil pada platform yang menggunakan mekanisme download. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005; UIR-UI-005 | `PRODUCTIVITY_SERVICE` | `PLANNED` | — |
| `FEAT-PROD-016` | **Share Result**<br>Share Result | Membagikan hasil melalui mekanisme platform yang diizinkan. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `MEDIUM` | Should / MVP | FR-FILE-001..005; UIR-UI-005 | `PRODUCTIVITY_SERVICE` | `PLANNED` | — |
| `FEAT-PROD-017` | **Copy Result to Clipboard**<br>Copy Result to Clipboard | Menyalin hasil gambar/teks yang sesuai ke clipboard atas tindakan user. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005; UIR-UI-005 | `PRODUCTIVITY_SERVICE` | `PLANNED` | — |

### 5.29 File & Productivity Tools — Help & Support

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-PROD-018` | **Offline Help/User Guide**<br>Offline Help/User Guide | Menyediakan bantuan dasar yang tetap tersedia tanpa Assistant/server. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005; UIR-UI-005 | `PRODUCTIVITY_SERVICE` | `PLANNED` | — |

### 5.30 File & Productivity Tools — Input & Management

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-INPUT-001` | **File Picker**<br>File Picker | Memilih satu atau banyak file. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005 | `INPUT_ADAPTERS` | `PLANNED` | — |
| `FEAT-INPUT-002` | **Multi-file Selection**<br>Multi-file Selection | Memilih banyak file sekaligus. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005 | `INPUT_ADAPTERS` | `PLANNED` | — |
| `FEAT-INPUT-003` | **Drag-and-Drop File**<br>Drag-and-Drop File | Memasukkan satu file dengan drag-and-drop. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005 | `INPUT_ADAPTERS` | `PLANNED` | — |
| `FEAT-INPUT-004` | **Drag-and-Drop Multiple Files**<br>Drag-and-Drop Multiple Files | Memasukkan banyak file. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005 | `INPUT_ADAPTERS` | `PLANNED` | — |
| `FEAT-INPUT-005` | **Drag-and-Drop Folder**<br>Drag-and-Drop Folder | Memasukkan folder yang diizinkan. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005 | `INPUT_ADAPTERS` | `PLANNED` | — |
| `FEAT-INPUT-006` | **Share/Open With**<br>Share/Open With | Menerima file dari aplikasi lain. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `MEDIUM` | Should / MVP | FR-FILE-001..005 | `INPUT_ADAPTERS` | `PLANNED` | — |
| `FEAT-INPUT-007` | **Camera Input**<br>Camera Input | Mengambil file/gambar dari kamera. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005 | `INPUT_ADAPTERS` | `PLANNED` | — |
| `FEAT-INPUT-008` | **Scanner Input**<br>Scanner Input | Menerima input dari alur scanner. | `USER_TOOL` | `PRIMARY_TOOL` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `MEDIUM` | Should / MVP | FR-FILE-001..005 | `INPUT_ADAPTERS` | `PLANNED` | — |
| `FEAT-INPUT-009` | **Folder Watcher — Optional**<br>Folder Watcher | Memantau folder pilihan user tanpa auto-processing. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005 | `INPUT_ADAPTERS` | `PLANNED` | — |
| `FEAT-INPUT-010` | **Session Recovery Entry**<br>Session Recovery Entry | Menawarkan sesi/draft yang belum selesai. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005 | `INPUT_ADAPTERS` | `PLANNED` | — |
| `FEAT-INPUT-011` | **File Preview**<br>File Preview | Menampilkan preview file. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005 | `INPUT_ADAPTERS` | `PLANNED` | — |
| `FEAT-INPUT-012` | **Details Pane**<br>Details Pane | Menampilkan detail file. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005 | `INPUT_ADAPTERS` | `PLANNED` | — |
| `FEAT-INPUT-013` | **Preview Pane**<br>Preview Pane | Menampilkan preview di panel. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005 | `INPUT_ADAPTERS` | `PLANNED` | — |

### 5.31 File & Productivity Tools — View Modes

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-INPUT-014` | **Extra Large Icons View**<br>Extra Large Icons View | Mode ikon sangat besar. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005 | `INPUT_ADAPTERS` | `PLANNED` | — |
| `FEAT-INPUT-015` | **Large Icons View**<br>Large Icons View | Mode ikon besar. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005 | `INPUT_ADAPTERS` | `PLANNED` | — |
| `FEAT-INPUT-016` | **Medium Icons View**<br>Medium Icons View | Mode ikon sedang. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005 | `INPUT_ADAPTERS` | `PLANNED` | — |
| `FEAT-INPUT-017` | **Small Icons View**<br>Small Icons View | Mode ikon kecil. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005 | `INPUT_ADAPTERS` | `PLANNED` | — |
| `FEAT-INPUT-018` | **List View**<br>List View | Mode daftar. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005 | `INPUT_ADAPTERS` | `PLANNED` | — |
| `FEAT-INPUT-019` | **Details View**<br>Details View | Mode detail. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005 | `INPUT_ADAPTERS` | `PLANNED` | — |
| `FEAT-INPUT-020` | **Tiles View**<br>Tiles View | Mode ubin. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005 | `INPUT_ADAPTERS` | `PLANNED` | — |
| `FEAT-INPUT-021` | **Content View**<br>Content View | Mode konten. | `USER_TOOL` | `CONTEXT_ACTION` | `FILE/TEXT/CLIPBOARD→FILE/DATA/ACTION` | `L1` | `LOW` | Should / MVP | FR-FILE-001..005 | `INPUT_ADAPTERS` | `PLANNED` | — |

### 5.32 File & Productivity Tools — Smart Workflow

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-FLOW-001` | **File-First Entry**<br>File-First Entry | Memulai pekerjaan dari file. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-005..010 | `FEATURE_REGISTRY` | `PLANNED` | — |
| `FEAT-FLOW-002` | **Tool-First Entry**<br>Tool-First Entry | Memulai pekerjaan dari tool. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-005..010 | `FEATURE_REGISTRY` | `PLANNED` | — |
| `FEAT-FLOW-003` | **File Type Detection**<br>File Type Detection | Mengenali jenis file. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-005..010 | `FEATURE_REGISTRY` | `PLANNED` | — |
| `FEAT-FLOW-004` | **File Count Detection**<br>File Count Detection | Mengenali jumlah file. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-005..010 | `FEATURE_REGISTRY` | `PLANNED` | — |
| `FEAT-FLOW-005` | **Smart Recommendations**<br>Smart Recommendations | Menawarkan sekitar 3–5 tindakan relevan. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-005..010 | `FEATURE_REGISTRY` | `PLANNED` | Feature Recommendation Assistant |
| `FEAT-FLOW-006` | **Context-Aware Actions**<br>Context-Aware Actions | Menyesuaikan aksi berdasarkan konteks dan capability. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-005..010 | `FEATURE_REGISTRY` | `PLANNED` | — |
| `FEAT-FLOW-007` | **Natural Language Tool Search**<br>Natural Language Tool Search | Mencari fitur menggunakan bahasa natural. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-005..010 | `FEATURE_REGISTRY` | `PLANNED` | Conversational Tool Search |
| `FEAT-FLOW-008` | **Feature Alias Search**<br>Feature Alias Search | Mengenali sinonim/istilah alternatif. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-005..010 | `FEATURE_REGISTRY` | `PLANNED` | — |
| `FEAT-FLOW-009` | **Tool Favorites in Workspace**<br>Tool Favorites in Workspace | Menampilkan shortcut favorit. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-005..010 | `FEATURE_REGISTRY` | `PLANNED` | — |
| `FEAT-FLOW-010` | **Recent Tools in Workspace**<br>Recent Tools in Workspace | Mempercepat pekerjaan berulang. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-005..010 | `FEATURE_REGISTRY` | `PLANNED` | — |

### 5.33 Workspace/Backup/History/Recovery — Editing & Working State

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-EDIT-001` | **Non-destructive Editing**<br>Non-destructive Editing | Edit tidak langsung mengubah file sumber. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-STOR-001..006; FR-UI-020..021 | `WORKING_STATE_SERVICE` | `PLANNED` | — |
| `FEAT-EDIT-002` | **Autosave Draft**<br>Autosave Draft | Menyimpan draft secara otomatis. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-STOR-001..006; FR-UI-020..021 | `WORKING_STATE_SERVICE` | `PLANNED` | — |
| `FEAT-EDIT-003` | **Working State**<br>Working State | Menyimpan perubahan aktif secara terpisah. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-STOR-001..006; FR-UI-020..021 | `WORKING_STATE_SERVICE` | `PLANNED` | — |
| `FEAT-EDIT-004` | **Undo**<br>Undo | Membatalkan perubahan terakhir. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-STOR-001..006; FR-UI-020..021 | `WORKING_STATE_SERVICE` | `PLANNED` | — |
| `FEAT-EDIT-005` | **Redo**<br>Redo | Mengembalikan perubahan yang dibatalkan. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-STOR-001..006; FR-UI-020..021 | `WORKING_STATE_SERVICE` | `PLANNED` | — |
| `FEAT-EDIT-006` | **Preview Before Save**<br>Preview Before Save | Melihat hasil sebelum finalisasi. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-STOR-001..006; FR-UI-020..021 | `WORKING_STATE_SERVICE` | `PLANNED` | — |
| `FEAT-EDIT-007` | **Change Summary**<br>Change Summary | Menampilkan ringkasan perubahan. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-STOR-001..006; FR-UI-020..021 | `WORKING_STATE_SERVICE` | `PLANNED` | — |
| `FEAT-EDIT-008` | **Save**<br>Save | Menyimpan hasil. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-STOR-001..006; FR-UI-020..021 | `WORKING_STATE_SERVICE` | `PLANNED` | — |
| `FEAT-EDIT-009` | **Save As**<br>Save As | Menyimpan sebagai file baru. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-STOR-001..006; FR-UI-020..021 | `WORKING_STATE_SERVICE` | `PLANNED` | — |
| `FEAT-EDIT-010` | **Cancel Changes**<br>Cancel Changes | Membatalkan perubahan belum final. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-STOR-001..006; FR-UI-020..021 | `WORKING_STATE_SERVICE` | `PLANNED` | — |
| `FEAT-EDIT-011` | **Restore Draft**<br>Restore Draft | Memulihkan draft. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `MEDIUM` | Should / MVP | FR-STOR-001..006; FR-UI-020..021 | `WORKING_STATE_SERVICE` | `PLANNED` | — |
| `FEAT-EDIT-012` | **Overwrite Original**<br>Overwrite Original | Menimpa file sumber hanya atas aksi eksplisit dan konfirmasi yang berlaku. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `HIGH` | Should / MVP | FR-STOR-001..006; FR-UI-020..021 | `WORKING_STATE_SERVICE` | `PLANNED` | — |

### 5.34 Workspace/Backup/History/Recovery — Offline-First Processing

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-PROC-001` | **Offline Mode Detection**<br>Offline Mode Detection | Mendeteksi online/offline. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-PROC-001..005; NFR-PERF-007..013 | `PROCESS_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-PROC-002` | **Local Processing**<br>Local Processing | Memproses di perangkat bila memungkinkan. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-PROC-001..005; NFR-PERF-007..013 | `PROCESS_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-PROC-003` | **Server Processing**<br>Server Processing | Memproses di server hanya bila diperlukan. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-PROC-001..005; NFR-PERF-007..013 | `PROCESS_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-PROC-004` | **Hybrid Processing**<br>Hybrid Processing | Membagi tahap proses antara local dan server. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-PROC-001..005; NFR-PERF-007..013 | `PROCESS_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-PROC-005` | **Automatic Engine Selection**<br>Automatic Engine Selection | Memilih engine secara otomatis. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-PROC-001..005; NFR-PERF-007..013 | `PROCESS_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-PROC-006` | **Device Capability Detection**<br>Device Capability Detection | Mendeteksi kemampuan perangkat. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-PROC-001..005; NFR-PERF-007..013 | `PROCESS_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-PROC-007` | **Memory Capacity Check**<br>Memory Capacity Check | Memeriksa memory perangkat. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-PROC-001..005; NFR-PERF-007..013 | `PROCESS_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-PROC-008` | **Storage Capacity Check**<br>Storage Capacity Check | Memeriksa ruang penyimpanan. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-PROC-001..005; NFR-PERF-007..013 | `PROCESS_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-PROC-009` | **Network Availability Check**<br>Network Availability Check | Memeriksa koneksi. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-PROC-001..005; NFR-PERF-007..013 | `PROCESS_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-PROC-010` | **Server Availability Check**<br>Server Availability Check | Memeriksa layanan server. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-PROC-001..005; NFR-PERF-007..013 | `PROCESS_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-PROC-011` | **Permission/Policy Check**<br>Permission/Policy Check | Memeriksa izin dan kebijakan. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `MEDIUM` | Must / MVP | FR-PROC-001..005; NFR-PERF-007..013 | `PROCESS_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-PROC-012` | **Server-required Notification**<br>Server-required Notification | Memberi tahu bila proses memerlukan server. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-PROC-001..005; NFR-PERF-007..013 | `PROCESS_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-PROC-013` | **Offline Job Queue**<br>Offline Job Queue | Menahan metadata job ketika offline. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-PROC-001..005; NFR-PERF-007..013 | `PROCESS_ORCHESTRATOR` | `PLANNED` | Queue Until Online |
| `FEAT-PROC-014` | **Resume When Online**<br>Resume When Online | Melanjutkan job ketika koneksi kembali. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-PROC-001..005; NFR-PERF-007..013 | `PROCESS_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-PROC-015` | **Capability Fallback**<br>Capability Fallback | Menawarkan alternatif jika capability lokal tidak tersedia. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-PROC-001..005; NFR-PERF-007..013 | `PROCESS_ORCHESTRATOR` | `PLANNED` | — |

### 5.35 Workspace/Backup/History/Recovery — File Validation

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-VALID-001` | **Extension Validation**<br>Extension Validation | Memeriksa ekstensi. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-FILE-010..015 | `VALIDATION_PIPELINE` | `PLANNED` | — |
| `FEAT-VALID-002` | **MIME Type Validation**<br>MIME Type Validation | Memeriksa MIME type. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-FILE-010..015 | `VALIDATION_PIPELINE` | `PLANNED` | — |
| `FEAT-VALID-003` | **File Signature Validation**<br>File Signature Validation | Memeriksa header/signature file. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-FILE-010..015 | `VALIDATION_PIPELINE` | `PLANNED` | — |
| `FEAT-VALID-004` | **File Size Validation**<br>File Size Validation | Memeriksa ukuran terhadap limit. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-FILE-010..015 | `VALIDATION_PIPELINE` | `PLANNED` | — |
| `FEAT-VALID-005` | **Read Permission Validation**<br>Read Permission Validation | Memastikan hak membaca file. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `MEDIUM` | Must / MVP | FR-FILE-010..015 | `VALIDATION_PIPELINE` | `PLANNED` | — |
| `FEAT-VALID-006` | **Format Allowlist**<br>Format Allowlist | Membatasi format yang diperbolehkan. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-FILE-010..015 | `VALIDATION_PIPELINE` | `PLANNED` | — |
| `FEAT-VALID-007` | **Corrupted File Detection**<br>Corrupted File Detection | Mendeteksi file rusak. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-FILE-010..015 | `VALIDATION_PIPELINE` | `PLANNED` | — |
| `FEAT-VALID-008` | **Password/Encryption Detection**<br>Password/Encryption Detection | Mendeteksi proteksi/enkripsi. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `MEDIUM` | Must / MVP | FR-FILE-010..015 | `VALIDATION_PIPELINE` | `PLANNED` | — |
| `FEAT-VALID-009` | **Page Count Validation**<br>Page Count Validation | Memeriksa jumlah halaman. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-FILE-010..015 | `VALIDATION_PIPELINE` | `PLANNED` | — |
| `FEAT-VALID-010` | **Macro Detection**<br>Macro Detection | Mendeteksi macro. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-FILE-010..015 | `VALIDATION_PIPELINE` | `PLANNED` | — |
| `FEAT-VALID-011` | **Embedded Object Detection**<br>Embedded Object Detection | Mendeteksi object tertanam. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-FILE-010..015 | `VALIDATION_PIPELINE` | `PLANNED` | — |
| `FEAT-VALID-012` | **Decompression Size Check**<br>Decompression Size Check | Mencegah decompression bomb. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-FILE-010..015 | `VALIDATION_PIPELINE` | `PLANNED` | — |
| `FEAT-VALID-017` | **Batch Validation**<br>Batch Validation | Memvalidasi banyak file secara independen. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-FILE-010..015 | `VALIDATION_PIPELINE` | `PLANNED` | — |
| `FEAT-VALID-018` | **Validation Status**<br>Validation Status | Menampilkan READY, WARNING, BLOCKED, dan status validasi lain. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-FILE-010..015 | `VALIDATION_PIPELINE` | `PLANNED` | — |

### 5.36 Workspace/Backup/History/Recovery — Security Inspection

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-VALID-013` | **Malware Inspection**<br>Malware Inspection | Memeriksa malware bila diwajibkan. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `HIGH` | Must / MVP | FR-FILE-010..015 | `VALIDATION_PIPELINE` | `PLANNED` | — |
| `FEAT-VALID-014` | **Suspicious Script Inspection**<br>Suspicious Script Inspection | Mendeteksi script mencurigakan. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-FILE-010..015 | `VALIDATION_PIPELINE` | `PLANNED` | — |
| `FEAT-VALID-015` | **Embedded Executable Inspection**<br>Embedded Executable Inspection | Mendeteksi executable tertanam. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-FILE-010..015 | `VALIDATION_PIPELINE` | `PLANNED` | — |
| `FEAT-VALID-016` | **Security Policy Validation**<br>Security Policy Validation | Memeriksa kesesuaian kebijakan keamanan. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `SYS-H` | `LOW` | Must / MVP | FR-FILE-010..015 | `VALIDATION_PIPELINE` | `PLANNED` | — |

### 5.37 Workspace/Backup/History/Recovery — Local Storage & Cache

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-STOR-001` | **Local Source Reference**<br>Local Source Reference | Menjaga sumber tetap di lokasi asal. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-STOR-001..006 | `LOCAL_STORAGE_SERVICE` | `PLANNED` | — |
| `FEAT-STOR-002` | **Local Working State Storage**<br>Local Working State Storage | Menyimpan working state lokal. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-STOR-001..006 | `LOCAL_STORAGE_SERVICE` | `PLANNED` | — |
| `FEAT-STOR-003` | **Local Draft Storage**<br>Local Draft Storage | Menyimpan draft lokal. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-STOR-001..006 | `LOCAL_STORAGE_SERVICE` | `PLANNED` | — |
| `FEAT-STOR-004` | **Local Checkpoint Storage**<br>Local Checkpoint Storage | Menyimpan checkpoint lokal. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-STOR-001..006 | `LOCAL_STORAGE_SERVICE` | `PLANNED` | — |
| `FEAT-STOR-005` | **Local Cache**<br>Local Cache | Menyimpan cache seperlunya. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-STOR-001..006 | `LOCAL_STORAGE_SERVICE` | `PLANNED` | — |
| `FEAT-STOR-006` | **Encrypted Sensitive Cache**<br>Encrypted Sensitive Cache | Mengenkripsi cache sensitif. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-STOR-001..006 | `LOCAL_STORAGE_SERVICE` | `PLANNED` | — |
| `FEAT-STOR-007` | **Thumbnail Generation**<br>Thumbnail Generation | Membuat thumbnail. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-STOR-001..006 | `LOCAL_STORAGE_SERVICE` | `PLANNED` | — |
| `FEAT-STOR-008` | **Thumbnail On-demand**<br>Thumbnail On-demand | Membuat thumbnail hanya saat dibutuhkan. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-STOR-001..006 | `LOCAL_STORAGE_SERVICE` | `PLANNED` | — |
| `FEAT-STOR-009` | **Lazy Loading**<br>Lazy Loading | Memuat data bertahap. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-STOR-001..006 | `LOCAL_STORAGE_SERVICE` | `PLANNED` | — |
| `FEAT-STOR-010` | **Virtual Scrolling**<br>Virtual Scrolling | Merender item yang terlihat saja. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-STOR-001..006 | `LOCAL_STORAGE_SERVICE` | `PLANNED` | — |
| `FEAT-STOR-011` | **Temporary File Management**<br>Temporary File Management | Mengelola temporary file. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-STOR-001..006 | `LOCAL_STORAGE_SERVICE` | `PLANNED` | — |
| `FEAT-STOR-012` | **Automatic Temp Cleanup**<br>Automatic Temp Cleanup | Membersihkan temporary file. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-STOR-001..006 | `LOCAL_STORAGE_SERVICE` | `PLANNED` | — |
| `FEAT-STOR-013` | **Background Cleanup**<br>Background Cleanup | Membersihkan data non-kritis di background. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-STOR-001..006 | `LOCAL_STORAGE_SERVICE` | `PLANNED` | — |
| `FEAT-STOR-014` | **Metadata-only History Storage**<br>Metadata-only History Storage | Menyimpan riwayat terutama sebagai metadata. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-STOR-001..006 | `LOCAL_STORAGE_SERVICE` | `PLANNED` | — |
| `FEAT-STOR-015` | **No Whole-storage Scan**<br>No Whole-storage Scan | Mencegah pemindaian seluruh storage tanpa kebutuhan. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-STOR-001..006 | `LOCAL_STORAGE_SERVICE` | `PLANNED` | — |
| `FEAT-STOR-016` | **Hash On-demand**<br>Hash On-demand | Menghitung hash penuh hanya bila diperlukan. | `WORKFLOW_CAPABILITY` | `SYSTEM_INTERNAL` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-STOR-001..006 | `LOCAL_STORAGE_SERVICE` | `PLANNED` | — |

### 5.38 Workspace/Backup/History/Recovery — Amankan ke Akun

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-BACKUP-001` | **Amankan ke Akun**<br>Secure to Account | Menyimpan hasil yang dipilih user ke akun. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `E1` | `MEDIUM` | Should / MVP | FR-BACKUP-001..010 | `SECURE_ACCOUNT_STORAGE` | `PLANNED` | — |
| `FEAT-BACKUP-002` | **Selective Secure Backup**<br>Selective Secure Backup | Hanya mencadangkan hasil yang dipilih. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `E1` | `MEDIUM` | Should / MVP | FR-BACKUP-001..010 | `SECURE_ACCOUNT_STORAGE` | `PLANNED` | — |
| `FEAT-BACKUP-003` | **Account-bound Ownership**<br>Account-bound Ownership | Mengikat backup pada pemilik/permission. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `E1` | `LOW` | Should / MVP | FR-BACKUP-001..010 | `SECURE_ACCOUNT_STORAGE` | `PLANNED` | — |
| `FEAT-BACKUP-004` | **Secure Download**<br>Secure Download | Mengunduh backup secara aman. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `E1` | `LOW` | Should / MVP | FR-BACKUP-001..010 | `SECURE_ACCOUNT_STORAGE` | `PLANNED` | — |
| `FEAT-BACKUP-005` | **Cross-device File Access**<br>Cross-device File Access | Mengakses file diamankan dari perangkat lain. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `E1` | `LOW` | Should / MVP | FR-BACKUP-001..010 | `SECURE_ACCOUNT_STORAGE` | `PLANNED` | — |
| `FEAT-BACKUP-006` | **Restore Secured File**<br>Restore Secured File | Memulihkan file dari akun. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `E1` | `MEDIUM` | Should / MVP | FR-BACKUP-001..010 | `SECURE_ACCOUNT_STORAGE` | `PLANNED` | — |
| `FEAT-BACKUP-007` | **Delete Secured File**<br>Delete Secured File | Menghapus backup sesuai permission/policy. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `E1` | `HIGH` | Should / MVP | FR-BACKUP-001..010 | `SECURE_ACCOUNT_STORAGE` | `PLANNED` | — |
| `FEAT-BACKUP-008` | **Backup Metadata**<br>Backup Metadata | Menyimpan metadata backup. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `E1` | `MEDIUM` | Should / MVP | FR-BACKUP-001..010 | `SECURE_ACCOUNT_STORAGE` | `PLANNED` | — |

### 5.39 Workspace/Backup/History/Recovery — Selective Sync

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-BACKUP-009` | **Sync Secured File List**<br>Sync Secured File List | Menyinkronkan daftar file diamankan. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `E1` | `LOW` | Should / MVP | FR-BACKUP-001..010 | `SECURE_ACCOUNT_STORAGE` | `PLANNED` | — |
| `FEAT-BACKUP-010` | **Sync Favorite Tools**<br>Sync Favorite Tools | Sinkronisasi favorit bila diizinkan. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `E1` | `LOW` | Should / MVP | FR-BACKUP-001..010 | `SECURE_ACCOUNT_STORAGE` | `PLANNED` | — |
| `FEAT-BACKUP-011` | **Sync Selected Presets**<br>Sync Selected Presets | Sinkronisasi preset tertentu. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `E1` | `LOW` | Should / MVP | FR-BACKUP-001..010 | `SECURE_ACCOUNT_STORAGE` | `PLANNED` | — |
| `FEAT-BACKUP-012` | **Sync Language Preference**<br>Sync Language Preference | Sinkronisasi pilihan bahasa. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `E1` | `LOW` | Should / MVP | FR-BACKUP-001..010 | `SECURE_ACCOUNT_STORAGE` | `PLANNED` | — |
| `FEAT-BACKUP-013` | **Sync View Preference**<br>Sync View Preference | Sinkronisasi preferensi tampilan ringan. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `E1` | `LOW` | Should / MVP | FR-BACKUP-001..010 | `SECURE_ACCOUNT_STORAGE` | `PLANNED` | — |

### 5.40 Workspace/Backup/History/Recovery — Future Cloud

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-BACKUP-014` | **Full Cloud Workspace — Future**<br>Full Cloud Workspace | Workspace cloud penuh jika infrastruktur kelak disetujui. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `FUTURE` | `LOW` | Future / Future | FR-BACKUP-001..010 | `SECURE_ACCOUNT_STORAGE` | `FUTURE` | — |
| `FEAT-BACKUP-015` | **Cloud Autosave — Future**<br>Cloud Autosave | Autosave cloud sebagai roadmap. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `FUTURE` | `LOW` | Future / Future | FR-BACKUP-001..010 | `SECURE_ACCOUNT_STORAGE` | `FUTURE` | — |
| `FEAT-BACKUP-016` | **Automatic Workspace Sync — Future**<br>Automatic Workspace Sync | Sinkronisasi penuh lintas perangkat sebagai roadmap. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `FUTURE` | `LOW` | Future / Future | FR-BACKUP-001..010 | `SECURE_ACCOUNT_STORAGE` | `FUTURE` | — |

### 5.41 Workspace/Backup/History/Recovery — User History

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-HIST-001` | **Personal History**<br>Personal History | Menampilkan riwayat proses user beserta metadata utama. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-HIST-001..003 | `HISTORY_SERVICE` | `PLANNED` | — |
| `FEAT-HIST-002` | **History File Name**<br>History File Name | Menampilkan nama file di riwayat. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-HIST-001..003 | `HISTORY_SERVICE` | `PLANNED` | — |
| `FEAT-HIST-003` | **History Feature Used**<br>History Feature Used | Menampilkan fitur yang digunakan. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-HIST-001..003 | `HISTORY_SERVICE` | `PLANNED` | — |
| `FEAT-HIST-004` | **History Timestamp**<br>History Timestamp | Menampilkan waktu proses. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-HIST-001..003 | `HISTORY_SERVICE` | `PLANNED` | — |
| `FEAT-HIST-005` | **History Status**<br>History Status | Menampilkan status proses. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-HIST-001..003 | `HISTORY_SERVICE` | `PLANNED` | — |
| `FEAT-HIST-006` | **History Result Location**<br>History Result Location | Menampilkan lokasi hasil. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-HIST-001..003 | `HISTORY_SERVICE` | `PLANNED` | — |
| `FEAT-HIST-007` | **History File Size**<br>History File Size | Menampilkan ukuran file. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-HIST-001..003 | `HISTORY_SERVICE` | `PLANNED` | — |
| `FEAT-HIST-008` | **History Duration**<br>History Duration | Menampilkan durasi proses. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-HIST-001..003 | `HISTORY_SERVICE` | `PLANNED` | — |
| `FEAT-HIST-009` | **Open Result from History**<br>Open Result from History | Membuka hasil dari riwayat. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-HIST-001..003 | `HISTORY_SERVICE` | `PLANNED` | — |
| `FEAT-HIST-010` | **Open Result Folder**<br>Open Result Folder | Membuka folder hasil. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-HIST-001..003 | `HISTORY_SERVICE` | `PLANNED` | — |
| `FEAT-HIST-011` | **Repeat Process from History**<br>Repeat Process from History | Mengulangi proses. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-HIST-001..003 | `HISTORY_SERVICE` | `PLANNED` | — |
| `FEAT-HIST-012` | **Delete Personal History Item**<br>Delete Personal History Item | Menghapus riwayat pribadi tanpa menghapus audit. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-HIST-001..003 | `HISTORY_SERVICE` | `PLANNED` | — |
| `FEAT-HIST-013` | **History Search**<br>History Search | Mencari riwayat. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-HIST-001..003 | `HISTORY_SERVICE` | `PLANNED` | — |
| `FEAT-HIST-014` | **History Filter**<br>History Filter | Menyaring riwayat. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-HIST-001..003 | `HISTORY_SERVICE` | `PLANNED` | — |
| `FEAT-HIST-015` | **History Pagination/Lazy Load**<br>History Pagination/Lazy Load | Memuat riwayat bertahap, awal maksimal 50 item. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-HIST-001..003 | `HISTORY_SERVICE` | `PLANNED` | — |

### 5.42 Workspace/Backup/History/Recovery — Recycle Bin

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-TRASH-001` | **Move to Recycle Bin**<br>Move to Recycle Bin | Memindahkan item ke Recycle Bin. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-TRASH-001..009 | `RECYCLE_SERVICE` | `PLANNED` | — |
| `FEAT-TRASH-002` | **Restore from Recycle Bin**<br>Restore from Recycle Bin | Mengembalikan item. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `MEDIUM` | Should / MVP | FR-TRASH-001..009 | `RECYCLE_SERVICE` | `PLANNED` | — |
| `FEAT-TRASH-003` | **Undo Delete**<br>Undo Delete | Membatalkan penghapusan segera. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-TRASH-001..009 | `RECYCLE_SERVICE` | `PLANNED` | — |
| `FEAT-TRASH-004` | **Permanent Delete**<br>Permanent Delete | Menghapus permanen setelah konfirmasi. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `HIGH` | Should / MVP | FR-TRASH-001..009 | `RECYCLE_SERVICE` | `PLANNED` | — |
| `FEAT-TRASH-005` | **Empty Recycle Bin**<br>Empty Recycle Bin | Mengosongkan Recycle Bin. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `HIGH` | Should / MVP | FR-TRASH-001..009 | `RECYCLE_SERVICE` | `PLANNED` | — |
| `FEAT-TRASH-006` | **30-day Reminder**<br>30-day Reminder | Mengingatkan item yang tersimpan lebih dari 30 hari. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-TRASH-001..009 | `RECYCLE_SERVICE` | `PLANNED` | — |
| `FEAT-TRASH-007` | **Recycle Bin Size Reminder**<br>Recycle Bin Size Reminder | Mengingatkan bila ukuran melewati threshold. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-TRASH-001..009 | `RECYCLE_SERVICE` | `PLANNED` | — |
| `FEAT-TRASH-008` | **Low Storage Reminder**<br>Low Storage Reminder | Memperingatkan bila penyimpanan rendah. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-TRASH-001..009 | `RECYCLE_SERVICE` | `PLANNED` | — |
| `FEAT-TRASH-009` | **Reminder Frequency Control**<br>Reminder Frequency Control | Membatasi frekuensi notifikasi. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-TRASH-001..009 | `RECYCLE_SERVICE` | `PLANNED` | — |
| `FEAT-TRASH-010` | **Critical Storage Warning**<br>Critical Storage Warning | Memberikan warning lebih cepat pada kondisi kritis. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-TRASH-001..009 | `RECYCLE_SERVICE` | `PLANNED` | — |
| `FEAT-TRASH-011` | **Admin Recycle Policy**<br>Admin Recycle Policy | Mengatur threshold/retention. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-TRASH-001..009 | `RECYCLE_SERVICE` | `PLANNED` | — |
| `FEAT-TRASH-012` | **Cache Bypass Recycle Bin**<br>Cache Bypass Recycle Bin | Cache/temp langsung dibersihkan. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Should / MVP | FR-TRASH-001..009 | `RECYCLE_SERVICE` | `PLANNED` | — |

### 5.43 Workspace/Backup/History/Recovery — Recovery/Retry/Resume

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-REC-001` | **Process Checkpoint**<br>Process Checkpoint | Menyimpan checkpoint proses panjang. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-REC-001..006 | `JOB_RECOVERY_SERVICE` | `PLANNED` | — |
| `FEAT-REC-002` | **Crash Recovery Detection**<br>Crash Recovery Detection | Mendeteksi proses yang terputus. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-REC-001..006 | `JOB_RECOVERY_SERVICE` | `PLANNED` | — |
| `FEAT-REC-003` | **Startup Recovery**<br>Startup Recovery | Menawarkan pemulihan saat startup. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-REC-001..006 | `JOB_RECOVERY_SERVICE` | `PLANNED` | — |
| `FEAT-REC-004` | **Continue Action**<br>Continue Action | Melanjutkan proses. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-REC-001..006 | `JOB_RECOVERY_SERVICE` | `PLANNED` | — |
| `FEAT-REC-005` | **Retry Action**<br>Retry Action | Mencoba ulang. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-REC-001..006 | `JOB_RECOVERY_SERVICE` | `PLANNED` | — |
| `FEAT-REC-006` | **Cancel Recovery**<br>Cancel Recovery | Membatalkan pemulihan. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-REC-001..006 | `JOB_RECOVERY_SERVICE` | `PLANNED` | — |
| `FEAT-REC-007` | **Stage Retry**<br>Stage Retry | Mengulang hanya tahap yang perlu. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-REC-001..006 | `JOB_RECOVERY_SERVICE` | `PLANNED` | — |
| `FEAT-REC-008` | **Resume Upload**<br>Resume Upload | Melanjutkan upload terputus. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-REC-001..006 | `JOB_RECOVERY_SERVICE` | `PLANNED` | — |
| `FEAT-REC-009` | **Resume Download**<br>Resume Download | Melanjutkan download terputus. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-REC-001..006 | `JOB_RECOVERY_SERVICE` | `PLANNED` | — |
| `FEAT-REC-010` | **Job ID**<br>Job ID | Memberikan ID unik pada job. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-REC-001..006 | `JOB_RECOVERY_SERVICE` | `PLANNED` | — |
| `FEAT-REC-011` | **Correlation ID**<br>Correlation ID | Menghubungkan error/log antar-komponen. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-REC-001..006 | `JOB_RECOVERY_SERVICE` | `PLANNED` | — |
| `FEAT-REC-012` | **Output Validation**<br>Output Validation | Memastikan hasil valid sebelum final. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-REC-001..006 | `JOB_RECOVERY_SERVICE` | `PLANNED` | — |
| `FEAT-REC-013` | **Source Protection on Failure**<br>Source Protection on Failure | Menjaga file sumber saat terjadi kegagalan. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-REC-001..006 | `JOB_RECOVERY_SERVICE` | `PLANNED` | — |
| `FEAT-REC-014` | **Retry Limit**<br>Retry Limit | Mencegah retry tanpa batas. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-REC-001..006 | `JOB_RECOVERY_SERVICE` | `PLANNED` | — |
| `FEAT-REC-015` | **Queue Full Handling**<br>Queue Full Handling | Menangani antrean penuh secara terkendali. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `L-SYS` | `LOW` | Must / MVP | FR-REC-001..006 | `JOB_RECOVERY_SERVICE` | `PLANNED` | — |

### 5.44 Workspace/Backup/History/Recovery — Versioning

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-VER-001` | **Three-version Limit**<br>Three-version Limit | Maksimal tiga versi file yang diamankan. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `E1` | `MEDIUM` | Should / MVP | FR-VER-001..004 | `VERSION_SERVICE` | `PLANNED` | — |
| `FEAT-VER-002` | **Version Rotation**<br>Version Rotation | Mengelola versi tertua saat versi keempat dibuat. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `E1` | `MEDIUM` | Should / MVP | FR-VER-001..004 | `VERSION_SERVICE` | `PLANNED` | — |
| `FEAT-VER-003` | **View Version Information**<br>View Version Information | Melihat daftar versi. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `E1` | `MEDIUM` | Should / MVP | FR-VER-001..004 | `VERSION_SERVICE` | `PLANNED` | Backup Version Info |
| `FEAT-VER-004` | **Restore Previous Version**<br>Restore Previous Version | Memulihkan versi sebelumnya. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `E1` | `MEDIUM` | Should / MVP | FR-VER-001..004 | `VERSION_SERVICE` | `PLANNED` | — |
| `FEAT-VER-005` | **Save as New Version**<br>Save as New Version | Menyimpan versi baru. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `E1` | `MEDIUM` | Should / MVP | FR-VER-001..004 | `VERSION_SERVICE` | `PLANNED` | — |
| `FEAT-VER-006` | **Save as New File**<br>Save as New File | Menyimpan sebagai file terpisah. | `WORKFLOW_CAPABILITY` | `CONTEXT_ACTION` | `STATE/METADATA→STATE/ACTION` | `E1` | `LOW` | Should / MVP | FR-VER-001..004 | `VERSION_SERVICE` | `PLANNED` | — |

### 5.45 Account/Admin/Audit/System — Account & Access

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-AUTH-001` | **Pertamina SSO**<br>Pertamina SSO | Login melalui Identity Provider Pertamina. | `ENTERPRISE_CAPABILITY` | `PRIMARY/CONTEXT` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-AUTH-001..005; FR-ROLE-001..003 | `IDENTITY_SERVICE` | `PLANNED` | — |
| `FEAT-AUTH-002` | **Login**<br>Login | Membuka sesi user. | `ENTERPRISE_CAPABILITY` | `PRIMARY/CONTEXT` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-AUTH-001..005; FR-ROLE-001..003 | `IDENTITY_SERVICE` | `PLANNED` | — |
| `FEAT-AUTH-003` | **Logout**<br>Logout | Mengakhiri sesi. | `ENTERPRISE_CAPABILITY` | `PRIMARY/CONTEXT` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-AUTH-001..005; FR-ROLE-001..003 | `IDENTITY_SERVICE` | `PLANNED` | — |
| `FEAT-AUTH-004` | **Persistent Session**<br>Persistent Session | Mengurangi login berulang. | `ENTERPRISE_CAPABILITY` | `PRIMARY/CONTEXT` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-AUTH-001..005; FR-ROLE-001..003 | `IDENTITY_SERVICE` | `PLANNED` | — |
| `FEAT-AUTH-005` | **Offline Session**<br>Offline Session | Memungkinkan fitur lokal selama sesi offline valid. | `ENTERPRISE_CAPABILITY` | `PRIMARY/CONTEXT` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-AUTH-001..005; FR-ROLE-001..003 | `IDENTITY_SERVICE` | `PLANNED` | — |
| `FEAT-AUTH-006` | **Automatic Token Refresh**<br>Automatic Token Refresh | Memperbarui token otomatis. | `ENTERPRISE_CAPABILITY` | `PRIMARY/CONTEXT` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-AUTH-001..005; FR-ROLE-001..003 | `IDENTITY_SERVICE` | `PLANNED` | — |
| `FEAT-AUTH-007` | **Session Revocation**<br>Session Revocation | Mencabut sesi. | `ENTERPRISE_CAPABILITY` | `PRIMARY/CONTEXT` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `HIGH` | Must / MVP | FR-AUTH-001..005; FR-ROLE-001..003 | `IDENTITY_SERVICE` | `PLANNED` | — |
| `FEAT-AUTH-008` | **Sensitive Re-authentication**<br>Sensitive Re-authentication | Re-authentication untuk aksi sensitif. | `ENTERPRISE_CAPABILITY` | `PRIMARY/CONTEXT` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-AUTH-001..005; FR-ROLE-001..003 | `IDENTITY_SERVICE` | `PLANNED` | — |
| `FEAT-AUTH-009` | **User Profile**<br>User Profile | Menampilkan profil yang diizinkan. | `ENTERPRISE_CAPABILITY` | `PRIMARY/CONTEXT` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-AUTH-001..005; FR-ROLE-001..003 | `IDENTITY_SERVICE` | `PLANNED` | — |
| `FEAT-AUTH-010` | **Role**<br>Role | Merepresentasikan kelompok kewenangan. | `ENTERPRISE_CAPABILITY` | `PRIMARY/CONTEXT` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-AUTH-001..005; FR-ROLE-001..003 | `IDENTITY_SERVICE` | `PLANNED` | — |
| `FEAT-AUTH-011` | **Permission-based Access**<br>Permission-based Access | Mengontrol akses berdasarkan permission. | `ENTERPRISE_CAPABILITY` | `PRIMARY/CONTEXT` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-AUTH-001..005; FR-ROLE-001..003 | `IDENTITY_SERVICE` | `PLANNED` | — |
| `FEAT-AUTH-012` | **Permission Synchronization**<br>Permission Synchronization | Menyinkronkan role/permission. | `ENTERPRISE_CAPABILITY` | `PRIMARY/CONTEXT` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-AUTH-001..005; FR-ROLE-001..003 | `IDENTITY_SERVICE` | `PLANNED` | — |

### 5.46 Account/Admin/Audit/System — Administration

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-ADMIN-001` | **User Management**<br>User Management | Mengelola user sesuai kewenangan. | `ENTERPRISE_CAPABILITY` | `ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `HIGH` | Must / MVP | FR-ROLE-001..003; NFR-MON-001..003 | `ADMIN_POLICY_SERVICE` | `PLANNED` | — |
| `FEAT-ADMIN-002` | **Role Management**<br>Role Management | Mengelola role. | `ENTERPRISE_CAPABILITY` | `ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `HIGH` | Must / MVP | FR-ROLE-001..003; NFR-MON-001..003 | `ADMIN_POLICY_SERVICE` | `PLANNED` | — |
| `FEAT-ADMIN-003` | **Permission Management**<br>Permission Management | Mengelola permission. | `ENTERPRISE_CAPABILITY` | `ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `HIGH` | Must / MVP | FR-ROLE-001..003; NFR-MON-001..003 | `ADMIN_POLICY_SERVICE` | `PLANNED` | — |
| `FEAT-ADMIN-004` | **Feature Enable/Disable**<br>Feature Enable/Disable | Mengaktifkan/menonaktifkan fitur. | `ENTERPRISE_CAPABILITY` | `ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-ROLE-001..003; NFR-MON-001..003 | `ADMIN_POLICY_SERVICE` | `PLANNED` | — |
| `FEAT-ADMIN-005` | **Feature Flags**<br>Feature Flags | Mengontrol rollout fitur. | `ENTERPRISE_CAPABILITY` | `ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Should / R2 | FR-ROLE-001..003; NFR-MON-001..003 | `ADMIN_POLICY_SERVICE` | `PLANNED` | — |
| `FEAT-ADMIN-006` | **File Size Limit**<br>File Size Limit | Mengatur batas file. | `ENTERPRISE_CAPABILITY` | `ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-ROLE-001..003; NFR-MON-001..003 | `ADMIN_POLICY_SERVICE` | `PLANNED` | — |
| `FEAT-ADMIN-007` | **Processing Limit**<br>Processing Limit | Mengatur batas proses. | `ENTERPRISE_CAPABILITY` | `ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-ROLE-001..003; NFR-MON-001..003 | `ADMIN_POLICY_SERVICE` | `PLANNED` | — |
| `FEAT-ADMIN-008` | **Storage Quota**<br>Storage Quota | Mengatur kuota. | `ENTERPRISE_CAPABILITY` | `ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-ROLE-001..003; NFR-MON-001..003 | `ADMIN_POLICY_SERVICE` | `PLANNED` | — |
| `FEAT-ADMIN-009` | **Retention Policy**<br>Retention Policy | Mengatur retensi. | `ENTERPRISE_CAPABILITY` | `ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-ROLE-001..003; NFR-MON-001..003 | `ADMIN_POLICY_SERVICE` | `PLANNED` | — |
| `FEAT-ADMIN-010` | **Recycle Bin Policy**<br>Recycle Bin Policy | Mengatur kebijakan Recycle Bin. | `ENTERPRISE_CAPABILITY` | `ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-ROLE-001..003; NFR-MON-001..003 | `ADMIN_POLICY_SERVICE` | `PLANNED` | — |
| `FEAT-ADMIN-011` | **Storage Threshold**<br>Storage Threshold | Mengatur threshold penyimpanan. | `ENTERPRISE_CAPABILITY` | `ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-ROLE-001..003; NFR-MON-001..003 | `ADMIN_POLICY_SERVICE` | `PLANNED` | — |
| `FEAT-ADMIN-012` | **Security Profile**<br>Security Profile | Mengatur profil keamanan. | `ENTERPRISE_CAPABILITY` | `ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `HIGH` | Must / MVP | FR-ROLE-001..003; NFR-MON-001..003 | `ADMIN_POLICY_SERVICE` | `PLANNED` | — |
| `FEAT-ADMIN-013` | **Classification Policy**<br>Classification Policy | Mengatur perilaku berdasarkan klasifikasi. | `ENTERPRISE_CAPABILITY` | `ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `HIGH` | Should / R2 | FR-ROLE-001..003; NFR-MON-001..003 | `ADMIN_POLICY_SERVICE` | `PLANNED` | — |
| `FEAT-ADMIN-014` | **Maintenance Mode**<br>Maintenance Mode | Menempatkan sistem dalam mode pemeliharaan. | `ENTERPRISE_CAPABILITY` | `ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-ROLE-001..003; NFR-MON-001..003 | `ADMIN_POLICY_SERVICE` | `PLANNED` | — |
| `FEAT-ADMIN-015` | **Minimum App Version**<br>Minimum App Version | Menentukan versi minimum client. | `ENTERPRISE_CAPABILITY` | `ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-ROLE-001..003; NFR-MON-001..003 | `ADMIN_POLICY_SERVICE` | `PLANNED` | — |
| `FEAT-ADMIN-016` | **Job Monitoring Admin**<br>Job Monitoring Admin | Memantau job. | `ENTERPRISE_CAPABILITY` | `ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-ROLE-001..003; NFR-MON-001..003 | `ADMIN_POLICY_SERVICE` | `PLANNED` | — |
| `FEAT-ADMIN-017` | **Queue Monitoring Admin**<br>Queue Monitoring Admin | Memantau antrean. | `ENTERPRISE_CAPABILITY` | `ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-ROLE-001..003; NFR-MON-001..003 | `ADMIN_POLICY_SERVICE` | `PLANNED` | — |
| `FEAT-ADMIN-018` | **Sync Monitoring Admin**<br>Sync Monitoring Admin | Memantau sinkronisasi. | `ENTERPRISE_CAPABILITY` | `ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-ROLE-001..003; NFR-MON-001..003 | `ADMIN_POLICY_SERVICE` | `PLANNED` | — |
| `FEAT-ADMIN-019` | **Error Monitoring Admin**<br>Error Monitoring Admin | Memantau error teknis. | `ENTERPRISE_CAPABILITY` | `ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-ROLE-001..003; NFR-MON-001..003 | `ADMIN_POLICY_SERVICE` | `PLANNED` | — |
| `FEAT-ADMIN-020` | **Server Health Admin**<br>Server Health Admin | Melihat kesehatan server. | `ENTERPRISE_CAPABILITY` | `ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-ROLE-001..003; NFR-MON-001..003 | `ADMIN_POLICY_SERVICE` | `PLANNED` | — |
| `FEAT-ADMIN-021` | **Storage Health Admin**<br>Storage Health Admin | Melihat kesehatan storage. | `ENTERPRISE_CAPABILITY` | `ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-ROLE-001..003; NFR-MON-001..003 | `ADMIN_POLICY_SERVICE` | `PLANNED` | — |
| `FEAT-ADMIN-022` | **Update Required / Version Compatibility Screen**<br>Update Required / Version Compatibility Screen | Menampilkan status kompatibilitas versi dan kewajiban update sesuai minimum app version. | `ENTERPRISE_CAPABILITY` | `ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-ROLE-001..003; NFR-MON-001..003 | `ADMIN_POLICY_SERVICE` | `PLANNED` | — |

### 5.47 Account/Admin/Audit/System — Audit & Compliance

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-AUDIT-001` | **Enterprise Audit Trail**<br>Enterprise Audit Trail | Mencatat aktivitas wajib sebagai metadata. | `ENTERPRISE_CAPABILITY` | `AUDITOR/ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-AUDIT-001..004 | `AUDIT_SERVICE` | `PLANNED` | Login Audit, Logout Audit, Failed Login Audit, Feature Usage Audit, Process Audit, Local/Server Processing Audit, Error Audit, Role Change Audit, Permission Change Audit, Configuration Change Audit |
| `FEAT-AUDIT-002` | **Audit Search**<br>Audit Search | Mencari audit. | `ENTERPRISE_CAPABILITY` | `AUDITOR/ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-AUDIT-001..004 | `AUDIT_SERVICE` | `PLANNED` | — |
| `FEAT-AUDIT-003` | **Audit Filter**<br>Audit Filter | Menyaring audit. | `ENTERPRISE_CAPABILITY` | `AUDITOR/ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-AUDIT-001..004 | `AUDIT_SERVICE` | `PLANNED` | — |
| `FEAT-AUDIT-004` | **Audit Export**<br>Audit Export | Mengekspor laporan audit. | `ENTERPRISE_CAPABILITY` | `AUDITOR/ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-AUDIT-001..004 | `AUDIT_SERVICE` | `PLANNED` | — |
| `FEAT-AUDIT-005` | **Offline Audit Queue**<br>Offline Audit Queue | Menampung metadata audit ketika offline. | `ENTERPRISE_CAPABILITY` | `AUDITOR/ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-AUDIT-001..004 | `AUDIT_SERVICE` | `PLANNED` | — |
| `FEAT-AUDIT-006` | **Batch Audit Sync**<br>Batch Audit Sync | Mengirim audit dalam batch. | `ENTERPRISE_CAPABILITY` | `AUDITOR/ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-AUDIT-001..004 | `AUDIT_SERVICE` | `PLANNED` | — |
| `FEAT-AUDIT-007` | **Audit Sync Retry**<br>Audit Sync Retry | Mencoba kembali sinkronisasi audit. | `ENTERPRISE_CAPABILITY` | `AUDITOR/ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-AUDIT-001..004 | `AUDIT_SERVICE` | `PLANNED` | — |
| `FEAT-AUDIT-008` | **Audit Retention**<br>Audit Retention | Mengelola retensi audit. | `ENTERPRISE_CAPABILITY` | `AUDITOR/ADMIN` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `MEDIUM` | Must / MVP | FR-AUDIT-001..004 | `AUDIT_SERVICE` | `PLANNED` | — |

### 5.48 Account/Admin/Audit/System — Progress & Activity

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-PROG-001` | **Detailed Progress Status**<br>Detailed Progress Status | Menampilkan persentase, tahap, item aktif, jumlah selesai/sisa, dan ETA bila tersedia. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Must / MVP | UIR-UI-003..004 | `ACTIVITY_SERVICE` | `PLANNED` | Current Step, Current File/Page/Sheet/Slide, Completed Count, Remaining Count, ETA |
| `FEAT-PROG-002` | **Safe Cancel**<br>Safe Cancel | Membatalkan proses bila aman. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Must / MVP | UIR-UI-003..004 | `ACTIVITY_SERVICE` | `PLANNED` | — |
| `FEAT-PROG-003` | **Indeterminate Activity**<br>Indeterminate Activity | Menunjukkan aktivitas bila persentase tidak dapat dihitung. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Must / MVP | UIR-UI-003..004 | `ACTIVITY_SERVICE` | `PLANNED` | — |
| `FEAT-PROG-004` | **Background Job Notification**<br>Background Job Notification | Memberikan notifikasi job background. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Must / MVP | UIR-UI-003..004 | `ACTIVITY_SERVICE` | `PLANNED` | — |
| `FEAT-PROG-005` | **Completed Notification**<br>Completed Notification | Memberikan notifikasi selesai. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Must / MVP | UIR-UI-003..004 | `ACTIVITY_SERVICE` | `PLANNED` | — |
| `FEAT-PROG-006` | **Failed Notification**<br>Failed Notification | Memberikan notifikasi gagal. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Must / MVP | UIR-UI-003..004 | `ACTIVITY_SERVICE` | `PLANNED` | — |
| `FEAT-PROG-007` | **Waiting Connection Notification**<br>Waiting Connection Notification | Memberikan status menunggu koneksi. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Must / MVP | UIR-UI-003..004 | `ACTIVITY_SERVICE` | `PLANNED` | — |
| `FEAT-PROG-008` | **Activity Center**<br>Activity Center | Menampilkan seluruh status job. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Must / MVP | UIR-UI-003..004 | `ACTIVITY_SERVICE` | `PLANNED` | Assistant Activity Progress |
| `FEAT-PROG-009` | **Snackbar Feedback**<br>Snackbar Feedback | Feedback ringan tanpa dialog berlebihan. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Must / MVP | UIR-UI-003..004 | `ACTIVITY_SERVICE` | `PLANNED` | — |

### 5.49 Account/Admin/Audit/System — Language & Accessibility

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-UX-001` | **Bahasa Indonesia**<br>Indonesian UI | Antarmuka Bahasa Indonesia. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-001..010; UIR-L10N-001..003 | `UI_PLATFORM_SERVICE` | `PLANNED` | — |
| `FEAT-UX-002` | **English**<br>English UI | Antarmuka English. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-001..010; UIR-L10N-001..003 | `UI_PLATFORM_SERVICE` | `PLANNED` | — |
| `FEAT-UX-003` | **Device Language Detection**<br>Device Language Detection | Mengikuti bahasa perangkat. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-001..010; UIR-L10N-001..003 | `UI_PLATFORM_SERVICE` | `PLANNED` | — |
| `FEAT-UX-004` | **Manual Language Selection**<br>Manual Language Selection | Memilih bahasa secara manual. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-001..010; UIR-L10N-001..003 | `UI_PLATFORM_SERVICE` | `PLANNED` | — |
| `FEAT-UX-005` | **Indonesian Fallback**<br>Indonesian Fallback | Bahasa Indonesia sebagai fallback. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-001..010; UIR-L10N-001..003 | `UI_PLATFORM_SERVICE` | `PLANNED` | — |
| `FEAT-UX-006` | **Localized UI Resources**<br>Localized UI Resources | Melokalisasi error, progress, notifikasi, tooltip, dan accessibility text melalui resource key. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-001..010; UIR-L10N-001..003 | `UI_PLATFORM_SERVICE` | `PLANNED` | Localized Progress, Localized Notifications, Localized Tooltips, Localized Accessibility Text |

### 5.50 Account/Admin/Audit/System — Adaptive UX

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-UX-007` | **Responsive Layout**<br>Responsive Layout | Menyesuaikan layout berbagai ukuran layar. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-001..010; UIR-L10N-001..003 | `UI_PLATFORM_SERVICE` | `PLANNED` | — |
| `FEAT-UX-008` | **Adaptive UX**<br>Adaptive UX | Pola interaksi disesuaikan dengan capability perangkat. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-001..010; UIR-L10N-001..003 | `UI_PLATFORM_SERVICE` | `PLANNED` | — |
| `FEAT-UX-009` | **Portrait Support**<br>Portrait Support | Mendukung portrait. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-001..010; UIR-L10N-001..003 | `UI_PLATFORM_SERVICE` | `PLANNED` | — |
| `FEAT-UX-010` | **Landscape Support**<br>Landscape Support | Mendukung landscape. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-001..010; UIR-L10N-001..003 | `UI_PLATFORM_SERVICE` | `PLANNED` | — |
| `FEAT-UX-011` | **Text Scaling**<br>Text Scaling | Mendukung pembesaran teks. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-001..010; UIR-L10N-001..003 | `UI_PLATFORM_SERVICE` | `PLANNED` | — |
| `FEAT-UX-012` | **Keyboard Navigation**<br>Keyboard Navigation | Mendukung keyboard. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-001..010; UIR-L10N-001..003 | `UI_PLATFORM_SERVICE` | `PLANNED` | — |
| `FEAT-UX-013` | **Screen Reader Support**<br>Screen Reader Support | Mendukung screen reader. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-001..010; UIR-L10N-001..003 | `UI_PLATFORM_SERVICE` | `PLANNED` | — |
| `FEAT-UX-014` | **Accessible Icon + Text**<br>Accessible Icon + Text | Aksi penting memiliki label yang jelas. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-001..010; UIR-L10N-001..003 | `UI_PLATFORM_SERVICE` | `PLANNED` | — |
| `FEAT-UX-015` | **No Color-only Communication**<br>No Color-only Communication | Informasi tidak hanya mengandalkan warna. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-001..010; UIR-L10N-001..003 | `UI_PLATFORM_SERVICE` | `PLANNED` | — |
| `FEAT-UX-016` | **No Overflow**<br>No Overflow | Mencegah konten penting terpotong. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-001..010; UIR-L10N-001..003 | `UI_PLATFORM_SERVICE` | `PLANNED` | — |

### 5.51 Account/Admin/Audit/System — Application Preferences

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-UX-017` | **Application Settings**<br>Application Settings | Mengelola preferensi user yang diizinkan secara terpusat di UI settings. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-001..010; UIR-L10N-001..003 | `UI_PLATFORM_SERVICE` | `PLANNED` | — |
| `FEAT-UX-018` | **Default Save Location**<br>Default Save Location | Mengatur lokasi simpan default pada platform yang mendukung. | `PLATFORM_CAPABILITY` | `BACKGROUND/CONTEXT` | `UI/STATE→UI/STATE` | `L-SYS` | `LOW` | Should / MVP | UIR-UI-001..010; UIR-L10N-001..003 | `UI_PLATFORM_SERVICE` | `PLANNED` | — |

### 5.52 Account/Admin/Audit/System — System Security

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-SEC-001` | **HTTPS Enforcement**<br>HTTPS Enforcement | Memastikan komunikasi menggunakan HTTPS. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `MEDIUM` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | — |
| `FEAT-SEC-002` | **Secure Token Storage**<br>Secure Token Storage | Menyimpan token secara aman. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `MEDIUM` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | — |
| `FEAT-SEC-003` | **Sensitive Data Log Protection**<br>Sensitive Data Log Protection | Mencegah data sensitif masuk log. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `MEDIUM` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | — |
| `FEAT-SEC-004` | **Upload Consent**<br>Upload Consent | Meminta persetujuan sebelum isi dokumen dikirim ke server. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `HIGH` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | — |
| `FEAT-SEC-005` | **Server File Access Control**<br>Server File Access Control | Mengontrol akses file server. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `MEDIUM` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | — |
| `FEAT-SEC-006` | **Encryption at Rest**<br>Encryption at Rest | Mengenkripsi data/file yang diwajibkan saat tersimpan. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `MEDIUM` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | Encrypted Server Storage |
| `FEAT-SEC-007` | **Encryption in Transit**<br>Encryption in Transit | Melindungi data selama transfer. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `MEDIUM` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | Encrypted Upload |
| `FEAT-SEC-008` | **Request Size Limit**<br>Request Size Limit | Membatasi ukuran request. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `MEDIUM` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | — |
| `FEAT-SEC-009` | **Rate Limiting**<br>Rate Limiting | Membatasi request tertentu jika diperlukan. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `MEDIUM` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | — |
| `FEAT-SEC-010` | **Reverse Proxy Security**<br>Reverse Proxy Security | Menggunakan reverse proxy secara aman. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `MEDIUM` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | — |
| `FEAT-SEC-011` | **Security Headers**<br>Security Headers | Menerapkan security headers. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `MEDIUM` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | — |
| `FEAT-SEC-012` | **Malware Scanning Integration**<br>Malware Scanning Integration | Integrasi malware scanner bila diwajibkan. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `HIGH` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | — |
| `FEAT-SEC-013` | **Vulnerability Assessment**<br>Vulnerability Assessment | Menjalankan vulnerability assessment. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `HIGH` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | — |
| `FEAT-SEC-014` | **Penetration Testing**<br>Penetration Testing | Menjalankan penetration test. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `HIGH` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | — |
| `FEAT-SEC-015` | **Dependency Scanning**<br>Dependency Scanning | Memeriksa dependency. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `MEDIUM` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | — |
| `FEAT-SEC-016` | **Secret Scanning**<br>Secret Scanning | Mendeteksi secret di repository. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `HIGH` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | — |
| `FEAT-SEC-017` | **SAST**<br>SAST | Static Application Security Testing. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `HIGH` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | — |
| `FEAT-SEC-018` | **DAST**<br>DAST | Dynamic Application Security Testing. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `HIGH` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | — |
| `FEAT-SEC-019` | **SBOM**<br>SBOM | Software Bill of Materials. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `MEDIUM` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | — |
| `FEAT-SEC-020` | **License Review**<br>License Review | Memeriksa lisensi library/plugin. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `MEDIUM` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | — |
| `FEAT-SEC-021` | **Security Approval for Third-party Components**<br>Security Approval for Third-party Components | Review keamanan komponen pihak ketiga. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `MEDIUM` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | — |
| `FEAT-SEC-022` | **Critical/High Remediation**<br>Critical/High Remediation | Menangani temuan Critical/High. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `HIGH` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | — |
| `FEAT-SEC-023` | **Security Retest**<br>Security Retest | Melakukan retest setelah remediasi. | `SECURITY_CONTROL` | `SYSTEM_INTERNAL` | `REQUEST/BUILD/DATA→SECURITY_DECISION/EVIDENCE` | `SEC1` | `MEDIUM` | Must / MVP | SR-SEC-001..018 | `SECURITY_PLATFORM` | `PLANNED` | — |

### 5.53 Account/Admin/Audit/System — Monitoring & System Health

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-MON-001` | **Application Insights/APM**<br>Application Insights/APM | Mengumpulkan telemetry aplikasi secara aman. | `OBSERVABILITY_CAPABILITY` | `ADMIN` | `TELEMETRY→METRIC/ALERT` | `E1` | `LOW` | Must / MVP | NFR-MON-001..003; NFR-PERF-010 | `OBSERVABILITY_PLATFORM` | `PLANNED` | — |
| `FEAT-MON-002` | **API Response Time Monitoring**<br>API Response Time Monitoring | Memantau waktu respons API. | `OBSERVABILITY_CAPABILITY` | `ADMIN` | `TELEMETRY→METRIC/ALERT` | `E1` | `LOW` | Must / MVP | NFR-MON-001..003; NFR-PERF-010 | `OBSERVABILITY_PLATFORM` | `PLANNED` | — |
| `FEAT-MON-003` | **Error Rate Monitoring**<br>Error Rate Monitoring | Memantau tingkat error. | `OBSERVABILITY_CAPABILITY` | `ADMIN` | `TELEMETRY→METRIC/ALERT` | `E1` | `LOW` | Must / MVP | NFR-MON-001..003; NFR-PERF-010 | `OBSERVABILITY_PLATFORM` | `PLANNED` | — |
| `FEAT-MON-004` | **Job Duration Monitoring**<br>Job Duration Monitoring | Memantau durasi job. | `OBSERVABILITY_CAPABILITY` | `ADMIN` | `TELEMETRY→METRIC/ALERT` | `E1` | `LOW` | Must / MVP | NFR-MON-001..003; NFR-PERF-010 | `OBSERVABILITY_PLATFORM` | `PLANNED` | — |
| `FEAT-MON-005` | **Queue Length Monitoring**<br>Queue Length Monitoring | Memantau panjang antrean. | `OBSERVABILITY_CAPABILITY` | `ADMIN` | `TELEMETRY→METRIC/ALERT` | `E1` | `LOW` | Must / MVP | NFR-MON-001..003; NFR-PERF-010 | `OBSERVABILITY_PLATFORM` | `PLANNED` | — |
| `FEAT-MON-006` | **Upload Failure Monitoring**<br>Upload Failure Monitoring | Memantau kegagalan upload. | `OBSERVABILITY_CAPABILITY` | `ADMIN` | `TELEMETRY→METRIC/ALERT` | `E1` | `LOW` | Must / MVP | NFR-MON-001..003; NFR-PERF-010 | `OBSERVABILITY_PLATFORM` | `PLANNED` | — |
| `FEAT-MON-007` | **Download Failure Monitoring**<br>Download Failure Monitoring | Memantau kegagalan download. | `OBSERVABILITY_CAPABILITY` | `ADMIN` | `TELEMETRY→METRIC/ALERT` | `E1` | `LOW` | Must / MVP | NFR-MON-001..003; NFR-PERF-010 | `OBSERVABILITY_PLATFORM` | `PLANNED` | — |
| `FEAT-MON-008` | **Database Response Monitoring**<br>Database Response Monitoring | Memantau respons database. | `OBSERVABILITY_CAPABILITY` | `ADMIN` | `TELEMETRY→METRIC/ALERT` | `E1` | `LOW` | Must / MVP | NFR-MON-001..003; NFR-PERF-010 | `OBSERVABILITY_PLATFORM` | `PLANNED` | — |
| `FEAT-MON-009` | **CPU Monitoring**<br>CPU Monitoring | Memantau CPU. | `OBSERVABILITY_CAPABILITY` | `ADMIN` | `TELEMETRY→METRIC/ALERT` | `E1` | `LOW` | Must / MVP | NFR-MON-001..003; NFR-PERF-010 | `OBSERVABILITY_PLATFORM` | `PLANNED` | — |
| `FEAT-MON-010` | **Memory Monitoring**<br>Memory Monitoring | Memantau RAM. | `OBSERVABILITY_CAPABILITY` | `ADMIN` | `TELEMETRY→METRIC/ALERT` | `E1` | `LOW` | Must / MVP | NFR-MON-001..003; NFR-PERF-010 | `OBSERVABILITY_PLATFORM` | `PLANNED` | — |
| `FEAT-MON-011` | **Storage Monitoring**<br>Storage Monitoring | Memantau kapasitas/kesehatan storage. | `OBSERVABILITY_CAPABILITY` | `ADMIN` | `TELEMETRY→METRIC/ALERT` | `E1` | `LOW` | Must / MVP | NFR-MON-001..003; NFR-PERF-010 | `OBSERVABILITY_PLATFORM` | `PLANNED` | — |
| `FEAT-MON-012` | **Application Version Monitoring**<br>Application Version Monitoring | Mencatat versi aplikasi. | `OBSERVABILITY_CAPABILITY` | `ADMIN` | `TELEMETRY→METRIC/ALERT` | `E1` | `MEDIUM` | Must / MVP | NFR-MON-001..003; NFR-PERF-010 | `OBSERVABILITY_PLATFORM` | `PLANNED` | — |
| `FEAT-MON-013` | **Correlation ID Tracking**<br>Correlation ID Tracking | Menelusuri job/request lintas komponen. | `OBSERVABILITY_CAPABILITY` | `ADMIN` | `TELEMETRY→METRIC/ALERT` | `E1` | `LOW` | Must / MVP | NFR-MON-001..003; NFR-PERF-010 | `OBSERVABILITY_PLATFORM` | `PLANNED` | — |
| `FEAT-MON-014` | **Availability Monitoring**<br>Availability Monitoring | Memantau ketersediaan layanan. | `OBSERVABILITY_CAPABILITY` | `ADMIN` | `TELEMETRY→METRIC/ALERT` | `E1` | `LOW` | Must / MVP | NFR-MON-001..003; NFR-PERF-010 | `OBSERVABILITY_PLATFORM` | `PLANNED` | — |
| `FEAT-MON-015` | **Health Check**<br>Health Check | Menyediakan pemeriksaan kesehatan layanan. | `OBSERVABILITY_CAPABILITY` | `ADMIN` | `TELEMETRY→METRIC/ALERT` | `E1` | `LOW` | Must / MVP | NFR-MON-001..003; NFR-PERF-010 | `OBSERVABILITY_PLATFORM` | `PLANNED` | — |
| `FEAT-MON-016` | **Client Crash/Error Monitoring**<br>Client Crash/Error Monitoring | Memantau crash/error client tanpa data sensitif. | `OBSERVABILITY_CAPABILITY` | `ADMIN` | `TELEMETRY→METRIC/ALERT` | `E1` | `LOW` | Must / MVP | NFR-MON-001..003; NFR-PERF-010 | `OBSERVABILITY_PLATFORM` | `PLANNED` | — |
| `FEAT-MON-017` | **Critical Alert**<br>Critical Alert | Memberikan alert pada kondisi kritis. | `OBSERVABILITY_CAPABILITY` | `ADMIN` | `TELEMETRY→METRIC/ALERT` | `E1` | `LOW` | Must / MVP | NFR-MON-001..003; NFR-PERF-010 | `OBSERVABILITY_PLATFORM` | `PLANNED` | — |

### 5.54 PERTAMINA OTO Assistant — Assistant

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-ASSIST-001` | **PERTAMINA OTO Assistant**<br>PERTAMINA OTO Assistant | Antarmuka percakapan untuk mengakses dan mengorkestrasi fitur aplikasi. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-002` | **Offline Chat Interface**<br>Offline Chat Interface | UI percakapan tetap dapat digunakan tanpa koneksi. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-003` | **Local Intent Router**<br>Local Intent Router | Memahami perintah umum secara lokal tanpa AI server. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-004` | **Natural Language Command**<br>Natural Language Command | Menjalankan perintah natural seperti gabungkan PDF lalu kompres. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-005` | **Intent Detection**<br>Intent Detection | Menentukan maksud user dari perintah natural. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-006` | **Parameter Extraction**<br>Parameter Extraction | Mengambil parameter seperti nomor halaman, ukuran, kualitas, posisi, atau format output. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-007` | **Multi-Step Workflow Planning**<br>Multi-Step Workflow Planning | Mengubah satu perintah menjadi beberapa proses berurutan. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-008` | **Plan Preview**<br>Plan Preview | Menampilkan rencana tindakan sebelum eksekusi bila diperlukan. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-009` | **Canonical Tool Invocation**<br>Canonical Tool Invocation | Assistant hanya memanggil Feature ID resmi yang tersedia di Feature Catalog. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | Unsupported Feature Protection |
| `FEAT-ASSIST-010` | **Capability-Aware Assistant**<br>Capability-Aware Assistant | Menyesuaikan rekomendasi dengan platform, offline/online, permission, dan capability perangkat. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-011` | **Local/Server AI Routing**<br>Local/Server AI Routing | Memilih parser lokal atau layanan AI internal hanya ketika diperlukan. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-012` | **Offline Workflow Execution**<br>Offline Workflow Execution | Menjalankan workflow yang seluruh komponennya mendukung pemrosesan lokal. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-013` | **Contextual Help**<br>Contextual Help | Menjelaskan fungsi tool dan cara penggunaannya. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-014` | **Repeat Workflow**<br>Repeat Workflow | Menjalankan ulang workflow sebelumnya. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-015` | **Error Explanation**<br>Error Explanation | Menjelaskan error dengan bahasa yang mudah dipahami. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-016` | **Recovery Guidance**<br>Recovery Guidance | Membimbing user memilih lanjutkan, coba lagi, atau batalkan. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-017` | **Permission-Aware Assistant**<br>Permission-Aware Assistant | Assistant tidak dapat melewati role/permission. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `MEDIUM` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-018` | **Safe Confirmation Routing**<br>Safe Confirmation Routing | Konfirmasi hanya untuk tindakan final, irreversible, upload, share, atau berisiko. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-019` | **Document Content Isolation**<br>Document Content Isolation | Isi dokumen tidak otomatis dianggap sebagai instruksi Assistant. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-020` | **Prompt-Injection Protection**<br>Prompt-Injection Protection | Instruksi di dalam dokumen diperlakukan sebagai data, bukan perintah sistem. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `HIGH` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-021` | **Sensitive Content Protection**<br>Sensitive Content Protection | Isi dokumen tidak otomatis dikirim ke AI/server. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `HIGH` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-022` | **Feature Schema Validation**<br>Feature Schema Validation | Assistant hanya dapat mengirim parameter yang valid untuk Feature ID terkait. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-023` | **Assistant Cancel/Undo Integration**<br>Assistant Cancel/Undo Integration | Menggunakan Undo atau Cancel sesuai karakter tindakan. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-024` | **Assistant History**<br>Assistant History | Menyimpan metadata riwayat perintah/workflow secara aman sesuai kebijakan. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-025` | **Assistant Bahasa Indonesia & English**<br>Assistant Indonesian & English | Percakapan mendukung kedua bahasa aplikasi. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-026` | **Optional Local Small Language Model**<br>Optional Local Small Language Model | Model lokal kecil dapat ditambahkan terutama pada Windows Desktop setelah benchmark dan approval. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-L` | `LOW` | Could / R3 | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `FEASIBILITY_GATE` | — |
| `FEAT-ASSIST-027` | **Internal AI Fallback**<br>Internal AI Fallback | Instruksi kompleks dapat menggunakan AI internal ketika online dan diizinkan policy. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-S` | `HIGH` | Should / R2 | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-028` | **Assistant Ambiguity Clarification**<br>Assistant Ambiguity Clarification | Meminta klarifikasi terarah ketika intent, target, atau parameter penting ambigu. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-029` | **Assistant Result Grounding**<br>Assistant Result Grounding | Pernyataan hasil harus bersumber dari output/status tool aktual. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |
| `FEAT-ASSIST-030` | **Clear Assistant Conversation**<br>Clear Assistant Conversation | Memungkinkan user membersihkan conversation state lokal tanpa menghapus audit wajib. | `ASSISTANT_CAPABILITY` | `ASSISTANT` | `USER_INTENT+CONTEXT→VALIDATED_PLAN/ACTION` | `AI-H` | `LOW` | Should / MVP | FR-ASSIST-001..015; SR-SEC-013..018 | `ASSISTANT_ORCHESTRATOR` | `PLANNED` | — |

### 5.55 Account/Admin/Audit/System — Support

| Feature ID | Nama ID / EN | Fungsi | Type | UI | Contract | Exec | Risk | Priority / Release | Requirement | Dependency | Status | Alias/Cross-ref |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `FEAT-SUPPORT-001` | **Safe Diagnostic/Support Package**<br>Safe Diagnostic/Support Package | Membuat paket diagnostik aman berisi metadata/log teknis tanpa isi dokumen atau secret. | `ENTERPRISE_CAPABILITY` | `CONTEXT_ACTION` | `IDENTITY/METADATA/POLICY→ACCESS/CONFIG/REPORT` | `E1` | `LOW` | Must / MVP | FR-SUP-004; NFR-MON-004 | `SUPPORT_DIAGNOSTICS` | `PLANNED` | — |
| `FEAT-SUPPORT-002` | **In-App Issue Reporting**<br>In-App Issue Reporting | User melaporkan ketidaksesuaian langsung dari PERTAMINA OTO menggunakan pesan singkat dan screenshot opsional. | `ENTERPRISE_CAPABILITY` | `CONTEXT_ACTION` | `TEXT/OPTIONAL_SCREENSHOT+CONTEXT→ISSUE_REPORT` | `E1` | `LOW` | Must / MVP | FR-SUP-001..002; UIR-UI-013 | `ISSUE_REPORTING_SERVICE` | `PLANNED` | Report a Problem |
| `FEAT-SUPPORT-003` | **Issue Screenshot Capture & Markup**<br>Issue Screenshot Capture & Markup | Mengambil screenshot aplikasi, preview, dan markup sederhana sebelum user mengirim laporan. | `PLATFORM_CAPABILITY` | `CONTEXT_ACTION` | `UI_FRAME→IMAGE/ANNOTATED_IMAGE` | `L1` | `MEDIUM` | Must / MVP | FR-SUP-003; UIR-UI-014; SR-SEC-019 | `SCREENSHOT_ADAPTER` | `PLANNED` | Screenshot Report |
| `FEAT-SUPPORT-004` | **User Issue Status Tracking**<br>User Issue Status Tracking | User melihat status sederhana laporan miliknya sendiri. | `ENTERPRISE_CAPABILITY` | `CONTEXT_ACTION` | `IDENTITY+ISSUE_ID→STATUS` | `E1` | `LOW` | Should / MVP | FR-SUP-006 | `ISSUE_REPORTING_SERVICE` | `PLANNED` | My Reports |
| `FEAT-SUPPORT-005` | **Superadmin Issue Console**<br>Superadmin Issue Console | Halaman khusus Superadmin/Support untuk melihat issue, screenshot, dan safe diagnostics sesuai permission. | `ENTERPRISE_CAPABILITY` | `SUPERADMIN` | `IDENTITY/PERMISSION+ISSUE_QUERY→ISSUE_LIST/DETAIL` | `E1` | `HIGH` | Must / MVP | FR-SUP-005; SR-SEC-020 | `ISSUE_REPORTING_SERVICE` | `PLANNED` | Support Console |
| `FEAT-SUPPORT-006` | **Issue Triage & Repair Workflow**<br>Issue Triage & Repair Workflow | Superadmin mengatur kategori, prioritas, status, catatan, duplicate/bukan bug, dan kesiapan perbaikan/testing. | `ENTERPRISE_CAPABILITY` | `SUPERADMIN` | `ISSUE+ADMIN_ACTION→ISSUE_STATE` | `E1` | `HIGH` | Must / MVP | FR-SUP-005 | `ISSUE_REPORTING_SERVICE` | `PLANNED` | Issue Management |
| `FEAT-SUPPORT-007` | **Export Issue Detail PDF**<br>Export Issue Detail PDF | Mengekspor satu issue lengkap ke PDF sebagai bahan tugas perbaikan programmer. | `ENTERPRISE_CAPABILITY` | `SUPERADMIN` | `ISSUE_DETAIL→PDF` | `E1` | `MEDIUM` | Must / MVP | FR-SUP-007 | `REPORT_EXPORT_SERVICE` | `PLANNED` | Repair Report PDF |
| `FEAT-SUPPORT-008` | **Export Issue Register Excel**<br>Export Issue Register Excel | Mengekspor rekap issue terfilter ke Excel untuk tracking dan penerusan ke programmer. | `ENTERPRISE_CAPABILITY` | `SUPERADMIN` | `ISSUE_QUERY→XLSX` | `E1` | `MEDIUM` | Must / MVP | FR-SUP-007 | `REPORT_EXPORT_SERVICE` | `PLANNED` | Issue Register Excel |


## 6. Anggaran Sumber Daya Server dan Metadata Teknis

Setiap record di atas mewarisi metadata server dari `Exec` profile. Field berikut **wajib** dipertahankan pada implementasi/database konfigurasi Feature Registry:

```text
FeatureId
RecordType
UiExposure
InputOutputContract
ExecutionProfile
ProcessingMode
ServerDependency
OfflineSupport
StorageImpact
NetworkImpact
ConcurrencyRisk
SecurityRisk
Destructive
Priority
Release
RequirementRefs
DependencyFamily
Status
Aliases
ParameterSchemaVersion
BuildPriority
DesktopExposure
TabletExposure
PhoneExposure
```

Rule: perubahan `ParameterSchemaVersion`, input/output contract, destructive flag, permission, atau server dependency adalah perubahan yang harus melalui impact analysis dan regression test.

`BuildPriority` minimum: `CORE` untuk capability yang mendukung 33 Primary Tool Cards; `STANDARD` untuk user-tool sekunder; `FOUNDATION` untuk capability platform/security yang menjadi dependency.

---

### 6.1 Kebijakan Eksposur Platform

Feature Catalog tetap canonical tunggal, tetapi exposure per perangkat mengikuti kebijakan:

| Nilai | Arti |
|---|---|
| `FULL` | Implementasi/UX terlengkap pada platform |
| `PRIMARY` | Fitur sangat sesuai dan diprioritaskan pada platform |
| `SUPPORTED` | Tersedia dengan UX/capability normal |
| `LIMITED` | Tersedia dengan fungsi/parameter yang disederhanakan |
| `NOT_EXPOSED` | Tidak ditampilkan pada platform tersebut |

Baseline:

- **Desktop** adalah target `FULL` untuk feature set produk, kecuali capability yang secara alami khusus mobile atau masih `FEASIBILITY_GATE`.
- **Tablet** menggunakan curated exposure; dapat lebih kaya daripada phone.
- **Phone** menggunakan curated exposure untuk scan, read PDF, visual signature/paraf, OCR/editing ringan, merge/compress sederhana, share, Amankan ke Akun, dan capability praktis lainnya.
- Platform exposure tidak mengubah Feature ID atau business rule.
- Setelah exposure mengizinkan fitur, runtime tetap melakukan capability, permission, policy, resource, engine, dan connectivity check.

## 7. Rekonsiliasi Merge/Alias Canonical

Entry berikut **tidak hilang**; semuanya dipetakan ke canonical Feature ID yang sama karena contract/implementation capability-nya identik:

| Raw No | Raw Entry | Canonical Feature ID | Canonical Name |
|---|---|---|---|
| 98 | Scan to Searchable PDF | `FEAT-OCR-004` | Searchable PDF OCR |
| 100 | Scan to Text | `FEAT-OCR-005` | OCR Text Extraction |
| 335 | Encrypted Upload | `FEAT-SEC-007` | Encryption in Transit |
| 336 | Encrypted Server Storage | `FEAT-SEC-006` | Encryption at Rest |
| 343 | Backup Version Info | `FEAT-VER-003` | View Version Information |
| 434 | Login Audit | `FEAT-AUDIT-001` | Enterprise Audit Trail |
| 435 | Logout Audit | `FEAT-AUDIT-001` | Enterprise Audit Trail |
| 436 | Failed Login Audit | `FEAT-AUDIT-001` | Enterprise Audit Trail |
| 437 | Feature Usage Audit | `FEAT-AUDIT-001` | Enterprise Audit Trail |
| 438 | Process Audit | `FEAT-AUDIT-001` | Enterprise Audit Trail |
| 439 | Local/Server Processing Audit | `FEAT-AUDIT-001` | Enterprise Audit Trail |
| 440 | Error Audit | `FEAT-AUDIT-001` | Enterprise Audit Trail |
| 441 | Role Change Audit | `FEAT-AUDIT-001` | Enterprise Audit Trail |
| 442 | Permission Change Audit | `FEAT-AUDIT-001` | Enterprise Audit Trail |
| 443 | Configuration Change Audit | `FEAT-AUDIT-001` | Enterprise Audit Trail |
| 452 | Current Step | `FEAT-PROG-001` | Detailed Progress Status |
| 453 | Current File/Page/Sheet/Slide | `FEAT-PROG-001` | Detailed Progress Status |
| 454 | Completed Count | `FEAT-PROG-001` | Detailed Progress Status |
| 455 | Remaining Count | `FEAT-PROG-001` | Detailed Progress Status |
| 456 | ETA | `FEAT-PROG-001` | Detailed Progress Status |
| 471 | Localized Progress | `FEAT-UX-006` | Localized UI Resources |
| 472 | Localized Notifications | `FEAT-UX-006` | Localized UI Resources |
| 473 | Localized Tooltips | `FEAT-UX-006` | Localized UI Resources |
| 474 | Localized Accessibility Text | `FEAT-UX-006` | Localized UI Resources |
| 529 | Conversational Tool Search | `FEAT-FLOW-007` | Natural Language Tool Search |
| 533 | Feature Recommendation Assistant | `FEAT-FLOW-005` | Smart Recommendations |
| 539 | Queue Until Online | `FEAT-PROC-013` | Offline Job Queue |
| 541 | Workflow Preset Creation | `FEAT-PROD-004` | Preset |
| 551 | Unsupported Feature Protection | `FEAT-ASSIST-009` | Canonical Tool Invocation |
| 552 | Assistant Activity Progress | `FEAT-PROG-008` | Activity Center |

---

## 8. Gerbang Kelayakan — Sebelum Coding Penuh

Feature berikut tidak boleh langsung dikunci ke library/engine tanpa Proof-of-Capability:

| Kandidat | Gate minimum |
|---|---|
| Edit Existing PDF Text | Fidelity, font/layout preservation, unsupported-PDF behavior, license |
| Office → PDF | Fidelity, pagination, font, formula/chart, headless/server support, license |
| PDF → Word/Excel/PowerPoint | Accuracy, layout/table fidelity, batch behavior, fallback |
| OCR tabel kompleks | Accuracy per sample, language/table structure, CPU/RAM, fallback |
| Background Removal | Quality, local/server resource, privacy, model/license |
| HTML/Web → PDF | Rendering fidelity, security/isolation, external-resource policy |
| PDF/A | Conformance validation, font embedding, metadata, repair behavior |
| Physical Scanner Integration | Driver/platform support, packaging, device permissions |
| Optional Local Small Language Model | Model size, RAM/CPU, latency, license, update, security |

Gate harus menghasilkan keputusan `APPROVED_ENGINE`, `APPROVED_WITH_LIMITATION`, atau `BLOCKED/ALTERNATIVE_REQUIRED` sebelum implementasi penuh.

---

## 9. Kontrak Assistant

Assistant mengikuti urutan:

```text
User Intent
→ Local Intent / Alias Resolution
→ Canonical Feature ID
→ Parameter Schema Validation
→ Input/Output Compatibility
→ Capability + Permission + Policy Check
→ Resource Preflight
→ Confirmation bila berisiko
→ Existing Application Service
→ Tool Result Grounding
```

- Assistant bukan processing engine.
- Assistant tidak memiliki direct SQL, arbitrary shell, unrestricted file access, atau arbitrary API execution.
- Dokumen/OCR content adalah untrusted data dan tidak dapat mengubah system policy.
- Internal AI fallback tidak boleh menjadi single point of failure untuk capability lokal.

---

## 10. Ruang Lingkup Digital Signature

- Visual signature/paraf/stamp/anotasi non-PKI **termasuk scope**.
- Digital Signature berbasis certificate/PKI **tidak termasuk scope saat ini** dan tidak mempunyai Feature ID implementasi.

---

## 11. Ketertelusuran dan Definition of Done

Setiap Feature ID harus dapat ditelusuri:

```text
Business Process
→ Requirement ID
→ Feature ID
→ Solution Component
→ Code Module
→ Unit/Integration Test
→ Test Case
→ UAT
→ Release
```

Feature hanya `DONE` setelah delapan lapis terpenuhi: Technical Correctness; Business Process Correctness; UI/UX Acceptance; Security & Privacy; Performance/Server Budget; Error & Recovery; Cross-Platform Behavior; Test & Documentation.

---

## 12. Ringkasan Rekonsiliasi

| Item | Jumlah |
|---|---:|
| Raw inventory yang telah disetujui | 557 |
| Tambahan hasil audit pre-solution | 12 |
| Tambahan Solution Design — Issue Reporting | 7 |
| Tambahan Primary Feature Audit | 4 |
| Raw candidate total | 580 |
| Entry yang menjadi alias/cross-reference | 30 |
| **Canonical Feature Record** | **550** |

Tidak ada raw capability yang dibuang tanpa mapping. Entry yang tidak menjadi canonical record tetap tercatat pada bagian alias/cross-reference.

---

## 13. Status Dokumen

```text
Nama dokumen : 03_FEATURE_CATALOG.md
Versi        : 1.3
Status       : FINAL — SOLUTION-ALIGNED FEATURE BASELINE
Keputusan    : 10 dari 10 selesai
Canonical    : 550 Feature Records
Progress     : 100%
```

Dokumen berikutnya:

```text
04_SOLUTION_DESIGN.md
```
