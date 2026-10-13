# Shared Agent Contract

Semua prompt Phase 0–11 mewajibkan kontrak ini. Agen harus membaca file ini sepenuhnya sebelum melakukan perubahan.

## 1. Urutan otoritas

1. Instruksi aktif Human Owner/Integrator yang sah.
2. Dokumen canonical 01–19 dan `MASTER_PROJECT_SUMMARY.md`.
3. Baseline audit, roadmap, register, dan keputusan phase yang sudah diterima manusia.
4. Prompt phase dan task card agen.

Jika dua sumber berwenang bertentangan dan tidak ada disposition resmi, hentikan bagian terdampak dan laporkan konflik. Jangan memilih nilai sendiri.

## 2. Placeholders wajib

Placeholder seperti `${PRODUCT_REPO}`, `${DEFAULT_BRANCH}`, `${TASK_BRANCH}`, `${WORKTREE}`, `${HUMAN_OWNER}`, `${HUMAN_INTEGRATOR}`, `${APPROVED_SDK}`, `${APPROVED_FEEDS}`, `${APPROVED_DEPENDENCIES}`, `${APPROVED_PROVIDER}` dan `${EVIDENCE_ROOT}` harus diisi dari keputusan/evidence resmi.

Placeholder yang belum terisi berarti pekerjaan tulis atau keputusan terkait **BLOCKED**. Agen boleh melakukan analisis read-only yang masih berguna, tetapi tidak boleh mengarang nilai.

## 3. Standar profesional

Setiap agen bekerja sebagai specialist senior/principal pada lane yang diberikan:

- telusuri alur nyata sebelum mengubah kode;
- gunakan exact Requirement ID, Feature ID, Decision ID dan Open Item ID;
- buat perubahan sekecil mungkin yang mencapai acceptance criteria;
- pertahankan behavior yang tidak termasuk scope;
- jangan melemahkan test, analyzer, security gate atau acceptance criteria agar hasil hijau;
- jangan menambahkan package/provider tanpa admission, license dan security evidence;
- jangan menaruh secret, token, password, document content atau raw sensitive path di source/log/evidence;
- local-only processing dilarang melakukan upload, public API, SignalR, server Job atau server document storage;
- jangan mengklaim UAT, Production readiness, handover atau approval manusia.

## 4. Isolasi dan ownership

- Satu task menggunakan satu branch dan satu worktree.
- Satu writer untuk root files, shared contracts dan registries pada satu waktu.
- Hanya edit `Allowed Paths`; `Do-Not-Touch Paths` bersifat mutlak.
- Jangan reset, checkout, rebase, force-push atau menghapus perubahan agen/pengguna lain.
- Agen pekerja tidak melakukan merge ke default branch.
- Konflik kontrak menghentikan downstream work dan dikirim ke Agent 0/Human Integrator.

## 5. Preflight wajib

Sebelum menulis, setiap agen harus:

1. membaca prompt, kontrak ini, dan dokumen phase yang ditunjuk;
2. memeriksa entry gate serta dependency;
3. memeriksa status branch/worktree dan perubahan existing;
4. mencatat baseline commit/hash dan allowed paths;
5. menyatakan asumsi, blocker dan test plan;
6. berhenti bila perubahan tidak dapat dilakukan tanpa melampaui scope.

## 6. Verification minimum

Pilih test sesuai perubahan, tetapi semua task implementasi minimal harus menyediakan:

- build target terdampak;
- unit/contract/architecture/integration test yang relevan;
- negative dan failure-path test untuk boundary berisiko;
- security/privacy check;
- performance/resource measurement bila ada budget;
- UI accessibility/responsive/visual evidence bila mengubah UI;
- offline/network-disabled proof untuk capability local-only;
- sanitized logs dan exact command/exit code.

Zero test discovery adalah kegagalan, bukan PASS.

## 7. Stop conditions

Segera berhenti pada bagian terdampak dan laporkan jika:

- membutuhkan approval, credential, secret, certificate atau data Production baru;
- requirement/provider/license/security boundary belum diputuskan;
- source file pengguna berisiko rusak atau tertimpa;
- test canonical bertentangan dengan implementasi;
- dependency atau API contract berubah tanpa owner;
- data sensitif terekspos;
- bukti tidak dapat disanitasi atau direproduksi;
- pekerjaan meminta deploy/merge/destructive action di luar otorisasi.

Jangan berhenti hanya karena pekerjaan sulit. Lanjutkan task independen yang aman sambil mengisolasi blocker.

## 8. Handoff wajib

Setiap agen mengembalikan ringkasan dengan format berikut:

```text
Agent / Task ID:
Objective:
Baseline commit/hash:
Branch / worktree:
Requirement / Feature / Decision IDs:
Files changed:
Behavior implemented:
Tests and exact results:
Security/privacy checks:
Performance/resource evidence:
Known limitations:
Open blockers and required human decision:
Evidence paths:
Recommended integration order:
```

Agent harus mengembalikan ringkasan, bukan dump log panjang. Log lengkap disimpan sebagai evidence yang disanitasi.

## 9. Kontrak Agent 0

Agent 0:

- membekukan baseline dan membuat task card sebelum dispatch;
- memilih hanya pekerjaan yang benar-benar independen untuk paralel;
- menunggu seluruh agen aktif pada checkpoint yang sama;
- memeriksa diff, ownership, test dan evidence setiap handoff;
- menolak perubahan di luar scope atau hasil tanpa bukti;
- mengintegrasikan menurut dependency order, bukan waktu agen selesai;
- menjalankan clean restore/build/test/scans setelah integrasi;
- meminta human merge authorization saat diwajibkan;
- membuat laporan phase berisi PASS/FAIL/BLOCKED per exit criterion.

`COMPLETE` hanya boleh digunakan jika seluruh exit criterion phase memiliki evidence. `BLOCKED` harus menyebut exact blocker, owner keputusan, pekerjaan aman yang sudah selesai, dan cara melanjutkan.

