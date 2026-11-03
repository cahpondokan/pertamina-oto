"""Freeze verified local release evidence and exact, unretouched store screenshots."""
from pathlib import Path
import hashlib, json, shutil
from PIL import Image
from pypdf import PdfReader

root = Path(__file__).resolve().parents[1]
evidence = root / 'evidence/play-release'
release = root / 'release-output/1.0.0'
qa = evidence / 'final-emulator'
ui = json.loads((qa/'play-release/checks.json').read_text())
native = json.loads((qa/'native-checks.json').read_text())
checks = json.loads((evidence/'release-verification.json').read_text())
assert ui['success'] and native['success'] and all(checks['checks'].values())
assert '0 errors' in (evidence/'lint-final.txt').read_text()
assert '+88: All tests passed!' in (evidence/'flutter-tests.log').read_text(encoding='utf-8-sig')
shots = {
    '04-gallery': ('01-galeri.png', 'Galeri Dokumen Saya dengan pratinjau PDF dan pencarian nama.'),
    '01-pages': ('02-halaman.png', 'Hasil crop otomatis dan susunan dua halaman sebelum disimpan.'),
    '02-reader': ('03-pembaca-pdf.png', 'PDF dibaca dengan menggulir halaman ke bawah dan tombol Kirim.'),
    '07-multi-select': ('04-kirim-bersama.png', 'Dua dokumen dipilih untuk dibagikan sekaligus.'),
    '06-landscape': ('05-landscape.png', 'Dokumen landscape mempertahankan perbandingan ukuran aslinya.'),
}
screens = release/'store/screenshots'; screens.mkdir(exist_ok=True)
alt = {}
for source, (name, description) in shots.items():
    image = Image.open(qa/f'play-release/{source}.png')
    assert image.size == (1080, 1920)
    if image.mode == 'RGBA':
        assert image.getchannel('A').getextrema() == (255, 255)
    rgb = image.convert('RGB')
    rgb.save(screens/name)
    assert Image.open(screens/name).tobytes() == rgb.tobytes()
    alt[name] = description
(screens/'alt-text-id.json').write_text(json.dumps(alt,ensure_ascii=False,indent=2),encoding='utf-8')
assert Image.open(release/'store/icon-512.png').mode == 'RGBA'
pdf = qa/'play-release/mixed-orientation.pdf'
pages = [{'width':float(p.mediabox.width),'height':float(p.mediabox.height),'rotation':p.rotation} for p in PdfReader(pdf).pages]
assert len(pages)==2 and pages[0]['height']>pages[0]['width'] and pages[1]['width']>pages[1]['height']
(evidence/'pdf-geometry-final.json').write_text(json.dumps({'bytes':pdf.stat().st_size,'pages':pages},indent=2))
summary = release/'verification'; summary.mkdir(exist_ok=True)
for name in ['release-verification.json','pdf-geometry-final.json','lint-final.txt','emulator-native.log','emulator-release-ui.log','installed-apk-sha256.txt','dart-analyze.log']:
    shutil.copyfile(evidence/name,summary/name)
shutil.copyfile(qa/'native-checks.json',summary/'native-checks.json')
shutil.copyfile(qa/'play-release/checks.json',summary/'ui-checks.json')
sources = []
for folder in ['lib','assets','test','tool','android/app/src']:
    sources.extend(p for p in (root/folder).rglob('*') if p.is_file() and '__pycache__' not in p.parts)
sources.extend(root/p for p in ['pubspec.yaml','pubspec.lock','analysis_options.yaml','android/settings.gradle.kts','android/build.gradle.kts','android/gradle.properties','android/gradle/wrapper/gradle-wrapper.properties','android/app/build.gradle.kts','android/app/proguard-rules.pro','.gitignore'])
hashes = {p.relative_to(root).as_posix():hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted(set(sources))}
(evidence/'source-manifest-final.json').write_text(json.dumps(hashes,indent=2))
shutil.copyfile(evidence/'source-manifest-final.json',summary/'source-manifest-final.json')
artifacts = {name:{'bytes':(release/name).stat().st_size,'sha256':hashlib.sha256((release/name).read_bytes()).hexdigest()} for name in ['OTO-Scan-1.0.0.aab','OTO-Scan-1.0.0.apk']}
(release/'artifacts.json').write_text(json.dumps(artifacts,indent=2))
report = f'''# OTO Scan 1.0.0+17 — hasil persiapan dan pengajuan rilis, 23 Oktober 2026

Status: kandidat selesai diverifikasi; AAB diterima Google Play dan seluruh 14 perubahan Alpha/listing/deklarasi/penguji sudah diajukan untuk review. Belum disetujui atau dirilis ke Production. Ini bukan kelulusan Phase 11 atau sign-off Production. Status Console terkini dicatat dalam `console-status.json`.

Package `com.niam.ayani.oto`; nama OTO Scan; minimum Android 10/API 29; target Android 16/API 36; ARMv7, ARM64 dan x86_64. AAB adalah berkas untuk Play Console. APK universal pendamping dipakai untuk uji emulator dan bukan perkiraan ukuran unduhan per perangkat dari Play.

## Hasil verifikasi

| Pemeriksaan | Hasil |
|---|---|
| Flutter/Dart | 88 tes lulus; analisis tanpa masalah |
| Android lint release | 0 error, 11 warning; laporan disertakan |
| Paket rilis | 23 pemeriksaan lulus, tanda tangan valid, QA nonaktif, APK/AAB berisi DEX dan AOT yang sama |
| Android native pada APK final | {native['count']} pemeriksaan lulus |
| Antarmuka APK final di emulator | {ui['count']} pemeriksaan lulus |
| PDF campuran potret/landscape | 2 halaman, {pdf.stat().st_size:,} byte; rasio dan orientasi benar, dirender ulang dengan Poppler |
| Halaman memori 16 KB | ELF dan alignment paket lulus; runtime emulator memakai 4 KB |
| Materi toko | Ikon asli 512x512 RGBA, feature graphic 1024x500 RGB, 5 screenshot asli 1080x1920 RGB, deskripsi dan catatan rilis Indonesia |

Uji memakai AVD terisolasi OTOScan_Play/API 36 dan gambar sintetis saja. Crop potret, landscape, kartu kecil, simpan PDF, zoom dua jari, kembali ke galeri, cari dokumen, pilih dua dokumen, chooser berbagi, halaman privasi offline, kamera virtual, dan scan baru kosong telah diperiksa. Berbagi berhenti pada chooser Android; tidak mengirim dokumen ke penerima eksternal. HP dan dokumen pengguna tidak diakses. Screenshot dikonversi ke PNG RGB tanpa perubahan ukuran atau piksel warna; tidak direka ulang.

PDF sintetis 137 KB ini bukan jaminan ukuran semua dokumen. Ketajaman dibatasi detail asli kamera; pembesaran ekstrem tidak dapat mengembalikan detail yang tidak tertangkap.

## Perubahan rilis

Versi 1.0.0+17, target SDK 36, AGP 8.10.1/Gradle 8.11.1, upload signing terpisah, halaman privasi dan lisensi offline, attribution dependency, skrip build/verifikasi, serta materi toko. Algoritma kamera, crop, orientasi, kualitas gambar, kompresi, dan koreksi zoom r16 tidak diganti. Aturan R8 sempit mempertahankan dua anggota internal untuk instrumentation terpisah; kelas penguji tidak ada di aplikasi rilis.

Flutter 3.29.3/Dart 3.7.2/NDK 27 tetap memakai toolchain yang tersedia. Bundletool resmi 1.18.1 diverifikasi sumber/digest/lisensinya. Tidak menambah SDK iklan, analitik, jaringan, atau scanner pihak ketiga. Izin platform aplikasi rilis hanya kamera; permission receiver signature internal AndroidX bukan akses data pengguna.

## Status Google Play dan halaman privasi

Per 23 Oktober 2026, seluruh 14 perubahan sudah masuk **Changes in review** untuk pengujian tertutup Alpha 1.0.0 (17), Indonesia. Awalnya 12 perubahan dikirim, kemudian dua pengaturan penguji (grup dan kontak umpan balik) ditambahkan. Google meminta review dimulai ulang untuk memasukkan keduanya; tindakan ini telah dilakukan sesuai instruksi pemilik untuk mengirim semua perubahan dan melanjutkan rilis. Tidak ada perubahan belum terkirim pada pemeriksaan terakhir. Pemeriksaan awal selesai; belum ada persetujuan review atau rilis Production. Managed publishing off.

Grup **OTO Scan - Penguji** sudah dibuat setelah pemilik menyelesaikan CAPTCHA. Tautan: https://groups.google.com/g/oto-scan-penguji-ayn . Alamat `oto-scan-penguji-ayn@googlegroups.com` sudah ditetapkan pada Alpha dan ikut review. Pemilik secara eksplisit mengizinkan halaman grup terlihat publik; siapa pun di web boleh bergabung. Percakapan hanya untuk anggota, daftar anggota hanya untuk pemilik. Anggota saat pembuatan: satu akun pemilik; ini bukan bukti opt-in pengujian. Tidak mengirim undangan, email atau percakapan. Kontak umpan balik: `aftachmadyanigps@gmail.com`.

Tautan pengujian yang ditampilkan Console: https://play.google.com/apps/testing/com.niam.ayani.oto . Tombol salin tautan masih nonaktif saat rilis belum diterbitkan; jangan mengklaim pemasangan tersedia sebelum review dan publikasi Alpha selesai. Calon penguji harus menggunakan akun Google yang sama untuk grup dan Google Play.

Kebijakan privasi kanonik https://oto.ayani.id/oto-scan/kebijakan-privasi sudah aktif dan ikut review. VPS, situs OTO yang ada dan unduhan desktop telah diverifikasi; hosting privasi lama tetap tersedia. Bukti migrasi: `evidence/play-release/privacy-vps-deployment.json`.

IARC Completed (Generic 3+, ESRB Everyone, PEGI 3); target pengguna 13+. Tidak ada iklan, login, pengumpulan atau berbagi data oleh pengembang yang dideklarasikan. Materi toko, lima screenshot emulator, label AI untuk ikon/feature graphic, kategori dan email publik sudah diajukan. App id `4972557040399357869`, developer `6264037910617880183`.

Pemantauan **baca-saja** tiap enam jam aktif dengan id `pantau-review-oto-scan`, setelah persetujuan eksplisit pemilik. Jadwal hanya membaca status dan memberi kabar ketika ada perubahan atau tindakan yang diperlukan; tidak mengubah pengaturan, mengirim formulir, mengundang penguji atau menerbitkan aplikasi otomatis.

## Langkah yang masih diperlukan

1. Tunggu hasil review Alpha. Managed publishing off, tetapi ketersediaan pengujian tetap harus diverifikasi setelah Google menyetujui rilis.
2. Pemilik dapat membagikan tautan grup kepada calon penguji. Setelah rilis Alpha tersedia, minimal 12 penguji sungguhan harus opt-in melalui tautan Google Play dan bertahan selama 14 hari berturut-turut. Console terakhir menunjukkan 0 penguji opt-in. Grup yang berisi satu pemilik tidak memenuhi syarat ini.
3. Kumpulkan hasil penggunaan dan umpan balik penguji yang nyata. Setelah syarat terpenuhi, ajukan akses Production dengan jawaban faktual; tombol Apply for production saat ini terkunci. Persetujuan Google dan proses rilis Production berikutnya masih diperlukan. Jangan mengarang hasil uji, jumlah penguji, atau jawaban deklarasi.
4. Pemilik perlu menyimpan backup upload key dengan aman sebelum distribusi. Folder `.release-private` dilindungi ACL dan password DPAPI terikat pengguna Windows ini; kunci/password tidak disertakan dalam bukti atau unggahan toko.

## Batas dan catatan audit

Lint warning tersisa meliputi ExifInterface platform untuk JPEG kamera milik aplikasi, pembatasan orientasi aktivitas pada perangkat besar/ChromeOS, resource lama, perhitungan ruang yang konservatif, ikon monochrome opsional, dan lokasi bitmap resource. Tidak disembunyikan dengan suppression. Belum ada validasi runtime Android 10, perangkat ARM melalui Play, 16 KB runtime, atau tablet pada task ini. Runtime sistem PDF tetap mengikuti pembaruan keamanan Android; hasil ini bukan audit keamanan menyeluruh.

Signer unggah berbeda dari signer APK uji HP lama. Jangan mencopot aplikasi HP untuk mengganti signer tanpa mengekspor PDF pengguna terlebih dahulu. Google Play App Signing akan mempunyai identitas distribusi tersendiri. Tidak ada pemasangan/uninstall/data clear pada HP dalam task ini.

Kendala lab: build dan emulator pernah kehabisan resource; Gradle dibatasi, cache transform yang bisa dibuat ulang dibersihkan, file image SDK dan image AVD lama dikompresi NTFS tanpa perubahan isi. AVD lama tidak dihapus. Uji final memakai AVD baru. Percobaan penguji yang gagal tersimpan: R8 mengoptimalkan wrapper, System UI emulator tidak merespons, label tooltip tidak terbaca oleh penguji, chooser belum siap, dan pencarian otomatis kosong ketika kembali. Masalah penguji tersebut sudah diperbaiki sebelum hasil lulus final; tidak dicatat sebagai kelulusan percobaan awal.

## Berkas dan identitas

AAB SHA256: `{artifacts['OTO-Scan-1.0.0.aab']['sha256']}`

APK SHA256: `{artifacts['OTO-Scan-1.0.0.apk']['sha256']}`

`verification/` berisi ringkasan pemeriksaan, hash APK emulator dan manifest {len(hashes)} file sumber/penguji. Bukti lengkap berada di `evidence/play-release/` pada proyek. Tidak ada commit, merge atau promosi phase yang dilakukan.

## Rujukan resmi

- Target API: https://support.google.com/googleplay/android-developer/answer/11926878
- Persyaratan pengujian akun personal: https://support.google.com/googleplay/android-developer/answer/14151465
- Spesifikasi aset toko: https://support.google.com/googleplay/android-developer/answer/9866151
- Halaman memori: https://developer.android.com/guide/practices/page-sizes
'''
(release/'RELEASE_HANDOFF.md').write_text(report,encoding='utf-8')
print(json.dumps({'native_checks':native['count'],'ui_checks':ui['count'],'release_checks':len(checks['checks']),'flutter_tests':88,'source_files':len(hashes),'pdf_bytes':pdf.stat().st_size,'screenshots':len(shots)}))
