# AGENT C — SECURITY, PRIVACY & DATA INCIDENT REPORT

**Task ID:** `DOC16-AGENT-C`  
**Status:** Rekomendasi specialist; keputusan final tetap milik user dan Integrator  
**Tanggal:** 2026-09-01  
**Target integrasi:** Document 16 — Incident & Support Guide  
**Sumber:** Document 01–15 FINAL

Laporan ini tidak menetapkan proses legal, regulator, forensik, atau notifikasi resmi Pertamina. Semua bagian yang memerlukan otoritas, kontak, waktu, klasifikasi resmi, atau prosedur enterprise ditandai untuk konfirmasi.

---

# 1. Security incident principles

1. **Safety first.** Hentikan perluasan dampak tanpa merusak sumber, bukti, atau data yang masih dapat dipulihkan.
2. **Least privilege tetap berlaku saat insiden.** Status investigator, Administrator, Support, Auditor, operator, atau developer tidak otomatis memberi akses document content, OCR content, signature asset, secret, secured file, atau data pengguna lain.
3. **Need-to-know dan data minimization.** Incident Record memuat fakta minimum yang diperlukan untuk klasifikasi, containment, pemulihan, dan keputusan resmi.
4. **Gunakan jalur resmi.** Containment, perubahan permission, session revocation, feature/provider disable, Job control, restore, dan recovery dilakukan melalui identity/application/control-plane/deployment process yang disetujui; bukan direct SQL, object-storage browsing, shell, atau perubahan manual Production.
5. **Evidence berorientasi append.** Bukti tidak diedit, dihapus, ditimpa, atau dibuat ulang untuk menyesuaikan narasi. Koreksi menjadi catatan tertaut baru.
6. **Audit tidak boleh dimatikan.** Jika mandatory Audit tidak sehat, privileged/destructive action yang mensyaratkan bukti durabel harus fail closed sesuai Document 10.
7. **File, document/OCR text, provider response, dan AI output tetap untrusted.** Jangan membuka file terduga secara manual atau menjalankan payload untuk “membuktikan” insiden.
8. **Containment paling sempit, reversible, dan dapat dipantau.** Jangan mematikan seluruh produk jika Feature, engine, tenant/scope, provider profile, Job class, atau export path yang terdampak dapat dibatasi dengan aman.
9. **Tidak ada public-AI fallback.** Gangguan atau insiden provider internal tidak mengizinkan pemindahan data ke public/unapproved AI.
10. **Tidak ada false success.** Output, restore, recovery, redaction, Audit, atau Job hanya dinyatakan pulih setelah validasi aktual.
11. **Issue bukan Incident Record.** Issue dapat menjadi sinyal awal, tetapi message, screenshot, internal note, dan export Support tidak boleh dipakai sebagai repositori bukti insiden umum.
12. **No implicit break-glass.** Baseline saat ini tidak menyediakan akses isi darurat atau “view as user”. Perluasan akses memerlukan keputusan terpisah yang belum tersedia.
13. **Fakta dipisahkan dari hipotesis.** Setiap catatan membedakan `CONFIRMED`, `SUSPECTED`, `NOT_OBSERVED`, dan `UNKNOWN`.
14. **Preservation dimulai sejak deteksi.** Walau lifecycle formal menempatkan Evidence Preservation setelah Containment, petugas sejak awal dilarang menghapus, mengubah, menyebarkan, atau mengkontaminasi bukti.

Hubungan istilah:

| Record | Tujuan | Bukan |
|---|---|---|
| Incident Record | Koordinasi security/privacy/data incident | Tempat payload sensitif atau kredensial |
| Enterprise Audit | Bukti typed, minimized, append-oriented | Repositori forensik/file berbahaya |
| Observability | Health, error, performance, dan detection | Otoritas compliance atau business state |
| Issue/Support | Laporan user, triage, status, dan safe diagnostics | Incident database umum |
| Defect/Test evidence | Remediation dan regression | Salinan bebas data Production |

---

# 2. Security classification

## 2.1 Klasifikasi kejadian

Klasifikasi awal harus menjawab:

- security objective: confidentiality, integrity, availability, authenticity/authorization, atau accountability;
- environment dan komponen yang terdampak;
- status kejadian: suspected atau confirmed;
- actor dan resource scope yang telah terverifikasi;
- data class yang benar-benar terlibat;
- apakah data hanya dapat diakses, benar-benar dilihat, diunduh, diekspor, dikirim, diubah, atau dihapus;
- jumlah account/resource/Job/output yang diketahui—tanpa broad enumeration yang tidak berwenang;
- durasi exposure dan persistence setelah session/logout/revocation;
- apakah privileged path, public/unapproved provider, signature, redaction, Audit integrity, atau destructive effect terlibat;
- recoverability dan risiko perluasan dampak.

Exact severity, threshold, SLA, notification trigger, dan risk-acceptance authority:

`REQUIRES_PERTAMINA_SECURITY_PROCESS_CONFIRMATION`

Kelas kerja `I/C/R/X/A` dalam Document 08 hanya engineering aid dan tidak menggantikan label resmi Pertamina:

`REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`

## 2.2 Data-class triage matrix

Legenda:

- `I` — lazim/inheren terhadap jenis insiden dan harus diperiksa;
- `P` — mungkin terlibat, hanya tandai setelah bukti menunjukkan scope;
- `–` — tidak inheren; jangan kumpulkan tanpa kebutuhan baru yang disetujui.

Kode data:

| Kode | Data |
|---|---|
| `ID` | Identity data |
| `AM` | Account metadata |
| `DM` | Document metadata |
| `DC` | Document content |
| `OC` | OCR/extracted content |
| `SA` | Visual signature asset |
| `IS` | Issue screenshot/attachment |
| `AU` | Enterprise Audit data/reference |
| `ST` | Secret/token/key/credential |
| `CF` | Configuration/policy/permission state |
| `SC` | Source code/build/dependency evidence |
| `SF` | Secured File metadata/content reference |

| Incident family | ID | AM | DM | DC | OC | SA | IS | AU | ST | CF | SC | SF |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Unauthorized access | I | I | P | P | P | P | P | I | P | P | – | P |
| Cross-user data exposure | I | I | I | P | P | P | P | I | P | P | – | P |
| Privilege escalation | I | I | P | P | P | P | P | I | P | I | P | P |
| Account compromise | I | I | P | P | P | P | P | I | P | P | – | P |
| Token/secret exposure | P | P | – | – | – | – | – | I | I | P | P | – |
| Malicious document | P | P | I | I | P | – | P | I | – | P | P | P |
| Parser/engine security event | P | P | P | P | P | – | – | I | P | P | I | P |
| Audit tampering/loss | I | P | P | P | P | P | P | I | P | I | P | P |
| Suspicious export/download | I | I | P | P | P | P | P | I | P | P | – | P |
| Signature asset exposure | P | P | P | P | – | I | P | I | – | P | – | P |
| Incorrect redaction | P | P | I | I | P | – | – | I | – | P | P | P |
| AI data leakage | P | P | P | P | P | P | P | I | P | I | P | P |
| Prompt/indirect prompt injection | P | P | I | P | P | P | P | I | P | P | P | P |
| Public/unapproved AI misuse | P | P | P | P | P | P | P | I | P | I | P | P |
| Storage/data-integrity issue | P | P | I | P | P | P | P | I | P | P | P | I |

Matriks ini adalah checklist pemeriksaan, bukan pernyataan bahwa semua data `P` telah terekspos. Secured File tetap mewarisi klasifikasi konten tertinggi; label “secured” tidak menurunkan sensitivitas.

## 2.3 Data-minimization decision

Untuk setiap data class, Incident Record hanya mencatat:

```text
DataClass
Involvement = CONFIRMED | SUSPECTED | NOT_OBSERVED | UNKNOWN
ScopeReference = opaque approved reference
ExposureMode = ACCESSIBLE | VIEWED | DOWNLOADED | EXPORTED | SENT | MODIFIED | DELETED | UNKNOWN
EarliestKnownAtUtc / LatestKnownAtUtc
ContainmentState
EvidenceReference
```

Jangan menyalin payload untuk membuktikan klasifikasi. Gunakan opaque reference, digest/hash yang disetujui, count/range, dan controlled evidence location.

---

# 3. Security incident lifecycle

Lifecycle wajib mengikuti urutan berikut:

```text
Detection
↓
Security Classification
↓
Containment
↓
Evidence Preservation
↓
Impact Assessment
↓
Security Escalation
↓
Eradication/Fix
↓
Recovery
↓
Validation
↓
Closure/PIR
```

| Tahap | Tujuan dan tindakan minimum | Exit criteria minimum |
|---|---|---|
| Detection | Validasi sinyal dari alert, user report, Audit, dependency notice, test, provider, atau operator; buat Incident ID; jangan mencoba exploit | Sinyal tercatat sebagai suspected/confirmed, waktu/environment/component diketahui, owner triage sementara ada |
| Security Classification | Tentukan objective, incident family, data class, scope awal, high-risk indicators, dan unknowns | Klasifikasi awal terdokumentasi tanpa klaim legal/severity rekaan |
| Containment | Terapkan kontrol paling sempit, reversible, authorized, time-bound, dan monitored | Jalur perluasan dampak dihentikan atau risk acceptance sementara disetujui |
| Evidence Preservation | Lindungi bukti aman, provenance, integrity, akses, dan transfer; segregasikan bukti restricted | Evidence reference dapat ditelusuri dan tidak dikontaminasi |
| Impact Assessment | Verifikasi account/resource/time/action/data/exposure/destination/derived-copy scope | Known impact dan remaining unknowns jelas; no unsupported certainty |
| Security Escalation | Handoff fakta minimum kepada route security/privacy/identity/owner yang disetujui | Penerimaan/owner resmi tercatat; keputusan wajib diminta |
| Eradication/Fix | Cabut material terdampak, hapus akar masalah melalui change pipeline, pin/replace/rebuild/reconfigure secara terkontrol | Root cause/control failure ditangani dan artifact/config baru terverifikasi |
| Recovery | Pulihkan identity, service, file, Job, Audit, storage, atau provider path secara bertahap | State dapat direkonsiliasi dan layanan aman tersedia dalam scope terkendali |
| Validation | Jalankan security regression, privacy canary, authorization, integrity, recovery, dan monitoring checks yang relevan | Tidak ada bypass/leak/corrupt/false-success berulang; evidence PASS/FAIL/BLOCKED nyata |
| Closure/PIR | Catat timeline, impact, root/contributing cause, control/test/runbook updates, residual risk, owner, dan follow-up | Closure authority menerima bukti atau exception terbatas yang resmi |

Aturan lintas tahap:

- Evidence preservation discipline berlaku sejak Detection dan berlanjut sampai Closure.
- Jika containment akan memusnahkan bukti, minta keputusan security process lebih dahulu kecuali keselamatan/risiko aktif menuntut tindakan segera oleh authority yang disetujui.
- Status `RECOVERED` tidak sama dengan `CLOSED`; Validation dan PIR tetap diperlukan.
- Reclassification diperbolehkan ketika bukti baru tersedia dan harus dicatat sebagai perubahan tertaut, bukan menimpa histori.

---

# 4. Evidence preservation

## 4.1 Evidence allowlist

Preserve hanya yang relevan dan berwenang:

- Incident ID serta UTC timestamps;
- source/detection channel dan environment;
- relevant Audit Event ID/reference serta safe event type/outcome;
- safe synchronized log/trace reference;
- Correlation ID, Job ID, canonical Feature ID, Issue ID, Export Request ID, atau resource reference opaque bila relevan;
- keputusan authentication/authorization yang aman: allow/deny code, policy/permission version, scope type, reauthentication state—bukan raw claims;
- application, API, contract, capability, Worker, engine/provider profile, dependency, package, build, schema, policy, dan configuration version;
- component/queue/Audit/storage health state dan bounded counters;
- file/object/version digest atau hash/reference **jika disetujui**, tanpa filename/path/content;
- SBOM, artifact digest, source commit/reference, dependency provenance, dan pipeline evidence yang relevan;
- sanitized screenshot yang memang dibutuhkan dan melalui controlled action;
- containment/recovery action, actor, approval/reference, time, target scope, outcome, dan rollback state.

## 4.2 Evidence yang dilarang dalam Incident Record biasa

- password, access/refresh token, cookie, authorization header, secret, private/encryption key, connection string;
- document/file bytes atau text, full OCR/extracted/translated/Markdown content;
- visual signature image/template;
- issue screenshot bytes atau unrelated personal data;
- prompt, system instruction, provider request/response, provider credential/endpoint detail;
- raw path, storage key, download URL, raw request/response, stack, SQL, claims dump, IPC payload, packet payload, atau exploit string;
- file berbahaya sebagai attachment Incident Record;
- full source repository bila commit/artifact/SBOM reference sudah cukup.

Jika restricted evidence benar-benar diperlukan, simpan di controlled evidence location terpisah dengan approval, least privilege, encryption, access logging, retention/hold, dan disposal yang disetujui. Incident Record hanya menyimpan opaque reference.

## 4.3 Provenance dan integrity record

Conceptual metadata minimum:

```text
EvidenceId
IncidentId
CollectorPrincipal/WorkloadReference
CollectedAtUtc
SourceType + SourceReference
Environment + ComponentVersion
AcquisitionMethodCode
IntegrityMethod/Value, if approved
Classification/HandlingReference
Access/Transfer/Disposition history
Retention/Hold reference
```

Chain-of-custody format, forensic tool, authorized collector, hashing standard, evidence store, retention, hold, dan disposition:

`REQUIRES_PERTAMINA_SECURITY_PROCESS_CONFIRMATION`

## 4.4 Audit unavailable

Jika Enterprise Audit tidak dapat menerima bukti:

1. jangan disable Audit atau menganggap observability sebagai pengganti permanen;
2. fail closed untuk action yang Document 10 wajibkan memiliki durable intent;
3. preserve owner outbox, Audit outbox, database/backup/health references, dan independent approved operational evidence;
4. setelah recovery, hanya append fakta typed yang benar dengan `OccurredAtUtc` dan `ReceivedAtUtc` nyata;
5. jangan backdate, mengarang event yang hilang, atau mengubah source event.

---

# 5. Identity/account incidents

## 5.1 Account compromise

Runbook konseptual:

1. Validasi laporan tanpa meminta password, token, MFA secret, atau screenshot kredensial.
2. Catat Incident ID, principal reference opaque, session/account state, waktu, client/platform, dan safe detection reference.
3. Batasi atau revoke application session melalui jalur resmi; tindakan Pertamina Identity hanya melalui proses identity yang disetujui.
4. Preserve safe authentication, reauthentication, session-revocation, authorization-denial, dan security event references.
5. Periksa tindakan terkait dalam scope: permission/role change, session creation/revocation, export/download/share, secured-file access, policy/config change, Issue attachment, high-risk Feature, dan Assistant invocation.
6. Bedakan tindakan user yang sah dari tindakan suspected; jangan membuat tuduhan tanpa bukti.
7. Pulihkan account/session hanya setelah identity eligibility, current permission/policy, device/client compatibility, dan revocation state divalidasi.
8. Reauthorize queued/retry/resume/Hub subscription saat reconnect; intent lama bukan grant.
9. Jalankan ownership, cross-user, export, dan session regression yang relevan.

Protocol SSO, global sign-out, token revocation, MFA reset, account recovery, session lookup, IdP evidence, dan contact route:

`REQUIRES_PERTAMINA_SSO_CONFIRMATION`

## 5.2 Token/secret exposure

- Jangan menyalin material yang terpapar ke Incident Record, Issue, chat, email, screenshot, log, atau source.
- Revoke/rotate material tepat melalui secret/identity process resmi; jangan melakukan rotation massal tanpa scope dan dependency assessment.
- Identifikasi tempat material mungkin berada: memory/store, config, artifact/build history, source history, CI/CD output, log/trace/dump, Issue, export, backup, Worker/provider configuration, dan device cache.
- Periksa penggunaan material, audience, purpose, permission, environment, waktu, dan artifact/deployment terdampak menggunakan reference aman.
- Setelah rotation, verifikasi old material ditolak, new material hanya tersedia bagi workload yang benar, startup fail closed bila secret hilang, dan tidak ada plaintext fallback.
- Jika source/dependency/artifact terlibat, freeze affected version, preserve commit/package/hash/SBOM evidence, rebuild dari controlled clean inputs, retest, dan redeploy melalui pipeline.

## 5.3 Privilege escalation

- Freeze mutation permission/role/config yang terkait bila authorized dan aman.
- Revoke grant atau session yang tidak sah melalui control plane resmi dengan optimistic concurrency dan reauthentication.
- Preserve before/after version, actor, target, scope, approval reference, Audit event, dan denied/allowed decision code.
- Periksa self-assignment, cross-scope assignment, stale session/offline grant, direct permission persistence, policy bypass, Support/Audit grant, dan workload identity misuse.
- Jangan memperbaiki dengan direct database edit atau memberi role lebih luas sebagai workaround.
- Verifikasi seluruh derived session/cache/reconnect path menggunakan current permission/policy.

---

# 6. Authorization/data exposure

## 6.1 Unauthorized access dan cross-user exposure

1. Hentikan route/action/subscription/download/export/Job continuation yang membuka resource secara salah.
2. Jangan mengubah owner resource sebagai jalan pintas.
3. Revoke inappropriate permission/scope/session melalui mekanisme resmi; pertahankan deny ketika authorization store/policy service tidak sehat.
4. Preserve actor, target resource type dan opaque ID, parent-child relation, decision code, permission/policy/resource version, Correlation/Job/Export ID, route, time, dan outcome.
5. Identifikasi apakah resource hanya enumerable, metadata terlihat, content streamed, range downloaded, exported/shared, cached, atau copied downstream.
6. Scope dari principal/resource exact yang berwenang; jangan melakukan broad browsing pada file pengguna lain.
7. Periksa list-before-count, exact lookup, cursor continuation, child membership, download/range, export publication/download, SignalR subscription, retry/resume, restore, cache, dan tombstone.
8. Perbaiki authorization pada API/application service; UI hiding tidak cukup.
9. Jalankan regression wrong-owner, wrong-scope, stale permission, guessed opaque ID, cursor replay, cross-user Hub, range/download, export, dan concurrent state change.
10. Validasi resource terdampak tidak lagi dapat diakses dan resource sah tetap bekerja.

Support tidak boleh diberi broader user-file access untuk investigasi. Current baseline tidak memiliki exceptional content access.

## 6.2 Suspicious export/download

- Stop admission/publication/download dari export request yang terdampak melalui official service bila authorized.
- Preserve normalized filter/scope, field projection version, as-of boundary, row/byte count, digest, expiry, actor, reauthentication, request/download Audit references, dan destination class yang diketahui—tanpa isi export.
- Periksa apakah query scope, permission, reauthentication, field masking, current authorization at download, expiry, private link, formula/external-link protection, atau retry/idempotency gagal.
- Jangan membuka export untuk “melihat isinya” kecuali explicit permission dan security process mengizinkan.
- Expire/cleanup artifact mengikuti controlled lifecycle; jangan menghapus bukti sebelum preservation/hold decision.

## 6.3 Authorization recovery acceptance

- denied principal tetap denied;
- owner yang benar tetap dapat memakai resource sesuai current policy;
- Administrator, Auditor, dan Support tidak memperoleh content access implisit;
- revocation/perubahan permission membatalkan stale cursor, Hub subscription, reauth context, dan queued continuation;
- public IDs, Job ID, Correlation ID, Event ID, Issue ID, dan export ID tidak menjadi grant;
- error tidak mengungkap existence, owner, filename, path, atau policy detail sensitif.

---

# 7. Malicious-document incidents

Incident family ini mencakup file malformed/polyglot, active content, archive/decompression bomb, image/page/pixel bomb, traversal/link entry, Office macro/OLE/DDE/external link, malicious HTML/remote fetch, parser crash, engine exploit suspicion, dan repeated unsafe input.

## 7.1 Immediate handling

1. Hentikan processing dan dependent retry untuk file/digest/engine profile terdampak.
2. Jadikan file non-addressable bagi user/Worker lain atau isolate reference sesuai quarantine policy; jangan membuka manual.
3. Pertahankan source user dan unrelated Jobs; jangan menghapus file sebagai langkah pertama.
4. Catat safe metadata: opaque file/job reference, digest/hash bila approved, declared/actual type result, size/page/object/container limit code, canonical Feature ID, engine/provider/dependency/build version, Worker instance class, Correlation ID, outcome, dan timestamps.
5. Block repeated unsafe execution menggunakan safe digest/reference/rule yang scoped dan time-bound; jangan memasukkan payload ke block log.
6. Jalankan approved malware/parser validation hanya pada isolated non-Production/security environment oleh authority yang sesuai.
7. Buat sanitized/synthetic reproduction bila memungkinkan; jangan memindahkan Production file ke Development/Test biasa.
8. Review engine/provider/dependency version, sandbox/least privilege, resource bounds, egress, temp/workdir isolation, output validation, crash/recycle, dan SBOM/advisory.
9. Re-enable hanya setelah patched/approved engine atau compensating control lulus malicious-corpus, resource, isolation, output, dan recovery regression.

## 7.2 Prohibited actions

- tidak membuat exploit atau proof-of-exploit pada Production;
- tidak menjalankan shell, desktop automation, arbitrary URL, atau parser alternatif yang belum disetujui;
- tidak mengirim sample ke public scanner/AI/provider;
- tidak menaruh file, exploit string, raw stack, atau memory dump dalam Audit/Issue/ordinary log;
- tidak menyatakan “scan clean” sebagai bukti file aman;
- tidak mengubah timeout/limit menjadi unlimited untuk menyelesaikan file.

Parser/engine zero-day tetap residual risk; exact malware handling, quarantine retention, vendor notification, dan forensic route:

`REQUIRES_PERTAMINA_SECURITY_PROCESS_CONFIRMATION`

---

# 8. Storage/integrity incidents

Contoh sinyal:

- SQL metadata menunjuk object hilang/salah/digest mismatch;
- object ada tanpa metadata yang sah;
- secured current pointer/version count/tombstone tidak konsisten;
- deleted/expired content kembali addressable setelah restore;
- cross-user owner/reference mismatch;
- duplicate destructive effect atau false terminal success;
- encrypted local profile/SQLite/WAL/artifact manifest tidak dapat divalidasi;
- source/result tertimpa, setengah-publish, corrupt, atau silent loss;
- unauthorized cleanup/archive/purge/restore.

## 8.1 Containment dan assessment

1. Stop affected write, promotion, download, restore, purge, archive, atau cleanup path; jangan mematikan seluruh storage tanpa scope evidence.
2. Tandai affected result/resource unavailable atau integrity-review melalui state resmi; jangan mengubah DB/object manual.
3. Preserve opaque SQL/object/version references, digest/key-version reference, tombstone/retention/hold state, Job/transfer/idempotency/outbox/Audit references, schema/migration/build version, dan timestamps.
4. Periksa state lintas SQL metadata, object existence/version/digest, key reference, current pointer, maximum-three invariant, recycle/tombstone, permission/owner, outbox/idempotency, backup/restore point, dan cache projection.
5. Jangan memulihkan access hanya karena object masih ada; authorization dan lifecycle state tetap authoritative.

## 8.2 Recovery

- Gunakan approved recovery point dan restore procedure; raw SQL backup saja bukan product restore lengkap.
- Reconcile schema/migration/constraint/module permission, SQL–object–digest–key–tombstone–retention/hold, outbox/idempotency/Job/lease/Audit, dan maksimum tiga active/addressable secured versions.
- Local restore mencakup protected SQLite plus required app-managed artifacts dan integrity manifest; jangan overwrite external source.
- Corrupt local DB tidak dihapus/reset diam-diam.
- Deleted/tombstoned/expired data tidak boleh menjadi addressable kembali.
- Verifikasi no duplicate logical action, no orphan addressability, no cross-user access, no source overwrite, dan Audit append tetap bekerja.

RPO/RTO, topology, key recovery, backup authority, immutable/offline copy, dan destructive recovery:

`REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`

---

# 9. Signature incidents

Visual signature/paraf adalah asset sensitif, local/profile-scoped, opt-in, dan **non-PKI**. Incident triggers:

- auto-upload/auto-sync atau consumer-cloud/device backup;
- cross-user/profile visibility;
- signature bytes/template masuk log, telemetry, Audit, Issue screenshot, thumbnail, cache, export, backup, atau AI/provider payload;
- insecure temp/cache atau lifecycle deletion gagal;
- unauthorized placement/finalization.

Runbook:

1. Stop affected save/sync/cache/placement/finalization path dan block further access dengan mechanism resmi.
2. Jangan menampilkan atau menyalin gambar ke Incident Record.
3. Preserve signature asset reference opaque, profile/device binding, affected document/result references, lifecycle state, access/sync decision, build/version, Correlation ID, dan timestamps.
4. Tentukan lokasi salinan yang mungkin: app-private storage, temp/cache/thumbnail, issue screenshot, log/telemetry, backup, secured transfer, result document, dan provider path.
5. Revoke/cleanup affected reference hanya setelah preservation, owner impact, retention, dan security decision; jangan menghapus bukti secara spontan.
6. Perbaiki explicit-action, profile isolation, no-auto-sync, cache exclusion, logging guard, screenshot preview, dan cleanup lifecycle.
7. Validasi wrong-user/profile, restart, backup exclusion, log/telemetry/Audit/Issue canary, temp cleanup, dan explicit finalization.

Jangan mengklaim certificate revocation, non-repudiation, atau PKI response; PKI berada di luar scope.

---

# 10. Redaction incidents

Incident terjadi ketika redaction tampak secara visual tetapi underlying text/image/annotation/layer/hidden content/OCR derivative masih dapat diekstrak, dicari, disalin, dipulihkan, atau terbaca pada turunan/cache.

Runbook:

1. Hentikan download, share, export, secured transfer, dan distribusi output terdampak.
2. Tandai output affected/non-final melalui state resmi; jangan mengubah source asli.
3. Preserve opaque result/source references, Feature ID `FEAT-PDF-069`, engine/build/version, redaction method/profile code, validation outcome codes, Job/Correlation ID, distribution/export references, dan timestamps—tanpa redacted text/coordinates.
4. Identifikasi scope salinan: result lokal, download/export, secured version, cross-device cache, preview/thumbnail, OCR/index/Markdown, Issue attachment, backup, dan recipient/destination yang telah terverifikasi.
5. Informasikan owner dan security/privacy route melalui komunikasi yang disetujui; jangan mengirim ulang output untuk menjelaskan masalah.
6. Buat corrected output dari source yang sah menggunakan fixed/approved engine; output lama tidak di-overwrite diam-diam.
7. Uji rendering serta negative extraction/search/copy dan structure/object/layer/annotation/OCR derivative.
8. Invalidasi dan cleanup preview/thumbnail/OCR/cache/undo/checkpoint pra-redaksi sesuai lifecycle setelah preservation/hold decision.
9. Record exposure scope dan follow-up; corrected output tidak membatalkan exposure yang sudah terjadi.

Jika konten telah keluar dari owner scope, perlakukan juga sebagai data-exposure incident dan eskalasi security/privacy.

---

# 11. AI/Assistant incidents

## 11.1 Incident patterns dan containment

| Pola | Containment minimum | Investigation focus |
|---|---|---|
| Document/content dikirim tidak semestinya | Stop affected Job/provider route; disable scoped provider/Feature melalui approved config; preserve safe refs | Permission, classification, notice/consent, selected pages/fields, route, provider profile, temporary lifecycle |
| Public/unapproved AI route berhasil/diduga berhasil | Block route/provider profile; stop queued/in-flight jobs yang dapat dihentikan aman; no public fallback | Egress/config/endpoint allowlist, client bypass, provider selection, payload/data classes, destination, retention/deletion terms |
| Direct/indirect prompt injection menghasilkan unsafe plan | Stop execution sebelum official service; disable affected orchestration path if recurring | Provenance separation, document-as-DATA, canonical Feature ID, schema, ambiguity/confirmation, per-step controls |
| Permission/ownership/policy bypass | Stop workflow/result publication; revoke inappropriate session/grant | Tool validation, resource ownership, current permission/policy, queued steps, reauth, result access |
| Sensitive data masuk logs/telemetry/Audit/Issue | Stop affected logging/export path without disabling mandatory Audit; rotate secret if applicable | Field allowlist/guard, prompt/provider capture, traces/dumps, retention/copies |
| Malicious provider/model output | Quarantine unvalidated output; stop auto-render/tool/fetch path | Schema/size/encoding, XSS/formula/link/URL, grounded result, downstream storage/rendering |
| Hallucinated command/false success | Mark result not completed; reconcile actual Job/persisted state | Official execution evidence, tool plan, terminal state, UI/SignalR stale event |

## 11.2 Invariants

- Minimum permitted context only.
- Document/OCR/HTML/metadata is `DATA`, never instruction.
- Canonical visible Feature ID and typed/versioned parameters only.
- Permission, ownership, policy, classification, consent, capability/provider, resource budget, ambiguity, confirmation, dan reauthentication checked in order.
- AI cannot call SQL, shell, filesystem, arbitrary HTTP/API/URL, plugin, Worker operation, engine, or provider directly.
- Model/provider output is untrusted and cannot auto-fetch, auto-render unsafe HTML, or declare success.
- Response must be grounded in persisted validated execution result.
- Public AI/translation endpoint remains technically blocked.

## 11.3 Recovery

1. Fix policy/contract/config/code through controlled change and rebuild from approved inputs.
2. Use approved local deterministic path only when capability, permission, policy, and security controls remain valid; otherwise mark unavailable/waiting.
3. Do not substitute an unapproved provider.
4. Validate no public egress, wrong-user context, sensitive canary, prompt/tool bypass, arbitrary URL, malicious response execution, or false success.
5. Verify provider temporary content/result cleanup and lease/expiry reconciliation under approved governance.

Provider incident notification, retention/training/reuse, deletion attestation, location/subprocessor, dan evidence access:

`REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`

---

# 12. Audit incidents

Incident Audit meliputi missing intent/event, unauthorized alteration/delete, conflict `same ID + different fingerprint`, inaccessible evidence, severe ingestion delay/gap, hold violation, unauthorized archive/purge/export, restore that breaks append/uniqueness, atau misleading green health.

## 12.1 Handling

1. Stop affected destructive/privileged action, Audit purge/archive/restore, atau disclosure path bila durable evidence/integrity tidak dapat dijamin.
2. Jangan disable Audit, sampling mandatory evidence, edit source event, atau delete failed/poison/unacknowledged rows.
3. Preserve owner outbox, local/server Audit outbox, ingestion batch, stable Event ID/fingerprint, source sequence, `OccurredAtUtc`, `ReceivedAtUtc`, policy/catalog/schema version, backup/archive manifest, hold/retention state, workload identity, health/alert, dan database integrity reference.
4. Scope apakah kegagalan terjadi pada intent creation, owner commit, dispatch, ingestion, deduplication, storage, query, export, archive/purge, restore, atau health projection.
5. Periksa append-only permissions, generic repository exposure, workload scope, unique constraints, outbox durability, deduplication, sequence gap, poison/version block, restore permissions, hold, dan lifecycle evidence.
6. Recover dari durable approved source; replay idempotently; preserve late provenance.
7. Untuk correction/supersession/late context, append linked typed event yang diizinkan katalog. Jangan membuat event fiktif untuk mengisi gap.
8. Validasi mandatory action fail-closed, append after restore, duplicate-safe replay, no source mutation, scope-controlled read/export, and health accuracy.

## 12.2 Audit incident boundaries

- Observability membantu detection tetapi bukan pengganti Enterprise Audit.
- Auditor tidak dapat memperbaiki, mengubah, atau menghapus event.
- Administrator/Support tidak otomatis memperoleh full Audit access.
- Audit Event ID/Correlation ID tidak memberi akses resource dasar.
- Bukti purge/archive/hold harus berada di luar exact target set dan mengikuti policy sendiri.
- Suspected Audit tampering harus dieskalasikan melalui independent approved security route.

---

# 13. Containment options

Semua option memerlukan actor/permission yang tepat, exact scope, reason, start time, review/expiry, rollback/forward path, mandatory Audit intent jika sehat, serta monitoring.

| Boundary | Containment option konseptual | Guardrail |
|---|---|---|
| Pertamina Identity | Revoke/limit enterprise session or account through approved process | `REQUIRES_PERTAMINA_SSO_CONFIRMATION`; app tidak melakukan password/MFA recovery |
| OTO session/account | Revoke application session, retain disabled/denied state | Reauth, no credential display, no broad account disable without authority |
| Permission/scope | Revoke wrong grant; restore last approved typed state | No self-elevation, no direct DB, revalidate all derived sessions |
| API/action | Deny affected operation/resource scope; tighten admission | Jangan return sensitive reason/existence; REST remains source of truth |
| Feature/configuration | Scoped maintenance/disable via approved feature control | Cannot disable permission, Audit, validation, quarantine, public-AI block, or Local-First invariant |
| Export/download/share | Stop admission/publication/download; expire affected artifact through lifecycle | Preserve evidence/hold; no public/durable link |
| Job/queue | Stop admission, cancel/hold safe boundary, drain/reconcile | Preserve source, accepted work, idempotency, Job state, and Audit intent |
| Worker/engine/provider | Remove affected profile/version from eligibility; recycle isolated Worker; stop dependent jobs | No arbitrary command; no unapproved/public fallback; preserve version/SBOM |
| File/document | Make unsafe reference non-addressable; quarantine by approved policy | Do not open/delete manually; no content in incident record |
| Storage/cleanup | Freeze affected write/purge/archive/restore; protect holds and manifests | No object browsing/direct SQL; avoid resurrection/orphan |
| AI | Disable scoped provider/orchestration/Feature flag through approved configuration | Local fallback only if already approved and identical controls pass |
| SignalR | Disconnect/revoke affected subscription and require REST reconciliation | Event is not authority; reconnect reauthorizes current scope |
| Release/artifact | Stop promotion; rollback or forward-fix through pipeline | New immutable verified artifact; preserve build/provenance evidence |

Containment authority, emergency approval, time limit, destructive option, and business continuity exception:

`REQUIRES_PERTAMINA_SECURITY_PROCESS_CONFIRMATION`

---

# 14. Recovery validation

Recovery hanya boleh dinyatakan berhasil jika applicable checks lulus:

## 14.1 Common gate

- root/contributing control failure telah diperbaiki atau compensating control resmi aktif;
- affected session/grant/secret/config/provider/artifact tidak lagi diterima;
- source dan unaffected user data tetap utuh;
- state SQL/object/local cache/Job/outbox/Audit telah direkonsiliasi;
- tidak ada duplicate effect, partial final, false `COMPLETED`, cross-user access, silent upload, atau silent data loss;
- mandatory Audit intent dapat dibuat durabel dan health state akurat;
- privacy canary tidak muncul di log, telemetry, Audit, Issue, error, export, dump, provider, atau sync payload;
- capacity/backlog/Worker/DB pool/storage kembali dalam safe operating state;
- staged re-enable dan post-recovery monitoring tersedia;
- residual risk/exception mempunyai owner, authority, compensating control, expiry, dan follow-up.

## 14.2 Incident-specific validation

| Incident | Required validation |
|---|---|
| Account/token | Old session/material denied; current permission/policy resolved; reconnect/offline/Hub revalidated; no token in evidence |
| Cross-user/BAC | Wrong owner/scope/cursor/range/export/subscription denied; correct owner works; errors safe |
| Privilege escalation | No self/cross-scope grant; stale session invalidated; mutation audited; SoD intact |
| Malicious file/parser | Synthetic corpus safe reject; isolation/resource/output checks pass; crash contained; no repeated unsafe retry |
| Storage/integrity | SQL–object–digest–key–tombstone–hold reconciled; max three; no resurrected/deleted addressability; append after restore |
| Signature | No auto-sync/backup/log/cache/cross-profile visibility; explicit placement/finalization only |
| Redaction | Render plus negative extraction/search/copy/structure/OCR derivative tests pass; old output remains blocked |
| AI | No public/unapproved egress; prompt remains data; canonical typed tool only; permissions/confirmation enforced; response grounded |
| Audit | Append-only, unique/idempotent replay, late provenance, query scope, hold/archive/purge, and health behavior pass |
| Suspicious export | Field/scope/as-of/expiry/current auth/reauth/private download and formula/link defenses pass |

Security tests memakai synthetic/sanitized data dan authorized non-Production environment. Production hanya menerima bounded approved health/smoke validation; tidak ada malicious corpus, fuzz, exploit, load, atau user-content testing.

---

# 15. Communication boundary

## 15.1 Principles

- need-to-know, permission-scoped, factual, minimal, approved, dan tracked;
- gunakan Incident ID serta opaque references, bukan payload;
- bedakan confirmed facts, suspected scope, unknowns, containment state, service/user impact, dan next decision;
- jangan menyebut akar masalah, actor attribution, atau jumlah terdampak sebagai fakta sebelum tervalidasi;
- jangan menyertakan exploit steps, token, secret, raw stack/SQL/path, document/OCR content, signature image, prompt/provider payload, atau unrelated personal data;
- jangan mengirim restricted evidence melalui Issue, ordinary email/chat, public link, atau screenshot tanpa controlled channel;
- komunikasi user/support tidak menampilkan detail teknis yang dapat membantu abuse atau data pengguna lain.

## 15.2 Safe status template

```text
Incident ID:
Status: SUSPECTED | CONFIRMED | CONTAINED | RECOVERING | VALIDATING | CLOSED
Environment / affected service:
Time window (UTC):
Verified impact summary:
Data classes involved (no payload):
Containment completed / pending:
User-visible service impact:
Known unknowns:
Required decision / owner:
Next update time or trigger:
Approved evidence references:
```

## 15.3 Audience boundaries

| Audience | Minimum information | Exclusions |
|---|---|---|
| User/owner | Verified impact to own service/resource, safe action, recovery state, approved next step | Other users, internal topology, exploit/security detail, raw evidence |
| Support | Issue-linked safe status and user guidance | Full Incident evidence, unrelated Audit, user document, secret/signature |
| Administrator/Operations | Affected control-plane/service scope and approved action | User content and credentials |
| Auditor | Approved Incident/Audit references and controlled evidence projection | Mutation authority, content access |
| Security/Privacy/Identity owner | Approved minimum evidence needed for decision | Unnecessary content or broad credentials |
| Developer/provider owner | Sanitized reproduction, version/hash/SBOM/control failure | Production file/account identity unless separately approved |

External, legal, regulator, employee/privacy, provider, management, or public communication:

`REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`

---

# 16. Required escalation

Security escalation wajib dipicu bila satu atau lebih hal berikut suspected atau confirmed:

- cross-user/cross-scope access atau unauthorized privileged access;
- self-assignment, role/permission escalation, workload impersonation, atau stale privileged allow;
- account compromise, token/secret/key exposure, replay, atau unexpected session;
- document/OCR/signature/restricted screenshot/Issue attachment/secured content leakage;
- suspicious broad export/download/share atau artifact link exposure;
- public/unapproved AI route, AI context leakage, permission bypass, atau provider compromise;
- incorrect redaction atau unsafe output yang telah/berpotensi didistribusikan;
- malicious file/parser/engine/dependency compromise atau repeated security rejection spike;
- missing/altered/deleted/inaccessible Audit, integrity/hold threat, or mandatory intent not durable;
- corruption, silent loss, false success, duplicate destructive effect, orphan, restore resurrection, atau digest/key mismatch;
- unauthorized configuration/policy/provider/feature change;
- critical service protection gagal, widespread abuse/DoS, or safe recovery unclear.

Handoff minimum:

```text
Incident ID + detection source
suspected/confirmed classification
UTC time window + environment/component/version
safe actor/resource scope
data classes and exposure mode
Feature/Job/Correlation/Audit/Export references
containment performed and authority reference
service/user impact
remaining unknowns and required decision
controlled evidence references
```

Exact severity, on-call contact, SOC/SIEM route, privacy/legal route, incident commander, escalation SLA, and closure authority:

`REQUIRES_PERTAMINA_SECURITY_PROCESS_CONFIRMATION`

---

# 17. Pertamina confirmation gaps

## 17.1 Security process

`REQUIRES_PERTAMINA_SECURITY_PROCESS_CONFIRMATION`

- official incident category/severity taxonomy dan threshold;
- Security Operations/SOC/SIEM contact, on-call, escalation route, dan acknowledgement SLA;
- Incident Commander, technical lead, evidence custodian, privacy/legal liaison, service owner, dan decision RACI;
- containment authority untuk account/session, permission, Feature/provider, Job/queue, export/download, storage, network, deployment, dan destructive recovery;
- emergency approval, risk acceptance, temporary exception, expiry, dan review;
- evidence acquisition method/tool, chain-of-custody, evidence store, access, integrity algorithm, transfer, retention, legal/investigation hold, dan disposal;
- malware/quarantine/sample handling serta dependency/provider incident route;
- forensic investigation boundary dan permitted Production diagnostics;
- recovery authorization, validation sign-off, closure criteria, PIR template, exercise/tabletop cadence;
- process bila mandatory Audit sendiri compromised/unavailable.

## 17.2 Identity/SSO

`REQUIRES_PERTAMINA_SSO_CONFIRMATION`

- account/session/token lookup dan revocation capability;
- global sign-out, refresh-token reuse/replay handling, MFA/device/account recovery;
- IdP evidence access, identity contact, lifetime/revocation propagation, and user verification;
- disabled/revoked account recovery and queued/offline session handling.

## 17.3 Enterprise standard, privacy, and communication

`REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`

- official data classification labels dan handling matrix;
- criteria untuk privacy/data incident, impact assessment, affected-person determination, dan notification triggers;
- legal/regulatory/contractual/external/provider notification decision and authority;
- approved internal/user/management communication channel dan template;
- retention/hold untuk Incident Record, restricted evidence, logs, exports, malicious samples, provider data, dan backups;
- public/internal AI provider governance, incident notice, retention/training/reuse, deletion, location, dan subprocessor obligations;
- RPO/RTO, backup/restore, key recovery, and disaster-recovery authority;
- official hashing/crypto/key/secret rotation standards;
- redaction incident disclosure dan visual-signature privacy handling;
- source/dependency vulnerability disclosure, emergency patch, and vendor coordination process.

Gap konfirmasi tidak mengizinkan secure default dilemahkan. Capability Produksi terdampak harus tetap blocked/degraded secara aman sampai keputusan tersedia.

---

# 18. Risks/conflicts

| Risiko/konflik | Dampak | Resolusi yang disarankan |
|---|---|---|
| Lifecycle Document 08 menyebut `Contain → Preserve Evidence`, sedangkan preservation harus dimulai dini | Bukti dapat hilang sebelum tahap formal | Pertahankan urutan 10 tahap user; nyatakan preservation discipline dimulai saat Detection dan formal custody setelah Containment |
| Kelas `I/C/R/X/A` belum label resmi Pertamina | Salah penanganan/komunikasi klasifikasi | Gunakan hanya engineering mapping; wajibkan konfirmasi label resmi |
| Exact severity/contact/SLA belum ada | Eskalasi terlambat atau tidak konsisten | Tandai semua sebagai `REQUIRES_PERTAMINA_SECURITY_PROCESS_CONFIRMATION` |
| Support Console mudah dianggap sebagai Incident Console | Evidence sensitif masuk Issue/note/export | Pisahkan record dan permission; hanya link/reference aman |
| Investigator memerlukan content untuk analisis | Potensi perluasan akses dan cross-user exposure kedua | Baseline no content access; workflow exceptional terpisah belum tersedia, tanpa implicit break-glass |
| “Disable provider/Feature” dapat disalahgunakan | Availability loss atau perubahan security control | Hanya typed scoped config, authorized, audited, time-bound, review/expiry, staged recovery |
| “Approved fallback” dapat ditafsirkan sebagai public AI | Data exfiltration | Fallback hanya capability yang sudah approved dan memenuhi kontrol sama; public AI tetap blocked |
| Audit compromised dan operator ingin “memperbaiki event” | Bukti dipalsukan/diubah | Preserve source/outbox; append linked factual event only; no backdating/fabrication |
| Token/secret exposure evidence sendiri membawa secret | Secondary disclosure | Never copy material; use opaque secret ID/version and rotation/use evidence |
| Malicious sample dipindah untuk troubleshooting | Parser exploit atau data leak | Approved isolated handling; sanitized/synthetic reproduction; no manual open/public service |
| Redaction output diperbaiki dianggap exposure selesai | Distribusi/copy lama tetap berisiko | Track old output scope, keep blocked, handle as data exposure, complete privacy decision |
| Signature dianggap PKI evidence | Response salah dan overclaim | Tegaskan visual signature non-PKI; handle as sensitive image asset |
| Storage restore menghidupkan kembali data deleted/tombstoned | Unauthorized disclosure/integrity loss | Mandatory SQL–object–digest–key–tombstone–hold–permission reconciliation |
| Broad containment mematikan local-safe capability | Gangguan server kecil meluas | Scope containment; pertahankan valid local path tanpa silent upload/fallback |
| Source code dicopy ke Incident Record | IP/secret leakage dan retention tidak tepat | Preserve commit/artifact/SBOM/hash references; restricted source access terpisah |
| Legal/privacy notification diasumsikan oleh technical team | Unapproved legal conclusion | Semua notification/attribution ditentukan process/authority Pertamina |
| Evidence retention bertabrakan dengan cleanup | Bukti terhapus atau data disimpan berlebihan | Hold-aware lifecycle dengan data minimization dan exact policy confirmation |
| Direct remediation Production mempercepat recovery tetapi melewati governance | New compromise, unreproducible state, lost evidence | Controlled immutable artifact/config change, expedited governance, rollback/forward-fix, PIR |

Tidak ditemukan kebutuhan untuk membuat role baru, mengubah Role Matrix, memberi Support full Audit, atau memberi Administrator access ke content.

---

# 19. Suggested Document 16 text

Bagian berikut dapat diintegrasikan ke master Document 16 setelah review Integrator.

## Security, Privacy, and Data Incident Handling

PERTAMINA OTO menangani dugaan unauthorized access, cross-user exposure, privilege escalation, account/token compromise, malicious document/parser event, Audit integrity failure, suspicious export, signature exposure, incorrect redaction, AI leakage/prompt injection/public-AI misuse, serta storage/data-integrity failure sebagai security incident candidate.

Security incident tidak mengubah model permission. Administrator, Support/Superadmin, Auditor, operator, developer, dan investigator tidak otomatis memperoleh akses document content, OCR content, visual signature, secret, secured file, atau data pengguna lain. Tidak tersedia implicit impersonation, “view as user”, atau break-glass content access. Seluruh action menggunakan identity, API/application service, control plane, deployment, backup, dan recovery flow resmi.

### Lifecycle

```text
Detection
→ Security Classification
→ Containment
→ Evidence Preservation
→ Impact Assessment
→ Security Escalation
→ Eradication/Fix
→ Recovery
→ Validation
→ Closure/PIR
```

Preservation discipline dimulai saat Detection: petugas tidak boleh menghapus, mengubah, menyebarkan, atau mengkontaminasi bukti. Containment harus paling sempit, reversible, dapat dipantau, memiliki actor/authority, target scope, alasan, waktu, review/expiry, dan recovery path. Audit, permission, validation, quarantine, rate limit, public-AI block, Local-First, atau no-silent-overwrite tidak boleh dimatikan sebagai workaround.

### Classification and minimization

Setiap Incident Record menetapkan `CONFIRMED`, `SUSPECTED`, `NOT_OBSERVED`, atau `UNKNOWN` untuk identity data, account metadata, document metadata/content, OCR content, visual signature, Issue screenshot, Audit data, secret/token, configuration, source/build evidence, dan Secured File. Record juga membedakan resource hanya accessible dari benar-benar viewed, downloaded, exported, sent, modified, atau deleted.

Klasifikasi resmi, severity, threshold, SLA, dan notification trigger mengikuti proses Pertamina:

`REQUIRES_PERTAMINA_SECURITY_PROCESS_CONFIRMATION`  
`REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`

### Evidence

Evidence minimum adalah Incident ID, UTC timestamps, environment/component/version, relevant Audit/log reference, safe authorization decision, Correlation ID, Job ID, canonical Feature ID, opaque resource/export reference, configuration/policy/build/dependency version, component state, bounded counters, sanitized screenshot bila approved, serta digest/hash/reference bila approved.

Incident Record, Audit, Issue, log, telemetry, dan ordinary export tidak boleh menyimpan password, token, cookie, secret/key, document/file bytes atau text, OCR content, visual signature, Issue attachment bytes, prompt/provider payload, raw path/storage key, download URL, raw request/response, stack, SQL, claims dump, IPC payload, atau exploit string. Restricted evidence yang disetujui disimpan terpisah pada controlled evidence location dan Incident Record hanya memuat opaque reference.

Evidence provenance mencatat collector, time, source, environment/component version, acquisition method, integrity reference, access/transfer/disposition, serta retention/hold. Exact chain-of-custody, tooling, repository, authority, retention, hold, dan disposal:

`REQUIRES_PERTAMINA_SECURITY_PROCESS_CONFIRMATION`

### Scenario controls

**Account/token.** Validasi report tanpa meminta credential. Revoke/limit session melalui approved Identity/application process, preserve safe evidence, review session/permission/export/download/high-risk action scope, rotate exposed material, dan verifikasi old material/session ditolak. SSO mechanics berstatus `REQUIRES_PERTAMINA_SSO_CONFIRMATION`.

**Cross-user/authorization.** Stop affected route, resource, subscription, Job continuation, download, atau export; revoke inappropriate grant/session; preserve actor/resource/permission-policy-version and decision references; determine whether metadata/content was accessible/viewed/downloaded/exported; fix API/application-service authorization; run wrong-owner/scope/cursor/range/SignalR/export regression. Support tidak diberi broader file access untuk investigasi.

**Malicious document/parser.** Stop processing/retry, isolate or quarantine reference according to approved policy, do not open manually, preserve safe digest/type/size/Feature/engine/dependency/Worker/Job/Correlation evidence, block repeated unsafe execution, validate only in approved isolated environment, review SBOM/provider/dependency, and re-enable only after malicious-corpus/isolation/resource/output/recovery tests pass. No exploit development or public scanning/provider upload.

**Storage/integrity.** Freeze affected writes, publication, purge/archive/restore, or download; never direct-edit SQL/object storage. Reconcile schema, SQL metadata, object/version/digest/key, owner/permission, tombstone/retention/hold, maximum-three versions, idempotency/outbox/Job/Audit, backup point, and local integrity manifest. Restore success requires no resurrected deleted data, orphan addressability, cross-user access, duplicate effect, source overwrite, false success, or broken Audit append.

**Visual signature.** Stop affected sync/cache/access/placement path. Signature image is not placed in Incident Record. Assess auto-upload, cross-profile visibility, logs/telemetry/Audit, screenshot, cache/thumbnail, backup, result, and provider copies. Correct explicit-action/profile isolation/no-auto-sync/no-log/cleanup controls and validate with privacy canaries. Visual signature remains non-PKI.

**Redaction.** Stop distribution/download/share/export/secured transfer and mark affected output non-final. Preserve safe output/engine/Feature/Job/Correlation/distribution references without text or coordinates. Determine copies and derived OCR/preview/cache artifacts. Generate a corrected new output and require rendering plus negative extraction/search/copy/structure/OCR tests. A corrected output does not erase prior exposure; cross-scope disclosure also follows privacy/data-exposure process.

**AI/Assistant.** Stop affected Job/provider/orchestration route and use approved scoped feature/provider control. Public/unapproved AI is never a fallback. Document/OCR/HTML remains DATA; Feature ID and parameters must be canonical and typed; permission, ownership, policy, classification, consent, capability, ambiguity, confirmation, and reauthentication are checked deterministically; AI cannot call SQL/shell/filesystem/arbitrary HTTP or execute provider output; success is grounded in persisted validated result. Validate no public egress, wrong-user context, sensitive logging, prompt/tool bypass, unsafe rendering, or false success.

**Audit.** If mandatory intent/integrity is unavailable, fail closed for affected privileged/destructive/disclosure action. Never disable Audit or edit/delete source events. Preserve owner/Audit outbox, event/fingerprint/sequence/time/policy/schema/manifest/hold references. Recover from durable approved sources with idempotent replay. Late/corrected context becomes a linked factual typed event; never fabricate or backdate missing evidence.

### Recovery and validation

Recovery requires a controlled fix/artifact/configuration, staged re-enable, reconciled source/state, current authorization, privacy canary, security regression, integrity/restore verification, mandatory Audit durability, safe error behavior, resource recovery, and monitoring. There must be no repeated bypass, leakage, corruption, duplicate/destructive effect, partial final, false completion, silent upload, or silent data loss. `RECOVERED` does not mean `CLOSED`; Closure requires validation evidence, residual-risk decision, PIR, and tracked actions.

### Communication and escalation

Communication is factual, need-to-know, minimal, approved, and tracked. It contains Incident ID, status, verified time/environment/service impact, data classes without payload, containment state, known unknowns, required decision, next update, and controlled evidence references. It excludes credentials, content, signatures, exploit steps, raw diagnostics, unrelated user data, unverified attribution, and unsupported legal conclusions.

Security escalation is mandatory for suspected/confirmed unauthorized privileged or cross-user access; account/token/secret compromise; document/OCR/signature/restricted screenshot leakage; suspicious broad export; incorrect redaction; public/unapproved AI or AI data leakage; parser/engine/dependency compromise; Audit tampering/unavailability; corruption/silent loss/false success/duplicate destructive effect; unauthorized policy/config change; or impact whose safe recovery is unclear.

Exact contact, on-call/SOC/SIEM route, Incident Commander/RACI, severity/SLA, evidence procedure, containment authority, privacy/legal/provider/user notification, recovery sign-off, closure, and exercise cadence are not invented by this guide:

`REQUIRES_PERTAMINA_SECURITY_PROCESS_CONFIRMATION`  
`REQUIRES_PERTAMINA_STANDARD_CONFIRMATION`

---

**Keputusan tetap milik user dan Integrator.**
