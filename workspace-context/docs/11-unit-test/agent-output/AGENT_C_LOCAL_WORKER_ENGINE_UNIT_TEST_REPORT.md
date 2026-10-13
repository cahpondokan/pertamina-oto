# AGENT C — LOCAL-FIRST, WORKER, STORAGE, SYNC & ENGINE UNIT-TEST REPORT

| Atribut | Nilai |
|---|---|
| Task ID | `DOC11-AGENT-C` |
| Fokus | Local-First routing, Processing Coordinator, Worker/IPC contract, storage/workspace, selective sync, retry/checkpoint, cleanup, Engine Adapter, dan Provider Resolver |
| Status | **REKOMENDASI — BELUM LOCKED** |
| Tanggal | 27 Agustus 2026 |
| Decision owner | User + Integrator |

## Baseline, scope, dan test boundary

Laporan ini diturunkan dari Document 01–10 FINAL, terutama:

- Solution Design sections 9–11, 13, 17–18, 28, dan 30;
- Technical Reference `TECH-DEC-006..013` serta Proof-of-Capability gates;
- API Final sections 11–14, 18, dan 22;
- Database Final sections 10, 13–15, 17–18, dan 24;
- Security Final sections 10, 15–17, 27;
- Role Matrix Final sections 28–32, 36, dan 48;
- Audit Trail Final sections 24–28, 45, dan 54.

Locked test technology:

```text
MSTest + Microsoft.Testing.Platform (MTP)
```

Report ini merancang **unit tests**, bukan production code. Unit under test harus menerima dependency melalui contract/abstraction dan diuji secara deterministik dengan in-memory fake, stub, spy, atau mock. Test dilarang menghubungi real cloud, real provider, public AI, physical scanner, full filesystem, database server, atau menjalankan full Local Worker process.

### Unit versus non-unit boundary

| Diuji sebagai unit | Dipindahkan ke integration/contract/PoC |
|---|---|
| Routing/policy/capability decision | Actual HTTP/cloud/provider connectivity |
| Coordinator state/result mapping | Full Worker process launch, crash, OOM, OS job-object limit |
| Typed Worker/IPC message validator dan dispatcher | Actual Named Pipe ACL, peer identity, handle inheritance, remote denial |
| Storage abstraction behavior dengan fake artifact store | NTFS atomic rename, symlink/junction/reparse race, real full disk |
| Repository/workspace state machine in-memory | SQLite/WAL/SHM, migration, encryption/key integration |
| Sync state/idempotency/conflict logic dengan fake gateway | Real object storage, network, server DB, cross-device E2E |
| Retry/backoff/checkpoint compatibility dengan fake clock/random | Long-running soak dan real process recovery |
| Adapter DTO/error/capability mapping dengan mock provider boundary | Vendor PDF/OCR/Office/image internal correctness dan malicious corpus |
| Resolver provider selection | Provider fidelity, license, resource benchmark, platform support |
| Cleanup eligibility/claim/finalization decision | Actual deletion/archive/secure erase behavior |

### Standard test doubles

Recommended reusable test doubles:

- `FakeClock`, `FakeMonotonicClock`, dan deterministic jitter source;
- stub Feature Registry, permission/policy/classification evaluator, capability probe, connectivity, consent, quota/capacity, compatibility, dan resource preflight;
- spy local executor, Local Worker transport, server gateway, upload gateway, secured-storage gateway, and audit-intent sink;
- in-memory workspace/job/checkpoint/sync repositories with row-version simulation;
- fake artifact/file storage that records operation order without touching the filesystem;
- mock engine/provider adapters and output validator;
- capture logger/telemetry sink with forbidden-field canaries;
- deterministic ID factory for Job ID, Correlation ID, Event ID, request, attempt, and idempotency references.

Every test must assert both the expected result **and prohibited side effects**, especially zero server/upload/provider calls on local/offline paths.

---

## 1. Local-First unit-test strategy

### 1.1 Routing decision model

The Local-First decision should be testable as a pure or near-pure application policy:

```text
Feature contract + input facts + platform capability + device capacity
+ connectivity + enterprise policy + security/classification
+ approved engine/provider + user consent state
→ LOCAL_IN_PROCESS | LOCAL_WORKER | SERVER_PREPARE | WAITING_CONNECTION
  | WAITING_RESOURCE | UNSUPPORTED | DENIED
```

Client preference cannot force local/server, vendor, engine, provider, URL, path, or command. Local provider wins only when it is safe, compatible, approved, and capable.

### 1.2 Mandatory Local-First cases

| ID | Given / When | Expected assertions |
|---|---|---|
| `LF-001` | Feature execution profile is `LOCAL`; capability and policy valid | Local executor selected; server preflight/upload/execute spies each called **0** times |
| `LF-002` | `LOCAL` feature while network is available | Availability of network does not trigger server call or upload |
| `LF-003` | `LOCAL` feature is heavy/risky | `LOCAL_WORKER`, not server; Worker command receives typed local refs only |
| `LF-004` | `OPTIONAL_SERVER`; local provider capable, compatible, approved, and within resource budget | Local path preferred; no transfer consent/preflight/upload |
| `LF-005` | `OPTIONAL_SERVER`; local capability absent; server permitted and online | `SERVER_PREPARE`; preflight required before upload; consent/policy gates preserved |
| `LF-006` | `OPTIONAL_SERVER`; local capacity temporarily insufficient; server permitted but consent absent | No upload; return consent-required/decision state, not silent server routing |
| `LF-007` | `OPTIONAL_SERVER`; local provider unavailable and server prohibited by policy | Safe unavailable/unsupported decision; no public or unapproved fallback |
| `LF-008` | Server-required route while offline | `WAITING_CONNECTION` or stable online-required result; upload/API/provider called **0** times |
| `LF-009` | Eligible local feature while offline with valid protected grant | Local execution allowed; audit metadata outbox may be written; no content transfer |
| `LF-010` | Offline grant expired/tampered/wrong profile | Fail closed before engine/Worker; no server attempt and no state mutation |
| `LF-011` | Client parameter asks to force server/local/provider | Parameter rejected/ignored per typed schema; trusted resolver owns route |
| `LF-012` | Unknown/inactive/hidden Feature ID | Denied before executor/resolver provider invocation |
| `LF-013` | Feature schema or client contract incompatible | Stable compatibility failure; no engine/provider/server call |
| `LF-014` | Server capacity unavailable but safe local path remains valid | Graceful local path preserved; no public provider fallback |
| `LF-015` | Both local and server providers technically capable, but local violates security/classification | Local not selected; server considered only if policy/consent allow; otherwise deny |
| `LF-016` | Resolver invoked twice with identical immutable facts | Same decision and reason code; no hidden nondeterministic vendor choice |

### 1.3 No-auto-sync matrix

Use one data-driven test over every local artifact class.

| ID | Artifact/state | Expected outbound behavior |
|---|---|---|
| `LF-020` | Source/unselected source | No sync trigger, no generic outbox, no upload |
| `LF-021` | Draft/working state | No auto-sync |
| `LF-022` | Undo/redo operation | No auto-sync |
| `LF-023` | Cache | No auto-sync |
| `LF-024` | Temporary/intermediate artifact | No auto-sync |
| `LF-025` | Thumbnail/preview | No auto-sync |
| `LF-026` | Checkpoint | No auto-sync |
| `LF-027` | Clipboard | No persistence/sync |
| `LF-028` | Password/token/secret | No persistence/sync; forbidden-field assertion |
| `LF-029` | Visual signature asset | No auto-sync |
| `LF-030` | Local History/recent file | No enterprise content sync |
| `LF-031` | Assistant conversation | No auto-sync |
| `LF-032` | Safe mandatory Audit outbox | Metadata-only batch sync is allowed; verify no document/content fields |
| `LF-033` | Allowlisted account preference | Only typed preference endpoint/path; no generic workspace payload |
| `LF-034` | User explicitly selects eligible validated final result and invokes **Amankan ke Akun** | Create one `secured_transfer`; upload remains gated by auth/policy/consent/quota/idempotency |
| `LF-035` | Same eligible result exists but user never invokes **Amankan ke Akun** | `secured_transfer` not created; secured gateway called **0** times |

---

## 2. Processing Coordinator tests

Processing Coordinator decides immediate/background and local/server based on actual execution facts, not a technical choice exposed to user.

| ID | Scenario | Expected result and collaborator assertions |
|---|---|---|
| `PC-001` | Light, deterministic, safe local operation below budgets | Immediate in-process plan; Worker and server spies untouched |
| `PC-002` | Heavy local OCR/batch/conversion but local-capable | Background Local Worker plan with bounded budget/deadline |
| `PC-003` | Heavy operation mistakenly marked “immediate” by UI hint | Coordinator ignores untrusted hint and chooses safe background plan |
| `PC-004` | Optional-server operation with valid local route | Local preferred as in `LF-004` |
| `PC-005` | Local unavailable; approved server route passes policy/capability/connectivity | Server preflight plan; does not upload within decision method |
| `PC-006` | Server route requires transfer consent and consent is missing | Return pending consent; no upload/job creation |
| `PC-007` | File count/size/page/memory/temp budget exceeded locally and server unavailable | Stable unavailable/waiting result, not UI freeze or forced execution |
| `PC-008` | Worker/local capacity temporarily full | `WAITING_RESOURCE`; no false `FAILED`; no busy retry loop |
| `PC-009` | Network unavailable after server plan but before dispatch | Move to `WAITING_CONNECTION`; retain typed pending intent; no partial upload side effect |
| `PC-010` | Invalid Feature ID or no registered handler | Reject before parameter mapping/engine resolution |
| `PC-011` | Parameter schema/version mismatch | Reject before any executor call; safe compatibility error |
| `PC-012` | Input reference missing, changed, unowned, or not READY/AVAILABLE | Reject/require revalidation; no engine/server call |
| `PC-013` | Policy lower layer preference conflicts with system safety | System safety wins; no override through user/device config |
| `PC-014` | Cancellation requested before dispatch | Plan/job becomes cancelled without invoking executor |
| `PC-015` | Cancellation during local execution | Cancellation token forwarded once; final state waits for safe result mapping |
| `PC-016` | Supplied Correlation ID valid | Same ID propagated to local job, Worker request, storage calls, safe audit intent, and result mapping |
| `PC-017` | Correlation ID absent/invalid | Coordinator uses approved ID factory and never accepts control characters/secret-bearing value |
| `PC-018` | Server/Worker/provider saturated beyond safe queue | Correct `WAITING_RESOURCE`/safe rejection semantics; no unbounded enqueue |
| `PC-019` | Primary provider fails with a transient normalized error and approved fallback exists | Coordinator requests resolver fallback according to policy and bounded attempt; not arbitrary provider chain |
| `PC-020` | Primary provider returns output but output validation fails | No final result publication; safe failed/fallback decision only if policy permits |
| `PC-021` | Atomic operation has one failed required stage | No `PARTIAL_SUCCESS`; intermediate outputs remain non-final |
| `PC-022` | Independent batch contract explicitly allows partial outcome | `COMPLETED + PARTIAL_SUCCESS` only with typed per-item outcomes |
| `PC-023` | Client/Worker disconnects after durable local/server acceptance | Logical job remains authoritative; no duplicate submission/cancellation inferred |
| `PC-024` | Same command replayed with same idempotency context | Same logical job/result returned; executor invoked once |

Coordinator unit tests must not instantiate document engines, HTTP clients, SQLite, named pipes, or Worker processes.

---

## 3. Worker contract tests

### 3.1 Unit under test

Test the typed/versioned message validator, handshake decision, operation dispatcher, Worker lifecycle/status mapper, and Desktop-side transport facade using an in-memory fake transport. Do not start the real executable.

### 3.2 Worker/IPC contract cases

| ID | Input/event | Expected result |
|---|---|---|
| `WK-001` | Valid authenticated handshake facts, supported contract/version, correct instance/job binding | Handshake accepted; dispatcher enabled only for allowed messages |
| `WK-002` | Valid allowed operation with canonical Feature ID, typed parameters, scoped refs, deadline/budget | Registered handler invoked exactly once; normalized status emitted |
| `WK-003` | Unknown operation/message type | Fail closed; handler/provider called **0** times; safe protocol error |
| `WK-004` | Unsupported contract major/version | Reject handshake/message with compatibility outcome; no fallback deserialization |
| `WK-005` | Parameter schema mismatch or unknown polymorphic type | Reject before handler; no unsafe type activation |
| `WK-006` | Invalid/noncanonical/inactive Feature ID | Reject before engine adapter |
| `WK-007` | Feature ID canonical but no registered local handler | Safe unsupported result; no dynamic plugin/assembly loading |
| `WK-008` | Message exceeds size/depth/count or has invalid length frame | Reject/disconnect fake peer; no allocation-heavy processing path |
| `WK-009` | Missing/invalid Request ID, Correlation ID, or deadline | Reject or generate only where contract allows; no uncorrelated execution |
| `WK-010` | Job/profile/environment/resource ref does not match launched instance | Reject as job/instance mismatch |
| `WK-011` | Stale Worker instance sends completion for new/restarted job | Completion ignored/rejected; authoritative job not mutated |
| `WK-012` | Cancellation for known cancellable running request | Same cancellation token source triggered; status maps only after safe point |
| `WK-013` | Duplicate cancellation | Idempotent; no duplicate terminal event |
| `WK-014` | Cancellation after completed terminal state | Terminal state unchanged |
| `WK-015` | Deadline/hard timeout signaled by lifecycle controller | Map to approved timeout failure/retryability; partial output not final; recycle requested |
| `WK-016` | Worker transport closes/crash signal while running | UI-side state mapper returns worker-crash safe failure/recovery candidate; no Desktop termination |
| `WK-017` | Restart detects job marked RUNNING without live matching lease/instance | Recovery candidate, never `COMPLETED`; retry/resume options follow checkpoint policy |
| `WK-018` | Worker reports resource pressure/wait | Map to `WAITING_RESOURCE`, preserve Job ID/Correlation ID, no false failure |
| `WK-019` | Worker progress regresses or exceeds bounds | Normalize/reject update; terminal/current revision never regresses |
| `WK-020` | Worker emits raw exception/path/provider error | Mapper exposes stable safe Error Code only; capture logger contains no raw detail |
| `WK-021` | Command includes arbitrary path, URL, provider, engine, SQL, shell, process, class, assembly, or user token | Contract validator rejects before dispatch |
| `WK-022` | Valid resource refs resolve outside injected allowed root facts | Path-scope policy rejects; handler not invoked |
| `WK-023` | Heartbeat arrives from wrong instance/request | Ignored/rejected; lease not extended |
| `WK-024` | Heartbeat stops in fake clock beyond policy deadline | Lifecycle maps unresponsive Worker and requests bounded recycle/recovery |
| `WK-025` | Handler returns validated result and terminal status | Result reference promoted through Coordinator/storage boundary, then terminal state emitted once |
| `WK-026` | Handler returns unvalidated/temporary result | No final promotion; safe failure |

### 3.3 Explicitly non-unit Worker evidence

Actual current-user/package/process ACL, remote pipe denial, rogue same-user process, PID/instance binding, OS handle inheritance, executable code integrity, process crash/OOM, CPU/RAM/temp/handle/child-process enforcement, named-pipe flood, and UI survival with a real process belong to integration/security PoC. Unit tests may inject already-verified peer/limit facts but must not claim those OS guarantees are proven.

---

## 4. Storage abstraction tests

Test business behavior through `IFileStorage`, `ILocalWorkspaceStore`, `ISecuredAccountStorage`, repository interfaces, and a fake managed artifact store. Physical paths remain outside business contracts.

### 4.1 Save/read/workspace cases

| ID | Scenario | Expected assertions |
|---|---|---|
| `ST-001` | Save new valid artifact/state | Fake call order: create staging → write/close/flush → validate/digest → atomic publish → commit pointer/state |
| `ST-002` | Save fails before validation | Existing pointer/state unchanged; staging marked cleanup candidate |
| `ST-003` | Pointer commit fails after fake publish | Reconciliation intent retained; old source/result not deleted; no false success |
| `ST-004` | Read existing owned artifact | Correct bytes/reference metadata returned through abstraction; physical path not leaked |
| `ST-005` | Read missing artifact | Stable missing/unavailable domain result; no empty file creation or silent reset |
| `ST-006` | Read sees changed identity/fingerprint | `CHANGED`/conflict outcome; no silent use/overwrite |
| `ST-007` | Expected row version matches | Mutation commits and version advances once |
| `ST-008` | Expected row version stale | Conflict; no last-write-wins; state remains current |
| `ST-009` | Second writer simulation | Serialized-writer/row-version rule yields one winner and one conflict |
| `ST-010` | Workspace operation after undo | Redo branch truncated/marked transactionally with new operation cursor |
| `ST-011` | Snapshot replacement valid | New snapshot and operation cursor commit atomically; old snapshot released afterward |
| `ST-012` | Replacement integrity invalid | Old snapshot remains addressable; invalid replacement quarantined/cleanup candidate |
| `ST-013` | Autosave repeated rapidly | Debounce/delta/compaction policy invoked; no full final output per edit |
| `ST-014` | User deletes Personal History | History projection removed only; result/source/audit unaffected |

### 4.2 Capacity, path, and lifecycle mapping

| ID | Scenario | Expected assertions |
|---|---|---|
| `ST-020` | Quota would be exceeded at preflight | Safe quota conflict before write; no artifact/pointer mutation |
| `ST-021` | Fake store reports full disk during staging | Safe storage-full mapping; source/old result preserved; staging cleanup tracked |
| `ST-022` | Fake store reports access revoked/missing parent | Stable safe error; no path disclosure |
| `ST-023` | Traversal, absolute, UNC, device, ADS, NUL/control, reserved, trailing ambiguity, overlong, or bidi-unsafe logical name | Path/name policy rejects before store operation |
| `ST-024` | Injected resolved-handle fact says outside canonical managed root or reparse escape | Reject; no read/write/delete |
| `ST-025` | Cleanup request supplies a raw user path instead of manifest artifact ID | Reject; no recursive/raw path deletion |
| `ST-026` | Temporary result not validated | Remains temporary/non-final and cannot be opened as authoritative result |
| `ST-027` | Temporary result validates and atomic promotion succeeds | New final result reference published once; source unchanged |
| `ST-028` | Result expires | Bytes become unavailable through lifecycle; manifest maps `EXPIRED`; local source not deleted |
| `ST-029` | Local result is mistaken for cache by cleanup policy | Test prevents deletion; result has different lifecycle class |

### 4.3 Secured version logic

| ID | Scenario | Expected assertions |
|---|---|---|
| `ST-030` | Create first/second/third version | Active version count advances to maximum three; current pointer consistent |
| `ST-031` | Create fourth valid version | New version activates; oldest active version tombstoned in same aggregate decision; exactly three remain addressable |
| `ST-032` | Fourth-version payload/prevalidation fails | No pointer/count/tombstone change |
| `ST-033` | Stale row version while adding fourth version | Conflict; no duplicate version or silent overwrite |
| `ST-034` | Concurrent fourth-version commands simulated | Unique/version/row-version guards yield one coherent sequence and maximum three active versions |
| `ST-035` | Physical purge of tombstoned oldest fails | User access remains removed; cleanup reference retained for retry; old version cannot be restored via failed purge |
| `ST-036` | Retry same secured-version request/idempotency context | Same version/resource returned; no fourth/fifth duplicate |
| `ST-037` | Restore exact eligible previous version | Current pointer/version rules applied atomically; policy/ownership state consulted |
| `ST-038` | Restore stale/deleted/ineligible version | Safe conflict/unavailable result; no pointer change |

Real object-store promotion and SQL transaction concurrency remain integration tests. Unit tests prove the aggregate decision and collaborator ordering only.

---

## 5. Sync tests

Selective sync is not database replication. Four outbound paths remain distinct: execution transfer, explicit secured transfer, Audit outbox, and allowlisted account preferences.

### 5.1 Secured-transfer state tests

| ID | Scenario | Expected state/side effects |
|---|---|---|
| `SY-001` | Explicit **Amankan ke Akun** requested for eligible validated result | `REQUESTED → PREPARING`; one typed transfer record with idempotency/fingerprint |
| `SY-002` | No explicit action | No transfer record or gateway call |
| `SY-003` | Network unavailable before upload | `WAITING_CONNECTION`; zero upload bytes sent |
| `SY-004` | Authentication/permission cannot be refreshed | `WAITING_AUTHORIZATION`/safe blocked state; no upload |
| `SY-005` | Preconditions pass and fake upload/commit succeed | `UPLOADING → COMMITTING → SYNCED`; server resource ID stored once |
| `SY-006` | Transient network error during upload | `RETRYING` with bounded policy; offset/idempotency preserved |
| `SY-007` | Permanent policy/classification/quota rejection | `FAILED` or policy-blocked result; no retry storm |
| `SY-008` | Duplicate same request/fingerprint/idempotency | Reuse same transfer/server resource; no duplicate secured file/version |
| `SY-009` | Same idempotency key with changed file/fingerprint | Conflict/security-safe result; no overwrite |
| `SY-010` | Local file/result changes after request, before first upload/resume/commit | Detect fingerprint conflict; stop and require explicit new request/cancel |
| `SY-011` | Remote secured base version/ETag changed | `CONFLICT`; refresh then explicit new version/new file/cancel; no silent overwrite |
| `SY-012` | Local secured metadata cache stale | Refresh authoritative server metadata before commit; SignalR/cache alone not authority |
| `SY-013` | Server resource tombstoned/deleted | Cache marked unavailable/tombstoned; local working copy kept unless user deletes |
| `SY-014` | Account/profile binding differs | Reject and require profile switch; never merge stores |
| `SY-015` | Cancel before/during cancellable transfer | `CANCELLED`; cleanup staged upload via idempotent gateway; local source retained |

### 5.2 Pending intent and reconnect

| ID | Scenario | Expected assertions |
|---|---|---|
| `SY-020` | Server-required command queued offline | Typed minimized `pending_server_intent`; no raw body/content/URL/command/secret |
| `SY-021` | Reconnect | Refresh auth/permission/policy/capability first, then input identity and consent, then transfer/execute |
| `SY-022` | Permission/policy revoked while offline | Pending intent denied/held; no upload/execute |
| `SY-023` | Input missing/access revoked on reconnect | Recovery choice required; no submit |
| `SY-024` | Ambiguous previous submission with no cached Job ID | Retry same Idempotency-Key, persist returned authoritative Job ID, reconcile REST |
| `SY-025` | Server responds duplicate committed/running request | Reuse same resource/Location/Job ID |
| `SY-026` | Remote job cache revision older than current REST revision | Ignore stale cache/event; REST revision wins |

### 5.3 Audit and preference sync distinctions

| ID | Scenario | Expected assertions |
|---|---|---|
| `SY-030` | Audit outbox pending and network unavailable | Protected metadata retained; no content upload; bounded retry scheduled |
| `SY-031` | Audit batch acknowledged durably | Local rows cleaned only after acknowledgement/policy; same IDs not duplicated |
| `SY-032` | Poison/unsupported Audit event | Retain visible `VERSION_BLOCKED`/policy-blocked state; no hot loop/drop |
| `SY-033` | Preference update contains allowlisted typed metadata | Preference gateway only; version/conflict handled |
| `SY-034` | Preference payload includes path/content/password/signature/arbitrary field | Reject before sync |
| `SY-035` | Generic sync scanner receives local-only tables/artifacts | No mapping exists; test fails if any outbound item produced |

No test calls real API, object storage, cloud, or second device. Contract/integration suites own those paths.

---

## 6. Retry/checkpoint tests

### 6.1 Retry policy

| ID | Scenario | Expected assertions |
|---|---|---|
| `RC-001` | Error classified transient and attempt remains | Retry scheduled with bounded exponential backoff + deterministic jitter |
| `RC-002` | Error classified permanent/validation/security/policy | No automatic retry |
| `RC-003` | Attempt limit reached | Final safe failed outcome; no further timer/dispatch |
| `RC-004` | Retry delay active | No database/network/Worker resource held by policy object |
| `RC-005` | `Retry-After` provided within policy bounds | Effective delay honors allowed server guidance |
| `RC-006` | Invalid/excessive `Retry-After` | Clamp/reject according to typed safety policy, not unbounded wait |
| `RC-007` | Cancellation arrives before retry fires | Timer cancelled; no next attempt |
| `RC-008` | Duplicate retry command | Same logical Job ID; idempotent current result; one new attempt maximum |
| `RC-009` | New valid attempt | Append attempt number/history; logical Job ID and Correlation ID stable |
| `RC-010` | Resource pressure rather than failure | `WAITING_RESOURCE`, then bounded re-evaluation; not counted as arbitrary busy loop |
| `RC-011` | Connectivity unavailable | `WAITING_CONNECTION`; retries do not trigger upload while offline |
| `RC-012` | Retry storm simulation over many fake jobs | Jitter/fair scheduling prevents identical dispatch instant; concurrency cap honored |

### 6.2 Checkpoint compatibility

Checkpoint binding must include exact job/generation, input identity/fingerprint, canonical Feature ID, parameter schema/contract version, stage, engine/provider version, integrity marker/digest, and retention/expiry.

| ID | Scenario | Expected assertions |
|---|---|---|
| `CP-001` | All binding facts and integrity match; checkpoint unexpired | Resume allowed from declared safe stage/item |
| `CP-002` | Input fingerprint changed | Resume rejected; no engine invocation from checkpoint |
| `CP-003` | Feature ID or parameter schema/version mismatch | Incompatible checkpoint result |
| `CP-004` | Engine/provider version incompatible | Resume rejected or adapter migration only if explicitly registered/tested |
| `CP-005` | Stage unknown/out of range | Reject as incompatible/corrupt |
| `CP-006` | Digest/integrity marker invalid | Reject/quarantine; safe signal; no partial reuse |
| `CP-007` | Checkpoint expired | Expired outcome; no silent resume |
| `CP-008` | Checkpoint exists but policy disallows resume | Resume denied; allowed safe action returned |
| `CP-009` | No compatible checkpoint after crash | Do not silently claim resume; offer typed retry-from-start/cancel only when policy permits |
| `CP-010` | Valid checkpoint contains password/token/secret/signature/content canary | Serialization/persistence test fails; forbidden field absent |
| `CP-011` | Checkpoint cleanup evaluated while job/recovery lease active | Keep; no deletion |
| `CP-012` | Terminal job plus recovery window expired and no reference | Eligible cleanup decision |
| `CP-013` | Crash after checkpoint metadata commit but artifact validation incomplete | Candidate rejected/reconciled; last valid checkpoint retained |

---

## 7. Engine adapter tests

Adapters are tested at the PERTAMINA OTO contract boundary. Mock the provider/library facade; do not test how a third-party PDF/OCR/Office/image library parses or renders internally.

### 7.1 Common adapter contract

| ID | Scenario | Expected assertions |
|---|---|---|
| `EA-001` | Valid domain request | Maps canonical inputs/options to provider DTO exactly; no engine/vendor selection from caller |
| `EA-002` | Provider returns successful typed output | Maps to domain result/reference; output validator called before success/final promotion |
| `EA-003` | Provider returns unsupported operation/format | Stable `UNSUPPORTED` domain outcome; no raw provider detail |
| `EA-004` | Provider unavailable/transient | Stable unavailable/transient classification for Resolver/retry policy |
| `EA-005` | Provider validation/permanent failure | Non-retryable safe error mapping |
| `EA-006` | Provider throws unexpected exception with secret/path/document text | Capture logger/result contain stable safe code only; raw detail not surfaced |
| `EA-007` | Cancellation token signaled | Token forwarded; cancellation mapped distinctly from provider failure |
| `EA-008` | Deadline/budget passed | Adapter avoids/inhibits call or maps timeout safely; no unbounded work |
| `EA-009` | Provider reports partial/intermediate file only | Domain result remains non-final until validation/promotion |
| `EA-010` | Provider returns output outside expected ref/root/type/size/count | Output validation rejects; no final publication |
| `EA-011` | Capability metadata requested | Maps supported features/formats/limits/platform/version without exposing secret/license key/path |
| `EA-012` | Provider contract/version compatible | Adapter accepts and records normalized capability version |
| `EA-013` | Provider contract/version incompatible | Safe incompatibility; no attempt to coerce unknown schema |
| `EA-014` | Unknown provider field/type | Strict mapping ignores/rejects according to version rule; never polymorphic activation |
| `EA-015` | Same domain input mapped repeatedly | Deterministic provider request excluding nondeterministic IDs explicitly owned by factory |

### 7.2 Family-specific assertions

| Adapter family | Unit assertions | Not unit-tested here |
|---|---|---|
| PDF | Page/range/order/options mapped; result/output validation required | Merge fidelity, malformed PDF handling, redaction forensic correctness, parser CVE behavior |
| OCR | Language/profile/page selection mapped; OCR text classified untrusted; no public endpoint | Recognition accuracy, large-page resource use, real scanner/image pipeline |
| Office conversion | Format/options/link/macro policy mapped; safe error normalization | Word/Excel/PowerPoint fidelity and actual Office/provider behavior |
| Image | Deterministic operation mapped; ML provider only for ML capability | Pixel/codec correctness, ML quality, resource benchmark |
| Content Intelligence | Minimized selected context, purpose, local/internal route, policy outcome mapped; prompt/output remains untrusted | Model quality, real provider retention/egress, public-network proof |
| PDF to Markdown | Deterministic extraction route first, OCR fallback request only when required | Complex layout/table fidelity and vendor extraction internals |

Each adapter failure test includes a forbidden-field canary assertion across returned error, state snapshot, captured log, audit intent, and telemetry event.

---

## 8. Resolver tests

Resolver inputs include capability, platform, policy, classification, feature risk/status, format/size/page/file count, memory/storage, connectivity, server/provider availability, approved-provider/version/license profile, and security/resource limits.

| ID | Scenario | Expected resolution |
|---|---|---|
| `ER-001` | Approved compatible primary local provider supports request | Select primary local provider |
| `ER-002` | Multiple equivalent local providers | Stable configured priority/tie-break; no nondeterministic selection |
| `ER-003` | Primary lacks exact feature/format but approved specialized local fallback supports it | Select fallback with explicit reason code |
| `ER-004` | Primary returns transient unavailable after selection | Only approved fallback eligible under policy/attempt budget |
| `ER-005` | Primary failure is validation/security/content error | Do not fallback automatically if that would bypass control |
| `ER-006` | Local provider capable but version incompatible | Do not select; safe compatibility outcome or approved fallback |
| `ER-007` | Local capacity insufficient and Optional Server allowed/online | Return server candidate requiring preflight/consent, not direct provider call |
| `ER-008` | Offline and only server provider can satisfy | No provider selected; waiting/online-required |
| `ER-009` | Server unavailable and deterministic local tool is unrelated | Unrelated local tool remains available; no global outage |
| `ER-010` | Public/unapproved AI/translation provider appears in configuration | Reject provider; no fallback to it under any circumstance |
| `ER-011` | Approved enterprise-internal AI available but permission/policy/minimization/consent missing | Not selected |
| `ER-012` | AI local provider approved and capable offline | Local provider selected; no server/network call |
| `ER-013` | UI/request names vendor/provider/engine/class/assembly | Input rejected/ignored; resolver uses approved registry only |
| `ER-014` | Unknown Feature ID or no capability mapping | Unsupported before provider discovery |
| `ER-015` | Feature disabled/not exposed/feasibility-blocked | Not selected even if provider claims support |
| `ER-016` | License profile disallows operation | Provider not selected; safe unavailable/alternative outcome |
| `ER-017` | Format/size/page/file-count outside provider capability | Provider excluded; approved fallback considered without weakening limits |
| `ER-018` | Security/classification disallows provider route | Provider excluded regardless of quality/capability |
| `ER-019` | `LOCAL` execution profile | Server candidates never returned |
| `ER-020` | `OPTIONAL_SERVER` current Core feature | Local-first logic applied; server only conditional |
| `ER-021` | Generic `REQUIRED_SERVER` contract used by a future/other feature | Correct server candidate behavior tested without claiming a current Core tool is universally server-required |
| `ER-022` | Dynamic plugin/provider not registered at startup | Not discoverable/selectable |
| `ER-023` | Capability metadata stale/revision mismatch | Refresh/re-evaluate or safe incompatible; no stale privileged selection |
| `ER-024` | Same facts after an approved fallback attempt already consumed | Attempt history prevents provider cycle/infinite fallback |

Resolver tests use mock provider descriptors only and do not instantiate vendor SDKs.

---

## 9. Cleanup tests

Cleanup is a lifecycle decision over manifest-owned artifacts/resources, not arbitrary filesystem traversal.

| ID | Scenario | Expected decision/side effects |
|---|---|---|
| `CL-001` | Expired unreferenced local temp in managed manifest | Eligible; claim bounded item; delete via artifact ID; finalize metadata after success |
| `CL-002` | Cache/thumbnail expired by bounded policy and parent inactive | Eligible; no source/result deletion |
| `CL-003` | Active job/workspace/reference/lease | Skip/defer |
| `CL-004` | Valid checkpoint inside recovery window | Keep |
| `CL-005` | Checkpoint terminal, unreferenced, recovery window expired | Eligible |
| `CL-006` | Pending/unacknowledged Audit outbox or secured transfer | Never evict as cache; retain/retry/block per policy |
| `CL-007` | Local final result mislabeled by age-only rule | Keep; age alone does not convert result to cache |
| `CL-008` | User clears Personal History | Cleanup does not delete local result, secured file, or Audit |
| `CL-009` | Raw path, missing manifest ownership, or path-scope failure | Reject and record safe failure; no recursive delete |
| `CL-010` | Fake external delete succeeds | Verify outcome then remove/finalize metadata exactly once |
| `CL-011` | Fake external delete transiently fails | Retain `DELETE_PENDING`/cleanup metadata and bounded retry |
| `CL-012` | Repeated cleanup after prior success | Idempotent no-op/same terminal outcome |
| `CL-013` | Cleanup crashes after claim, before delete | Lease expires/reclaimed; candidate remains recoverable |
| `CL-014` | Cleanup crashes after delete, before metadata finalization | Reconciliation recognizes missing object and finalizes idempotently |
| `CL-015` | Storage/CPU/DB pressure high | Reduce cleanup concurrency/batch; do not starve user work |
| `CL-016` | Candidate set larger than configured batch | Only bounded ordered subset claimed; continuation/checkpoint retained |
| `CL-017` | Temporary password/OCR/signature working state operation completes/cancels/expires | Password absent from store; sensitive temp cleanup eligible according to recovery need |
| `CL-018` | Tombstoned secured version physical purge fails | Access remains denied; retry metadata retained; max-three visible invariant preserved |
| `CL-019` | Legal hold or explicit retention reference injected | Skip; cleanup cannot override |
| `CL-020` | Server tombstone exists but user has local working copy | Local copy not silently deleted |
| `CL-021` | Cleanup error includes physical path/secret | Returned/logged safe code only |

Actual filesystem deletion, archive technology, SSD secure erase, and object-store behavior are not proven by these unit tests.

---

## 10. Security-sensitive local tests

### 10.1 Path and profile boundary

| ID | Scenario | Expected assertions |
|---|---|---|
| `SL-001` | Traversal/absolute/UNC/device/ADS/control/reserved/ambiguous name | Rejected before storage/Worker call |
| `SL-002` | Injected canonical-root/final-handle validation fails | Reject read/write/delete |
| `SL-003` | Resource/artifact belongs to another local profile | Reject; no metadata/content projection |
| `SL-004` | Account switch | Current store closed; target store selected; no implicit merge/cache reuse |
| `SL-005` | Local Worker command attempts arbitrary DB/SQLite/path/network/provider | Closed contract validator rejects |

### 10.2 Forbidden persistence and synchronization

Use unique canary strings for password, token, secret, clipboard, signature bytes, document text, OCR text, path, and provider credential. Serialize every relevant state object through the same approved mapper used by production design, then assert canaries are absent.

| ID | State/output inspected | Required assertion |
|---|---|---|
| `SL-010` | Offline authorization snapshot | No token/cookie/password/service credential/unrestricted claims/document content/privileged permission |
| `SL-011` | Workspace/local job/checkpoint/recovery metadata | No password/token/secret/signature bytes/document/OCR text |
| `SL-012` | Pending server intent/idempotency snapshot | No raw HTTP body, arbitrary URL/command/path, file bytes, password, clipboard, signature |
| `SL-013` | Audit outbox | Metadata only; no filename/path/document/OCR/prompt/issue/signature/secret |
| `SL-014` | Captured logger/telemetry/error | No forbidden canary or raw provider/engine exception |
| `SL-015` | Sync candidate enumeration | Signature asset, draft, cache, temp, thumbnail, checkpoint excluded |
| `SL-016` | Password-required operation completed/failed/cancelled | Password not persisted and operation-memory holder cleared/disposed |
| `SL-017` | Visual signature save/use | Asset remains protected local and never enters generic sync/Audit/log |
| `SL-018` | Temporary sensitive artifact lifecycle | Cleanup decision honors final/cancel/recovery expiry and classification |
| `SL-019` | Local AI/Intelligence route | Only approved local descriptor; no network/public endpoint fallback |
| `SL-020` | Worker request | No human access/refresh token or Administrator/Support/Auditor role payload |

### 10.3 Security decision behavior

- corrupt/unknown newer local schema never silently resets database or deletes draft;
- uncertain trusted time/clock rollback fails closed for offline grant;
- lower-priority user/device preference cannot disable validation, safe path, audit, or content-log protections;
- raw Worker/provider output remains untrusted until normalized and validated;
- “temporary” artifacts retain source classification;
- secure-delete guarantee is never asserted by a unit test—only lifecycle/reference removal behavior is tested;
- platform backup-exclusion decision can be unit-tested, but actual OS backup exclusion is a release integration check.

---

## 11. Failure/recovery tests

| ID | Failure point | Expected recovery behavior |
|---|---|---|
| `FR-001` | Desktop run marked unclean with valid draft snapshot | Startup recovery classifies candidate and offers resume/retry/cancel; no silent discard |
| `FR-002` | Unclean run with corrupt latest snapshot but previous valid snapshot | Preserve current source; select/report last valid recovery point according to policy |
| `FR-003` | Worker crashes during job | Desktop/UI coordinator remains alive; job maps failure/recovery candidate; source unchanged |
| `FR-004` | Restart sees `RUNNING` local job without live Worker lease | Expire/reclassify as recovery candidate, never completed |
| `FR-005` | Worker crash with compatible checkpoint | Resume option allowed; attempt history advances; same logical Job/Correlation IDs |
| `FR-006` | Worker crash with incompatible checkpoint | Resume denied; explicit retry-from-safe-start/cancel only |
| `FR-007` | Network disconnect during local-capable operation | Local operation continues; no forced server wait/upload |
| `FR-008` | Network disconnect during server-required pending operation | `WAITING_CONNECTION`; preserve typed intent/idempotency; no duplicate |
| `FR-009` | Storage nearly full before start | Preflight warning/block; no partial final output |
| `FR-010` | Full disk during artifact staging | Old/source state preserved; staging cleanup/recovery candidate created |
| `FR-011` | Engine primary fails and approved fallback available | Resolver evaluates typed fallback once under policy; safe status/correlation preserved |
| `FR-012` | No approved fallback | Stable safe failure; no dynamic/public provider |
| `FR-013` | Server job cache stale after reconnect | REST authoritative revision replaces cache; stale SignalR/local projection ignored |
| `FR-014` | Sync conflict remote version changed | Explicit conflict options; no silent overwrite/merge |
| `FR-015` | Crash before owner/state commit | Mutation and intent invisible; source unchanged |
| `FR-016` | Crash after durable intent/state commit before dispatch/ack | Startup dispatcher/retry resumes same logical operation/idempotency |
| `FR-017` | Audit append succeeds but acknowledgement lost | Replay stores/acknowledges once; no semantic duplicate |
| `FR-018` | Audit outbox unavailable for mandatory L3 action | Action blocked before mutation according to Audit Final fail-closed rule |
| `FR-019` | Audit poison/version-blocked event | Retained visible; no hot loop or silent drop |
| `FR-020` | Cleanup/archive deletion step fails | Keep source/reference state and checkpoint; bounded resume |
| `FR-021` | Local store reports corruption/unsupported newer schema | Safe unavailable/recovery flow; no empty replacement DB |
| `FR-022` | Issue attachment upload fails | Issue message state preserved; attachment retry/send-without decision available |
| `FR-023` | Timeout/cancellation races with completion | One valid terminal state; validated completed result cannot regress to cancelled |
| `FR-024` | Restart during secured-transfer commit ambiguity | Reconcile authoritative server state with same idempotency key before new upload |

Recovery tests use deterministic fake crash points and repositories. Actual process kill, power loss, WAL recovery, server failover, and object-store ambiguity require integration/fault-injection suites.

---

## 12. Risks/gaps

| ID | Risk/gap | Unit-test implication | Required next boundary |
|---|---|---|---|
| `UT-C-001` | Exact Processing Coordinator/Resolver public contracts are not yet production code | Test names remain semantic; avoid inventing concrete class signatures | Integrator defines stable seams before coding |
| `UT-C-002` | Named Pipe message schema/handshake field names and version catalog are not fully locked in current documents | Test required semantics/version failure, not fabricated wire DTO | Controlled IPC contract artifact + contract tests |
| `UT-C-003` | Actual Named Pipe ACL/peer identity/remote denial cannot be proven in unit tests | Fake verified peer facts only | Windows integration/security PoC |
| `UT-C-004` | Worker crash/OOM/recycle/resource containment is OS/process behavior | Unit-test lifecycle mapping only | Real Worker process integration/PoC |
| `UT-C-005` | Safe path semantics depend on NTFS/mobile platform and race-resistant final handle | Unit-test lexical/policy and injected handle facts | Filesystem integration matrix |
| `UT-C-006` | SQLite/WAL/SHM/encryption/key/backup behavior is not an abstraction-only guarantee | Unit-test repository rules only | SQLite/artifact/key integration + Security PoC |
| `UT-C-007` | Real full-disk, atomic rename, flush/durability, file lock, reparse, and crash behavior differ by filesystem | Fake error/order is necessary but insufficient | Platform integration/fault injection |
| `UT-C-008` | Vendor engine fidelity/security/resource behavior remains PoC-gated | Mock only adapter boundary; no internal library tests | Engine malicious corpus, fidelity, performance PoC |
| `UT-C-009` | Approved provider list, capability/version semantics, and fallback priority are configuration/governance owned | Data-driven resolver tests need approved fixtures | Provider/engine decision artifact |
| `UT-C-010` | Numeric timeout/retry/backoff/concurrency/quota/cleanup thresholds are not finalized | Test invariants and injected values; no hardcoded production numbers | Document 12 benchmark/load-test decisions |
| `UT-C-011` | Cross-platform opaque file identity and trusted clock semantics remain implementation-specific | Inject file/time evidence | Desktop/Mobile platform integration tests |
| `UT-C-012` | Generic full workspace sync is intentionally absent | Do not invent sync repository/route to satisfy tests | Test only four locked outbound paths |
| `UT-C-013` | Audit outbox sync is automatic but metadata-only, unlike content sync | Tests must not incorrectly reject required Audit sync | Keep distinct Audit contract suite |
| `UT-C-014` | `WAITING_RESOURCE`/`WAITING_CONNECTION` Job states differ from secured-transfer lifecycle labels | Avoid one universal state enum in tests | Separate state-machine fixtures |
| `UT-C-015` | No current Core Feature is universally `REQUIRED` server-side | Generic resolver case may exist, but test must not rewrite Core mappings | Keep Document 06/09 mapping fixtures authoritative |
| `UT-C-016` | Maximum-three secured versions combines aggregate logic, DB concurrency, and object cleanup | Unit tests prove decision; not transaction isolation | SQL/object integration race tests |
| `UT-C-017` | Secure deletion cannot be guaranteed on SSD/flash/snapshots | Never assert forensic erase | Test only access removal/lifecycle/key-policy handoff |
| `UT-C-018` | Physical scanner and OS camera/share/open-with are outside unit boundary | Use adapter mocks only | Device/platform integration/PoC |
| `UT-C-019` | Public AI must be technically unreachable, not merely unselected by a mock | Unit resolver denial is necessary but insufficient | Egress/config/security integration tests |
| `UT-C-020` | Forbidden-field tests can leak their own canaries through failure output/snapshots | Use synthetic non-secret canaries and safe assertion messages | Test-data handling standard |
| `UT-C-021` | Production error-code catalog may not yet contain every local/Worker/storage mapping | Assert semantic category and safe shape until canonical codes locked | Error catalog/contract handoff |
| `UT-C-022` | Local L2 Audit degraded behavior when outbox unavailable still requires approved policy | Unit test both policy-selected paths; L3 remains fail-closed | Audit/enterprise policy confirmation |

No gap justifies testing against real cloud/provider/scanner in the unit suite, coupling business tests to SQLite/filesystem/vendor internals, or weakening Local-First/security behavior.

---

## 13. Suggested Document 11 text

Teks berikut dapat dipertimbangkan Integrator untuk Document 11.

### 13.1 Local/Worker/Storage unit-test boundary

```text
Local-First, Processing Coordinator, Worker contract, local storage/workspace,
selective sync, retry/checkpoint, cleanup, Engine Adapter, and Provider Resolver
are tested through deterministic contracts with MSTest + MTP.

Unit tests use fake clock/random/connectivity/capability/policy/capacity,
in-memory repositories, fake artifact stores/transports, and mock providers.
They do not start a real Worker, open Named Pipes, use full filesystem/SQLite,
call real server/cloud/provider, or use a physical scanner. Those guarantees
belong to integration, security contract, fault-injection, and PoC suites.

Every unit test asserts the expected result and forbidden side effects. A
local or offline route must prove zero server-preflight/upload/execute calls.
An unsupported/denied route must prove zero Worker/engine/provider calls.
```

### 13.2 Mandatory Local-First tests

```text
LOCAL Feature:
  execute in-process or Local Worker according to workload safety;
  never call server processing or upload.

OPTIONAL SERVER:
  prefer approved compatible local provider when capability, policy,
  classification, and resource budgets pass;
  use server preparation only when local is unsuitable and online
  permission/policy/capacity/notice/consent pass.

OFFLINE:
  eligible local work may continue under a valid protected grant;
  server-required work becomes ONLINE_REQUIRED/WAITING_CONNECTION;
  no automatic upload occurs.

NO AUTO-SYNC:
  source, draft, working state, undo/redo, cache, thumbnail, temp,
  checkpoint, clipboard, password/token/secret, signature, local history,
  recent file, and Assistant conversation have no generic sync mapping.

Amankan ke Akun:
  only an explicit action on one eligible validated selected file/result
  creates a secured transfer. Execution transfer, secured transfer, Audit
  outbox, and preference sync remain separate typed paths.
```

### 13.3 Processing Coordinator and Worker contract tests

```text
Processing Coordinator tests cover immediate/background, local/server,
capability and resource budgets, policy precedence, connectivity, consent,
capacity/backpressure, input/Feature/schema validation, cancellation,
Correlation ID propagation, idempotency, partial-result rules, output
validation, and approved fallback only.

Worker/IPC unit tests cover valid operation, closed operation allowlist,
unknown operation/type/version/field, schema mismatch, invalid Feature ID,
message bounds, job/profile/instance/ref mismatch, stale Worker, cancellation,
timeout, crash mapping, restart/recovery, heartbeat, Correlation ID,
WAITING_RESOURCE, safe progress/error normalization, and prohibition of
arbitrary path/URL/provider/engine/SQL/shell/process/assembly/user token.

Actual Named Pipe identity/ACL, process crash/OOM/recycle, and OS resource
containment are not claimed by unit tests and require integration/PoC.
```

### 13.4 Storage, workspace, sync, and cleanup tests

```text
Storage abstraction tests cover save/read/missing/conflict/quota/full-disk
mapping, safe logical path policy, staged write order, atomic state-pointer
decision, workspace snapshot/operation cursor, temporary-result validation,
result expiry, and source immutability.

Secured-file aggregate tests prove maximum three active/addressable versions:
creating version four activates the new version and tombstones the oldest in
one decision; failures and stale/concurrent requests cannot exceed three or
silently move the current pointer.

Sync tests cover pending/success/network unavailable/retry/duplicate/conflict,
remote version/ETag change, stale local cache, reconnect reauthorization,
same Idempotency-Key reconciliation, explicit sync only, metadata-only Audit
outbox, and allowlisted preference sync. Binary content is never auto-merged
or silently overwritten.

Cleanup tests use manifest artifact IDs only and cover eligibility, active
reference/lease/hold skip, bounded claim, idempotent delete/finalize/retry,
crash reconciliation, pressure-aware concurrency, checkpoint recovery window,
unacknowledged outbox protection, sensitive-temp lifecycle, and the rule that
History deletion or cache cleanup never deletes source/final result/Audit.
```

### 13.5 Retry, checkpoint, adapter, and resolver tests

```text
Retry tests use a fake clock and deterministic jitter. Only transient
allowlisted outcomes retry; attempts/backoff are bounded; cancellation stops
retry; logical Job ID and Correlation ID remain stable; attempt history is
append-only; offline/resource waiting does not become a retry storm.

Checkpoint resume requires matching input identity/fingerprint, canonical
Feature ID, contract/parameter schema, engine/provider version, stage,
integrity marker, and expiry. Mismatch/corruption/expiry rejects resume and
never silently restarts as if resume succeeded. Password/token/secret/content
is absent from checkpoint state.

Engine Adapter tests mock the provider boundary and verify input mapping,
provider output mapping, output validation, safe errors, unsupported feature,
unavailable provider, cancellation/timeout, capability metadata, and version
compatibility. The unit suite does not test third-party PDF/OCR/Office/image
library internals, fidelity, or parser security.

Resolver tests verify deterministic approved-provider selection, local-first,
capability/platform/policy/classification/resource/version/license gates,
approved fallback only, no provider cycles, no dynamic plugin/vendor choice,
no public AI, no server result for LOCAL profiles, and no server route while
offline without an approved waiting state.
```

### 13.6 Security and recovery acceptance

```text
Use synthetic canaries to prove password, token, secret, document/OCR text,
clipboard, signature bytes, raw path, provider credential, and end-user token
are absent from offline grants, workspace/job/checkpoint/recovery state,
pending intents, Audit outbox, logs/telemetry/errors, Worker messages, and
sync candidates.

Failure tests cover Desktop unclean shutdown, Worker crash, missing live lease,
compatible/incompatible checkpoint, network loss, storage pressure/full disk,
engine failure/fallback, stale server cache, sync conflict, crash before/after
commit/ack, mandatory Audit failure, poison versions, cleanup interruption,
corrupt/unsupported local schema, and terminal cancellation/completion races.

Unit tests prove decision/state/mapping behavior only. Real filesystem,
SQLite/WAL/encryption/key, Named Pipe/process, object storage, network/cloud,
engine/provider, scanner, security containment, and performance evidence remain
mandatory in their appropriate integration/PoC/test-plan layers.
```

---

## Closing recommendation

Document 11 sebaiknya membuat test inventory yang traceable ke invariant, bukan ke implementation detail. Pola paling penting untuk area ini adalah: **input facts → decision/state → expected collaborator calls → explicitly forbidden calls/data**. Pola tersebut menjaga test cepat dan stabil sekaligus mencegah unit suite memberikan keyakinan palsu terhadap OS, filesystem, Worker process, database, provider, atau cloud behavior yang hanya dapat dibuktikan pada layer lebih tinggi.

