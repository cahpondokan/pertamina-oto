# Pengamatan runtime Sandbox — 8 September 2026

Agent0, izin P2-W0H-EXEC-001. Input baseline f6fa411; konfigurasi poc/wave0h/environment/benign-observation.wsb SHA256 6221EDF587209E6D94AAFB5E056C1C7108570A488724010C2022FDDAFD1B1E19. Verifier statis lulus, launcher Microsoft signature Valid. Tidak ada sesi sebelumnya pada inventory. Tidak ada engine/dokumen/file host yang diproses.

## Percobaan pertama

- RAM bebas6088MiB, virtual13246MiB pada04:17:58+07:00.
- WindowsSandbox.exe dengan argumen path konfigurasi diluncurkan04:19:36.7937457, PID11008. Desktop guest terlihat sekitar04:20:45, tetapi script logon tidak terlihat. Kontrol efektif belum terbukti dari desktop.
- Sesi c582b4e1-bedb-4221-9a7c-0a99cb0abe66 dihentikan memakai CLI bawaan `wsb stop --id` pada04:25:13.1543040. `wsb list --raw` mengembalikan array kosong.
- Durasi sekitar336detik, MELEBIHI target300detik; pemulihan CLI menutup sesi. Tidak diberi nilai PASS untuk batas waktu. Tidak menghentikan aplikasi pengguna atau layanan virtualisasi bersama.

## Percobaan kedua: konfigurasi XML eksplisit

- Guard awal5120MiB menahan satu percobaan sebelum launch; RAM kemudian5110MiB. Untuk pengamatan ringan saja root memakai margin minimal512MiB di atas4096MiB profile (4608MiB guard), dekat target awal sekitar5GB. Ini keputusan operasional, bukan threshold kinerja produk.
- `wsb start --config <exact XML text> --raw` pada04:27:09.6649689 mengembalikan Id67461cce-f69d-4142-9a38-ab4a14dc9788; `wsb connect --id` exit0. Desktop guest terlihat; ringkasan logon tetap NOT_OBSERVED.
- Root menjalankan query berikut melalui `wsb exec --id <same id> --command powershell.exe -NoLogo -NoProfile -EncodedCommand <UTF16LE base64 source> --run-as ExistingLogin --raw`:

```powershell
try { $n = @([System.Net.NetworkInformation.NetworkInterface]::GetAllNetworkInterfaces() | Where-Object { $_.NetworkInterfaceType -ne 'Loopback' }).Count; if ($n -eq 0) { exit 0 } else { exit 21 } } catch { exit 22 }
```

- CLI mengembalikan `{"ExitCode":0}` pada04:29:17.0160695. Artinya query berhasil dan tidak menemukan interface non-loopback. Ini BUKAN attempted-egress proof, bukan bukti setiap redirection, dan bukan engine processing PASS. Query tidak membuat koneksi jaringan, membaca dokumen atau menulis file.
- `wsb stop --id <same id>` exit0; `wsb list --raw` array kosong pada04:29:39.9646468. Durasi sekitar150detik; selesai di dalam target300detik.
- CLI sudah terpasang, WindowsSandbox package0.8.107.0. Tidak ada acquisition/install/restart/biaya.

Sumber sintaks: https://learn.microsoft.com/en-us/windows/security/application-security/application-isolation/windows-sandbox/windows-sandbox-cli dan installed CLI help. User permission tetap berlaku, tetapi engine staging/runtime/oracle/supervision harus direview terpisah sebelum run.

Screenshot pertama ter-occlude menampilkan host, sehingga tidak disimpan/dipromosikan sebagai evidence. Snapshot guest sesudah aktivasi dipakai untuk observasi desktop saja; tidak ada screenshot digest/visual test PASS yang diklaim. Bukti mesin untuk query adalah output CLI dan source di atas.
