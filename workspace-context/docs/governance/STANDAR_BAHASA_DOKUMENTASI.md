# STANDAR BAHASA DOKUMENTASI
## PERTAMINA OTO — Office Tools Organizer

> **Versi standar:** ID-1.0  
> **Tanggal berlaku:** 27 Agustus 2026  
> **Status:** DIKUNCI  
> **Cakupan:** Seluruh dokumen proyek 01–19 dan artefak pendukung

---

# 1. Kebijakan Utama

Bahasa Indonesia adalah bahasa utama dan resmi untuk narasi dokumentasi PERTAMINA OTO. Standar ini berlaku untuk judul, penjelasan, keputusan, alasan, dampak, aturan, tabel naratif, risiko, temuan audit, kriteria penerimaan, panduan, dan catatan serah terima.

Bahasa Inggris hanya dipertahankan ketika merupakan identitas teknis canonical, nama resmi teknologi/produk, sintaks, atau istilah yang kehilangan ketepatan apabila diterjemahkan tanpa padanan.

---

# 2. Aturan Penulisan

1. Gunakan Bahasa Indonesia yang formal, ringkas, jelas, dan konsisten.
2. Gunakan istilah Indonesia terlebih dahulu, lalu istilah Inggris dalam tanda kurung pada kemunculan pertama jika diperlukan.
3. Setelah istilah diperkenalkan, gunakan satu bentuk yang konsisten dalam dokumen yang sama.
4. Jangan mencampur bahasa dalam satu kalimat jika tersedia padanan Indonesia yang jelas.
5. Akronim teknis boleh dipertahankan dan dijelaskan pada glosarium.
6. Istilah normatif menggunakan `WAJIB`, `DILARANG`, `SEHARUSNYA`, `DAPAT`, dan `OPSIONAL` secara konsisten.
7. Status machine-readable tetap menggunakan nilai canonical yang telah dikunci.

---

# 3. Elemen yang Tidak Diterjemahkan

- Feature ID, Requirement ID, Decision ID, dan identifier canonical lain;
- endpoint API, HTTP method, header, media type, dan route parameter;
- permission code, policy code, claim, scope code, dan workload identity code;
- nama schema, tabel, kolom, entity, class, interface, method, package, dan namespace;
- JSON/XML/YAML, kode program, regex, SQL, command, dan contoh payload;
- enum, state, status code, Error Code, Event Type, dan nilai machine-readable;
- nama teknologi/produk resmi seperti C#, .NET, ASP.NET Core, WinUI 3, .NET MAUI, EF Core, SQL Server, SQLite, REST, SignalR, OpenAPI, dan Application Insights;
- nama file canonical dan struktur folder yang sudah menjadi traceability baseline;
- kutipan standar eksternal yang harus dipertahankan persis, dengan penjelasan Bahasa Indonesia.

---

# 4. Padanan Istilah Baku

| Istilah Inggris | Istilah Indonesia yang digunakan |
|---|---|
| Purpose | Tujuan |
| Scope | Ruang Lingkup |
| Decision | Keputusan |
| Reason | Alasan |
| Impact | Dampak |
| Requirement | Kebutuhan |
| Feature | Fitur, kecuali `Feature ID` |
| Access Control | Kontrol Akses |
| Authentication | Autentikasi |
| Authorization | Otorisasi |
| Audit Trail | Jejak Audit atau Enterprise Audit sesuai konteks |
| User History | Riwayat Pengguna |
| Technical Log | Log Teknis |
| Issue Report | Laporan Masalah |
| Data Model | Model Data |
| Failure and Recovery | Kegagalan dan Pemulihan |
| Acceptance Criteria | Kriteria Penerimaan |
| Monitoring | Pemantauan |
| Health | Kesehatan layanan/sistem |
| Retention | Retensi |
| Archive | Arsip |
| Purge | Penghapusan permanen berdasarkan kebijakan |
| Query | Kueri |
| Export | Ekspor |
| Retry | Percobaan ulang |
| Backpressure | Pengendalian tekanan beban (`backpressure`) pada kemunculan pertama |
| Outbox | `outbox` karena merupakan pola/artefak teknis |
| Worker | Worker karena merupakan jenis deployable/identity canonical |

---

# 5. Kontrol Revisi Bahasa

Revisi bahasa:

- tidak boleh mengubah keputusan, hak akses, ruang lingkup bisnis, requirement, Feature ID, kontrak API, schema database, atau perilaku sistem;
- tidak mengubah versi teknis dokumen jika hanya memperbaiki bahasa;
- menggunakan penanda revisi bahasa `ID-1.0`;
- harus memiliki audit sebelum/sesudah dan validasi struktur/identifier;
- harus menandai konflik makna sebagai controlled technical revision, bukan menerjemahkannya diam-diam;
- mempertahankan dokumen sumber/final yang berwenang dan jalur traceability.

---

# 6. Kriteria Lulus

Dokumen dinyatakan sesuai apabila:

1. narasi utamanya berbahasa Indonesia;
2. judul dan tabel naratif menggunakan Bahasa Indonesia;
3. istilah Inggris yang tersisa termasuk dalam pengecualian atau telah dijelaskan;
4. seluruh identifier canonical sebelum dan sesudah tetap sama;
5. keputusan dan status tidak berubah;
6. code fence, tabel, tautan, dan contoh kontrak tetap valid;
7. tidak muncul angka baseline lama atau Feature ID baru;
8. hasil tercatat pada audit bahasa.

---

# 7. Status

```text
Standar bahasa : Bahasa Indonesia
Kode revisi    : ID-1.0
Status         : DIKUNCI
Cakupan        : Documents 01–19 + artefak pendukung
```

