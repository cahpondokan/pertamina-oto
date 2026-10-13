# AGENT A — DOCUMENT INTEGRITY, VERSION & TERMINOLOGY AUDIT REPORT

**Task ID:** `BASELINE-AUDIT-AGENT-A`  
**Tanggal audit:** 2 September 2026  
**Ruang lingkup:** Document 01–19 FINAL dan `MASTER_PROJECT_SUMMARY.md` v4.2, termasuk file pendukung yang diperlukan untuk memeriksa versi, keputusan, duplikasi, dan arsip  
**Sifat laporan:** audit independen; bukan persetujuan Pertamina, bukan perubahan sumber kebenaran, dan bukan bukti implementasi  
**Baseline substansi yang diuji:** 550 Feature ID canonical; 33 Primary PDF Tool Cards; dokumentasi pra-implementasi

---

## 1. Executive summary

Seluruh **19 dokumen FINAL** dan Master Project Summary terbaru tersedia secara lokal dan dapat dibaca. Versi yang tercantum pada filename cocok dengan versi metadata masing-masing. Baseline substansi utama juga konsisten:

- Document 03 berisi **550 baris Feature**, **550 Feature ID unik**, dan tidak mempunyai duplicate Feature ID;
- permukaan utama berisi **33 Primary PDF Tool Cards**; 33 card tersebut mereferensikan 36 Feature ID unik karena beberapa card adalah bundle resmi, bukan karena duplikasi;
- tidak terdapat angka aktif `546` pada Document 01–19 FINAL atau Master v4.2;
- `Sign PDF` konsisten sebagai tanda tangan visual/paraf non-PKI, sedangkan Digital Signature berbasis certificate/PKI tetap di luar scope;
- `Local-First`, `Offline-First`, REST authority, SignalR notification, Job waiting states, `uploadId`, dan `fileRef` tidak menunjukkan konflik material;
- seluruh jumlah keputusan yang diklaim pada Document 04–19 cocok dengan register atau daftar keputusan yang tersedia;
- tidak ditemukan klaim positif bahwa implementasi selesai, UAT telah diterima, Production telah dideploy, handover telah diterima, atau proyek telah ditutup.

Namun baseline **belum bersih secara configuration control**. Audit menemukan empat masalah mayor:

1. Folder snapshot `PERTAMINA_OTO_CODEX_HANDOFF_DOC06_2026-08-24` memuat Document 01–05 dengan **filename dan version yang sama** seperti current FINAL, tetapi hash dan isi berbeda. Snapshot lama D02–D03 masih memuat angka `546`.
2. Document 07 dan 08 masing-masing mempunyai dua salinan identik pada root dan folder `docs`; walaupun hash sama, canonical path tidak dinyatakan oleh manifest berwenang.
3. Master v3.4, v4.0, dan v4.1 dirujuk oleh D12, D18, dan D19, tetapi immutable snapshot tersebut tidak tersedia. Current Master adalah alias mutable v4.2 tanpa file snapshot bernama-versi.
4. Master menyatakan audit bahasa Document 01–10 `10/10 PASS`, tetapi masih terdapat narasi penuh berbahasa Inggris pada D05–D06 dan campuran naratif pada dokumen lain. Status tersebut tidak dapat direproduksi dengan kriteria `ID-1.0` yang tertulis.

**Opini audit:** dokumen dapat digunakan sebagai baseline substansi pra-implementasi, tetapi **memerlukan controlled remediation sebelum baseline integrity/configuration freeze dinyatakan bersih atau diterima secara formal**. Opini ini tidak mengubah status FINAL pada master dan tidak mengubah business meaning.

---

## 2. Document inventory

### 2.1 Aturan pembacaan manifest audit

- `DOC-01` sampai `DOC-17` adalah ID audit yang diturunkan dari nomor dokumen; hanya D18 dan D19 yang mencantumkan `Document ID` eksplisit di dalam file.
- Parent dan Child di bawah adalah relasi baseline yang diturunkan dari isi. Sebagian dokumen hanya menyebut “Documents 01–N FINAL”, bukan exact filename/hash.
- `Current=YES` berarti file dipakai Master v4.2 sebagai current baseline, bukan berarti owner/approver telah mengesahkan hash.
- “Source of Truth” berarti domain informasi yang dikuasai dokumen; Master hanya current index, bukan pengganti source domain.

### 2.2 Identity, version, lifecycle, and relationship inventory

| Document ID | Filename canonical yang diaudit | Version | Status metadata | Date metadata | Parent | Child/downstream | Source of Truth? | Current? |
|---|---|---:|---|---|---|---|---|---|
| `DOC-01` | `01_BUSINESS_PROCESS_FINAL_v3.4_PERTAMINA_OTO.md` | 3.4 | FINAL — Selaras Standar Teknologi IT Pertamina | Tidak dicantumkan | — | D02–D19 | Ya — proses dan aturan bisnis | YES |
| `DOC-02` | `02_REQUIREMENT_SPECIFICATION_FINAL_v1.5_PERTAMINA_OTO.md` | 1.5 | FINAL — PRE-CODING BASELINE | Tidak dicantumkan | D01 | D03–D19 | Ya — requirement | YES |
| `DOC-03` | `03_FEATURE_CATALOG_FINAL_v1.3_PERTAMINA_OTO.md` | 1.3 | FINAL — SOLUTION-ALIGNED FEATURE BASELINE | Tidak dicantumkan | D01–D02 | D04–D19 | Ya — Feature ID, record, count, dan 33 Core mapping | YES |
| `DOC-04` | `04_SOLUTION_DESIGN_FINAL_v1.2_PERTAMINA_OTO.md` | 1.2 | FINAL — LOCKED PRE-CODING SOLUTION BASELINE | Tidak dicantumkan | D01–D03 | D05–D19 | Ya — solution/architecture decisions | YES |
| `DOC-05` | `05_TECHNICAL_REFERENCE_FINAL_v1.2_PERTAMINA_OTO.md` | 1.2 | FINAL — LOCKED PRE-CODING TECHNICAL BASELINE | 18 Agustus 2026 | D01–D04 | D06–D19 | Ya — technical baseline | YES |
| `DOC-06` | `docs/06-api/06_API_DOCUMENTATION_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL — LOCKED PRE-CODING API BASELINE | 24 Agustus 2026 | D01–D05 + API decisions | D07–D19 | Ya — API/contract | YES |
| `DOC-07` | `docs/07-database/07_DATABASE_DESIGN_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL — PRE-CODING BASELINE | 24 Agustus 2026 | D01–D06 | D08–D19 | Ya — database/data ownership | YES |
| `DOC-08` | `docs/08-security/08_SECURITY_DESIGN_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL — PRE-CODING BASELINE | 24 Agustus 2026 | D01–D07 | D09–D19 | Ya — security design | YES |
| `DOC-09` | `docs/09-role-matrix/09_ROLE_MATRIX_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL — PRE-CODING BASELINE | 26 Agustus 2026 | D01–D08 | D10–D19 | Ya — role/permission/policy model | YES |
| `DOC-10` | `docs/10-audit-trail/10_AUDIT_TRAIL_DESIGN_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL — PRE-CODING BASELINE | 27 Agustus 2026 | D01–D09 | D11–D19 | Ya — Enterprise Audit design | YES |
| `DOC-11` | `docs/11-unit-test/11_UNIT_TEST_DOCUMENT_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL — PRE-CODING BASELINE | 28 Agustus 2026 | D01–D10 | D12–D19 | Ya — unit-test specification | YES |
| `DOC-12` | `docs/12-test-plan/12_TEST_PLAN_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL — PRE-CODING BASELINE | 28 Agustus 2026 | D01–D11 + Master v3.4 | D13–D19 | Ya — test strategy/plan, bukan actual result | YES |
| `DOC-13` | `docs/13-uat/13_UAT_DOCUMENT_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL — PRE-CODING UAT BASELINE; execution belum direkam | 1 September 2026 | D01–D12 | D14–D19 | Ya — UAT specification, bukan sign-off | YES |
| `DOC-14` | `docs/14-user-manual/14_USER_MANUAL_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL — PRE-IMPLEMENTATION USER MANUAL BASELINE | 1 September 2026 | D01–D13 | D15–D19 | Ya — user-manual baseline, bukan release manual | YES |
| `DOC-15` | `docs/15-admin-manual/15_ADMIN_MANUAL_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL — PRE-IMPLEMENTATION ADMIN MANUAL BASELINE | 1 September 2026 | D01–D14 | D16–D19 | Ya — admin-manual baseline | YES |
| `DOC-16` | `docs/16-incident-support/16_INCIDENT_SUPPORT_GUIDE_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL — PRE-IMPLEMENTATION INCIDENT SUPPORT BASELINE | 1 September 2026 | D01–D15 | D17–D19 | Ya — incident/support process baseline | YES |
| `DOC-17` | `docs/17-deployment/17_DEPLOYMENT_GUIDE_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL — PRE-IMPLEMENTATION DEPLOYMENT BASELINE | 1 September 2026 | D01–D16 | D18–D19 | Ya — deployment governance, bukan deployment evidence | YES |
| `DOC-18` | `docs/18-application-cartography/18_APPLICATION_CARTOGRAPHY_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL — PRE-IMPLEMENTATION APPLICATION CARTOGRAPHY BASELINE | 1 September 2026 | D01–D17 + Master v4.0 | D19 | Ya — conceptual cartography, bukan actual CMDB inventory | YES |
| `DOC-19` | `docs/19-handover/19_HANDOVER_CHECKLIST_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL — PRE-IMPLEMENTATION HANDOVER BASELINE | 1 September 2026 | D01–D18 + Master v4.1 | Master current index/next phase | Ya — checklist/evidence/gate model, bukan actual handover | YES |
| `MASTER` | `MASTER_PROJECT_SUMMARY.md` | 4.2 | Documents 01–19 FINAL — DOCUMENTATION BASELINE COMPLETE / IMPLEMENTATION NEXT | Snapshot 1 September 2026 | D01–D19 | Current index | Tidak untuk domain; ya sebagai current index | YES |

### 2.3 File integrity inventory

| Document ID | File Size (bytes) | SHA-256 | Duplicate? | Replaced By | Open Gap utama |
|---|---:|---|---|---|---|
| `DOC-01` | 49,603 | `D13DDC9CD3259BAD7C7889635DB44DDDCC47FB228A62CC3B8CCE203AC23E1E1B` | Ya — divergent snapshot | — | Date, explicit Document ID, owner/reviewer/approver, canonical path manifest |
| `DOC-02` | 57,634 | `24986DC7D4D369935CA7F8D382F6AB2AE88464E9F640AB5CFD726B7F761B1F11` | Ya — divergent snapshot | — | Date, owner/reviewer/approver, logical-vs-physical filename mapping |
| `DOC-03` | 187,018 | `B908B1E6E45D84AD26EBB4E8504F1B0FAA7AD96A6C85FB5C92EE0BC580429A45` | Ya — divergent snapshot | — | Date, owner/reviewer/approver, formula raw-candidate perlu diperjelas |
| `DOC-04` | 35,082 | `9C973809CE1083C4BC36EA87371BC31D5CAD4C95684531357B6ECC4665C31A5D` | Ya — divergent snapshot | — | Date; historical parent refs v3.3/v1.3/v1.1 tidak tersedia; decision rows tanpa stable ID |
| `DOC-05` | 19,641 | `A2D212402396F4DB26A62F3A1B756D4477988AF73A9D84F4171C35029E4854BD` | Ya — divergent snapshot | — | Revision history v1.0–v1.2 dan approval metadata tidak tersedia |
| `DOC-06` | 62,104 | `A9D1DB2E327152C9944589286D49CFFB48CBD787C7921ACAB3CA8DAEDCAABDC3` | Tidak untuk FINAL | — | Changelog/owner; “Final documents 6/19” perlu diberi label historical snapshot |
| `DOC-07` | 63,514 | `4786BFC5C632F619DCFF89004C314202A39C84A66576CD8AD5C577B66B0D6738` | Ya — identical root copy | — | Canonical path belum dimanifestasikan; owner/changelog/parent hash |
| `DOC-08` | 81,859 | `DF9C8BA8B70B224206918096FC2ACBE990C452AA932F27871FB36B282F6F404F` | Ya — identical root copy | — | Canonical path belum dimanifestasikan; owner/changelog/parent hash |
| `DOC-09` | 70,674 | `DA5F13B4C2E330D755D79C3F143FB270C75B046CFBDC438791ECD70EC8EDEC5B` | Tidak | — | Owner/changelog/exact parent hashes |
| `DOC-10` | 61,735 | `C2926933268E315625F943C2DA3C97648BBF91973089C7CF3D1AF81895AD57BE` | Tidak | — | Owner/changelog/exact parent hashes |
| `DOC-11` | 55,205 | `AD1E9323961C64380C863795A2E20767D4EBA67999C4F28DEA9A0B578C596E14` | Tidak | — | Owner/changelog; specification has no execution evidence by design |
| `DOC-12` | 82,038 | `A4449E43F877327FA645C3D02E1875958345F727DDC2FA576E88D7F8C419586D` | Tidak | — | Master v3.4 snapshot missing; owner/changelog |
| `DOC-13` | 37,489 | `13C1495361FD65969BF5CB38D50E79DD99CE045DE03B6FF9FA24B75386B6DB51` | Tidak | — | Actual UAT execution/sign-off intentionally pending; owner/changelog |
| `DOC-14` | 35,964 | `15BE8D47A4FC7D7442488180C188B42E8369270F5E905F051F09D57B37D407EB` | Tidak | — | RC/UI label/screenshot/release validation pending; owner/changelog |
| `DOC-15` | 49,581 | `2B0285DF799539E028F611540EC6FAE65F1CEC8746A5137A21D9955AAEAA2520` | Tidak | — | RC/UI/screenshot/SSO confirmation pending; owner/changelog |
| `DOC-16` | 27,948 | `CF821E9D9A6502921951858FDD54EE2DDD93CAB26FF07A76FB904B0DE66E8F85` | Tidak | — | Organization/contact/SLA/actual drill pending; owner/changelog |
| `DOC-17` | 28,053 | `D66287E4ABB41BFBD6D711F2A0A906D125716FDDBC457F3E238FFB623C04D03E` | Tidak | — | Production topology/access/build/deployment evidence pending; owner/changelog |
| `DOC-18` | 39,287 | `1EB8D28DE56016A92AE8609E1298791AB33174084C3A7C1B4C1D3F33F0FBAB83` | Tidak | — | Master v4.0 snapshot missing; owner/CMDB/Application ID/actual inventory pending |
| `DOC-19` | 27,362 | `7280D200F1A3F7A6F4A70DC2CE8F8D0913622822552C26954BA15C752DAD6ECC` | Tidak | — | Master v4.1 snapshot missing; actual transfer/KT/acceptance/closure pending |
| `MASTER` | 34,207 | `781F57D6AFDD1229F794515C26E6DE51E0EBE948A17D1CC76F39C3BC3D674004` | Older summaries exist | — | Immutable v4.2 snapshot, owner/approver, per-document hashes, supersession chain |

Tidak ada current file yang telah digantikan oleh versi lebih baru di baseline aktif. Kolom “Replaced By” karena itu `—`; file lama diklasifikasikan pada Bagian 5 dan 13, tanpa dihapus.

---

## 3. Version audit

| Scope | Hasil | Temuan |
|---|---|---|
| D01 v3.4 | PASS dengan gap metadata | Riwayat 1.0, 1.1, 2.0, 2.1, 2.2, 3.0–3.4 tercatat; tanggal dan approval tidak tercatat |
| D02 v1.5 | PASS dengan gap metadata | Riwayat 1.0–1.5 tercatat; 14 decision pada catatan v1.0 adalah historical, current document mempunyai 30 `DEC-REQ` |
| D03 v1.3 | PASS | Riwayat 1.0–1.3 tercatat; current count 550; 539 hanya historical v1.0 |
| D04 v1.2 | PASS dengan stale historical refs | Riwayat 1.0–1.2 tercatat; current parents benar, tetapi hasil audit lama menunjuk v3.3/v1.3/v1.1 yang tidak tersedia |
| D05 v1.2 | PARTIAL | Filename/header cocok, tetapi riwayat perubahan 1.0–1.2 tidak tersedia di dalam dokumen |
| D06–D19 v1.0 | PASS untuk filename/header; PARTIAL untuk lifecycle | Semua filename/version cocok. Working v0.1 atau draft tersedia untuk D06–D19, tetapi changelog/supersession tidak konsisten dicantumkan pada FINAL |
| Master v4.2 | PARTIAL | Current alias v4.2 tersedia. Hanya older v2.2 dan v2.5 yang tersimpan; v3.4, v4.0, dan v4.1 yang dirujuk tidak tersedia sebagai immutable snapshot |

### 3.1 Same-version content divergence

Snapshot handoff lama mempunyai filename/version sama seperti current D01–D05 tetapi hash berbeda. Perbandingan baris menunjukkan perubahan nyata, bukan hanya newline:

| Document | Difference entries | Contoh perbedaan material/editorial |
|---|---:|---|
| D01 | 68 | Heading Inggris diganti Indonesia dan penanda `ID-1.0` ditambahkan |
| D02 | 56 | Narasi lama menyebut `546`; current menghapus angka lama dan melokalkan heading |
| D03 | 36 | Riwayat lama menyebut 546; current menormalkan historical wording dan bahasa |
| D04 | 106 | Heading/register dilokalkan |
| D05 | 376 | Sebagian besar narasi dilokalkan; metadata `ID-1.0` ditambahkan |

Master menyatakan controlled language revision tidak mengubah technical version. Itu menjelaskan niat, tetapi version string saja tidak lagi cukup sebagai identity. Baseline manifest wajib memakai **version + content revision + SHA-256** atau menerbitkan editorial revision baru.

### 3.2 Lifecycle terminology

`PRE-CODING` dipakai D02–D13, sedangkan `PRE-IMPLEMENTATION` dipakai D14–D19. Keduanya secara substansi menunjuk tahap sebelum implementasi/evidence, tetapi belum didefinisikan sebagai alias resmi. Pilih satu lifecycle vocabulary dan pertahankan descriptor domain dokumen secara terpisah.

---

## 4. Parent-reference audit

### 4.1 Hasil umum

Rantai substansi bersifat linear dan dapat dipahami: Business Process → Requirement → Feature Catalog → Solution Design → Technical Reference → API → Database → Security → Role → Audit → Unit Test → Test Plan → UAT → Manuals → Incident → Deployment → Cartography → Handover. Tidak ditemukan child yang secara substansi menunjuk parent domain yang salah.

Namun configuration identification parent belum reproducible:

- D07–D17 umumnya menyebut `Documents 01–N FINAL` atau baseline table, bukan exact canonical path, version, dan hash.
- D07 menyebut “Master Project Summary — snapshot terakhir yang disetujui” tanpa version/hash.
- D12 mereferensikan Master v3.4; D18 Master v4.0; D19 Master v4.1. Ketiga snapshot itu tidak tersedia.
- D19 juga menyatakan `MASTER_PROJECT_SUMMARY.md` telah menjadi current v4.2. Ini tidak menghapus kebutuhan menyimpan parent snapshot v4.1 yang dipakai saat D19 dibentuk.
- Child relationship tidak dicatat sebagai metadata pada dokumen; hubungan harus diturunkan manual.

### 4.2 Stale and unresolved filename references

Tidak terdapat Markdown hyperlink, sehingga tidak ada broken clickable link. Namun ditemukan **22 filename-like references yang tidak resolve sebagai file lokal**:

- 19 logical names dari `01_BUSINESS_PROCESS.md` sampai `19_HANDOVER_CHECKLIST.md`;
- 3 historical physical names pada D04: `01_BUSINESS_PROCESS_FINAL_v3.3_PERTAMINA_OTO.md`, `02_REQUIREMENT_SPECIFICATION_FINAL_v1.3_PERTAMINA_OTO.md`, dan `03_FEATURE_CATALOG_FINAL_v1.1_PERTAMINA_OTO.md`.

Logical name dapat dipertahankan sebagai `LogicalDocumentName`, tetapi manifest wajib memetakannya ke canonical physical filename. Tiga historical physical reference harus diberi label `HISTORICAL_SUPERSEDED_REFERENCE` dan, bila merupakan evidence penting, disertai archive location/hash.

### 4.3 Parent-reference conclusion

| Check | Result |
|---|---|
| Substantive parent ordering | PASS |
| Current parent versions D01–D06 | PASS |
| Exact parent hash/revision D07–D19 | PARTIAL |
| Master snapshot reproducibility | FAIL — referenced v3.4/v4.0/v4.1 missing |
| Child metadata | MISSING |
| Logical-to-physical filename mapping | MISSING |

---

## 5. Duplicate/stale file audit

| Asset/group | CURRENT FINAL | Superseded/archive candidate | Classification | Risk |
|---|---|---|---|---|
| D01–D05 | Root current files dengan hashes pada Bagian 2 | Lima file bernama sama di `PERTAMINA_OTO_CODEX_HANDOFF_DOC06_2026-08-24` | Snapshot harus `SUPERSEDED`; divergent content | Critical ambiguity; snapshot D02–D03 masih memuat 546 |
| D07 | `docs/07-database/07_DATABASE_DESIGN_FINAL_v1.0_PERTAMINA_OTO.md` | Root `07_DATABASE_DESIGN_FINAL_v1.0_PERTAMINA_OTO.md` | Identical duplicate; root menjadi `ARCHIVE_CANDIDATE` atau controlled pointer | Path ambiguity, tidak ada content conflict |
| D08 | `docs/08-security/08_SECURITY_DESIGN_FINAL_v1.0_PERTAMINA_OTO.md` | Root `08_SECURITY_DESIGN_FINAL_v1.0_PERTAMINA_OTO.md` | Identical duplicate; root menjadi `ARCHIVE_CANDIDATE` atau controlled pointer | Path ambiguity, tidak ada content conflict |
| D06 working | Current FINAL di `docs/06-api` | `06_API_DOCUMENTATION_WORKING.md` dan handoff `06_API_DOCUMENTATION_WORKING_v0.2...` | Working/reference only | Dapat disalahgunakan sebagai contract aktif |
| D07–D19 working | FINAL di folder masing-masing | 13 file `*_WORKING_v0.1_*` | `SUPERSEDED_WORKING`; archive candidate | Search result dapat memilih working lebih dulu |
| Master | `MASTER_PROJECT_SUMMARY.md` v4.2 | v2.2 root dan v2.5 dalam handoff | `SUPERSEDED_MASTER`; archive candidate | Old status dapat dibaca sebagai current |
| Agent reports/audit reports | Bukan source of truth | Tetap sebagai evidence/input | `NON_AUTHORITATIVE_INPUT`; jangan dihapus tanpa retention decision | Kutipan lama dapat membawa baseline historis |

Current canonical path untuk D07 dan D08 dipilih berdasarkan struktur `docs/<domain>` dan pemakaian Master; pilihan ini adalah rekomendasi audit sampai disahkan oleh manifest resmi.

Tidak ada file yang dihapus atau dipindahkan oleh audit ini.

---

## 6. Feature-count audit

| Check | Evidence | Result |
|---|---|---|
| Canonical catalog rows | 550 baris yang diawali exact `FEAT-*` pada D03 | PASS |
| Unique Feature IDs | 550 unique; duplicate list kosong | PASS |
| Primary Tool surface | 33 numbered tool-card rows | PASS |
| Feature mapping pada 33 card | 36 unique Feature IDs karena card bundle seperti Forms/Page operations | PASS — bukan conflict |
| Angka `550` pada authoritative set | 81 occurrence pada 17 dari 20 file audit | CONSISTENT; D01, D02, dan D04 tidak menyebut angka tetapi tidak menyangkal D03 |
| Angka `546` pada authoritative set | 0 occurrence | PASS |
| Angka `546` di workspace non-current | 3 occurrence pada handoff snapshot D02–D03; occurrence lain hanya laporan yang membahas angka lama | ARCHIVE CONTAMINATION RISK |
| Angka `539` | D03 revision history v1.0 dan row number 539 (`FEAT-PROC-013`) | HISTORICAL/ROW INDEX; bukan active count |

D03 menyatakan raw inventory 557, tambahan audit 12, raw candidate setelah audit + Solution Design additions 580, lalu canonical 550. Perhitungan mengimplikasikan 11 Solution Design additions (`557 + 12 + 11 = 580`) dan dedup 30 record. Angka akhir dapat dibuktikan, tetapi formula intermediate sebaiknya ditulis eksplisit untuk mencegah salah tafsir.

**Source of truth feature count:** D03 v1.3. Dokumen downstream dan Master tidak boleh membuat count baru.

---

## 7. Decision-count audit

| Document | Claimed | Verified source | Unique decisions/rows | Result |
|---|---:|---|---:|---|
| D01 | Tidak menetapkan count | Business process/revision history | N/A | N/A |
| D02 | Tidak dicantumkan sebagai total current | `DEC-REQ-001..030` di dokumen | 30 | PASS; “14 keputusan” hanya revision note v1.0 |
| D03 | Tidak menetapkan decision count | Feature catalog | N/A | N/A |
| D04 | 18/18 LOCKED | 18 numbered rows di Decision Lock Register | 18 | PASS; stable Decision ID belum tersedia |
| D05 | 15/15 LOCKED | `TECH-DEC-001..015` | 15 | PASS |
| D06 | 9/9 LOCKED | 9 file `API-DEC-001..009` | 9 | PASS |
| D07 | 12/12 AUTO-LOCKED | `DB-AUTO-001..012` | 12 | PASS |
| D08 | 20/20 AUTO-LOCKED | `SEC-DEC-001..020` | 20 | PASS |
| D09 | 18/18 AUTO-LOCKED | `ROLE-DEC-001..018` | 18 | PASS |
| D10 | 20/20 AUTO-LOCKED | `AUD-DEC-001..020` | 20 | PASS |
| D11 | 20/20 AUTO-LOCKED | `UT-DEC-001..020` | 20 | PASS; external `TECH-DEC-013` reference tidak dihitung ulang |
| D12 | 25/25 AUTO-LOCKED | `TP-DEC-001..025` | 25 | PASS; external `TECH-DEC-013` reference tidak dihitung ulang |
| D13 | 25/25 AUTO-LOCKED | `UAT-AUTO-001..025` | 25 | PASS |
| D14 | 31/31 LOCKED | `MAN-AUTO-001..030` + `MAN-USER-001` | 31 | PASS |
| D15 | 40/40 AUTO-LOCKED | `ADM-AUTO-001..040` | 40 | PASS |
| D16 | 45/45 AUTO-LOCKED | `INC-AUTO-001..045` | 45 | PASS |
| D17 | 55/55 AUTO-LOCKED | `DEP-AUTO-001..055` | 55 | PASS |
| D18 | 30/30 AUTO-LOCKED | `CART-DEC-001..030` | 30 | PASS |
| D19 | 40/40 AUTO-LOCKED | `HO-DEC-001..040` | 40 | PASS |

Tidak ada global decision total yang ditetapkan sebagai source of truth. Jumlah antar-dokumen tidak boleh dijumlahkan sebagai satu “project decision count” karena beberapa keputusan adalah turunan domain dan beberapa register mereferensikan keputusan parent.

Gap utama adalah D04: 18 row dapat dihitung, tetapi tidak mempunyai stable ID seperti register lain. Penambahan documentation-only Decision ID memerlukan controlled revision agar existing decision meaning tidak berubah.

---

## 8. Terminology audit

### 8.1 Mandatory search-term result

Pencarian case-insensitive dilakukan pada D01–D19 + Master v4.2.

| Search term | Occurrences / documents | Audit conclusion |
|---|---:|---|
| `546` | 0 / 0 | Tidak aktif; hanya ada pada superseded handoff snapshot dan laporan historis |
| `550` | 81 / 17 | Baseline konsisten; D03 authority |
| `33 Primary` / `33 Core` | 111 / 20 | Konsisten sebagai 33 Primary PDF Tool Cards/Core Tool Cards |
| `Digital Signature` | 23 / 10 | Selalu dibatasi sebagai certificate/PKI out of scope |
| `PKI` | 90 / 18 | Konsisten: non-PKI visual sign included; PKI signing excluded |
| `Sign PDF` | 37 / 15 | Konsisten ke `FEAT-SIGN-013`, visual signature/paraf/stamp |
| `Local-First` | 83 / 17 | Konsisten sebagai routing/storage/processing preference berbasis capability/policy |
| `Offline-First` | 31 / 11 | Konsisten sebagai behavior ketika koneksi tidak tersedia; bukan sinonim Local-First |
| `SignalR` | 141 / 16 | Konsisten sebagai notification-only; REST/persisted state authority |
| `source of truth` / authority | 30 / 12 | Substansi konsisten; bahasa istilah bercampur |
| `PAUSED` | 5 / 4 | Hanya muncul sebagai state yang dilarang/tidak diperkenalkan |
| `WAITING_CONNECTION` | 11 / 7 | Canonical Job waiting state |
| `WAITING_RESOURCE` | 10 / 7 | Canonical Job waiting state |
| `JobWaiting` | 4 / 3 | Event notification, bukan state baru |
| `uploadId` | 11 / 3 | Transfer lifecycle identity; casing JSON konsisten |
| `fileRef` | 22 / 8 | Typed opaque execution reference; bukan authorization grant |
| `Hostinger` | 9 / 3 | Hanya temporary Development/limited QA/Pilot; bukan Production baseline |
| `Production` | 144 / 15 | Target environment/gate; tidak ada klaim current deployment |
| `FINAL` | 351 / 20 | Status dokumen; bukan otomatis implementation/test/release/handover status |
| `UAT completed` | 0 positive claim | UAT actual tetap `NOT_RUN`/belum direkam |
| `handover completed` | 0 positive claim | Actual handover `NOT_STARTED`/not ready |
| `project completed` | 0 positive claim | Project closure `PENDING`; penggunaan istilah hanya larangan/negasi |

### 8.2 Proposed terminology register

| Istilah Utama | Istilah Indonesia | Istilah English | Identifier Teknis | Allowed Alias | Forbidden/Stale Alias |
|---|---|---|---|---|---|
| Fitur canonical | Fitur canonical | Canonical Feature | `FeatureId`, `FEAT-*` | Fitur kanonis hanya bila standard bahasa mengizinkan; tetap gunakan ID asli | Feature baru tanpa D03; “546 fitur aktif” |
| Primary Tool surface | 33 Kartu Tool PDF Utama | 33 Primary PDF Tool Cards | `PRIMARY_TOOL` | 33 Core Tool Cards; 33 Core | 33 endpoint; 33 service; count selain 33 tanpa revision |
| Sign PDF | Tanda tangani PDF secara visual | Sign PDF / Visual Sign PDF | `FEAT-SIGN-013` | Tanda tangan visual; paraf/stamp visual | PKI Sign PDF; certificate signing; Digital Signature included |
| Digital Signature PKI | Tanda tangan digital berbasis sertifikat/PKI | Certificate-based Digital Signature | Tidak ada Feature ID current | PKI digital signing | Menyamakan dengan visual Sign PDF |
| Local-First | Utamakan pemrosesan/penyimpanan lokal bila aman dan mampu | Local-First | Execution profile/policy | Capability-aware Local-First | Offline-only; always-local tanpa policy |
| Offline-First | Tetap berguna saat offline sesuai capability dan valid grant | Offline-First | Offline/session/job policies | Offline-capable | Sinonim Local-First; bypass online authorization |
| REST authority | State REST/persisted menjadi sumber berwenang | REST/persisted state authority | REST resource + revision | REST source of truth | SignalR source of truth; UI/cache authority |
| SignalR notification | Notifikasi realtime | Realtime notification | `/hubs/activity`, versioned envelope | SignalR event | Command channel; authoritative state |
| Job waiting state | Menunggu koneksi/sumber daya | Waiting for connection/resource | `WAITING_CONNECTION`, `WAITING_RESOURCE` | Status user-facing yang disederhanakan | `PAUSED`; `JobWaiting` sebagai state |
| Job waiting event | Event Job sedang menunggu | Job waiting notification | `JobWaiting` | Waiting notification | State ketiga selain dua waiting state |
| Upload identity | ID siklus hidup transfer | Upload ID | JSON `uploadId`; resource ID | Upload session ID | File authority; storage key |
| File execution reference | Referensi file eksekusi bertipe dan opaque | File reference | JSON `fileRef` | Typed file reference | Raw path; storage key; authorization token |
| FINAL document | Dokumen final untuk scope baseline yang disebut | Final document | `FINAL` + lifecycle qualifier | FINAL pre-coding/pre-implementation setelah vocabulary disahkan | Implemented; tested; release-ready tanpa evidence |
| UAT specification | Spesifikasi UAT | UAT specification | D13/test IDs | UAT baseline | UAT completed/passed tanpa execution record |
| Production environment | Lingkungan Production | Production environment | `Production` | Produksi pada narasi Indonesia bila didefinisikan | Current Production untuk topology yang masih TBD |
| Handover | Alih kelola/serah terima yang diterima dengan bukti | Handover | Gate/evidence/acceptance IDs | Serah terima | Pengiriman link; documentation FINAL; handover completed tanpa sign-off |
| Hostinger KVM 2 | Kandidat node sementara non-Production | Temporary non-Production host | `SINGLE_NODE_TEMPORARY` | Limited Dev/QA/Pilot host | Production baseline; HA topology |
| Support/Superadmin | Profil permission Support | Support permission profile | Permission/policy scope | Superadmin sebagai istilah user-facing | Formal unlimited role; God Mode |
| Riwayat | Aktivitas pribadi pengguna | History | History resources | User History | Enterprise Audit; telemetry |
| Jejak Audit | Bukti Enterprise Audit | Audit Trail / Enterprise Audit | Audit event/resources | Audit | History/log/telemetry sebagai sinonim |

Rekomendasi bahasa: gunakan **“sumber berwenang”** sebagai istilah naratif utama. `source of truth` boleh dipertahankan dalam quote, nama prinsip teknis, atau istilah English column.

---

## 9. Language audit

### 9.1 Overall result

Narasi utama secara dominan menggunakan Bahasa Indonesia dan identifier teknis tetap English. Nama teknologi seperti ASP.NET Core, SignalR, SQL Server, Entity Framework Core, OpenAPI, SHA-256, REST, dan PKI tidak diterjemahkan secara keliru.

Hasil keseluruhan: **PARTIAL — substantively understandable, editorial normalization required**.

### 9.2 Findings

1. D05 baris adendum Sign PDF memakai kalimat naratif penuh berbahasa Inggris: “`Sign PDF means ...`”. Identifier dapat tetap English, tetapi penjelasan seharusnya Bahasa Indonesia.
2. D06 memuat kalimat naratif penuh seperti “`Local Recycle Bin ... are not server API resources by default`” dan “`Document 07 owns physical database design`”. Ini bukan identifier teknis semata.
3. D13–D19 dan Master sering mencampur kata `actual`, `current`, `owner`, `risk`, `acceptance`, `missing`, `required`, `and`, atau seluruh frasa English ke dalam kalimat Indonesia.
4. `Document` dan `Dokumen`, `Production` dan `Produksi`, `authority`, `source of truth`, `sumber kebenaran`, serta `sumber berwenang` dipakai bergantian tanpa terminology rule tunggal.
5. D18–D19 memakai heading title tanpa pemisah em dash (`# 18 APPLICATION CARTOGRAPHY`, `# 19 HANDOVER CHECKLIST`), sedangkan D01–D17 memakai `# NN — ...`.
6. Master menyatakan “Audit akhir revisi bahasa Documents 01–10: SELESAI — 10/10 PASS”. Berdasarkan contoh D05–D06, klaim ini tidak dapat direproduksi bila kriterianya adalah seluruh narasi utama Bahasa Indonesia.

### 9.3 Allowed English

English tetap tepat untuk:

- Feature/Requirement/Decision IDs;
- endpoint, JSON field, enum, state, event, permission, schema/table/column;
- class/interface/package/tool/product names;
- exact status code atau protocol term;
- glossary English column;
- code block yang merupakan contract atau identifier.

Controlled language revision tidak boleh mengubah requirement, decision, status semantics, Feature ID, contract, permission, atau architecture.

---

## 10. Heading/format audit

| Check | Result | Detail |
|---|---|---|
| Duplicate exact headings | PASS | 0 duplicate heading pada setiap D01–D19 dan Master |
| Main numeric sequence | PASS | Sequence utama pada setiap dokumen tidak mempunyai lompatan atau duplicate number |
| Heading hierarchy | PARTIAL | D02 memakai H2 untuk bagian awal lalu H1 mulai Bagian 6; D04 memakai H2 untuk Bagian 1–2 lalu H1 mulai Bagian 3 |
| Heading title style | PARTIAL | D18–D19 tidak memakai pola em dash yang digunakan D01–D17 |
| Code blocks | PASS | Seluruh backtick/tilde fence berpasangan menurut pemeriksaan struktural |
| Markdown table structure | PASS | Tidak ditemukan mismatch jumlah kolom pada recognized table blocks |
| Table numbering | GAP | Tidak ada controlled Table ID/caption scheme lintas dokumen |
| Diagram numbering | PARTIAL | D18 mencantumkan `CART-DGM-001`; dokumen lain tidak memakai scheme lintas dokumen |
| Appendix | PARTIAL | Hanya D13 mempunyai heading `Lampiran`; tidak ada appendix naming/version convention lintas dokumen |
| Markdown links | N/A | Tidak ada Markdown link; karena itu tidak ada broken clickable link yang dapat diuji |
| Filename references | FAIL/PARTIAL | 22 filename-like references tidak resolve: 19 logical names + 3 historical physical names |
| Filename/version match | PASS | Semua current FINAL filename version cocok dengan metadata |
| Logical/physical filename match | PARTIAL | D01–D04 dan status blocks memakai logical filename tanpa mapping manifest |
| Date metadata | GAP | D01–D04 tidak mencantumkan tanggal dokumen |
| Status qualifier | PASS dengan normalization gap | Semua status tetap pra-implementasi; format `Versi: FINAL v1.0` vs separate Version/Status tidak seragam |

Table/diagram numbering tidak wajib diubah bila dokumen memang hanya dibaca sebagai file tunggal. Jika tabel/diagram akan direferensikan lintas dokumen atau di-render ke PDF, ID stabil dan caption register menjadi kebutuhan configuration control.

---

## 11. False-status audit

### 11.1 Evidence-backed status

| Claim/status area | Evidence found | Audit result |
|---|---|---|
| Documents 01–19 FINAL | 19 files exist; versions/hash captured | TRUE untuk document baseline availability |
| Documentation baseline “complete 19/19” | Semua 19 tersedia | TRUE secara structural count; belum sama dengan approved integrity baseline |
| Implementation complete | Source/build/runtime evidence tidak tersedia | Tidak diklaim; actual tetap pending |
| Testing executed | D11–D12 adalah specification; no executable result | Tidak diklaim sebagai actual PASS |
| UAT accepted/completed | D13 menyatakan execution belum direkam; Master `NOT_RUN` | Tidak diklaim; PENDING |
| Production deployed/ready | D17 dan Master menyatakan belum `PRODUCTION_READY` | Tidak diklaim; PENDING |
| Handover accepted/completed | D19 `NOT_STARTED`/`NOT_HANDOVER_READY` | Tidak diklaim; PENDING |
| Project closed/completed | D19/Master `PENDING` dan melarang `PROJECT COMPLETED` | Tidak diklaim; PENDING |

### 11.2 Ambiguous status language requiring qualification

- D04 memakai `production-ready` sebagai sasaran desain. Ubah secara editorial menjadi “dirancang untuk mendukung Production readiness” agar tidak terbaca sebagai current readiness.
- D05, D06, dan D18 memakai `PASS` pada consistency/source/design checks. Prefix hasil menjadi `DESIGN_CONSISTENCY_PASS`, `SOURCE_VALIDATED`, atau `DOCUMENT_REVIEW_PASS`; jangan memakai PASS tanpa evidence scope.
- D17 `12/12 DESIGN REVIEW PASS` sudah memisahkan operational exercise `BELUM` dan dapat dipertahankan.
- D19 `GATE-0 Documentation Baseline Complete = PASS` didukung oleh keberadaan 19 file, tetapi belum membuktikan owner approval, unique canonical paths, immutable Master snapshots, atau clean language/integrity audit. Label sebaiknya menyebut `STRUCTURAL_AVAILABILITY_PASS — FORMAL_BASELINE_ACCEPTANCE_PENDING` sampai remediation selesai.
- Master `10/10 language audit PASS` tidak reproducible dan merupakan status audit yang paling perlu dikoreksi atau dilengkapi evidence/kriteria.

Tidak ditemukan false positive claim bahwa aplikasi atau proyek selesai. Risiko utama adalah **status dokumentasi dibaca melampaui scope** dan penggunaan PASS tanpa qualifier.

---

## 12. Proposed controlled revisions

Revisi berikut tidak mengubah business meaning, Feature ID, architecture, contract, atau keputusan:

| Priority | Controlled revision | Affected assets | Acceptance evidence |
|---|---|---|---|
| P0 | Terbitkan satu Baseline Manifest berisi exact canonical path, version/content revision, status, date, parent hash, SHA-256, owner/reviewer/approver, duplicate/supersession | D01–D19 + Master | Independent hash recheck 20/20; one current path per Document ID |
| P0 | Tandai handoff snapshot D01–D05 sebagai `SUPERSEDED_SNAPSHOT`; jangan biarkan search/index menganggapnya current | Handoff folder | Archive manifest; read-only marker; current replacement path/hash |
| P0 | Buat immutable snapshot Master v4.2 dan pulihkan/rekonstruksi v3.4/v4.0/v4.1 bila wajib sebagai parent evidence | Master chain | Versioned filenames + hashes + supersession chain |
| P0 | Normalisasi status language audit; jalankan ulang D01–D19 berdasarkan kriteria ID-1.0 | Master + D01–D19 | Finding register, reviewer, exact hash, 19/19 result |
| P1 | Tambahkan metadata standard: Document ID, Logical Name, Physical Filename, Version, Content Revision, Date, Status, Parent IDs/hashes, Owner, Reviewer, Approver | D01–D19 | Schema completeness check |
| P1 | Map 19 logical `.md` names ke canonical physical path; label tiga D04 old-version refs sebagai historical/superseded | Manifest + D01/D04 references | 0 unresolved current reference |
| P1 | Nyatakan `PRE-CODING` dan `PRE-IMPLEMENTATION` sebagai alias atau pilih satu canonical lifecycle term | Manifest/glossary | Terminology review |
| P1 | Qualify every `PASS` with evidence scope; bedakan document/design/source check dari runtime/test/UAT/Production | D04–D19 + Master | False-status lint passes |
| P1 | Tambahkan revision history untuk D05 dan controlled changelog untuk D06–D19 | D05–D19 | Version chain completeness |
| P2 | Standardisasi heading level/title punctuation dan Version/Status header format | D01–D19 | Heading lint; no semantic diff |
| P2 | Tetapkan Table/Diagram/Appendix ID convention bila artefak akan direferensikan atau dirender lintas dokumen | D01–D19 | Register and link validation |
| P2 | Beri stable documentation ID pada 18 D04 decision rows tanpa mengubah keputusan | D04/decision index | 18/18 mapping review |
| P2 | Jelaskan formula D03 `557 + 12 + 11 = 580 → dedup 550` | D03 | Count audit remains 550/550 |

Setiap revisi harus menghasilkan hash baru dan dicatat sebagai editorial/content revision. Jangan mengganti file current secara diam-diam.

---

## 13. Archive recommendations

1. Buat archive root terkendali, misalnya `docs/archive/`, dengan read-only archive manifest. Nama lokasi final mengikuti kebijakan Pertamina.
2. Pertahankan folder handoff 24 Agustus sebagai historical evidence, tetapi tambahkan marker yang menyatakan:
   - snapshot date;
   - `SUPERSEDED`;
   - tidak current;
   - replacement exact paths/hashes;
   - diketahui memuat angka lama 546 pada D02–D03.
3. Setelah canonical path disahkan, klasifikasikan root D07/D08 sebagai `ARCHIVE_CANDIDATE_IDENTICAL_DUPLICATE`; jangan hapus sebelum retention/record decision.
4. Pindahkan atau tandai D06–D19 working files sebagai `SUPERSEDED_WORKING`; working file tidak boleh berada pada default current search/index tanpa status marker.
5. Tandai Master v2.2 dan v2.5 sebagai superseded. Simpan immutable Master snapshot setiap published version; `MASTER_PROJECT_SUMMARY.md` boleh menjadi current pointer/alias.
6. Agent reports, integration audits, dan final-audit reports tetap disimpan sebagai `NON_AUTHORITATIVE_INPUT/EVIDENCE`, terpisah dari canonical FINAL set.
7. Archive manifest minimum memuat `ArchiveItemId`, original path, filename, version, SHA-256, status, snapshot date, source, replaced-by path/hash, reason, owner, retention class, legal hold, access, dan disposal approval.
8. Tidak ada penghapusan sebelum owner, records management, legal/security, dan retention authority memberi keputusan.

---

## 14. Confirmation gaps

| Gap | Required decision/confirmation |
|---|---|
| Canonical document owner/reviewer/approver | Tetapkan fungsi dan principal resmi untuk D01–D19 dan Master |
| Canonical physical path | Sahkan root vs `docs/<domain>` dan kebijakan current alias |
| Document date D01–D04 | Tetapkan effective/publication date tanpa mengambil file-system timestamp sebagai pengganti |
| Content/editorial revision policy | Tentukan apakah language-only change menaikkan version atau memakai `ContentRevision` terpisah |
| Master snapshot policy | Konfirmasi retensi dan pemulihan v3.4/v4.0/v4.1; terbitkan immutable v4.2 |
| Historical D04 parents | Tentukan apakah v3.3/v1.3/v1.1 harus dipulihkan sebagai evidence atau cukup dicatat superseded |
| Logical document names | Konfirmasi logical name tetap dipakai dan wajib dimap ke physical filename |
| Auto-locked decision approval | Tentukan siapa yang mengesahkan decision register; status AUTO-LOCKED bukan bukti external approval |
| Language acceptance | Sahkan terminology register dan kriteria ID-1.0 untuk narasi campuran |
| Table/diagram/appendix IDs | Tentukan apakah required untuk publishing/PDF/cross-document citation |
| Archive retention/disposal | Tetapkan retention, legal hold, access, dan disposal authority |
| Formal baseline acceptance | Tetapkan gate, evidence, dan signature yang mengubah structural availability menjadi accepted configuration baseline |

---

## 15. Risks

| Risk ID | Risk | Impact | Severity | Recommendation |
|---|---|---|---|---|
| `R-DOC-001` | Same filename/version D01–D05 mempunyai divergent hashes | Reader/build dapat memakai baseline salah | Critical | Unique current path + hash manifest + superseded marker |
| `R-DOC-002` | Archive snapshot memuat active-looking 546 | Feature count regression | High | Exclude archive from current indexing; lint active baseline |
| `R-DOC-003` | D07/D08 identical duplicate pada dua path | Ambiguous edit target dan future divergence | High | Canonicalize path; archive/pointer duplicate |
| `R-DOC-004` | Master v3.4/v4.0/v4.1 missing | Parent baseline tidak reproducible | High | Restore or document loss; immutable snapshots |
| `R-DOC-005` | Mutable current Master tanpa versioned v4.2 file | Summary dapat berubah tanpa trace | High | Publish immutable v4.2 snapshot + current alias |
| `R-DOC-006` | 22 filename-like references tidak resolve | Broken traceability/automation | High | Logical-to-physical manifest and historical archive mapping |
| `R-DOC-007` | Language PASS tidak reproducible | False governance assurance | High | Re-audit with measurable criteria/evidence |
| `R-DOC-008` | PASS labels tidak menyebut evidence scope | Design review dapat dibaca sebagai test/UAT/Production PASS | High | Qualify all statuses |
| `R-DOC-009` | D01–D04 tidak mempunyai date | Effective baseline chronology lemah | Medium | Controlled date metadata |
| `R-DOC-010` | Owner/reviewer/approver belum ditetapkan | Tidak ada accountable acceptance/change authority | High | Ownership register and signatures |
| `R-DOC-011` | Changelog D05–D19 tidak seragam | Same-version drift sulit diaudit | Medium | Standard revision history/content revision |
| `R-DOC-012` | D04 18 decisions tanpa stable ID | Weak decision traceability | Medium | Controlled documentation IDs |
| `R-DOC-013` | PRE-CODING vs PRE-IMPLEMENTATION tidak didefinisikan | Lifecycle interpretation drift | Medium | Canonical lifecycle vocabulary |
| `R-DOC-014` | Heading/table/diagram conventions berbeda | Render/citation automation rapuh | Low/Medium | Formatting standard and lint |
| `R-DOC-015` | Structural 19/19 dapat dianggap formal acceptance | Premature implementation/handover governance decision | High | Separate availability, integrity verification, approval, readiness |
| `R-DOC-016` | Working/agent/audit files berada dekat current FINAL | Search or AI retrieval may elevate non-authoritative content | High | Source classification metadata and search scopes |

---

## 16. Suggested Baseline Manifest content

### 16.1 Suggested normative text

> **PERTAMINA OTO Controlled Documentation Baseline Manifest** menetapkan satu dan hanya satu current canonical file untuk setiap Document ID. Status FINAL hanya berlaku pada ruang lingkup dokumentasi yang dinyatakan dan tidak membuktikan implementation, test execution, UAT acceptance, Production deployment, handover acceptance, atau project closure. Setiap manifest entry wajib mengikat logical identity, physical path, version, content revision, document date, lifecycle status, parent versions/hashes, file size, SHA-256, source-of-truth domain, owner, reviewer, approver, approval evidence, supersession, archive location, access classification, dan open gap. Perubahan byte, termasuk revisi bahasa, menghasilkan content revision/hash baru dan tidak boleh menimpa evidence lama secara diam-diam.

> Baseline Feature berwenang adalah 550 Feature ID unik pada Document 03 v1.3. Primary Tool surface adalah 33 Primary PDF Tool Cards. `546` adalah stale baseline dan dilarang pada current set. Sign PDF adalah visual signature/paraf non-PKI; certificate/PKI Digital Signature tetap di luar scope. REST/persisted state adalah authority; SignalR hanya notification. `PAUSED` bukan Job state; waiting hanya `WAITING_CONNECTION` atau `WAITING_RESOURCE`, sedangkan `JobWaiting` adalah event. Hostinger KVM 2 bukan Production baseline.

### 16.2 Required manifest fields

```text
BaselineId
BaselineVersion
BaselineStatus
ManifestCreatedAtUtc
ManifestOwner / Reviewer / Approver
ApprovalEvidenceRef

DocumentId
LogicalDocumentName
CanonicalPhysicalPath
Filename
Version
ContentRevision
LanguageStandard
DocumentStatus
DocumentDate
EffectiveFrom / EffectiveUntil
SourceOfTruthDomain
ParentDocumentIds
ParentVersions
ParentSha256
ChildDocumentIds
FileSizeBytes
Sha256
Current
DuplicateStatus
Supersedes / ReplacedBy
ArchiveLocation
AccessClassification
Owner / Reviewer / Approver
ApprovalEvidenceRef
OpenGaps
Notes
```

### 16.3 Baseline validation rules

```text
1. Exactly one Current=YES entry per DocumentId.
2. CanonicalPhysicalPath must exist and match Filename, FileSizeBytes, and Sha256.
3. Filename version, metadata version, manifest version, and status must agree.
4. Every parent must resolve to an immutable version/hash or an approved archive exception.
5. Working, agent-output, audit, snapshot, and archive files cannot be Source of Truth.
6. Divergent duplicate with same filename/version blocks integrity acceptance.
7. Current D03 must contain exactly 550 rows and 550 unique Feature IDs.
8. Primary Tool mapping must contain exactly 33 card rows.
9. Active baseline must contain zero stale 546 references.
10. FINAL must always carry a lifecycle/readiness qualifier.
11. UAT/Production/handover/project status requires separate evidence and human approval.
12. Language audit status must reference exact file hashes, criteria, findings, reviewer, and date.
13. No current reference may resolve only to a working or superseded file.
14. Archive/disposal follows approved retention and legal-hold policy.
```

### 16.4 Proposed current-set statement

```text
Documentation availability : 19/19 FINAL files present
Master current index        : v4.2
Canonical Features          : 550 unique IDs
Primary Tool Cards          : 33
Implementation status       : PENDING / evidence not available
UAT execution               : NOT_RUN
Production readiness        : NO
Actual handover             : NOT_STARTED
Project closure             : PENDING
Integrity acceptance        : PENDING controlled remediation and human approval
```

Manifest tidak boleh menyimpan password, token, secret, private key, bearer link, user document content, OCR text, signature payload, atau Production data.
