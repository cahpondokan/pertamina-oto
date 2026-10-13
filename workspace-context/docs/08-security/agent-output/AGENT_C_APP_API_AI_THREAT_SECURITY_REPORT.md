# AGENT C — APPLICATION, API, WORKER & AI THREAT SECURITY REPORT

| Atribut | Nilai |
|---|---|
| Task ID | `DOC08-AGENT-C` |
| Fokus | Application, API, Worker/IPC, document engine, Content Intelligence, dan AI threat security |
| Status | **REKOMENDASI — BELUM LOCKED** |
| Tanggal | 24 Agustus 2026 |
| Decision owner | User + Integrator |

## Baseline, scope, dan metode

Laporan ini diturunkan dari Documents 01–07 FINAL dan `MASTER_PROJECT_SUMMARY.md` v3.0. Baseline yang tidak dibuka ulang:

- REST adalah source of truth; SignalR hanya event notification;
- request, workflow, IPC, dan event memakai typed/versioned contracts;
- canonical Feature ID + closed registered handler;
- engine/provider berada di belakang adapter/resolver;
- Local Worker terisolasi dan memakai Named Pipe/local IPC;
- public AI/translation endpoint dilarang;
- hanya approved local atau enterprise-internal AI/provider;
- Assistant/AI tidak mempunyai direct SQL, shell, unrestricted filesystem, atau arbitrary API;
- AI output dan document/OCR content adalah untrusted;
- permission, policy, capability, consent, confirmation, dan result grounding tidak dapat dilewati Assistant;
- layered admission control, bounded queue, quota, concurrency, dan backpressure wajib;
- metadata berada di SQL/SQLite; restricted content berada pada approved protected artifact/object-storage boundary;
- Administrator/Support tidak otomatis memperoleh akses isi dokumen user.

Metode menggunakan STRIDE secara praktis. Setiap threat mempunyai entry point, impact, mitigation, dan test. Laporan ini tidak memilih product/vendor, tidak mengubah arsitektur, tidak menghapus fitur, tidak mengubah master Document 08, dan tidak membuat kode.

Traceability utama: `SR-SEC-001..023`, `NFR-PERF-007..013`, `IR-API-001..012`, `FR-PROC-001..005`, `FR-ASSIST-001..015`, `FR-SUP-001..007`, `FR-INTEL-001..003`, `FEAT-VALID-001..018`, `FEAT-SEC-001..023`, `FEAT-ASSIST-001..030`, `FEAT-INTEL-001..002`, `FEAT-CONV-017..018`, `FEAT-CONV-036`, API Final sections 9, 11–18, dan Database Final sections 10, 13–18.

---

## 1. System attack surface

### 1.1 Assets yang dilindungi

- Pertamina identity/session, permission, offline authorization snapshot, dan service identity;
- source document, upload, temporary result, secured file/version, checkpoint, issue screenshot, visual signature asset, OCR/extracted text, summary, translation, dan Markdown output;
- canonical Feature contract, workflow plan, parameter schema, policy, feature flag, quota, confirmation state, dan consent evidence;
- job state, idempotency record, lease, outbox, audit evidence, Correlation ID, dan safe diagnostics;
- SQL Server, SQLite per-profile, app-managed filesystem, object storage, Named Pipe, API, SignalR hub, Worker, document engine, dan approved internal provider;
- application availability dan server resource budget: CPU, RAM, DB connections, storage, I/O, network, Worker slots, queue, serta provider capacity.

### 1.2 Attacker assumptions

Threat model mencakup:

- unauthenticated network attacker;
- authenticated user yang mencoba akses object/permission lain;
- compromised browser/native session atau stolen device;
- malicious same-device process, malware, atau different OS account;
- malicious document, archive, image, font, HTML, Office/PDF embedded object, OCR text, atau issue content;
- compromised/buggy engine, dependency, internal provider, Worker, atau build artifact;
- retry/replay/bot/flood client;
- honest user yang memberi perintah ambigu/destructive kepada Assistant;
- malicious or manipulated AI/provider response.

### 1.3 Attack-surface inventory

| Surface | Entry/data | Primary threats |
|---|---|---|
| Web UI / MVC | cookie/session, form, issue text, filename, OCR/Markdown/AI output, preview/download | XSS, CSRF, clickjacking, CORS/origin error, unsafe rendering, token/session theft |
| REST API / reverse proxy | headers, route IDs, JSON, multipart/chunks, query/filter/cursor, range, idempotency key | BOLA/BFLA, injection, smuggling, oversized request, replay, DoS, sensitive error |
| Desktop WinUI | drag/drop, clipboard, file association, local path, SQLite/artifacts, offline snapshot, Local Worker | malicious file, traversal/reparse, local privilege/IPC abuse, stale permission, data-at-rest leakage |
| Mobile/Tablet MAUI | camera, share/open-with, local store, automatic OS backup, clipboard | malicious input, backup leakage, permission misuse, lost device, resource exhaustion |
| Named Pipe/local IPC | operation, file reference, parameters, progress, cancel, heartbeat, error | unauthorized client, Worker impersonation, unsafe deserialization, command injection, flood |
| Local Worker | local documents, engines, temp/work directory, process launch | parser RCE, privilege escalation, filesystem escape, CPU/RAM/temp exhaustion, crash propagation |
| Server Worker | queue/job, object storage, DB/service identity, engines/providers | forged job, over-privileged identity, duplicate execution, SSRF, resource starvation, lateral movement |
| SignalR Activity Hub | negotiate/connect, subscription, server events, reconnect | spoofed subscription, token leakage, stale/replayed event, connection flood, information disclosure |
| Feature execution API | Feature ID, typed parameter, `fileRef`, workflow step, retry/resume | arbitrary dispatch, tool hijack, input ownership bypass, state race, idempotency abuse |
| Upload/download | multipart/chunk/range, content headers, filename, compressed/embedded data | size abuse, decompression bomb, malware, parser exploit, path traversal, content sniffing |
| PDF/Office/image/OCR engines | malformed document, font/image/codec, macro/OLE/link, native library | parser exploit, code execution, external fetch, output corruption, resource exhaustion |
| HTML/Web-to-PDF | HTML, CSS, script, URL, redirects, fonts/images, browser renderer | SSRF, local-file access, script execution, DNS rebinding, credential leakage, infinite rendering |
| Assistant | user intent, local context, document/OCR excerpt, workflow plan | prompt injection, indirect injection, tool hijack, hallucinated action, confirmation bypass |
| Content Intelligence | minimized content, internal provider request/response | content exfiltration, provider compromise, malicious output, retention/logging leakage, capacity abuse |
| Issue/support/export | message, note, screenshot, diagnostics, PDF/Excel export | stored XSS, spreadsheet formula injection, oversharing, unsafe diagnostics, permission bypass |
| Supply chain | NuGet/native library/model/package/update/build pipeline | malicious dependency, typosquat, vulnerable parser/model, artifact tampering |

---

## 2. Trust-boundary diagram dalam teks

```text
UNTRUSTED / USER-CONTROLLED
  Browser input, Desktop/Mobile file, clipboard, camera/share intent,
  document/HTML/OCR text, issue message/screenshot, AI/provider output
                │
                ├───────────────────────────────────────────────┐
                ▼                                               ▼
        [Web UI / Desktop / Mobile]                     [Pertamina Identity]
                │ HTTPS / approved session                       │
                └──────────────────┬──────────────────────────────┘
                                   ▼
                         [Reverse Proxy / API]
                         authn → authz → policy
                         typed schema → quota
                                   │
                 ┌─────────────────┼──────────────────┐
                 ▼                 ▼                  ▼
        [Application Service] [SignalR Hub]     [Upload/Download]
                 │             event only        stream + validate
                 │                 │                  │
                 ▼                 │                  ▼
        [SQL metadata/outbox]──────┘        [Secure Object Storage]
                 │                                    │
                 ▼                                    ▼
            [Server Queue] ───────────────► [Server Worker]
                                                │
                                  job-scoped ref│least privilege
                                                ▼
                                  [Engine / OCR / Converter]
                                                │
                          approved fixed egress │ only when required
                                                ▼
                               [Internal Content Intelligence]

LOCAL DEVICE TRUST BOUNDARY
  [Desktop UI / application service]
                 │ typed/versioned Named Pipe; per-user/process boundary
                 ▼
          [Isolated Local Worker]
                 │ job-scoped file/artifact refs
                 ▼
        [Local Engine + isolated temp]

CONTROL-PLANE RULE
  AI/document/provider output
      → always DATA/untrusted
      → never direct SQL/shell/filesystem/HTTP
      → canonical Feature ID + typed parameters
      → permission/policy/capability/confirmation
      → official application service
      → actual result grounding
```

Trust is not inherited from being “internal,” from a queue, from an opaque ID, from a pipe name, from an authenticated SignalR connection, or from an approved provider. Every boundary revalidates the claims/data it needs.

---

## 3. Threat model

| No | Threat / STRIDE | Entry Point | Impact | Mitigation | Required Test |
|---:|---|---|---|---|---|
| 1 | Broken access control / EoP | API object ID, cursor, `fileRef`, job/result, secured file, issue/attachment, support/admin, hub subscription | cross-user document/metadata access; unauthorized action/export/delete | deny-by-default; permission + ownership + policy on every request/continuation/subscription; server-derived scope; consistent concealment; no ID as authority | IDOR/BOLA/BFLA matrix across two users and every method; permission revoke during cursor/job/download |
| 2 | Injection / T | JSON, query/filter/sort, headers, filename, issue text, workflow/feature parameter, export cell | control/data corruption, XSS, log forging, downstream interpreter abuse | typed schema; `additionalProperties=false`; allowlisted fields/enums; contextual output encoding; structured logs; safe export cell handling | fuzz metacharacters/CRLF/Unicode; stored/reflected injection; export formula payloads |
| 3 | Command injection / T/EoP | converter/process arguments, filenames, HTML renderer switches, AI tool plan | arbitrary OS command/code execution | no shell; fixed executable/operation allowlist; structured argument API; reject client engine/class/command; low-privilege isolated process | filenames/parameters with quotes, separators, expansion, newline; verify no child command/process escape |
| 4 | SQL injection / T/I | API search/filter, report/export filter, admin query, raw diagnostics | data disclosure/modification, privilege escalation | EF/parameterized query; allowlisted query shape; no client direct SQL; no dynamic identifier from client; least-privilege DB identity | SQLi corpus including time/error/boolean cases; SAST; verify DB identity cannot cross owner schemas |
| 5 | Path traversal / T/I | upload/output filename, archive entry, local artifact, file association/share intent, download name | read/write outside workspace, overwrite source, secret disclosure | opaque refs; canonical path under approved root; reject absolute/UNC/device/ADS; safe generated names; no trust in filename; reparse/symlink defense | `../`, encoded/Unicode traversal, UNC, device names, NTFS ADS, zip-slip, symlink/reparse race |
| 6 | Unsafe deserialization / T/EoP | API JSON, SignalR payload, Named Pipe, persisted workflow/checkpoint, provider response | type confusion, gadget execution, memory DoS, contract bypass | sealed typed/versioned contract; no type-name polymorphism; depth/size/count bounds; unknown type/version fails closed | malformed/recursive/duplicate-field/unknown discriminator/version fuzz; serialization compatibility tests |
| 7 | SSRF / S/I/EoP | Web-to-PDF URL, HTML CSS/image/font/link, redirects, provider callback/config | internal service/metadata access, credential theft, port scan, data exfiltration | remote fetch off by default except feature policy; typed URL input; scheme/host/IP allowlist/deny rules; resolve and revalidate redirects/DNS; block loopback/link-local/internal metadata; no ambient cookies/credentials; egress/time/size limits | encoded IP, IPv6, DNS rebinding, redirect chain, alternate schemes, localhost/link-local/private target, large/slow response |
| 8 | XSS / T/I | issue message/note, filename, OCR/Markdown/AI output, error/status, HTML preview | session theft, unauthorized action, support console compromise | contextual encoding by default; no raw HTML; strict sanitizer only if markup required; CSP; safe Markdown renderer; inert links; safe content disposition | stored/reflected/DOM XSS corpus, SVG/event handlers, Markdown raw HTML/link schemes, CSP regression |
| 9 | CSRF / T/EoP | Web cookie/session mutating API, issue/admin actions, hub negotiate/logout | action performed using victim session | anti-forgery token; SameSite/Secure/HttpOnly cookies; Origin/Referer validation; no state-changing GET; reauth for sensitive action | cross-site form/fetch, missing/invalid token, SameSite variants, logout/admin/delete CSRF |
| 10 | CORS misuse / I/EoP | REST API, upload, SignalR negotiate | hostile origin reads/acts with credentials | same-origin preferred; exact approved origins/methods/headers; no wildcard with credentials; no origin reflection; CORS not authorization | hostile/null/file origin, subdomain confusion, preflight bypass, credentialed wildcard test |
| 11 | Request smuggling / T/I | reverse proxy ↔ ASP.NET Core, HTTP/2 downgrade, `Content-Length`/`Transfer-Encoding` ambiguity | request boundary confusion, cache/auth bypass, poisoned next request | one governed proxy chain; reject ambiguous duplicate framing/headers; aligned limits/timeouts/protocol; trust forwarded headers only from known proxy; patch stack | CL.TE/TE.CL/duplicate CL/obs-fold/H2 downgrade tests through actual deployment path |
| 12 | Oversized request / DoS | headers, JSON, multipart, chunk, batch, hub/IPC message | RAM/thread/temp/storage exhaustion | layered proxy/API/IPC limits; streaming; hard byte/part/depth/count limits; early cancellation; bounded temp storage | boundary ±1 tests, chunked body without declared length, large headers/JSON arrays, slow upload |
| 13 | Zip/decompression bomb / DoS | ZIP/Office container, PDF stream, embedded archive/image | CPU/RAM/disk exhaustion, Worker crash | compressed/uncompressed total, ratio, entry count, nesting/depth, dimensions/pages, timeout; no unsafe extraction; isolated temp | nested/overlapping archive, huge declared size, many tiny entries, known bomb corpus |
| 14 | Malformed document / T/DoS | file picker, drag/drop, upload, share intent, clipboard, secured file | crash, corruption, parser confusion, unsafe output | staged validation; quarantine; parse in Worker; immutable source; result validation; safe failure code | malformed/truncated/cross-MIME/polyglot corpus; differential parser test; source integrity check |
| 15 | Parser exploit / EoP | PDF/Office/image/font/OCR/HTML native parser | code execution under UI/Worker/service identity, lateral movement | low-privilege isolated Worker; minimal filesystem/network; resource/child-process limits; patch/SBOM; crash containment/recycle; no admin | fuzz/property tests; exploit regression corpus; verify process/file/network containment and UI/API survival |
| 16 | Resource exhaustion / DoS | huge pages/images/fonts/object graph, OCR/table, AI context, conversion batch | unresponsive client/server, OOM, storage full | preflight cost; CPU/RAM/time/temp/page/pixel/context budgets; cancel; bounded concurrency; checkpoint/cleanup | worst valid input, concurrent heavy jobs, cancellation, memory/temp limit, leak/recycle soak |
| 17 | Job flooding / DoS | execute/retry/resume/export/offline backlog | queue/Worker/DB/provider starvation | subject/category quota; weighted expensive-job budget; atomic reservation; bounded fair workload queues; idempotency; gradual backlog release | burst, multi-account, retry storm, reconnect backlog, queue saturation and fairness |
| 18 | Race condition / T/EoP | chunk offset, job lease/state, delete/download, secured fourth version, issue/admin update, permission change | duplicate work, stale overwrite, use-after-delete, policy bypass | rowversion/ETag; expected state; unique constraints; atomic lease/reservation; short transaction; reauthorize at use/commit | parallel chunk/complete/delete/download/restore/version/triage; TOCTOU file replacement |
| 19 | Replay/idempotency abuse / T/DoS | `Idempotency-Key`, chunk digest/range, retry, outbox, offline intent | duplicate job/charge/output, state confusion, resource exhaustion | scoped key hash + fingerprint + expiry; same logical Job ID; duplicate range/digest acknowledgment; mismatched replay conflict; unique event ID | simultaneous same key, same key/different body, expired key, replay after permission change, chunk overlap |
| 20 | SignalR spoofing / S/I | hub connect/subscription/group/event surface | receive other user status, fake UI state, unauthorized command attempt | authenticated hub; server-derived groups; subscription ownership check; hub carries no commands; safe event types | forged user/group/job ID; unauthorized subscription; attempt cancel/execute via hub; cross-user event test |
| 21 | Stale/reordered event / T | duplicate/delayed/gapped SignalR event and reconnect | terminal state regression, wrong progress/action | persisted revision/sequence; duplicate/stale ignore; gap/unknown version → REST reconcile; terminal never regresses | reorder/duplicate/gap/unknown event/version; reconnect with old cache; REST wins |
| 22 | Worker impersonation / S/EoP | server queue/service identity, Local Worker launch/IPC, provider endpoint | forged result/progress, unauthorized file/DB access | distinct scoped identity; authenticated endpoint/pipe; job lease/token/reference; no end-user token forwarding; verify output and state | rogue Worker/service, stolen/reused lease, wrong job/owner, forged completion/output |
| 23 | Named Pipe unauthorized access / S/EoP | discover/connect/send to Desktop↔Local Worker pipe | command execution, file disclosure, job tampering, DoS | current-user/app identity ACL; local-only pipe; first-instance/instance binding; authenticated versioned handshake; size/time limits; operation allowlist; pipe name not a secret | different account, same-user rogue process, remote pipe, stale instance, version mismatch, flood/partial message |
| 24 | Privilege escalation / EoP | admin/support APIs, Worker service account, engine child process, local profile switch | document access, policy change, lateral movement | least privilege; separate identities; granular permission; profile binding; no implicit Admin/Support content access; no token forwarding | BFLA/role matrix negative tests, service account file/network/DB access, account-switch isolation |
| 25 | Engine/provider compromise / T/I/EoP | adapter-loaded engine, internal converter/OCR/AI endpoint | malicious output, data theft, code execution, supply-chain foothold | approved provider registry; integrity/provenance; isolation; job-scoped data; no default network; output/schema/file validation; circuit/disable path | simulated compromised provider response/binary, unauthorized egress, corrupted output, failure containment |
| 26 | Dependency compromise / T/EoP | NuGet/native/model/plugin/build/update source | malicious code/model, secret theft, widespread compromise | approved source; version lock; provenance/integrity where supported; SBOM; SCA/SAST/secret scan; review/license; reproducible governed build; signed artifact/update | tampered/unknown package, typosquat source, vulnerable version gate, SBOM completeness, artifact integrity |
| 27 | Unsafe HTML rendering / T/I/EoP | HTML-to-PDF, Web-to-PDF, Markdown/AI preview, issue export | script/local file execution, XSS, network exfiltration | isolated renderer; scripts/active content off by default; block `file:`/custom schemes/local storage/download; sanitize preview; CSP; remote content policy | JS/CSS exfiltration, local file iframe, data/blob URL, SVG, event handler, endless animation |
| 28 | Remote-content fetch risk / S/I/DoS | CSS `@import`, images/fonts, redirects, web page, nested frames | SSRF, content substitution, tracking, large/slow fetch | explicit policy/consent; no ambient credentials; redirect/host/IP revalidation; request/byte/depth/time limits; approved egress; record safe provenance | remote font/image/frame, redirect to internal IP, DNS rebinding, slowloris, oversized content, websocket/download attempt |
| 29 | OCR untrusted output / T/I | OCR text into UI, Assistant, search, filename/metadata, export | XSS, prompt injection, formula/command/path injection | label as DATA with provenance; encode at sink; length/schema limits; never execute/auto-fetch/use as path/SQL/tool; preview | OCR text containing HTML, formulas, shell/SQL, URLs, hidden prompt, control characters |
| 30 | Prompt injection / T/EoP | user prompt to Assistant/model | attempts to override policy, permission, confirmation, tool scope | system/policy priority; closed canonical registry; typed plan validation; no direct tools; clarification/confirmation; result grounding | jailbreak corpus, policy override, secret request, destructive confirmation bypass |
| 31 | Indirect prompt injection / T/EoP | document/OCR/HTML/metadata/hidden text provided to AI | document controls Assistant/tool or exfiltrates context | separate instruction and data channels; provenance labels; minimal excerpt; content never instruction; no tools during pure content transform; policy checks after model | malicious PDF/OCR/HTML hidden instruction, white text/metadata, translated prompt, cross-document injection |
| 32 | Tool hijacking / T/EoP | model emits invented Feature ID, parameters, workflow edge, provider/URL | unauthorized/destructive action or wrong tool | Feature ID visible/canonical/registered; typed parameter schema; per-step input/permission/policy/capability; allowlisted workflow edges; official service only | invented/deprecated/hidden Feature ID, extra property, unsafe parameter, cyclic/oversized plan, unauthorized step |
| 33 | AI hallucinated command / T | ambiguous intent/low confidence, fabricated result | wrong/destructive processing, false success | clarify ambiguity; preview risky multi-step plan; confirmation cannot be bypassed; no execute on invalid plan; ground response in actual job/result | ambiguous delete/replace/share/upload prompts, low-confidence routing, provider says success without result |
| 34 | Data exfiltration to AI / I | document/OCR/prompt/context sent to provider | confidential/restricted content leaves approved purpose/boundary | public endpoint impossible; approved provider only; classification/permission/consent; minimum pages/fields; purpose-bound service identity; no content logs; retention/training controls | outbound allowlist/deny test; inspect minimized request safely; verify no unauthorized endpoint/log/cache/retry payload |
| 35 | Malicious AI response / T/I | provider returns script, URL, fake tool call, embedded secret, huge output | XSS, auto-fetch/command, UI DoS, misinformation | response untrusted; schema/size bound; output encode/sanitize; tool response separated from display text; no auto-open/fetch; provenance label | malicious HTML/Markdown/URL/tool JSON/control chars/oversized response; provider signature/state mismatch |
| 36 | Excessive document disclosure / I | full document to OCR/AI/translation/support/export when subset sufficient | privacy/confidentiality breach, unnecessary provider exposure | data minimization by page/range/field; explicit purpose/consent; owner/policy check; short-lived content; no Admin implicit access | verify selected pages only; canceled operation cleanup; cross-purpose reuse blocked; provider payload/log review |
| 37 | Unsafe issue diagnostics / I/T | auto context, screenshot, log window, message, PDF/Excel export | tokens/document content leaked; support XSS/formula injection | allowlisted safe context; no auto document/log attachment; preview + explicit send; image validation; separate permissions; output encode; safe spreadsheet cells | report during sensitive screen; verify no token/path/doc/OCR; stored XSS; Excel formula; attachment authorization |
| 38 | Sensitive-error disclosure / I | API/Worker/engine/provider/AI/DB exception, health/readiness | paths, stack, SQL, storage key, provider/topology, content leakage | stable safe error codes/envelope; raw detail only restricted sanitized server diagnostics; health minimal; redaction tests | forced parser/DB/provider/IPC failures; snapshot API/event/log/issue output for forbidden fields |
| 39 | DoS / DoS | API/proxy, downloads/ranges, SignalR connections, search/export, malformed data | service unavailable to legitimate users | layered rate/timeout/body/query/range/connection/subscription limits; circuit breaker; cancellation; health/backpressure | protocol fuzz, slow request, range amplification, connection storm, search/export soak, dependency timeout |
| 40 | Server resource starvation / DoS | AI/OCR/conversion queues, Worker pools, DB connections, object storage, temp disk | small server collapse; all workloads blocked | workload-separated bounded fair queues; weighted quota; reservations; per-provider/engine concurrency; pool/temp limits; pressure-aware cleanup; preserve local capability | saturate each pool and combined load; verify 429/503 before exhaustion, fairness, recovery, no public fallback |

---

## 4. API threats

### 4.1 Authentication, authorization, dan object access

- API validates approved Pertamina token/session signature, issuer, audience, lifetime, subject, dan claim contract.
- Authorization MUST occur at application service for list, detail, execute, retry, resume, cancel, download/range, attachment, export, admin mutation, dan every cursor continuation.
- Feature discovery only returns visible capability; discovery does not grant execution.
- Route Feature ID is authoritative. Body cannot select engine, provider, service, class, assembly, SQL, shell, local path, storage key, or arbitrary endpoint.
- `fileRef`, Job ID, Correlation ID, cursor, ETag, upload ID, and public opaque IDs are locators only.
- Workflow authorization is per-step, not only parent plan.
- Permission/policy is rechecked at admission and again before sensitive use/commit if the job waited or permission changed.
- Support issue list/detail/attachment/note/triage/export are separate authorization surfaces. Support permission never grants general user-document access.

### 4.2 Strict input and query validation

- JSON/IPC/event command contract defaults to `additionalProperties=false`, explicit discriminator/version, bounded nesting/arrays/string length, and canonical enum.
- Query/filter/sort/search fields remain allowlisted; no raw expression, regex, client-selected database column, or dynamic SQL.
- Filename is display metadata only. Output/download filename is normalized/generated and cannot determine server/local path.
- Header limits include count, total bytes, individual length, and allowed syntax for correlation/idempotency/digest/range.
- Duplicate JSON properties, ambiguous Unicode normalization, malformed numeric/time values, and unknown schema versions fail closed.
- Spreadsheet export neutralizes formula cells beginning with dangerous formula prefixes and does not create macros/external links.

### 4.3 Browser security

- Web UI encodes all user/document/OCR/AI/issue values by context. Raw HTML is not rendered by default.
- Apply CSP with restrictive `default-src`, `object-src`, `base-uri`, `frame-ancestors`, and explicit sources required by the application. Inline/eval exceptions require review.
- Apply secure cookie attributes, anti-forgery for cookie-authenticated mutations, no state-changing GET, origin checks, and reauthentication for sensitive destructive/admin actions.
- CORS is disabled if unnecessary. If required, use exact approved origins; never reflect arbitrary origin or combine wildcard origin with credentials.
- WebSocket Origin is validated separately; browser WebSocket is not protected by ordinary CORS alone.
- Prevent clickjacking, MIME sniffing, open redirect, unsafe `Content-Disposition`, and untrusted URL auto-navigation.

### 4.4 Reverse proxy and HTTP parsing

- Only approved reverse proxy addresses can supply forwarded host/protocol/client information.
- Proxy and API use compatible body/header/request-line/time limits.
- Ambiguous `Content-Length`/`Transfer-Encoding`, duplicate framing, invalid whitespace/line folding, and unsupported transfer coding are rejected.
- Host header and absolute-form target are allowlisted/normalized before URL generation, redirects, or callback construction.
- Request-smuggling tests MUST traverse the actual Production-equivalent proxy/API path, including HTTP/2-to-HTTP/1 transitions if present.

### 4.5 Upload, download, and Feature execution

- Stream uploads; never whole-file buffer. Enforce file, batch, part, chunk, header, temp, decompressed, page/pixel, and processing limits.
- Validation is staged and bounded. Lightweight sniffing is also parser code and remains isolated/defensive.
- Only `READY` upload or authorized available `fileRef` can execute.
- Execution performs closed handler lookup, contract version/schema validation, input ownership/state, permission, policy, classification, capability, consent, engine availability, capacity reservation, and output validation.
- Downloads reauthorize every HEAD/GET/range request, set validated content type, safe filename, length, ETag, `nosniff`, and private/no-store for temporary sensitive results.
- Multiple tiny range requests are rate-limited to prevent amplification.
- Safe errors expose code, safe message, correlation, and typed validation/retry/conflict details only.

### 4.6 SSRF and remote input

Fixed internal provider endpoints come from approved configuration, never client/model output. Web/HTML-to-PDF remote content requires an explicit typed remote-resource contract and fetch policy. Required controls:

- HTTP/HTTPS only unless a narrower approved scheme is defined;
- reject userinfo, control characters, ambiguous/encoded host, unsupported port, local file/custom schemes;
- resolve DNS, validate all resulting IPs, connect only to allowed destination, and revalidate every redirect;
- block loopback, link-local, multicast, network metadata, and internal ranges unless explicitly approved for a specific business destination;
- no ambient browser/API cookies, credentials, client certificates, proxy credentials, or internal authorization headers;
- bound redirects, nested resources, frames, bytes, response time, render time, DNS time, and concurrent fetches;
- block downloads, WebSocket, local storage, and navigation outside policy;
- do not log full URL query if it can contain sensitive data; store safe normalized provenance only.

---

## 5. SignalR threats

### 5.1 Hub security contract

- `/hubs/activity` is notification-only. No execute, upload, cancel, retry, resume, admin, or issue mutation method exists on the hub.
- Connection is authenticated; server derives user group from validated identity. Client-supplied user/tenant/issue/correlation group is rejected.
- Optional job subscription rechecks job ownership/permission, active state, and per-connection subscription limit.
- Authorization changes/session revocation must stop future delivery and cause reconnect/revalidation or connection termination according to policy.
- Event payload remains safe codes/metadata only; never document/preview/OCR, prompt/chat, stack/raw engine message, path/storage key, token, or secret.

### 5.2 Replay, ordering, and spoofing

- `eventId`, `eventVersion`, `streamId`, and monotonic per-stream `sequence` are validated.
- Duplicate/stale event is ignored. Gap/unknown type/version triggers REST reconciliation; it does not crash or invent state.
- Terminal persisted state cannot regress due delayed event.
- Client does not treat correlation/job ID or event arrival as authorization or success proof.
- Reconnect uses bounded backoff+jitter and never resubmits execute command.

### 5.3 Connection abuse

- Bound connection attempts, simultaneous connections per subject/device policy, subscription count, inbound/outbound message size, queued outbound bytes, handshake/keepalive/idle time, and reconnect rate.
- Slow consumer does not create unbounded server memory. Low-value progress may be coalesced; client reconciles authoritative REST state.
- Origin validation is required for browser clients; CORS configuration alone is insufficient for WebSocket origin protection.
- Logs redact hub credential material and query strings.

### 5.4 Credential-transport conflict

Document 06 prohibits tokens in URLs. Browser WebSocket APIs commonly cannot set arbitrary `Authorization` headers and many SignalR bearer flows place an access token in the query string. Document 08 must lock one compliant pattern:

1. browser hub uses same-site secure HttpOnly server session cookie, with Origin/CSRF/session controls; or
2. a controlled revision explicitly permits a very short-lived audience-scoped hub credential in query with strict proxy/app log redaction and leakage analysis.

Option 2 conflicts with the current “token tidak masuk URL” invariant and cannot be assumed. Native transport behavior also requires PoC. Until resolved, SignalR authentication implementation is a security gate.

---

## 6. Worker/IPC threats

### 6.1 Local Worker process

- Run as the current intended user or a stricter approved local identity; never require Administrator by default.
- Worker receives only job-scoped operation, canonical Feature ID, typed parameters, approved input/artifact reference, deadline, resource budget, and Correlation ID.
- No arbitrary SQLite, shell, network, whole-filesystem, registry, environment-secret, or general application-service access.
- Working/temp directory is unique per profile/job with restrictive ACL, randomized internal name, bounded quota, and app-owned manifest.
- Canonicalize path inside the owned root; reject reparse/symlink/UNC/device/ADS escape. Verify source identity/fingerprint again before use to reduce TOCTOU.
- Network is denied by default. An engine needing approved egress receives only the specific destination/purpose policy.
- Apply CPU, memory, child-process, handle, thread, output-size, and wall-clock limits using approved OS/runtime containment concepts.
- Cancellation and hard timeout terminate/recycle the Worker safely; partial output never becomes final.
- Worker crash/OOM/parser failure cannot terminate Desktop UI. Source file remains unchanged.

### 6.2 Named Pipe/local IPC

- Pipe is local-machine only and created by the trusted parent/server side with current-user/app identity ACL. Pipe name is not a secret.
- Prefer current-user-only/package/process binding where supported; deny other logon sessions and remote clients.
- Use one first-instance/instance-bound server and authenticated versioned handshake before commands.
- Exact protection mechanism must be PoC-tested; parent PID or a guess-resistant pipe name alone is insufficient authentication.
- Frame every message with contract/version/type/length/request ID/Correlation ID; enforce hard size/depth/count/deadline.
- Operation allowlist is closed. Unknown operation/version/field fails closed.
- Bind request to launched Worker instance and expected job; stale Worker cannot complete a new job.
- Progress/error returned by Worker is untrusted and normalized before UI/log/event.
- Limit connections/messages/heartbeat failures; disconnect/recycle on malformed/flooding client.

Same-user malware may operate with the same OS rights and can be difficult to distinguish from the legitimate app. ACL alone does not fully remove this residual risk; app/package identity, handle inheritance, instance binding, code integrity, and endpoint protection assumptions require PoC and formal acceptance.

### 6.3 Server Worker

- Separate short-lived/scoped service identity; no forwarded end-user token.
- Queue access, object access, database access, provider access, and secret access are independently least-privileged.
- Worker claims a bounded job batch using lease/state/version and validates job/owner/policy/reference before use.
- A queue message is a locator, not authority. Worker reloads authoritative persisted job state.
- No DB transaction/connection remains open during engine/provider/file work.
- Engine output is scanned/validated, size-bounded, and matched to expected result contract before commit.
- High-risk engine crash triggers job failure/retry policy and Worker recycle; it does not poison other queued jobs.
- Per-engine/provider concurrency pools prevent one converter/OCR/AI workload starving all workers.

### 6.4 Correlation and diagnostics

Correlation ID follows API → job → Worker → engine/provider → storage → safe telemetry, but is never a credential. Diagnostics include safe codes, durations, resource counters, and engine profile code only. They exclude content, full path, password, token, prompt, OCR text, signature, raw provider response, and raw parser exception returned to client.

---

## 7. Document-engine threats

### 7.1 Common engine containment

Every PDF/Office/image/OCR/HTML input remains untrusted after extension/MIME/signature validation. Required pipeline:

```text
bounded preflight
→ streamed quarantine/staging
→ MIME/signature/structure/decompression/page/pixel checks
→ malware/active-content policy
→ low-privilege isolated processing
→ output type/structure/size/security validation
→ safe result commit
→ bounded cleanup/recycle
```

No engine may:

- silently overwrite source;
- inherit broad filesystem/network/service credentials;
- execute macro, embedded executable, document script, launch action, or shell command;
- resolve local/UNC/network link without explicit feature policy;
- return raw exception/content into API, SignalR, issue diagnostics, or ordinary logs.

### 7.2 Format-specific threats and controls

| Engine/format | Threats | Required controls |
|---|---|---|
| PDF | malformed xref/object graph, embedded JS/launch/action/file, fonts/images, recursive stream, password abuse | bounded parser; active actions disabled/removed per feature; embedded object policy; password ephemeral; isolated process; validate output |
| Office/converter | macro/VBA, OLE/embedded executable, external template/link/DDE-style behavior, formula/data connection, font/image parser | macro/active-content execution disabled; no user profile/interactive desktop; external links off; read-only source; isolated service identity |
| Image/OCR | pixel/dimension bomb, malformed codec/EXIF/ICC/font, CPU-heavy OCR, adversarial text | pixel/page/resource caps; codec isolation; timeout; OCR output DATA-only; no automatic command/path/search/tool use |
| Archive/container | zip slip, symlink, duplicate/overlap, nested bomb, huge entry count | safe extraction or no extraction; canonical root; entry/depth/ratio/total limits; reject link/device entries |
| HTML-to-PDF | script, local file, internal URL, CSS import/font/image, iframe, download, infinite render | isolated renderer; script/active content off by default; block local/custom schemes; controlled remote policy; time/resource limit |
| Web Page-to-PDF | SSRF, DNS rebinding, redirect to internal, ambient cookie/credential, changing remote content | dedicated typed URL/fetch contract; approved egress; revalidate DNS/redirect; credential-less fetch; provenance; explicit consent/policy |
| PDF-to-Markdown | raw HTML/script/link, hidden prompt, data URI, oversized text | deterministic extraction first; escape/sanitize; raw HTML off; links inert/no auto-fetch; preview as untrusted text |
| Redaction | visual overlay only, hidden text/object remains | engine PoC verifies physical removal/flattening, metadata/attachment cleanup, output validation, final confirmation |
| Unlock PDF | password capture/logging, protection bypass | valid password/authority only; memory-only ephemeral secret; no bypass; audit safe metadata; throttle attempts |

### 7.3 Output safety

- Output extension, MIME, signature, structure, size, page/item count, and expected feature contract are revalidated.
- Download uses safe `Content-Type`, `Content-Disposition`, `nosniff`, private cache rules, and no active inline render for risky formats.
- Markdown, OCR, translation, summary, issue text, and filenames are encoded at every display/export sink.
- PDF/Excel issue exports must not introduce active scripts, macros, external formulas, or formula injection.
- Result success is declared only from validated actual engine output.

---

## 8. AI/Assistant threats

### 8.1 Mandatory control flow

```text
User intent
→ minimum permitted context
→ document/OCR content labeled DATA + provenance
→ canonical Feature ID resolution
→ typed parameter/workflow validation
→ input compatibility and ownership
→ permission check
→ policy/classification/consent check
→ capability/resource/provider check
→ ambiguity clarification
→ risk confirmation when required
→ official application service
→ engine/provider resolver
→ actual job/result
→ grounded, encoded response
```

AI output cannot skip or modify any step. Policy, permission, capability, confirmation, and system instruction remain outside model control.

### 8.2 Prompt-injection defense

- Separate trusted instructions, user intent, application metadata, and document/OCR content into distinct typed/provenance channels.
- The prompt/template explicitly states that document text, OCR, metadata, comments, hidden text, HTML, and translated text are data, never instructions.
- Content Intelligence transform calls do not expose application tools. A summarization/translation model cannot execute a Feature.
- Assistant planning may propose only visible canonical Feature IDs. Proposed plan is still untrusted until deterministic validation.
- Ignore instructions within documents to reveal secrets, change policy, call tools, browse URLs, upload content, or mark a task successful.
- Bound context length, number of documents/pages, retrieved chunks, output length, and workflow steps.
- Do not insert secrets, tokens, storage keys, unrestricted filesystem paths, other-user metadata, full issue diagnostics, or unnecessary document pages into model context.

### 8.3 Tool and command safety

- No arbitrary shell, SQL, filesystem, HTTP/API, engine/provider selection, dynamic code, or plugin execution.
- Feature ID must be canonical, visible, active, registered, server/local-capable, and allowed for caller/platform.
- Parameter schema is closed and versioned. Unknown/extra parameter fails.
- Each workflow step rechecks permission, policy, input/output compatibility, resource budget, and confirmation.
- Destructive/irreversible/upload/share/permanent action never runs solely because AI inferred it. Ambiguous target or parameter causes clarification.
- AI-generated filename, URL, page range, path, language, or option is normalized and validated as ordinary untrusted input.
- Result response is grounded in persisted Job/result state. The model cannot assert success from its own prediction.

### 8.4 Content Intelligence provider boundary

- Approved provider endpoint/profile is selected from configuration; model/client/document cannot choose it.
- Public AI/translation endpoint is technically unreachable from application/provider egress policy.
- Before transfer: ownership, permission, classification, consent/notice, data-minimization, page/range selection, provider capacity, and purpose are checked.
- Service identity is provider/audience/purpose scoped. No end-user token is forwarded.
- Provider request/response content is not stored in ordinary SQL, idempotency snapshot, audit, telemetry, Assistant metadata, or raw logs.
- Temporary protected content/result follows explicit lease, encryption, access, expiry, cleanup, and orphan reconciliation.
- Provider policy must address retention, training/reuse, operator access, location, incident response, deletion, and subprocessor/dependency governance before approval.
- Provider response is untrusted and size/schema/encoding validated. It cannot cause auto-fetch or tool invocation.

### 8.5 Feature-specific AI controls

| Feature | Control |
|---|---|
| AI Summarizer `FEAT-INTEL-001` | minimum selected pages/text; no tools in transform; factuality/provenance notice; never replace source; output untrusted |
| Translate PDF `FEAT-INTEL-002` | language and selected content explicit; layout/fidelity limitations visible; no public endpoint; output reviewed before final export |
| PDF to Markdown `FEAT-CONV-036` | deterministic extraction first; OCR only when needed; AI not default; raw HTML/script disabled; links do not auto-fetch |
| Assistant workflow `FEAT-ASSIST-*` | closed canonical plan, bounded steps, per-step controls, clarification/confirmation, actual-result grounding |

---

## 9. Abuse/DoS threats

### 9.1 Layered controls

Effective limit remains the strictest of system safety, enterprise policy, subject scope, endpoint/workload, environment, Worker, storage/database, and provider capacity.

| Workload | Controls |
|---|---|
| Auth/session | attempt/burst limits, session/revocation checks, no verbose failure reason |
| API metadata/query | page/date/filter/search limits, timeout, index-aligned query, connection budget |
| Upload/download | simultaneous streams, bytes/file/batch/chunk, temp quota, range request budget, slow-client timeout |
| Feature/job | weighted cost, per-user/category/global concurrent jobs, atomic reservation, bounded queue |
| Retry/resume/offline backlog | same idempotency identity, bounded attempts, backoff+jitter, gradual backlog release |
| SignalR | connect/negotiate/reconnect/subscription/message/outbound-buffer limits |
| Engine/OCR/conversion | per-engine pool, CPU/RAM/time/temp/output cap, cancellation, recycle |
| AI/translation | context/output/token/time/call/concurrency quota, provider circuit/capacity, no public fallback |
| Export/support | date/result/attachment/output limit, background job, artifact expiry |
| Cleanup/archive | pressure-aware low-priority bounded batches; pauses/reduces under user workload |

### 9.2 Backpressure and degradation

- `429` for caller rate/quota/concurrency with `Retry-After`.
- `413` for hard request/file/batch limit.
- `409` for state/storage conflict.
- `503` for shared unsafe saturation with retry guidance where safe.
- Queue is bounded, persistent, fair, and workload-separated.
- Admission fails before DB pool, Worker memory, temp disk, or provider is exhausted.
- Local safe capability remains available during server/provider pressure.
- Internal AI outage returns deterministic/local fallback or safe unavailable state; never public AI.
- Slow SignalR client reconciles via REST rather than forcing unbounded server buffering.
- Feature is delayed/queued/degraded safely, not removed because server awal kecil.

---

## 10. Mitigations

### 10.1 Control catalog

| ID | Required mitigation | Applies to |
|---|---|---|
| `APPSEC-01` | Pertamina authn + deny-by-default object/permission/policy authz at application boundary | API, SignalR, jobs, storage, support, admin |
| `APPSEC-02` | Closed typed/versioned contracts; no arbitrary dispatch/deserialization | API, workflow, IPC, events, AI tool plan |
| `APPSEC-03` | Opaque refs plus purpose/state/owner/version validation; no physical path/storage key authority | file/job/result/secured/attachment |
| `APPSEC-04` | Contextual encoding, CSP, safe Markdown/export, CSRF/origin/CORS controls | Web/support/AI/OCR output |
| `APPSEC-05` | Governed reverse-proxy parsing, forwarded-header trust, header/body/time limits | HTTP edge/API |
| `APPSEC-06` | Streaming validation, quarantine, decompression/page/pixel/active-content limits | uploads/document engines |
| `APPSEC-07` | Least-privilege Worker/engine containment, resource limits, crash isolation, recycle | Local/Server Worker/engines |
| `APPSEC-08` | Controlled remote fetch/egress; SSRF and credential-leak prevention | HTML/Web-to-PDF/providers |
| `APPSEC-09` | Named Pipe ACL/identity/instance binding, handshake, operation/message limits | Desktop↔Local Worker |
| `APPSEC-10` | Rowversion/lease/idempotency/atomic reservation and bounded retry | job/upload/version/issue/admin |
| `APPSEC-11` | Notification-only SignalR, server-derived groups, revision/sequence/REST recovery | Activity Hub |
| `APPSEC-12` | AI instruction/data separation, canonical tool validation, confirmation, result grounding | Assistant/Content Intelligence |
| `APPSEC-13` | Data minimization, classification, explicit transfer/consent, temporary content lifecycle | upload/AI/OCR/support |
| `APPSEC-14` | Safe error and diagnostic allowlist; forbidden content redaction | API/Worker/engine/health/issue |
| `APPSEC-15` | Layered rate/quota/concurrency, bounded fair queues, graceful degradation | API/Worker/provider/storage/DB |
| `APPSEC-16` | Dependency provenance, locked versions, SBOM, scanning, signed/governed artifacts | build/runtime/engine/model |
| `APPSEC-17` | Secure local profile boundary including SQLite/WAL/SHM/temp/crash dump/backup exclusion | Desktop/Mobile local data |
| `APPSEC-18` | Encryption/key/credential lifecycle via approved stores; no raw key beside data | server/local/storage/services |
| `APPSEC-19` | Idempotent tombstone/cleanup/orphan reconciliation and retention/legal hold | uploads/jobs/secured/issues/exports |
| `APPSEC-20` | Security telemetry using safe codes/counters/correlation plus alert/runbook | all deployables |

### 10.2 Implementation priorities

Before any high-risk parser/provider feature can leave PoC:

1. authn/authz and object ownership negative tests pass;
2. typed contract and arbitrary-dispatch rejection pass;
3. Worker/engine isolation and resource limit are demonstrated;
4. controlled egress and public-AI prohibition are demonstrated;
5. content/log/error redaction is demonstrated;
6. malicious file/prompt/provider-response tests pass;
7. bounded admission/backpressure and cleanup pass;
8. supply-chain artifacts and approved dependency evidence exist;
9. incident-safe correlation and recovery behavior exist.

---

## 11. Required security tests

### 11.1 Automated baseline

- SAST/security analyzer, secret scanning, dependency vulnerability/license review, SBOM generation, and artifact integrity verification.
- OpenAPI/contract negative tests for unknown fields/types/versions, duplicate properties, oversized/deep JSON, unsafe examples, and forbidden output fields.
- Authorization matrix tests for every route/method/object/action, including cross-user, owner change, permission revoke, role downgrade, offline reconnect, cursor continuation, range download, export, and SignalR subscription.
- SQLi/command/path/CRLF/XSS/CSV-formula/unsafe-URL injection corpus.
- CSRF, CORS, Origin, CSP, cookie, open redirect, Host/forwarded-header, and cache/header tests.
- Request-smuggling tests through Production-equivalent proxy/API chain.
- Rate/quota/concurrency/idempotency/replay/race tests.

### 11.2 File and engine tests

- Curated malicious corpus: corrupt/truncated/polyglot PDF/Office/image/archive/HTML; macro/OLE/external links; embedded executable; JS/actions; font/image/ICC/EXIF; nested/decompression bomb; page/pixel bomb.
- Coverage-guided/parser fuzzing where technically supported; mutation fuzz for every accepted format.
- Source immutability, output validation, quarantine, partial cleanup, timeout/cancel, crash/OOM, Worker recycle, and recovery.
- Filesystem containment tests for traversal, symlink/reparse, UNC/device/ADS, file replacement between validation/use, and cross-job temp access.
- Network containment tests proving engines cannot reach arbitrary local/internal/public destinations.
- Redaction verification proving removed content cannot be recovered from text layer, object, metadata, attachment, incremental revision, or copy/paste.
- Unlock tests proving only valid password/authority works and password never persists/logs.

### 11.3 Worker/IPC tests

- Different OS user, different session, same-user rogue process, stale Worker, remote pipe, guessed pipe, malformed handshake, unsupported version, oversized/partial/flooded message.
- Worker cannot invoke unregistered operation, arbitrary process, arbitrary path, SQLite, SQL, HTTP, or provider.
- Job/instance/lease mismatch cannot return success.
- CPU/RAM/time/temp/child-process limits terminate safely and preserve UI/API.
- Correlation works without content/secret leakage.

### 11.4 SignalR tests

- Invalid/expired/revoked credential, hostile Origin, cross-user/group/job subscription, connection/subscription flood, slow consumer.
- Duplicate, stale, reordered, gap, unknown type/version, terminal regression, reconnect, REST reconciliation.
- Hub has no business command method and cannot trigger execute/cancel/retry/upload.
- Verify chosen credential transport never leaks token to URL, proxy logs, app logs, telemetry, browser history, diagnostics, or referrer.

### 11.5 AI/Assistant tests

- Direct and indirect prompt-injection/jailbreak corpus in Indonesian and English.
- Malicious instructions in PDF text, OCR, metadata, annotations, white/hidden text, HTML, Markdown, translated content, and filenames.
- Invented/hidden/deprecated Feature ID, extra parameter, wrong schema version, unauthorized step, cyclic/oversized workflow, capability mismatch, confirmation bypass.
- Ambiguous destructive prompts must clarify; no action on low-confidence unsafe plan.
- Model/provider cannot access SQL, shell, filesystem, arbitrary HTTP, public AI, other-user data, secret, or unrestricted issue diagnostics.
- Data-minimization test verifies only authorized selected pages/fields leave the app boundary.
- Malicious provider response XSS/Markdown/URL/tool JSON/oversize/control-character handling.
- Grounding test: AI cannot report success before validated actual job/result state.
- AI/OCR/translation content absent from ordinary SQL, logs, telemetry, audit, idempotency, SignalR, and issue auto context.

### 11.6 Assurance and operational tests

- VA and penetration test before Production with authenticated and unauthenticated scopes.
- Threat-model abuse cases included in Test Plan and regression suite.
- Local SQLite/artifact encryption boundary covers DB, WAL, SHM, temp, migration backup, crash dump, diagnostics, and OS backup exclusion.
- Service identities and Worker permissions reviewed against actual filesystem/network/DB/object/provider access.
- Incident drill for parser exploit, compromised provider/dependency, token leakage, job flood, object exposure, and AI content egress.
- Security tests use synthetic/masked data and do not place Production secrets/documents in test logs or artifacts.

---

## 12. High-risk features

| Feature/surface | Risk | Main reason | Release gate |
|---|---|---|---|
| Web Page-to-PDF `FEAT-CONV-018` | Critical | SSRF, DNS/redirect, credential/local-file access, active content | typed remote input + egress/SSRF/render isolation PoC; API contract gap resolved |
| HTML-to-PDF `FEAT-CONV-017` | Critical/High | script/local-resource/remote-resource access, renderer exploit | active content policy, isolated renderer, remote-resource policy, fuzz/resource tests |
| Office↔PDF/PDF→Office `FEAT-CONV-001..006` | High | macro/OLE/external link/native parser/fidelity | no active execution, isolation, malicious corpus, output validation, dependency approval |
| OCR PDF `FEAT-OCR-004` and complex OCR | High | codec/parser exploit, pixel/page bomb, adversarial OCR text | resource cap, isolated engine, OCR-as-DATA tests, provider/engine PoC |
| PDF/general document parser features | High | malformed object/font/image/embedded content exploit | parser fuzz, low privilege, no network, crash containment, patch/SBOM |
| AI Summarizer `FEAT-INTEL-001` | High | restricted-content transfer, prompt injection, hallucination/provider risk | provider approval, minimization/consent, no tools, red-team, egress proof |
| Translate PDF `FEAT-INTEL-002` | High | high content disclosure, external/internal provider, fidelity | same AI gate + language/data scope + review before export |
| Assistant multi-step `FEAT-ASSIST-*` | High | tool hijack, permission/confirmation bypass, cumulative resource | closed plan, per-step checks, clarification/confirmation, resource preflight, grounding |
| PDF-to-Markdown `FEAT-CONV-036` | Medium/High | raw HTML/links/XSS/indirect prompt | deterministic first, safe renderer, no auto-fetch/raw HTML, OCR data controls |
| Redact PDF `FEAT-PDF-069` | High | apparent redaction but recoverable content | irreversible removal verification, final confirmation, forensic output tests |
| Unlock PDF `FEAT-PDF-062` | High | password handling/protection bypass/resource abuse | valid authority only, ephemeral password, no bypass, attempt limits, audit metadata |
| Issue screenshot/support/export `FEAT-SUPPORT-002..008` | High | sensitive display, stored XSS/formula injection, privileged support access | explicit preview/send, separate permissions, safe diagnostics/export, retention |
| Upload/secured-file/result download | High | restricted content, BOLA, parser input, lifecycle race | ownership on every operation, streaming/validation, ETag/state, encryption/retention |
| Named Pipe + Local Worker | High | same-device privilege/tool execution boundary | IPC identity/ACL/handshake PoC, operation allowlist, process/resource containment |
| SignalR Activity Hub | Medium/High | cross-user event leakage, credential URL issue, connection DoS | credential transport decision, Origin/group/subscription/replay/load tests |

Risk level does not remove any feature. It determines PoC, release gate, isolation, limits, and evidence.

---

## 13. PoC security requirements

### 13.1 Document engine/converter PoC

Evidence required:

```text
[ ] runs without Administrator/root privilege
[ ] cannot read/write outside job-scoped approved locations
[ ] cannot reach network unless explicit feature policy allows exact destinations
[ ] macro/script/launch/external-link behavior is disabled or safely controlled
[ ] CPU/RAM/time/temp/output/child-process limits work
[ ] crash/OOM/hang is contained and Worker recycles
[ ] malicious corpus/fuzz failures are controlled
[ ] source remains immutable; output is structurally/security validated
[ ] logs/errors contain no content/path/password/secret
[ ] package/model/native dependencies have provenance, license, SBOM, and vulnerability evidence
```

### 13.2 HTML/Web-to-PDF PoC

Must demonstrate:

- `file:`, UNC, custom schemes, loopback, link-local, metadata/internal targets, encoded IP, redirect-to-internal, DNS rebinding, iframe/import/font/image fetch, download, WebSocket, and ambient credentials are blocked unless explicitly approved;
- JavaScript/active content default and exception policy are documented;
- nested resource, redirect, byte, time, CPU/RAM, page/output limits work;
- remote content provenance and user consent are safe/minimized;
- no remote URL can be supplied by AI/model/provider outside typed feature validation;
- API remote-input contract conflict is resolved before implementation.

### 13.3 Named Pipe/Worker PoC

- prove cross-user/remote denial;
- exercise same-user rogue-client scenario and document remaining limitations;
- demonstrate Worker instance binding, version handshake, allowed operations, message limits, timeouts, crash containment, recycle, and job-scoped filesystem access;
- prove Worker cannot access arbitrary SQLite/SQL/network/provider or inherit UI secrets;
- verify source-replacement/reparse race defenses.

### 13.4 SignalR PoC

- select credential transport consistent with “no token in URL,” or obtain controlled baseline revision;
- validate browser Origin, session revocation, server-derived groups, per-job authorization, payload prohibition, replay/order/gap, slow consumer, reconnect, and REST recovery;
- prove proxy/app/telemetry logs do not capture credential.

### 13.5 AI/Content Intelligence PoC

```text
[ ] provider is approved local/enterprise-internal; public route technically blocked
[ ] data flow, classification, consent, minimization, retention/training/reuse/location are approved
[ ] prompt/document channels separated; indirect injection corpus tested
[ ] pure content transform has no tools
[ ] Assistant plan accepts canonical Feature IDs and closed schemas only
[ ] per-step permission/policy/capability/confirmation cannot be bypassed
[ ] malicious provider response is encoded/schema/size validated
[ ] actual result grounding enforced
[ ] context/output/call/time/concurrency budgets and provider outage behavior pass
[ ] no content in ordinary DB/log/telemetry/audit/SignalR/idempotency/issue diagnostics
```

### 13.6 Local data protection PoC

- approved protection covers SQLite DB, WAL, SHM, temp, migration backup, crash dump, diagnostics, and app-managed artifacts;
- key/token is not stored beside data and account switch cannot cross-open another profile;
- Mobile/Tablet automatic cloud/device backup exclusion is verified on supported OS versions;
- logout/lock/key destruction/retention/recovery behavior is tested;
- secure overwrite is not promised where storage cannot guarantee it.

Failure of a mandatory PoC gate results in `BLOCKED_ALTERNATIVE_REQUIRED`, `APPROVED_WITH_LIMITATION`, or other governed status—not silent risk acceptance and not feature deletion.

---

## 14. Residual risks

| Residual risk | Why it remains | Required ownership/acceptance |
|---|---|---|
| Parser/codec zero-day | validation cannot prove native parser safety | isolate/patch/monitor/kill/recycle; Security + Product acceptance before engine approval |
| Same-user local malware | may share OS rights and observe/interact beyond pipe ACL | endpoint security assumption, app/package identity PoC, minimize local secrets/privilege |
| Approved internal AI sees selected content | AI feature inherently processes content | classification, consent, minimization, provider governance, retention/training terms |
| AI/OCR/translation factual/fidelity error | model/recognition is probabilistic | preview, provenance/AI label, user review, no automatic authoritative replacement |
| Remote web content changes after validation | network content is mutable and adversarial | fetch-time policy, digest/provenance, no ambient credentials, bounded renderer |
| Offline permission staleness | revocation may be unknowable offline | narrow time-bounded local capability; sensitive/server action waits online |
| User sends sensitive screenshot intentionally | explicit action may still overshare | preview/warning/markup, restricted support access, retention; user education |
| Secure erase on flash/SSD | physical storage remapping prevents guaranteed overwrite | encryption/key lifecycle and policy; no false secure-delete claim |
| Redaction engine limitation | some formats/incremental objects may retain content | approved engine PoC, forensic tests, limitation disclosure/block unsafe finalization |
| Supply-chain unknown/zero-day | scanning is not complete assurance | approved sources, lock/provenance/SBOM, monitoring, emergency disable/update path |
| Extreme distributed DoS | finite infrastructure can be saturated | upstream/edge capacity, layered quotas, graceful degradation, incident response |
| Misconfiguration | origin/egress/permission/limit policy can weaken controls | typed safe bounds, change approval/audit, configuration tests, staged rollout |

Residual risks must be recorded with owner, affected feature/environment, compensating control, review date, evidence, and acceptance authority. “Internal system” is not an acceptance reason.

---

## 15. Conflicts

### 15.1 Material conflicts/gaps

| No | Finding | Severity | Recommendation | Owner |
|---:|---|---|---|---|
| 1 | `FEAT-CONV-018` Web Page-to-PDF requires URL input/fetch, while API Final section 11.2 says client cannot send arbitrary URL and `fileRef` kinds do not include remote resource | High — contract gap | add a controlled typed remote-resource preflight/reference or explicitly define safe URL parameter semantics with SSRF policy through controlled API revision/addendum; never pass raw URL to generic engine/provider | User + API Integrator + Security |
| 2 | API says token never enters URL, but browser SignalR/WebSocket bearer flows commonly use query-string credentials | High — implementation conflict | prefer secure same-site server session cookie for browser hub; otherwise controlled revision for short-lived scoped hub credential with strict redaction. Do not silently use query token | User + API Integrator + Security |
| 3 | Web auth says server-side session boundary “bila sesuai arsitektur”; final cookie/BFF versus bearer behavior is not locked | Medium/High — control ambiguity | Document 08 must lock Web session, CSRF, SameSite, Origin, logout/revocation, and hub authentication behavior | Security + Web owner |
| 4 | Uploaded HTML may contain remote CSS/image/font/iframe even without Web Page-to-PDF URL input | High — policy gap | define remote-resource default deny/allow and renderer egress controls for both HTML-to-PDF and Web-to-PDF | Security + Conversion owner |
| 5 | Local Worker IPC is typed/versioned, but exact ACL/client identity/instance binding is not locked | High — handoff gap | mandatory IPC PoC and release gate; do not treat pipe name/current user alone as complete defense | Security + Desktop owner |
| 6 | Database Final requires SQLite/WAL/SHM/artifact protection and OS backup exclusion, but provider/key/logout behavior is deferred | High — release dependency | select only after platform/security/dependency review; unsupported backup exclusion is a platform release blocker per Document 07 | Security + Desktop/Mobile owner |
| 7 | Approved internal AI policy lacks final retention/training/location/operator/subprocessor terms and technical egress profile | High — governance gap | provider approval checklist and PoC before AI Summarizer/Translate release; public AI remains impossible | Security + Data owner + AI owner |
| 8 | Content Intelligence may require restricted content while SQL metadata prohibits content; temporary object lifecycle is defined but exact encryption/TTL/key/access values are deferred | Medium/High — parameter gap | Document 08 defines content handling class, temporary access/lease, encryption/key boundary, deletion verification, and minimum context | Security + Storage owner |
| 9 | Request smuggling, Host/forwarded-header trust, WebSocket Origin, CSP, CSRF, and CORS are not detailed in Documents 01–07 | Medium — expected Security Design handoff | add normative controls and Production-path tests in Document 08; no architecture change required | Security + Deployment owner |
| 10 | Working data classes `I/C/R/X/A` are not official Pertamina classification | High — governance handoff | map to approved Pertamina classification/handling matrix before Production; do not let working labels create policy | User + Security/Data Governance |

### 15.2 No contradiction found

- REST authority and SignalR event-only model are security-compatible.
- Closed Feature dispatch and Assistant/tool boundary are consistent.
- Document/OCR/AI output untrusted baseline is consistent across 01–07.
- SQL metadata versus protected object content is consistent.
- Administrator/Support no-implicit-document-access rule is consistent.
- Resource limits and graceful degradation preserve features without weakening architecture.

---

## 16. Suggested Document 08 text

Teks berikut siap dipertimbangkan Integrator. Statusnya **PROPOSED**, bukan LOCKED.

### Application, API, Worker, Document Engine, and AI Security Baseline

PERTAMINA OTO treats every client field, identifier, file, archive, PDF/Office/image/HTML structure, OCR/extracted text, issue content, model input, model output, provider response, IPC message, queue message, and SignalR event as untrusted until validated at the boundary that consumes it.

#### Trust and authorization

```text
Client
→ Pertamina Identity authentication
→ API/application permission + ownership + policy
→ typed canonical application service
→ job/engine/provider under least privilege
```

Opaque IDs, cursor, Feature ID, Job ID, Correlation ID, `fileRef`, ETag, pipe name, queue membership, internal network location, and SignalR group never grant authority. Authorization is deny-by-default and repeated for every list/detail/continuation/download/range/retry/resume/export/subscription and before delayed sensitive execution/commit.

#### Web and API

- Web output uses contextual encoding. Raw HTML is disabled by default. Markdown/OCR/AI/issue content is rendered through a safe inert renderer.
- Cookie-authenticated mutations require anti-forgery, secure SameSite/HttpOnly cookies, Origin validation, and no state-changing GET. Sensitive actions require current authorization and reauthentication when policy requires.
- CORS is disabled unless required; approved exact origins only. WebSocket Origin is validated separately.
- Reverse proxy/API reject ambiguous HTTP framing, untrusted forwarded headers, invalid Host, excessive headers/body, deep/unknown JSON, and unsupported contract versions.
- Feature execution is closed dispatch: path Feature ID, registered handler, typed schema, owned available input, permission, policy, classification, capability, consent, capacity, and output validation are mandatory. Client/model cannot select engine/provider/class/SQL/shell/path/endpoint.
- Upload is streamed and bounded. Validation covers bytes, parts, MIME/signature, structure, page/pixel/object count, active content, decompression, malware policy, quarantine, and temp capacity. Only `READY` input executes.
- Errors expose stable safe code/message/correlation and typed safe details only; never stack, SQL, path, storage key, token, password, document/OCR/prompt, provider detail, queue depth, or topology.

#### SignalR

SignalR is an authenticated notification-only channel. Server derives user groups; optional job subscriptions recheck ownership/permission and are bounded. Events contain safe codes/metadata only, use event ID/version/per-stream sequence, ignore duplicate/stale data, reconcile gaps by REST, and never regress terminal persisted state. Hub has no execute/upload/cancel/retry/admin command. Credential transport must comply with the no-token-in-URL invariant; implementation cannot proceed until the browser/native pattern is security-approved.

#### Worker and IPC

Local and Server Workers use least-privilege identities, job-scoped file/object references, isolated working/temp directories, closed operation allowlists, typed/versioned contracts, bounded message/input/output, CPU/RAM/time/temp/child-process limits, cancellation, crash containment, and recycle. Worker/engine has no arbitrary SQL, shell, filesystem, network, provider, or user token. Named Pipe is local, ACL- and identity-bound, instance-bound, version-handshaken, size/time limited, and PoC-tested against cross-user, same-user rogue, stale, remote, and flooding clients.

#### Document engines and remote content

All document engines run behind adapters in isolated Worker boundaries. Macro, embedded executable, document script/launch action, external template/link, local-file access, and unrestricted network are disabled unless a specific approved feature policy requires a controlled behavior. Path traversal, archive/decompression, page/pixel, parser, and output-validation controls are mandatory.

HTML/Web-to-PDF uses a dedicated typed remote-resource policy. It blocks local/custom schemes, loopback/link-local/metadata/internal targets, ambient credentials, uncontrolled redirects, DNS rebinding, downloads, WebSocket, and unbounded nested resources. Every DNS result and redirect is revalidated; bytes/time/render/concurrency are bounded. The API contract for remote URL input must be resolved through controlled revision/addendum before `FEAT-CONV-018` implementation.

#### Assistant and Content Intelligence

```text
minimum context
→ content labeled DATA
→ canonical Feature ID
→ typed parameters/workflow
→ ownership + permission + policy + capability + consent
→ clarification/confirmation
→ official application service
→ actual result grounding
```

Assistant/AI cannot execute arbitrary shell, SQL, filesystem, HTTP/API, code, engine/provider, or hidden tool. Document/OCR/HTML/metadata content cannot alter policy or become an instruction. Pure summarization/translation calls expose no tools. AI-generated plans and responses are untrusted and bounded; plans are deterministically validated per step and responses are encoded/sanitized. Destructive, irreversible, upload, share, and permanent actions cannot bypass clarification/confirmation.

Only approved local or enterprise-internal providers are allowed. Public AI/translation endpoints are technically prohibited. Content transfer requires ownership, permission, classification, consent/notice, page/field minimization, purpose-scoped service identity, temporary encrypted lifecycle, and approved provider retention/training/location/access terms. Provider request/response content is excluded from ordinary SQL, idempotency, audit, telemetry, SignalR, Assistant metadata, and raw logs.

AI Summarizer and Translate PDF output is labeled, previewed, and never replaces the source automatically. PDF-to-Markdown uses deterministic extraction first, disables raw HTML/auto-fetch, and treats OCR/Markdown as untrusted display data.

#### Abuse and resilience

Rate, quota, concurrency, request size, query cost, upload/temp/storage, Worker/engine/provider, SignalR, and export controls use the strictest effective safety bound. Queues are persistent, bounded, fair, and workload-separated. Retry/reconnect/offline backlog uses idempotency, bounded backoff, and jitter. `429`, `413`, `409`, and `503` follow the locked API contract. Pressure preserves safe local capability and never falls back to public AI.

#### Security release gates

Before Production or high-risk feature release:

```text
[ ] object/permission/ownership negative tests pass
[ ] API/browser/proxy/SignalR threat tests pass
[ ] malicious document corpus and parser fuzz/crash containment pass
[ ] Named Pipe/Worker identity, containment, and resource PoC pass
[ ] HTML/Web SSRF and renderer isolation PoC pass
[ ] AI direct/indirect prompt-injection and data-egress PoC pass
[ ] no restricted content/credential in log/error/event/diagnostics/persistence
[ ] dependency provenance, SBOM, scanning, VA, and PenTest evidence pass
[ ] bounded load/DoS/backpressure/recovery tests pass
[ ] residual risks have named owner and formal acceptance
```

High-risk or blocked PoC does not remove a feature. It produces a governed limitation, alternative, phase, or approval decision.
