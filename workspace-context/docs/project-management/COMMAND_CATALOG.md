# PERTAMINA OTO — Katalog Perintah Singkat

Gunakan kalimat di bawah ini langsung di task Codex yang berada pada workspace proyek ini. `phase`, `fase`, dan `pashe` akan dipahami sama, tetapi ejaan baku di bawah dianjurkan.

## Perintah utama

| Perintah | Efek |
|---|---|
| `kerjakan phase 0 sesuai rencana` | Memulai Phase 0 dari preflight sampai exit gate atau blocker sah |
| `kerjakan phase 1 sesuai rencana` | Memulai Phase 1 jika Phase 0 COMPLETE |
| `kerjakan phase 2 sesuai rencana` | Memulai Phase 2 jika Phase 1 COMPLETE |
| `kerjakan phase 3 sesuai rencana` | Memulai Phase 3 jika Phase 2 COMPLETE |
| `kerjakan phase 4 sesuai rencana` | Memulai Phase 4 jika Phase 3 COMPLETE |
| `kerjakan phase 5 sesuai rencana` | Memulai Phase 5 jika Phase 4 COMPLETE |
| `kerjakan phase 6 sesuai rencana` | Memulai Phase 6 jika Phase 5 COMPLETE |
| `kerjakan phase 7 sesuai rencana` | Memulai Phase 7 jika Phase 6 COMPLETE |
| `kerjakan phase 8 sesuai rencana` | Memulai Phase 8 jika Phase 7 COMPLETE |
| `kerjakan phase 9 sesuai rencana` | Memulai Phase 9 jika Phase 8 COMPLETE |
| `kerjakan phase 10 sesuai rencana` | Menjalankan Production Readiness; tidak deploy Production |
| `kerjakan phase 11 sesuai rencana` | Menjalankan launch hanya setelah seluruh otorisasi manusia tersedia |

Urutan normal setelah Phase 0 adalah Phase 1, bukan Phase 2. Jika phase sebelumnya belum `COMPLETE`, Agent 0 berhenti dengan `BLOCKED_BY_SEQUENCE` dan menunjukkan phase yang harus diselesaikan.

## Kontrol pelaksanaan

| Perintah | Efek |
|---|---|
| `kerjakan phase berikutnya sesuai rencana` | Memilih phase pertama setelah phase terakhir yang COMPLETE |
| `lanjutkan phase N sesuai rencana` | Melanjutkan dari checkpoint setelah blocker/jeda selesai |
| `status phase N` | Laporan read-only; tidak mengubah file/kode |
| `status proyek` | Ringkasan seluruh Phase 0–11 dan blocker terdekat |
| `audit phase N` | Audit bukti, trace, tests, dan exit gate tanpa implementasi |
| `jeda phase N` | Menyimpan checkpoint aman dan mengubah status menjadi PAUSED |
| `hentikan phase N` | Menghentikan pekerjaan baru tanpa menghapus hasil yang ada |

## Bentuk Goal untuk pekerjaan panjang

Bila ingin Codex mempertahankan objective jangka panjang melalui fasilitas Goal, gunakan:

```text
/goal kerjakan phase N sesuai rencana sampai exit gate lulus
```

Perintah biasa tetap cukup untuk memuat seluruh rencana dan memulai eksekusi. Prefix `/goal` dipakai bila pengguna memang ingin membuat goal persisten; framework tidak membuat goal atau jadwal otomatis tanpa permintaan eksplisit.

## Contoh alur benar

```text
kerjakan phase 0 sesuai rencana
status phase 0
lanjutkan phase 0 sesuai rencana
kerjakan phase 1 sesuai rencana
kerjakan phase 2 sesuai rencana
```

Setelah setiap perintah eksekusi, Agent 0 harus berhenti pada batas phase dan melaporkan salah satu:

- `COMPLETE`: exit gate lulus; perintah phase berikutnya boleh diberikan.
- `BLOCKED`: pekerjaan aman sudah dimaksimalkan tetapi keputusan/akses/bukti masih diperlukan.
- `PAUSED`: checkpoint aman tersimpan atas permintaan pengguna.
- `FAILED`: terjadi kegagalan yang membuat candidate tidak dapat diterima; penyebab dan recovery harus disebutkan.

## Batas kewenangan perintah singkat

Perintah `kerjakan` mengizinkan pekerjaan lokal dalam scope phase: membaca dokumen, membuat/mengubah kode dan test, menjalankan verification, serta mengoordinasikan agen. Perintah tersebut tidak dengan sendirinya mengizinkan merge ke branch terlindungi, perubahan akun/credential, pengadaan berbayar, publishing, atau Production deployment. Phase 11 tetap membutuhkan exact human authorization yang tercatat.
