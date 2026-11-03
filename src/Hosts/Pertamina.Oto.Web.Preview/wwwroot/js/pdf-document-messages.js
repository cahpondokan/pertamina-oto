/* Document presentation copy shared by controllers. Profiles retain truthful
 * operation-specific limits; this catalog never authorizes PDF execution. */
(function(root,factory){'use strict';const api=factory(typeof module==='object'&&module.exports?require('./pdf-capacity.js'):root.OtoPdfCapacity);if(typeof module==='object'&&module.exports)module.exports=api;else root.OtoPdfMessages=api;})(typeof globalThis!=='undefined'?globalThis:this,function(C){
 'use strict';
 const COPY_ERRORS={UNSUPPORTED_SIGNATURE:'Tampilan tanda tangan pada PDF ini belum berhasil disiapkan. Coba file lain; file asli tetap utuh.',UNSUPPORTED_FORM:'Tampilan isian PDF ini belum berhasil disiapkan. Coba file lain; file asli tetap utuh.',ENCRYPTED:'PDF belum berhasil dibuka. Periksa kata sandi atau coba kembali; file asli tetap utuh.',COPY_LIMIT:'Salinan kompatibilitas dibatasi 128 MiB dan 200 halaman. Gunakan dokumen lebih kecil.',COPY_XFA:'Formulir XFA ini belum dapat dirender lengkap. File asli tetap utuh; tidak ada salinan kosong yang dibuat.',COPY_RENDER:'Tampilan dokumen belum dapat disalin lengkap. File asli tetap utuh.',COPY_FEATURE:'Gunakan Chrome atau Edge terbaru untuk menyiapkan salinan PDF khusus.',COPY_CANCELLED:'Penyiapan salinan dibatalkan. File asli tetap utuh.'};
 class DocumentMessages {
  constructor(errors,unsupported={}){this.errors=Object.freeze({...errors,...COPY_ERRORS});this.unsupported=Object.freeze({...unsupported});Object.freeze(this);}
  error(error){const code=error?.name==='AbortError'?'CANCELLED':error?.message;return this.errors[code]||(this.unsupported[code]?`PDF memuat ${this.unsupported[code]}. Pemrosesan dihentikan; sumber tetap utuh.`:this.errors.INVALID);}
  compatibility(warnings=[]){return [...new Set(warnings)].map(code=>COMPATIBILITY[code]||'').filter(Boolean).join(' ');}
  summary(warnings=[]){return warnings.length?'Catatan dokumen: '+[...new Set(warnings)].map(code=>SHORT[code]||code).join('; ')+'.':'';}
  details(warnings=[]){return [...new Set(warnings)].map(code=>DETAILS[code]||'Ada bagian dokumen yang perlu diperiksa pada pratinjau.');}
 }
 const COMPATIBILITY=Object.freeze({COPY_STATIC:'Salinan noninteraktif: tampilan halaman dan isian dipertahankan; sertifikat tanda tangan, password, lampiran, tautan dan multimedia tidak disertakan. File asli tetap utuh.',COPY_RASTER:'Salinan berupa gambar halaman: teks tidak dapat dipilih atau dicari. Tanda tangan digital dan fitur interaktif tidak ikut aktif; periksa tampilan hasil.',
    SOURCE_NAVIGATION:'Pengaturan halaman awal dan bookmark sumber tidak disalin ke PDF baru.',
    PARTIAL_TAGS:'Tag yang tersedia dipertahankan. Sebagian dokumen belum bertag lengkap; hasil bukan PDF yang sepenuhnya aksesibel.',
    ORPHAN_TAG:'Tag yang terputus tetap disimpan tanpa menebak urutan baca. Struktur aksesibilitas perlu diperiksa manual.',
    RELATIVE_LINKS:'Tautan file Excel relatif dipertahankan, tidak dibuka atau dicari. Targetnya bergantung pada susunan folder asal.'
  });
 const SHORT=Object.freeze({COPY_STATIC:'salinan noninteraktif; asli tetap utuh',COPY_RASTER:'salinan gambar halaman; periksa hasil',PARTIAL_TAGS:'struktur aksesibilitas belum lengkap; periksa hasil',ORPHAN_TAG:'tag terputus dipertahankan',RELATIVE_LINKS:'tautan file relatif bergantung folder asal',SOURCE_NAVIGATION:'pengaturan halaman awal dan bookmark sumber tidak disalin ke PDF baru'});
 const DETAILS=Object.freeze({COPY_STATIC:'Salinan noninteraktif: tampilan halaman dan isian dipertahankan; sertifikat tanda tangan, password, lampiran, tautan dan multimedia tidak disertakan. File asli tetap utuh.',COPY_RASTER:'Salinan berupa gambar halaman: teks tidak dapat dipilih atau dicari. Tanda tangan digital dan fitur interaktif tidak ikut aktif; periksa tampilan hasil.',PARTIAL_TAGS:'Struktur untuk pembaca layar belum lengkap. Periksa hasil bila diperlukan untuk aksesibilitas.',ORPHAN_TAG:'Sebagian tag sumber tidak terhubung. Tag tersebut tetap dipertahankan.',RELATIVE_LINKS:'Tautan ke file lain bergantung pada lokasi folder asal.',SOURCE_NAVIGATION:'Bookmark dan pengaturan halaman awal sumber tidak disalin.'});
 function merge(){const message={FILE_COUNT:`Maksimal ${C.number(C.merge.files)} PDF. File ke-${C.number(C.merge.files+1)} tidak ditambahkan.`,TOTAL_BYTES:`Total input melebihi ${C.bytes(C.merge.totalBytes)} (${C.number(C.merge.totalBytes)} byte). Kurangi pilihan Anda.`,TYPE:'Pilih file PDF saja.',MERGE:'Tambahkan minimal dua PDF.',RANGE:'Periksa rentang halaman. Contoh: 1, 3-5; tanpa pengulangan.',LIMIT:'Struktur PDF atau gabungan ini melewati batas pemrosesan lokal. Coba lebih sedikit PDF atau rentang halaman; file asli tetap utuh.',STORAGE_BROWSER:'Browser ini belum menyediakan penyimpanan lokal yang diperlukan. Gunakan Chrome atau Edge terbaru pada perangkat ini.',STORAGE:'Penyimpanan sementara tidak cukup atau tidak dapat ditulis. Hasil belum dibuat; file asli tetap utuh.',TIMEOUT:`Proses melewati ${C.merge.timeout/60000} menit dan dihentikan. Pilihan tetap tersedia untuk dicoba kembali.`,CANCELLED:'Proses dibatalkan. Pilihan sebelumnya tetap tersedia; file asli tetap utuh.',INVALID:'PDF rusak atau strukturnya belum didukung. File asli tidak diubah.',ENCRYPTED:'PDF terenkripsi belum didukung. File tidak ditambahkan.',VERIFY:'Hasil tidak lolos pemeriksaan dan telah dibuang.',WORKER:'Mesin lokal tidak dapat dimuat. Buka kembali halaman saat aset aplikasi tersedia.',OPTIONS:'Periksa pengaturan halaman dan hasil.'};return new DocumentMessages({...message,...{CLOUD_CONFIG:'Sumber cloud belum dikonfigurasi oleh pengelola OTO. Gunakan Perangkat untuk PDF lokal.',CLOUD_LOAD:'Komponen sumber cloud belum dapat dimuat. Periksa koneksi, lalu coba lagi.',CLOUD_AUTH:'Akses akun belum berhasil. Pilihan PDF lokal tetap tersedia.',CLOUD_DOWNLOAD:'PDF cloud belum dapat diambil. Coba pilih kembali.',CLOUD_RESPONSE:'Pilihan sumber cloud tidak valid. Pilih kembali file PDF.',CLOUD_CHANGED:'Ukuran file cloud berubah selama pengambilan. Pilih kembali.',CANCELLED:message.CANCELLED}},{UNSUPPORTED_FORM:'formulir berisi',UNSUPPORTED_SIGNATURE:'tanda tangan digital',UNSUPPORTED_ATTACHMENT:'lampiran',UNSUPPORTED_ACTION:'skrip atau aksi otomatis',UNSAFE_LINK:'tautan yang tidak aman',LINK_TARGET:'tujuan tautan yang tidak ikut dipilih',UNSUPPORTED_ANNOTATIONS:'anotasi yang belum didukung',UNSUPPORTED_TAGGED:'tag yang belum didukung',TAGGED_STRUCTURE:'struktur tag yang tidak konsisten',TAGGED_OPERATION:'perubahan yang memutus struktur tag/tabel',TAGGED_CONFLICT:'definisi tag yang bertentangan'});}
 function browser(images=false,tool=images?'images-pdf':'extract'){const limits=C.input(tool);return new DocumentMessages({
    LIMIT:`Dokumen melewati batas pemrosesan browser. Batas input: ${limits.files} ${images?'gambar':'PDF'}, ${C.bytes(limits.fileBytes)} per file${limits.multiple?', '+C.bytes(limits.totalBytes)+' total':''}${images?'':', '+limits.pages+' halaman total'}. Struktur dokumen juga dibatasi; gunakan file lebih kecil.`,
    SESSION_LIMIT:`Memori ruang kerja penuh. Tutup dokumen pada alat yang tidak dipakai: batas bersama ${C.bounded.files} file / ${C.bytes(C.bounded.totalBytes)} input dan ${C.bytes(C.bounded.outputBytes)} hasil.`,
    INVALID:'PDF tidak valid, rusak, atau memakai struktur yang belum didukung. Tidak ada file yang diubah.',
    ENCRYPTED:'PDF ini terenkripsi. Versi web belum mendukung PDF dengan kata sandi.',
    UNSUPPORTED:'PDF ini memiliki formulir, anotasi/tautan, tanda tangan digital, lampiran, atau struktur khusus yang belum didukung. File tidak diubah.',
    UNSUPPORTED_ANNOTATIONS:'PDF ini berisi anotasi atau tautan yang belum dapat dipertahankan oleh alat web. File tidak diubah; anotasi tidak dihapus otomatis.',
    UNSAFE_LINK:'PDF ini berisi tautan dengan alamat atau skema yang tidak aman/didukung. Tautan tidak dijalankan atau dihapus; file asli tetap utuh.',
    LINK_TARGET:'Halaman tujuan tautan tidak ditemukan atau tidak disertakan dalam hasil. Sertakan halaman tujuan atau pertahankan dokumen utuh; tautan tidak dihapus.',
    ANNOTATION_OPERATION:'Duplikasi halaman pada dokumen beranotasi belum didukung karena hubungan anotasi/tautannya dapat menjadi ambigu. Gunakan setiap halaman satu kali; file asli tetap utuh.',
    UNSUPPORTED_FORM:'PDF ini berisi struktur formulir yang belum didukung. File tidak diubah; isian formulir tidak dihapus otomatis.',
    UNSUPPORTED_ACTION:'PDF ini memuat aksi otomatis atau skrip yang tidak diizinkan. Pemrosesan dihentikan; file tidak diubah.',
    UNSUPPORTED_ATTACHMENT:'PDF ini memuat lampiran atau referensi file yang belum didukung. File tidak diubah; lampiran tidak dihapus otomatis.',
    UNSUPPORTED_SIGNATURE:'PDF ini memuat struktur tanda tangan digital atau izin sertifikasi. Pemrosesan dihentikan agar tidak mengubah keabsahannya; file tidak diubah.',
    UNSUPPORTED_TAGGED:'PDF ini memakai jenis tag aksesibilitas lanjutan yang belum didukung. Tag tidak dihapus; file asli tetap utuh.',
    TAGGED_STRUCTURE:'Hubungan tag aksesibilitas PDF ini tidak lengkap atau tidak konsisten. Pemrosesan dihentikan agar struktur tidak rusak; file asli tetap utuh.',
    TAGGED_OPERATION:'Perubahan halaman ini akan memutus hubungan tag atau tabel, menduplikasi tag, atau menyisipkan halaman ke tengah struktur dokumen lain. Gunakan susunan utuh per dokumen; file asli tetap utuh.',
    TAGGED_MIXED:'Penggabungan PDF bertag dengan PDF tanpa tag belum didukung tanpa kehilangan cakupan aksesibilitas. Gunakan dokumen bertag saja atau dokumen tanpa tag saja; file asli tetap utuh.',
    TAGGED_CONFLICT:'Dokumen memakai nama tag khusus yang sama dengan arti berbeda. Penggabungan dihentikan agar struktur aksesibilitas tidak berubah; file asli tetap utuh.',
    RANGE:'Periksa nomor halaman: gunakan angka dalam jangkauan, tanpa pengulangan. Contoh: 1, 3-5.',
    TOTAL_BYTES:`Total ukuran file melebihi ${C.bytes(limits.totalBytes)} (${C.number(limits.totalBytes)} byte). File asli tidak diubah.`,
    OUTPUT_LIMIT:`Total hasil melebihi ${C.bytes(limits.outputBytes)}. Hasil belum diterbitkan; kurangi isi atau jumlah halaman.`,
    STORAGE:'Ruang penyimpanan sementara tidak cukup atau tidak dapat ditulis. Kosongkan ruang atau coba perangkat lain; file asli tetap utuh.',
    STORAGE_BROWSER:'Browser ini belum mendukung penyimpanan lokal yang diperlukan. Gunakan Chrome atau Edge yang mendukung OPFS dan Web Locks.',
    FILE_COUNT:`Maksimal ${limits.files} file untuk alat ini.`,
    BUSY:'Alat PDF sedang memproses pekerjaan lain pada situs ini. Tunggu atau batalkan pekerjaan tersebut dahulu.',
    SINGLE:'Alat ini memakai satu PDF. Tutup dokumen saat ini untuk memilih file lain.',
    MERGE:'Tambahkan minimal dua PDF untuk digabungkan.',
    EMPTY:'Sisakan minimal satu halaman pada hasil.',
    SPLIT:`Buat ${C.splitMinimumParts}–${C.split.outputs} bagian, dipisahkan titik koma. Contoh: 1-2; 3-4.`,
    PARTITION:'Semua halaman harus masuk tepat satu bagian. Gunakan Ekstrak halaman untuk mengambil sebagian saja.',
    VERIFY:'Hasil belum lolos pemeriksaan. Tidak ada hasil yang diberikan.',
    TIMEOUT:`Proses melewati ${limits.timeout>=60000?limits.timeout/60000+' menit':limits.timeout/1000+' detik'} dan dihentikan. Coba file lebih kecil.`,
    CANCELLED:'Proses dibatalkan. File asli dan dokumen sebelumnya tetap utuh.',
    WORKER:'Mesin PDF tidak dapat dimuat. Periksa koneksi ke halaman ini, lalu coba lagi.',
    TYPE:images?'Pilih JPG atau PNG saja. Format lain belum didukung.':'Pilih file PDF saja. Teks atau file jenis lain tidak ditambahkan.',
    OPTIONS:'Periksa pengaturan hasil. Isi angka dalam batas yang ditampilkan dan jangan kosongkan nilai wajib.',
    TEXT:'Isi tanda air dengan 1–80 karakter Latin/angka/tanda baca yang didukung. Emoji, baris baru, dan aksara lain belum didukung.',
    LAYOUT:'Pengaturan tidak muat pada halaman, atau geometri PDF belum didukung. Kurangi margin, potongan, atau ukuran teks dan coba lagi.',
    IMAGE_FORMAT:'Gambar tidak valid atau format belum didukung. Gunakan JPG RGB/grayscale atau PNG 8-bit statis.',
    IMAGE_LIMIT:`Gambar terlalu besar: maks. ${C.images.pixels/1000000} juta piksel per gambar, ${C.images.totalPixels/1000000} juta piksel total, dan ${C.number(C.images.dimension)} piksel per sisi.`,
    IMAGE_BROWSER:'Browser ini belum mendukung pemrosesan gambar lokal yang diperlukan. Coba browser desktop terbaru.'
  });}
 return {DocumentMessages,merge,browser};
});
