# Monitor privat OTO

Dashboard pengelola terpisah dari OTO Web. Pengunjung OTO tetap tanpa akun. Endpoint publik hanya menerima kontrak metadata anonim; tidak menerima PDF, gambar, nama/path file, input bebas, pesan exception, stack, atau identitas pengunjung.

## Jalur akses

`browser pengelola → HTTPS monitor.ayani.id → 127.0.0.1:443 di komputer pengelola → SSH terautentikasi → 127.0.0.1:5443 di VPS → monitor 127.0.0.1:5091`

DNS publik A monitor.ayani.id menunjuk VPS untuk validasi/perpanjangan sertifikat. Listener publik Nginx mengembalikan 404; hanya path ACME challenge dapat dibaca. Aplikasi monitor tidak memiliki route pada oto.ayani.id. Tidak ada tautan dashboard pada situs publik. Ini bukan pengamanan berdasarkan nama subdomain tersembunyi.

## Membuka dashboard di Windows

1. Satu kali: jalankan `Connect-Monitor.ps1 -SetupHosts` dari PowerShell Administrator. Skrip menyimpan backup hosts dan menambahkan tepat satu pemetaan `127.0.0.1 monitor.ayani.id`. Record DNS publik tetap dipertahankan untuk HTTPS. Skrip menolak pemetaan yang bertentangan.
2. Jalankan `Connect-Monitor.ps1` sebagai pengguna Windows biasa yang memiliki akses SSH existing. Port lokal 443 harus tersedia. Terminal tetap terbuka selama penggunaan; tidak ada browser yang dipilih/diubah otomatis.
3. Buka `https://monitor.ayani.id` pada browser yang Anda pilih, lalu masuk dengan akun pengelola yang dibuat saat aktivasi. Jangan melewati peringatan sertifikat.
4. Dashboard memperbarui data setiap 15 detik ketika tab aktif. Pilih rentang waktu, fitur atau hasil, lalu Terapkan. Klik ID proses untuk menelusuri kejadian terkait. Klik Keluar untuk mencabut sesi, lalu Ctrl+C di terminal untuk memutus SSH.

Tanpa tunnel, alamat pada komputer yang telah disiapkan tidak dapat dijangkau. Dari internet tanpa pemetaan privat, situs mengembalikan 404. Keduanya merupakan hasil yang diharapkan.

## Login dan batas data

Password dibuat lewat terminal sudo dengan input tersembunyi; PBKDF2-HMAC-SHA256, salt acak 32 byte, 600.000 iterasi. Tidak ada akun/password bawaan atau pendaftaran publik. File kredensial root:oto-monitor 0640, directory 0750; keys cookie hanya milik service monitor. Cookie host-only, Secure, HttpOnly, SameSite Strict, tidak persisten. CSRF dan Origin divalidasi, akses lintas subdomain ditolak. Lima kegagalan login memblokir percobaan selama lima menit. Sesi berakhir setelah dua jam, dicabut saat logout dan invalid setelah service restart/reset password. Maksimal lima sesi aktif; yang tertua dicabut.

Monitor membaca log dengan akun service terpisah dan filesystem read-only. Collector oto-web menulis ke directory privat setgid 2750, file 0640 dengan group pembaca khusus. Berkas credential/keys tidak berada dalam group pembaca dan tidak dapat dibaca collector.

Log saat ini dibatasi 16 MiB atau 32.768 catatan tanpa penghapusan otomatis. Dashboard memberi peringatan pada 80% kapasitas; pengelola perlu mengarsipkan log secara terkendali sebelum penuh. Seluruh catatan di dashboard berarti seluruh log aktif yang tersedia, bukan retensi tak terbatas. Saat penuh collector menolak pencatatan tambahan; pemrosesan PDF tetap lokal. Catatan browser dapat hilang, tidak membuktikan identitas orang, dan tidak membuktikan file sudah tersimpan. “Hasil akhir belum diterima” tidak selalu berarti gagal. Angka berhasil menghitung tahap process, bukan pemeriksaan input.

## Operasi VPS

Aktivasi/rollback harus menggunakan paket immutable dengan SHA-256 yang terikat pada launcher. `deploy_monitor.py verify` tidak mengubah service. `activate` memeriksa baseline web/katalog, menyalin runtime .NET 10.0.12 existing (tidak mengunduh dependency), menguji host pada Linux, membuat kredensial secara interaktif, menerbitkan sertifikat menggunakan akun ACME existing, mengaktifkan monitor privat, lalu merilis collector. Bila pemeriksaan gagal, service monitor dihentikan dan rilis web dikembalikan jika telah diaktifkan. Artefak/data/kredensial dipertahankan untuk pemeriksaan; jangan menjalankan ulang secara buta.

`rollback` memeriksa pin dan drift konfigurasi, memulihkan rilis OTO sebelumnya, menonaktifkan monitor dan listener privat, serta mempertahankan data privat/kredensial/sertifikat. Tidak mengubah katalog Desktop atau DNS aplikasi lain.

Reset password: `sudo python3 /opt/oto-monitor/set_admin.py --reset`. Tindakan ini me-restart monitor dan mencabut seluruh sesi. Operator perlu memantau kapasitas storage serta hasil perpanjangan Certbot. Tidak ada endpoint web untuk mengganti akun, menghapus catatan, atau membaca file arbitrer.

## Verifikasi dan acuan

Tes HTTP memakai data sintetis: auth, CSRF, same-origin, cookie, logout/restart revocation, lockout, filter/paginasi, data historis, tail parsial, kapasitas, serta canary nama/isi PDF. Tes browser memeriksa login, refresh otomatis, filter dan detail proses. Paket Linux menjalankan suite host sebelum aktivasi dan memeriksa TLS, 404 publik, API 401 tanpa login, serta bind loopback.

Acuan framework: [cookie authentication](https://learn.microsoft.com/aspnet/core/security/authentication/cookie?view=aspnetcore-10.0), [antiforgery](https://learn.microsoft.com/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0), [PBKDF2 API](https://learn.microsoft.com/dotnet/api/system.security.cryptography.rfc2898derivebytes.pbkdf2). Tidak ada paket NuGet tambahan.
