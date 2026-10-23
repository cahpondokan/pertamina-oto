"""Generate store text and branded graphics from existing first-party assets."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import html, json, re

root=Path(__file__).resolve().parents[1]
out=root/'release-output/1.0.0/store'
out.mkdir(parents=True,exist_ok=True)
short='Scan dokumen ke PDF, rapikan tepi, cari dan bagikan. Diproses di perangkat.'
full='''OTO Scan membantu Anda memindai dokumen, catatan, nota, dan kartu menjadi PDF langsung dari perangkat Android.

PINDAI DAN RAPIKAN
Gunakan kamera untuk mengambil dokumen. Deteksi tepi membantu memotong kertas secara otomatis. Bila diperlukan, sesuaikan sudut secara manual, putar halaman, atau pilih tampilan jernih, warna asli, abu-abu, dan hitam putih.

SIMPAN SEBAGAI PDF
Gabungkan hingga 20 halaman dalam satu PDF. Pengolahan gambar dan kompresi menjaga detail tulisan sambil menghemat ukuran file. Hasil akhirnya tetap bergantung pada fokus, pencahayaan, jarak kamera, dan kondisi dokumen.

DOKUMEN SAYA
Lihat dokumen melalui galeri dengan pratinjau halaman. Cari berdasarkan nama, ganti nama, dan pilih beberapa dokumen untuk dibagikan sekaligus. Dokumen tersimpan di ruang aplikasi pada perangkat.

BACA DAN BAGIKAN
Baca PDF dengan menggulir ke bawah dan memperbesar halaman. Halaman potret dan landscape mempertahankan perbandingan ukurannya. Buka PDF dari aplikasi lain yang memberikan izin akses, lalu gunakan pilihan berbagi Android atau simpan salinan di lokasi yang Anda pilih. PDF yang terkunci kata sandi belum didukung.

PEMROSESAN LOKAL
Pemindaian, pemotongan, perbaikan gambar, dan pembuatan PDF berlangsung di perangkat. Tidak perlu akun. Tidak ada iklan atau analitik dalam aplikasi. Koneksi internet hanya mungkin diperlukan oleh aplikasi tujuan saat Anda membagikan atau menyimpan salinan.

OTO Scan gratis digunakan. Aplikasi memerlukan Android 10 atau lebih baru. Izin kamera digunakan untuk pemindaian; dokumen tersimpan dan pembaca PDF tetap dapat digunakan tanpa izin kamera.

Simpan salinan PDF penting sebelum menghapus data atau mencopot aplikasi. Salinan di luar OTO Scan dikelola melalui aplikasi atau lokasi penyimpanan yang Anda pilih.'''
assert len(short)<=80 and len(full)<=4000
(out/'title-id.txt').write_text('OTO Scan',encoding='utf-8')
(out/'short-description-id.txt').write_text(short,encoding='utf-8')
(out/'full-description-id.txt').write_text(full,encoding='utf-8')
(out/'release-notes-id.txt').write_text('''<id>
Rilis pertama OTO Scan: scan dan potong dokumen, simpan PDF, baca dengan gulir vertikal dan zoom, cari melalui galeri, serta bagikan beberapa dokumen sekaligus. Pemrosesan dilakukan di perangkat, tanpa akun dan tanpa iklan.
</id>''',encoding='utf-8')
source=(root/'lib/privacy_screen.dart').read_text(encoding='utf-8')
policy=re.search(r"const privacyPolicy = '''(.*?)''';",source,re.S).group(1)
sections=policy.split('\n\n')
body='\n'.join('<p>'+html.escape(s).replace('\n','<br>')+'</p>' for s in sections)
body=body.replace('Kontak dukungan pengembang tersedia pada halaman OTO Scan di Google Play.', 'Pengembang: AFT Ahmad Yani. Hubungi <a href="mailto:aftachmadyanigps@gmail.com">aftachmadyanigps@gmail.com</a> untuk pertanyaan privasi dan dukungan OTO Scan.')
page='''<!doctype html><html lang="id"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Kebijakan Privasi OTO Scan</title><style>body{font:17px/1.7 system-ui,sans-serif;color:#09243c;margin:0;background:#f0f1f3}main{max-width:760px;margin:32px auto;padding:32px;background:white;border-radius:16px}h1{font-size:32px;line-height:1.2}img{width:76px;background:#09243c;border-radius:16px;padding:12px}p{margin:24px 0}@media(max-width:600px){main{margin:0;padding:24px;border-radius:0}}a{color:#a90024}</style><main><img src="icon-512.png" alt="Ikon OTO Scan"><h1>Kebijakan Privasi OTO Scan</h1><p>Package Android: <strong>com.niam.ayani.oto</strong></p>'''+body+'</main></html>'
(out/'privacy-policy.html').write_text(page,encoding='utf-8')
icon=Image.open(root/'assets/oto.png').convert('RGBA')
store_icon=Image.new('RGBA',(512,512),'#09243c')
store_icon.paste(icon.resize((448,448),Image.Resampling.LANCZOS),(32,32),icon.resize((448,448),Image.Resampling.LANCZOS))
store_icon.save(out/'icon-512.png')
banner=Image.new('RGB',(1024,500),'#09243c');d=ImageDraw.Draw(banner)
font=lambda size,bold=False:ImageFont.truetype('C:/Windows/Fonts/'+('segoeuib.ttf' if bold else 'segoeui.ttf'),size)
d.rounded_rectangle((44,82,320,358),radius=44,fill='#132f49')
brand=icon.resize((250,250),Image.Resampling.LANCZOS);banner.paste(brand,(57,95),brand)
d.text((368,95),'OTO Scan',font=font(66,True),fill='white')
d.text((371,197),'Dokumen rapi. Mudah dibagikan.',font=font(29),fill='white')
d.text((371,259),'Pindai • Simpan PDF • Kirim',font=font(27),fill='#c2d0dc')
d.rounded_rectangle((372,331,740,383),radius=26,fill='#f5102e')
d.text((394,339),'Diproses di perangkat Anda',font=font(23,True),fill='white')
banner.save(out/'feature-graphic-1024x500.png')
declarations={
 'status':'release uploaded as internal draft; listing saved; see console-status.json for current progress',
 'app':'OTO Scan','package':'com.niam.ayani.oto','language':'id','pricing':'free','country_proposal':['Indonesia'],
 'category_proposal':'Productivity','ads':False,'account_required':False,'in_app_purchases':False,
 'data_collected_by_developer':False,'data_shared_by_developer':False,
 'user_initiated_sharing':'Android chooser transfers only selected PDFs to an app selected by the user; user-initiated sharing exception must be mapped to the current Console questionnaire.',
 'permissions':['CAMERA (scan only)'],
 'app_access':'All functionality accessible without sign-in. Allow camera to scan; use Dokumen Saya for saved PDFs.',
 'target_audience':'13+ general productivity; owner approved and Console saved 13-15, 16-17, 18+',
 'content_rating':'Answer official questionnaire from actual app behavior; do not invent an IARC rating.',
 'health_features':False,'financial_features':False,'news_app':False,
 'support_email':'aftachmadyanigps@gmail.com','privacy_url':'https://oto.ayani.id/oto-scan/kebijakan-privasi',
 'owner_approvals':['public support email','public privacy website','AI labels for icon and feature graphic only; five emulator screenshots not AI'],
 'limits':['Android 10+','20 pages per scanned PDF','No OCR/text search: search is by document name','No password-protected PDFs','Scan clarity depends on capture quality'],
 'private_key':'Excluded from this package. Owner backup required; Windows DPAPI password is bound to this Windows user.',
}
(out/'console-preparation.json').write_text(json.dumps(declarations,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'short_chars':len(short),'full_chars':len(full),'output':str(out)},ensure_ascii=False))
