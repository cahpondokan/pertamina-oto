# BASELINE MANIFEST RECONCILIATION CANDIDATE
## PERTAMINA OTO

> Tanggal persiapan: 2 September 2026  
> Status: **CANDIDATE — BELUM MENGUBAH BASELINE**  
> Target Baseline ID: `PERTAMINA-OTO-DOCS-BASELINE-1.0`  
> Tujuan: menyiapkan koreksi terkontrol untuk entri `MASTER` tanpa mengubah DOC-01 sampai DOC-19 atau menganggap persetujuan manusia telah diberikan.

---

## 1. Ringkasan Perbedaan

`DOCUMENTATION_BASELINE_MANIFEST_v1.0.md` masih mencatat `MASTER_PROJECT_SUMMARY.md` v4.3, sedangkan file aktual sudah v4.4. Hash dan ukuran DOC-01 sampai DOC-19 telah diverifikasi tetap cocok dengan manifest; kandidat ini hanya membahas entri `MASTER` dan referensi supersession yang terkait.

| Field | Nilai pada manifest saat ini | Nilai aktual yang diverifikasi |
|---|---|---|
| Version | `4.3` | `4.4` |
| Bytes | `37,358` | `38,026` |
| SHA-256 | `BC9306D583822E97E54F7610155B4FD8457B9A776EAAD29216255A8FD5AE4BAF` | `9C983D8090E60A0EF42035EC6C2621C0FBEFC0CF54C9288ED8BA840C9601284C` |
| Peran | Current mutable index | Current mutable index |

Nilai aktual diverifikasi secara lokal dengan `Get-Item` dan `Get-FileHash -Algorithm SHA256` pada 2 September 2026.

---

## 2. Kandidat Perubahan

Jika Documentation Owner menyetujui rekonsiliasi, perubahan minimum yang diusulkan pada manifest adalah:

1. Ganti baris `MASTER` pada §2 menjadi:

```text
| `MASTER` | `MASTER_PROJECT_SUMMARY.md` | 4.4 | Current mutable index | 38,026 | `9C983D8090E60A0EF42035EC6C2621C0FBEFC0CF54C9288ED8BA840C9601284C` | YES — index, bukan domain authority |
```

2. Ganti referensi pada §4 dari `Current: MASTER_PROJECT_SUMMARY.md v4.3` menjadi `Current: MASTER_PROJECT_SUMMARY.md v4.4`.
3. Tambahkan catatan perubahan bahwa v4.3 digantikan oleh v4.4 sebagai index/status, bukan sebagai domain authority.
4. Hitung ulang hash manifest candidate setelah perubahan diterapkan.
5. Bekukan file yang akan dinilai sebelum meminta acceptance agar hash approval tidak berubah di tengah review.

Kandidat ini tidak mengizinkan perubahan terhadap isi atau hash DOC-01 sampai DOC-19.

---

## 3. Pemeriksaan Sebelum Diterapkan

```text
[x] MASTER aktual ditemukan pada path canonical
[x] Ukuran MASTER aktual dihitung
[x] SHA-256 MASTER aktual dihitung
[x] Hash DOC-01 sampai DOC-19 dilaporkan cocok oleh audit BOOT-001
[ ] Documentation Owner menyetujui cara rekonsiliasi: revisi manifest atau addendum
[ ] Paket audit/manifest/roadmap/open-item/change-register dibekukan
[ ] Owner, Reviewer, Approver, tanggal, dan lokasi evidence ditetapkan
[ ] Manifest hasil rekonsiliasi dihitung ulang dan diperiksa reviewer
```

---

## 4. Keputusan Manusia yang Diperlukan

```text
Pilihan rekonsiliasi : [ ] Revisi manifest  [ ] Addendum terkontrol
Documentation Owner  : TBC
Reviewer             : TBC
Approver             : TBC
Tanggal keputusan    : TBC
Lokasi evidence      : TBC
Keputusan            : [ ] ACCEPTED  [ ] REVISION_REQUIRED
```

`SETUJUI PROMPT` hanya memberi izin menjalankan orkestrasi multi-agent. Pernyataan itu tidak dicatat sebagai acceptance baseline atau persetujuan atas kandidat rekonsiliasi ini.

---

## 5. Batasan

- Tidak ada dokumen FINAL yang diubah oleh kandidat ini.
- Tidak ada file yang dihapus, dipindahkan, diarsipkan, di-commit, atau di-tag.
- Tidak ada klaim bahwa baseline, BOOT-001, Phase 0, RC, UAT, atau Production sudah disetujui.
- Jika `MASTER_PROJECT_SUMMARY.md` berubah lagi sebelum acceptance, ukuran dan hash wajib dihitung ulang dan kandidat ini harus direvisi.

