# AGENT A — Account, Role, Permission & Access Administration Report

**Task ID:** DOC15-AGENT-A  
**Tujuan:** Rancangan prosedur Admin Manual PERTAMINA OTO untuk administrasi account dan akses  
**Status:** Rekomendasi specialist; bukan perubahan terhadap master Document 15  
**Baseline sumber:** Document 01–14 FINAL dan `MASTER_PROJECT_SUMMARY.md` v3.7

> **Batas implementasi:** `FEAT-ADMIN-001` User Management, `FEAT-ADMIN-002` Role Management, `FEAT-ADMIN-003` Permission Management, dan `FEAT-AUTH-007` Session Revocation berstatus `PLANNED`. Document 09 juga menyatakan operasi tersebut memerlukan addendum contract Document 06 yang terkendali. Prosedur dalam laporan ini adalah baseline operasional pra-implementasi; nama layar atau tombol diberi tanda `TBD_UI_LABEL` dan tidak boleh dianggap tersedia sebelum dibuktikan pada build yang disetujui.

## 1. Target administrator

Panduan ini ditujukan kepada personel yang memperoleh kewenangan administratif PERTAMINA OTO secara eksplisit dan berada dalam scope delegasi yang disetujui. Memiliki jabatan, akses server, akses database, atau label “Superadmin” tidak dengan sendirinya menjadikan seseorang administrator aplikasi.

Target pembaca:

- Administrator OTO yang mengelola kelayakan account, application session, role, dan permission dalam scope delegasinya;
- administrator keamanan yang melakukan perubahan privileged dengan segregation of duties (SoD) dan reauthentication;
- reviewer independen yang memverifikasi perubahan akses;
- Auditor yang memeriksa bukti perubahan melalui grant Audit terpisah;
- personel Support yang perlu memahami bahwa capability Support bukan kewenangan administrasi atau akses dokumen umum.

Prasyarat sebelum melakukan administrasi:

1. Gunakan account personal, bukan shared account.
2. Pastikan perangkat dan aplikasi berada pada versi yang didukung.
3. Pastikan koneksi tersedia; seluruh administrasi account/role/permission hanya daring.
4. Pastikan permission tindakan dan `ADMIN_SCOPE` mencakup target.
5. Siapkan alasan bisnis, nomor permintaan/perubahan, dan persetujuan yang diwajibkan policy.
6. Pastikan tidak ada konflik SoD atau self-assignment.
7. Selesaikan reauthentication ketika diminta.
8. Jangan membuka file pengguna untuk membuktikan perubahan akses.

Role formal hanya:

| Role | Tujuan | Batas utama |
|---|---|---|
| `User` | Menggunakan feature yang disetujui pada sumber daya sendiri | Tanpa kewenangan administrasi, Audit, Support, atau lintas pengguna |
| `Administrator` | Mengelola control plane dan metadata aman dalam `ADMIN_SCOPE` | Tidak otomatis memiliki capability User, Auditor, Support, atau content access |
| `Auditor` | Membaca bukti Enterprise Audit yang disetujui dalam `AUDIT_SCOPE` | Read-only; tanpa perubahan account, permission, policy, atau Audit event |

“Superadmin” adalah label UI untuk profil permission Support yang dibatasi. Ia bukan role formal keempat dan bukan tingkat kewenangan di atas Administrator.

## 2. Account administration

### 2.1 Batas kewenangan

Pertamina Identity mengautentikasi pengguna. PERTAMINA OTO mengelola kelayakan aplikasi, application session, role, permission, scope, dan keputusan akses terhadap tindakan/sumber daya OTO.

Administrator OTO dapat diberi permission granular berikut:

| Kebutuhan | Permission canonical | Batas |
|---|---|---|
| Mencari account | `admin.user.list` | Safe metadata dalam `ADMIN_SCOPE` |
| Melihat account summary | `admin.user.read` | Tanpa password, token, raw claim, atau document content |
| Mengubah kelayakan OTO | `admin.user.update` | Daring, reauthentication, SoD, exact target, current version |
| Melihat application session | `admin.session.read` | Metadata aman; tanpa credential/cookie/token |
| Mencabut application session | `admin.session.revoke` | Daring, reauthentication, idempotent, audited |
| Melihat role | `admin.role.list`, `admin.role.read` | Role canonical dan assignment dalam scope |
| Assign/revoke role | `admin.role.assign`, `admin.role.revoke` | Daring, reauthentication, delegation/SoD, no self-elevation |
| Melihat permission | `admin.permission.list`, `admin.permission.read` | Definisi dan assignment yang diizinkan |
| Assign/revoke permission/profile | `admin.permission.assign`, `admin.permission.revoke` | Daring, reauthentication, delegation/SoD |

Administrator OTO tidak melakukan:

- reset password Pertamina;
- reset atau penggantian MFA Pertamina;
- enterprise account recovery atau account unlock independen;
- pembuatan identity provider atau identity paralel;
- perubahan raw claim;
- pemberian wildcard permission;
- impersonation atau “view as user”;
- perubahan owner sumber daya sebagai jalan pintas;
- akses file pengguna untuk pengujian administratif.

Jika masalah berada pada autentikasi, password, MFA, enterprise identity, atau directory Pertamina, arahkan melalui prosedur Pertamina yang disetujui. Detail jalurnya berstatus `REQUIRES_PERTAMINA_SSO_CONFIRMATION`.

### 2.2 Empat status yang tidak boleh dicampur

| Lapisan | Pertanyaan yang dijawab | Contoh status | Pemilik kewenangan |
|---|---|---|---|
| Identity Pertamina | Apakah identity dapat diautentikasi oleh Pertamina? | Sesuai status resmi Pertamina | Pertamina Identity/SSO |
| Kelayakan account OTO | Apakah principal boleh memakai aplikasi OTO? | Active / Disabled | OTO sesuai policy/delegasi |
| Application session | Apakah session aplikasi masih berlaku? | Active / Expired / Revoked | OTO dan sinyal identity/policy |
| Effective access | Bolehkah tindakan tepat dilakukan pada sumber daya tepat saat ini? | Allowed / Denied + reason aman | Otorisasi OTO saat ini |

Satu lapisan tidak membuktikan lapisan lain. Account OTO yang aktif tetap dapat gagal login apabila Pertamina Identity menolak autentikasi. Role yang benar tetap dapat menghasilkan deny karena scope, ownership, policy, connectivity, resource state, capability, atau reauthentication.

## 3. User search/status

### 3.1 Mencari pengguna

Gunakan layar konseptual **`TBD_UI_LABEL: Account Administration`** hanya setelah tersedia pada build yang disetujui.

1. Buka **`TBD_UI_LABEL: Administration › Accounts`**.
2. Gunakan identifier yang diizinkan oleh directory selector atau field pencarian yang disetujui.
3. Masukkan nilai yang cukup spesifik; hindari pencarian luas tanpa tujuan.
4. Pilih pengguna dari hasil yang berada dalam `ADMIN_SCOPE` Anda.
5. Cocokkan identifier aman, unit/scope yang diizinkan, dan status sebelum membuka ringkasan.

Aturan:

- hasil di-scope sebelum count, search, filter, sort, dan paging;
- hasil tidak boleh menampilkan raw claim, password, token, credential, atau informasi file;
- display name atau email tidak dipakai sebagai identity stabil;
- tidak adanya hasil tidak boleh membocorkan account di luar scope;
- pencarian account bukan sarana mencari file atau aktivitas isi pengguna.

### 3.2 Account summary minimum

Ringkasan account yang aman sebaiknya memperlihatkan hanya data yang telah disetujui:

- identifier principal internal yang dimasking;
- display reference yang diizinkan;
- OTO eligibility: Active atau Disabled;
- status application session secara ringkas: Active, Expired, atau Revoked;
- role saat ini;
- permission/profile assignment yang terlihat dalam scope;
- effective permission summary dan sumber assignment;
- delegation/scope code yang aman;
- versi assignment/policy yang relevan;
- metadata last known access bila purpose, field, retention, dan permission telah disetujui;
- status apakah tindakan tertentu online-only atau membutuhkan reauthentication.

Jangan tampilkan:

- password, token, cookie, secret, raw claim, atau header;
- nama/path/preview/thumbnail/isi file;
- OCR text, Assistant prompt/chat, AI payload, clipboard, atau signature asset;
- unrelated Issue atau attachment;
- data pengguna lain di luar `ADMIN_SCOPE`.

### 3.3 Membaca status

| Status | Arti administratif | Tindakan yang diperbolehkan |
|---|---|---|
| **Active** | Principal OTO memenuhi kelayakan aplikasi saat ringkasan dibuat | Review effective access; jangan menganggap semua tindakan allowed |
| **Disabled** | Kelayakan aplikasi OTO dinonaktifkan | Cari alasan/status perubahan yang aman; re-enable hanya bila SSO/policy/delegasi mengizinkan |
| **Revoked** | Application session/grant tertentu telah dicabut, atau sinyal pencabutan identity telah diketahui | Identifikasi objek yang revoked; jangan menyamakan session revocation dengan penghapusan account |
| **Expired** | Session atau grant telah melewati masa berlaku | Pengguna perlu login/otorisasi ulang; jangan memperpanjang manual |
| **Unknown/Unavailable** | Status upstream atau metadata tidak dapat dipastikan | Fail closed untuk mutasi; jangan menebak Active |

Exact enum status, mapping status enterprise ke OTO, dan apakah “revoked account” merupakan status identity, eligibility, atau session harus dikonfirmasi: `REQUIRES_PERTAMINA_SSO_CONFIRMATION`.

## 4. Account lifecycle

### 4.1 Menonaktifkan kelayakan account OTO

1. Cari pengguna dan buka account summary.
2. Pastikan target, `ADMIN_SCOPE`, dan status saat ini benar.
3. Periksa perubahan role/permission atau policy yang masih tertunda.
4. Pilih **`TBD_UI_LABEL: Disable OTO Access`**.
5. Baca dampak terhadap login, session, pekerjaan tertunda, dan offline access.
6. Masukkan reason code yang disetujui dan referensi permintaan/perubahan bila diwajibkan.
7. Selesaikan reauthentication.
8. Konfirmasi exact target dan perubahan Active → Disabled.
9. Simpan; jangan ulangi request jika hasil belum jelas.
10. Verifikasi status baru, effective access, session yang terdampak, dan audit receipt.

Hasil yang diharapkan:

- akses API/notification baru dihentikan setelah perubahan berwenang diketahui;
- application session dan offline grant dibatalkan sesuai desain invalidation;
- queued/active work tidak boleh mengekspos atau memberi kontrol hasil kepada principal yang tidak lagi berwenang;
- output yang tidak boleh dilanjutkan dibatalkan atau dikarantina pada batas aman;
- source, draft, atau file tidak dihapus secara diam-diam;
- audit mencatat outcome dan target opaque tanpa isi sensitif.

### 4.2 Mengaktifkan kembali kelayakan account OTO

Re-enable hanya dilakukan jika Pertamina Identity, enterprise policy, delegation, SoD, dan status account mengizinkan.

1. Cari pengguna yang berstatus Disabled.
2. Pastikan Anda tidak sedang mencoba membuka kembali enterprise identity yang dikelola Pertamina SSO.
3. Periksa reason/status disablement, persetujuan, scope, dan status Pertamina yang tersedia secara resmi.
4. Periksa role/permission yang masih valid; jangan otomatis memulihkan grant yang telah dicabut.
5. Pilih **`TBD_UI_LABEL: Re-enable OTO Access`**.
6. Masukkan reason code dan referensi persetujuan yang diwajibkan.
7. Selesaikan reauthentication.
8. Konfirmasi exact target dan perubahan Disabled → Active.
9. Simpan dan tunggu outcome berwenang.
10. Verifikasi effective access minimum; minta pengguna login kembali secara online.
11. Periksa audit receipt/event sesuai kewenangan terpisah.

`REQUIRES_PERTAMINA_SSO_CONFIRMATION`: apakah administrator OTO boleh memulai re-enable, apakah diperlukan approval tambahan, bagaimana status enterprise identity diverifikasi, dan apakah re-enable OTO diperbolehkan ketika identity Pertamina masih disabled/revoked.

### 4.3 Mencabut application session

1. Buka tab **`TBD_UI_LABEL: Sessions`** pada account summary.
2. Periksa referensi session aman, platform/client, status, dan waktu yang diizinkan.
3. Pilih session tepat; jangan menggunakan identifier sebagai bukti scope.
4. Pilih **`TBD_UI_LABEL: Revoke Session`**.
5. Masukkan reason code dan referensi perubahan.
6. Selesaikan reauthentication.
7. Konfirmasi exact account dan session.
8. Simpan dan verifikasi status Revoked.
9. Periksa audit receipt; jangan mengharapkan logout menghapus file/draft secara otomatis.

### 4.4 Last known access metadata

Metadata akses terakhir hanya boleh ditampilkan jika ada tujuan operasional yang disetujui, permission yang tepat, field allowlist, klasifikasi, masking, sumber waktu, dan retention yang jelas. Tampilkan waktu dengan zona yang eksplisit dan label sumber, misalnya “last successful application session” alih-alih label ambigu “last active”.

Jangan menyimpulkan aktivitas pengguna dari satu timestamp. Jangan membuat field baru atau mengambil data dari Audit secara tidak terkontrol. Sumber, definisi, granularity, retention, dan visibility berstatus `REQUIRES_PERTAMINA_SSO_CONFIRMATION` untuk bagian identity dan memerlukan konfirmasi data/Audit OTO untuk bagian aplikasi.

## 5. Role assignment

Role adalah bundle permission. Role bukan keputusan akhir, bukan clearance content, dan tidak menciptakan ownership.

### Prosedur assign role

1. **Cari user.** Gunakan pencarian yang disetujui dan pilih exact principal dalam `ADMIN_SCOPE`.
2. **Buka akses.** Pilih **`TBD_UI_LABEL: Access`** pada account summary.
3. **Lihat role saat ini.** Periksa seluruh role aktif, assignment source, scope, versi, dan status revocation.
4. **Pilih role.** Pilih hanya `User`, `Administrator`, atau `Auditor`; tidak tersedia role Superadmin.
5. **Lihat permission yang berubah.** Periksa delta permission: ditambahkan, tetap, hilang karena konflik, online-only, reauth-required, dan content-access flag.
6. **Isi alasan.** Gunakan reason code dan referensi permintaan/persetujuan. Jangan memasukkan isi dokumen atau data sensitif.
7. **Konfirmasi.** Periksa target, role, scope, dampak, SoD, no self-assignment, current version, dan approval.
8. **Simpan.** Selesaikan reauthentication dan kirim satu perubahan idempotent; tangani version conflict sebelum retry.
9. **Verifikasi effective access.** Pastikan hasil setelah policy, scope, ownership, connectivity, dan reauthentication—bukan hanya role baru tampil.
10. **Periksa audit event.** Administrator memeriksa audit receipt. Event sebenarnya diverifikasi oleh Auditor atau principal dengan `audit.event.read` terpisah; Administrator tidak memperoleh akses Enterprise Audit secara implisit.

Expected audit event: `Administration.RoleAssigned` dengan initiator, target opaque, role/scope/version, before/after code, outcome, correlation, dan hasil reauthentication yang aman.

### Prosedur revoke role

1. Cari user dan buka akses.
2. Pilih exact role assignment, bukan definisi role canonical.
3. Periksa permission yang akan hilang dan permission yang tetap berasal dari role/profile lain.
4. Periksa job/session/action sensitif yang mungkin memerlukan invalidation.
5. Masukkan reason code dan referensi persetujuan.
6. Selesaikan reauthentication.
7. Konfirmasi target, role, scope, dan current version.
8. Simpan.
9. Verifikasi effective access dan session state saat ini.
10. Periksa audit receipt/event `Administration.RoleRevoked` sesuai pemisahan akses Audit.

Aturan wajib:

- tidak ada hierarki role;
- Administrator tidak mewarisi User, Auditor, atau Support;
- Auditor tidak mewarisi User, Administrator, atau Support;
- role multiple hanya dapat digunakan bila disetujui dan lolos SoD;
- deny/hard policy mengalahkan role grant;
- perubahan role tidak boleh mengubah makna permission canonical;
- role tidak boleh dipakai untuk memberi akses dokumen lintas pengguna.

## 6. Permission assignment

Permission mengizinkan principal mencoba tindakan. Hasil akhir tetap bergantung pada account, session, subject type, scope/ownership, policy, resource state/version, platform/capability, connectivity, consent, dan fresh reauthentication.

### Sebelum assign

Periksa setiap atribut permission:

| Atribut | Pertanyaan admin |
|---|---|
| Code | Apakah kode canonical `domain.resource.action` tepat dan aktif? |
| Assignment source | Apakah grant berasal dari role, direct assignment, atau Support profile? |
| Subject type | Apakah permission untuk manusia, bukan workload? |
| Scope | Sumber daya/unit/queue mana yang dicakup? |
| Sensitivity | Normal, elevated, sensitive, destructive, atau bulk disclosure? |
| Connectivity | Apakah hanya daring? |
| Reauthentication | Apakah wajib atau kondisional? |
| Content access | Apakah akses isi dinyatakan eksplisit? Jangan menyimpulkannya dari metadata access |
| SoD/delegation | Apakah admin boleh memberi grant ini kepada target tersebut? |
| Expiry/version | Apakah assignment masih valid dan menggunakan versi saat ini? |

### Prosedur assign permission/profile

1. Cari user dan buka **`TBD_UI_LABEL: Access › Permissions`**.
2. Lihat permission yang berasal dari role dan assignment terpisah.
3. Pilih **`TBD_UI_LABEL: Add Permission`**.
4. Pilih kode canonical dari katalog; jangan mengetik wildcard atau kode baru.
5. Tentukan scope yang disetujui. Jangan gunakan `ALL` implisit.
6. Periksa sensitivity, online-only, reauthentication, ownership limitation, dan content-access flag.
7. Periksa konflik policy, SoD, duplicate grant, assignment expiry, dan kewenangan delegasi.
8. Periksa delta effective access sebelum menyimpan.
9. Masukkan reason code dan referensi approval.
10. Selesaikan reauthentication.
11. Konfirmasi exact target, permission/profile, scope, dan dampak.
12. Simpan dan tangani version conflict secara eksplisit.
13. Verifikasi effective access dan audit receipt/event `Administration.PermissionAssigned` atau `Administration.SupportGrantChanged`.

### Default, inherited, dan effective permission

- **Default role permission** berasal dari role formal yang aktif.
- **Inherited permission** dalam UI berarti berasal dari bundle role/profile, bukan role hierarchy; role tidak mewarisi role lain.
- **Direct/optional permission** adalah assignment terpisah yang harus mempunyai scope, tujuan, dan lifecycle yang jelas.
- **Effective permission** adalah gabungan grant yang masih aktif setelah deny, SoD, policy, scope, ownership, resource state, connectivity, dan reauthentication diperhitungkan.
- **Visible action** bukan bukti effective permission; server tetap membuat keputusan akhir.

> **Gap implementasi:** Document 07 belum mendefinisikan persistence assignment permission langsung kepada principal atau scoped Support grant, dan Document 06 belum memiliki kontrak administrasi yang diperlukan. Prosedur direct permission/profile assignment tidak boleh dioperasionalkan sampai controlled addendum disetujui. Laporan ini tidak menciptakan tabel, endpoint, atau mekanisme baru.

## 7. Permission revocation

### Prosedur revoke

1. Cari user dan buka **`TBD_UI_LABEL: Access › Permissions`**.
2. Pilih exact assignment dan lihat sumbernya.
3. Jika permission berasal dari role, revoke role atau ubah role mapping melalui governance yang benar; jangan membuat deny ad hoc yang tidak didukung.
4. Periksa tindakan/resource yang akan terdampak, session aktif, offline grant, job tertunda, dan assignment lain yang dapat mempertahankan permission.
5. Pilih **`TBD_UI_LABEL: Revoke Permission`**.
6. Masukkan reason code dan referensi persetujuan.
7. Selesaikan reauthentication.
8. Konfirmasi target, permission/profile, scope, dan current version.
9. Simpan.
10. Verifikasi bahwa effective access berubah sesuai policy.
11. Bila risiko mewajibkan, cabut application session melalui tindakan terpisah yang berwenang.
12. Periksa audit receipt/event `Administration.PermissionRevoked` atau `Administration.SupportGrantChanged`.

Dampak yang harus dipahami:

- perubahan assignment membawa version/invalidation dan tindakan sensitif memakai state saat ini;
- UI atau page yang sudah terbuka bukan authority;
- cached claim tidak boleh mempertahankan privileged allow setelah server mengetahui revocation;
- offline admin/Support/Audit tidak pernah diizinkan;
- offline permission pengguna yang telah kedaluwarsa tidak boleh diperpanjang tanpa online refresh;
- queued intent diperiksa ulang saat reconnect dan bukan grant lama;
- continuation, retry, resume, cancel, restore, export, dan download diotorisasi ulang;
- revoke tidak menghapus file, Audit event, atau account secara otomatis.

Jangan menyatakan propagation “langsung di semua perangkat” tanpa bukti implementasi dan target waktu yang disetujui. Exact invalidation channel, retry, dan propagation expectation perlu dikonfirmasi pada implementation/UAT.

## 8. Effective-access review

Effective access menjawab: “Apakah principal ini boleh melakukan tindakan tertentu terhadap sumber daya tertentu, dalam konteks saat ini?” Pemeriksaan role saja tidak cukup.

Formula normatif:

```text
ALLOW = AuthenticatedOrEligibleOfflineGrant
    AND AccountActive
    AND SessionOrGrantValid
    AND ActionPermission
    AND SubjectTypeAllowed
    AND ResourceOwnershipOrExplicitScope
    AND EnterprisePolicyAndClassification
    AND ResourceStateAndVersion
    AND ClientCompatibilityAndCapability
    AND ConnectivityAllows
    AND RequiredConsentAndConfirmation
    AND FreshReauthenticationWhenRequired
```

### Prosedur review

1. Cari user dan buka **`TBD_UI_LABEL: Effective Access`**.
2. Pilih action/permission yang ingin diperiksa.
3. Pilih tipe sumber daya dan referensi opaque bila dibutuhkan; jangan membuka content.
4. Periksa account dan session state.
5. Periksa role/profile/direct assignment yang menjadi sumber grant.
6. Periksa `ADMIN_SCOPE`, `SUPPORT_SCOPE`, `AUDIT_SCOPE`, ownership, atau relasi resource yang berlaku.
7. Periksa deny/hard policy, classification, SoD, dan version.
8. Periksa apakah tindakan online-only dan apakah platform/capability mendukung.
9. Periksa kebutuhan consent, confirmation, dan reauthentication.
10. Baca hasil **Allowed** atau **Denied** beserta reason code aman.
11. Jangan mengubah akses hanya untuk “membuat tes berhasil”; perbaiki assignment atau policy melalui perubahan berwenang.

Tampilan review yang direkomendasikan:

| Dimensi | Contoh hasil aman |
|---|---|
| Account | Active / Disabled |
| Session | Valid / Expired / Revoked |
| Permission | Present / Missing / Inactive |
| Source | Role / Optional profile / Direct assignment |
| Scope | In scope / Out of scope |
| Ownership | Own / Not owner / Explicit scope absent |
| Policy | Allowed / Denied by policy |
| Resource state/version | Eligible / Stale / Invalid state |
| Connectivity | Online / Online required |
| Reauthentication | Fresh / Required / Expired |
| Result | Allowed / Denied + safe reason code |

Catatan:

- read-only “effective access view” adalah projection penjelasan, bukan endpoint bypass atau token grant;
- resource identifier, menu, route, Job ID, Correlation ID, atau hasil lama tidak memberi kewenangan;
- jika status tidak dikenal atau dependency authorization gagal, hasil privileged harus deny/fail closed;
- exact UI dan contract view belum tersedia dan memerlukan controlled addendum, bukan asumsi pada manual.

## 9. Reauthentication

Reauthentication membuktikan freshness untuk tindakan tepat. Ia tidak menambah role, permission, scope, atau content access.

### Tindakan yang wajib atau dapat memerlukan reauthentication

| Tindakan | Baseline |
|---|---|
| Assign/revoke role | Wajib |
| Assign/revoke permission/profile | Wajib |
| Enable/disable eligibility principal lain | Wajib |
| Revoke session principal lain | Wajib |
| Policy security/classification/retention berdampak tinggi | Wajib |
| Broad Feature/maintenance/minimum-version/limit change | Kondisional sampai wajib menurut policy |
| Audit export berdampak tinggi | Kondisional menurut policy; permission ekspor terpisah |
| Support attachment/export berdampak tinggi | Kondisional menurut policy; scope Issue tepat |
| Future exceptional content access | Tidak tersedia saat ini; selalu wajib jika kelak disetujui formal |

### Prosedur admin

1. Periksa ringkasan exact action, target, scope, dan dampak.
2. Pilih **`TBD_UI_LABEL: Continue and Verify`**.
3. Selesaikan autentikasi pada jalur Pertamina yang disetujui.
4. Kembali ke aplikasi.
5. Periksa kembali action, target, permission, policy, version, dan scope; kondisi dapat berubah selama autentikasi.
6. Konfirmasi dan simpan hanya jika seluruh pemeriksaan masih lulus.
7. Periksa outcome dan audit receipt.

Jika reauthentication gagal atau kedaluwarsa:

- tidak boleh ada mutasi parsial;
- aplikasi menampilkan alasan aman dan langkah yang diizinkan;
- jangan meminta atau menyimpan password pengguna;
- jangan menganggap reauthentication sukses sebagai izin tindakan;
- retry/lock mengikuti Pertamina Identity dan policy yang disetujui;
- identity fresh tanpa permission tetap ditolak.

Konteks reauthentication harus singkat dan terikat pada principal, session, client, action, resource/scope, serta revisi policy/permission. Logout, disablement, revocation, perubahan assignment/policy/resource version, mismatch, atau expiry membatalkannya.

Metode, assurance level, usia maksimum, retry, dan lockout berstatus `REQUIRES_PERTAMINA_SSO_CONFIRMATION`.

## 10. Offline/session implications

### Aturan dasar

- seluruh administrasi account, session, role, permission, configuration, Support, dan Audit hanya daring;
- cached admin page, role, atau claim tidak memberikan kewenangan offline;
- reauthentication-required action menunggu autentikasi online;
- offline grant tidak memuat permission Admin, Support, Auditor, atau permission server privileged;
- session offline menggunakan absolute expiry tanpa sliding renewal;
- reconnect memeriksa ulang identity, account, session, permission, policy, capability, dan compatibility sebelum menjalankan intent tertunda.

### Dampak perubahan akses

| Kondisi | Dampak yang diharapkan |
|---|---|
| Permission/role berubah ketika pengguna online | Request berikutnya dan tindakan sensitif menggunakan state saat ini; stale page tidak berwenang |
| Pengguna sedang offline | Privileged/admin access tetap tidak tersedia; local User capability hanya selama protected grant valid |
| Offline grant kedaluwarsa | Tindakan lokal terlindungi fail closed; pengguna harus online/login kembali |
| Account dinonaktifkan atau session dicabut | Akses baru dihentikan saat sinyal diketahui; offline grant diinvalidate sesuai desain |
| Reconnect | Semua intent tertunda diotorisasi ulang; intent bukan grant lama |
| Session aktif setelah revocation | Jangan mengandalkan menu; server menolak dan notification/REST merekonsiliasi state |
| Sensitive action | Memerlukan permission/policy/state saat ini dan fresh reauthentication |

Jika perubahan urgent, ikuti policy apakah session revocation terpisah juga wajib. Jangan menjanjikan bahwa role/permission update sendiri identik dengan enterprise logout pada semua client.

## 11. Administrative vs content access

```text
Administrative authority ≠ Content access
Support authority        ≠ User-document authority
Audit authority          ≠ Document-content authority
```

Administrator dapat diberi akses ke metadata control-plane yang aman, tetapi tidak otomatis boleh:

- membuka atau melihat file pengguna;
- mengunduh result atau secured file pengguna;
- membaca OCR/extracted/translated text;
- membaca Assistant prompt, chat, atau AI document payload;
- mengambil signature/paraf/stamp asset;
- membaca clipboard, password PDF, token, atau secret;
- menelusuri raw storage, temp, cache, checkpoint, atau backup;
- menggunakan Job ID/Correlation ID/path sebagai grant;
- melakukan “view as user” atau impersonation.

### Prosedur saat investigasi membutuhkan informasi

1. Gunakan metadata aman yang memang tersedia: status, waktu, version, reason/error code, safe aggregate, dan opaque reference.
2. Jangan mencari storage atau database secara langsung.
3. Jika terkait Issue, minta pengguna mengirim informasi/lampiran secara eksplisit melalui Laporkan Masalah.
4. Support hanya membuka exact Issue/attachment dalam `SUPPORT_SCOPE` dan dengan permission terpisah.
5. Jika informasi masih tidak cukup, eskalasi melalui Incident Support Guide; jangan membuat grant content sementara.

Tidak tersedia capability break-glass content access pada baseline saat ini. Setiap capability masa depan memerlukan persetujuan Product/Security/Legal, tujuan, scope, waktu, reauthentication, evidence, dan perubahan terkendali Document 09/10. Laporan ini tidak menciptakannya.

## 12. Auditor boundary

Auditor adalah pembaca bukti Enterprise Audit dalam `AUDIT_SCOPE`, bukan administrator account atau operator data.

Auditor dapat:

- menggunakan `audit.event.list` dan `audit.event.read` dalam scope yang disetujui;
- mencari/filter field allowlisted tanpa memperluas scope;
- membaca bukti aman untuk authentication, role, permission, policy, configuration, dan privileged events;
- membuat/mengambil export hanya jika `audit.export.create/read` diberikan terpisah dan seluruh guard lulus.

Auditor tidak boleh:

- mengaktifkan/menonaktifkan account;
- assign/revoke role atau permission;
- revoke session;
- mengubah policy/configuration;
- mengedit atau menghapus Audit event;
- mengubah retention/legal hold tanpa kewenangan lifecycle terpisah yang bukan role Auditor baseline;
- memakai event reference untuk membuka file, result, secured file, Issue, atau Assistant payload;
- mengakses document content.

Administrator tidak otomatis memiliki Audit access. Jika langkah administrasi meminta “periksa audit event”, Administrator hanya memeriksa receipt/reference aman kecuali ia memiliki grant `audit.event.*` terpisah yang lolos SoD. Verifikasi independen dilakukan oleh Auditor atau reviewer berwenang.

## 13. Support boundary

Support/Superadmin adalah scoped permission profile, bukan formal role dan bukan unlimited access.

| Capability | Permission | Batas |
|---|---|---|
| Melihat antrean Issue | `support.issue.list` | Queue/unit/assignment yang disetujui |
| Melihat detail Issue | `support.issue.read` | Exact Issue dan safe diagnostics |
| Melihat attachment | `support.attachment.read` | Exact child yang sengaja dikirim reporter |
| Triage/status | `support.issue.triage` | Typed transition dan current version |
| Catatan internal | `support.note.read/create` | Exact Issue; restricted note |
| Export | `support.export.create/read` | Query/Issue terbatas; expiry dan current authorization |

Support tidak otomatis boleh:

- membuka seluruh file pengguna;
- membuka source, result job, secured file, atau storage di luar exact Issue;
- membaca OCR penuh atau Assistant conversation;
- mengambil signature asset, password, token, clipboard, atau raw diagnostic;
- melakukan account/role/permission administration;
- bekerja offline;
- pivot lintas scope menggunakan filename, Job ID, Correlation ID, Attachment ID, atau Export ID.

Support grant harus assign/revoke secara terpisah, scoped, dapat ditinjau, dan diaudit sebagai `Administration.SupportGrantChanged`. Exact queue/unit/region/assignment scope dan persistence grant masih memerlukan konfirmasi serta controlled addendum.

## 14. Audit expectations

### Event minimum

| Perubahan/tindakan | Event yang diharapkan | Outcome yang perlu terlihat |
|---|---|---|
| Role assigned | `Administration.RoleAssigned` | SUCCEEDED / FAILED / DENIED |
| Role revoked | `Administration.RoleRevoked` | SUCCEEDED / FAILED / DENIED |
| Permission assigned | `Administration.PermissionAssigned` | SUCCEEDED / FAILED / DENIED |
| Permission revoked | `Administration.PermissionRevoked` | SUCCEEDED / FAILED / DENIED |
| Support grant changed | `Administration.SupportGrantChanged` | SUCCEEDED / FAILED / DENIED |
| Account disabled/re-enabled | `Administration.AccountEligibilityChanged` | Before/after status + outcome |
| Session revoked | `Administration.SessionRevoked` dan/atau catalogued `Auth.SessionRevoked` sesuai authoritative event flow | REVOKED / FAILED / DENIED |
| Reauthentication succeeded | `Auth.ReauthenticationSucceeded` | SUCCEEDED |
| Reauthentication failed | `Auth.ReauthenticationFailed` | FAILED / DENIED |
| Privileged action denied | `Authorization.PrivilegedActionDenied` | DENIED + safe reason |
| Cross-resource access denied | `Authorization.CrossResourceScopeDenied` | DENIED |
| Policy denial | `Authorization.PolicyDenied` | DENIED |
| Reauthentication required | `Authorization.ReauthenticationRequired` | DENIED/pending fresh authentication |
| Sensitive privileged access granted bila policy membutuhkan positive evidence | `Authorization.PrivilegedActionGranted` | SUCCEEDED/ACCEPTED |

Jangan mengaudit setiap halaman, klik, atau `403` biasa. Audit denial yang sensitif/privileged, pola probe lintas resource, atau threshold penyalahgunaan sesuai katalog.

### Metadata aman minimum

- initiator/actor terverifikasi;
- target principal/resource opaque;
- action code;
- role/permission/profile/scope/version code;
- kode sebelum/sesudah yang aman;
- time, platform/channel, environment;
- outcome dan reason/error code aman;
- policy/resource version bila diperlukan;
- reauthentication result bila diwajibkan;
- Correlation ID untuk mutasi privileged;
- Job ID hanya bila memang terkait Job.

Jangan masukkan ke Audit:

- password, token, cookie, secret, raw claim, atau header;
- file, preview, thumbnail, OCR, Assistant/AI payload, atau signature asset;
- nama/path file;
- raw request/response, stack, SQL, atau storage key;
- justifikasi bebas, pesan Issue, atau catatan internal lengkap.

Gunakan reason code terkendali. Jika alasan bebas atau case reference diperlukan oleh proses bisnis, penyimpanan dan access policy-nya harus ditetapkan pada domain berwenang; jangan menyalinnya tanpa batas ke Audit.

### Verifikasi setelah perubahan

1. Periksa outcome perubahan pada layar/receipt.
2. Catat reference/correlation aman yang ditampilkan.
3. Periksa effective access pada state terbaru.
4. Jika memiliki `audit.event.read`, cari exact event dalam `AUDIT_SCOPE`.
5. Jika tidak memiliki permission Audit, minta reviewer/Auditor berwenang melakukan verifikasi.
6. Jangan meminta direct database query atau raw log sebagai pengganti Audit berwenang.

## 15. Troubleshooting

| Masalah | Pemeriksaan aman | Tindakan administrator | Jangan dilakukan |
|---|---|---|---|
| **User tidak dapat login** | Bedakan kegagalan Pertamina SSO, OTO eligibility, session, compatibility, dan policy; lihat reason code aman | Pastikan exact account; periksa OTO Active/Disabled dan session; arahkan masalah password/MFA/identity ke jalur Pertamina; gunakan correlation resmi | Reset password lokal, meminta password, membuat account bayangan, assign role untuk memperbaiki autentikasi |
| **Account Disabled** | Reason/status aman, current version, approval, status SSO resmi bila tersedia | Re-enable hanya dalam scope, dengan reauth/SoD/policy; minta user login online kembali | Mengaktifkan enterprise identity dari OTO, memulihkan seluruh grant lama otomatis |
| **Role tidak muncul** | Exact principal, assignment status/version, scope, revocation, account switch, refresh state | Buka Access terbaru; periksa assignment outcome dan version conflict; verifikasi audit receipt | Membuat role baru, mengedit role canonical, mengandalkan cache/menu |
| **Permission belum berlaku** | Assignment source, effective access, deny policy, SoD, scope/ownership, online state, session/version | Refresh current state; minta reconnect/login bila sesuai; perbaiki assignment berwenang; revoke session hanya bila policy mengharuskan | Menambah role lebih tinggi tanpa analisis, memberi wildcard, menjadikan UI authority |
| **Menu terlihat tetapi action ditolak** | UI dapat stale; periksa server reason, effective access, resource state/owner, reauth, compatibility | Jelaskan bahwa visibility bukan izin; refresh; perbaiki penyebab sebenarnya | Bypass API/application authorization, mengubah owner, mencoba ID lain |
| **Offline permission expired** | Offline grant expiry/integrity/profile dan koneksi | Minta user online dan login kembali; current permission/policy akan diperiksa ulang | Memperpanjang offline grant manual, memberi Admin/Support/Audit offline |
| **Reauthentication gagal** | Session aktif, exact action binding, perubahan permission/policy, status SSO | Pastikan action masih valid; ulangi hanya melalui jalur SSO resmi sesuai policy; eskalasi bila identity issue | Meminta password, menyimpan credential, menganggap reauth sebagai permission |
| **Account/session Revoked** | Identifikasi apakah identity, eligibility, session, atau grant yang revoked | Pertahankan deny; periksa authoritative status; hanya re-enable/relogin melalui proses resmi | Mengubah status secara tebak-tebakan, membuat session lokal baru, mengabaikan revoke |
| **Wrong resource ownership** | Owner/scope dari metadata berwenang, parent-child, exact resource | Minta pengguna memakai resource sendiri atau proses delegasi formal yang tersedia | Mengubah owner sebagai workaround, membuka file, memakai Job/Correlation ID sebagai akses |
| **Admin mendapat access denied** | Admin permission, `ADMIN_SCOPE`, self-assignment rule, SoD, current session, reauth | Minta Administrator lain yang berwenang atau approval sesuai tata kelola | Self-elevation, memakai Support/Auditor sebagai bypass |
| **Audit event tidak terlihat** | Apakah admin memiliki grant Audit terpisah, scope/time/filter/event outcome | Periksa receipt; minta Auditor/reviewer berwenang; cek event type dan safe correlation | Memberi admin Audit access permanen hanya untuk troubleshooting, query database langsung |
| **Support tidak dapat membuka attachment** | Exact Issue scope, explicit attachment relation, permission, online state, reauth policy | Pastikan reporter memang mengirim attachment dan assignment Support masih aktif | Membuka source/result/secured file yang tidak terkait atau pivot via identifier |

Untuk masalah yang tidak selesai, lanjutkan ke Document 16 Incident Support Guide. Bagian ini bukan prosedur incident response penuh.

## 16. Screenshot requirements

Jangan membuat screenshot palsu. Gunakan build kandidat rilis yang telah disetujui, account uji, data sintetis, dan permission/scope uji yang terkendali.

| ID | Placeholder | State yang harus ditangkap | Callout minimum |
|---|---|---|---|
| `ADM-ACC-001` | `[SCREENSHOT-ADMIN-ACCOUNT-SEARCH-TBD]` | Pencarian account dengan hasil scoped | Search field, scope indicator, safe result |
| `ADM-ACC-002` | `[SCREENSHOT-ADMIN-ACCOUNT-SUMMARY-TBD]` | Account summary Active | Identity reference, OTO eligibility, session summary, role |
| `ADM-ACC-003` | `[SCREENSHOT-ADMIN-ACCOUNT-DISABLE-TBD]` | Konfirmasi disable sebelum submit | Exact target, before/after, reason, impact, reauth notice |
| `ADM-ACC-004` | `[SCREENSHOT-ADMIN-SESSION-REVOKE-TBD]` | Session revoke | Session reference, target, reason, reauth |
| `ADM-ROLE-001` | `[SCREENSHOT-ADMIN-ROLE-ASSIGN-TBD]` | Assign role sebelum konfirmasi | Current role, selected role, scope, SoD warning |
| `ADM-ROLE-002` | `[SCREENSHOT-ADMIN-ROLE-DELTA-TBD]` | Permission delta | Added/retained/denied, online-only, reauth/content flag |
| `ADM-PERM-001` | `[SCREENSHOT-ADMIN-PERMISSION-ASSIGN-TBD]` | Assign permission/profile | Canonical code, source, scope, expiry/version, sensitivity |
| `ADM-PERM-002` | `[SCREENSHOT-ADMIN-PERMISSION-REVOKE-TBD]` | Revoke confirmation | Exact assignment, remaining sources, impact |
| `ADM-EFF-001` | `[SCREENSHOT-ADMIN-EFFECTIVE-ACCESS-TBD]` | Effective access denied | Permission source, scope/ownership, policy, reason code |
| `ADM-REAUTH-001` | `[SCREENSHOT-ADMIN-REAUTH-TBD]` | Reauthentication prompt before high-impact action | Exact action/target, continue/cancel; tanpa credential dalam gambar |
| `ADM-AUD-001` | `[SCREENSHOT-ADMIN-AUDIT-RECEIPT-TBD]` | Mutation receipt atau scoped audit verification | Outcome, event type/reference, correlation masked |
| `ADM-TS-001` | `[SCREENSHOT-ADMIN-ACCESS-DENIED-TBD]` | Menu visible tetapi action denied | Safe reason, refresh/review next action |

Standar screenshot:

- gunakan `TBD_UI_LABEL` sampai string UI final tersedia;
- jangan menampilkan nama pegawai nyata, employee number, email lengkap, unit sensitif, token, claim, file, atau Issue nyata;
- masking harus terlihat konsisten;
- satu tujuan per screenshot dan callout bernomor;
- sertakan alternative text;
- catat app version, platform, locale, theme, role/permission test fixture, dan tanggal capture;
- buat bukti terpisah untuk allowed dan denied state;
- screenshot UI bukan bukti otorisasi; sertakan hasil UAT berwenang;
- ambil ulang bila layout, label, permission, atau policy berubah.

Document 14 menyatakan actual UI/control/screenshots dan validated build belum tersedia. Semua placeholder tetap terbuka sampai implementation dan UAT execution selesai.

## 17. Pertamina SSO confirmation gaps

Setiap item berikut harus diberi status `REQUIRES_PERTAMINA_SSO_CONFIRMATION` pada master sampai pemilik standar Pertamina mengonfirmasi:

| ID | Gap konfirmasi |
|---|---|
| `SSO-GAP-01` | Protocol/profile, issuer, audience, redirect URI, token/session type, dan approved client |
| `SSO-GAP-02` | Stable subject dan claim allowlist untuk memetakan identity ke PrincipalId |
| `SSO-GAP-03` | Provisioning subject baru/tidak dikenal dan authoritative account source |
| `SSO-GAP-04` | Arti dan mapping enterprise status Active/Disabled/Locked/Revoked ke OTO eligibility dan session |
| `SSO-GAP-05` | Siapa yang berwenang mengaktifkan kembali OTO eligibility dan prerequisite enterprise approval |
| `SSO-GAP-06` | Apakah/how OTO menerima sinyal disable/revoke dan expected propagation/invalidation behavior |
| `SSO-GAP-07` | Logout aplikasi versus enterprise logout, session revocation, dan account/device-wide sign-out behavior |
| `SSO-GAP-08` | Reauthentication method, assurance level, freshness age, retry, lockout, dan recovery route |
| `SSO-GAP-09` | Password/MFA/account unlock/recovery support route yang harus dirujuk Admin Manual |
| `SSO-GAP-10` | Offline session/grant lifetime policy, approved trusted-time behavior, dan disable/revoke signal handling |
| `SSO-GAP-11` | Account switch behavior dan profile/device binding yang disetujui |
| `SSO-GAP-12` | Definisi, source, purpose, visibility, masking, dan retention untuk last known login/access metadata |
| `SSO-GAP-13` | MFA/step-up wording dan halaman Pertamina yang boleh ditampilkan/didokumentasikan |
| `SSO-GAP-14` | Approved escalation/contact path untuk identity issue tanpa menciptakan vendor/provider baru |

Konfirmasi SSO tidak boleh digunakan untuk menunda secure default. Sampai dikonfirmasi:

- tidak ada reset password lokal;
- status tidak dikenal diperlakukan fail closed;
- OTO tidak menyimpan atau menampilkan credential;
- admin mutation tetap membutuhkan online current authorization dan reauthentication;
- enterprise identity dan OTO eligibility tetap dipisahkan.

## 18. Risks/conflicts

| ID | Risiko/gap/konflik | Dampak | Rekomendasi terkendali |
|---|---|---|---|
| `R-15A-01` | Feature admin/session revocation masih `PLANNED`; UI dan build belum tersedia | Prosedur belum dapat divalidasi operasional | Pertahankan `TBD_UI_LABEL`; lakukan implementation dan UAT sebelum publikasi |
| `R-15A-02` | Document 09 menyatakan `FEAT-ADMIN-001..003` dan `FEAT-AUTH-007` memerlukan addendum contract Document 06 | Manual dapat menjanjikan operasi tanpa contract resmi | Buat controlled API addendum; jangan menambah route melalui manual |
| `R-15A-03` | Document 07 hanya memuat RolePermissions dan PrincipalRoles; persistence direct principal-permission dan scoped Support grant belum didefinisikan | Assign/revoke permission/profile tidak dapat diimplementasikan konsisten | Controlled database/design addendum setelah keputusan persistence; jangan invent tabel di Document 15 |
| `R-15A-04` | Active/Disabled/Revoked dapat merujuk identity, OTO eligibility, session, atau offline grant | Admin dapat mengubah objek yang salah | Tampilkan status per lapisan dan source; selesaikan `SSO-GAP-04` |
| `R-15A-05` | Re-enable OTO versus re-enable enterprise identity belum dikonfirmasi | Risiko bypass lifecycle Pertamina | Batasi OTO pada eligibility; konfirmasi SSO/authority sebelum prosedur final |
| `R-15A-06` | Admin tidak memiliki akses Enterprise Audit implisit, tetapi workflow meminta “periksa audit event” | Prosedur dapat mendorong over-grant Audit | Admin memeriksa receipt; Auditor/grant terpisah melakukan verifikasi event |
| `R-15A-07` | Reason wajib secara prosedural, sedangkan Audit melarang justifikasi bebas | Risiko data sensitif masuk Audit atau alasan hilang | Gunakan reason code; tentukan repository/case reference melalui desain terkendali |
| `R-15A-08` | Effective-access explanation view belum memiliki contract/UI | Admin dapat mengandalkan kalkulasi klien yang tidak berwenang | Sediakan server-authoritative safe projection melalui addendum; UI tetap explanation-only |
| `R-15A-09` | Exact revocation/invalidation propagation dan SLA belum ditetapkan | Admin dapat mengira akses berhenti di semua client secara instan | Konfirmasi behavior, test active/offline/reconnect clients, dan dokumentasikan tanpa klaim SLA palsu |
| `R-15A-10` | Matrix SoD, delegation, approval, expiry, dan self-service grant tepat masih memerlukan standar Pertamina | Assignment privileged dapat terlalu luas | Fail closed; gunakan approval/scope terkecil; tandai `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `R-15A-11` | Support queue/unit/region/assignment scope belum final | Label Superadmin dapat berubah menjadi akses lintas Issue | Tunda grant produksi sampai scope dan lifecycle disetujui |
| `R-15A-12` | Last known access metadata belum memiliki definisi field/sumber/retention | Risiko privasi dan interpretasi salah | Jangan tampilkan sampai purpose, access, masking, dan retention disetujui |
| `R-15A-13` | Document 13 adalah spesifikasi UAT FINAL, tetapi execution gate belum dinyatakan selesai | Belum ada bukti perilaku actual untuk admin boundary | Jalankan UAT Administrator/Auditor/Support/offline/reauth sebelum sign-off manual |
| `R-15A-14` | Document 14 adalah pre-implementation baseline dengan no validated build | Terminologi/login/help route dapat berubah | Sinkronkan istilah dan screenshot setelah UI string inventory |
| `R-15A-15` | Multiple role/profile diizinkan tetapi role tidak berhierarki | Effective access sulit dijelaskan dan rawan SoD conflict | Tampilkan semua assignment source dan delta; deny/hard policy menang |
| `R-15A-16` | Tidak ada permission exceptional cross-user content access saat ini | Permintaan investigasi dapat mendorong workaround storage/DB | Tolak; gunakan metadata/Issue; perubahan masa depan memerlukan approval Product/Security/Legal |

Tidak ditemukan konflik yang membolehkan Administrator, Support, atau Auditor membuka content pengguna secara otomatis. Jika interpretasi UI atau operasional mengarah ke akses tersebut, interpretasi itu bertentangan dengan Document 08–10 dan harus ditolak.

## 19. Suggested Document 15 text

Teks berikut dapat digunakan sebagai baseline bagian **Account, Role, Permission & Access Administration** pada master Document 15 setelah contract, UI label, SSO confirmation, dan UAT execution tersedia.

---

### Administrasi Account dan Akses

Bagian ini digunakan oleh Administrator PERTAMINA OTO yang memiliki permission dan scope delegasi yang sesuai. Seluruh tindakan administrasi dilakukan secara online. Gunakan account personal dan jangan melakukan self-assignment atau self-elevation.

Pertamina Identity mengelola autentikasi, password, MFA, dan pemulihan identity. PERTAMINA OTO mengelola kelayakan aplikasi, application session, role, permission, scope, dan effective access. Administrator OTO tidak melakukan reset password lokal.

#### Memahami status account

Jangan mencampur:

- status identity Pertamina;
- kelayakan account OTO: Active atau Disabled;
- status application session: Active, Expired, atau Revoked;
- effective access terhadap tindakan dan sumber daya tertentu.

Account Active tidak berarti semua tindakan diizinkan. Role yang sesuai tetap dibatasi oleh permission, scope, ownership, policy, resource state, connectivity, dan reauthentication.

#### Mencari dan membuka account

1. Buka **`TBD_UI_LABEL: Administration › Accounts`**.
2. Cari pengguna menggunakan identifier yang disetujui.
3. Pilih exact principal dalam scope Anda.
4. Cocokkan referensi identity aman, OTO eligibility, session summary, dan role.
5. Jangan mencari atau membuka file pengguna.

Account summary hanya menampilkan metadata aman. Password, token, raw claim, file, OCR, Assistant/AI payload, clipboard, dan signature asset tidak tersedia bagi Administrator.

#### Menonaktifkan account OTO

1. Cari user dan periksa exact target.
2. Periksa status, scope, SoD, dan current version.
3. Pilih **`TBD_UI_LABEL: Disable OTO Access`**.
4. Baca dampak terhadap session dan offline access.
5. Pilih reason code dan masukkan referensi persetujuan.
6. Selesaikan reauthentication.
7. Konfirmasi perubahan Active → Disabled.
8. Simpan satu kali dan tunggu outcome.
9. Verifikasi status, effective access, dan audit receipt.

Disable tidak menghapus source, draft, file, atau Audit event secara otomatis.

#### Mengaktifkan kembali account OTO

Re-enable hanya dilakukan jika status Pertamina Identity dan policy mengizinkan.

1. Cari account Disabled.
2. Pastikan masalah bukan password, MFA, account lock, atau enterprise identity lifecycle.
3. Periksa reason, approval, scope, dan role/permission yang masih valid.
4. Pilih **`TBD_UI_LABEL: Re-enable OTO Access`**.
5. Masukkan reason code dan referensi persetujuan.
6. Selesaikan reauthentication.
7. Konfirmasi Disabled → Active.
8. Simpan, verifikasi effective access, dan minta user login online kembali.

Detail authority dan status Pertamina mengikuti `REQUIRES_PERTAMINA_SSO_CONFIRMATION`.

#### Assign role

1. Cari user.
2. Buka **`TBD_UI_LABEL: Access`**.
3. Lihat role saat ini.
4. Pilih `User`, `Administrator`, atau `Auditor`.
5. Periksa permission yang ditambahkan, dipertahankan, atau dibatasi.
6. Isi reason code dan referensi persetujuan.
7. Periksa target, scope, SoD, dan no self-assignment.
8. Selesaikan reauthentication dan simpan.
9. Verifikasi effective access.
10. Periksa audit receipt; minta Auditor/reviewer dengan permission terpisah memverifikasi `Administration.RoleAssigned`.

Role tidak berhierarki. Administrator tidak otomatis menjadi User, Auditor, atau Support. “Superadmin” bukan role formal.

#### Revoke role

1. Pilih exact role assignment.
2. Periksa permission yang hilang dan yang masih berasal dari assignment lain.
3. Periksa session atau tindakan sensitif yang terdampak.
4. Isi reason code.
5. Selesaikan reauthentication.
6. Konfirmasi target, role, scope, dan version.
7. Simpan.
8. Verifikasi effective access dan audit receipt `Administration.RoleRevoked`.

#### Assign permission atau Support profile

1. Buka **`TBD_UI_LABEL: Access › Permissions`**.
2. Periksa permission dari role dan assignment terpisah.
3. Pilih kode canonical; jangan membuat kode atau wildcard.
4. Tentukan scope terkecil yang disetujui.
5. Periksa sensitivity, online-only, reauthentication, ownership, dan content-access flag.
6. Periksa SoD, delegation, duplicate grant, policy conflict, dan expiry/version.
7. Periksa delta effective access.
8. Isi reason code dan referensi persetujuan.
9. Selesaikan reauthentication dan simpan.
10. Verifikasi `Administration.PermissionAssigned` atau `Administration.SupportGrantChanged`.

Permission hanya memberi kemampuan mencoba tindakan. Ia tidak melewati ownership, scope, policy, atau current resource state.

#### Revoke permission

1. Pilih exact assignment dan sumbernya.
2. Jika berasal dari role, kelola role assignment melalui prosedur role.
3. Periksa assignment lain yang masih memberi permission.
4. Periksa session, offline grant, job tertunda, dan tindakan sensitif yang terdampak.
5. Isi reason code.
6. Selesaikan reauthentication.
7. Konfirmasi dan simpan.
8. Verifikasi effective access.
9. Cabut session melalui tindakan terpisah jika policy mewajibkan.
10. Periksa receipt/event `Administration.PermissionRevoked`.

#### Meninjau effective access

Effective access harus mempertimbangkan:

- identity, account, dan session;
- permission dan assignment source;
- scope dan resource ownership;
- policy, classification, state, dan version;
- platform, capability, dan connectivity;
- consent, confirmation, dan reauthentication.

Menu yang terlihat bukan bukti izin. Jika action ditolak, baca reason code aman dan periksa seluruh dimensi tersebut. Jangan mengubah owner atau memberikan role lebih tinggi sebagai jalan pintas.

#### Reauthentication

Assign/revoke role, assign/revoke permission, enable/disable account lain, revoke session lain, dan perubahan policy berdampak tinggi memerlukan reauthentication.

1. Periksa exact action dan target.
2. Lanjutkan ke autentikasi Pertamina.
3. Kembali ke aplikasi.
4. Periksa kembali scope, permission, policy, dan version.
5. Konfirmasi dan simpan.

Reauthentication tidak menambah permission. Jika gagal, tidak boleh ada mutasi parsial.

#### Session dan offline

Admin, Support, dan Audit tidak tersedia offline. Cached page atau role tidak memberi kewenangan. Setelah reconnect, identity, account, session, permission, policy, capability, dan compatibility diperiksa ulang. Intent tertunda bukan grant lama.

Untuk mencabut session lain, pilih exact session, masukkan reason code, selesaikan reauthentication, konfirmasi target, lalu verifikasi status Revoked dan audit receipt.

#### Batas content access

Administrator tidak otomatis boleh membuka atau mengunduh file user, membaca OCR/AI payload, atau mengambil signature asset. Support hanya dapat membuka exact Issue/attachment yang dikirim pengguna dan berada dalam scope assignment. Auditor hanya membaca bukti Audit yang aman dan tidak dapat mengubah account, permission, atau Audit event.

Jika investigasi memerlukan informasi, gunakan metadata aman dan Laporkan Masalah. Jangan menelusuri storage/database atau menggunakan Job ID/Correlation ID sebagai grant.

#### Audit perubahan

Role, permission, Support grant, account eligibility, session revocation, reauthentication, dan privileged denial harus menghasilkan event yang sesuai. Audit hanya berisi actor, target opaque, action, scope/version, before/after code, outcome, reason code aman, dan correlation yang diperlukan. Audit tidak menyimpan password, token, content, OCR, prompt, signature, path, atau justifikasi bebas.

Administrator memeriksa receipt perubahan. Event Enterprise Audit diperiksa oleh Auditor atau principal dengan permission Audit terpisah; role Administrator tidak memberikan Audit access otomatis.

#### Jika user tidak dapat login

1. Bedakan kegagalan Pertamina SSO dari OTO eligibility/session/permission.
2. Periksa exact account dan reason code aman.
3. Jika OTO Disabled, ikuti prosedur re-enable hanya bila diizinkan.
4. Jika session Revoked/Expired, minta user login kembali melalui jalur resmi.
5. Jika masalah password, MFA, lock, atau enterprise identity, gunakan jalur Pertamina yang disetujui.
6. Jangan reset password lokal atau memberi role tambahan untuk memperbaiki login.

---

Sebelum teks ini dimasukkan ke master Document 15, selesaikan seluruh `REQUIRES_PERTAMINA_SSO_CONFIRMATION`, controlled addendum Document 06–07, UI string inventory, screenshot aktual, dan UAT execution untuk Administrator/Auditor/Support.
