import 'package:flutter/material.dart';
import 'app_config.dart';

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
  final bool googleScanner;
  const PrivacyScreen({super.key, this.googleScanner = googleScannerEnabled});

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(title: const Text('Privasi & tentang')),
    body: SafeArea(
      child: ListView(
        padding: const EdgeInsets.all(24),
        children: [
          Image.asset('assets/oto.png', width: 72, height: 72),
          const SizedBox(height: 16),
          Text(
            applicationName,
            style: Theme.of(context).textTheme.headlineSmall,
          ),
          const Text('Pindai, simpan, dan bagikan PDF dari perangkat Anda.'),
          const SizedBox(height: 24),
          Text(
            'Kebijakan privasi',
            style: Theme.of(context).textTheme.titleLarge,
          ),
          const SizedBox(height: 12),
          SelectableText(
            googleScanner
                ? (isMLKitTrial ? googleTrialPrivacy : googlePrivacy)
                : privacyPolicy,
          ),
          const SizedBox(height: 24),
          OutlinedButton(
            onPressed:
                () => showLicensePage(
                  context: context,
                  applicationName: applicationName,
                ),
            child: const Text('Lisensi perangkat lunak'),
          ),
        ],
      ),
    ),
  );
}

const googlePrivacy =
    '''OTO Scan 1.0.1 dan versi berikutnya yang menggunakan Google ML Kit
Diperbarui 24 Oktober 2026

OTO Scan memproses isi dokumen di perangkat Anda. Aplikasi tidak memerlukan akun, tidak menampilkan iklan, dan tidak mengirim isi dokumen ke server pengembang. Google ML Kit Document Scanner digunakan untuk kamera, pengambilan otomatis, pemotongan dan peningkatan gambar. Menurut dokumentasi Google, gambar masukan dan hasil pemrosesan ML Kit tidak dikirim ke server Google.

Layanan Google dan data teknis
Google Play services mengunduh dan memperbarui komponen pemindai saat diperlukan. Pemakaian pertama dapat memerlukan internet dan perangkat harus mendukung layanan tersebut. Google menerima data teknis SDK untuk diagnostik, analitik penggunaan, pemeliharaan, peningkatan layanan dan pencegahan penyalahgunaan. Data ini dapat meliputi produsen/model perangkat, versi sistem dan aplikasi, pengenal perangkat atau instalasi, konfigurasi fitur, format/resolusi serta ukuran masukan dan keluaran, waktu pemrosesan, peristiwa penggunaan dan kode kesalahan. Data teknis dikirim melalui HTTPS. Data tersebut bukan isi foto, tulisan, atau PDF hasil scan.
Pengembang tidak menyediakan pengaturan untuk menonaktifkan metrik Google di dalam OTO Scan. Pengelolaan dan masa simpan data teknis Google mengikuti kebijakan Google; menghapus dokumen lokal tidak berarti menghapus metrik yang sudah diterima Google. Rujukan: https://developers.google.com/ml-kit/terms , https://developers.google.com/ml-kit/android-data-disclosure dan https://policies.google.com/privacy .

Kamera dan galeri
Pemindai Google memakai akses kamera Google Play services. Anda memilih foto galeri yang diproses dan hasil yang dikembalikan ke OTO Scan. Izin kamera OTO tetap tersedia untuk komponen scanner lokal. Dokumen lama dapat dibaca tanpa membuka kamera.

Penyimpanan dan PDF
Foto sumber, draf, hasil scan, PDF, nama dokumen dan thumbnail disimpan di ruang aplikasi pada HP. Pembuatan, kompresi, galeri dan pembacaan PDF dilakukan secara lokal oleh OTO. PDF dari aplikasi lain disalin sementara untuk dibaca. Salinan sementara dibersihkan saat sesi selesai atau sesuai masa simpan cache pada pembukaan berikutnya setelah proses terhenti. Tidak ada unggah dokumen otomatis oleh OTO.

Kirim dan simpan salinan
Dokumen diteruskan ke aplikasi atau lokasi pilihan Anda melalui Kirim atau Simpan salinan. Penyedia tujuan memiliki kebijakan privasi sendiri. OTO tidak mengendalikan salinan yang telah Anda bagikan.

Penghapusan
Hapus dokumen melalui Dokumen Saya dan buang draf yang tidak diperlukan. Menghapus data aplikasi atau mencopotnya menghapus data lokal OTO. Cadangan otomatis Android dinonaktifkan. Ekspor dokumen penting sebelum mencopot aplikasi; hapus salinan di luar OTO dari lokasi masing-masing.

Keamanan dan kontak
OTO membatasi akses file dan menggunakan izin berbagi sementara. Keamanan HP dan aplikasi penerima tetap memengaruhi keamanan dokumen. Kualitas hasil dibatasi detail sumber kamera. Pengembang: AFT Ahmad Yani. Pertanyaan aplikasi atau privasi: aftachmadyanigps@gmail.com. Kebijakan publik: https://oto.ayani.id/oto-scan/kebijakan-privasi .''';

const googleTrialPrivacy = '''Versi uji Google ML Kit — 23 Oktober 2026

OTO Scan Uji menggunakan Google ML Kit Document Scanner untuk kamera, pemotongan, dan peningkatan gambar. Pemrosesan isi dokumen dilakukan di perangkat. Isi gambar dan hasil pemrosesan tidak dikirim ke server Google oleh ML Kit, menurut dokumentasi Google. Aplikasi tidak memerlukan akun dan tidak menampilkan iklan.

Layanan Google
Google Play services mengunduh komponen pemindai saat diperlukan; pemakaian pertama dapat memerlukan internet. Google dapat menerima metrik teknis SDK, seperti informasi perangkat/aplikasi, pengenal, penggunaan, konfigurasi, waktu pemrosesan, dan kesalahan untuk diagnostik dan peningkatan layanan. Ini berbeda dari pengiriman isi dokumen. Ketentuan: https://developers.google.com/ml-kit/terms . Kebijakan Google: https://policies.google.com/privacy .

Kamera dan galeri
Layar pemindai Google menyediakan pengambilan otomatis/manual, crop dan filter. Anda memilih gambar yang dikembalikan ke OTO. Pemindai memakai akses kamera Google Play services; izin kamera aplikasi tetap ada untuk komponen scanner lokal. Foto dari galeri hanya diproses ketika Anda memilihnya di pemindai.

Penyimpanan dan PDF
Hasil pilihan disalin ke ruang privat OTO Scan Uji. Pembuatan, kompresi, galeri, dan pembacaan PDF tetap dilakukan di perangkat oleh OTO. Versi uji terpasang terpisah dan tidak mengakses dokumen pada OTO Scan sebelumnya. Unduhan awal komponen Google diperlukan sebelum mengandalkan pemindaian tanpa jaringan.

Kirim, simpan, dan penghapusan
Dokumen diteruskan hanya ke aplikasi atau lokasi yang Anda pilih melalui Kirim atau Simpan salinan. Penyedia tujuan memiliki kebijakan sendiri. Hapus dokumen melalui Dokumen Saya; menghapus data atau mencopot versi uji menghapus data lokalnya. Cadangan otomatis Android dinonaktifkan. Simpan salinan PDF penting sebelum mencopot aplikasi.

Keamanan dan kontak
File privat dan izin berbagi sementara tetap digunakan. Ketajaman bergantung pada sumber; aplikasi tidak menjanjikan pemulihan tulisan yang tidak tertangkap kamera. Kontak: aftachmadyanigps@gmail.com. Kebijakan ini khusus APK uji, bukan perubahan deklarasi versi yang sedang ditinjau Google Play.''';
