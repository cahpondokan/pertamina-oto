"""Prepare local 1.0.1 release copy. Does not publish a website or Play changes."""
import html
import pathlib
import re

root = pathlib.Path(__file__).resolve().parents[1]
out = root / 'release-output/1.0.1'
out.mkdir(parents=True, exist_ok=True)
source = (root / 'lib/privacy_screen.dart').read_text(encoding='utf-8')
match = re.search(r"const googlePrivacy\s*=\s*'''(.*?)''';", source, re.S)
assert match, 'Google production privacy copy is missing'
policy = match.group(1)
(out / 'privacy-policy-id.txt').write_text(policy + '\n', encoding='utf-8')
sections = policy.split('\n\n')
paragraphs = ['<p>' + html.escape(sections[1]) + '</p>']
for section in sections[2:]:
    title, body = section.split('\n', 1)
    paragraphs.append('<section><h2>' + html.escape(title) + '</h2><p>' +
                      html.escape(body).replace('\n', '</p><p>') + '</p></section>')
page = '''<!doctype html>
<html lang="id"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Kebijakan Privasi OTO Scan — AFT Ahmad Yani</title>
<link rel="canonical" href="https://oto.ayani.id/oto-scan/kebijakan-privasi">
<style>body{margin:0;background:#f5f7fa;color:#172b40;font:17px/1.7 system-ui,sans-serif}main{max-width:800px;margin:32px auto;padding:32px;background:#fff;border-radius:18px}h1{line-height:1.2}h2{font-size:1.25rem;margin-top:32px}.meta{color:#526477}.note{padding:16px;background:#edf4fc;border-radius:10px}a{color:#005aaa;overflow-wrap:anywhere}p{overflow-wrap:anywhere}@media(max-width:600px){main{margin:0;padding:24px;border-radius:0}}</style>
</head><body><main><a href="https://oto.ayani.id/">OTO · AFT Ahmad Yani</a>
<h1>Kebijakan Privasi OTO Scan</h1><p class="meta">Diperbarui 24 Oktober 2026 · com.niam.ayani.oto</p>
<p class="note">Kebijakan berikut berlaku untuk versi 1.0.1 dan versi berikutnya yang menggunakan Google ML Kit. Versi 1.0.0 memakai pemindai lokal OTO tanpa SDK pemindai Google dan tanpa metrik SDK tersebut. Penyimpanan serta berbagi PDF tetap dilakukan oleh OTO sesuai bagian di bawah.</p>
''' + '\n'.join(paragraphs) + '''
<p>Dokumentasi: <a href="https://developers.google.com/ml-kit/terms">Ketentuan ML Kit</a> · <a href="https://developers.google.com/ml-kit/android-data-disclosure">Pengungkapan data ML Kit</a> · <a href="https://policies.google.com/privacy">Kebijakan Privasi Google</a></p>
<p><a href="mailto:aftachmadyanigps@gmail.com">Hubungi dukungan OTO Scan</a></p>
</main></body></html>'''
(out / 'privacy-policy.html').write_text(page, encoding='utf-8')
(out / 'OtoScanPrivacy.cshtml').write_text('@page "/oto-scan/kebijakan-privasi"\n@{ Layout = null; }\n' + page.replace('@', '@@'), encoding='utf-8')
notes = 'Pemindai Google ML Kit dengan Auto capture, crop otomatis, koreksi perspektif, dan filter penjernihan. Galeri dokumen, pembaca PDF vertikal, kompresi, serta berbagi beberapa PDF tetap tersedia. Penjelasan privasi diperbarui untuk layanan Google.'
short = 'Scan otomatis ke PDF, rapikan tepi, cari dan bagikan. Diproses di perangkat.'
full = '''OTO Scan membantu Anda memindai, menyimpan dan membagikan dokumen PDF dari HP.

PINDAI DAN RAPIKAN
Gunakan Google ML Kit Document Scanner untuk pengambilan otomatis saat dokumen terdeteksi dengan baik, crop tepi, koreksi perspektif dan filter penjernihan. Anda tetap dapat mengambil gambar secara manual dan menyesuaikan hasilnya sebelum digunakan. Kejelasan tulisan bergantung pada cahaya, fokus dan kualitas gambar sumber.

DOKUMEN SAYA
Lihat hasil scan sebagai galeri thumbnail, cari nama dokumen dan tandai beberapa PDF untuk dibagikan sekaligus.

BACA DAN SIMPAN PDF
Baca PDF beberapa halaman dengan menggulir ke bawah dan perbesar bagian yang diperlukan. Pembuatan PDF, kompresi dan pembacaan tetap dilakukan oleh OTO di perangkat. Simpan salinan di lokasi pilihan Anda atau bagikan melalui aplikasi yang tersedia di HP.

PRIVASI
Tidak perlu akun OTO dan tidak ada iklan. Isi gambar dan hasil pemrosesan ML Kit diproses di perangkat, bukan dikirim ke server Google menurut dokumentasi ML Kit. Google menerima metrik teknis SDK, termasuk informasi perangkat/aplikasi, pengenal, penggunaan, konfigurasi dan kinerja. Baca kebijakan privasi untuk rinciannya.

PERANGKAT
Memerlukan Android 10 atau lebih baru dan Google Play services yang mendukung pemindai. Google menetapkan sedikitnya 1,7 GB RAM untuk Document Scanner. Internet diperlukan saat komponen Google diunduh atau diperbarui. Dokumen yang sudah tersimpan dapat dibaca tanpa membuka kamera.

Dokumen disimpan di ruang aplikasi. Ekspor salinan penting sebelum menghapus data atau mencopot aplikasi.

Dukungan: aftachmadyanigps@gmail.com
Kebijakan privasi: https://oto.ayani.id/oto-scan/kebijakan-privasi
'''
assert len(notes) <= 500 and len(short) <= 80 and len(full) <= 4000
for name, value in [('release-notes-id.txt', notes), ('short-description-id.txt', short), ('full-description-id.txt', full)]:
    (out / name).write_text(value + '\n', encoding='utf-8')
print('Prepared 7 local files; no website or Play changes made.')
