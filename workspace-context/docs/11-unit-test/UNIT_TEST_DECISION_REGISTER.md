# REGISTER KEPUTUSAN UNIT TEST
## PERTAMINA OTO — Office Tools Organizer

> **Versi:** 1.0  
> **Tanggal:** 28 Agustus 2026  
> **Status:** FINAL — 20/20 AUTO-LOCKED  
> **Dokumen:** `11_UNIT_TEST_DOCUMENT.md`  
> **Baseline:** Dokumen 01–10 FINAL; 550 fitur canonical; 33 Primary PDF Tool Cards

---

# 1. Prinsip Penguncian

Keputusan berikut diturunkan dari Dokumen 01–10 dan laporan Agent A–D. Seluruh keputusan bersifat teknis, tidak mengubah requirement, Feature ID, kontrak API, rule keamanan, atau ruang lingkup bisnis.

---

# 2. Keputusan

| ID | Keputusan yang dikunci | Alasan/ketertelusuran | Status |
|---|---|---|---|
| `UT-DEC-001` | Unit Test menguji perilaku kecil dan deterministik pada Domain, Application, policy, validator, mapper, resolver, state machine, serta adapter murni | Dokumen 02 §16–17; Dokumen 04 §29–30 | AUTO-LOCKED |
| `UT-DEC-002` | MSTest menjadi framework utama dan Microsoft.Testing.Platform menjadi runner/platform utama | `TECH-DEC-013`; `IR-DEVOPS-004` | AUTO-LOCKED |
| `UT-DEC-003` | Project Unit Test mengikuti ownership assembly/modul produksi; tidak ada satu project global yang menembus internal semua modul | Modular Monolith, Clean Architecture, ownership Dokumen 04/07 | AUTO-LOCKED |
| `UT-DEC-004` | Nama test menggunakan `MethodOrBehavior_Condition_ExpectedResult` | Arahan user dan kebutuhan keterbacaan/maintenance | AUTO-LOCKED |
| `UT-DEC-005` | Struktur test menggunakan Arrange–Act–Assert; Given–When–Then boleh dipakai konsisten untuk decision table/state machine | Arahan user; determinisme dan readability | AUTO-LOCKED |
| `UT-DEC-006` | Real Domain/Application behavior dijalankan; fake/stub/spy hanya untuk batas nondeterministik, privileged, lambat, atau infrastruktur | Mencegah excessive mocking dan false assurance | AUTO-LOCKED |
| `UT-DEC-007` | Clock, ID, randomness, scheduler, connectivity, capability, configuration, dan policy harus dapat dikontrol deterministik | Error/retry/expiry/offline harus repeatable | AUTO-LOCKED |
| `UT-DEC-008` | Setiap test mengisolasi mutable state; parallel execution diizinkan untuk test aman; `DoNotParallelize` hanya pengecualian sempit terdokumentasi | Mencegah flaky test dan coupling urutan | AUTO-LOCKED |
| `UT-DEC-009` | Test data hanya sintetis; Production data, credential, dokumen rahasia, tanda tangan nyata, dan secret dilarang | `SR-SEC-003..004,010..018`; Dokumen 08/10 | AUTO-LOCKED |
| `UT-DEC-010` | Prioritas test menggunakan P0 keamanan/bisnis kritis, P1 jalur Core, P2 perilaku normal, P3 edge case kredibel | Menjaga fokus risiko tanpa menyamakan test priority dengan MoSCoW | AUTO-LOCKED |
| `UT-DEC-011` | Coverage adalah sinyal pendukung; behavior matrix, negative test, branch kritis, dan changed-code regression lebih berwenang daripada persentase | Tidak ada threshold Pertamina yang telah disetujui | AUTO-LOCKED |
| `UT-DEC-012` | Quality gate PR mencakup scope, restore terkendali, Release build/analyzer, seluruh Unit Test, P0/P1, traceability, integritas test, dan human review | `IR-DEVOPS-003..004`; Definition of Done 8 lapis | AUTO-LOCKED |
| `UT-DEC-013` | Flaky test adalah defect; rerun hanya untuk diagnosis dan tidak mengubah run gagal menjadi hijau | Keandalan CI dan anti-false-green | AUTO-LOCKED |
| `UT-DEC-014` | Traceability minimum adalah Requirement ID → Feature ID → business/security rule → component → Test ID/method → hasil pipeline | `BR-GOV-003`; Dokumen 02 §17 | AUTO-LOCKED |
| `UT-DEC-015` | 33 Core Tools diuji melalui shared contract per implementation family ditambah critical feature-specific tests | Mencegah 33 × ratusan test copy-paste | AUTO-LOCKED |
| `UT-DEC-016` | `FEAT-PDF-005 — Merge PDF` menjadi spesifikasi Unit Test vertical slice pertama yang lengkap | Dokumen 03/05/Master Summary | AUTO-LOCKED |
| `UT-DEC-017` | Job state mencakup `QUEUED`, `RUNNING`, `WAITING_CONNECTION`, `WAITING_RESOURCE`, `RETRYING`, `COMPLETED`, `FAILED`, `CANCELLED`; `PAUSED` tidak valid | Dokumen 04/06/07; konflik prompt diselesaikan dari baseline berwenang | AUTO-LOCKED |
| `UT-DEC-018` | Unit suite wajib mencakup authorization, ownership, privacy, Local-First, no-auto-sync, retry/idempotency, Assistant no-bypass, dan Audit safe metadata | Dokumen 06/08/09/10 | AUTO-LOCKED |
| `UT-DEC-019` | Unit Test tidak membuktikan SQL/SQLite, filesystem atomik, HTTP/SignalR/IPC nyata, engine fidelity, perangkat, performance, VA/PT, atau UAT | Batas Document 11 dan handoff ke Document 12/13/PoC | AUTO-LOCKED |
| `UT-DEC-020` | Threshold numerik coverage, durasi, worker, timeout, retry, dan retensi report tidak dikarang; gunakan konfigurasi terpusat setelah baseline/approval | `NFR-PERF-006`; tata kelola Pertamina | AUTO-LOCKED |

---

# 3. Aturan Turunan

1. Merge PDF membutuhkan minimum dua input; satu input ditolak sebagai command Merge yang tidak bermakna.
2. Input duplikat eksplisit tidak boleh dideduplikasi diam-diam; occurrence dan order dipertahankan sampai ada kontrak terkendali yang menyatakan lain.
3. Pure DTO/query/header/error/idempotency mapping dapat diuji sebagai Unit Test. Generated OpenAPI, wire serialization, middleware, HTTP header aktual, dan endpoint wiring menjadi Contract/Integration Test pada Dokumen 12.
4. `ContentIntelligence` hanya nama kelompok test logis sampai ownership assembly produksi ditetapkan dalam task scaffold; keputusan ini tidak membuat modul produksi baru.
5. Exact schema, error code yang belum canonical, SSO/reauth/offline lifetime, dan numeric limits dipasok sebagai typed contract/policy. Unit Test tidak boleh mengarang nilai Produksi.

---

# 4. Status

```text
Keputusan teknis : 20 / 20 AUTO-LOCKED
Konflik material : 0
Persetujuan user : 0 diperlukan
Perubahan 01–10  : 0
```
