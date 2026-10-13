# Audit alur ruang kerja — 16 September 2026

## Scope
Navigasi Beranda, katalog, favorit, riwayat, dan editor lokal. Tidak ada perubahan engine PDF, dependency baru, upload, atau persetujuan rilis.

## Temuan dan perbaikan
- Reproduksi runtime COMException 800F1000 saat kontrol XAML bersama dipasang ulang ke wadah baru. Parent visual dapat null walau koleksi lama masih memiliki kontrol. Lepaskan dari struktur pemilik sebelum menyusun editor baru; bersihkan wadah aksi sementara pada editor khusus.
- Draft sebelumnya disimpan saat heading halaman baru sudah dibuat sehingga berisiko merekam state editor yang salah. Simpan sebelum transisi; jangan simpan ulang ke tujuan saat navigasi mundur/maju sedang berlangsung.
- Jalur pemilihan alat disatukan melalui OpenWorkspaceToolAsync. Pilihan dokumen Beranda diteruskan, jenis input diperiksa, dan alat satu-PDF meminta pilihan eksplisit bila beberapa dokumen dipilih.
- Saat keluar dari editor menuju Beranda/katalog, sumber dimasukkan ke konteks dokumen sesi. Katalog menampilkan jumlah dokumen terpilih dan akses kembali untuk memilih dokumen.
- Navigasi sidebar menangkap dan mencatat kegagalan; navigasi utama dibatasi selama proses/import/dialog berlangsung. Tidak menandai seluruh exception global sebagai handled.

## Alur pengguna
Beranda: masukkan/pilih dokumen -> pilih alat langsung atau melalui Semua Alat -> editor -> proses -> hasil. Pindah katalog tidak berarti menghapus dokumen. Menghapus dokumen dari ruang kerja tetap memakai tindakan eksplisit dan tidak menghapus sumber di disk.

## Verifikasi
Build Pertamina.Oto.Development.slnx Debug lulus. Executable artifacts/desktop-workspace-flow/Pertamina.Oto.Desktop.App.exe --self-test-flow menghasilkan 11 PASS di flow-selftest.txt: sembilan transisi editor/katalog dan pemeriksaan draft nama hasil, satu rangkaian navigasi umum, satu rangkaian organizer/katalog/merge berulang tiga kali.

## Batas bukti
Self-test membangun kontrol WinUI tanpa menampilkan jendela dan tanpa memproses dokumen. Belum merupakan UAT klik nyata, uji dialog pemilihan multi-file, atau bukti semua penyebab crash historis telah hilang. Draft hanya untuk sesi berjalan; bukan pemulihan seluruh editor setelah restart. Pengujian lanjutan: paste PDF di Beranda, pilih melalui katalog, pindah antaralat, mundur/maju, lalu proses salinan uji dan periksa hasil.
