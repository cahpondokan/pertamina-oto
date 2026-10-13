# AGENT A — Incident Intake, Classification, Triage & Communication Report

**Task ID:** DOC16-AGENT-A  
**Tujuan:** Rancangan Incident Support Guide PERTAMINA OTO untuk intake, klasifikasi, triage, komunikasi, dan closure  
**Status:** Rekomendasi specialist; bukan perubahan terhadap master Document 16  
**Baseline sumber:** Document 01–15 FINAL dan `MASTER_PROJECT_SUMMARY.md` v3.8

> **Batas tata kelola:** Model `SEV-1..SEV-4`, priority Incident, target response/update, escalation route, dan ownership group dalam laporan ini adalah usulan pra-operasional. Model tersebut bukan standar resmi Pertamina. Seluruh target waktu bersifat `POLICY_CONFIGURABLE` dan memerlukan `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` sebelum digunakan sebagai komitmen layanan.

> **Pemisahan workflow:** Issue PERTAMINA OTO tetap memakai status canonical Document 15: `BARU`, `DIPERIKSA`, `PERLU_PERBAIKAN`, `BUTUH_INFORMASI`, `DUPLIKAT`, `BUKAN_BUG`, `SEDANG_DIPERBAIKI`, `READY_FOR_TEST`, `TERVERIFIKASI`, `SELESAI`. Lifecycle Incident dalam laporan ini adalah record koordinasi terpisah dan tidak mengubah enum atau transisi Issue.

## 1. Terminology

### 1.1 Definisi dan contoh

| Istilah | Definisi operasional | Contoh PERTAMINA OTO | Bukan berarti |
|---|---|---|---|
| **Issue** | Laporan, pertanyaan, atau ketidaksesuaian yang diterima dan belum tentu terbukti sebagai gangguan layanan atau defect | Seorang user melaporkan Merge PDF gagal dan mengirim screenshot opsional | Setiap Issue otomatis menjadi Incident atau Defect |
| **Incident** | Gangguan yang menyebabkan atau berpotensi menyebabkan penurunan layanan, kegagalan proses bisnis, data tidak aman, atau pelanggaran keamanan yang perlu dikoordinasikan sampai layanan stabil | Setelah deployment, banyak user tidak dapat memproses PDF dan Job menumpuk | Root cause sudah diketahui atau fix permanen sudah tersedia |
| **Defect** | Perilaku produk yang telah diverifikasi tidak sesuai requirement/acceptance pada build dan environment tertentu | Hasil Merge PDF terbukti mengabaikan urutan input pada build tertentu | Defect selalu sedang berdampak pada Production atau selalu menjadi Incident |
| **Problem** | Penyebab dasar yang belum diketahui atau risiko penyebab berulang yang memerlukan analisis lintas satu atau beberapa Incident | Timeout provider berulang pada beberapa Incident, sementara root cause belum terverifikasi | Semua Incident harus menunggu Problem selesai sebelum dapat ditutup |
| **Service Request** | Permintaan layanan normal, akses, konfigurasi, informasi, atau bantuan yang bukan gangguan layanan | Permintaan pemberian permission yang telah disetujui atau permintaan mengaktifkan Feature sesuai proses | Incident; kecuali kegagalan memenuhi layanan tersebut menimbulkan gangguan nyata |
| **Security Incident** | Incident dengan dugaan atau bukti akses tidak sah, kebocoran, penyalahgunaan, kompromi, pelanggaran confidentiality/integrity/availability, atau bypass kontrol keamanan | User dapat melihat file user lain, token muncul pada laporan, public/unapproved AI route diduga menerima content | Ticket Support biasa yang boleh memuat detail forensik terbuka |

### 1.2 Hubungan antarrecord

```text
Intake signal
├─ tetap sebagai Issue
├─ menjadi Service Request
├─ dikonfirmasi sebagai Defect
├─ ditautkan ke Incident
├─ ditautkan ke Security Incident process
└─ membentuk/menaut ke Problem bila berulang atau root cause belum jelas
```

Aturan:

- satu Issue dapat ditautkan ke nol atau satu Incident aktif yang relevan;
- beberapa Issue duplicate dapat ditautkan ke satu Incident tanpa menggabungkan data reporter;
- satu Incident dapat ditautkan ke beberapa Defect atau satu Problem;
- Incident dapat dibuat dari monitoring/security/deployment tanpa In-App Issue;
- resolved Incident tidak otomatis menutup Defect atau Problem;
- closed Defect tidak otomatis membuktikan pemulihan Production;
- Security Incident mengikuti proses security resmi yang dibatasi, bukan internal note Support biasa;
- Service Request tidak diberi severity Incident kecuali benar-benar berubah menjadi gangguan.

### 1.3 Istilah yang tidak boleh dipertukarkan

- **Severity** mengukur besarnya dampak Incident.
- **Priority** menentukan urutan dan urgensi penanganan.
- **Status** menunjukkan posisi Incident pada lifecycle.
- **Ownership** menunjukkan siapa yang bertanggung jawab mengoordinasikan dan menyelesaikan.
- **Resolution** mengembalikan kondisi layanan atau mengendalikan dampak.
- **Closure** memastikan verifikasi, komunikasi, evidence, dan follow-up lengkap.
- **RCA** adalah keputusan apakah analisis akar penyebab diperlukan dan, bila diperlukan, hasil proses terpisah tersebut.

## 2. Intake model

### 2.1 Prinsip intake ringan

Intake harus cepat, aman, dan cukup untuk menentukan apakah triage dibutuhkan. Pengguna tidak diminta memilih Incident, category, severity, priority, owner, Feature ID, platform, atau status. Beban klasifikasi berada pada Support/incident triage.

Satu model intake dipakai untuk seluruh sumber:

```text
Receive signal
→ preserve original source reference
→ capture minimum safe facts
→ screen data/security risk
→ acknowledge when a human reporter exists
→ deduplicate/link
→ assign triage owner
→ classify Issue/Incident/Defect/Problem/Request/Security
```

### 2.2 Field intake minimum

| Field | Kebutuhan saat intake | Aturan keamanan |
|---|---|---|
| Intake reference | Wajib; identifier sistem of record yang disetujui | Opaque; bukan permission |
| Received time | Wajib; UTC dan display timezone jelas | Jangan memakai waktu client sebagai satu-satunya authority |
| Source | Wajib: In-App, monitoring, admin, user, UAT/test, Audit/security, deployment, provider | Enum/source terkendali |
| Source reference | Bila tersedia: Issue, Alert, Defect, Audit event, deployment/change, provider event | Reference tidak memberi content access |
| Environment | Wajib: Development, QA/Test, Production, atau approved environment | Jangan mencampur data antar-environment |
| Symptom summary | Wajib; satu atau dua kalimat | Tanpa password, content, raw diagnostic, atau data pribadi berlebih |
| Observed impact | Wajib; pekerjaan apa yang gagal/terganggu | Bedakan fakta dari dugaan |
| Data/security flag | Wajib: none suspected / potential / confirmed / unknown | `potential`, `confirmed`, atau `unknown` memicu restricted review |
| Affected scope | Awal dapat berupa unknown, single user, group, platform, widespread | Jangan menampilkan daftar user luas pada komunikasi umum |
| Feature/platform/version | Bila diketahui | Feature ID canonical; versi tidak diterjemahkan |
| Safe identifiers | Error Code, Job ID, Correlation ID bila tersedia | Tidak memberi scope; jangan meminta user mencari jika aplikasi tidak menampilkan |
| Workaround/recovery | Bila diketahui dan telah divalidasi aman | Jangan menyebarkan workaround spekulatif |
| Recent change | Bila relevan: release/config/policy/provider reference | Jangan menyimpulkan sebab hanya karena berdekatan waktu |
| Triage owner | Wajib setelah acknowledgement | Fungsi/owner reference, bukan kontak palsu |
| Classification/SEV/Priority | Provisional selama triage | Harus mempunyai reason dan dapat direvisi |
| Linked records | Issue/Defect/Problem/Security case/known risk | Status tetap independen |

### 2.3 Data yang dilarang pada intake umum

Jangan menyalin atau meminta:

- password/passphrase, token, cookie, secret, key, connection string;
- byte atau isi dokumen, OCR text, preview/thumbnail, clipboard, signature asset;
- Assistant prompt/chat, AI provider request/response, atau model payload;
- raw stack, SQL, request/response body, packet, storage path/key, atau full log dump;
- screenshot tanpa preview dan explicit send;
- nama/path file asli bila tidak benar-benar diperlukan dan diizinkan;
- detail forensik Security Incident pada record Support umum;
- daftar pengguna terdampak yang tidak dimasking.

### 2.4 Sumber intake

| Sumber | Minimum yang diterima | Perlakuan triage |
|---|---|---|
| **In-App Issue Report** | Pesan singkat, screenshot opsional yang dipreview, safe diagnostic context otomatis | Pertahankan Issue; Support menentukan category/priority/status dan apakah perlu Incident |
| **Monitoring alert** | Alert type, environment, component/capability, timestamp/freshness, threshold source, safe metric summary | Validasi alert tidak stale; nilai dampak bisnis sebelum menetapkan severity |
| **Admin observation** | Symptom, environment, safe check, version/config reference, impact yang terlihat | Jangan mengubah Production untuk “menguji”; tautkan change/health evidence |
| **User report melalui kanal resmi lain** | Reporter reference, waktu, symptom, platform, dampak | Catat pada system of record resmi; jangan meminta form teknis panjang atau memindahkan data ke kanal tidak resmi |
| **UAT/test** | UAT ID, Defect ID bila ada, build/digest, environment, expected/actual, sanitized evidence | Tetap Defect kecuali berdampak pada shared/live service atau memerlukan koordinasi Incident |
| **Audit/security event** | Event type, outcome, environment, safe actor/resource reference, correlation, reason code | Restricted security review; jangan menyalin payload atau memperluas scope Support |
| **Deployment issue** | Change/release reference, environment, start time, affected version/capability, verification result | Nilai hubungan waktu dan bukti; jangan langsung menyimpulkan deployment sebagai root cause |
| **Provider failure** | Approved provider profile code, health/circuit state, affected Feature/route, safe error, time | Pertahankan Local-First bila valid; jangan memakai public/unapproved fallback |

### 2.5 In-App Issue Reporting sebagai intake utama pengguna

Alur yang dipertahankan:

```text
Bantuan/error
→ Laporkan Masalah
→ pesan singkat
→ screenshot opsional
→ preview/markup/remove
→ explicit Kirim
→ user-visible status sederhana
```

User tidak diwajibkan memilih severity, priority, category, owner, atau Incident status. Safe context seperti Feature ID, app version, platform, Error Code, Job ID, dan Correlation ID ditambahkan otomatis bila tersedia dan diizinkan. Isi dokumen dan secret tidak ikut otomatis.

## 3. Issue-to-incident mapping

### 3.1 Keputusan mapping

| Kondisi hasil triage | Perlakuan |
|---|---|
| Satu laporan belum terverifikasi dan dampak terbatas | Tetap Issue; minta informasi atau lakukan pemeriksaan dalam scope |
| Pertanyaan/cara penggunaan sesuai desain | Issue dapat menjadi `BUKAN_BUG` atau Service Request sesuai workflow resmi; tidak menjadi Incident |
| Laporan sama sudah ada | Tandai Issue `DUPLIKAT` sesuai transisi yang diizinkan dan tautkan ke Issue/Incident induk tanpa membuka data reporter lain |
| Perilaku produk terbukti melanggar requirement pada build tertentu | Buat/tautkan Defect; Incident hanya jika ada service impact yang perlu koordinasi |
| Banyak Issue menunjukkan gangguan layanan yang sama | Buat satu Incident; tautkan Issue terkait; status Issue tetap dikelola sendiri |
| Monitoring/deployment/provider menunjukkan dampak aktual | Buat Incident walau belum ada Issue; Issue berikutnya ditautkan bila relevan |
| Dampak berulang dengan penyebab belum pasti | Tautkan/buat Problem setelah Incident dikendalikan; jangan menunda restoration |
| Permintaan akses/config normal | Service Request; bukan Incident |
| Dugaan akses lintas user, content leak, token/secret exposure, public AI route, atau bypass security | Security Incident path; batasi akses dan komunikasi |

### 3.2 Gate “buat Incident atau tidak”

Buat Incident bila sekurangnya satu kondisi berikut tervalidasi atau memiliki indikasi kredibel yang memerlukan koordinasi segera:

- layanan/capability penting tidak tersedia atau sangat terdegradasi;
- dampak meluas ke beberapa user, unit, platform, atau environment;
- data corruption, silent loss, false success, atau duplicate destructive effect;
- source/target safety tidak dapat dipastikan;
- unauthorized cross-user/cross-scope access;
- document/OCR/signature/secret/restricted screenshot diduga bocor;
- mandatory Audit tidak durable atau integrity/hold terancam;
- queue/provider/storage/database/Worker/API/Issue service gagal pulih;
- perubahan privileged/configuration/export terjadi tanpa authority;
- monitoring kritis unavailable/stale dan dampak tidak dapat dipastikan;
- workaround atau safe recovery tidak jelas dan risiko meningkat.

Jangan membuat Incident hanya karena:

- user meminta Feature baru;
- kesalahan penggunaan yang sudah mempunyai panduan dan tidak berdampak layanan;
- satu defect minor di environment test tanpa shared-service impact;
- permintaan akses yang normal;
- alert singkat yang terbukti false positive dan tidak ada dampak;
- Issue berstatus `BUTUH_INFORMASI`, `DUPLIKAT`, atau `BUKAN_BUG` tanpa kondisi Incident lain.

### 3.3 Sinkronisasi status tanpa mengubah workflow Issue

Incident status tidak disalin otomatis menjadi Issue status. Support memakai transisi Issue yang sudah ada:

```text
BARU → DIPERIKSA → PERLU_PERBAIKAN/BUTUH_INFORMASI/...
→ SEDANG_DIPERBAIKI → READY_FOR_TEST → TERVERIFIKASI → SELESAI
```

Proyeksi user tetap sederhana: **Dikirim**, **Sedang diperiksa**, **Sedang ditindaklanjuti**, atau **Selesai**. Issue hanya menjadi `SELESAI` setelah kondisi Issue terpenuhi, bukan semata karena Incident `RESOLVED`. Exact transition/reopen Issue tetap `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

## 4. Classification

Classification menggunakan beberapa dimensi kecil. Jangan memaksa semua informasi menjadi satu category.

### 4.1 Kelas gangguan utama

| Kelas | Cakupan | Contoh |
|---|---|---|
| `AVAILABILITY` | Layanan/capability tidak tersedia | API atau Issue service tidak dapat digunakan |
| `FUNCTIONAL_CORRECTNESS` | Hasil atau perilaku tidak sesuai requirement | Merge menghasilkan urutan salah atau status Completed palsu |
| `DATA_INTEGRITY` | Corruption, loss, duplicate effect, salah target, atau hasil tidak valid | Output rusak dipromosikan sebagai final |
| `SECURITY_PRIVACY` | Akses tidak sah, content/secret exposure, bypass, atau suspicious activity | User melihat secured file user lain |
| `PERFORMANCE_CAPACITY` | Latency/backlog/resource menghambat pekerjaan | Queue tidak drain atau storage mendekati batas |
| `IDENTITY_ACCESS` | Login/session/permission/scope/reauthentication terganggu | Banyak user valid ditolak setelah policy change |
| `INTEGRATION_PROVIDER` | Dependency resmi gagal atau incompatible | Provider internal unavailable pada Feature yang bergantung kepadanya |
| `DEPLOYMENT_COMPATIBILITY` | Release/config/version contract menyebabkan gangguan | Client supported diblokir setelah minimum-version change |
| `AUDIT_COMPLIANCE` | Audit intent/integrity/retention/hold atau evidence wajib terganggu | Privileged mutation berhasil tetapi Audit intent tidak durable |
| `SUPPORT_WORKFLOW` | Intake/status/attachment/Support Console terganggu | Issue submission berulang gagal |

Satu Incident memiliki satu primary class dan dapat memiliki secondary flags. `SECURITY_PRIVACY` atau `DATA_INTEGRITY` tidak boleh disembunyikan sebagai `FUNCTIONAL_CORRECTNESS` biasa.

### 4.2 Dimensi klasifikasi tambahan

| Dimensi | Nilai awal yang disarankan |
|---|---|
| Environment | Development / QA-Test / Production / Unknown |
| Scope | Single user / Multiple users / Unit or group / Platform / Widespread / Unknown |
| Capability | Account/SSO; core processing; Job/Activity; Secured Files; Assistant/AI; Support; Audit; configuration; dependency |
| Processing mode | Local / Server / Hybrid path / Unknown |
| Data impact | None known / Potential / Confirmed / Unknown |
| Security impact | None known / Potential / Confirmed / Unknown |
| Recoverability | Safe retry / Workaround / Restore required / No known recovery / Unknown |
| Trend | Stable / Improving / Worsening / Intermittent / Unknown |
| Change relation | No recent change known / Possible / Confirmed by evidence / Unknown |

Classification bersifat provisional selama `TRIAGE` dan diperbarui hanya dengan reason/evidence. “Unknown” lebih aman daripada menebak “none”.

## 5. Severity model

### 5.1 Prinsip

Severity mengukur dampak terburuk yang telah terkonfirmasi atau secara kredibel sedang berlangsung. Severity tidak diturunkan hanya karena tim sibuk, fix sulit, reporter sedikit, atau target waktu belum disetujui.

Model berikut adalah usulan, bukan standar resmi Pertamina:

| Level | Kriteria dampak | Contoh PERTAMINA OTO | Workaround | Target response/update |
|---|---|---|---|---|
| **SEV-1** | Gangguan sangat kritis: Production luas tidak dapat digunakan; confirmed/credible cross-user unauthorized access; significant data loss/corruption; public/unapproved AI content route; mandatory security/Audit control gagal dengan risiko aktif | Banyak user menerima file user lain; hasil destruktif salah dipromosikan secara luas; layanan inti Production total unavailable tanpa jalur aman | Tidak ada atau tidak aman | `POLICY_CONFIGURABLE`; `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| **SEV-2** | Dampak tinggi: capability kritis atau kelompok besar terganggu; data/security risk serius tetapi terkendali; backlog luas; workaround terbatas dan membebani | Server processing luas gagal tetapi local subset masih aman; secured access banyak user unavailable; queue tidak drain | Ada tetapi terbatas/tidak cocok untuk semua | `POLICY_CONFIGURABLE`; `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| **SEV-3** | Dampak sedang/terbatas: satu Feature, platform, unit, atau beberapa user; tidak ada confirmed data/security breach; workaround aman tersedia | Satu converter gagal pada build/platform tertentu dan user dapat memakai workflow lain yang disetujui | Ada dan tervalidasi | `POLICY_CONFIGURABLE`; `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| **SEV-4** | Dampak rendah: gangguan kecil/non-kritis yang tidak menghalangi hasil bisnis utama dan tidak mempunyai risiko data/security | Label/status membingungkan tetapi proses dan hasil tetap benar; minor reporting defect | Mudah atau tidak diperlukan | `POLICY_CONFIGURABLE`; `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |

### 5.2 Aturan penetapan

1. Tetapkan severity provisional saat acknowledgement/triage awal.
2. Gunakan fakta dampak, bukan nama komponen. Database alert belum tentu SEV-1 bila tidak ada dampak; satu cross-user disclosure dapat menjadi SEV-1 walau hanya satu reporter.
3. Jika data/security impact `Potential`, `Confirmed`, atau `Unknown` dengan indikasi kredibel, libatkan Security reviewer dan batasi distribusi.
4. Local-First memengaruhi dampak: kegagalan server tidak otomatis mematikan Feature lokal yang masih valid.
5. Workaround hanya menurunkan dampak bila aman, tersedia, teruji, dan tidak mengurangi security/privacy.
6. Reassess ketika scope, data risk, workaround, atau recovery berubah.
7. Catat reason setiap perubahan severity.
8. Jangan memetakan otomatis ke severity Defect UAT `BLOCKER/CRITICAL/MAJOR/MINOR`.

### 5.3 Trigger review minimum

SEV-1/SEV-2 review wajib dipertimbangkan ketika terdapat:

- unauthorized privileged/cross-user access;
- content, OCR, signature, token, secret, atau restricted screenshot exposure;
- corruption, silent loss, false success, wrong-account result, atau duplicate destructive effect;
- mandatory Audit integrity/durability failure;
- outage luas pada critical business flow;
- safe recovery tidak diketahui;
- worsening impact atau workaround tidak aman.

Authority deklarasi major/security incident dan exact threshold tetap `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

## 6. Priority model

Priority menentukan urutan penanganan berdasarkan severity **dan** konteks bisnis. Priority tidak mengubah fakta dampak.

### 6.1 Usulan level

Gunakan namespace Incident agar tidak tertukar dengan Defect priority `DEF-P0..DEF-P3` atau Issue priority catalog yang belum final.

| Priority | Arti urutan | Contoh alasan |
|---|---|---|
| `INC-P1` | Penanganan paling mendesak dan terkoordinasi | Active SEV-1; security/data exposure ongoing; critical deadline tanpa workaround |
| `INC-P2` | Tinggi; segera setelah `INC-P1` dan dapat berjalan paralel bila owner tersedia | SEV-2, scope membesar, workaround terbatas, repeat occurrence |
| `INC-P3` | Normal; dampak terbatas dan workaround aman | SEV-3 stabil pada satu Feature/platform |
| `INC-P4` | Rendah/terencana | SEV-4 atau tindak lanjut non-urgent setelah layanan stabil |

Nama/angka priority ini adalah usulan: `POLICY_CONFIGURABLE` dan `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### 6.2 Faktor priority

Nilai secara eksplisit:

- severity saat ini;
- urgency dan business deadline yang dapat dibuktikan;
- jumlah/scope user dan tren pertumbuhan;
- keberadaan, keamanan, dan beban workaround;
- critical business process yang terdampak;
- security/privacy/data risk;
- repeat occurrence dan kemungkinan meluas;
- dependency/deployment window yang relevan;
- kebutuhan coordination lintas owner;
- kewajiban komunikasi atau review resmi yang telah dikonfirmasi.

### 6.3 Aturan

- Priority dapat lebih tinggi daripada perkiraan berdasarkan severity bila deadline/risiko eskalasi nyata.
- Priority lebih rendah tidak menurunkan severity.
- Perubahan priority memerlukan reason dan owner.
- Issue priority, Defect priority, dan Incident priority tetap terpisah.
- Jangan memakai priority untuk menghindari deklarasi severity tinggi.
- Target response/resolution tidak diisi dengan angka sampai policy resmi tersedia.

## 7. Impact assessment

### 7.1 Pertanyaan impact

| Area | Pertanyaan |
|---|---|
| Business outcome | Pekerjaan apa yang tidak dapat diselesaikan? Apakah hasil dapat dipercaya? |
| User scope | Satu user, beberapa user, unit/group, platform, atau luas? Apakah scope bertambah? |
| Environment | Production, QA/Test, Development, atau belum diketahui? |
| Feature/flow | Feature ID apa? File-First, Tool-First, Assistant-First, atau semua entry? |
| Platform/version | Desktop, Web, Phone, Tablet; app/client/contract/capability version apa? |
| Availability | Tidak tersedia, terdegradasi, intermittent, atau hanya notification yang putus? |
| Data integrity | Adakah corruption, loss, wrong target/account, duplicate effect, invalid output, atau false success? |
| Security/privacy | Adakah unauthorized access, disclosure, public provider, secret/content exposure, atau suspicious activity? |
| Local-First | Apakah local-capable work tetap tersedia? Apakah server route dipaksakan atau transfer tak dipilih? |
| Workaround | Apakah aman, diuji, accessible, dan tidak melanggar policy? |
| Recovery | Retry/resume/restore/rollback mana yang aman dan sudah diverifikasi? |
| Duration/trend | Sejak kapan, stabil atau memburuk, telemetry segar atau stale? |
| Recent change | Deployment/config/policy/permission/provider change apa yang berdekatan dan sudah terbukti relevan? |
| Audit/evidence | Apakah mandatory Audit durable? Evidence aman apa yang tersedia? |

### 7.2 Impact statement

Gunakan format singkat:

> Sejak **[waktu + zona]**, pada **[environment/platform/version]**, **[scope terdampak]** mengalami **[symptom terverifikasi]** pada **[Feature/capability]** sehingga **[business outcome]**. Status data/security: **[none known/potential/confirmed/unknown]**. Workaround: **[validated workaround/tidak tersedia]**. Tren: **[stabil/membaik/memburuk/unknown]**.

Pisahkan fakta, hipotesis, dan unknown:

- **Fakta:** hasil pemeriksaan/evidence yang telah diverifikasi.
- **Hipotesis:** kemungkinan penyebab yang belum terbukti.
- **Unknown:** informasi yang belum tersedia dan tindakan untuk memperolehnya.

### 7.3 Indikator dampak tinggi

- source atau hasil lama tidak aman;
- output invalid digunakan user karena false success;
- banyak Job berulang/duplicate akibat retry;
- akses lintas account/scope;
- content/secret masuk Issue, Audit, log, provider, atau export;
- Audit wajib gagal atau tidak dapat dipercaya;
- recovery membutuhkan direct database/storage edit;
- monitoring critical path stale/unknown sehingga dampak tidak dapat diukur;
- workaround memerlukan bypass permission, public AI, atau pemindahan content tidak resmi.

## 8. Status lifecycle

### 8.1 Status Incident

Gunakan hanya status berikut:

```text
DETECTED
ACKNOWLEDGED
TRIAGE
IN_PROGRESS
WAITING_INFORMATION
MONITORING
RESOLVED
CLOSED
REOPENED
```

### 8.2 Definisi, entry, dan exit

| Status | Arti | Entry minimum | Exit/next action |
|---|---|---|---|
| `DETECTED` | Signal diterima tetapi belum diakui/ditetapkan owner | Source reference dan waktu tercatat | Assign triage owner; validasi minimum |
| `ACKNOWLEDGED` | Penerimaan diakui dan owner triage sementara ditetapkan | Acknowledgement dikirim bila reporter manusia; security screen awal | Masuk `TRIAGE` |
| `TRIAGE` | Validity, impact, classification, severity, priority, owner, dan link dinilai | Intake minimum tersedia | `IN_PROGRESS`, `WAITING_INFORMATION`, atau tutup sebagai non-Incident melalui proses record yang tepat |
| `IN_PROGRESS` | Investigation, containment, workaround, repair, atau recovery aktif | Incident owner dan next action jelas | `WAITING_INFORMATION`, `MONITORING`, atau `RESOLVED` setelah verifikasi |
| `WAITING_INFORMATION` | Informasi yang benar-benar diperlukan belum tersedia | Permintaan spesifik, owner respons, dan dampak menunggu tercatat | Kembali ke `TRIAGE`/`IN_PROGRESS`; severe Incident tetap dimonitor dan ditangani aman |
| `MONITORING` | Perubahan/workaround/recovery telah diterapkan dan stabilitas sedang diverifikasi | Recovery verification awal lulus; monitoring signal tersedia | `RESOLVED` bila stabil sesuai policy, atau `IN_PROGRESS` bila gejala kembali |
| `RESOLVED` | Layanan/dampak telah dipulihkan atau dikendalikan dan resolution telah diverifikasi | Resolution evidence, affected scope check, user update siap | Lengkapi closure checklist lalu `CLOSED`; kembali `IN_PROGRESS` bila verifikasi gagal |
| `CLOSED` | Closure criteria lengkap dan governance selesai | User/affected communication, linked records, evidence, risk/RCA/audit decision lengkap | `REOPENED` bila reopen criteria terpenuhi |
| `REOPENED` | Gejala/dampak relevan kembali atau resolution terbukti tidak efektif | Reason, evidence, reassessed impact/SEV/Priority, owner | `TRIAGE` atau `IN_PROGRESS` |

### 8.3 Alur sederhana

```text
DETECTED → ACKNOWLEDGED → TRIAGE → IN_PROGRESS
                              ↘ WAITING_INFORMATION ↗
IN_PROGRESS → MONITORING → RESOLVED → CLOSED
      ↑              ↓           ↓        ↓
      └──────────────┴───────────┴── REOPENED
```

Aturan:

- `WAITING_INFORMATION` tidak digunakan untuk menyembunyikan kurangnya owner atau menghentikan mitigasi pada Incident berat;
- `MONITORING` bukan `RESOLVED`;
- `RESOLVED` bukan `CLOSED`;
- status berubah dengan actor, time, reason, dan current version;
- version conflict di-refresh dan dinilai ulang, bukan dioverwrite;
- status Incident tidak mengubah Issue status otomatis;
- target lama setiap status adalah `POLICY_CONFIGURABLE`.

## 9. Ownership

Ownership di bawah adalah fungsi tanggung jawab, bukan role baru, struktur organisasi, permission, atau contact list.

| Fungsi | Tanggung jawab | Batas |
|---|---|---|
| **Intake/Triage Owner** | Memastikan intake valid, acknowledgement, dedupe, klasifikasi, severity/priority provisional, dan assignment | Tidak membuka content di luar Support permission |
| **Incident Owner/Coordinator** | Satu pihak accountable atas status, decision log, koordinasi, next update, dan closure | Tidak otomatis mendapat Admin/Support/Audit/content permission |
| **Technical Resolution Owner** | Melakukan diagnosis/perbaikan pada komponen yang berwenang | Menggunakan environment dan akses resmi; no direct unsafe action |
| **Support Owner** | Mengelola linked Issue, reporter update, request information, dan user-visible status | Hanya exact Issue/attachment dalam `SUPPORT_SCOPE` |
| **Administrator** | Melakukan safe control-plane checks/change bila berwenang | Tidak otomatis incident owner atau content reader |
| **Security Reviewer** | Menilai data/security impact, containment, restricted evidence, dan security communication | Mengikuti proses Security Incident resmi; detail dibatasi |
| **Communications Owner** | Menjaga message konsisten untuk audience yang disetujui | Tidak membuat SLA, root cause, atau scope yang belum terverifikasi |
| **Developer/Engineering Owner** | Menilai defect, build/change, fix, test, dan technical recovery | Tidak menerima raw user content tanpa authority/purpose |
| **Project/Product/Business Decision Owner** | Menilai business impact, workaround acceptance, known risk, dan release decision | Nama/authority resmi harus dikonfirmasi |
| **Auditor/Independent Reviewer** | Memeriksa evidence/Audit sesuai grant dan scope | Read-only; bukan Incident mutator kecuali kewenangan terpisah |

### 9.1 Aturan assignment

1. Setiap Incident memiliki satu Incident Owner aktif.
2. Technical, Support, Security, dan Communications owner dapat berbeda.
3. Handoff tidak menghapus accountability sebelum owner baru mengakui.
4. Handoff memuat status, impact, SEV/Priority, tindakan selesai/berjalan, risk, next decision/update, dan safe references.
5. Ownership mengikuti environment/capability dan jalur resmi yang dikonfirmasi.
6. Support/Superadmin tidak memperoleh content/Admin/Audit access tambahan karena menjadi owner komunikasi.
7. Job ID, Correlation ID, alert, atau Issue assignment bukan authorization grant.
8. On-call, L1/L2/L3 mapping, nama tim, dan contact route tetap `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

## 10. Triage checklist

Gunakan checklist ini pada `TRIAGE`. Jawaban dapat `YES`, `NO`, atau `UNKNOWN`; `UNKNOWN` tidak dianggap aman secara otomatis.

### Validity dan konteks

- [ ] Apakah signal valid, dapat dipahami, dan bukan test/false positive yang sudah dijelaskan?
- [ ] Apa source dan source reference-nya?
- [ ] Environment mana yang terdampak?
- [ ] Kapan mulai terlihat dan apakah waktu/telemetry masih segar?
- [ ] Feature ID/capability/business flow apa yang terdampak?
- [ ] Platform, app version, ContractVersion, dan CapabilityVersion apa yang relevan?
- [ ] Apakah satu user, beberapa user, group/unit, platform, atau widespread?
- [ ] Apakah dampak meningkat, stabil, intermittent, atau membaik?

### User dan pekerjaan

- [ ] Pekerjaan bisnis apa yang gagal atau tidak dapat dipercaya?
- [ ] Apakah user masih dapat memakai local-capable route yang valid?
- [ ] Apakah source, draft, target lama, dan result tetap aman?
- [ ] Adakah workaround yang aman, diuji, dan sesuai permission/policy?
- [ ] Apakah reporter perlu informasi atau tindakan segera?

### Data dan keamanan

- [ ] Adakah data corruption, silent loss, false success, wrong account/target, atau duplicate destructive effect?
- [ ] Adakah cross-user/cross-scope access atau privileged action tanpa authority?
- [ ] Adakah document/OCR/signature/secret/token/screenshot/Assistant/AI payload exposure?
- [ ] Adakah public/unapproved provider route atau unexpected upload?
- [ ] Apakah mandatory Audit durable dan integrity/hold tetap aman?
- [ ] Apakah Security reviewer dan restricted handling diperlukan?

### Reproduction dan evidence

- [ ] Dapatkah symptom direproduksi dengan data sintetis/aman?
- [ ] Expected dan actual behavior jelas?
- [ ] Error Code tersedia?
- [ ] Job ID tersedia dan berada dalam scope?
- [ ] Correlation ID tersedia dan berada dalam scope?
- [ ] Ada safe screenshot, alert, event, test, atau version evidence?
- [ ] Adakah recent deployment/config/policy/permission/provider change?
- [ ] Hubungan change dengan Incident sudah terbukti atau masih hipotesis?

### Dedupe, classification, dan ownership

- [ ] Adakah Issue/Alert/Defect/Incident yang sama dalam scope?
- [ ] Apakah record ini Issue, Incident, Defect, Problem, Service Request, atau Security Incident?
- [ ] Primary class dan secondary flags sudah dipilih?
- [ ] Severity provisional dan reason sudah dicatat?
- [ ] Priority provisional dan reason sudah dicatat?
- [ ] Incident Owner dan Technical/Support/Security owner sudah ditentukan?
- [ ] Next action, blocker, dan communication checkpoint sudah jelas?
- [ ] Linked Issue/Defect/Problem/change/known risk sudah dicatat?

### Keputusan keluar triage

- [ ] Masuk `IN_PROGRESS`, `WAITING_INFORMATION`, atau dikembalikan ke workflow non-Incident?
- [ ] Acknowledgement/request information sudah dikirim melalui kanal resmi?
- [ ] Workaround/containment yang dikomunikasikan telah divalidasi aman?
- [ ] Detail sensitif dibatasi dari audience umum?

## 11. Communication matrix

### 11.1 Prinsip komunikasi

- jelaskan apa yang diketahui, dampak, tindakan aman, dan apa yang berikutnya;
- bedakan fakta dari hipotesis;
- tidak menyalahkan user, tim, atau provider;
- tidak menjanjikan waktu yang belum disetujui;
- tidak menyatakan “selesai” sebelum resolution diverifikasi;
- tidak membocorkan daftar user, content, security detail, raw diagnostic, atau internal topology;
- gunakan satu Incident reference resmi dan tautkan Issue tanpa membagikan data reporter lain;
- koreksi informasi sebelumnya secara jelas bila fakta berubah;
- cadence, channel, approval, dan recipient list bersifat `POLICY_CONFIGURABLE`.

### 11.2 Audience dan isi

| Audience | Tujuan komunikasi | Isi minimum | Yang dilarang/tidak perlu |
|---|---|---|---|
| **User pelapor** | Mengakui laporan dan memberi next action/status | Issue reference, symptom yang dipahami, tindakan aman, request spesifik, status sederhana | Severity internal, user lain, raw root cause, data internal, janji waktu palsu |
| **Affected users** | Mengurangi dampak dan mencegah tindakan berisiko | Capability/flow terdampak, waktu mulai diketahui, workaround tervalidasi, apa yang tidak boleh dilakukan, checkpoint update | Reporter identity, security exploit detail, unverified cause |
| **Support internal** | Menyamakan triage, linked Issue, dan pesan | Incident reference, scope, SEV/Priority provisional, safe evidence, script/template, owner, next action | Content tidak perlu, attachment lintas Issue, permission baru |
| **Administrator** | Meminta safe health/config/account checks atau control-plane action | Environment, affected capability, safe metric/status, exact authorized action, change reference | File/OCR/Assistant payload, direct DB/storage instructions |
| **Developer** | Mendukung reproduction, fix, dan validation | Requirement/Feature, build/digest, environment, expected/actual, safe reproduction, Error/Job/Correlation reference, sanitized evidence | Password/token/raw production content, broad storage access |
| **Security reviewer** | Menilai exposure, containment, reporting, dan restricted evidence | Confirmed/potential data types, actor/resource scope aman, control failure, containment state, Audit/security references | Distribusi luas; detail forensik pada Issue/note umum |
| **Project/Product Owner** | Membuat keputusan bisnis/risk/release | Business impact, affected scope/trend, SEV/Priority, workaround, known risk, decision needed | Raw diagnostics, unnecessary personal data, unverified blame |

### 11.3 Communication checkpoint

Update dikirim ketika:

- Incident diakui;
- impact/severity berubah material;
- workaround tersedia/berubah/ditarik;
- resolution tertunda melebihi expectation policy;
- containment atau recovery diterapkan;
- monitoring membuktikan stabil/tidak stabil;
- Incident resolved, closed, atau reopened;
- security communication authority meminta holding statement.

Exact cadence dan response target: `POLICY_CONFIGURABLE` dan `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

## 12. Communication templates

Gunakan placeholder hanya dengan data aman yang telah diverifikasi. Jangan memasukkan content, secret, raw diagnostic, daftar user, atau nama individu yang tidak perlu.

### 12.1 Acknowledgement

**Subjek:** Laporan diterima — `[Issue/Incident Reference]`

> Kami telah menerima laporan mengenai **[ringkasan gejala]** pada **[Feature/capability]**. Tim sedang memeriksa dampak dan kondisi terkait. Untuk sementara, **[tindakan aman bila ada / tidak ada tindakan tambahan]**. Kami akan memberikan pembaruan melalui kanal resmi setelah tahap pemeriksaan berikutnya. Jangan mengirim password, token, atau isi dokumen.

### 12.2 Request information

**Subjek:** Informasi tambahan diperlukan — `[Reference]`

> Agar pemeriksaan dapat dilanjutkan, mohon kirimkan **[informasi spesifik dan minimum]**, misalnya **[platform/app version/waktu kejadian/Error Code yang terlihat]**. Jangan mengirim password, token, isi dokumen, OCR text, atau data sensitif. Jika screenshot diperlukan, periksa preview dan hapus informasi yang tidak relevan sebelum mengirim.

### 12.3 Investigation update

**Subjek:** Pembaruan pemeriksaan — `[Incident Reference]`

> Pemeriksaan masih berlangsung. Saat ini kami telah mengonfirmasi **[fakta terverifikasi]**. Dampak yang diketahui adalah **[scope dan business impact]**. **[Penyebab masih diteliti / hipotesis belum dikonfirmasi]**. Tindakan yang sedang dilakukan: **[safe action]**. Gunakan **[workaround tervalidasi]** bila sesuai; hindari **[tindakan berisiko]**. Pembaruan berikutnya mengikuti checkpoint komunikasi yang berlaku.

### 12.4 Workaround

**Subjek:** Workaround sementara tersedia — `[Incident Reference]`

> Sambil menunggu pemulihan penuh, pengguna yang terdampak dapat **[langkah workaround yang telah diuji]**. Workaround berlaku untuk **[platform/version/scope]** dan tidak mengubah file asli secara diam-diam. Jangan gunakan workaround di luar kondisi tersebut atau melewati permission/policy. Jika hasil tidak sesuai, hentikan dan laporkan melalui kanal resmi.

### 12.5 Delayed resolution

**Subjek:** Pemulihan memerlukan pemeriksaan lanjutan — `[Incident Reference]`

> Pemulihan belum dapat dinyatakan selesai karena **[alasan aman: verifikasi belum lulus/dependency belum stabil/informasi masih diperlukan]**. Dampak saat ini: **[impact terbaru]**. Tindakan aman yang tetap berlaku: **[workaround/containment]**. Kami tidak akan menyatakan selesai sebelum hasil diverifikasi. Pembaruan berikutnya akan diberikan melalui kanal resmi sesuai checkpoint yang berlaku.

### 12.6 Resolved

**Subjek:** Gangguan telah dipulihkan — `[Incident Reference]`

> Kondisi **[symptom/capability]** telah dipulihkan pada **[environment/platform/scope]** dan verifikasi awal telah lulus. Pengguna dapat **[langkah aman untuk melanjutkan]**. Kami masih melengkapi monitoring, linked Issue, dan tindak lanjut sebelum Incident ditutup. Jika gejala kembali, laporkan dengan reference ini dan waktu kejadian terbaru.

### 12.7 Closed

**Subjek:** Incident ditutup — `[Incident Reference]`

> Incident telah ditutup setelah resolution diverifikasi, komunikasi pengguna diperbarui, evidence dan linked record dilengkapi, serta risiko/tindak lanjut dicatat. Ringkasan dampak: **[safe summary]**. Resolution: **[safe resolution summary]**. Tindak lanjut yang tetap terbuka, bila ada: **[Problem/Defect/known risk reference]**. Penutupan Incident tidak menghapus Issue, Defect, Problem, atau Audit record terkait.

### 12.8 Reopened

**Subjek:** Incident dibuka kembali — `[Incident Reference]`

> Incident dibuka kembali karena **[gejala kembali/verifikasi gagal/dampak baru ditemukan]**. Dampak, severity, priority, dan owner sedang dinilai ulang. Untuk sementara, **[workaround/containment aman]**. Jangan mengulang tindakan yang dapat menghasilkan duplicate effect. Pembaruan berikutnya akan diberikan melalui kanal resmi.

### 12.9 Security-sensitive holding statement

**Subjek:** Pemeriksaan keamanan sedang berlangsung — `[Restricted Reference]`

> Kami sedang meninjau laporan yang berpotensi berkaitan dengan keamanan atau privasi. Untuk melindungi pengguna dan bukti, detail teknis dibatasi selama pemeriksaan. Mohon jangan mengirim password, token, file, screenshot, atau informasi sensitif melalui kanal yang tidak diminta. Ikuti hanya instruksi resmi **[tindakan aman yang telah disetujui]**. Informasi tambahan akan disampaikan kepada audience yang berwenang setelah validasi dan review keamanan.

### 12.10 Aturan penggunaan template

- hapus placeholder yang tidak relevan;
- jangan mengisi root cause sebelum terverifikasi;
- jangan memakai “tidak ada dampak” bila status masih unknown;
- jangan menyebut target waktu numerik tanpa policy/authority;
- security-sensitive message memerlukan review/approval yang dikonfirmasi;
- mass communication recipient/channel harus disetujui;
- simpan communication reference, audience class, waktu, sender function, dan version; jangan menyimpan content sensitif di Audit.

## 13. Closure criteria

Incident dapat berpindah dari `RESOLVED` ke `CLOSED` hanya jika seluruh kriteria yang berlaku telah dipenuhi atau exception tercatat oleh authority yang disetujui.

### 13.1 Checklist closure

- [ ] Resolution diterapkan pada environment/scope yang benar.
- [ ] Resolution diverifikasi dengan acceptance yang relevan, bukan hanya “perubahan sudah dideploy”.
- [ ] Source, target lama, result, dan data integrity telah diperiksa bila relevan.
- [ ] Tidak ada false success, silent loss, duplicate destructive effect, atau unsafe partial result.
- [ ] Security/privacy containment dan review lengkap bila relevan.
- [ ] Monitoring menunjukkan kondisi stabil selama observation window yang berlaku.
- [ ] Workaround ditarik, dipertahankan dengan governance, atau didokumentasikan dengan batas yang jelas.
- [ ] User pelapor dan affected audience telah menerima update yang sesuai.
- [ ] Related Issue diperbarui melalui status canonical yang benar.
- [ ] Defect, Problem, Service Request, change, deployment, dan known risk yang relevan telah ditautkan/diperbarui.
- [ ] Evidence aman, decision log, timeline, impact, severity/priority history, dan owner lengkap.
- [ ] Follow-up action mempunyai owner/reference/status.
- [ ] Known residual risk dicatat dan diterima/ditolak oleh authority yang benar.
- [ ] Keputusan RCA dicatat: required / not required + reason/authority.
- [ ] Audit/support/security evidence yang diwajibkan lengkap dan dapat diakses oleh pihak berwenang.
- [ ] Communication closure dikirim dan tidak mengandung janji/claim yang belum terbukti.
- [ ] Incident Owner dan reviewer/approver yang diwajibkan menyetujui closure.

### 13.2 Hal yang tidak cukup untuk closure

- alert berubah hijau satu kali;
- developer menyatakan fix selesai tanpa verifikasi;
- Issue user ditandai duplicate;
- workaround tersedia tetapi dampak masih aktif;
- tidak ada laporan baru karena kanal Issue gagal;
- reporter tidak merespons sementara dampak luas masih ada;
- Incident sudah melewati target waktu;
- root cause belum diketahui tetapi Problem juga belum dicatat;
- Security reviewer belum menyelesaikan containment/evidence yang diwajibkan.

Observation window, closure authority, evidence minimum per severity, dan RCA threshold bersifat `POLICY_CONFIGURABLE` dan `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

## 14. Reopen criteria

Incident `CLOSED` atau `RESOLVED` dibuka kembali jika:

- gejala yang sama kembali pada scope/environment terkait;
- resolution verification gagal atau menghasilkan hasil berbeda;
- monitoring menunjukkan layanan tidak stabil;
- workaround gagal, tidak aman, atau ternyata memperluas dampak;
- user/Issue baru memberi bukti bahwa dampak masih aktif;
- scope terdampak bertambah atau critical business flow baru terpengaruh;
- data corruption/loss/false success/duplicate effect baru ditemukan;
- security/privacy impact baru terkonfirmasi;
- Audit/evidence menunjukkan tindakan/resolution tidak lengkap;
- linked change/rollback tidak bertahan;
- closure dilakukan berdasarkan informasi yang kemudian terbukti salah.

### Prosedur reopen

1. Ubah status menjadi `REOPENED` dengan reason dan evidence aman.
2. Tetapkan kembali Incident Owner; pastikan handoff diakui.
3. Reassess impact, classification, severity, dan priority tanpa menyalin nilai lama otomatis.
4. Periksa apakah ini recurrence Incident yang sama atau event baru yang memerlukan Incident baru dan/atau Problem link.
5. Kembalikan ke `TRIAGE` atau `IN_PROGRESS`.
6. Komunikasikan kepada reporter/affected audience menggunakan template Reopened.
7. Perbarui linked Issue/Defect/Problem dan decision log tanpa mengubah status record lain otomatis.
8. Batasi komunikasi/evidence bila security-sensitive.

Exact reopen window, siapa yang dapat reopen, same-versus-new Incident rule, dan notification audience adalah `POLICY_CONFIGURABLE` dan `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

## 15. Pertamina confirmation gaps

| ID | Item yang harus dikonfirmasi | Status |
|---|---|---|
| `INC-GAP-01` | Apakah `SEV-1..SEV-4` sesuai taxonomy Incident Pertamina dan exact criteria/authority-nya | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `INC-GAP-02` | Target acknowledgement, triage, update, containment, resolution, dan closure per severity/priority | `POLICY_CONFIGURABLE`; `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `INC-GAP-03` | Incident priority naming/catalog dan hubungan dengan Issue/Defect priority | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `INC-GAP-04` | Daftar critical business service/process dan business impact threshold | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `INC-GAP-05` | System of record Incident, identifier, mandatory field, status transition, version/concurrency, dan retention | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `INC-GAP-06` | Authority untuk declare/downgrade/resolve/close/reopen major atau Security Incident | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `INC-GAP-07` | L1/L2/L3, on-call, owner group, handoff, escalation chain, dan backup owner | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `INC-GAP-08` | Kanal/contact resmi untuk reporter, affected users, mass communication, provider, Security, management, dan after-hours | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `INC-GAP-09` | Communication cadence, approval, sender authority, language, template, dan audience distribution | `POLICY_CONFIGURABLE`; `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `INC-GAP-10` | Security Incident notification, evidence, forensic, legal/compliance/privacy, and need-to-know process | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `INC-GAP-11` | Provider escalation route dan contractual responsibility tanpa memilih vendor baru | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `INC-GAP-12` | RCA/PIR trigger, owner, format, due target, approval, dan distribution | `POLICY_CONFIGURABLE`; `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `INC-GAP-13` | Observation window dan evidence minimum untuk resolution/closure per class/severity | `POLICY_CONFIGURABLE`; `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `INC-GAP-14` | Reopen window dan same Incident versus new Incident rule | `POLICY_CONFIGURABLE`; `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `INC-GAP-15` | Incident category taxonomy dan mapping resmi ke monitoring/service catalog | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `INC-GAP-16` | Exact Issue category/priority/transition/reopen catalog yang masih terbuka pada Document 15 | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `INC-GAP-17` | Access/classification/retention untuk Incident communication, evidence, export, dan linked records | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |
| `INC-GAP-18` | Audit requirement untuk Incident lifecycle yang belum memiliki event catalog canonical sendiri | `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION` |

Sampai dikonfirmasi:

- jangan menerbitkan SLA/OLA atau angka target;
- jangan membuat contact, nama tim, atau on-call route;
- jangan membuat Incident permission atau role baru;
- jangan memasukkan detail security/content ke Support record umum;
- jangan menyatakan taxonomy usulan sebagai standar Pertamina;
- tetap gunakan secure default dan existing Issue/Support permissions.

## 16. Risks/conflicts

| ID | Risiko/konflik | Dampak | Resolusi yang disarankan |
|---|---|---|---|
| `R-16A-01` | Incident status usulan berbeda dari Issue status canonical Document 15 | Implementasi dapat mengubah Issue workflow secara tidak sah | Pertahankan record/lifecycle terpisah; tautkan reference saja |
| `R-16A-02` | Document 15 melarang membuat enum Issue `RESOLVED/CLOSED`, sedangkan Incident memakai `RESOLVED/CLOSED` | Nama sama dapat disalin ke Issue | Label semua field dengan tipe record; tidak ada automatic mapping |
| `R-16A-03` | UAT Defect memakai severity `BLOCKER/CRITICAL/MAJOR/MINOR`, priority `DEF-P0..P3`, dan lifecycle berbeda | Defect dan Incident dapat tertukar | Jangan mapping 1:1; simpan link dan penilaian masing-masing |
| `R-16A-04` | Issue category/priority/transition/reopen belum final | Triage Incident dapat mencoba mengubah catalog Issue | Gunakan existing typed values saja; tunggu konfirmasi Pertamina |
| `R-16A-05` | Belum ada Incident schema/API/permission/audit catalog pada Document 06–10 | Manual dapat menciptakan mekanisme tanpa desain | Document 16 hanya proses; perubahan teknis memerlukan controlled addendum |
| `R-16A-06` | Support permission hanya untuk exact Issue/attachment dalam `SUPPORT_SCOPE` | Incident coordination dapat disalahartikan sebagai content access | Incident ownership tidak menambah permission; gunakan safe references dan restricted handoff |
| `R-16A-07` | Severity/priority/response target bukan standar resmi | Komunikasi dapat membuat SLA palsu | Gunakan `POLICY_CONFIGURABLE` dan confirmation gap, tanpa angka |
| `R-16A-08` | Alert owner/contact/escalation route belum ditetapkan | DETECTED dapat tidak beralih ke ACKNOWLEDGED | Konfirmasi ownership/on-call; monitoring harus punya escalation trigger |
| `R-16A-09` | Security Incident handling/notification/forensics belum didefinisikan lengkap | Detail sensitif dapat bocor atau evidence rusak | Restricted security process; holding statement; no ordinary Issue note for forensics |
| `R-16A-10` | Incident lifecycle Audit events belum ada pada Document 10 | Tim dapat mengarang `Incident.*` event dan payload | Gunakan existing Support/Auth/Authorization/Security/Audit evidence; addendum diperlukan untuk event baru |
| `R-16A-11` | In-App Issue sengaja minimal, sedangkan incident intake memerlukan detail | User dapat dibebani form teknis | Enrichment dilakukan Support/system; user tetap hanya pesan + screenshot opsional |
| `R-16A-12` | Monitoring status/telemetry dapat stale/unknown | Severity dapat terlalu rendah | Unknown critical telemetry bukan healthy; nilai impact dan fail safe |
| `R-16A-13` | Local-First dapat diabaikan ketika server/provider gagal | Incident communication dapat mematikan capability lokal yang aman | Assess actual route; pertahankan local-capable behavior; no public fallback |
| `R-16A-14` | Workaround belum melalui validation/security review | Workaround dapat menyebabkan data loss/exposure | Komunikasikan hanya workaround yang diuji, scoped, reversible bila mungkin |
| `R-16A-15` | Duplicate Issue dapat membocorkan reporter lain saat ditautkan | Risiko privacy/cross-scope | User hanya melihat status Issue sendiri; Incident summary tidak menampilkan reporter lain |
| `R-16A-16` | Provider failure dapat memicu penyebutan vendor atau fallback tidak disetujui | Konflik dengan arsitektur/provider policy | Gunakan provider profile aman; no vendor selection/public fallback |
| `R-16A-17` | UAT specification FINAL tetapi execution dan validated production build belum direkam | Prosedur belum terbukti pada operasi nyata | Lakukan tabletop, simulation, dan UAT/operational validation sebelum release |
| `R-16A-18` | Closure dapat dipakai untuk mengejar metrik walau risk/Problem terbuka | Incident ditutup prematur | Gunakan checklist closure, explicit residual risk/RCA decision, dan independent review sesuai severity |

Tidak ditemukan sumber yang memberi Support akses baru, menetapkan SLA resmi, atau mengesahkan contact/on-call tertentu. Rancangan ini tidak boleh digunakan untuk menginferensikan kewenangan tersebut.

## 17. Suggested Document 16 text

Teks berikut dapat digunakan sebagai baseline bagian **Incident Intake, Classification, Triage & Communication** pada master Document 16 setelah confirmation gap diselesaikan.

---

### Incident Intake dan Triage

PERTAMINA OTO membedakan Issue, Incident, Defect, Problem, Service Request, dan Security Incident.

- **Issue** adalah laporan atau pertanyaan yang belum tentu terbukti sebagai gangguan.
- **Incident** adalah gangguan layanan atau risiko aktif yang perlu dikoordinasikan sampai stabil.
- **Defect** adalah perilaku produk yang telah diverifikasi tidak sesuai requirement.
- **Problem** adalah penyebab dasar yang belum diketahui atau berulang.
- **Service Request** adalah permintaan layanan normal, bukan gangguan.
- **Security Incident** adalah dugaan/bukti akses tidak sah, kebocoran, kompromi, atau bypass security dan diproses secara terbatas.

Beberapa Issue dapat ditautkan ke satu Incident. Status Issue dan Incident selalu terpisah. Issue tetap memakai `BARU`, `DIPERIKSA`, `PERLU_PERBAIKAN`, `BUTUH_INFORMASI`, `DUPLIKAT`, `BUKAN_BUG`, `SEDANG_DIPERBAIKI`, `READY_FOR_TEST`, `TERVERIFIKASI`, dan `SELESAI`.

#### Sumber intake

Intake dapat berasal dari:

- In-App Issue Report;
- monitoring alert;
- observasi Administrator;
- laporan user melalui kanal resmi;
- UAT/test dan Defect;
- Audit/security event;
- deployment issue;
- approved provider failure.

Gunakan satu intake minimum: source/reference, received time, environment, symptom, observed impact, data/security flag, affected scope, Feature/platform/version bila diketahui, safe Error/Job/Correlation reference, workaround, recent change, dan triage owner.

Jangan memasukkan password, token, file/content, OCR, Assistant/AI payload, clipboard, signature, raw stack/SQL/body, storage key, atau detail forensik ke intake umum.

#### In-App Issue Report

User hanya menulis pesan singkat dan dapat menambahkan screenshot opsional setelah preview. Sistem menambahkan safe context bila tersedia. User tidak memilih category, severity, priority, owner, atau Incident status. Support melakukan enrichment dan triage.

#### Issue menjadi Incident bila

- capability penting unavailable/terdegradasi;
- dampak meluas;
- corruption, silent loss, false success, wrong target, atau duplicate destructive effect terjadi;
- source/result safety tidak pasti;
- cross-user/cross-scope access atau content/secret exposure diduga;
- mandatory Audit/control gagal;
- dependency kritis tidak pulih;
- safe recovery atau workaround tidak jelas.

Pertanyaan penggunaan, Service Request, Defect minor di test, atau false-positive alert tidak otomatis menjadi Incident.

### Classification

Pilih satu primary class: `AVAILABILITY`, `FUNCTIONAL_CORRECTNESS`, `DATA_INTEGRITY`, `SECURITY_PRIVACY`, `PERFORMANCE_CAPACITY`, `IDENTITY_ACCESS`, `INTEGRATION_PROVIDER`, `DEPLOYMENT_COMPATIBILITY`, `AUDIT_COMPLIANCE`, atau `SUPPORT_WORKFLOW`.

Catat juga environment, scope, capability, local/server route, data/security impact, recoverability, trend, dan change relation. Gunakan `Unknown` jika informasi belum dapat dipastikan.

### Severity

Model usulan:

- **SEV-1:** dampak sangat kritis, luas, data/security aktif, atau tidak ada jalur aman.
- **SEV-2:** dampak tinggi pada capability/group besar atau workaround sangat terbatas.
- **SEV-3:** dampak terbatas dengan workaround aman dan tanpa confirmed data/security breach.
- **SEV-4:** dampak rendah yang tidak menghalangi hasil bisnis utama.

Model ini bukan standar resmi Pertamina. Target waktu adalah `POLICY_CONFIGURABLE` dan `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

Severity berdasarkan dampak, bukan nama komponen. Local-capable Feature yang tetap aman mengurangi actual service impact. Satu cross-user disclosure dapat tetap SEV-1 walau hanya satu reporter.

### Priority

Priority terpisah dari Severity. Usulan Incident priority: `INC-P1` sampai `INC-P4`. Pertimbangkan urgency, deadline, user scope, workaround, critical process, security risk, repeat occurrence, trend, dan dependency. Priority dapat berubah dengan reason, tetapi tidak mengubah severity.

Nama dan target priority tetap `POLICY_CONFIGURABLE` dan `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

### Triage checklist

Periksa:

1. Apakah signal valid?
2. Environment mana yang terdampak?
3. Feature/business flow/platform/version apa yang terdampak?
4. Siapa dan seberapa luas dampaknya?
5. Adakah data/security/source/result risk?
6. Adakah workaround aman?
7. Dapat direproduksi dengan data aman?
8. Adakah Error Code, Job ID, Correlation ID, atau safe evidence?
9. Adakah recent deployment/config/policy/permission/provider change?
10. Adakah duplicate Issue/Incident/Defect?
11. Apa classification, severity, priority, dan reason provisional?
12. Siapa Incident Owner dan resolver/reviewer yang diperlukan?
13. Apa next action dan communication checkpoint?

Identifier tidak memberi akses. Jangan membuka content pengguna untuk triage.

### Status Incident

Gunakan:

```text
DETECTED
ACKNOWLEDGED
TRIAGE
IN_PROGRESS
WAITING_INFORMATION
MONITORING
RESOLVED
CLOSED
REOPENED
```

`MONITORING` belum berarti `RESOLVED`. `RESOLVED` belum berarti `CLOSED`. `WAITING_INFORMATION` tidak menghentikan mitigasi Incident berat. `REOPENED` harus menilai ulang impact, severity, priority, dan owner.

Status Incident tidak mengubah status Issue otomatis.

### Ownership

Setiap Incident mempunyai satu Incident Owner aktif. Fungsi lain dapat mencakup triage, Support, technical resolution, Administrator, Security reviewer, communications, Product/Business decision owner, dan Auditor/reviewer.

Fungsi tersebut bukan role/permission baru. Incident Owner tidak otomatis mendapat akses file, Admin, Support, atau Audit. Handoff harus diakui dan memuat status, impact, severity/priority, tindakan, risk, next update, dan safe references.

### Communication

Setiap pesan menjelaskan fakta yang diketahui, impact, tindakan aman, dan next checkpoint. Jangan menyalahkan pihak lain, menjanjikan waktu palsu, menyebut root cause yang belum terverifikasi, atau membocorkan content/security detail.

Audience:

- user pelapor menerima acknowledgement, request information, workaround, dan status sederhana;
- affected users menerima impact dan tindakan aman;
- Support menerima triage/linked Issue/safe evidence;
- Administrator menerima safe health/config action;
- Developer menerima build, Feature, expected/actual, safe reproduction/evidence;
- Security reviewer menerima restricted impact/containment evidence;
- Product/Project Owner menerima business impact, risk, dan decision needed.

Cadence, channel, contact, dan approval adalah `POLICY_CONFIGURABLE` dan `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`.

#### Acknowledgement

> Kami telah menerima laporan mengenai **[gejala]** pada **[Feature/capability]**. Tim sedang memeriksa dampak. Untuk sementara, **[tindakan aman]**. Pembaruan berikutnya disampaikan melalui kanal resmi setelah tahap pemeriksaan berikutnya. Jangan mengirim password, token, atau isi dokumen.

#### Request information

> Mohon kirimkan **[informasi minimum]**, misalnya platform, app version, waktu, atau Error Code yang terlihat. Jangan mengirim password, token, isi dokumen, OCR, atau data sensitif. Preview screenshot sebelum mengirim.

#### Investigation update

> Kami telah mengonfirmasi **[fakta]**. Dampak saat ini **[scope/business impact]**. Penyebab **[masih diteliti/belum dikonfirmasi]**. Tindakan: **[safe action]**. Gunakan **[validated workaround]** bila sesuai.

#### Workaround

> Untuk sementara, gunakan **[validated steps]** pada **[scope/platform]**. Jangan melewati permission/policy atau memakai workaround di luar kondisi tersebut. Hentikan dan laporkan bila hasil tidak sesuai.

#### Delayed resolution

> Pemulihan belum dinyatakan selesai karena **[safe reason]**. Dampak terbaru: **[impact]**. Tindakan aman tetap: **[workaround/containment]**. Kami akan memperbarui melalui kanal resmi setelah checkpoint berikutnya.

#### Resolved

> Gangguan telah dipulihkan dan verifikasi awal lulus pada **[scope]**. Pengguna dapat **[next step]**. Monitoring dan tindak lanjut masih dilengkapi sebelum penutupan.

#### Closed

> Incident ditutup setelah resolution, communication, evidence, linked record, risk, RCA decision, dan Audit requirement lengkap. Tindak lanjut yang masih terbuka: **[reference]**.

#### Reopened

> Incident dibuka kembali karena **[reason]**. Impact, severity, priority, dan owner sedang dinilai ulang. Gunakan **[safe workaround]** dan hindari tindakan yang dapat menghasilkan duplicate effect.

#### Security-sensitive holding statement

> Kami sedang meninjau laporan yang berpotensi berkaitan dengan keamanan atau privasi. Detail dibatasi selama pemeriksaan. Jangan mengirim password, token, file, screenshot, atau informasi sensitif melalui kanal yang tidak diminta. Ikuti hanya instruksi resmi yang telah disetujui.

### Closure

Tutup Incident hanya setelah:

- resolution verified;
- data/source/result safety diperiksa;
- monitoring stabil sesuai policy;
- user/affected update dikirim;
- linked Issue/Defect/Problem/change diperbarui;
- evidence dan decision log lengkap;
- follow-up dan known risk mempunyai owner/reference;
- RCA decision dicatat;
- Audit/security requirement lengkap;
- closure disetujui authority yang benar.

Alert hijau sekali, fix yang belum diuji, duplicate Issue, atau target waktu yang lewat bukan alasan closure.

### Reopen

Reopen bila gejala kembali, verifikasi gagal, monitoring tidak stabil, workaround gagal, impact meluas, data/security risk baru ditemukan, atau closure didasarkan pada informasi yang salah. Reassess classification, severity, priority, owner, dan communication. Status record lain tidak berubah otomatis.

---

Sebelum teks ini masuk ke master Document 16, selesaikan seluruh `REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`, tetapkan system of record/ownership/contact/security handling, lakukan controlled addendum bila diperlukan, dan validasi prosedur melalui tabletop serta UAT/operational exercise.
