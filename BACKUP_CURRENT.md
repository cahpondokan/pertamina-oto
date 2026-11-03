# Cadangan privat PERTAMINA OTO — 3 November 2026

Isi sumber:
- Web dan Desktop: snapshot kode lokal saat pencadangan, termasuk perubahan setelah cadangan 13 Oktober.
- Android algoritma sendiri: OTO Scan 1.0.0+17, package com.niam.ayani.oto; subtree flutter identik commit 697655358a6d2d3736ca0677be63c350a3d6183b. Tidak memakai Google ML Kit.
- assets/biblo-reference, workspace-context dan backup-evidence yang lama dipertahankan dari cadangan terdahulu sebagai riwayat, bukan klaim status terkini.

Paket aplikasi yang sudah dibuat sebelumnya:
- Desktop 0.3.0, build 20261026-112441-ede2bdee. ZIP berisi manifest.json dan app/. Seluruh berkas sesuai hash manifest rilis lokal.
- Web oto-web-20261030-8c712f5c62b7. ZIP berisi paket deployment VPS yang telah dipublikasikan, bukan perubahan layanan/konfigurasi baru.
- Android 1.0.0 APK dan AAB dengan algoritma sendiri; hash cocok catatan rilis terdahulu.

Pemulihan:
1. Clone repositori privat dan pilih cabang backup/android-web-desktop-20261103.
2. Baca README proyek, dokumen development, global.json dan lockfile sebelum membangun. Paket runtime/dependency dipulihkan melalui prosedur yang sudah ada.
3. Unduh aset dari draft Release cadangan, lalu cocokkan SHA256SUMS.txt.
4. Desktop: ekstrak ZIP ke folder pemulihan baru; jangan timpa instalasi/data pengguna yang aktif. Gunakan prosedur lokal eng/Build-LocalUpdate.ps1 dan docs/development/LOCAL_UPDATES.md bila perlu integrasi instalasi.
5. Web: baca README.txt dan Publish-Web.ps1 di paket; aktivasi VPS memerlukan tindakan/otorisasi terpisah.
6. Android: APK dapat dipasang setelah otorisasi perangkat. Signing key/password tidak disertakan; sumber dapat dipelajari/dibangun tanpa membawa keystore privat ke GitHub.

Batas:
Ini backup sumber dan paket yang sudah ada, bukan build baru atau kelulusan uji aplikasi baru. Sumber saat pencadangan dapat mencakup perubahan yang lebih baru daripada tanggal paket. Draft Release tetap privat dan tidak mengubah rilis Play Store, situs VPS atau instalasi Desktop. Dokumen pengguna, kredensial akun, signing key, cache Python dan salinan scratch .before tidak disertakan.
