# REGISTER KEPUTUSAN UAT
## PERTAMINA OTO — Office Tools Organizer

> **Dokumen:** 13 — UAT Document  
> **Versi:** 1.0  
> **Tanggal:** 1 September 2026  
> **Status:** FINAL — 25/25 AUTO-LOCKED  
> **Dasar:** Dokumen 01–12 FINAL dan laporan Agent A–D  
> **Baseline fitur:** 550 fitur canonical

---

# 1. Aturan Penguncian

Keputusan berikut tidak membuat proses bisnis, Feature ID, role, platform, security control, atau SLA baru. Seluruhnya diturunkan dari Dokumen 01–12 dan karena itu dikunci otomatis. Detail yang harus ditetapkan oleh pemilik standar Pertamina tetap diberi label `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` dan tidak diisi dengan asumsi.

---

# 2. Keputusan yang Dikunci

| ID | Keputusan | Status | Dasar utama |
|---|---|---|---|
| `UAT-AUTO-001` | UAT menilai penerimaan pengguna dan hasil bisnis; bukan pengulangan test teknis | **AUTO-LOCKED** | Dokumen 01, 02, 11, 12 |
| `UAT-AUTO-002` | FINAL pada Dokumen 13 berarti spesifikasi UAT telah final, bukan bukti bahwa UAT produk telah dijalankan atau lulus | **AUTO-LOCKED** | Batas pre-coding dan evidence-based governance |
| `UAT-AUTO-003` | Delapan lapisan Definition of Done dipakai; empat lapisan teknis menjadi entry evidence dan empat lapisan pengguna menjadi fokus UAT | **AUTO-LOCKED** | Dokumen 12 |
| `UAT-AUTO-004` | Kedalaman UAT berbasis risiko: `D1-STANDARD`, `D2-ENHANCED`, dan `D3-CRITICAL` | **AUTO-LOCKED** | Risk-based testing Dokumen 11–12 |
| `UAT-AUTO-005` | Actor aplikasi tetap User, Administrator, Auditor, serta profil Support/Superadmin berbasis permission; fungsi UAT bukan role aplikasi | **AUTO-LOCKED** | Dokumen 09 |
| `UAT-AUTO-006` | UAT memakai release candidate immutable pada logical UAT lane terkontrol; topology fisik menunggu konfirmasi | **AUTO-LOCKED** | Dokumen 12 |
| `UAT-AUTO-007` | Data UAT mengutamakan synthetic/generated; data representative harus disanitasi dan disetujui | **AUTO-LOCKED** | Dokumen 08 dan 12 |
| `UAT-AUTO-008` | Status penerimaan skenario hanya `PASS`, `FAIL`, `BLOCKED`, dan `NOT_APPLICABLE` | **AUTO-LOCKED** | Instruksi Dokumen 13 |
| `UAT-AUTO-009` | Skenario yang belum dijalankan tidak diberi hasil; field hasil dibiarkan kosong pada rencana eksekusi | **AUTO-LOCKED** | Larangan false PASS dan empat status resmi |
| `UAT-AUTO-010` | Workflow dan severity defect mengikuti istilah canonical Dokumen 12: `NEW`, `CONFIRMED`, `IN_PROGRESS`, `READY_FOR_RETEST`, `VERIFIED`, `CLOSED`; disposition `AS_DESIGNED`/`DUPLICATE` | **AUTO-LOCKED** | Dokumen 12 sebagai sumber lebih tinggi |
| `UAT-AUTO-011` | `FOUND`, `IN PROGRESS`, `READY FOR RETEST`, dan `NOT A BUG` hanya boleh menjadi label tampilan untuk `NEW`, `IN_PROGRESS`, `READY_FOR_RETEST`, dan `AS_DESIGNED`; bukan enum baru | **AUTO-LOCKED** | Normalisasi laporan Agent D |
| `UAT-AUTO-012` | Tidak ada Blocker atau Critical terbuka pada entry maupun final acceptance; Critical tidak dapat diterima sebagai known issue oleh Dokumen 13 | **AUTO-LOCKED** | Gate Dokumen 12 dan larangan pengurangan security |
| `UAT-AUTO-013` | File-First, Tool-First, dan Assistant-First harus memakai Feature canonical, aturan, permission, konfirmasi, hasil, dan recovery yang sama | **AUTO-LOCKED** | Triple Entry Flow Dokumen 01–04 |
| `UAT-AUTO-014` | Seluruh 33 Primary PDF Tool Cards wajib mempunyai coverage; kedalaman berbeda mengikuti risiko | **AUTO-LOCKED** | Core Set Dokumen 03, 05, 11, 12 |
| `UAT-AUTO-015` | `FEAT-PDF-005 — Merge PDF` menjadi contoh paling lengkap dengan 27 skenario UAT dan tetap diproses lokal | **AUTO-LOCKED** | Vertical slice Dokumen 03, 05, 11, 12 |
| `UAT-AUTO-016` | Offline-First dan Local-First diuji berdasarkan capability; Feature local-only tidak memakai processing API atau upload server | **AUTO-LOCKED** | Dokumen 04–08 |
| `UAT-AUTO-017` | “Amankan ke Akun” adalah selective sync eksplisit, bukan generic workspace sync, dengan maksimum tiga versi aktif/addressable | **AUTO-LOCKED** | Dokumen 02, 07–10 |
| `UAT-AUTO-018` | Windows Desktop paling lengkap; Web, Phone, dan Tablet diuji sesuai exposure/capability, bukan dipaksa identik | **AUTO-LOCKED** | Adaptive UX Dokumen 01–05 |
| `UAT-AUTO-019` | `NOT_APPLICABLE` hanya sah dengan bukti release scope/capability; capability yang dijanjikan tetapi gagal adalah `FAIL`; dependency yang belum siap adalah `BLOCKED` | **AUTO-LOCKED** | Risk/capability baseline |
| `UAT-AUTO-020` | Bahasa Indonesia menjadi fallback utama; English juga diuji; identifier canonical tidak diterjemahkan | **AUTO-LOCKED** | Standar bahasa `ID-1.0` dan adaptive UX |
| `UAT-AUTO-021` | Accessibility UAT hanya menyatakan basic usability sesuai scope; tidak mengklaim sertifikasi tanpa standar resmi | **AUTO-LOCKED** | Larangan membuat standar/SLA baru |
| `UAT-AUTO-022` | Recovery harus memenuhi No Silent Wait, source safety, finality, current authorization, dan retry yang bounded/idempotent | **AUTO-LOCKED** | Dokumen 06, 08, 10, 12 |
| `UAT-AUTO-023` | Evidence diminimalkan, disanitasi, traceable, dan dipisahkan dari Audit, History, Support, serta Observability | **AUTO-LOCKED** | Dokumen 08, 10, 12 |
| `UAT-AUTO-024` | UAT sign-off hanya oleh manusia berwenang dan terikat build/scope; UAT acceptance tidak menyiratkan persetujuan Production | **AUTO-LOCKED** | Governance Dokumen 12 |
| `UAT-AUTO-025` | Temuan usage/admin/support diteruskan ke Dokumen 14/15/16 tanpa menulis manual atau runbook lengkap di Dokumen 13 | **AUTO-LOCKED** | Boundary roadmap 13–16 |

---

# 3. Konfirmasi Pertamina Sebelum Eksekusi UAT

Item berikut bukan keputusan desain baru dan tidak menghalangi finalisasi spesifikasi Dokumen 13, tetapi wajib tersedia sebelum skenario terkait dijalankan:

1. pemilik keputusan UAT, delegasi, dan mekanisme sign-off resmi;
2. matriks Feature × platform × device/browser untuk release candidate;
3. logical lane/topology, reset window, dan support path UAT;
4. akun, permission, delegated scope, reauthentication, dan offline-grant fixture;
5. approved engine/provider/device build serta batas PoC;
6. dataset, glossary, answer key, dan sanitization standard;
7. policy retention, legal hold, reauthentication, dan secured-version lifecycle;
8. lokasi, klasifikasi, akses, retensi, dan disposal evidence.

---

# 4. Hasil

```text
Keputusan UAT terkunci : 25 / 25
Konflik material terbuka: 0
Keputusan user tertunda : 0
Status Dokumen 13       : FINAL v1.0
```
