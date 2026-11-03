# Pemantauan anonim OTO Web

Tidak ada akun pengunjung, cookie analitik, ID perangkat tetap atau upload dokumen. Ini observability operasional anonim, bukan Enterprise Audit yang membuktikan identitas manusia. Perubahan diotorisasi oleh `WEB-AUDIT-20261027`.

## Pengaktifan

Secara default diagnostik **nonaktif**. Kode dan halaman privasi sudah ada dalam paket; pengelola mengaktifkan penyimpanan terpusat melalui tiga konfigurasi framework:

```text
WebDiagnostics__Enabled=true
WebDiagnostics__Directory=/var/lib/oto-downloads/diagnostics
WebDiagnostics__Release=<ID rilis immutable yang sedang diaktifkan>
```

Direktori harus absolut dan di luar content root/wwwroot. Akun service yang sudah ada (`oto-web`) memerlukan akses tulis hanya ke direktori tersebut. Linux: direktori 0700 dan file 0600; operator membaca melalui otorisasi OS/SSH existing. Jangan membuka direktori melalui Nginx atau menaruh laporan di wwwroot. Dalam unit dengan `ProtectSystem=strict`, tambahkan `ReadWritePaths=/var/lib/oto-downloads/diagnostics` melalui drop-in yang ditinjau. Jangan memperluas izin seluruh `/var/lib` atau mematikan proteksi service. Pembuatan direktori/owner/drop-in/restart memerlukan otorisasi aktivasi dan operator yang berwenang; belum dijalankan oleh task implementasi.

Set `anonymousDiagnostics: true` dalam publication manifest saat aktivasi diagnostik menjadi bagian rilis. Helper `deploy_web.py` kemudian mengharuskan CSP tepat `connect-src 'self'`, marker diagnostik, halaman `/privasi-web`, serta penolakan endpoint baca/upload yang tidak diizinkan. Tanpa flag tersebut, helper tetap mensyaratkan `connect-src 'none'`. Tetap gunakan prosedur rilis immutable, baseline/katalog unduhan terpin, staging, verifikasi HTTPS dan rollback existing. Tidak ada data diagnostik dalam paket rilis.

Untuk uji lokal, jalankan host **dari folder hasil publish** dengan argumen `--urls http://127.0.0.1:56337 --WebDiagnostics:Enabled=true --WebDiagnostics:Directory="<folder privat absolut>"`. Downloads menghubungkan aset shared ke output; menjalankannya dari source directory dapat menghasilkan aset 404 di luar Development.

## Melihat masalah

Jalankan dengan hak baca OS yang sesuai:

```sh
python3 diagnostics_report.py --data-dir /var/lib/oto-downloads/diagnostics --output /lokasi-privat/oto-monitor.html
python3 diagnostics_report.py --data-dir /var/lib/oto-downloads/diagnostics --output /lokasi-privat/oto-monitor.json
```

Gunakan lokasi output privat milik operator yang sudah ada. Salin laporan melalui SSH yang diotorisasi jika ingin membukanya lokal. Jangan menyediakan endpoint HTTP publik untuk log atau laporan. Alat ini hanya membaca; tidak mengubah atau menghapus log, tidak memerlukan dependency, dan tidak mengirim data. Jalankan kembali untuk memperbarui snapshot. Tidak ada scheduler atau notifikasi otomatis yang dibuat.

Laporan menunjukkan jumlah event, kelompok kesalahan menurut fitur/tahap/kode/rilis/keluarga browser, kejadian terakhir dan ID proses. Cari kelompok kesalahan yang meningkat, lalu gunakan rilis dan kode tetap untuk mereproduksi masalah dengan fixture sintetis. PDF rusak/fitur belum mendukung tidak otomatis berarti bug aplikasi. `startedWithoutTerminalReceived` dapat berarti tab ditutup atau laporan hilang; jangan menganggap semua kasus sebagai crash.

## Kontrak dan privasi

- Satu POST `/api/diagnostics/events`, JSON versi 1, maksimum 16 event/12 KiB; hanya nama field yang terdaftar, tanpa property bebas, nested payload, raw exception/stack, nama/path file, teks watermark, rentang halaman, byte PDF/gambar, token, URL, IP, atau user-agent mentah.
- Setiap event: UUID v4 acak per event/proses; event/feature/stage/code enum; durasi integer terbatas; waktu klien; versi klien tetap; keluarga browser umum. Server menambahkan waktu penerimaan, rilis dan label `anonymous-client` atau `server`. Waktu/hasil klien tidak dianggap fakta terverifikasi.
- Semua sepuluh processor memakai pencatatan input dan pemrosesan. Shared result mencatat permintaan download, native save berhasil setelah stream ditutup, pembatalan/kegagalan simpan. Unhandled error/rejection memakai kode tetap. Error server tidak mencatat path atau exception mentah.
- Header khusus, pemeriksaan Origin/Fetch Metadata, tanpa CORS, cookie/referrer/redirect; 60 request/menit global, body timeout 3 detik. Browser mengirim tanpa credentials. Ini mengurangi penyalahgunaan, bukan autentikasi; klien langsung masih dapat memalsukan metadata atau menghabiskan budget global.
- Antrean maksimal 128 event **hanya di memori tab**; batch 16, timeout jaringan 4 detik, 6 percobaan terbatas dengan backoff. ID event tetap selama retry. Tidak ada `sendBeacon`, serialisasi File/Blob, storage permanen pengunjung, atau ketergantungan processing pada respons log.
- Server menerima ulang ID+isi yang sama tanpa duplikasi, menolak ID dengan isi berbeda; flush sebelum 204. File append-only di jalur aplikasi biasa. Bukan WORM atau bukti non-repudiation; admin OS masih dapat mengubahnya.

## Kapasitas dan kegagalan

Maksimum **16 MiB atau 32.768 event**, mana yang lebih dahulu. Tidak ada penghapusan/retensi otomatis: angka tersebut adalah batas resource development, bukan masa retensi perusahaan. Ketika penuh/tidak dapat ditulis, endpoint mengembalikan 503 dan PDF lokal tetap berfungsi. Pantau ukuran/jumlah/terakhir diterima di laporan; ambang notifikasi dan kebijakan arsip/purge belum diaktifkan. Sebelum pemakaian berkelanjutan, pengelola perlu menetapkan rotasi/retensi yang sesuai kebijakannya. Jangan hapus log untuk memaksa status sehat; lakukan penanganan/arsip terotorisasi.

Satu proses host menulis satu direktori. Scaling multi-instance memerlukan desain storage/dedupe bersama yang terpisah. Baris akhir terpotong/rusak menghentikan append sampai diperiksa operator; tidak diperbaiki diam-diam. Metadata aman dapat hilang ketika tab ditutup, retry habis, antrean penuh, server tidak tersedia, atau browser crash sebelum mengirim. Monitoring ini tidak menjamin pencatatan seluruh kejadian atau memulihkan dokumen.

Alamat IP tetap diperlukan pada transport HTTP dan mungkin terdapat dalam log infrastruktur existing. Fitur ini tidak menyalin IP ke diagnostik dan tidak mengubah kebijakan Nginx/systemd existing. Periksa kebijakan infrastruktur sebagai bagian aktivasi, tanpa mengklaim seluruh stack tidak pernah mencatat IP.

## Verification

`node --test src/Hosts/Pertamina.Oto.Web.Preview/tests/web-diagnostics.test.cjs`; suite lengkap memerlukan `OTO_PDF_TEST_DIR` menunjuk fixture A.pdf/B.pdf existing. `test_web_diagnostics.py` menjalankan dua host loopback dan membutuhkan `OTO_DIAGNOSTICS_EVIDENCE` absolut untuk fixture/log. Build kedua host tanpa restore dependency baru. `test_deploy_web.py` mempunyai empat test aktivasi/symlink khusus Linux yang tetap harus dijalankan saat rilis VPS; Windows tidak membuktikan kelulusan empat test itu.
