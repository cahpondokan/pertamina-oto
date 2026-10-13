# BOOT-001 DECISION AND EVIDENCE INPUT PACK
## PERTAMINA OTO

> Versi formulir: 1.0  
> Tanggal persiapan: 2 September 2026  
> Status: **CANDIDATE — NON-AUTHORITATIVE — BELUM DISETUJUI**  
> Tujuan: mengumpulkan keputusan dan bukti minimum untuk penilaian `BOOT-001`.  
> Aturan pengisian: nilai yang belum dikonfirmasi harus tetap `TBC`. Jangan menaruh token, password, secret, atau data Production di dokumen ini.

---

# 1. Cara Menggunakan Paket Ini

1. Pemilik fungsi yang berwenang mengisi atau melampirkan keputusan pada setiap bagian.
2. Reviewer memeriksa bukti, bukan hanya isian teks.
3. Bukti harus menyebut tanggal, lingkungan, principal, repo/branch, serta hasil uji yang relevan.
4. Approver memberi keputusan hanya setelah seluruh item wajib berstatus `PASS`.
5. Jika artefak baseline berubah, versi dan SHA-256 harus dihitung ulang sebelum acceptance.

Dokumen ini adalah formulir persiapan. Isian di sini tidak otomatis memberi akses, menetapkan owner, mengubah branch policy, menerima baseline, atau mengizinkan coding.

---

# 2. Ringkasan Keputusan BOOT-001

| Field | Isian |
|---|---|
| Task ID | `BOOT-001` |
| Keputusan yang diminta | Repository authority, access, ownership, dan accepted baseline |
| Repository/application authority | `TBC` |
| Reviewer | `TBC` |
| Approver | `TBC` |
| Tanggal review | `TBC` |
| Lokasi paket evidence | `TBC` |
| Hasil | `TBC` (`PASS` / `REVISION_REQUIRED` / `BLOCKED`) |
| Catatan keputusan | `TBC` |

Aturan: `BOOT-001` hanya dapat dinilai `PASS` jika seluruh bukti wajib pada §11 tersedia dan tervalidasi. Persetujuan prompt orkestrasi tidak sama dengan acceptance `BOOT-001`.

---

# 3. Formulir Repository Azure DevOps Resmi

| Field | Nilai yang harus dikonfirmasi | Evidence minimum |
|---|---|---|
| Azure DevOps organization | `TBC` | Referensi organization yang dapat diverifikasi |
| Azure DevOps project | `TBC` | Project ID/nama resmi dan bukti akses |
| Repository name | `TBC` | Repo ID/nama resmi |
| Repository URL | `TBC` | URL tanpa credential/token |
| Default branch | `TBC` | Bukti setting default branch |
| Source-of-truth statement | `TBC` | Pernyataan tertulis bahwa repo ini adalah authority source aplikasi |
| Repository owner/function | `TBC` | Penetapan fungsi/principal dan ruang lingkup wewenang |
| Application owner/function | `TBC` | Penetapan fungsi/principal dan release authority |
| Batas penggunaan repo prototipe UI/UX | `TBC` | Pernyataan apakah prototipe hanya referensi, sumber migrasi, atau tidak dipakai |
| Tanggal berlaku | `TBC` | Approval record |
| Evidence location | `TBC` | Tautan/path evidence yang dapat ditinjau |

Pemeriksaan reviewer:

```text
[ ] Repo dapat diidentifikasi tanpa keraguan
[ ] Default branch cocok dengan policy yang disetujui
[ ] Source of truth aplikasi dinyatakan eksplisit
[ ] Repo prototipe tidak dianggap otomatis sebagai repo resmi
[ ] URL/evidence tidak memuat secret
```

---

# 4. Formulir Owner, Reviewer, dan Approver

| Fungsi wajib | Principal/unit | Ruang lingkup wewenang | Deputy/backup | Evidence penetapan | Status |
|---|---|---|---|---|---|
| Product/Application Owner | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| Technical Owner | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| Repository Integrator | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| Repository Owner/Admin | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| Build/DevOps Owner | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| Security Reviewer | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| Dependency/License Reviewer | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| Baseline Reviewer | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| BOOT-001 Approver | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |

Ketentuan:

- Agen/AI tidak boleh diisi sebagai owner, reviewer, atau approver organisasi.
- Satu principal boleh memegang lebih dari satu fungsi hanya jika pemisahan tugas tetap sesuai policy dan disetujui authority.
- Jika fungsi digabung, alasan dan kontrol penggantinya dicatat sebagai `TBC` sampai disetujui.

---

# 5. Matriks Akses Least-Privilege

Isi satu baris per principal atau service identity. Jangan menyalin token atau credential ke evidence.

| Principal/service identity | Fungsi | Resource/scope | Izin yang diperlukan | Izin yang dilarang/tidak diperlukan | Grant owner | Masa berlaku/review | Evidence | Status |
|---|---|---|---|---|---|---|---|---|
| `TBC` | Developer | `TBC` | Read repo; create/push task branch sesuai policy | Bypass policy; direct push ke protected branch; manage permissions; force push | `TBC` | `TBC` | `TBC` | `TBC` |
| `TBC` | Reviewer | `TBC` | Read repo/PR/build evidence; review/vote sesuai policy | Bypass policy; mengubah evidence; grant permission | `TBC` | `TBC` | `TBC` | `TBC` |
| `TBC` | Repository Integrator | `TBC` | Integrasi sesuai branch/PR authority yang disetujui | Self-approval jika dilarang policy; bypass tanpa emergency record | `TBC` | `TBC` | `TBC` | `TBC` |
| `TBC` | Build service identity | `TBC` | Read source; publish report/artifact ke lokasi yang disetujui | Interactive login; broad project admin; Production deploy pada PR CI | `TBC` | `TBC` | `TBC` | `TBC` |
| `TBC` | Security reviewer | `TBC` | Read security reports/waivers sesuai klasifikasi | Mengubah source atau hasil scan tanpa workflow | `TBC` | `TBC` | `TBC` | `TBC` |

## 5.1 Access Test dan Negative Test

| Test ID | Principal | Uji | Hasil yang diharapkan | Hasil aktual | Tanggal/env | Evidence | Status |
|---|---|---|---|---|---|---|---|
| `ACC-POS-001` | `TBC` | Clone/fetch repo resmi | Berhasil | `TBC` | `TBC` | `TBC` | `TBC` |
| `ACC-POS-002` | `TBC` | Membuat dan push task branch yang diizinkan | Berhasil | `TBC` | `TBC` | `TBC` | `TBC` |
| `ACC-POS-003` | `TBC` | Membuat PR dan membaca hasil validation | Berhasil sesuai fungsi | `TBC` | `TBC` | `TBC` | `TBC` |
| `ACC-NEG-001` | `TBC` | Direct push ke protected branch | Ditolak | `TBC` | `TBC` | `TBC` | `TBC` |
| `ACC-NEG-002` | `TBC` | Bypass required review/check tanpa emergency authority | Ditolak | `TBC` | `TBC` | `TBC` | `TBC` |
| `ACC-NEG-003` | `TBC` | Mengubah repo permission/branch policy oleh non-admin | Ditolak | `TBC` | `TBC` | `TBC` | `TBC` |
| `ACC-NEG-004` | `TBC` | Akses build service identity di luar scope | Ditolak | `TBC` | `TBC` | `TBC` | `TBC` |

Jika policy mengharuskan metode uji berbeda, ganti skenario hanya melalui keputusan Repository/Security Owner dan simpan alasan perubahan.

---

# 6. Exact SDK dan Build Agent

Nilai SDK lokal yang teramati bukan keputusan resmi. Nilai exact hanya diisi setelah disetujui Technical/Build authority.

| Field | Nilai disetujui | Evidence minimum | Reviewer | Status |
|---|---|---|---|---|
| Target .NET family | `TBC` | Keputusan arsitektur/toolchain | `TBC` | `TBC` |
| Exact .NET SDK patch | `TBC` | Output SDK pada developer dan build agent + approval | `TBC` | `TBC` |
| `global.json` version | `TBC` | Nilai sama dengan keputusan SDK | `TBC` | `TBC` |
| Roll-forward policy | `TBC` | Keputusan tertulis dan hasil compatibility test | `TBC` | `TBC` |
| Build pool | `TBC` | Referensi pool yang disetujui | `TBC` | `TBC` |
| Build agent image/version | `TBC` | Image/capability inventory | `TBC` | `TBC` |
| OS/architecture | `TBC` | Agent job evidence | `TBC` | `TBC` |
| Required workloads/tools | `TBC` | Capability check tanpa instalasi diam-diam | `TBC` | `TBC` |
| Support/patch ownership | `TBC` | Owner dan review cadence | `TBC` | `TBC` |
| Smoke job result | `TBC` | Log job sederhana yang terikat run/agent | `TBC` | `TBC` |

---

# 7. NuGet, Source Mapping, dan Dependency Admission

## 7.1 Feed dan Source Mapping

| Source key | Feed URL/nama | Jenis | Package pattern yang diizinkan | Credential mechanism | Owner | Availability test | Status |
|---|---|---|---|---|---|---|---|
| `TBC` | `TBC` | `TBC` (internal/public proxy/other) | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |

Policy minimum yang harus diputuskan:

```text
NuGet.Config authority           : TBC
Source mapping rule              : TBC
Unapproved source behavior       : TBC (target: ditolak)
Credential storage/mechanism     : TBC
Offline/cache/fallback policy    : TBC
Feed outage owner/escalation     : TBC
Package signature policy         : TBC
Vulnerability severity policy    : TBC
License allow/deny policy        : TBC
Waiver authority dan masa berlaku: TBC
```

## 7.2 Initial Dependency Admission

Tambah satu baris per package/provider sebelum masuk `Directory.Packages.props` atau restore formal.

| Package/provider | Exact version | Tujuan | Source | License | Vulnerability result/date | Data/network impact | Owner | Security/license decision | Evidence | Status |
|---|---|---|---|---|---|---|---|---|---|---|
| `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |

Tidak ada package yang dianggap disetujui hanya karena tersedia di internet, sudah terpasang lokal, atau dibutuhkan oleh rancangan awal.

---

# 8. Branch Protection, PR, dan Pipeline Authority

| Kontrol | Nilai/keputusan | Authority | Evidence | Status |
|---|---|---|---|---|
| Protected/default branch | `TBC` | `TBC` | `TBC` | `TBC` |
| Direct push | `TBC` | `TBC` | `TBC` | `TBC` |
| Force push/delete branch | `TBC` | `TBC` | `TBC` | `TBC` |
| Minimum reviewer/quorum | `TBC` | `TBC` | `TBC` | `TBC` |
| Required reviewer groups | `TBC` | `TBC` | `TBC` | `TBC` |
| Self-approval rule | `TBC` | `TBC` | `TBC` | `TBC` |
| Comment/work-item resolution | `TBC` | `TBC` | `TBC` | `TBC` |
| Build validation | `TBC` | `TBC` | `TBC` | `TBC` |
| Required test/security checks | `TBC` | `TBC` | `TBC` | `TBC` |
| Zero-test behavior | `TBC` (target: gagal) | `TBC` | `TBC` | `TBC` |
| Unapproved dependency/source behavior | `TBC` (target: gagal) | `TBC` | `TBC` | `TBC` |
| Pipeline definition authority | `TBC` | `TBC` | `TBC` | `TBC` |
| Service identity/service connection | `TBC` | `TBC` | `TBC` | `TBC` |
| PR CI deployment scope | `TBC` (Phase 0: tidak ada Production deploy) | `TBC` | `TBC` | `TBC` |
| Artifact/report storage | `TBC` | `TBC` | `TBC` | `TBC` |
| Retention/immutability | `TBC` | `TBC` | `TBC` | `TBC` |
| Emergency/break-glass flow | `TBC` | `TBC` | `TBC` | `TBC` |
| Negative canary proof | `TBC` | `TBC` | `TBC` | `TBC` |

Minimum negative canary: buktikan required validation benar-benar memblokir PR ketika test/check wajib sengaja dibuat gagal di lingkungan terkontrol. Canary tidak boleh melemahkan expected behavior atau tertinggal pada branch resmi.

---

# 9. Acceptance Baseline Terikat Versi dan Hash

## 9.1 Identitas Paket yang Direview

Bekukan daftar ini sebelum meminta acceptance. Jangan menerima file yang masih berubah.

| Artefak | Versi/revisi | SHA-256 | Tanggal hash | Lokasi/path | Reviewer result |
|---|---|---|---|---|---|
| Baseline ID `PERTAMINA-OTO-DOCS-BASELINE-1.0` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| Documentation baseline audit | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| Baseline manifest hasil rekonsiliasi/addendum | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| Cross-document consistency matrix | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| Traceability completeness register | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| Implementation readiness | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| Implementation roadmap | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| PoC master plan | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| Repository bootstrap plan | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| Open item register | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| Change control register | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| `MASTER_PROJECT_SUMMARY.md` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |

Catatan rekonsiliasi saat paket ini disiapkan: manifest existing masih mencatat Master v4.3, sedangkan audit terbaru menemukan Master v4.4. Nilai final harus diambil dari manifest hasil rekonsiliasi/addendum yang sudah direview; jangan menyalin nilai lama secara otomatis.

## 9.2 Pernyataan Acceptance

```text
Baseline ID                     : PERTAMINA-OTO-DOCS-BASELINE-1.0
Daftar artefak/versi/hash       : TBC (lihat §9.1)
Open item/residual risk diterima: TBC
Scope acceptance                : TBC
Hal yang tidak termasuk         : TBC
Owner                           : TBC
Reviewer                        : TBC
Approver                        : TBC
Tanggal/waktu                   : TBC
Evidence approval               : TBC
Keputusan                       : TBC (ACCEPTED / REVISION_REQUIRED / REJECTED)
```

Acceptance tidak berarti seluruh open item selesai, aplikasi sudah dibuat, UI/UX sudah final, UAT selesai, atau Production siap. Scope keputusan harus ditulis eksplisit.

---

# 10. Evidence Index

Gunakan indeks ini untuk menghindari bukti yang tercecer atau tidak dapat ditelusuri.

| Evidence ID | Kategori | Deskripsi | Tanggal/env | Principal/owner | Repo/commit/run | Lokasi | Klasifikasi/sanitasi | Reviewer | Status |
|---|---|---|---|---|---|---|---|---|---|
| `BOOT001-EV-001` | Repository authority | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| `BOOT001-EV-002` | Ownership | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| `BOOT001-EV-003` | Access test | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| `BOOT001-EV-004` | Negative access test | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| `BOOT001-EV-005` | SDK/build agent | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| `BOOT001-EV-006` | NuGet/dependency | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| `BOOT001-EV-007` | Branch/PR/pipeline | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |
| `BOOT001-EV-008` | Baseline acceptance | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` | `TBC` |

Bukti screenshot/log harus disanitasi. Simpan evidence di lokasi yang disetujui dan pastikan reviewer dapat mengaksesnya tanpa memperoleh izin berlebih.

---

# 11. Checklist Kelengkapan

## 11.1 Wajib untuk Penilaian BOOT-001

```text
[ ] Azure DevOps organization/project/repo/default branch resmi terisi
[ ] Repository/application authority tertulis dan dapat diverifikasi
[ ] Product, Technical, Repository Integrator, Repository/Build, Security,
    baseline reviewer, dan approver ditetapkan
[ ] Matriks least-privilege disetujui
[ ] Positive access test berhasil
[ ] Negative/over-scope access test ditolak sesuai harapan
[ ] Baseline package dibekukan dan terikat versi + SHA-256
[ ] Mismatch Master/manifest sudah diselesaikan melalui revisi/addendum terkontrol
[ ] Human acceptance menyebut Baseline ID, scope, open item, owner, reviewer,
    approver, tanggal, dan evidence
[ ] Evidence index lengkap, dapat diakses reviewer, dan bebas secret
```

## 11.2 Wajib Sebelum Phase 0 Formal Dimulai

```text
[ ] Exact .NET SDK patch dan roll-forward disetujui
[ ] Build pool/image/capability disetujui dan smoke job terbukti
[ ] NuGet feeds/source mapping/credential mechanism disetujui
[ ] Initial dependency + license + vulnerability review disetujui
[ ] Protected branch/PR checks/pipeline authority disetujui
[ ] Service identity least-privilege dan artifact/report location disetujui
[ ] BOOT-001 dinyatakan PASS oleh human authority
```

---

# 12. Stop Conditions

Hentikan pekerjaan dan jangan mulai coding/scaffolding/restore formal jika salah satu kondisi berikut terjadi:

1. Repo resmi, default branch, atau source-of-truth masih `TBC` atau bertentangan.
2. Owner/reviewer/approver manusia belum ditetapkan atau authority tidak dapat dibuktikan.
3. Akses hanya berhasil karena broad admin permission, bypass, shared credential, atau secret yang tidak dikelola.
4. Negative access test gagal menolak akses berlebih.
5. Baseline yang akan diterima berubah setelah hash dihitung.
6. Manifest belum direkonsiliasi dengan Master aktual atau approval tidak terikat versi/hash.
7. Exact SDK/build agent belum disetujui atau hasil lokal berbeda dari CI tanpa disposition.
8. Feed/source mapping/dependency belum lolos security/license admission.
9. Branch/PR/pipeline policy dapat dilewati tanpa authority/evidence yang sah.
10. Evidence tidak dapat ditelusuri, tidak dapat direview, mengandung secret, atau berasal dari Production tanpa izin.
11. Ada permintaan untuk mengubah Dokumen 01–19 FINAL tanpa controlled revision.
12. Ada klaim bahwa planning approval, agent output, atau persetujuan prompt sama dengan human acceptance.

Jika stop condition terpicu, catat:

```text
Stop condition ID/referensi : TBC
Tanggal/waktu               : TBC
Temuan                      : TBC
Dampak                      : TBC
Owner tindakan              : TBC
Evidence                    : TBC
Keputusan lanjut            : TBC
```

---

# 13. Sign-off Penilaian

```text
Semua checklist wajib PASS : TBC
Stop condition aktif       : TBC
Residual risk/open item    : TBC
Reviewer recommendation    : TBC
Approver decision          : TBC (PASS / REVISION_REQUIRED / BLOCKED)
Effective date             : TBC
Approval evidence          : TBC
```

Sampai blok ini lengkap dan bukti telah diverifikasi, status tetap:

```text
BOOT-001              : BLOCKED
PHASE 0 FORMAL        : NO-GO
APPLICATION CODING    : NO-GO
PRODUCTION ACTION     : NOT AUTHORIZED
```
