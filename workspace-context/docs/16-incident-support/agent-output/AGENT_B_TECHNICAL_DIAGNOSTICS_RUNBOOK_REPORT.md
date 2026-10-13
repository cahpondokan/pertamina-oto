# AGENT B — TECHNICAL DIAGNOSTICS, RECOVERY & RUNBOOK

**Task ID:** `DOC16-AGENT-B`  
**Status:** Specialist report untuk integrasi Document 16  
**Sumber berwenang:** Document 01–15 FINAL, terutama Document 04, 05, 06, 07, 10, 12, dan 15  
**Batas:** Safe diagnostics, containment, recovery decision, validation, dan escalation. Bukan Deployment Guide; tidak berisi exploit, direct Production database/storage repair, secret handling, atau pemilihan vendor.

## Baseline terkunci

- Local-First dan offline-capable processing tetap dipertahankan. Gangguan server tidak memindahkan file lokal ke server atau public provider.
- Source file tidak ditimpa otomatis. Temp/intermediate bukan final. Output hanya dinyatakan berhasil setelah validation.
- Job state canonical: `QUEUED`, `RUNNING`, `WAITING_CONNECTION`, `WAITING_RESOURCE`, `RETRYING`, `COMPLETED`, `FAILED`, `CANCELLED`.
- Persisted state adalah source of truth. SignalR hanya notifikasi; REST/current-state reconciliation wajib setelah reconnect.
- Worker crash tidak boleh menjatuhkan Desktop UI. Retry bounded; resume hanya dari checkpoint compatible dan integrity-valid.
- SQL Server menyimpan metadata; payload file berada pada secure file/object storage. SQLite menyimpan local metadata/state, bukan user file bytes.
- Administrator/Support/Operations tidak otomatis memperoleh document-content permission. Diagnosis dimulai dari metadata aman.
- Password, token, secret, full document/OCR content, signature asset, clipboard, prompt/chat, storage key, raw exception, SQL text, dan credential tidak diminta atau dimasukkan ke evidence.
- Semua recovery Production menggunakan application/control-plane/API/workload resmi. Command deployment, migration execution, secret injection, infrastructure restart, dan restore detail tetap berada pada Document 17/prosedur operasi Pertamina.

---

# 1. Diagnostic principles

## 1.1 Urutan diagnosis aman

```text
Confirm scope and authorization
→ classify user impact and data/security risk
→ freeze unsafe new admission if needed
→ capture safe identifiers and freshness
→ read authoritative persisted state
→ distinguish dependency, capacity, configuration, compatibility, data, or code failure
→ contain with the narrowest reversible control
→ recover through official path
→ validate state, result, security, queue, monitoring, and Audit
→ communicate and escalate with safe evidence
```

## 1.2 Prinsip keputusan

1. **Safety before availability.** Jangan mengembalikan layanan dengan mematikan permission, validation, Audit, quarantine, encryption, rate limit, Local-First, atau content boundary.
2. **Authoritative state before UI symptom.** UI, SignalR, notification, cache, dan local projection dapat stale; baca REST/persisted state yang berwenang.
3. **No false success.** `COMPLETED` hanya sah setelah required output tersedia dan tervalidasi. Missing/corrupt payload tidak boleh dipresentasikan sebagai selesai.
4. **No retry storm.** Retry hanya untuk transient allowlist, memakai attempt budget, backoff + jitter, dan idempotency.
5. **No silent restart from zero.** Jika resume tidak bisa, jelaskan bahwa retry memulai ulang stage/operation yang diizinkan.
6. **Contain narrowly.** Batasi Feature, provider path, workload class, platform, atau environment yang terdampak; pertahankan safe local capability lain.
7. **Evidence minimization.** Mulai dari ID, code, version, timing, state, counter, digest/status, dan synthetic reproduction.
8. **Expected limitation is not automatically a defect.** Batas yang didokumentasikan, jujur, dan fail-safe adalah limitation. Crash, false success, corrupt output, undocumented regression, security bypass, atau data loss adalah incident/defect.
9. **Recovery is not complete at “service up.”** Wajib membuktikan data/state consistency, security control, queue drain, monitoring freshness, dan Audit health.
10. **No dangerous Production command.** Runbook ini memberikan decision/action class, bukan shell, SQL, storage, migration, or service-control commands.

## 1.3 Required runbook format

Setiap runbook di laporan ini menggunakan field:

```text
Runbook ID | Incident Type | Symptoms | Affected Components
Safe Initial Checks | Evidence | Likely Causes | Containment
Workaround | Recovery Steps | Validation | Escalation Trigger
Forbidden Actions | Related Requirement/Feature
Related Monitoring | Related Test
```

Field **Recovery Steps** selalu menjawab: retry, resume, checkpoint, restart, fallback provider, local fallback, feature flag, rollback, data validation, dan user notification.

---

---

# 2. Safe evidence

## 2.1 Diagnostic reference minimum

| Field | Aturan |
|---|---|
| Incident ID | ID case/incident resmi; tidak memuat nama/content |
| Issue ID | Jika laporan berasal dari In-App Issue Reporting |
| Job ID | Opaque; bukan permission |
| Correlation ID | Benang merah lintas component; bukan token |
| Feature ID | Canonical dan exact |
| Error Code | Stable safe code; bukan raw exception |
| Version | App, API contract, parameter schema, workflow/config/event/capability bila relevan |
| Environment | Development, QA/Test, atau Production exact |
| Platform | Desktop, Web, Phone, Tablet, API, Worker |
| Processing Mode | `LOCAL`, `SERVER`, atau approved hybrid path |
| Provider | Approved safe provider profile code saja |
| Status | Job/upload/component/compatibility state yang typed |
| Timestamp | UTC occurrence, receipt, observation, dan last-known-good |

Tambahkan bila relevan: affected user count category, workload class, queue age/depth aggregate, duration, size/page/count category, storage-free category, health freshness, retry attempt, state/row version, checkpoint availability, result expiry, safe digest comparison outcome, and configuration/policy version.

## 2.2 Evidence yang dilarang

- password/passphrase, access/refresh token, cookie, authorization header, secret, key, connection string;
- document/file bytes atau full text, OCR/translation/Markdown/summary content;
- filename/path asli secara default, local/UNC/server path, bucket/object key, signed/delegated URL;
- clipboard, signature/paraf image, redaction coordinates/text, screenshot yang belum explicit-send;
- prompt/chat/provider request-response, public AI endpoint, raw provider payload;
- raw exception/stack, SQL statement/query text, process dump, full HTTP body, unrestricted logs;
- raw identity claims, device identifier, unrestricted owner/user list.

Jika reproduksi membutuhkan konten, gunakan synthetic/minimized fixture yang disetujui. Jangan meminta user mengirim dokumen penuh melalui chat, email, Issue note, atau telemetry.

## 2.3 Evidence pack aman

```text
Incident/Issue ID
ObservedAtUtc + last-known-good
Environment/platform/component/version
Feature/Job/Correlation/Error identifiers
Authoritative status + state version + health freshness
Safe workload/resource/queue counters and trend
Configuration/policy/capability version
Exact safe actions already attempted and outcomes
Synthetic reproduction reference and result digest/status
Security/Audit health statement
Current containment and user-facing message
```

---

---

# 3. Component map

| Component | Authority/state | Key dependencies | Safe degradation/recovery boundary |
|---|---|---|---|
| Desktop | Local workspace + SQLite + app process | OS profile, filesystem, local engines, Local Worker | UI stays alive; local result/source protected |
| Web | Server session + API state | Browser, SSO, API, CSP/cookie/anti-forgery | Safe error; no offline business authority |
| Mobile/Tablet | Local curated state + API | OS lifecycle, secure store, camera/files/share, API | Curated local capability only; no silent upload |
| API | Persisted domain state | SSO/session, SQL, storage, Worker/provider | `429/503` or safe wait; no false `202` |
| SignalR | Notification projection | API auth, outbox/persisted revision | Reconnect + REST reconciliation |
| Local Worker | Local typed IPC job state | Desktop, filesystem/temp, engine, SQLite coordinator | Crash isolated from UI; compatible recovery only |
| Server Worker | Persisted Job/attempt/lease | SQL, queue, storage, provider | Lease reclaim, bounded retry, no duplicate effect |
| Queue | Persisted bounded admission | SQL/state, Worker capacity, workload policy | Fair wait/reject; local capability unaffected |
| SQL Server | Authoritative metadata/state | Network, pool, storage/log/tempdb, schema | Fail closed for durable mutation; no direct row fix |
| SQLite | Local metadata/state | Profile/key boundary, WAL/SHM, filesystem | Protect user files; no silent reset/delete |
| Secure Storage | Payload and content-bearing temp/result/version | SQL metadata, owner/purpose/state, key boundary | Withhold unavailable/corrupt result; reconcile |
| PDF Engine | Local/server adapter | Valid input, temp, memory, approved version | Expected unsupported behavior or approved fallback |
| Conversion Engine | Adapter/provider | Fonts, format, provider version, resource | Fidelity warning/approved fallback; no vendor switch |
| OCR | Local-first provider | Image quality/language/layout/resource | Honest confidence/limitation; approved fallback |
| Content Intelligence | Approved local/internal provider | Policy, consent, OCR/text extraction, quota | Disable affected path; never public AI fallback |
| SSO | Pertamina Identity + OTO authorization | IdP, session/revocation, client compatibility | Fail closed; local offline grant only within policy |
| Audit Pipeline | Owner intent/outbox + ingestion + SQL | Owning transaction, queue, schema/version | Preserve durable intent; privileged mutation fail closed |
| Issue Reporting | Issue aggregate/outbox + secure attachment | API, SQL, storage, permission | Preserve message if attachment fails; explicit retry |

Cross-component diagnostic flow:

```text
UI symptom
→ exact platform/version/environment
→ Feature/Job/Correlation/Error reference
→ current API/Job/Upload/Storage/Audit state
→ dependency health and configuration provenance
→ narrow containment
→ official recovery
→ synthetic transaction + result/state/Audit validation
```

---

---

# 4. Desktop runbooks

## RB-DESK-001 — Aplikasi tidak dapat dibuka

| Field | Isi |
|---|---|
| Runbook ID | `RB-DESK-001` |
| Incident Type | Desktop startup failure |
| Symptoms | Process tidak membuka UI, splash berhenti, atau UI langsung tertutup |
| Affected Components | Desktop host, local profile, configuration, SQLite startup, package/runtime, optional Local Worker |
| Safe Initial Checks | Confirm environment/build/platform; periksa apakah hanya satu profile/device; lihat safe app/crash health dan last startup stage; bedakan UI host dengan Worker |
| Evidence | Incident/Issue ID, timestamp, app/capability/config schema version, platform/OS family, safe Error Code/startup stage, last-known-good; tanpa dump/content |
| Likely Causes | Unsupported/incomplete release, local profile/key/SQLite issue, invalid config/schema, filesystem permission/storage, package/runtime corruption |
| Containment | Hentikan repeated launch loop; lindungi local workspace; jangan reset profile atau hapus SQLite/temp massal |
| Workaround | Gunakan approved Web/account channel untuk metadata/server work bila sesuai; jangan upload local source hanya untuk melewati startup failure |
| Recovery Steps | Retry: satu controlled relaunch setelah checks; Resume: hanya setelah app membuka recovery screen dan state cocok; Checkpoint: gunakan hanya checkpoint yang tervalidasi; Restart: app/device hanya via normal user action; Provider fallback: N/A; Local fallback: unavailable until host safe; Flag: hanya affected released Feature, bukan startup security; Rollback: approved client rollback/reinstall route milik release ops; Data validation: profile/workspace manifest + source untouched; User notification: jelaskan local state dipertahankan |
| Validation | UI terbuka; profile benar; SQLite/workspace health; Local Worker failure tidak menjatuhkan UI; synthetic local Core smoke bila released; no source loss; monitoring/Audit outbox sehat |
| Escalation Trigger | Repeated crash, profile corruption, key failure, data loss indication, security control unavailable, atau multi-device spike |
| Forbidden Actions | Hapus profile/SQLite/WAL/temp secara manual; disable encryption; collect raw dump/content; run unreviewed reinstall/rollback command |
| Related Requirement/Feature | `NFR-REC-001..005`, `FR-REC-002..006`; `FEAT-REC-001..015`, `FEAT-MON-016` |
| Related Monitoring | Client crash/error rate, app version, startup stage, local storage health |
| Related Test | Document 12 `REC-SYS-007`; client startup/recovery regression |

## RB-DESK-002 — Disk lokal penuh

| Field | Isi |
|---|---|
| Runbook ID | `RB-DESK-002` |
| Incident Type | Local disk/storage pressure |
| Symptoms | Preflight gagal, job lokal `WAITING_RESOURCE`/failed, autosave/checkpoint/temp tidak dapat ditulis |
| Affected Components | Desktop, local workspace, SQLite/WAL, temp/checkpoint, Local Worker |
| Safe Initial Checks | Periksa approved free-space metric, app-managed artifact manifest, active lease/job, growth trend; bedakan user final file dengan cache/temp |
| Evidence | Safe free-space category, artifact type/count/bytes aggregate, Job/Feature/Correlation/Error, status/attempt, timestamp |
| Likely Causes | Large input/output, cumulative temp, stuck cleanup, orphan app-managed artifact, unrelated device pressure |
| Containment | Stop new large local admission; preserve active draft/checkpoint/source/final result; notify no-silent-wait |
| Workaround | User membebaskan ruang melalui OS-approved path atau memilih authorized target dengan ruang cukup; server route hanya jika resolver/policy semula mengizinkan |
| Recovery Steps | Retry: setelah preflight ruang lolos; Resume: compatible checkpoint only; Checkpoint: jangan purge active; Restart: tidak menyelesaikan capacity sendiri; Provider fallback: N/A; Local fallback: smaller approved workload; Flag: narrow expensive Feature bila broad incident; Rollback: N/A; Data validation: staged/final/source integrity; User notification: kebutuhan ruang dan state job |
| Validation | Free-space kembali dalam approved band; SQLite writes/flush berhasil; cleanup eligible berjalan; job synthetic selesai; source/old target unchanged |
| Escalation Trigger | Ruang terus turun, cleanup tidak maju, active state terancam, filesystem error, atau banyak device terdampak |
| Forbidden Actions | Recursive delete pada user path/workspace root; hapus active checkpoint/result; klaim secure erase; pindah processing ke server diam-diam |
| Related Requirement/Feature | `FR-STOR-005..006`, `NFR-PERF-013`, `NFR-REC-001..003`; `FEAT-STOR-*`, `FEAT-REC-*` |
| Related Monitoring | Disk/temp bytes, free space, orphan/cleanup, job duration/wait reason |
| Related Test | Document 12 `REC-SYS-010`, local `POLICY_BOUNDARY/OVER_LIMIT` workload |

## RB-DESK-003 — Temp lokal tidak dapat digunakan

| Field | Isi |
|---|---|
| Runbook ID | `RB-DESK-003` |
| Incident Type | Local temp/workspace write failure |
| Symptoms | Temp create/write/rename/validation gagal; partial output tertahan; job gagal sebelum publish |
| Affected Components | App-managed private temp, Desktop, Local Worker, filesystem, SQLite artifact manifest |
| Safe Initial Checks | Periksa managed-root resolution, profile permission, quota/free space, reparse/symlink guard, active artifact state, safe Error Code |
| Evidence | Operation/Job/Correlation, artifact class/state, size category, filesystem error code yang disanitasi, version/timestamp |
| Likely Causes | ACL/profile mismatch, storage full, unsafe path/reparse detection, filesystem unavailable, stale artifact lease |
| Containment | Jangan publish partial output; stop affected admission; preserve source/old target and recovery metadata |
| Workaround | Gunakan approved alternate local save target hanya untuk final export; working temp tetap harus pada protected managed root |
| Recovery Steps | Retry: after root/space/permission health restored; Resume: verified artifact/checkpoint only; Checkpoint: retain active; Restart: app/Worker normal restart only after state persist; Provider fallback: no; Local fallback: none if protected temp unavailable; Flag: disable affected processing admission; Rollback: last validated state; Data validation: digest/length/type + atomic publish; User notification: output belum final |
| Validation | Exclusive temp create/write/flush/validate/atomic publish synthetic test berhasil; no path escape; orphan cleanup state normal |
| Escalation Trigger | Managed root resolves outside boundary, repeated permission change, integrity mismatch, or source/target risk |
| Forbidden Actions | Gunakan Global Temp/Public/Downloads sebagai working root; bypass reparse/ACL check; rename partial sebagai final |
| Related Requirement/Feature | `FR-STOR-002..006`, `SR-SEC-001..023`; `FEAT-STOR-*`, `FEAT-SEC-*` |
| Related Monitoring | Temp create/write failure, disk latency/free, orphan/cleanup, Worker error |
| Related Test | Document 12 `REC-SYS-010`, `REC-SYS-011`, local filesystem fault injection |

## RB-DESK-004 — File picker atau file permission gagal

| Field | Isi |
|---|---|
| Runbook ID | `RB-DESK-004` |
| Incident Type | Client input access failure |
| Symptoms | File tidak terlihat, picker ditolak, share/open-with gagal, atau file berubah/tidak dapat dibaca |
| Affected Components | Desktop/Mobile file adapter, OS permission, selected source, validation pipeline |
| Safe Initial Checks | Confirm user-selected exact file, current OS permission, availability/lock/change, safe file fingerprint/size/type category; do not enumerate folders |
| Evidence | Feature/platform/version, safe Error Code, source kind, selected-file fingerprint outcome, declared/detected type status; no path/content |
| Likely Causes | Permission revoked, provider/file handle expired, external app lock/change, unsupported location/type, mobile picker lifecycle |
| Containment | Stop operation before temp/job side effect; preserve existing working state; do not broaden filesystem access |
| Workaround | User reselects file through OS picker/share flow or creates an authorized local copy; no unrestricted path entry |
| Recovery Steps | Retry: after explicit reselection/permission; Resume: only if input identity unchanged; Checkpoint: invalidate if source fingerprint changed; Restart: app only if picker adapter stale; Provider fallback: platform adapter only if approved; Local fallback: user-authorized copy; Flag: no; Rollback: return to prior working state; Data validation: revalidate type/digest/size; User notification: explain access vs unsupported |
| Validation | Picker/share works on synthetic file; ownership/permission exact; changed input detected; source not modified; operation produces valid new result |
| Escalation Trigger | Cross-file access, path traversal/reparse issue, permission bypass, repeated adapter failure on supported platform |
| Forbidden Actions | Ask for whole directory access without need; paste path into privileged tool; disable sandbox/ACL; log full path/name/content |
| Related Requirement/Feature | `FR-FILE-001..015`, `UIR-UI-009..010`; platform/file-input Features |
| Related Monitoring | Client adapter error, validation status, platform/version distribution |
| Related Test | File-input integration, UAT cross-platform/file-first, security path tests |

## RB-DESK-005 — Client version unsupported

| Field | Isi |
|---|---|
| Runbook ID | `RB-DESK-005` |
| Incident Type | Client compatibility failure |
| Symptoms | `UPDATE_REQUIRED`, `UNSUPPORTED`, schema/version reject, Feature hilang setelah deploy |
| Affected Components | Desktop/Web/Mobile client, `/api/compatibility`, Feature registry, API/IPC contract |
| Safe Initial Checks | Read Current/Minimum/Contract/Capability versions, platform/channel, policy version, last compatible state; distinguish notice from hard block |
| Evidence | Four version fields, status, platform/environment, Feature/Error/Correlation, config Audit reference |
| Likely Causes | Client below minimum, newer unknown client, contract/capability mismatch, wrong channel policy, partial rollout |
| Containment | Block only incompatible/security-sensitive server action; preserve local workspace/data; show explicit update guidance |
| Workaround | Continue compatible/local eligible operation for `UPDATE_AVAILABLE`; no workaround for unsafe `UPDATE_REQUIRED/UNSUPPORTED` |
| Recovery Steps | Retry: after compatible client/policy; Resume: long job only if schema/checkpoint compatible; Checkpoint: preserve pending local state; Restart: after approved update; Provider fallback: no; Local fallback: only still-supported local capability; Flag: narrow affected Feature; Rollback: approved release/policy rollback; Data validation: stored job/result/secured compatibility; User notification: exact status without false forced-update reason |
| Validation | Compatibility matrix fixtures return expected status; supported operation works; below-minimum action fails safely; local data intact |
| Escalation Trigger | Broad unexpected block, data inaccessible, mixed-version job corruption, incorrect minimum-version change |
| Forbidden Actions | Lower minimum solely to bypass security; edit client/server version field; force update without security/compatibility reason |
| Related Requirement/Feature | `NFR-ARCH-*`, `IR-API-010`; `FEAT-ADMIN-015/022`, compatibility capability |
| Related Monitoring | Client/app/capability version, compatibility status/error rate |
| Related Test | Document 12 Section 39 compatibility/versioning; `REC-SYS-004` where rollout affects accepted jobs |

## RB-CLIENT-006 — Mobile/Tablet capability atau lifecycle gagal

| Field | Isi |
|---|---|
| Runbook ID | `RB-CLIENT-006` |
| Incident Type | Mobile/Tablet lifecycle, camera, picker, share, or secure-store capability failure |
| Symptoms | State hilang setelah background/rotation, camera/picker/share gagal, app tidak dapat resume, curated Feature dead-end |
| Affected Components | Mobile/Tablet app, OS lifecycle, secure credential/key store, SQLite/artifact root, capability adapter |
| Safe Initial Checks | Exact device/OS/app/capability version, orientation/lifecycle stage, permission/capability state, workspace/profile binding, current authoritative server state |
| Evidence | Platform/version, Feature/Job/Correlation/Error, lifecycle event/time, capability status, safe local-state revision; no device unique ID/content |
| Likely Causes | OS revoked permission, background eviction, unsupported device capability, stale state, secure-store/key issue, incorrect exposure |
| Containment | Hide/disable only affected capability with clear back path; preserve state; stop upload until explicit selection/authorization restored |
| Workaround | Approved import/share alternative or continue on Desktop/Tablet only when user explicitly chooses and secured/local transfer rules permit |
| Recovery Steps | Retry: after explicit permission/capability refresh; Resume: compatible local/server state; Checkpoint: validate profile/artifact; Restart: normal app restart; Provider fallback: approved platform adapter only; Local fallback: curated supported path; Flag: narrow wrong exposure; Rollback: approved app release; Data validation: state/input/result consistency; User notification: capability limitation vs incident |
| Validation | Background/foreground/orientation/picker/share scenario passes on real device; no state leak/account switch; result valid; no silent upload |
| Escalation Trigger | Profile cross-leak, secure-store failure, repeated state loss, exposure promises unsupported capability |
| Forbidden Actions | Disable OS security, copy Production data to debug device, collect device identifiers/content, claim Desktop parity |
| Related Requirement/Feature | `DEC-REQ-016..017`, `UIR-UI-006..010`, `FR-SCAN-*`, `FR-SIGN-*` |
| Related Monitoring | Client crash/error, capability/exposure/version, lifecycle recovery failure |
| Related Test | Document 12 cross-platform/recovery; Document 13 Mobile/Tablet adaptive UAT |

---

---

# 5. Local Worker runbooks

## RB-LWORK-001 — Local Worker tidak aktif

| Field | Isi |
|---|---|
| Runbook ID | `RB-LWORK-001` |
| Incident Type | Local Worker startup/heartbeat unavailable |
| Symptoms | Heavy local Feature cannot start; Worker health missing; UI remains open |
| Affected Components | Desktop, Local Worker, typed IPC, package/version, local resource |
| Safe Initial Checks | Confirm UI healthy, Worker version/contract, IPC health, resource/free space, last exit/error code, affected Features |
| Evidence | App/Worker/IPC contract version, Feature/Job/Correlation/Error, heartbeat freshness, process health aggregate |
| Likely Causes | Worker package/version mismatch, startup crash, IPC unavailable, insufficient resource, security/profile binding failure |
| Containment | Stop new Worker-dependent jobs; keep UI/local workspace available; mark affected Features limited/unavailable |
| Workaround | Run only safe in-process/local Features designed for it; server fallback only if resolver/policy/user notice already allow |
| Recovery Steps | Retry: one controlled Worker activation through app; Resume: compatible job/checkpoint after health; Checkpoint: preserve; Restart: Worker through official host lifecycle, not arbitrary kill; Provider fallback: approved resolver; Local fallback: in-process only where contract permits; Flag: narrow Worker-dependent Features; Rollback: approved Worker package; Data validation: IPC handshake/job/result; User notification: background capability unavailable |
| Validation | Worker heartbeat/typed handshake healthy; UI never terminated; synthetic Worker job completes; cleanup/Audit safe |
| Escalation Trigger | Repeated startup crash, contract mismatch, IPC authorization failure, broad device cohort impact |
| Forbidden Actions | Run Worker elevated, expose network port, change IPC ACL broadly, force status row, kill UI |
| Related Requirement/Feature | `NFR-ARCH-*`, `NFR-REC-*`; Worker/`FEAT-REC-*`, affected Core Feature |
| Related Monitoring | Worker heartbeat/crash/recycle, IPC failure, job admission |
| Related Test | Document 12 `REC-SYS-006`; Local Worker component tests |

## RB-LWORK-002 — Local Worker crash

| Field | Isi |
|---|---|
| Runbook ID | `RB-LWORK-002` |
| Incident Type | Local Worker process crash during job |
| Symptoms | Job stops/progress stale, Worker restarts/exits, UI tetap hidup, temp/checkpoint tersisa |
| Affected Components | Local Worker, Local Job, IPC, engine, checkpoint/temp, Desktop UI |
| Safe Initial Checks | Confirm UI alive, authoritative local job state, attempt/last checkpoint, input identity, Worker/error/resource trend |
| Evidence | Job/Feature/Correlation/Error, Worker/app/engine profile version, attempt, checkpoint summary, CPU/RAM/temp categories, timestamp |
| Likely Causes | Engine crash, memory/resource exhaustion, malformed input, provider defect, cancellation race |
| Containment | Quarantine affected input/job; stop repeated automatic retry; preserve source and validated checkpoint; keep other safe Features available |
| Workaround | Approved alternative provider/path only after validation; otherwise user can cancel and retain source/working state |
| Recovery Steps | Retry: transient allowlist + budget; Resume: only compatible checkpoint; Checkpoint: verify digest/schema/engine/stage; Restart: Worker recycle via app; Provider fallback: approved resolver only; Local fallback: alternate local provider if approved; Flag: disable affected Feature/provider profile; Rollback: approved engine build; Data validation: source/temp/final integrity; User notification: crash, retained state, available choices |
| Validation | UI alive; stale attempt cannot publish; resumed/retried synthetic result valid; no duplicate/partial final; Worker memory returns |
| Escalation Trigger | Security parser crash, repeated same-build crash, data corruption, memory leak, cross-feature spike |
| Forbidden Actions | Auto-retry permanent/security failure; upload crash input publicly; publish intermediate; change crash status manually |
| Related Requirement/Feature | `FR-REC-001..006`, `NFR-REC-001..005`; affected Feature + `FEAT-REC-*` |
| Related Monitoring | Worker crash/recycle, residual memory, attempt/error code, checkpoint/recovery |
| Related Test | Document 12 `REC-SYS-006`, `REC-SYS-014/015`; engine fault injection |

## RB-LWORK-003 — Job lokal stuck

| Field | Isi |
|---|---|
| Runbook ID | `RB-LWORK-003` |
| Incident Type | Local job makes no progress |
| Symptoms | Progress/current step tidak berubah, heartbeat ada/tidak ada, UI menunjukkan running/waiting terlalu lama |
| Affected Components | Local Job coordinator, Worker, IPC, engine/provider, local resource/checkpoint |
| Safe Initial Checks | Compare state revision/progress/heartbeat over approved window; resource saturation; cancellation capability; checkpoint freshness; input lock |
| Evidence | Job/Feature/Correlation/Status, state version, current step, duration, heartbeat, resource categories, attempt/checkpoint |
| Likely Causes | Engine hang, blocked I/O, IPC loss, resource starvation, lock, fake/stale progress projection |
| Containment | Stop new same-workload admission; keep UI responsive; offer cancel only if safe; no silent wait |
| Workaround | Other local Features remain available; approved provider fallback on a new authorized attempt only |
| Recovery Steps | Retry: only after cause/transience and cancellation/reclaim; Resume: verified checkpoint; Checkpoint: do not trust stale/unflushed; Restart: Worker after cooperative cancel/lease recovery; Provider fallback: approved; Local fallback: alternative approved engine; Flag: narrow affected path; Rollback: approved build; Data validation: no partial final/duplicate; User notification: current wait/cancel/retry options |
| Validation | State advances or reaches honest terminal; queue drains; synthetic job completes; no source/target loss; monitoring fresh |
| Escalation Trigger | Non-cancellable hang, repeated stale state, UI affected, resource does not recover, integrity uncertain |
| Forbidden Actions | Force `COMPLETED/FAILED`; kill Worker before state/evidence safe; retry in parallel; show fake ETA |
| Related Requirement/Feature | `FR-REC-*`, `UIR-UI-003..004`; `FEAT-PROG-*`, `FEAT-REC-*` |
| Related Monitoring | Job duration/current step, Worker heartbeat, IPC, resource, queue |
| Related Test | Document 12 local workload/soak, `REC-SYS-006/016` |

---

---

# 6. Web/API runbooks

## RB-API-001 — API not ready atau health-check failure

| Field | Isi |
|---|---|
| Runbook ID | `RB-API-001` |
| Incident Type | API readiness/liveness failure |
| Symptoms | `/health/ready` `503`, API request gagal, readiness stale; liveness dapat tetap normal |
| Affected Components | API, proxy, SQL/storage/security dependency, Worker admission, Web/Mobile/Desktop server routes |
| Safe Initial Checks | Distinguish live vs ready; check freshness/environment/version and safe dependency reason; compare local-only capability |
| Evidence | Incident/time, environment/API version, health status/reason code, request/Correlation/Error rate, affected route category |
| Likely Causes | Required dependency unavailable, pool/storage pressure, config/version mismatch, startup/deploy transition, security dependency failure |
| Containment | Stop new server admission; return safe `503`; preserve durable accepted jobs; keep local-capable processing available |
| Workaround | Local eligible Feature only; no anonymous/direct storage/public provider fallback |
| Recovery Steps | Retry: clients use bounded `Retry-After`; Resume: accepted jobs from persisted state; Checkpoint: preserve; Restart: infrastructure owner only via Document 17; Provider fallback: approved route only; Local fallback: yes where canonical; Flag: maintenance/narrow affected server Features; Rollback: approved deployment/config; Data validation: durable acceptance/no false 202; User notification: server path unavailable |
| Validation | Readiness fresh/normal; synthetic authenticated metadata + server job transaction succeeds; accepted jobs reconcile; security/Audit active |
| Escalation Trigger | Liveness down, repeated readiness flapping, durable state uncertain, security dependency missing, broad user impact |
| Forbidden Actions | Return ready while dependency unsafe; bypass auth; expose health topology; force restart command from manual |
| Related Requirement/Feature | `IR-API-007`, `NFR-MON-*`, `NFR-REC-*`; `FEAT-MON-014/015` |
| Related Monitoring | Liveness/readiness, API error/latency, dependency health, queue/admission |
| Related Test | Document 12 `REC-SYS-004`, `REC-SYS-008/009`, smoke tests |

## RB-API-002 — Request timeout

| Field | Isi |
|---|---|
| Runbook ID | `RB-API-002` |
| Incident Type | API/upstream timeout |
| Symptoms | `408` one-shot timeout, `504` upstream timeout before durable acceptance, client uncertainty after lost response |
| Affected Components | Client, proxy/API, upload, provider, SQL/storage, idempotency record |
| Safe Initial Checks | Determine whether durable resource/job was committed; query by idempotency key/resource/Correlation; distinguish client/proxy/provider stage |
| Evidence | Route category, status, Correlation, idempotency outcome, Job/Upload ID if any, duration/stage/version, Retry-After |
| Likely Causes | Slow client/network, provider latency, DB/storage wait, request budget mismatch, response lost after commit |
| Containment | Do not repeat with new key; stop unbounded retries; apply backpressure if shared latency |
| Workaround | Resumable upload/background job when contract recommends; local path only if resolver allows |
| Recovery Steps | Retry: same idempotency key/fingerprint; Resume: upload committed offset or job checkpoint; Checkpoint: verify; Restart: no client/server restart assumption; Provider fallback: approved resolver; Local fallback: canonical only; Flag: narrow timed-out provider path; Rollback: config/provider build if approved; Data validation: same resource/effect once; User notification: accepted vs not accepted clearly |
| Validation | No duplicate job/resource; status/resource discoverable; timeout rate normal; representative request finishes; Audit single logical effect |
| Escalation Trigger | Unknown commit state cannot reconcile, duplicate effect, sustained timeout, proxy/API budget conflict |
| Forbidden Actions | New idempotency key for same intent; mark failed without checking durable state; expose upstream detail |
| Related Requirement/Feature | `IR-API-004..006`, `NFR-PERF-009..012`; relevant Feature |
| Related Monitoring | API/provider latency, timeout stage, idempotency conflict/replay, DB/storage waits |
| Related Test | Document 12 `REC-SYS-003/004/012`; contract timeout tests |

## RB-API-003 — Spike 401/403 dan SSO/session failure

| Field | Isi |
|---|---|
| Runbook ID | `RB-API-003` |
| Incident Type | Authentication/authorization denial spike |
| Symptoms | Banyak `401`, `403`, concealed `404`, reauth loop, login succeeds tetapi action denied |
| Affected Components | Pertamina SSO, OTO session/revocation, API policy, role/permission/policy projection, client compatibility |
| Safe Initial Checks | Separate authentication (`401`) from valid identity denial (`403/404`); environment/issuer/audience/time/session/account/permission version; recent policy/role change |
| Evidence | Safe principal reference category, Error/Correlation, route/action/Feature, environment/platform/version, auth/policy decision code, timestamp; no token/claim dump |
| Likely Causes | IdP/session outage, expiry/revocation, clock/version issue, role/permission invalidation, wrong environment/audience, policy change, malicious probing |
| Containment | Fail closed; stop privileged mutation; preserve valid local offline capability only within protected grant; alert security on probe pattern |
| Workaround | Official relogin/reauth; no shared account, token copy, permission elevation, or anonymous route |
| Recovery Steps | Retry: login/reauth after dependency healthy, bounded; Resume: server intent reauthorized anew; Checkpoint: local work preserved; Restart: client session refresh only; Provider fallback: none for identity; Local fallback: eligible offline grant only; Flag: not to bypass auth; Rollback: approved policy/assignment change; Data validation: no unauthorized side effect; User notification: generic safe guidance |
| Validation | Expected allow/deny accounts behave correctly; revoked remains denied; wrong-owner denied; no token leakage; Audit Auth/privileged events healthy |
| Escalation Trigger | Suspected credential attack, cross-user allow, revocation failure, broad SSO outage, repeated reauth failure |
| Forbidden Actions | Request token/password/claim dump; disable auth; assign broad role; reveal existence/owner through errors |
| Related Requirement/Feature | `FR-AUTH-001..005`, `FR-ROLE-001..003`; `FEAT-AUTH-001..012` |
| Related Monitoring | Login success/failure, revocation, 401/403/404 trend, policy denial, probe signals |
| Related Test | Authorization/security suite; Document 12 identity tests; Audit Auth event validation |

## RB-API-004 — Rate limit atau 429 spike

| Field | Isi |
|---|---|
| Runbook ID | `RB-API-004` |
| Incident Type | Caller quota/rate/concurrency backpressure |
| Symptoms | `429` dengan `Retry-After`, caller blocked, job admission turun |
| Affected Components | API admission, subject/category quota, job reservation, upload/export/provider quota |
| Safe Initial Checks | Identify subject scope/category/effective policy version, offered vs accepted rate, reservation release, whether shared system is also saturated |
| Evidence | Route/workload, safe subject category, `Retry-After`, limit key/source/version, accepted/rejected counts, Correlation |
| Likely Causes | Legitimate burst, client retry storm, leaked reservation, policy too narrow, automation defect, abuse |
| Containment | Enforce `Retry-After`; stop retry storm; isolate offending category; protect other callers/workloads |
| Workaround | Queue/wait where contract allows; local eligible processing; smaller/bounded request |
| Recovery Steps | Retry: after `Retry-After` with jitter; Resume: existing accepted job only; Checkpoint: unchanged; Restart: no; Provider fallback: only approved; Local fallback: yes if canonical; Flag: narrow abusive/expensive path; Rollback: recent policy only with evidence/approval; Data validation: reservation/idempotency; User notification: wait/limit explanation |
| Validation | Rejection/acceptance fair; reservations release; no 503 exhaustion; request completes after window; no duplicate |
| Escalation Trigger | Cross-subject impact, policy regression, suspected abuse, reservation leak, 429 persists below expected load |
| Forbidden Actions | Remove hard ceiling/rate control; mass immediate retry; disclose exact internal capacity/topology |
| Related Requirement/Feature | `NFR-PERF-009..012`, `IR-API-*`; `FEAT-ADMIN-006..011`, `FEAT-MON-*` |
| Related Monitoring | 429 by category, Retry-After compliance, admission reservation, queue/resource |
| Related Test | Document 12 load/concurrency/stress; contract 429 vs 503 tests |

## RB-API-005 — Upload gagal

| Field | Isi |
|---|---|
| Runbook ID | `RB-API-005` |
| Incident Type | One-shot/resumable upload failure |
| Symptoms | `408/409/413/415/422/429/503`, offset/digest conflict, session failed/expired |
| Affected Components | Client, API, upload session, quarantine/temp storage, validation/security inspection |
| Safe Initial Checks | State/offset/expiry, actual size/count/type/digest status, policy/capacity, ownership/purpose, session version; no payload opening |
| Evidence | Upload/Feature/Correlation/Error, state/offset range category, declared/detected MIME status, digest match boolean, policy/version/timestamp |
| Likely Causes | Network interruption, stale offset, conflicting chunk, hard limit, invalid/malicious file, storage pressure, session expiry |
| Containment | Keep data quarantined; stop affected session; no execute until `READY`; cleanup/tombstone via lifecycle |
| Workaround | Resumable session for eligible large transfer; local processing when resolver chooses local; smaller valid input |
| Recovery Steps | Retry: same session/range/digest or same idempotency key; Resume: committed offset; Checkpoint: upload offset/digest; Restart: not required; Provider fallback: N/A; Local fallback: canonical; Flag: disable server upload path under storage pressure; Rollback: policy only if wrong; Data validation: MIME/signature/structure/digest/security; User notification: exact safe state/action |
| Validation | Session `READY`; bytes/digest/size consistent; duplicate chunk ack safe; no orphan; execution references correct upload once |
| Escalation Trigger | Integrity mismatch after committed write, quarantine escape, repeated storage failure, cross-owner access, malware/security signal |
| Forbidden Actions | Bypass validation/quarantine/limit; open payload for diagnosis; modify offset row/object; reuse failed upload as READY |
| Related Requirement/Feature | `IR-API-003/012`, `FR-PROC-004`; upload/storage/security Features |
| Related Monitoring | Upload failure by status/stage, temp capacity, digest conflict, session expiry/cleanup |
| Related Test | Document 12 `REC-SYS-002`, upload concurrency/range tests, security validation tests |

## RB-API-006 — Download gagal

| Field | Isi |
|---|---|
| Runbook ID | `RB-API-006` |
| Incident Type | Result/secured download failure |
| Symptoms | `401/403/404/409/410/416/503`, interrupted range, length/digest/type mismatch |
| Affected Components | API authorization, result/secured metadata, storage stream, client staging |
| Safe Initial Checks | Parent Job/file/version membership, owner/scope, result state/retention, ETag/range, storage health; authorize before resolution |
| Evidence | Result/Secured Version/Job/Correlation/Error, state/expiry, size/type/digest match, range/ETag status; no URL/key/content |
| Likely Causes | Permission revoked, result not ready/expired, invalid range, missing payload, storage unavailable, client network interruption |
| Containment | Withhold corrupt/missing/unauthorized payload; keep source/old target safe; do not regenerate silently |
| Workaround | Resume valid range; rerun operation only when result expired and user/input/permission still valid |
| Recovery Steps | Retry: authorized HEAD/GET/range; Resume: ETag/If-Range supported path; Checkpoint: download offset not Job checkpoint; Restart: client staging only; Provider fallback: N/A; Local fallback: local original/result if available; Flag: narrow downloads if storage unsafe; Rollback: storage metadata/object recovery official; Data validation: length/digest/type/ETag; User notification: not-ready/expired/unavailable |
| Validation | Authorized synthetic download completes/resumes; unauthorized remains denied; staging validates; no overwrite; storage/Audit healthy |
| Escalation Trigger | Metadata says available but payload missing, digest mismatch, cross-user access, repeated range corruption |
| Forbidden Actions | Generate public/permanent URL; browse object storage; bypass auth/retention; mark missing payload available |
| Related Requirement/Feature | `FR-BACKUP-004..010`, `IR-API-*`; `FEAT-BACKUP-004..006`, result Features |
| Related Monitoring | Download failure/status/range, storage latency, missing object, authorization deny |
| Related Test | Document 12 range/download and `REC-SYS-009`; security authorization tests |

## RB-API-007 — Idempotency conflict

| Field | Isi |
|---|---|
| Runbook ID | `RB-API-007` |
| Incident Type | Duplicate/replay ambiguity |
| Symptoms | `409 IDEMPOTENCY_KEY_REUSED`, in-progress conflict, duplicate client attempt, response lost after commit |
| Affected Components | API command, idempotency record, Job/upload/secured/Issue resource |
| Safe Initial Checks | Same principal/method/route/contract scope, key hash state, fingerprint equality, committed resource ID/status |
| Evidence | Key hash/reference only, operation scope, fingerprint match boolean, resource/Job/Correlation, original/current status; no raw key/body/secret |
| Likely Causes | Client reused key for different intent, response loss, concurrent duplicate, retention too short, fingerprint/version mismatch |
| Containment | Reject different fingerprint; replay same committed resource; prevent second side effect |
| Workaround | Same intent uses same key; genuinely new intent uses new key after user action |
| Recovery Steps | Retry: same key + same canonical request; Resume: existing resource/job; Checkpoint: resource state; Restart: no; Provider fallback: no effect on identity; Local fallback: only separate user intent; Flag: no; Rollback: owning resource lifecycle only; Data validation: exactly-once logical effect; User notification: request in progress/replayed/conflict |
| Validation | Concurrency test stores one logical resource; same replay returns same ID; different fingerprint consistently conflicts; Audit once |
| Escalation Trigger | Duplicate destructive/resource effect, same ID with different data, missing idempotency after commit |
| Forbidden Actions | Delete idempotency record to retry; expose raw key/fingerprint/body; use Correlation ID as dedup key |
| Related Requirement/Feature | `NFR-REC-004..005`, API idempotency contract; affected resource Feature |
| Related Monitoring | Duplicate/replay/conflict rate, record state/expiry, resource creation count |
| Related Test | Document 12 `REC-SYS-003`, idempotency storm/concurrency tests |

## RB-API-008 — Compatibility contract failure

| Field | Isi |
|---|---|
| Runbook ID | `RB-API-008` |
| Incident Type | API/Feature/workflow/config/event version mismatch |
| Symptoms | Unknown schema/version, capability stale, cursor/checkpoint/event rejected, mixed rollout failure |
| Affected Components | Client/API/Worker, Feature registry, workflow, SignalR event, configuration, persisted resources |
| Safe Initial Checks | Compare API major, Contract, Parameter/Workflow/Configuration/Event/Capability versions and supported window |
| Evidence | Exact safe versions, Feature/Job/Correlation/Error, resource state/version, platform/channel/environment |
| Likely Causes | Breaking change without compatibility path, partial deploy, stale client/cache, wrong capability snapshot, rollback mismatch |
| Containment | Fail closed for unknown/breaking combination; preserve existing resource; narrow affected Feature/channel |
| Workaround | Supported prior/current client/API path if approved; REST refresh for stale event; no schema coercion |
| Recovery Steps | Retry: after compatible contract; Resume: only compatible checkpoint/resource; Checkpoint: retain but do not consume; Restart: client refresh after release; Provider fallback: only same contract; Local fallback: supported local schema; Flag: disable affected exposure; Rollback: approved coordinated release; Data validation: old/new stored resources and result; User notification: update/unsupported |
| Validation | Version matrix passes current/oldest/below/newer; long jobs/resources survive supported rollout; no data loss |
| Escalation Trigger | Stored resource unreadable, mixed-version corruption, forced update without reason, event/state regression |
| Forbidden Actions | Edit persisted version field; ignore unknown schema; translate payload ad hoc; downgrade security validation |
| Related Requirement/Feature | `IR-API-010`, compatibility requirements; `FEAT-ADMIN-015/022` |
| Related Monitoring | Version/error distribution, capability freshness, deploy/rollback correlation |
| Related Test | Document 12 Section 39 compatibility/versioning and `REC-SYS-004/014/015` |

## RB-API-009 — Web client stale atau browser-safe flow gagal

| Field | Isi |
|---|---|
| Runbook ID | `RB-API-009` |
| Incident Type | Web UI/session/cache/anti-forgery/CSP integration failure |
| Symptoms | UI stale, action tidak sesuai API state, anti-forgery reject, browser feature dead-end, repeated reconnect |
| Affected Components | Web UI, browser session/cookie, API, SignalR, CSP/anti-forgery, feature discovery |
| Safe Initial Checks | Exact browser/app build, session/environment, API current state, cache freshness, Feature exposure/compatibility; distinguish UI from server failure |
| Evidence | Platform/browser family, app/capability version, route/action, Correlation/Error/status, safe console/network code only; no headers/body/token |
| Likely Causes | Stale client asset/state, expired session, anti-forgery mismatch, exposure mismatch, SignalR gap, unsupported browser capability |
| Containment | Stop repeated mutating clicks; reconcile via REST; hide unavailable action with clear back path |
| Workaround | Approved refresh/relogin; Desktop for local-only work when user explicitly chooses; no browser offline authority |
| Recovery Steps | Retry: after state/session refresh with same idempotency key; Resume: existing job via REST; Checkpoint: server state only; Restart: browser tab/app normal refresh; Provider fallback: no; Local fallback: Desktop/curated client, explicit; Flag: narrow broken Web exposure; Rollback: approved web release; Data validation: no duplicate mutation/result; User notification: state refreshed/action outcome |
| Validation | UI matches API/REST state; anti-forgery/session secure; action once; keyboard/browser flow works; Audit correct |
| Escalation Trigger | Duplicate mutation, session/cross-user leak, CSP/anti-forgery bypass, broad browser regression |
| Forbidden Actions | Disable CSP/anti-forgery/SameSite; inspect/log auth cookie; use GET for mutation; promise offline Web business authority |
| Related Requirement/Feature | `UIR-UI-*`, `SR-SEC-*`, Web platform Features |
| Related Monitoring | Web/API errors, session/anti-forgery reject, SignalR gap/REST reconcile, version |
| Related Test | Document 12 Web/security/platform tests; Document 13 Web UAT |

---

---

# 7. SignalR runbooks

## RB-SIG-001 — SignalR disconnected atau event gap

| Field | Isi |
|---|---|
| Runbook ID | `RB-SIG-001` |
| Incident Type | Realtime notification disconnect, duplicate, reorder, gap, or stale event |
| Symptoms | Progress berhenti di UI, Activity Center stale, reconnect loop, event duplicate/out-of-order |
| Affected Components | Client, `/hubs/activity`, authentication/session, outbox/event stream, REST Job/resource API |
| Safe Initial Checks | Confirm Job continues in persisted state; connection/session/version, stream sequence/revision, gap/duplicate code, REST availability |
| Evidence | Job/Correlation/Feature, event type/version/sequence/revision, connection state/timestamps, reconnect count, REST state version; no token/message payload |
| Likely Causes | Network interruption, session expiry/revocation, deploy/eventVersion mismatch, slow consumer/buffer, missed/out-of-order notification |
| Containment | Stop treating realtime view as authoritative; prevent terminal-state regression; reduce reconnect herd with backoff+jitter |
| Workaround | Poll/reconcile via authorized REST current state and Activity Center refresh |
| Recovery Steps | Retry: SignalR reconnect bounded+jittered; Resume: subscription from current authorization/revision, not blind replay; Checkpoint: persisted Job state; Restart: client realtime channel only; Provider fallback: N/A; Local fallback: local IPC for local jobs; Flag: disable noisy realtime UI path if REST remains; Rollback: event/client build if approved; Data validation: REST state wins, terminal monotonic; User notification: job tetap berjalan dan status dipulihkan |
| Validation | Required flow: disconnected → Job continues → reconnect → REST fetch → Activity Center correct; duplicate/stale ignored; gap recovered; no unauthorized event |
| Escalation Trigger | REST also unavailable, cross-user event, terminal regression, reconnect storm, buffer/resource exhaustion |
| Forbidden Actions | Mark Job failed/cancelled due to disconnect; use event arrival as auth/success; put token in URL/log; mutate through Hub |
| Related Requirement/Feature | `UIR-UI-003..004`, `NFR-REC-004`; `FEAT-PROG-001..009`, `FEAT-MON-013` |
| Related Monitoring | Connections, reconnect/gap/duplicate, slow consumer/buffer, REST reconciliation latency |
| Related Test | Document 12 `REC-SYS-001`; SignalR duplicate/reorder/gap concurrency tests |

---

---

# 8. Worker/Queue runbooks

## RB-QUEUE-001 — Queue depth atau WAITING_RESOURCE tinggi

| Field | Isi |
|---|---|
| Runbook ID | `RB-QUEUE-001` |
| Incident Type | Persistent queue backlog/resource wait |
| Symptoms | Depth/oldest age naik, banyak `WAITING_RESOURCE`, latency/job duration meningkat, `429/503` |
| Affected Components | Admission, workload queues, Server Worker, SQL/storage/provider, Job state |
| Safe Initial Checks | Arrival vs service/drain, oldest age, workload fairness, active/reserved capacity, CPU/RAM/storage/DB/provider health, cleanup |
| Evidence | Environment/workload, depth/age/rates aggregate, status distribution, capacity/limit policy version, affected Feature/provider, timestamps |
| Likely Causes | Demand spike, Worker/provider saturation, DB/storage pressure, leaked reservation/lease, retry storm, cleanup contention |
| Containment | Narrow expensive admission; lower concurrency if host pressured; preserve accepted jobs and local-capable routes; explicit wait/reject |
| Workaround | Local eligible operation; scheduled retry with `Retry-After`; smaller workload where user chooses |
| Recovery Steps | Retry: no mass retry; release gradually with jitter; Resume: persisted jobs/checkpoints; Checkpoint: preserve active; Restart: Worker/infrastructure owner only; Provider fallback: approved and capacity-safe; Local fallback: yes; Flag: narrow affected server Feature/workload; Rollback: recent limit/provider/config if causal; Data validation: no lost/duplicate/starved job; User notification: queue/wait state |
| Validation | Arrival ≤ service and queue drains; oldest age falls; fairness maintained; resource returns; synthetic job completes; Audit/monitoring healthy |
| Escalation Trigger | Backlog not draining, starvation, unsafe saturation, durable state uncertain, critical security/Audit job blocked |
| Forbidden Actions | Delete/reprioritize rows; raise limits blindly; mass retry; remove backpressure; hide wait state |
| Related Requirement/Feature | `NFR-PERF-009..013`, `FR-REC-*`; `FEAT-ADMIN-017`, `FEAT-MON-005`, `FEAT-PROG-*` |
| Related Monitoring | Depth/age/arrival/service/drain/fairness, capacity, status/retry |
| Related Test | Document 12 load/stress/backpressure/soak and `REC-SYS-005` |

## RB-QUEUE-002 — Retry loop atau retry rate tinggi

| Field | Isi |
|---|---|
| Runbook ID | `RB-QUEUE-002` |
| Incident Type | Retry storm/hot loop |
| Symptoms | `RETRYING` spike, attempt count cepat naik, provider/API load meningkat, queue tidak drain |
| Affected Components | Job retry policy, Worker, provider, queue, idempotency, Audit/monitoring |
| Safe Initial Checks | Error class transient/permanent/security, attempt budget/backoff/jitter, same logical Job ID, provider/circuit health, recent config |
| Evidence | Job/Feature/Correlation/Error, attempt distribution/rate, backoff policy version, provider/workload, idempotency outcome |
| Likely Causes | Permanent error misclassified, missing attempt bound, synchronized reconnect, provider busy, stale checkpoint, bug in acknowledgement |
| Containment | Stop new automatic retry for affected safe error class/path; open approved circuit/maintenance; preserve jobs/state |
| Workaround | User-visible wait/fail; local/approved provider route only if semantically valid |
| Recovery Steps | Retry: resume only after cause fixed and budget reset by official policy; Resume: compatible checkpoint; Checkpoint: validate before use; Restart: no hot-loop restart; Provider fallback: approved; Local fallback: canonical; Flag: disable affected provider/Feature path; Rollback: retry policy/build if causal; Data validation: one logical Job/effect; User notification: delayed/failed and next action |
| Validation | Retry rate normal, attempts bounded/spaced, queue drains, no duplicate effect, permanent error fails once, Audit not noisy |
| Escalation Trigger | Destructive duplicate, cost/resource runaway, security failure retried, policy cannot stop loop |
| Forbidden Actions | Reset attempt counters/delete history; retry security/validation failure; create new Job ID for same retry; disable idempotency |
| Related Requirement/Feature | `FR-REC-004..006`, `NFR-REC-005`; `FEAT-REC-*`, `FEAT-MON-*` |
| Related Monitoring | Retry/attempt/error/backoff/circuit, queue, provider capacity |
| Related Test | Document 12 provider timeout/retry storm stress, `REC-SYS-012/013` |

## RB-QUEUE-003 — Provider timeout atau busy

| Field | Isi |
|---|---|
| Runbook ID | `RB-QUEUE-003` |
| Incident Type | Approved engine/provider latency/unavailability |
| Symptoms | Provider timeout, circuit opens, jobs wait/retry/fail, optional-server Features degraded |
| Affected Components | Resolver, provider adapter, Worker, queue, affected OCR/conversion/intelligence Features |
| Safe Initial Checks | Approved provider profile/version/health/freshness, timeout/circuit/quota, affected Feature scope, local/alternate capability |
| Evidence | Provider profile code, Feature/Job/Correlation/Error, duration/status/attempt, circuit/quota summary, environment/version |
| Likely Causes | Provider outage/saturation, network/internal dependency, version mismatch, request too large, timeout policy |
| Containment | Stop new admission to affected provider path; keep other/local Features; no public fallback |
| Workaround | Approved local provider or alternate internal provider only through resolver and current policy; otherwise honest unavailable/wait |
| Recovery Steps | Retry: bounded after health/circuit window; Resume: checkpoint if provider/version compatible; Checkpoint: verify; Restart: provider owner only; Provider fallback: approved resolver; Local fallback: if capability; Flag: maintenance affected path; Rollback: approved provider/config build; Data validation: output validity/provenance; User notification: provider unavailable/limitation |
| Validation | Provider health fresh, synthetic request succeeds within approved behavior, queue drains, output valid, no public endpoint traffic |
| Escalation Trigger | Public/unapproved route attempted, repeated timeout after health green, result corruption, broad capacity loss |
| Forbidden Actions | Call provider directly; expose endpoint/credential/raw response; extend timeout/queue without headroom; public AI fallback |
| Related Requirement/Feature | `NFR-PERF-012`, `SR-SEC-021`; affected Feature and provider capability |
| Related Monitoring | Provider availability/latency/quota/circuit, affected queue/jobs |
| Related Test | Document 12 `REC-SYS-012`; provider PoC/load/fallback tests |

## RB-WORK-004 — Server Worker unavailable

| Field | Isi |
|---|---|
| Runbook ID | `RB-WORK-004` |
| Incident Type | Server Worker health/claim unavailable |
| Symptoms | No job starts, leases expire, queue grows, readiness may degrade |
| Affected Components | Server Worker, queue, SQL lease, storage/provider, deployment identity |
| Safe Initial Checks | Worker health/version, claim/lease age aggregate, DB/storage/provider readiness, resource capacity, recent deployment |
| Evidence | Worker profile/version/environment, heartbeat/lease metrics, queue/job status, safe Error/Correlation, last-known-good |
| Likely Causes | Process/service unavailable, identity/config mismatch, DB/storage dependency, resource exhaustion, incompatible build |
| Containment | Stop dependent admission or return `503`; preserve queue/persisted jobs; prevent duplicate stale Worker completion |
| Workaround | Safe local capability; approved other healthy Worker pool if architecture/ops already supports it |
| Recovery Steps | Retry: dispatch after Worker healthy; Resume: persisted job/checkpoint; Checkpoint: verify; Restart: Document 17/operations only; Provider fallback: irrelevant unless cause; Local fallback: canonical; Flag: maintenance affected server Features; Rollback: approved Worker release; Data validation: single active lease/attempt, stale completion rejected; User notification: waiting/unavailable |
| Validation | Worker claims jobs atomically; old lease cannot complete; queue drains; synthetic job/result/Audit valid |
| Escalation Trigger | Multiple stale owners, duplicate completion, lost queue, workload identity/permission failure |
| Forbidden Actions | Manually assign lease/status; share human credential; start ad hoc Worker; delete queued jobs |
| Related Requirement/Feature | `NFR-REC-*`, `NFR-PERF-*`; `FEAT-ADMIN-016/017`, `FEAT-MON-*` |
| Related Monitoring | Worker heartbeat/lease/claim/crash, queue, resource/dependency |
| Related Test | Document 12 `REC-SYS-005`; multi-worker claim/lease expiry concurrency |

## RB-WORK-005 — Failed job spike

| Field | Isi |
|---|---|
| Runbook ID | `RB-WORK-005` |
| Incident Type | Elevated terminal Job failure |
| Symptoms | `FAILED` rate naik pada Feature/provider/version/workload tertentu |
| Affected Components | Feature handler, Worker, engine/provider, input validation, storage/DB, policy/config |
| Safe Initial Checks | Segment by Feature/Error/version/provider/platform/processing mode; compare validation vs transient vs security; recent changes |
| Evidence | Aggregate failure rate/count, representative safe Job/Correlation/Error, attempt/stage/version, dependency health; no input content |
| Likely Causes | Bad release/provider, unsupported input cohort, capacity/storage, policy/schema mismatch, parser/engine defect |
| Containment | Disable/narrow affected Feature/provider/build path; preserve unaffected/local functions; quarantine suspicious inputs |
| Workaround | Documented limitation or approved alternate provider/path; no generic success output |
| Recovery Steps | Retry: only transient cohort; Resume: compatible checkpoint; Checkpoint: validate; Restart: only if Worker health cause; Provider fallback: approved; Local fallback: canonical; Flag: narrow affected exposure; Rollback: approved release/config; Data validation: output/source, failure classification; User notification: honest failure/limitation |
| Validation | Failure returns baseline; synthetic and representative safe fixtures pass/fail as expected; no corrupt/partial final; Audit correct |
| Escalation Trigger | Security parser failure, corrupt output, silent loss, cross-user effect, unknown broad spike |
| Forbidden Actions | Suppress error/monitoring; mark failed as completed; retry all; request Production documents |
| Related Requirement/Feature | `NFR-REC-*`, `BR-GOV-006..009`; affected Feature |
| Related Monitoring | Failure by Feature/error/provider/version/stage, result validation, resource |
| Related Test | Feature family/PoC tests, Document 12 stress/regression/recovery |

## RB-WORK-006 — Cancellation gagal

| Field | Isi |
|---|---|
| Runbook ID | `RB-WORK-006` |
| Incident Type | Cooperative cancellation not reaching terminal state |
| Symptoms | `cancellation.requested=true` tetapi Job tetap running; `JOB_NOT_CANCELLABLE`; late result published |
| Affected Components | API command, Job state/version, Worker safe point, provider/engine, result publication |
| Safe Initial Checks | Actor/owner permission, current state/version, canCancel, request time, Worker heartbeat/step, lease, terminal/result state |
| Evidence | Job/Feature/Correlation/Error, cancellation fields/times, state versions, attempt/Worker health; no content |
| Likely Causes | No safe point, provider not cancellable, Worker hang/lost signal, race cancel vs completion, stale UI |
| Containment | Stop duplicate cancel/retry; withhold uncertain result until authoritative terminal validation; no direct kill/status edit |
| Workaround | Wait for safe point with no-silent-wait; if contract says non-cancellable, explain and authorize result only at terminal |
| Recovery Steps | Retry: repeat cancel idempotently; Resume: N/A until terminal; Checkpoint: preserve verified; Restart: Worker only after lease/recovery policy; Provider fallback: no; Local fallback: no for same job; Flag: stop new affected jobs; Rollback: affected build if defect; Data validation: terminal monotonic/no late partial; User notification: requested vs completed cancellation |
| Validation | Cancel reaches `CANCELLED` or governed conflict; no later `COMPLETED`; temp/result lifecycle correct; Audit cancellation once |
| Escalation Trigger | Late result after cancel, nonterminal stranded job, Worker cannot reach safe point, duplicate effect |
| Forbidden Actions | Force database status; kill process before state safety; treat request receipt as terminal; delete result/object manually |
| Related Requirement/Feature | `FR-REC-003..006`; `FEAT-REC-*`, `FEAT-PROG-*` |
| Related Monitoring | Cancellation requested-to-terminal latency, race outcome, stranded nonterminal |
| Related Test | Document 12 `REC-SYS-016`; cancel vs complete/fail/checkpoint concurrency |

## RB-WORK-007 — Stale job atau expired lease

| Field | Isi |
|---|---|
| Runbook ID | `RB-WORK-007` |
| Incident Type | Persisted nonterminal Job without valid owner/progress |
| Symptoms | Old `RUNNING/WAITING/RETRYING`, no heartbeat, expired lease, stale Worker returns |
| Affected Components | Job/attempt/lease, Worker, queue/recovery, checkpoint/result |
| Safe Initial Checks | Lease owner/expiry aggregate, last state revision/progress/checkpoint, Worker health, attempt history, terminal/result state |
| Evidence | Job/Feature/Correlation/Status, attempt/lease timestamps, state version, checkpoint summary, Worker profile |
| Likely Causes | Worker crash/network partition, missed recovery dispatch, DB timing, stale completion, version mismatch |
| Containment | Prevent stale Worker from finalizing; claim recovery atomically; stop parallel manual retry |
| Workaround | User sees recovery/wait status; other jobs continue if queue fair |
| Recovery Steps | Retry: new attempt under same Job ID if policy; Resume: verified checkpoint; Checkpoint: compatibility/integrity check; Restart: Worker owner as needed; Provider fallback: approved; Local fallback: only new authorized route; Flag: stop affected workload if systemic; Rollback: recent Worker build; Data validation: one active attempt, terminal monotonic, result digest; User notification: recovering/retry/cancel choices |
| Validation | Exactly one active lease/attempt; stale completion rejected; job reaches honest terminal; no duplicate result/Audit |
| Escalation Trigger | Multiple active owners, state history conflict, checkpoint corruption, unresolved nonterminal beyond policy |
| Forbidden Actions | Edit lease/status/attempt; delete stale row; accept late output; create unrelated duplicate job |
| Related Requirement/Feature | `FR-REC-*`, `NFR-REC-*`; `FEAT-REC-*` |
| Related Monitoring | Lease expiry/reclaim/stale completion, nonterminal age, attempt count |
| Related Test | Document 12 `REC-SYS-005`; lease concurrency/fault injection |

## RB-WORK-008 — Result expired

| Field | Isi |
|---|---|
| Runbook ID | `RB-WORK-008` |
| Incident Type | Temporary Job result past retention |
| Symptoms | Manifest `EXPIRED`, content returns `410 RESULT_EXPIRED`, user cannot download |
| Affected Components | Result manifest, secure storage, retention/cleanup, Job/history UI |
| Safe Initial Checks | Owner/scope, result state/expiry/policy version, whether payload removal and tombstone agree, input still available |
| Evidence | Job/Result/Feature/Correlation, expiry/state, policy version, cleanup outcome; no storage key/content |
| Likely Causes | Expected retention lifecycle, stale UI link, cleanup timing; incident only if before visible expiry or metadata/object disagree |
| Containment | Keep expired unavailable; invalidate stale link/cache; do not resurrect deleted object |
| Workaround | User reprocesses from authorized input/history under current permission/policy or uses explicitly secured copy if one exists |
| Recovery Steps | Retry: download no; rerun is new explicit intent; Resume: no expired result; Checkpoint: only if separately valid; Restart: no; Provider fallback: current resolver on rerun; Local fallback: local input/result; Flag: no; Rollback: retention policy only if incorrect, not object resurrection; Data validation: expiry/tombstone/object consistency; User notification: expired and rerun/secured option |
| Validation | `410` stable; no access after expiry; rerun creates new authorized result; cleanup/Audit consistent |
| Escalation Trigger | Payload missing before expiry, still downloadable after expiry/revocation, policy applied incorrectly |
| Forbidden Actions | Restore object manually; extend expiry to hide issue; reuse expired `fileRef`; expose storage reference |
| Related Requirement/Feature | Result retention/API lifecycle; `FEAT-REC-*`, history/secured Features |
| Related Monitoring | Result expiry/cleanup/missing-object, rerun rate, storage growth |
| Related Test | Document 12 result expiry/soak/recovery; download lifecycle tests |

---

---

# 9. SQL Server runbooks

## RB-SQL-001 — Readiness atau connection unavailable

| Field | Isi |
|---|---|
| Runbook ID | `RB-SQL-001` |
| Incident Type | SQL connectivity/readiness failure |
| Symptoms | API/Worker readiness `503`, connection checkout timeout, server mutation unavailable |
| Affected Components | API/Worker, SQL Server/network/TLS, connection pool, all persisted server domains |
| Safe Initial Checks | Environment, readiness freshness, pool active/wait/timeout, TLS/network safe status, DB response, recent deploy/config; no connection string |
| Evidence | Component/version, safe dependency reason, pool/latency/error aggregates, Correlation/time, affected route/workload |
| Likely Causes | Network/TLS/DNS/internal service, SQL unavailable, pool exhaustion, credential/permission config, resource/storage pressure |
| Containment | Fail closed for required durable mutation/Audit intent; stop new server admission; preserve local capability and queued intents |
| Workaround | Local eligible operation; read-only cached UI only if not authority and clearly stale; no direct SQL |
| Recovery Steps | Retry: bounded transient connection policy; Resume: persisted jobs after DB health; Checkpoint: preserve external/local; Restart: DBA/infra Document 17 only; Provider fallback: N/A; Local fallback: yes; Flag: maintenance server paths; Rollback: approved config/release; Data validation: transaction/outbox/idempotency/state after recovery; User notification: server unavailable |
| Validation | Ready/connection pool normal; synthetic authorized transaction commits once; Worker claim/outbox/Audit work; no lost/duplicate state |
| Escalation Trigger | Production DB unavailable, TLS/credential anomaly, durable intent uncertain, fail-closed action bypassed |
| Forbidden Actions | Expose/edit connection string; use shared/db_owner credential; bypass TLS; direct row repair |
| Related Requirement/Feature | `DR-DB-001..007`, `NFR-REC-*`; DB/monitoring Features |
| Related Monitoring | Readiness, response/pool/waits/timeouts, API/Worker admission |
| Related Test | Document 12 `REC-SYS-008`, DB pool stress/recovery |

## RB-SQL-002 — Transaction failure

| Field | Isi |
|---|---|
| Runbook ID | `RB-SQL-002` |
| Incident Type | Transaction commit/rollback ambiguity |
| Symptoms | Command fails, response lost, partial external staging, outbox absent/pending, state conflict |
| Affected Components | Owning application service, SQL transaction, idempotency/outbox, storage/job resource |
| Safe Initial Checks | Determine before/after commit using authoritative resource/idempotency/outbox state; inspect state/row version and external artifact lifecycle |
| Evidence | Resource/Job/Correlation/Error, idempotency state, state version, transaction outcome code, outbox/reference status; no SQL/body |
| Likely Causes | Transient DB error, deadlock/timeout, constraint/concurrency conflict, crash around commit, external/DB finalization race |
| Containment | Stop duplicate/new-key retry; keep staged artifact non-final; preserve owning transaction invariants |
| Workaround | Same idempotent command after state query; no manual compensation unless official workflow |
| Recovery Steps | Retry: same key for transient/precommit; Resume: existing committed resource; Checkpoint: owning state/outbox; Restart: no assumption; Provider fallback: no; Local fallback: separate canonical intent only; Flag: stop affected mutation if systemic; Rollback: official compensation/tombstone; Data validation: atomic resource+outbox/idempotency; User notification: accepted/not accepted based on evidence |
| Validation | Exactly one resource/effect, correct outbox, no partial final, state/version valid, Audit outcome correct |
| Escalation Trigger | Commit ambiguity cannot reconcile, cross-store inconsistency, duplicate destructive effect, constraint invariant violated |
| Forbidden Actions | Edit rows to “complete”; delete idempotency/outbox; disable constraints/transaction; retry with new key |
| Related Requirement/Feature | `DR-DB-*`, idempotency/API requirements; affected domain Feature |
| Related Monitoring | Transaction/error/deadlock/timeout, outbox backlog, idempotency conflicts |
| Related Test | Document 12 `REC-SYS-003/004/008/009`, transaction fault injection |

## RB-SQL-003 — Slow query atau database pressure

| Field | Isi |
|---|---|
| Runbook ID | `RB-SQL-003` |
| Incident Type | Query latency/IO/pool pressure |
| Symptoms | API metadata slow, pool waits, SQL CPU/I/O/log/tempdb/waits naik, queue backs up |
| Affected Components | SQL/query paths, API/Worker, audit/history/secured/support/admin projections |
| Safe Initial Checks | Segment route/query category, bounded filter/page, plan/latency aggregate through approved tooling, pool/wait/deadlock, foreground vs cleanup/export |
| Evidence | Query category/operation code, duration/reads/writes aggregate, row/page count, pool/wait/resource trend, build/config; no SQL text/data |
| Likely Causes | Missing/regressed index/plan, unbounded query/export, N+1/wide projection, cleanup contention, data growth, connection leak |
| Containment | Reduce expensive admission/concurrency; pause noncritical export/cleanup through official control; enforce pagination/bounds |
| Workaround | Narrow date/filter/page; asynchronous bounded export; local capability unaffected |
| Recovery Steps | Retry: after pressure/backoff; Resume: cursor/export checkpoint; Checkpoint: cleanup/export persisted; Restart: not first response; Provider fallback: N/A; Local fallback: yes; Flag: narrow expensive query/export Feature; Rollback: approved app/query release; Data validation: same query result/authorization; User notification: delayed/limited query |
| Validation | Latency/resource/pool return within approved baseline; worst allowlisted query bounded; queue drains; no missing/duplicate rows |
| Escalation Trigger | Sustained saturation, deadlocks/data loss, authorization bypass in query, plan regression needing schema change |
| Forbidden Actions | Run arbitrary Production SQL; add/drop index/unreviewed schema; remove pagination; kill sessions indiscriminately |
| Related Requirement/Feature | `DR-DB-006`, `NFR-PERF-*`; query/monitoring Features |
| Related Monitoring | SQL duration/IO/CPU/waits/pool/tempdb/log, API latency, queue |
| Related Test | Document 12 DB performance/load/soak, worst allowlisted query tests |

## RB-SQL-004 — Migration mismatch

| Field | Isi |
|---|---|
| Runbook ID | `RB-SQL-004` |
| Incident Type | Application/schema compatibility mismatch |
| Symptoms | Startup/readiness failure, unknown/missing column/table/version, mixed nodes behave differently |
| Affected Components | API/Worker, EF migration history, SQL schema, compatibility/config |
| Safe Initial Checks | Compare approved artifact/build, expected migration/schema version, applied version through approved read-only status, deployment wave |
| Evidence | Environment/build/schema/migration version identifiers, readiness/Error/Correlation, affected module; no DDL/SQL text |
| Likely Causes | Partial/out-of-order deployment, failed migration, wrong environment, rollback without compatible schema, drift |
| Containment | Stop incompatible application admission; keep prior compatible service path only if approved; preserve data |
| Workaround | None via Admin/runbook; deployment/migration owner follows verified plan |
| Recovery Steps | Retry: no until schema/app aligned; Resume: accepted jobs after compatibility validation; Checkpoint: preserve; Restart: after official migration/deploy only; Provider fallback: no; Local fallback: local client features; Flag: narrow affected server Features; Rollback: approved compatible artifact/schema path; Data validation: migration history/invariants/backup point; User notification: server maintenance/unavailable |
| Validation | All deployables report compatible schema; smoke transactions/read/write/outbox pass; old persisted resources readable; Audit append works |
| Escalation Trigger | Production mismatch, failed/partial migration, data conversion uncertainty, rollback incompatibility |
| Forbidden Actions | Run ad hoc migration/DDL; edit migration history; disable constraint; change schema from application console |
| Related Requirement/Feature | `DR-DB-*`, `NFR-ARCH-*`; configuration/compatibility Features |
| Related Monitoring | Readiness by build/schema version, migration status, error distribution |
| Related Test | Document 12 migration compatibility/backup-restore; `REC-SYS-020` |

## RB-SQL-005 — SQL storage/log pressure

| Field | Isi |
|---|---|
| Runbook ID | `RB-SQL-005` |
| Incident Type | Database data/log/tempdb capacity pressure |
| Symptoms | Writes slow/fail, readiness warning, transaction log/storage growth, cleanup/archive backlog |
| Affected Components | SQL Server, jobs/audit/history/support/secured metadata, API/Worker |
| Safe Initial Checks | Approved capacity metrics/trend, growth by safe schema/category, log/tempdb/IO latency, cleanup/archive throughput, active transactions/leases |
| Evidence | Capacity/growth/IO/log aggregates, environment/time, workload/category/version, recent change |
| Likely Causes | Sustained growth, progress/log write storm, retention/cleanup lag, large export/delete, index amplification, transaction issue |
| Containment | Reduce admission/write-heavy/export/cleanup concurrency; protect required Audit/state; stop unsafe new content admission upstream |
| Workaround | Local-capable work; bounded reads; defer noncritical exports |
| Recovery Steps | Retry: writes after capacity healthy; Resume: persisted cleanup/archive/job checkpoints; Checkpoint: preserve; Restart: DBA only; Provider fallback: no; Local fallback: yes; Flag: narrow write-heavy server Features; Rollback: recent noisy release/config; Data validation: DB/object/outbox consistency; User notification: server limited |
| Validation | Free headroom and IO/log stable; write/read synthetic transaction passes; cleanup exceeds growth; no missing Audit/job state |
| Escalation Trigger | Out-of-space imminent, log cannot advance, Audit durability threatened, recovery objective risk |
| Forbidden Actions | Delete Audit/history/jobs ad hoc; shrink/truncate/change recovery model from runbook; disable logging/constraints |
| Related Requirement/Feature | `NFR-PERF-*`, `FR-AUDIT-*`; `FEAT-MON-008/011` |
| Related Monitoring | Data/log/tempdb size/growth/IO, cleanup/archive, transaction duration |
| Related Test | Document 12 DB/storage stress/soak/cleanup-under-load |

## RB-SQL-006 — Deadlock atau transient retry

| Field | Isi |
|---|---|
| Runbook ID | `RB-SQL-006` |
| Incident Type | Database deadlock/transient concurrency failure |
| Symptoms | Transaction aborted/retried, increased latency, conflicting secured version/job claim/config update |
| Affected Components | Owning module transaction, EF concurrency, idempotency, queue/Worker/API |
| Safe Initial Checks | Operation category, retry count/backoff, transaction duration/order, state/rowversion, duplicate logical effect |
| Evidence | Safe operation/resource type, Correlation/Error, retry/attempt, state version, deadlock/transient count; no graph/SQL data in ordinary Issue |
| Likely Causes | Lock-order conflict, long transaction, hot aggregate, concurrency race, external work held inside transaction |
| Containment | Bound retries; reduce concurrency; stop affected hot operation if repeated; keep transactions short |
| Workaround | User refreshes current state; command retries with same idempotency key when safe |
| Recovery Steps | Retry: approved transient strategy + jitter; Resume: existing resource/job; Checkpoint: state version; Restart: no; Provider fallback: no; Local fallback: separate eligible work; Flag: narrow hot path; Rollback: recent transaction/query change; Data validation: invariant/unique/idempotency; User notification: conflict/retry guidance |
| Validation | Race test yields one valid outcome; retry bounded; no duplicate version/job/audit; latency normal |
| Escalation Trigger | Persistent deadlock, invariant violation, retry storm, destructive duplicate, schema/query fix required |
| Forbidden Actions | Disable isolation/constraints; retry indefinitely; hold connection across engine/storage; direct row unlock/edit |
| Related Requirement/Feature | `DR-DB-*`, `NFR-REC-005`; affected job/secured/config Feature |
| Related Monitoring | Deadlock/transient/retry/transaction duration/lock waits |
| Related Test | Document 12 DB concurrency, fourth-version/job-claim/config-rowversion races |

## RB-SQL-007 — Audit write delay

| Field | Isi |
|---|---|
| Runbook ID | `RB-SQL-007` |
| Incident Type | Audit intent/outbox/ingestion database delay |
| Symptoms | Audit event terlambat, outbox age/backlog naik, mandatory privileged action blocked |
| Affected Components | Owning transaction/outbox, Audit dispatcher/ingestion, SQL, offline queue |
| Safe Initial Checks | Owning commit, intent durability, outbox/ingestion state/age/version, SQL health, duplicate/conflict, Audit health |
| Evidence | Audit Event/Batch/Correlation/Job/Feature, occurred/received times, safe event type/outcome, backlog/age/rate, schema version |
| Likely Causes | SQL pressure, dispatcher unavailable, unsupported event version/poison, connection issue, retry/backlog spike |
| Containment | Fail closed high-impact actions whose required intent cannot persist; preserve durable outbox; prioritize critical terminal evidence within policy |
| Workaround | Ordinary already-durable business state continues while central projection retries; no disable Audit |
| Recovery Steps | Retry: bounded idempotent dispatch/ingestion; Resume: outbox checkpoint/ack state; Checkpoint: preserve sequence; Restart: owner ops only; Provider fallback: none; Local fallback: protected offline outbox; Flag: restrict privileged mutation if unhealthy; Rollback: bad event/app version; Data validation: once-only event, occurred/received preserved; User notification: operation blocked/delayed only as appropriate |
| Validation | Audit health `HEALTHY`, backlog drains, duplicate replay stores once, query finds correct safe event, privileged transaction behavior correct |
| Escalation Trigger | Intent not durable, integrity/hold threatened, poison/hot loop, lost/duplicate/conflicting event |
| Forbidden Actions | Delete/edit Audit/outbox; disable mandatory Audit; log payload; mark health green while stale |
| Related Requirement/Feature | `FR-AUDIT-001..004`; `FEAT-AUDIT-001..008` |
| Related Monitoring | Intent/write/outbox/ingestion age/rate/retry/conflict, Audit health |
| Related Test | Document 12 `REC-SYS-017/018`; Audit replay/backlog/load tests |

---

---

# 10. SQLite runbooks

## RB-SQLITE-001 — Local database locked

| Field | Isi |
|---|---|
| Runbook ID | `RB-SQLITE-001` |
| Incident Type | SQLite writer/lock contention |
| Symptoms | Local state/autosave/job update gagal atau menunggu; database busy/locked |
| Affected Components | Desktop/Mobile SQLite, serialized writer, Worker IPC, WAL/SHM, local repository |
| Safe Initial Checks | Confirm single profile/writer ownership, pending operation/transaction duration, app/Worker state, storage/free space, WAL health |
| Evidence | Profile/device opaque ref, schema/app version, safe operation/Error, lock duration/count, Job/Correlation/time; no DB copy/content |
| Likely Causes | Multiple writer path, long transaction, crashed writer, filesystem/storage latency, antivirus/backup interference, implementation defect |
| Containment | Stop new local mutations requiring DB; preserve user files/draft artifacts; keep read-only safe view if validated |
| Workaround | Wait for bounded writer completion; user may continue only operations that do not risk state inconsistency |
| Recovery Steps | Retry: bounded busy policy after writer clears; Resume: job/workspace from committed state; Checkpoint: verify committed metadata/artifact; Restart: app normally only after write stops; Provider fallback: N/A; Local fallback: none for metadata authority; Flag: narrow autosave/job admission if systemic; Rollback: approved client build; Data validation: SQLite/artifact manifest consistency; User notification: state saving delayed |
| Validation | Single writer works, transaction latency normal, WAL checkpoint/recovery via application path, autosave/job state durable, source untouched |
| Escalation Trigger | Persistent lock, multiple profile/writer evidence, corruption, data loss, unbounded UI block |
| Forbidden Actions | Delete WAL/SHM; open DB with ad hoc editor; disable locking; copy DB while live; kill process repeatedly |
| Related Requirement/Feature | `FR-STOR-*`, `FR-REC-*`; local workspace/job Features |
| Related Monitoring | Local DB busy/transaction duration/write failure, UI stall, storage latency |
| Related Test | Document 12 local concurrency/soak and `REC-SYS-007` |

## RB-SQLITE-002 — Migration gagal

| Field | Isi |
|---|---|
| Runbook ID | `RB-SQLITE-002` |
| Incident Type | Local schema migration failure |
| Symptoms | App startup/recovery blocked, newer/unknown schema, protected migration backup present |
| Affected Components | SQLite, client app, migration service, local artifacts/profile key |
| Safe Initial Checks | App/schema version, migration stage/status, backup/manifest integrity, profile binding, free space; do not rerun blindly |
| Evidence | Version IDs, safe migration Error/stage, timestamps, backup/manifest state, app/platform/environment |
| Likely Causes | Interrupted migration, incompatible downgrade, insufficient space, corrupt DB, key/access failure, defective migration |
| Containment | Block writes and preserve original DB/artifacts/migration backup; no silent reset/new empty profile |
| Workaround | Keep app closed/read-only recovery screen; use server/account channel only for independent resources |
| Recovery Steps | Retry: only idempotent approved migration after cause fixed; Resume: supported migration checkpoint; Checkpoint: migration journal/backup manifest; Restart: after safe state; Provider fallback: N/A; Local fallback: last validated protected backup; Flag: block affected client release; Rollback: compatible app+schema path via release process; Data validation: profile/schema/artifact/recovery; User notification: local data preserved, app limited |
| Validation | Migration completes once; old data/state readable; artifacts bound; source not overwritten; newer unknown schema still fails safely |
| Escalation Trigger | Production cohort failure, backup invalid, downgrade path unclear, data conversion loss, key failure |
| Forbidden Actions | Edit schema/version manually; delete DB; use cleartext backup; rerun non-idempotent migration; downgrade blindly |
| Related Requirement/Feature | `NFR-ARCH-*`, local schema migration requirements; `FEAT-STOR-*` |
| Related Monitoring | Migration failure/version distribution/startup health |
| Related Test | Document 12 `REC-SYS-007/020`; migration upgrade/downgrade/interrupt tests |

## RB-SQLITE-003 — Corrupted local metadata

| Field | Isi |
|---|---|
| Runbook ID | `RB-SQLITE-003` |
| Incident Type | SQLite integrity/corrupt metadata |
| Symptoms | Integrity failure, invalid state graph, workspace/history/job metadata unreadable |
| Affected Components | SQLite, workspace manifest, local artifacts, recovery/history projections |
| Safe Initial Checks | Read-only integrity status through application diagnostics, last validated backup/snapshot, artifact manifest, source files separate |
| Evidence | Profile/schema/app version, safe integrity/Error code, affected entity category/count, last-good time; no DB file upload/content |
| Likely Causes | Storage/device failure, abrupt interruption, migration defect, key/crypto failure, filesystem corruption |
| Containment | Stop writes/cleanup; preserve DB/WAL/SHM/artifacts/source; isolate profile; inform user not to overwrite/export uncertain result |
| Workaround | Open external source independently; server secured resources remain separate; no new local state until recovery safe |
| Recovery Steps | Retry: no repeated writes; Resume: only from validated recovery snapshot; Checkpoint: verify artifact/digest binding; Restart: after recovery preparation; Provider fallback: N/A; Local fallback: protected last-good backup; Flag: block affected local operations; Rollback: app release if causal; Data validation: DB+artifact manifest+source/result; User notification: metadata incident, source distinction |
| Validation | Profile opens, state graph/integrity valid, required artifacts resolve within managed root, no cross-profile data, source unchanged |
| Escalation Trigger | No valid recovery snapshot, key/integrity anomaly, source/result uncertainty, multiple-device pattern |
| Forbidden Actions | Silent DB reset/delete; upload DB to Issue/chat; rebuild state by guessing paths; edit rows; overwrite source |
| Related Requirement/Feature | `FR-REC-002..006`, `SR-SEC-*`; `FEAT-REC-*`, `FEAT-STOR-*` |
| Related Monitoring | Local integrity/recovery failure, crash/storage health |
| Related Test | Document 12 `REC-SYS-007/020`; corrupted metadata recovery tests |

## RB-SQLITE-004 — Recovery state unavailable

| Field | Isi |
|---|---|
| Runbook ID | `RB-SQLITE-004` |
| Incident Type | Draft/checkpoint/recovery metadata missing or incompatible |
| Symptoms | Startup tidak menawarkan expected recovery, checkpoint cannot resume, working state incomplete |
| Affected Components | SQLite recovery metadata, operation log, snapshots/checkpoints/artifacts, app/Worker version |
| Safe Initial Checks | Workspace generation, operation/snapshot/checkpoint sequences, artifact existence/digest, app/schema/engine version, terminal job state |
| Evidence | Workspace/Job opaque ID, Feature/Correlation/Error, sequence/version/checkpoint summary, last saved time; no artifact bytes |
| Likely Causes | Cleanup/expiry, crash before commit, incompatible version/engine, missing artifact, corrupt metadata |
| Containment | Prevent silent discard/overwrite; keep source and available artifacts; show honest recovery options |
| Workaround | Retry from source only after user confirmation when resume unavailable; retain edits that can be verified |
| Recovery Steps | Retry: new attempt only if resume invalid; Resume: verified compatible checkpoint; Checkpoint: validate generation/schema/input/engine/stage/digest/expiry; Restart: app after reconciliation; Provider fallback: only new attempt; Local fallback: source+last validated snapshot; Flag: no; Rollback: compatible client if version cause; Data validation: operation replay/snapshot/result; User notification: continue/retry/cancel consequences |
| Validation | Correct options shown; invalid checkpoint rejected; valid resume continues expected stage; no edit/source loss; history/Audit correct |
| Escalation Trigger | Verified checkpoint ignored, wrong workspace restored, silent loss, recovery repeatedly fails |
| Forbidden Actions | Force checkpoint compatible; edit generation/version; silently retry from zero; delete residue before evidence |
| Related Requirement/Feature | `FR-REC-001..006`; `FEAT-REC-001..015` |
| Related Monitoring | Recovery offer/success/failure, checkpoint age/compatibility |
| Related Test | Document 12 `REC-SYS-007/014/015` |

## RB-SQLITE-005 — Pending local queue stuck

| Field | Isi |
|---|---|
| Runbook ID | `RB-SQLITE-005` |
| Incident Type | Offline intent/audit/sync queue not advancing |
| Symptoms | Pending item age/count naik, reconnect tidak mengirim, repeated retry/conflict |
| Affected Components | SQLite queue/outbox, connectivity/auth refresh, API idempotency, Audit/secured/Issue intent type |
| Safe Initial Checks | Queue type separated, current auth/policy/compatibility, oldest age/attempt/backoff, payload schema/version, ack/idempotency state |
| Evidence | Queue item opaque ID/type/status, Feature/Correlation/Error, occurred time, attempt/version/count/bytes aggregate; no content |
| Likely Causes | Auth expired, API unavailable, unsupported schema, poison item, idempotency conflict, capacity limit |
| Containment | Stop hot loop; preserve unacknowledged item; reserve capacity for terminal/critical Audit; do not conflate queues |
| Workaround | Local user work continues if eligible; server action remains pending/online-required with clear status |
| Recovery Steps | Retry: after reauth/compatibility with bounded backoff; Resume: from ack state; Checkpoint: queue sequence; Restart: app normal; Provider fallback: none; Local fallback: continue local work only; Flag: restrict new queued intent if capacity; Rollback: bad schema/client build; Data validation: once-only server effect/ack; User notification: pending/failed/action needed |
| Validation | Queue drains in order/allowed parallelism, duplicates acknowledged once, occurred/received preserved, no dropped item |
| Escalation Trigger | Critical Audit queue full/unreadable, poison blocks progress, same ID different data, cross-account queue |
| Forbidden Actions | Delete unacknowledged queue; merge Audit/upload/secured intents; reset attempts; sync content not allowlisted |
| Related Requirement/Feature | `FR-AUDIT-003`, `FR-ASSIST-009`, `FR-BACKUP-*`; relevant queue Features |
| Related Monitoring | Pending/failed/oldest age/retry/ack/conflict, connectivity/auth |
| Related Test | Document 12 `REC-SYS-017/018`, reconnect backlog herd |

## RB-SQLITE-006 — Stale cache metadata

| Field | Isi |
|---|---|
| Runbook ID | `RB-SQLITE-006` |
| Incident Type | Local projection/cache out of sync with authoritative state |
| Symptoms | Activity/Secured list/history shows old status/version, stale thumbnail/cache reference |
| Affected Components | SQLite projections/cache, REST sync, local artifact cache, UI |
| Safe Initial Checks | Determine authority per data type; compare revision/ETag/capability/policy and parent lifecycle; reauthorize before refresh |
| Evidence | Resource/Feature/Correlation, local vs server revision/state, cache type/age/version; no cached content |
| Likely Causes | Missed notification, offline period, failed sync, account switch, parent deleted/expired, bad invalidation |
| Containment | Mark stale/unknown; prevent action based solely on cache; isolate account/profile |
| Workaround | Authorized REST refresh/regenerate cache from source; UI explains stale state |
| Recovery Steps | Retry: bounded refresh; Resume: cursor/revision; Checkpoint: last server revision; Restart: UI/app if projection stuck; Provider fallback: N/A; Local fallback: local authority for truly local data; Flag: disable stale projection surface; Rollback: client build; Data validation: parent membership/ETag/owner/state; User notification: refreshed/unavailable |
| Validation | Projection matches authority, deleted/expired unavailable, no cross-account cache, regenerated thumbnail remains private |
| Escalation Trigger | Cache grants access, cross-profile leak, repeated invalidation failure, destructive action from stale state |
| Forbidden Actions | Treat cache as authorization truth; sync draft/temp/content; retain stale secured/result access |
| Related Requirement/Feature | `FR-BACKUP-008..010`, `FR-HIST-*`; cache/sync Features |
| Related Monitoring | Sync/projection lag, invalidation failures, account-switch events |
| Related Test | Document 12 selective sync/reconnect; UAT offline/recovery |

---

---

# 11. Storage runbooks

## RB-STOR-001 — Secure storage unavailable

| Field | Isi |
|---|---|
| Runbook ID | `RB-STOR-001` |
| Incident Type | Object/file storage readiness failure |
| Symptoms | Upload/finalization/download/result/secured version unavailable; `503`; object operation timeout |
| Affected Components | Secure storage, API/Worker, upload/result/secured/Issue export metadata, key boundary |
| Safe Initial Checks | Storage health/freshness/capacity, affected zone/purpose, SQL metadata state, lease/tombstone, recent change; no browse/list payload |
| Evidence | Resource/Job/Upload/Correlation/Error, zone/purpose/state, operation outcome, size category, storage profile/version |
| Likely Causes | Service/network outage, key/access policy, capacity, provider maintenance, configuration mismatch |
| Containment | Stop new content admission/promotion; keep quarantine/nonfinal state; withhold unavailable result; preserve metadata intent |
| Workaround | Local eligible processing/results; retry later; do not redirect to unapproved storage |
| Recovery Steps | Retry: bounded idempotent storage operation after health; Resume: upload/download ranges/checkpoint; Checkpoint: persisted metadata+digest; Restart: storage owner only; Provider fallback: approved storage profile only if already designed; Local fallback: local result; Flag: maintenance content server Features; Rollback: approved storage/config; Data validation: object/metadata/digest/state; User notification: unavailable/pending |
| Validation | Synthetic put/finalize/read/delete lifecycle through app succeeds; SQL/object reconcile; no cross-owner access; queue drains |
| Escalation Trigger | Missing/corrupt payload, key/integrity issue, broad outage, unauthorized object access, data loss |
| Forbidden Actions | Browse bucket/share; copy payload; expose object key/URL; change ACL/key manually; mark unavailable object available |
| Related Requirement/Feature | `DR-DB-004/005/007`, `FR-BACKUP-*`; storage/secured Features |
| Related Monitoring | Storage readiness/latency/error/capacity, transfer/finalization/cleanup |
| Related Test | Document 12 `REC-SYS-009`, storage fault injection/restore reconciliation |

## RB-STOR-002 — Storage quota penuh

| Field | Isi |
|---|---|
| Runbook ID | `RB-STOR-002` |
| Incident Type | Subject/workload/secured/temp quota exhaustion |
| Symptoms | `409` quota conflict, `429`, upload/secured save rejected, capacity warning |
| Affected Components | Admission/quota reservation, storage, upload/result/secured/Issue export |
| Safe Initial Checks | Configured/effective quota/hard ceiling/version, usage/reservation/cleanup aggregate, active leases/results/versions, growth trend |
| Evidence | Scope/category, limit key/version, usage/reservation category, resource/Correlation/Error, timestamps |
| Likely Causes | Legitimate growth, leaked reservation, retention/cleanup lag, large workload, incorrect policy |
| Containment | Reject new unsafe admission; preserve active/recoverable/user-secured content; no manual deletion |
| Workaround | User cleans eligible own resources through official lifecycle or stores final locally; wait for cleanup/expiry |
| Recovery Steps | Retry: after quota/reservation release; Resume: existing leased upload/job; Checkpoint: preserve; Restart: no; Provider fallback: no; Local fallback: yes; Flag: narrow content-heavy Feature; Rollback: wrong quota policy with approval; Data validation: usage/reservation/object count; User notification: quota and safe options |
| Validation | Usage below effective bound, reservations release, new synthetic operation accepted, active/secured max-three intact |
| Escalation Trigger | Usage mismatch, reservation leak, protected/held content threatened, pressure approaching hard capacity |
| Forbidden Actions | Delete user object/oldest version manually; raise hard ceiling blindly; count version limit as configurable beyond three |
| Related Requirement/Feature | `FR-STOR-006`, `FR-VER-001..004`; `FEAT-ADMIN-008/011`, `FEAT-VER-*` |
| Related Monitoring | Usage/quota/reservation/growth/cleanup, reject status |
| Related Test | Document 12 storage pressure/secured fourth-version/cleanup-under-load |

## RB-STOR-003 — Orphan metadata atau orphan payload

| Field | Isi |
|---|---|
| Runbook ID | `RB-STOR-003` |
| Incident Type | Cross-store reconciliation mismatch |
| Symptoms | SQL metadata references no object, object appears without final metadata, cleanup pending persists |
| Affected Components | Owning SQL schema, secure storage, tombstone/outbox/reconciliation worker |
| Safe Initial Checks | Resource state/lease/tombstone/retention/hold, storage existence/digest status through official reconciliation; distinguish staging vs final |
| Evidence | Opaque resource/object reference internal to approved tool, state/version, reconciliation Error/attempt/time; no key/content |
| Likely Causes | Crash between staging/finalization, failed delete, transaction/outbox delay, restore mismatch, storage outage |
| Containment | Withhold metadata-without-payload; quarantine object-without-authority; stop automated destructive cleanup if hold/lease uncertain |
| Workaround | Rerun from source only by owner if result unavailable; existing secured copy if authorized |
| Recovery Steps | Retry: idempotent finalize/delete/reconcile based on persisted intent; Resume: reconciliation checkpoint; Checkpoint: lease/tombstone/outbox; Restart: lifecycle Worker ops; Provider fallback: no; Local fallback: local source/result; Flag: stop affected promotion; Rollback: official compensation; Data validation: metadata/object/digest/owner/state; User notification: unavailable/retry option |
| Validation | Every accessible metadata has valid object; orphan object removed/quarantined only after proof; max-three/retention/hold intact |
| Escalation Trigger | Accessible missing payload, object exposed without metadata, hold violation, repeated reconciliation failure |
| Forbidden Actions | Delete guessed orphan; recreate metadata by browsing; expose key; mark missing payload healthy |
| Related Requirement/Feature | `DR-DB-004/005`, storage lifecycle; `FEAT-BACKUP-*`, `FEAT-VER-*` |
| Related Monitoring | Orphan/mismatch/reconciliation age/attempt, cleanup/tombstone |
| Related Test | Document 12 `REC-SYS-009/019/020` |

## RB-STOR-004 — Checksum atau integrity mismatch

| Field | Isi |
|---|---|
| Runbook ID | `RB-STOR-004` |
| Incident Type | Payload digest/length/type/ETag mismatch |
| Symptoms | Upload complete fails, download validation fails, result/secured version withheld |
| Affected Components | Client/API/Worker, storage, upload/result/secured metadata, validation pipeline |
| Safe Initial Checks | Digest algorithm/version, expected vs validated match boolean, length/type/ETag/state, transfer ranges, source identity |
| Evidence | Resource/Job/Upload/Correlation/Error, digest match outcome, size/type/range state, provider/version; no digest value on broad view/content |
| Likely Causes | Corrupt transfer/storage, conflicting chunk, wrong object binding, stale ETag, engine output corruption, implementation defect |
| Containment | Quarantine/withhold object; block execute/download/secure promotion; preserve source and evidence |
| Workaround | Re-transfer/reprocess from verified source through official flow; no use of suspect object |
| Recovery Steps | Retry: only from verified bytes/source with same logical intent as appropriate; Resume: committed ranges if digest-valid; Checkpoint: invalidate mismatched artifact; Restart: no; Provider fallback: approved after new attempt; Local fallback: verified local source/result; Flag: disable affected transfer/provider path; Rollback: bad build; Data validation: independent length/digest/type/parser; User notification: integrity failure/no result |
| Validation | New object passes all checks, suspect object inaccessible/cleaned by lifecycle, metadata points correct version, Audit security signal as required |
| Escalation Trigger | Stored secured payload changed, cross-object binding, repeated mismatch, suspected tampering/security incident |
| Forbidden Actions | Ignore/replace digest; serve suspect payload; log bytes/key; repair metadata to match corrupt object |
| Related Requirement/Feature | `SR-SEC-*`, `NFR-REC-002`; transfer/secured Features |
| Related Monitoring | Digest/length/type mismatch, quarantine, provider/build correlation |
| Related Test | Upload/download integrity, Document 12 `REC-SYS-002/009/011` |

## RB-STOR-005 — Secured version rotation issue

| Field | Isi |
|---|---|
| Runbook ID | `RB-STOR-005` |
| Incident Type | Maximum-three/current-pointer/tombstone inconsistency |
| Symptoms | Lebih dari tiga active versions, wrong current version, oldest still accessible, cleanup pending |
| Affected Components | `secured.Files/FileVersions`, storage objects, version transaction/outbox/cleanup |
| Safe Initial Checks | Active count/current pointer/version sequence/rowversion/tombstone, concurrent create/restore, cleanup/hold state |
| Evidence | Secured File/Version opaque IDs, count/state/version, Correlation/Error, transaction/reconcile time; no payload/name/key |
| Likely Causes | Concurrency race, transaction failure, stale client, cleanup failure, restore/version bug |
| Containment | Block new version mutations and content access where invariant uncertain; preserve objects/tombstones/hold |
| Workaround | Existing verified current version only if invariant/authorization valid; local file remains separate |
| Recovery Steps | Retry: idempotent owning version command/reconciliation; Resume: transaction/outbox state; Checkpoint: aggregate rowversion; Restart: no; Provider fallback: no; Local fallback: local copy; Flag: disable version create/restore path; Rollback: approved app build; Data validation: max three, current pointer, object/digest/access; User notification: version operation unavailable |
| Validation | At most three active/addressable, oldest tombstoned inaccessible, current pointer correct, cleanup retryable, Audit once |
| Escalation Trigger | Rotated version accessible, wrong version served, data loss, legal hold/retention conflict |
| Forbidden Actions | Delete object/update pointer/count via SQL; increase max; restore tombstoned access manually |
| Related Requirement/Feature | `FR-VER-001..004`; `FEAT-VER-001..006` |
| Related Monitoring | Active-version distribution, fourth-version race, rotation/cleanup failure |
| Related Test | Document 12 fourth secured version concurrency and `REC-SYS-009/019/020` |

## RB-STOR-006 — Upload staging/finalization gagal

| Field | Isi |
|---|---|
| Runbook ID | `RB-STOR-006` |
| Incident Type | Storage-layer upload/finalization failure |
| Symptoms | Upload bytes staged tetapi session tidak `READY`; metadata/object promotion incomplete |
| Affected Components | Upload session, quarantine/staging, validation, SQL finalization/outbox, storage |
| Safe Initial Checks | Upload state/offset/digest/expiry, validation status, staged object/metadata intent, lease/reconciliation |
| Evidence | Upload/Feature/Correlation/Error, state/version/offset category, validation/digest outcome, operation timestamp |
| Likely Causes | Storage/DB failure around promotion, validation failure, lease expiry, response loss, quota |
| Containment | Keep staged object quarantined; no `fileRef` execute; schedule official reconcile/cleanup |
| Workaround | Resume same session when valid or create new explicit upload after terminal/expiry; local processing if canonical |
| Recovery Steps | Retry: same idempotent complete/finalize; Resume: committed offset; Checkpoint: upload state/digest; Restart: lifecycle Worker only; Provider fallback: no; Local fallback: yes; Flag: stop uploads under pressure; Rollback: tombstone/abort staging; Data validation: READY only after full validation; User notification: upload pending/failed |
| Validation | Session reaches one correct terminal, object/state/digest consistent, no duplicate/orphan, execute accepts only READY |
| Escalation Trigger | Object promoted before validation, cross-owner binding, recurring finalize ambiguity |
| Forbidden Actions | Flip status to READY; move object manually; bypass validation; use expired session |
| Related Requirement/Feature | `IR-API-003`, upload contract; storage/upload Features |
| Related Monitoring | Upload stage/finalization/reconcile/expiry/orphan |
| Related Test | Document 12 `REC-SYS-002/003/009`, upload finalize fault injection |

## RB-STOR-007 — Download/storage stream gagal

| Field | Isi |
|---|---|
| Runbook ID | `RB-STOR-007` |
| Incident Type | Storage-layer read/range interruption |
| Symptoms | Stream stops, range invalid/stale, storage read timeout, client staging mismatch |
| Affected Components | Storage read, API content endpoint, result/secured metadata, client staging |
| Safe Initial Checks | Authorization/state/expiry, object health, ETag/length/range support, storage latency/error, client committed bytes |
| Evidence | Resource/Job/Correlation/Error, ETag/range/length state, storage profile/version, timestamps; no URL/key/content |
| Likely Causes | Storage outage, network interruption, expired/revoked resource, stale ETag, object corruption/missing |
| Containment | Stop serving suspect/missing object; preserve staged client file as nonfinal; no overwrite |
| Workaround | Resume authorized range with matching ETag or retry later; rerun only if expired/unrecoverable and user authorizes |
| Recovery Steps | Retry: authorized bounded read; Resume: Range/If-Range; Checkpoint: client offset+ETag; Restart: client staging only; Provider fallback: no; Local fallback: verified local copy; Flag: maintenance download path; Rollback: official storage recovery; Data validation: length/digest/type; User notification: interrupted/unavailable |
| Validation | Full/range synthetic read valid, wrong range `416`, expired `410`, unauthorized denied, no partial final/overwrite |
| Escalation Trigger | Repeated corruption, missing accessible payload, cross-owner read, storage integrity concern |
| Forbidden Actions | Serve from alternate unapproved location; bypass auth; expose object URL/key; rename partial final |
| Related Requirement/Feature | Download/result/secured requirements; `FEAT-BACKUP-004..006` |
| Related Monitoring | Storage read/range/timeout/missing object, download validation |
| Related Test | Document 12 range/download and `REC-SYS-009` |

---

---

# 12. PDF/Conversion runbooks

## RB-PDF-001 — Malformed atau unsupported PDF

| Field | Isi |
|---|---|
| Runbook ID | `RB-PDF-001` |
| Incident Type | Invalid, malformed, corrupted, or unsupported input |
| Symptoms | `UNSUPPORTED`, `CORRUPTED`, `422`, parser rejects; no output |
| Affected Components | File validation, PDF engine/adapter, Feature workflow, local/server Worker |
| Safe Initial Checks | Extension/MIME/signature/structure/password/security status, size/page category, engine capability/version; do not open content manually |
| Evidence | Feature/Job/Correlation/Error, validation status, detected type/structure category, provider/version, synthetic comparable fixture |
| Likely Causes | Damaged/truncated PDF, unsupported feature/encryption/object, malicious structure, engine limitation/regression |
| Containment | Reject/quarantine as required; no execute/publish; preserve source unchanged |
| Workaround | Repair PDF tool only if released/appropriate, user obtains valid copy, or documented supported format conversion |
| Recovery Steps | Retry: no unless source/release/provider changes; Resume: no invalid input; Checkpoint: invalidate; Restart: no; Provider fallback: approved repair/parser only; Local fallback: approved; Flag: disable affected parser profile if crash/security; Rollback: bad engine build; Data validation: parser/output/readability; User notification: limitation vs corrupt input |
| Validation | Invalid fixture rejected safely; supported fixture succeeds; no crash/resource bomb/partial final; error consistent |
| Escalation Trigger | Previously supported corpus regression, crash/security trigger, false accept, inconsistent provider result |
| Forbidden Actions | Bypass parser/security validation; upload to public repair service; edit bytes; call invalid result success |
| Related Requirement/Feature | `FR-FILE-010..015`, `SR-SEC-*`; relevant `FEAT-PDF-*`, Repair `FEAT-PDF-025/026` |
| Related Monitoring | Validation status/error by engine/version, parser crash/resource limit |
| Related Test | File validation/security corpus, Feature PoC/regression/fuzz-adjacent tests |

## RB-PDF-002 — Encrypted/password-protected PDF

| Field | Isi |
|---|---|
| Runbook ID | `RB-PDF-002` |
| Incident Type | Expected password requirement, invalid password, or unauthorized unlock attempt |
| Symptoms | `PASSWORD_REQUIRED`, wrong password, operation denied, unlock/protect failure |
| Affected Components | PDF validation/security engine, UI ephemeral credential handling, Unlock/Protect Feature |
| Safe Initial Checks | Confirm Feature/action/authority status and whether password is required; never request/capture password; check provider/version and safe Error Code |
| Evidence | Feature/Job/Correlation/Error, password-required/result boolean, attempt category, provider/version; no password/hash/content |
| Likely Causes | Expected encryption, wrong password, unsupported encryption scheme, authority/policy deny, engine defect |
| Containment | Fail closed; clear credential from memory after operation; do not persist/retry automatically |
| Workaround | User enters valid password in app if authorized; obtain authorized unprotected copy; no cracking/bypass |
| Recovery Steps | Retry: explicit user attempt within policy; Resume: only after credential re-entry and compatible state; Checkpoint: never stores password; Restart: clears ephemeral credential; Provider fallback: approved engine supporting scheme; Local fallback: local only if policy; Flag: disable faulty Unlock path; Rollback: engine build; Data validation: reopened result/protection state; User notification: password required/unsupported/denied |
| Validation | Correct password succeeds, wrong fails, no credential persistence/log/Audit, source remains protected, output valid |
| Escalation Trigger | Password leakage, unauthorized unlock, correct supported password fails broadly, protection semantics false |
| Forbidden Actions | Ask/log/store password; brute force/crack; bypass authority; send file to external service |
| Related Requirement/Feature | `FR-FILE-015`, `SR-SEC-003/022`; `FEAT-PDF-061/062` |
| Related Monitoring | Safe password-required/denied count, engine/version error; no credential telemetry |
| Related Test | Unlock/Protect security negative tests, Document 13 UAT Core 26/27 |

## RB-PDF-003 — Corrupted atau invalid output

| Field | Isi |
|---|---|
| Runbook ID | `RB-PDF-003` |
| Incident Type | Output validation/finalization failure |
| Symptoms | Output tidak dapat dibuka, missing pages/content, digest/type invalid, partial file published |
| Affected Components | PDF/conversion/OCR engine, Worker, temp/publish, result storage, validation |
| Safe Initial Checks | Is output final or intermediate; expected count/order/type/size/digest/readability/conformance; source/old target state; engine/version |
| Evidence | Feature/Job/Correlation/Error, input/output count categories, validation outcomes, provider/build, publish stage; synthetic fixture |
| Likely Causes | Engine defect/crash, storage/temp failure, cancellation/publish race, provider corruption, insufficient resource |
| Containment | Withhold/tombstone invalid result; preserve source/old target; disable affected engine/Feature path if systemic |
| Workaround | Retry from verified source using approved alternate provider/build only after containment |
| Recovery Steps | Retry: explicit or transient-safe new attempt; Resume: verified checkpoint before corrupt stage; Checkpoint: invalidate suspect artifact; Restart: Worker recycle if crash; Provider fallback: approved; Local fallback: canonical; Flag: disable affected path; Rollback: approved engine release; Data validation: structure/count/order/digest/open/conformance; User notification: result invalid/not delivered |
| Validation | Representative output reopens and matches oracle; no partial final; source/old target intact; monitoring/Audit accurate |
| Escalation Trigger | Silent false success, data loss, security/redaction failure, multiple Feature/provider outputs corrupt |
| Forbidden Actions | Let user download corrupt output; repair metadata only; overwrite source; suppress validation |
| Related Requirement/Feature | `NFR-REC-001..003`; affected Core Feature |
| Related Monitoring | Output-validation/publish failure by Feature/provider/version |
| Related Test | Document 12 `REC-SYS-011/013`; Feature oracle/result validation |

## RB-CONV-004 — Conversion fidelity, font, atau layout issue

| Field | Isi |
|---|---|
| Runbook ID | `RB-CONV-004` |
| Incident Type | Conversion quality limitation or defect |
| Symptoms | Font substitution, pagination/layout/table/chart/slide mismatch, editable structure poor |
| Affected Components | Conversion provider, fonts/resource profile, input format, output validator/UI limitation messaging |
| Safe Initial Checks | Exact released format/profile/provider/version, documented limitation, synthetic/approved corpus, deterministic comparison/oracle, missing-font status |
| Evidence | Feature/Job/Correlation/Error, provider/version, input/output format, page/sheet/slide category, fidelity metric/result summary; no document content |
| Likely Causes | Expected non-identical conversion, unavailable font, unsupported object/macro/media, provider regression, platform difference |
| Containment | Label result/Feature limitation; block final use when acceptance-critical output invalid; narrow bad provider/build |
| Workaround | Adjust source/print area/font through user workflow, approved alternate provider, or manual review; no claim of exact fidelity |
| Recovery Steps | Retry: only with changed supported option/provider/build; Resume: stage checkpoint if safe; Checkpoint: preserve source; Restart: no; Provider fallback: approved; Local fallback: if capable; Flag: mark limited/disable affected format; Rollback: provider build; Data validation: content/page/order/layout oracle; User notification: expected limitation vs confirmed regression |
| Validation | Approved corpus meets accepted fidelity band; unsupported elements warned; no missing content/false promise; UAT evidence |
| Escalation Trigger | Previously accepted corpus regression, material data loss, wrong values, widespread provider/version issue |
| Forbidden Actions | Call expected variation success without review; install fonts/provider ad hoc; send file externally; alter source silently |
| Related Requirement/Feature | `BR-GOV-006/007`; `FEAT-CONV-001..007`, related PDF conversion Features |
| Related Monitoring | Fidelity failures/provider/version/duration/resource; safe category only |
| Related Test | Conversion Proof-of-Capability, Document 12 Feature coverage, UAT Core 11–19 |

## RB-CONV-005 — HTML external-resource block

| Field | Isi |
|---|---|
| Runbook ID | `RB-CONV-005` |
| Incident Type | Expected security block or HTML-to-PDF rendering defect |
| Symptoms | Remote image/font/script missing, active content blocked, conversion denied/limited |
| Affected Components | `FEAT-CONV-017`, isolated renderer, egress/resource policy, conversion provider |
| Safe Initial Checks | Approved input source type, remote-resource/active-content policy, provider/version, documented limitation, safe blocked-resource count/category |
| Evidence | Feature/Job/Correlation/Error, renderer profile/version, resource category/block reason, output validation; no URL/content |
| Likely Causes | Expected egress/security restriction, unsupported CSS/resource, mutable remote dependency, renderer regression |
| Containment | Keep remote/active resource blocked; do not relax egress/sandbox; withhold unusable output |
| Workaround | User supplies approved self-contained HTML/resources or accepted static representation; no arbitrary public URL fetch |
| Recovery Steps | Retry: after approved self-contained input; Resume: no unsafe remote dependency; Checkpoint: verified local resources only; Restart: renderer Worker if defect; Provider fallback: approved isolated renderer; Local fallback: approved; Flag: disable affected HTML path; Rollback: renderer build; Data validation: output/content/resource provenance; User notification: security block/limitation |
| Validation | Remote/script access remains blocked; self-contained synthetic fixture converts; output valid; no network leakage |
| Escalation Trigger | Renderer accesses prohibited network, script executes unexpectedly, regression on approved self-contained input |
| Forbidden Actions | Whitelist arbitrary URL/domain; disable sandbox/CSP/egress; log source HTML/URL; public converter |
| Related Requirement/Feature | `SR-SEC-*`, `BR-GOV-007`; `FEAT-CONV-017` |
| Related Monitoring | Blocked-resource/security-deny, renderer error/timeout, network egress denial |
| Related Test | HTML/Web-to-PDF PoC/security/isolation tests, UAT Core 14 |

## RB-CONV-006 — PDF/A validation gagal

| Field | Isi |
|---|---|
| Runbook ID | `RB-CONV-006` |
| Incident Type | PDF/A conversion/conformance failure |
| Symptoms | Validator rejects profile, missing font/metadata/conformance issue, output not marked PDF/A |
| Affected Components | `FEAT-PDF-075`, converter, conformance validator, font/metadata handling |
| Safe Initial Checks | Requested released profile, converter/validator version, validation reason code/category, input capability, documented limitation |
| Evidence | Feature/Job/Correlation/Error, profile, converter/validator version, conformance pass/fail categories; no content |
| Likely Causes | Unsupported content/font/transparency, converter defect, wrong profile, validator mismatch, expected unrepairable input |
| Containment | Do not label output PDF/A or publish as compliant; preserve source and failed nonfinal artifact |
| Workaround | User chooses supported profile/input correction or ordinary PDF result clearly not claimed PDF/A |
| Recovery Steps | Retry: after input/profile/provider change; Resume: compatible stage; Checkpoint: preserve source; Restart: no; Provider fallback: approved converter+validator; Local fallback: if approved; Flag: disable affected profile; Rollback: converter/validator build; Data validation: independent approved conformance validation; User notification: conformance failed |
| Validation | Synthetic profile fixtures pass/fail correctly; only passing output labeled PDF/A; result opens and preserves required content |
| Escalation Trigger | False conformance, validator disagreement on approved corpus, regression, material content loss |
| Forbidden Actions | Remove/forge validation result; label nonconformant PDF/A; change profile silently; choose new validator vendor |
| Related Requirement/Feature | `BR-GOV-006/007`; `FEAT-PDF-075` |
| Related Monitoring | Conformance failure by profile/provider/version, duration/resource |
| Related Test | PDF/A PoC/conformance suite, UAT Core 19 |

## RB-CONV-007 — PDF/Conversion provider unavailable

| Field | Isi |
|---|---|
| Runbook ID | `RB-CONV-007` |
| Incident Type | PDF/conversion engine/provider health failure |
| Symptoms | Affected Feature unavailable, provider initialization/error spike, jobs wait/fail |
| Affected Components | Engine resolver, provider adapter, Local/Server Worker, affected Core Tools |
| Safe Initial Checks | Provider profile/version/health/freshness, Feature mapping, platform/resource/license approval state, fallback path |
| Evidence | Feature/provider/version/environment, Job/Correlation/Error/status, health/circuit/resource summary |
| Likely Causes | Provider process/dependency unavailable, incompatible build, license/config, resource saturation, regression |
| Containment | Disable/narrow affected provider/Features; keep unrelated/local capabilities; no vendor selection by operator |
| Workaround | Approved alternate provider or local implementation through resolver; honest unavailable if none |
| Recovery Steps | Retry: after health; Resume: compatible provider checkpoint; Checkpoint: validate version; Restart: Worker/provider owner only; Provider fallback: approved resolver; Local fallback: yes if capability; Flag: maintenance affected path; Rollback: approved provider build; Data validation: representative output; User notification: affected tools/limitation |
| Validation | Resolver selects approved healthy provider; synthetic Core transaction/result valid; queue drains; no public/unapproved calls |
| Escalation Trigger | No approved provider, license/security failure, cross-feature outage, corrupt output |
| Forbidden Actions | Install/switch vendor ad hoc; call engine directly; expose provider details/credential; public service fallback |
| Related Requirement/Feature | `TECH-DEC-006/008`, `BR-GOV-006`; affected Core Feature |
| Related Monitoring | Provider health/version/error, resolver decision, queue/resource |
| Related Test | Provider adapter/PoC/load/fallback tests, `REC-SYS-012/013` |

---

---

# 13. OCR/Intelligence runbooks

## RB-OCR-001 — OCR tidak akurat

| Field | Isi |
|---|---|
| Runbook ID | `RB-OCR-001` |
| Incident Type | OCR quality limitation or regression |
| Symptoms | Known terms tidak terbaca, character/reading order salah, confidence/hasil buruk |
| Affected Components | `FEAT-OCR-004`, OCR provider, image preprocessing, language/layout profile |
| Safe Initial Checks | Scan quality/orientation/language/layout category, released capability/provider/version, synthetic known-answer corpus, documented limitations |
| Evidence | Feature/Job/Correlation/Error, provider/version, language/layout/quality category, aggregate accuracy result; no OCR text/document |
| Likely Causes | Blur/skew/low resolution, wrong language, handwriting/stamp/multicolumn, expected engine limit, provider regression |
| Containment | Label low confidence/limitation; do not treat text as authoritative; narrow bad provider/build if regression |
| Workaround | User improves scan/orientation/language, reviews/corrects output, or uses approved alternate OCR provider |
| Recovery Steps | Retry: after preprocessing/language/provider change; Resume: page/stage checkpoint if compatible; Checkpoint: preserve verified completed pages; Restart: Worker if health issue; Provider fallback: approved; Local fallback: local-first; Flag: mark limited/disable affected profile; Rollback: provider build; Data validation: known-answer accuracy/searchability; User notification: review required |
| Validation | Approved synthetic corpus meets accepted baseline; page order/search works; inaccurate output not silently final/authoritative |
| Escalation Trigger | Material regression on accepted corpus, wrong data without warning, privacy/provider route issue |
| Forbidden Actions | Send content to public OCR; log full OCR text; claim perfect accuracy; alter source |
| Related Requirement/Feature | `FR-INTEL-*`, OCR requirements; `FEAT-OCR-004` |
| Related Monitoring | Accuracy outcome category, provider/version, duration/resource/failure |
| Related Test | OCR Proof-of-Capability/corpus tests, UAT Core 9 |

## RB-OCR-002 — Table OCR gagal

| Field | Isi |
|---|---|
| Runbook ID | `RB-OCR-002` |
| Incident Type | Complex table/layout extraction limitation |
| Symptoms | Cell/row/column structure salah, merged cells lost, reading order/value association wrong |
| Affected Components | OCR/table layout provider, preprocessing, conversion/export validator |
| Safe Initial Checks | Confirm table OCR is released/approved for profile, input quality/layout complexity, provider/version, known-answer synthetic table |
| Evidence | Feature/Job/Correlation/Error, table complexity/quality category, provider/version, structural accuracy summary; no table content |
| Likely Causes | Expected complex-layout limitation, low-quality scan, unsupported table structure, provider regression |
| Containment | Do not publish structured table as verified; label/manual review; disable profile if unsafe |
| Workaround | Plain searchable OCR, image retention, manual correction, or approved specialized provider if already approved |
| Recovery Steps | Retry: with improved scan/preprocess/provider; Resume: compatible page/table checkpoint; Checkpoint: validate completed structures; Restart: no unless Worker issue; Provider fallback: approved; Local fallback: plain OCR; Flag: limit complex-table option; Rollback: provider build; Data validation: cell/value/row/column oracle; User notification: structured output requires review |
| Validation | Known table fixtures meet approved structural criteria; failures are honest, no fabricated cells/formulas |
| Escalation Trigger | Wrong critical values, regression, feature claims support without evidence, unapproved provider path |
| Forbidden Actions | Invent missing values/formulas; send to public service; hide confidence/limitation |
| Related Requirement/Feature | `BR-GOV-006/007`, OCR/Conversion Features |
| Related Monitoring | Table-profile failure/accuracy/provider/resource |
| Related Test | Complex table OCR PoC, PDF-to-Excel UAT/validation |

## RB-INTEL-003 — Summarizer/Translate provider unavailable

| Field | Isi |
|---|---|
| Runbook ID | `RB-INTEL-003` |
| Incident Type | Content Intelligence provider outage/degradation |
| Symptoms | Summarizer/Translate unavailable, jobs wait/fail, circuit/timeout/quota warning |
| Affected Components | `FEAT-INTEL-001/002`, Content Intelligence resolver/provider, Worker/queue, policy |
| Safe Initial Checks | Approved local/internal provider health/version/freshness/quota/circuit, affected language/Feature, local deterministic prerequisites |
| Evidence | Feature/Job/Correlation/Error, safe provider profile/version/status, duration/quota/circuit, environment; no prompt/content/response |
| Likely Causes | Internal provider outage/saturation, quota, policy/config/version, OCR prerequisite, resource |
| Containment | Disable/maintenance affected intelligence path; preserve deterministic/local PDF tools; no public fallback |
| Workaround | Retry later, approved local provider if available, PDF-to-Markdown deterministic path where it satisfies separate user intent |
| Recovery Steps | Retry: bounded after health/circuit; Resume: compatible stage/checkpoint; Checkpoint: no provider payload, only safe stage state; Restart: provider owner; Provider fallback: approved internal/local only; Local fallback: yes if approved; Flag: affected Feature; Rollback: approved provider/config; Data validation: grounded output/provenance; User notification: provider unavailable |
| Validation | Synthetic/minimized request succeeds, queue drains, correct provider profile, no public traffic/content telemetry, output review available |
| Escalation Trigger | Public endpoint attempted, content retention/leak concern, broad outage, repeated corrupt/unsafe output |
| Forbidden Actions | Send content to public AI/translation endpoint; expose endpoint/credential; store prompt/response in logs/Audit |
| Related Requirement/Feature | `FR-INTEL-001..003`, `SR-SEC-021`; `FEAT-INTEL-001/002` |
| Related Monitoring | Provider availability/latency/quota/circuit, intelligence queue/error |
| Related Test | Content Intelligence PoC/security/load, `REC-SYS-012`, UAT Core 31/32 |

## RB-INTEL-004 — Unsafe atau low-quality intelligence output

| Field | Isi |
|---|---|
| Runbook ID | `RB-INTEL-004` |
| Incident Type | Hallucination, mistranslation, unsafe/untrusted output, or grounding failure |
| Symptoms | Summary invents/omits material facts, translation changes meaning/number/term, instruction injection influences behavior |
| Affected Components | Intelligence provider, grounding/review UI, policy/content isolation, Assistant orchestration |
| Safe Initial Checks | Compare against approved synthetic known-answer corpus; verify provider/version/policy, grounding/provenance, document treated as data; no Production content collection |
| Evidence | Feature/Job/Correlation/Error, output-quality category/score, provider/version, policy/grounding result; no generated/source text |
| Likely Causes | Model/provider limitation/regression, prompt injection, OCR/extraction error, wrong language/context, inadequate review guard |
| Containment | Withhold unsafe output; disable affected provider/profile/Feature; preserve source; alert Security for injection/boundary issue |
| Workaround | Human review, deterministic extraction, approved prior provider/build, or no result |
| Recovery Steps | Retry: only after changed approved provider/input extraction/guard; Resume: no unsafe response reuse; Checkpoint: invalidate affected stage; Restart: no; Provider fallback: approved; Local fallback: deterministic extraction; Flag: disable affected intelligence path; Rollback: provider/prompt-template policy build; Data validation: source-grounded known-answer review; User notification: output unreliable/not delivered |
| Validation | Known-answer safety/quality suite passes; prompt injection cannot change policy/tool; output labeled/reviewable; no content leakage |
| Escalation Trigger | Security policy/tool bypass, material false output broadly, restricted content sent/stored, public provider |
| Forbidden Actions | Treat AI output as trusted command/fact; hide limitation; log source/output; weaken confirmation/permission |
| Related Requirement/Feature | `SR-SEC-013..021`, `FR-ASSIST-*`, `FR-INTEL-*`; relevant Intelligence Feature |
| Related Monitoring | Safe quality/grounding/injection-deny outcome, provider/version/error |
| Related Test | Intelligence security/grounding/prompt-injection tests, UAT Core 31/32 |

## RB-INTEL-005 — Policy denial

| Field | Isi |
|---|---|
| Runbook ID | `RB-INTEL-005` |
| Incident Type | Expected classification/provider/permission denial |
| Symptoms | `403`, `ONLINE_REQUIRED`, provider disallowed, content transfer denied |
| Affected Components | Authorization/policy/classification, intelligence resolver, client UI |
| Safe Initial Checks | Current permission, owner/scope, classification/policy version, provider approval, consent/connectivity; distinguish expected deny from config defect |
| Evidence | Feature/Correlation/Error, safe policy decision code/version, environment/platform/provider profile, timestamp; no classification content/detail beyond scope |
| Likely Causes | Correct high-classification block, missing permission/consent, provider not approved, stale policy/client, misconfiguration |
| Containment | Maintain deny; no provider/route bypass; retain local source/workspace |
| Workaround | Approved local deterministic operation if policy allows; request policy/permission through official governance, not incident bypass |
| Recovery Steps | Retry: only after legitimate current authorization/policy change; Resume: reauthorize pending intent; Checkpoint: preserve local state only; Restart: session refresh; Provider fallback: only approved policy; Local fallback: permitted; Flag: no bypass; Rollback: unauthorized/bad policy change through control plane; Data validation: zero unauthorized transfer/effect; User notification: safe denial/route |
| Validation | Allowed/denied fixtures match policy; no server/public transfer on deny; Audit privileged denial where required |
| Escalation Trigger | Denied content transferred, unexpected broad deny after change, permission/policy inconsistency |
| Forbidden Actions | Override classification/permission; change provider directly; ask user to reclassify falsely; public AI |
| Related Requirement/Feature | `SR-SEC-016/021`, `FR-INTEL-*`; relevant Feature |
| Related Monitoring | Policy deny by safe code/Feature/provider, zero-transfer assertion |
| Related Test | Authorization/policy negative tests, Intelligence security UAT |

## RB-INTEL-006 — Document terlalu besar atau OCR dependency gagal

| Field | Isi |
|---|---|
| Runbook ID | `RB-INTEL-006` |
| Incident Type | Intelligence input/resource/preprocessing limit |
| Symptoms | `413/422/429/503`, text extraction/OCR stage fails, context/output limit reached |
| Affected Components | Preflight, OCR/extraction, intelligence provider, queue/temp/storage |
| Safe Initial Checks | Effective byte/page/context/output/timeout limits, input type/scan status, OCR availability, cumulative resource/preflight, policy version |
| Evidence | Feature/Job/Correlation/Error, size/page/context category, failed stage, provider/OCR version, limit key/source |
| Likely Causes | Expected hard limit, insufficient resource/temp, OCR/provider outage, oversized extraction, wrong workload profile |
| Containment | Reject before unsafe transfer/processing where possible; stop repeated large admission; preserve source |
| Workaround | User selects smaller page range/section, performs approved OCR first, or uses deterministic PDF-to-Markdown for separate need |
| Recovery Steps | Retry: after bounded scope/resource/dependency; Resume: page/stage checkpoint if compatible; Checkpoint: completed pages/stage only; Restart: no unless Worker issue; Provider fallback: approved; Local fallback: local OCR/extraction if capable; Flag: narrow large-input provider path; Rollback: wrong limit/config; Data validation: selected scope/result completeness; User notification: limit and safe options |
| Validation | Boundary fixtures: below/equal/above behave correctly; scoped input completes; no truncation presented as full; resource normal |
| Escalation Trigger | Below-limit input fails broadly, silent truncation, resource runaway, limit mismatch across layers |
| Forbidden Actions | Raise limit blindly; truncate without disclosure; send large content public; buffer whole file unnecessarily |
| Related Requirement/Feature | `NFR-PERF-003/012/013`, `FR-INTEL-*`; OCR/Intelligence Features |
| Related Monitoring | Limit rejects, preprocessing/OCR failure, resource/context/quota, queue |
| Related Test | Document 12 boundary/load/stress, OCR/Intelligence large corpus PoC |

## RB-INTEL-007 — Intelligence response timeout

| Field | Isi |
|---|---|
| Runbook ID | `RB-INTEL-007` |
| Incident Type | Provider response timeout/ambiguous completion |
| Symptoms | Job waits/retries/fails, provider response not received, user sees no progress |
| Affected Components | Intelligence Worker/provider, queue/job/idempotency, timeout/circuit, UI progress |
| Safe Initial Checks | Durable Job/attempt state, provider request stage, timeout/circuit/attempt budget, checkpoint, result state |
| Evidence | Job/Feature/Correlation/Error, provider profile/version, duration/stage/attempt/status, timeout policy version; no request/response |
| Likely Causes | Provider saturation/outage, network, oversized request, timeout policy, lost acknowledgement |
| Containment | Open approved circuit/stop admission; no parallel duplicate; no public fallback; show wait/failure |
| Workaround | Approved local provider/deterministic operation or retry later |
| Recovery Steps | Retry: bounded same logical Job after ambiguity reconciled; Resume: stage checkpoint if provider semantics allow; Checkpoint: safe stage metadata only; Restart: provider owner; Provider fallback: approved; Local fallback: yes if capable; Flag: maintenance path; Rollback: provider/config build; Data validation: one result, grounded/complete; User notification: timeout/retry/wait |
| Validation | Timeout rate normal, retry bounded, no duplicate result/provider call effect, output valid, queue drains |
| Escalation Trigger | Ambiguous duplicate billing/effect, sustained timeout, unsafe late response published, content boundary concern |
| Forbidden Actions | Retry immediately with new Job/key; accept late stale response; expose/store provider payload; public AI fallback |
| Related Requirement/Feature | `NFR-PERF-012`, `NFR-REC-*`; `FEAT-INTEL-001/002` |
| Related Monitoring | Provider/job duration/timeout/attempt/circuit/queue |
| Related Test | Document 12 `REC-SYS-012`, timeout/idempotency/retry tests |

---

---

# 14. Audit/Issue runbooks

## RB-AUD-001 — Audit pipeline delayed atau unhealthy

| Field | Isi |
|---|---|
| Runbook ID | `RB-AUD-001` |
| Incident Type | Audit intent/outbox/dispatcher/ingestion/query health degradation |
| Symptoms | Event tidak muncul, backlog/oldest age naik, health `DEGRADED/UNHEALTHY`, privileged action blocked |
| Affected Components | Owner transaction/outbox, local offline queue, dispatcher, ingestion, SQL Audit, query/export |
| Safe Initial Checks | Intent durability, event/batch ID, occurred/received time, outbox/ingestion state/version/age, SQL/queue/compatibility health |
| Evidence | Audit Event/Batch/Correlation/Job/Feature, safe event type/version/outcome/reason, backlog/age/rate/health; no payload |
| Likely Causes | SQL/connection pressure, dispatcher/ingestion failure, poison/unsupported version, queue full, authorization/query issue |
| Containment | Fail closed actions whose Audit intent must be durable; preserve queues; stop purge/archive; prioritize integrity over availability |
| Workaround | Already-durable intent may project later; local ordinary work only per approved degraded policy; no disable Audit |
| Recovery Steps | Retry: bounded idempotent dispatch/ingest; Resume: sequence/ack/checkpoint; Checkpoint: preserve outbox/order; Restart: Audit workload owner only; Provider fallback: none; Local fallback: protected offline outbox; Flag: restrict privileged actions; Rollback: incompatible event/app build; Data validation: once-only append/occurred-received/order; User notification: action blocked/delayed where relevant |
| Validation | Audit health healthy, backlog drains, event query correct, duplicate stores once, no missing mandatory event, monitoring fresh |
| Escalation Trigger | Intent cannot persist, integrity/hold/purge risk, unreadable queue, lost/conflicting evidence |
| Forbidden Actions | Disable/delete/edit Audit; drop poison event; reset sequence; log payload; claim healthy while stale |
| Related Requirement/Feature | `FR-AUDIT-001..004`; `FEAT-AUDIT-001..008` |
| Related Monitoring | Intent/outbox/ingestion/query/export/retention health, oldest age/retry/conflict |
| Related Test | Document 12 `REC-SYS-017/018/019/020`; Audit load/replay tests |

## RB-AUD-002 — Duplicate atau integrity conflict Audit

| Field | Isi |
|---|---|
| Runbook ID | `RB-AUD-002` |
| Incident Type | Audit replay/deduplication/integrity conflict |
| Symptoms | Duplicate event delivery, same Event ID different fingerprint, sequence conflict, event overwritten suspicion |
| Affected Components | Audit ID/fingerprint, ingestion batch, append path, outbox/retry, SQL constraints |
| Safe Initial Checks | Event ID/fingerprint match boolean, source scope/sequence, original event presence, ingestion acknowledgement, event version |
| Evidence | Event/Batch/Correlation, safe type/version/outcome, duplicate/conflict code, source/sequence/time; no metadata payload dump |
| Likely Causes | Ack lost/replay, client bug reused ID, corruption/tampering, incorrect dedup scope, restore/replay issue |
| Containment | Ack same fingerprint once; reject/quarantine same ID different data; stop affected source ingestion if integrity risk |
| Workaround | None for conflicting evidence; valid duplicate replay can be safely acknowledged |
| Recovery Steps | Retry: same valid event ID/fingerprint; Resume: source sequence after ack; Checkpoint: ingestion batch/sequence; Restart: no; Provider fallback: none; Local fallback: retain protected outbox; Flag: stop affected source/version; Rollback: bad client/build; Data validation: append-only original intact/unique; User notification: operational/security escalation, not content |
| Validation | Valid replay stores once, conflict rejected/signaled, original unchanged, subsequent sequence advances, backup/restore integrity intact |
| Escalation Trigger | Same ID different event, missing/modified original, suspected tampering, widespread sequence conflict |
| Forbidden Actions | Update original; delete duplicate/conflict evidence blindly; use Correlation ID for dedup; add crypto chain without decision |
| Related Requirement/Feature | `FR-AUDIT-003/004`; `FEAT-AUDIT-*` |
| Related Monitoring | Duplicate/conflict/rejected/unsupported, source sequence, ingestion health |
| Related Test | Audit idempotency/concurrency/restore; `REC-SYS-018/020` |

## RB-ISSUE-003 — Issue submission gagal

| Field | Isi |
|---|---|
| Runbook ID | `RB-ISSUE-003` |
| Incident Type | In-App Issue message submission failure |
| Symptoms | Issue tidak mendapat ID/status, response lost, duplicate submission/conflict |
| Affected Components | Client Issue draft, API, support SQL/idempotency/outbox, authorization |
| Safe Initial Checks | Current reporter/session, idempotency outcome, Issue ID/status, message schema/size, service/SQL health; do not copy message into logs |
| Evidence | Issue/Correlation/Error, reporter scope category, submission/idempotency/status/version/timestamp; message content excluded |
| Likely Causes | Network/API/SQL failure, response lost after commit, invalid/oversized message, auth/session, service outage |
| Containment | Preserve protected local draft; stop duplicate new-key submit; no automatic screenshot/content attachment |
| Workaround | Retry same logical submission/idempotency key; official support route if configured, with safe reference only |
| Recovery Steps | Retry: same key/fingerprint; Resume: existing Issue resource/status; Checkpoint: local draft + submission state; Restart: app normal; Provider fallback: none; Local fallback: draft only; Flag: issue service maintenance; Rollback: bad app/API release; Data validation: one Issue/outbox/status; User notification: submitted vs pending/failed |
| Validation | One Issue created, reporter can view safe status, duplicate replay same ID, Support queue receives once, Audit event safe |
| Escalation Trigger | Message lost after reported success, cross-reporter access, repeated service outage, content leaked into telemetry |
| Forbidden Actions | Paste Issue text into Audit/log/chat; create duplicate manually; auto-attach document/screenshot; direct SQL insert |
| Related Requirement/Feature | `FR-SUP-001..007`, `SR-SEC-019/020`; `FEAT-SUPPORT-002/004..006` |
| Related Monitoring | Issue submission/error/idempotency/outbox/backlog, authorization |
| Related Test | Issue API/E2E/idempotency/recovery tests; UAT Issue Reporting |

## RB-ISSUE-004 — Issue attachment gagal

| Field | Isi |
|---|---|
| Runbook ID | `RB-ISSUE-004` |
| Incident Type | Explicit screenshot/attachment upload/validation/storage failure |
| Symptoms | Message Issue berhasil tetapi attachment pending/failed; validation/limit/storage error |
| Affected Components | Client preview/staging, attachment API, validation, secure storage, Issue relation |
| Safe Initial Checks | Confirm explicit preview/send state, parent Issue/reportership, upload/validation/storage status, size/type/limit; do not open payload |
| Evidence | Issue/Attachment/Upload/Correlation/Error, explicit-send state, type/size category, validation/storage state; no screenshot bytes/content |
| Likely Causes | User cancelled, invalid/oversized image, network/storage, idempotency, parent scope, permission/session |
| Containment | Preserve Issue message; keep attachment private/pending/quarantined; never auto-resend/capture |
| Workaround | Send Issue without attachment or explicitly retry/remove/re-capture after user preview |
| Recovery Steps | Retry: explicit same attachment/key after user choice; Resume: upload session offset if supported; Checkpoint: local staging short-lived; Restart: app only; Provider fallback: no; Local fallback: draft preview; Flag: disable attachment while Issue text works; Rollback: bad upload build; Data validation: parent-child/type/digest/state; User notification: Issue sent, attachment failed/removed |
| Validation | Attachment belongs exact Issue, authorized preview/read only, message preserved, no duplicate, staging cleaned by policy |
| Escalation Trigger | Attachment auto-uploaded, cross-Issue access, screenshot content leak, missing payload after success |
| Forbidden Actions | Auto-capture/send; log/screenshot content; bypass validation; store SQL blob; expose object URL/key |
| Related Requirement/Feature | `FR-SUP-003/004`, `SR-SEC-019/020`; `FEAT-SUPPORT-003/005` |
| Related Monitoring | Attachment validation/upload/storage/error/orphan, explicit-send audit |
| Related Test | Issue attachment security/recovery, Document 12 storage/upload tests, UAT Issue Reporting |

## RB-ISSUE-005 — Issue queue/status/outbox stuck

| Field | Isi |
|---|---|
| Runbook ID | `RB-ISSUE-005` |
| Incident Type | Support queue projection/status notification failure |
| Symptoms | Issue tidak muncul ke Support, status user stale, outbox/backlog meningkat, duplicate notification |
| Affected Components | Support SQL aggregate/status history/outbox, Support Console, user projection/SignalR/REST |
| Safe Initial Checks | Authoritative Issue/status/version, Support scope/assignment, outbox state/age, REST user projection, notification sequence |
| Evidence | Issue/Correlation/Error, status/version/outbox state, queue/age aggregate, environment/timestamp; no message/note/attachment content |
| Likely Causes | Outbox dispatcher failure, permission/scope, stale projection/SignalR, state transition conflict, service/SQL pressure |
| Containment | Preserve Issue/history/outbox; use REST authoritative state; stop repeated transition; protect scope |
| Workaround | Support searches exact authorized Issue ID via official UI; user refreshes status; no content sent outside Issue system |
| Recovery Steps | Retry: idempotent outbox/status command with current version; Resume: sequence/ack; Checkpoint: status history/outbox; Restart: service owner; Provider fallback: none; Local fallback: reporter draft only; Flag: disable realtime/projection not source; Rollback: bad status/UI build; Data validation: one transition, correct scope/projection; User notification: status delayed/refreshed |
| Validation | Support queue and reporter projection agree, notifications deduped, unauthorized queues denied, status history/Audit correct |
| Escalation Trigger | Issue lost, cross-scope visibility, unauthorized status mutation, backlog not draining |
| Forbidden Actions | Edit status row/history; broaden Support wildcard; copy content to logs/chat; delete outbox |
| Related Requirement/Feature | `FR-SUP-004..007`; `FEAT-SUPPORT-004..008` |
| Related Monitoring | Issue backlog/oldest age/outbox/status transition/projection lag |
| Related Test | Support workflow/authorization/concurrency/recovery tests, UAT Support/Issue |

---

---

# 15. Recovery validation

Recovery dinyatakan selesai hanya setelah semua applicable checks mempunyai evidence. “Process/service hidup” saja tidak cukup.

## 15.1 Universal validation gate

| Gate | Bukti minimum |
|---|---|
| Health normal | Sinyal fresh; affected component dan mandatory dependency dalam approved state |
| Test transaction | Synthetic/minimized authorized transaction pada exact environment/path berhasil sekali |
| Core Feature | Representative released Core Feature pada affected processing mode berjalan; local-only smoke menghasilkan zero server upload/processing |
| Result valid | Type/structure/count/order/length/digest/readability/conformance/grounding sesuai Feature oracle |
| Queue stable | Arrival/service/drain/fairness/oldest age/retry kembali stabil; no stranded nonterminal |
| No data loss | Source, old target, workspace, persisted resource, object, version, and Audit invariants reconciled |
| Security active | Auth/permission/ownership, validation/quarantine, encryption/key boundary, rate limit, content exclusion, public-AI block aktif |
| Monitoring stable | Metric/log/trace freshness dan safe cardinality; tidak ada hidden error/retry/resource drift |
| Audit healthy | Required intent durable, event/outbox/ingestion/query once-only dan no sensitive payload |
| User communication | Status/action/outcome jelas; no silent wait, false success, or misleading limitation |

## 15.2 Component-specific validation

| Component | Tambahan validation |
|---|---|
| Desktop/Mobile | Correct profile/account, local state/artifact manifest, source unchanged, lifecycle/restart safe |
| Local Worker | UI remains alive, typed IPC/version, stale attempt cannot publish, residual memory returns |
| API | No false `202`; idempotency replay correct; expected 401/403/409/429/503 behavior |
| SignalR | REST wins, terminal monotonic, gap/duplicate recovered, authorization current |
| Server Worker/Queue | One active lease/attempt; stale completion rejected; queue drains fairly |
| SQL | Transaction/outbox/idempotency/rowversion/constraint integrity; pool/IO normal |
| SQLite | No silent reset; DB/WAL/artifact/profile binding consistent; single writer |
| Storage | Metadata/object/owner/state/digest/tombstone/retention/hold consistent |
| Engine/OCR/Conversion | Approved corpus/oracle; limitation honest; no partial/corrupt final |
| Intelligence | Approved provider only; grounded/reviewable; no content telemetry/public route |
| Audit | Occurred/received, ordering, dedup, append behavior, retention/hold safe |
| Issue | Reporter/Support scope, message/attachment separation, status/outbox consistency |

## 15.3 Observation after recovery

Gunakan observation window yang **disetujui**, bukan angka rekaan. Periksa trend, bukan satu titik: error/timeout/retry, resource residual, queue/backlog, cleanup/orphan, provider/circuit, client/session, and Audit/Issue delivery. Jika threshold/window belum disetujui, status tetap `TARGET_TO_BE_APPROVED` dan recovery decision memerlukan owner approval.

---

---

# 16. Escalation triggers

Eskalasi segera bila salah satu kondisi berikut terjadi:

## 16.1 Security/privacy

- cross-user/cross-scope access atau unauthorized privileged mutation;
- password/token/secret/document/OCR/signature/clipboard/prompt/screenshot/storage key muncul di evidence/log/telemetry/Audit;
- public/unapproved AI, OCR, conversion, storage, atau identity route digunakan/diupayakan;
- authentication/authorization/reauth/validation/quarantine/encryption/rate limit/Audit berhasil dilewati;
- path traversal, symlink/reparse escape, arbitrary URL/provider/SQL/shell behavior;
- suspected tampering, digest mismatch pada secured payload, or Audit same-ID different data.

## 16.2 Data/integrity

- source atau previous final target berubah/rusak tanpa explicit authorized overwrite;
- false `COMPLETED`, partial/intermediate published, corrupt/missing payload, or silent truncation;
- duplicate destructive/logical effect, more than three active secured versions, rotated version remains accessible;
- SQL/object/SQLite/artifact metadata cannot be reconciled;
- migration/restore/rollback compatibility or key/profile binding uncertain;
- mandatory Audit lost, editable/deleted, hold/retention threatened.

## 16.3 Availability/reliability

- API/Worker/SQL/storage/provider/SSO/Audit/Issue critical path does not recover through bounded safe action;
- queue/backlog/resource trend continues worsening or no forward progress;
- retry/reconnect/cancellation/lease loop cannot be contained;
- client/Worker crash, data loss, or unsupported-version block affects broad cohort;
- monitoring is missing/stale/unknown for a critical path;
- recovery requires Production command, schema change, direct data repair, credential/key action, infrastructure restore, or vendor decision.

## 16.4 Handoff minimum

```text
Incident ID / Issue ID
Impact and affected scope category
Environment/platform/component/build/version
Feature/Job/Correlation/Error/status/timestamps
Authoritative state and health freshness
Safe metrics/trend and last-known-good
Containment currently active
Actions attempted and exact outcomes
Data/security/Audit integrity statement
Decision needed and owner domain
```

Severity, named contact, on-call route, communication cadence, containment authority, and SLA remain policy inputs; laporan ini tidak mengarangnya.

---

---

# 17. Forbidden actions

Dalam semua runbook, tindakan berikut dilarang:

- meminta, menyalin, menampilkan, menyimpan, atau mengirim password, token, cookie, secret, key, connection string, atau authorization header;
- meminta full document/OCR/translation/summary/Markdown/Issue screenshot/signature/clipboard/prompt/provider content;
- mengedit Production row/status/lease/pointer/version/idempotency/outbox/Audit secara langsung;
- menonaktifkan constraint, validation, quarantine, permission/ownership, encryption, Audit, rate limit, CSP/anti-forgery, Local-First, atau public-AI block;
- menjalankan unreviewed DDL/migration/schema change, delete/purge, service-control command, storage command, or recursive filesystem cleanup;
- browse object storage, generate permanent/public URL, expose physical path/object key, or change ACL/key manually;
- delete SQLite/WAL/SHM/profile/database, reset local state silently, or edit local DB with generic tool;
- kill/restart Worker/API/SQL/storage indiscriminately; restart is not a substitute for state reconciliation;
- retry without bound/jitter/idempotency, create new Job/key for same ambiguous intent, or mass retry queue;
- force Job `COMPLETED/FAILED/CANCELLED`, accept stale completion, or publish temp/intermediate/invalid output;
- bypass expected limitation with new/unapproved vendor/provider/library/public service;
- weaken thresholds or expand quota/concurrency solely to make warning disappear;
- overwrite source/old target or delete user content to free space without explicit lifecycle authority;
- disclose internal topology, provider endpoint, query/stack/raw exception, or unrestricted identity/owner data;
- call a workaround “recovered” before validation gates and observation complete.

---

---

# 18. Risks/conflicts

| Risiko/konflik | Dampak | Rekomendasi |
|---|---|---|
| Exact severity, owner, on-call/contact, communication cadence belum disetujui | Escalation dapat tidak sampai ke pihak tepat | Tetapkan Incident Governance Pertamina sebelum release |
| Numeric threshold/window/timeout/retry/queue/capacity/RPO/RTO belum disetujui | Runbook tidak dapat menyatakan timed pass/fail resmi | Gunakan `MEASURED_BASELINE` → `TARGET_TO_BE_APPROVED` → typed policy |
| Production restart/migration/restore/storage commands milik Document 17 | Runbook teknis dapat berubah menjadi Deployment Guide berbahaya | Pertahankan action class + handoff; command hanya pada approved ops runbook |
| Admin cross-owner Job cancel/retry/resume tidak diizinkan Document 09/15 | Tombol incident action dapat memperluas privilege | Monitoring-only sampai permission/API/Audit/UAT resmi |
| Per-file Secured Files metadata/content lintas-owner tidak diizinkan | Diagnosis storage dapat menjadi content access | Aggregate/reconciliation service; no payload open |
| Actual RC/UI/Error Catalog/Feature exposure 33 Core belum tersedia | Langkah/label/status dapat tidak cocok release | Validate setiap runbook terhadap exact RC dan generated contract |
| Seluruh 33 Core masih `BELUM TERSEDIA` pada Document 15 | Synthetic Core validation belum dapat dieksekusi sekarang | Jadikan release gate, bukan claim current operation |
| Typed provider health/effective availability schema belum final | Containment/fallback reason dapat tidak konsisten | Freeze schema/reason codes before operations acceptance |
| SSO claim/lifetime/reauth/help route belum dikonfirmasi | Auth recovery dapat salah arah | Pertamina SSO confirmation and test tenant lifecycle required |
| Secure exceptional content/forensic access tidak ada baseline | Incident analyst mungkin meminta payload sembarangan | Tetap no-content; future process needs Product/Security/Legal design |
| Audit degraded behavior untuk ordinary L2 action memerlukan policy | Availability vs evidence risk belum final | Fail closed privileged/L3; confirm ordinary degraded policy |
| Provider limitation vs defect thresholds/corpus belum approved | Quality incident dapat salah klasifikasi | Versioned PoC corpus, oracle, acceptance band, release limitations |
| Issue/Audit/diagnostic evidence retention dan export scope belum final | Evidence over-retention/leak | Typed retention/access/hold policy; minimize until approved |
| Mobile real-device, secure-store, lifecycle, backup-exclusion evidence belum lengkap | Recovery claim lintas platform belum terbukti | Real-device recovery/security matrix before release |

Decision dependencies:

```text
BLOCKED_BY_DECISION: PERTAMINA_INCIDENT_SEVERITY_OWNER_ONCALL_AND_COMMUNICATION_POLICY
BLOCKED_BY_DECISION: APPROVED_NUMERIC_ALERT_RECOVERY_RPO_RTO_AND_OBSERVATION_TARGETS
BLOCKED_BY_DECISION: DOCUMENT17_PRODUCTION_COMMAND_DEPLOYMENT_MIGRATION_RESTART_AND_RESTORE_RUNBOOKS
BLOCKED_BY_DECISION: ADMIN_CROSS_OWNER_JOB_ACTION_PERMISSION_AND_API_CONTRACT
BLOCKED_BY_DECISION: ADMIN_SECURED_METADATA_AND_EXCEPTIONAL_CONTENT_ACCESS_SCOPE
BLOCKED_BY_DECISION: RELEASE_RC_FEATURE_EXPOSURE_ERROR_CATALOG_AND_SCREEN_EVIDENCE
BLOCKED_BY_DECISION: TYPED_PROVIDER_HEALTH_CAPABILITY_AND_FALLBACK_SCHEMA
BLOCKED_BY_DECISION: PERTAMINA_SSO_REAUTH_AND_HELP_ROUTE_CONFIRMATION
BLOCKED_BY_DECISION: AUDIT_DEGRADED_OPERATION_RETENTION_HOLD_AND_EVIDENCE_POLICY
```

Label tersebut adalah dependency kerja laporan, bukan Feature ID, permission, endpoint, atau keputusan arsitektur baru.

---

---

# 19. Suggested Document 16 text

## Technical Diagnostics and Recovery Runbook

Diagnosis PERTAMINA OTO dimulai dari safe identifiers dan authoritative state, bukan isi dokumen atau asumsi dari UI. Evidence minimum terdiri dari Incident/Issue ID, Job ID, Correlation ID, Feature ID, Error Code, version, environment, platform, processing mode, approved provider profile, status, dan UTC timestamp. Password, token, secret, document/OCR content, signature, clipboard, prompt, screenshot bytes, raw exception, SQL, path, object key, dan provider response tidak diminta.

Urutan standar adalah:

```text
authorize scope
→ classify impact and safety
→ capture safe evidence
→ read persisted state and health freshness
→ contain narrowly
→ recover through official application/control-plane path
→ validate data/result/security/queue/monitoring/Audit
→ communicate and escalate
```

SignalR tidak menjadi source of truth. Jika SignalR putus, Job tetap berjalan; client reconnect dengan backoff/jitter; REST mengambil current persisted state; Activity Center direkonsiliasi; duplicate/stale event diabaikan dan terminal state tidak mundur.

Worker crash tidak boleh mematikan Desktop UI. Affected job hanya di-retry untuk transient allowlist dan tetap memakai logical Job ID/idempotency yang benar. Resume hanya dari checkpoint yang cocok dengan Feature/schema/input/engine/stage/digest/expiry. Intermediate output tidak pernah menjadi final. Jika recovery tidak aman, user mendapat pilihan menunggu, cancel, retry dari awal, atau mempertahankan source/working state sesuai capability.

Queue, Worker, SQL, storage, dan provider pressure ditangani dengan bounded admission, fair waiting, concurrency reduction dalam approved range, `Retry-After`/`WAITING_RESOURCE`/safe `503`, lifecycle cleanup resmi, dan narrow feature/provider maintenance. Local-capable tools tetap tersedia. Operator tidak mengubah Job/queue/database/storage state secara langsung dan tidak melakukan mass retry.

SQL failure yang membuat durable mutation atau mandatory Audit intent tidak dapat disimpan menghasilkan fail-closed behavior. Setelah SQL pulih, validation mencakup transaction, idempotency, outbox, lease, rowversion, result/object, and Audit consistency. Migration, schema, backup/restore, restart, infrastructure, and secret commands berada pada approved Document 17 operations runbook.

SQLite adalah local metadata/state dan harus dibedakan dari user files. DB locked/migration/corruption/recovery failure tidak boleh memicu silent reset atau delete. SQLite main DB, WAL/SHM, protected backup, and managed artifacts dipertahankan untuk recovery; source file tetap terpisah dan tidak ditimpa.

Secure storage diagnosis dimulai dari metadata/object health, state, owner/purpose, digest, lease, tombstone, retention, and reconciliation status. Payload tidak dibuka otomatis. Missing object, orphan object, checksum mismatch, or maximum-three version inconsistency adalah integrity signal. Recovery menggunakan idempotent finalization/tombstone/reconciliation worker, bukan storage browsing atau direct SQL.

PDF/Conversion/OCR/Intelligence runbook wajib membedakan documented limitation dari incident. Malformed/unsupported/encrypted input yang ditolak aman adalah expected behavior bila sesuai contract. Crash, corrupt output, false success, data loss, undocumented fidelity regression, redaction/security failure, or public provider route adalah incident. AI Summarizer/Translate hanya memakai approved local/enterprise-internal provider; tidak pernah fallback ke public AI.

Recovery dianggap berhasil hanya jika component health normal dan fresh, synthetic authorized transaction berhasil, representative released Core Feature pada affected path menghasilkan valid result, queue stabil, tidak ada data loss/duplicate/corruption, security controls tetap aktif, monitoring stabil, dan Audit pipeline sehat. Observation duration and numeric threshold berasal dari approved policy; tidak dikarang oleh runbook.

Eskalasi segera untuk unauthorized/cross-user access, secret/content leakage, public provider use, validation/Audit bypass, source/result corruption, duplicate destructive effect, missing payload, Audit integrity/hold risk, unreconciled cross-store state, retry/queue/resource runaway, atau recovery yang membutuhkan Production command/schema/key/restore/vendor decision. Handoff hanya membawa safe evidence dan current containment.
