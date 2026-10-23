import 'package:flutter/material.dart';

const privacyPolicy = '''Berlaku sejak 22 Oktober 2026

OTO Scan memproses dokumen di perangkat Anda. Aplikasi tidak memerlukan akun, tidak memuat iklan atau analitik, dan tidak mengirim isi dokumen ke server pengembang.

Kamera
Izin kamera dipakai untuk menampilkan pratinjau dan memotret dokumen ketika Anda menggunakan pemindai. Pemotongan tepi, perbaikan gambar, dan pembuatan PDF dikerjakan secara lokal. Izin kamera dapat dicabut melalui Setelan Android; pembaca PDF dan dokumen tersimpan tetap dapat digunakan.

Penyimpanan
Foto sumber, draf, gambar hasil scan, PDF, nama dokumen, dan thumbnail disimpan di ruang aplikasi pada perangkat. PDF yang dibuka dari aplikasi lain disalin sementara untuk dibaca. Salinan sementara dibersihkan saat sesi ditutup; sisa setelah proses terhenti dibersihkan pada pembukaan berikutnya sesuai masa simpan cache.

Kirim dan simpan salinan
Dokumen hanya diteruskan ke aplikasi atau lokasi yang Anda pilih melalui Kirim atau Simpan salinan. Penerima dan penyedia penyimpanan memiliki kebijakan privasi sendiri. OTO Scan tidak mengendalikan salinan yang sudah Anda bagikan atau ekspor.

Penghapusan
Hapus dokumen melalui Dokumen Saya. Draf dapat dibuang saat memulai scan baru. Menghapus data aplikasi atau mencopot aplikasi menghapus data lokal aplikasi. Cadangan otomatis Android untuk data aplikasi ini dinonaktifkan. Simpan salinan PDF penting di lokasi pilihan Anda sebelum mencopot aplikasi. Salinan di luar aplikasi perlu dihapus dari lokasi tersebut.

Keamanan dan batasan
Aplikasi membatasi akses file, memproses PDF secara lokal, dan menggunakan izin berbagi sementara. Keamanan perangkat dan aplikasi penerima juga menentukan keamanan dokumen. Aplikasi tidak menjanjikan pemulihan detail yang tidak tertangkap kamera.

Perubahan dan kontak
Kebijakan diperbarui jika cara aplikasi menangani data berubah. Kontak dukungan pengembang tersedia pada halaman OTO Scan di Google Play.''';

class PrivacyScreen extends StatelessWidget {
  const PrivacyScreen({super.key});

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(title: const Text('Privasi & tentang')),
    body: SafeArea(
      child: ListView(
        padding: const EdgeInsets.all(24),
        children: [
          Image.asset('assets/oto.png', width: 72, height: 72),
          const SizedBox(height: 16),
          Text('OTO Scan', style: Theme.of(context).textTheme.headlineSmall),
          const Text('Pindai, simpan, dan bagikan PDF dari perangkat Anda.'),
          const SizedBox(height: 24),
          Text('Kebijakan privasi', style: Theme.of(context).textTheme.titleLarge),
          const SizedBox(height: 12),
          const SelectableText(privacyPolicy),
          const SizedBox(height: 24),
          OutlinedButton(
            onPressed: () => showLicensePage(
              context: context,
              applicationName: 'OTO Scan',
            ),
            child: const Text('Lisensi perangkat lunak'),
          ),
        ],
      ),
    ),
  );
}
