# 18 APPLICATION CARTOGRAPHY
## PERTAMINA OTO — Office Tools Organizer

> Versi: FINAL v1.0
> Tanggal: 1 September 2026
> Status: FINAL — PRE-IMPLEMENTATION APPLICATION CARTOGRAPHY BASELINE
> Standar bahasa: Bahasa Indonesia — ID-1.0
> Baseline: 550 Feature ID canonical; 33 Primary PDF Tool Cards
> Sumber: Dokumen 01–17 FINAL dan MASTER_PROJECT_SUMMARY v4.0

Dokumen ini mengunci peta konseptual dan governance cartography. Status FINAL bukan bukti implementasi, pengujian aktual, UAT sign-off, deployment, Production readiness, CMDB registration, atau penyerahan operasional.

---

## 1. Informasi dokumen

| Atribut | Nilai |
|---|---|
| Document ID | DOC-18 |
| Nama | Application Cartography |
| Pemilik dokumen | TBD — REQUIRES_PERTAMINA_OWNER_CONFIRMATION |
| Lifecycle | DESIGNED |
| Readiness | FINAL sebagai baseline pra-implementasi |
| Application ID | REQUIRES_PERTAMINA_APPLICATION_ID_CONFIRMATION |
| CMDB ID | REQUIRES_PERTAMINA_CMDB_CONFIRMATION |

## 2. Tujuan

Menjawab apa yang membentuk PERTAMINA OTO, di mana setiap bagian berjalan, data dan interface yang dipakai, dependency, trust boundary, pemilik fungsi, serta hubungan ke requirement, Feature, test, manual, dan runbook.

## 3. Scope

Mencakup context, application, host, module, component, Feature, interface, data flow, data store, deployment, security zone, ownership, lifecycle, test, runbook, dan traceability. Dokumen ini tidak mengganti Dokumen 06, 07, 08, 17, atau membuat Handover Checklist Dokumen 19.

## 4. Target pembaca

Developer, Codex, architect, administrator, security, auditor, tester, deployment/operations, support, maintainer, dan penerima handover.

## 5. Definisi Application Cartography

Application Cartography adalah peta terkendali yang menghubungkan capability bisnis hingga komponen operasional:

~~~text
Business Capability → Requirement → Feature → UI Tool Card
→ Module → Component → Interface → Data Store
→ Deployable → Environment → Security Boundary
→ Owner → Test → Runbook
~~~

## 6. Prinsip cartography

1. Satu sumber berwenang untuk setiap fakta.
2. Feature ID tidak dibuat ulang.
3. UI card bukan service, endpoint, engine, atau tabel.
4. Local-First dan Offline-First dipertahankan.
5. REST adalah authority; SignalR hanya notifikasi.
6. Assistant hanya orchestrator.
7. Provider berada di belakang adapter/resolver.
8. Satu write owner per aggregate/data store.
9. Diagram dibagi per view dan tidak menjadi source of truth.
10. Gap resmi selalu terlihat; fakta Pertamina tidak dikarang.

## 7. Source of truth

| Fakta | Otoritas |
|---|---|
| Proses dan aturan bisnis | Dokumen 01–02 |
| Feature identity dan 33 Core | Dokumen 03 |
| Arsitektur dan module | Dokumen 04 |
| Teknologi | Dokumen 05 |
| API/OpenAPI/SignalR | Dokumen 06 |
| Data/schema/ownership | Dokumen 07 |
| Security | Dokumen 08 |
| Permission | Dokumen 09 |
| Audit | Dokumen 10 |
| Unit/Test/UAT | Dokumen 11–13 |
| User/Admin guidance | Dokumen 14–15 |
| Incident/runbook | Dokumen 16 |
| Deployment/artifact/environment | Dokumen 17 |

## 8. Assumptions

- Sistem masih pra-implementasi.
- Nama logical component tidak menyatakan class, process, atau microservice aktual.
- Platform exposure bersifat capability-aware dan belum berarti parity.
- Owner adalah fungsi TBD sampai penugasan resmi.
- Hostinger KVM 2 hanya kandidat temporary non-Production.

## 9. Confirmation gaps

Marker wajib:

- REQUIRES_PERTAMINA_APPLICATION_ID_CONFIRMATION
- REQUIRES_PERTAMINA_CMDB_CONFIRMATION
- REQUIRES_PERTAMINA_OWNER_CONFIRMATION
- REQUIRES_PERTAMINA_CARTOGRAPHY_TEMPLATE_CONFIRMATION
- REQUIRES_PERTAMINA_DATA_CLASSIFICATION_CONFIRMATION
- REQUIRES_PERTAMINA_CRITICALITY_CONFIRMATION
- REQUIRES_PERTAMINA_NETWORK_CONFIRMATION
- REQUIRES_PERTAMINA_INFRASTRUCTURE_CONFIRMATION

## 10. Identifier convention

| Prefix | Objek |
|---|---|
| CART-APP | Application |
| CART-ACT | Actor |
| CART-CAP | Business Capability |
| CART-HOST | Host/deployable |
| CART-MOD | Module |
| CART-CMP | Component |
| CART-INT | Interface |
| CART-FLOW | Data flow |
| CART-DATA | Data store/asset |
| CART-EXT | External dependency |
| CART-ENV | Environment |
| CART-ZONE | Conceptual trust zone |
| CART-OWN | Ownership assignment |

ID cartography tidak mengganti Feature ID, Requirement ID, API route, permission, schema, atau database key.

## 11. Legend

| Simbol/status | Arti |
|---|---|
| LOCAL | Proses dan data tetap pada device |
| HYBRID | Resolver memilih local/server yang disetujui |
| SERVER | Dependency server diperlukan |
| OPTIONAL | Dependency hanya untuk route/capability tertentu |
| SOT | Source of truth |
| TBD | Belum disahkan |
| POC_REQUIRED | Memerlukan feasibility evidence |
| PERTAMINA_CONFIRMATION_REQUIRED | Memerlukan otoritas resmi |

## 12. System Context

Diagram ID: CART-DGM-001; validasi: SOURCE_VALIDATED; owner: TBD.

~~~mermaid
flowchart LR
  U[User] --> OTO[PERTAMINA OTO]
  AD[Administrator] --> OTO
  AU[Auditor] --> OTO
  SP[Support] --> OTO
  OTO --> ID[Pertamina Identity/SSO]
  OTO --> ST[Approved Secure Storage]
  OTO --> PR[Approved Engine/Internal Provider]
  OTO --> MON[Monitoring/Telemetry]
  DEV[Developer/Maintainer] --> AZ[Azure DevOps]
  OPS[Operations/Deployment] --> AZ
  AZ --> OTO
~~~

External node adalah dependency konseptual. Nama produk, endpoint, zone, dan owner final belum ditetapkan.

## 13. Actor map

| ID | Actor/fungsi | Tujuan | Batas |
|---|---|---|---|
| CART-ACT-001 | User | Memproses file dan mengelola resource sendiri | Tidak mengelola user/policy atau resource user lain |
| CART-ACT-002 | Administrator | Mengelola akses, policy, capability, dan health | Tidak otomatis memperoleh akses content |
| CART-ACT-003 | Auditor | Query/export bukti sesuai scope | Read-only dan tidak mengubah Audit |
| CART-ACT-004 | Support/Superadmin profile | Triage Issue sesuai permission | Bukan unlimited role |
| CART-ACT-005 | Developer/Maintainer | Memelihara code/contract/test | Bukan actor content Production |
| CART-ACT-006 | Operations/Deployment | Menjaga artifact, deployment, health, recovery | Tidak direct-patch business data |

## 14. Business Capability map

| ID | Capability | Owner logical |
|---|---|---|
| CART-CAP-001 | Document Organization dan PDF Optimization | Pdf |
| CART-CAP-002 | Document Conversion | Conversion |
| CART-CAP-003 | PDF/Image Editing | Pdf / Image |
| CART-CAP-004 | PDF Security | Pdf / SignatureAnnotation |
| CART-CAP-005 | Scanner/OCR | ScannerOcr |
| CART-CAP-006 | Visual Signature/Annotation | SignatureAnnotation |
| CART-CAP-007 | Workspace/Recovery/Secured interaction | Workspace |
| CART-CAP-008 | Account/Administration/Configuration | AccountAdministration |
| CART-CAP-009 | Audit | Audit |
| CART-CAP-010 | Support/Issue | Support |
| CART-CAP-011 | Assistant Orchestration | Assistant |
| CART-CAP-012 | PDF Intelligence | ContentIntelligence / Conversion |

## 15. Application overview

PERTAMINA OTO adalah aplikasi internal multi-channel dengan WinUI 3 Desktop sebagai pengalaman utama, .NET MAUI untuk Phone/Tablet, Responsive Web untuk browser/admin, ASP.NET Core Web/API untuk resource server, Worker untuk background server, dan Local Worker terisolasi untuk pekerjaan lokal berat/berisiko.

## 16. Application catalog

| Field | Nilai |
|---|---|
| ApplicationCartographyId | CART-APP-001 |
| ApplicationName / ShortName | PERTAMINA OTO / OTO |
| Description | Office Tools Organizer internal |
| BusinessPurpose | Pengolahan office/PDF lokal-first yang aman dan dapat diaudit |
| PrimaryUsers / FormalRoles | User; Administrator; Auditor; Support profile |
| LifecycleStatus | DESIGNED |
| Criticality / DataClassification | TBD_OFFICIAL |
| ArchitectureStyle | Modular Monolith; Clean Architecture; Separate Deployables |
| TechnologyStack | C#/.NET; ASP.NET Core; WinUI 3; .NET MAUI; SQL Server; SQLite |
| Channels / Hosts | Desktop; Mobile/Tablet; Web; API; Worker; Local Worker |
| Repository / Pipeline | Azure DevOps — exact identifier TBD |
| Monitoring | OpenTelemetry + approved Application Insights path |
| CurrentVersion | Pre-implementation baseline |
| Owner fields | REQUIRES_PERTAMINA_OWNER_CONFIRMATION |

## 17. Logical architecture

~~~mermaid
flowchart TB
  UI[Web / Desktop / Mobile] --> APP[Module Application Services]
  APP --> DOM[Module Domain]
  APP --> BB[Contracts / Security / Observability / Storage / Processing Abstractions]
  BB --> INF[Infrastructure Adapters]
  INF --> ENG[Approved Engine / Provider]
  INF --> DATA[SQL / SQLite / Workspace / Secure Storage]
~~~

Business rules tidak ditempatkan di UI, provider, storage, atau Worker host.

## 18. Container/Host map

| ID | Host | Purpose | Processing/data | Zone |
|---|---|---|---|---|
| CART-HOST-001 | Desktop | UI utama dan local application | Local/hybrid; local files/SQLite | Client + Local Processing |
| CART-HOST-002 | Mobile/Tablet | Curated mobile capability | Local/hybrid curated | Client |
| CART-HOST-003 | Web | Responsive user/admin surface | Server resource UI | Enterprise API |
| CART-HOST-004 | API | REST authority, authz, orchestration | Server metadata/streams | Enterprise API |
| CART-HOST-005 | Server Worker | Background server work/export | Job-scoped data | Server Processing |
| CART-HOST-006 | Local Worker | Isolated heavy/risky local work | Job-scoped local refs | Local Processing |
| CART-HOST-007 | SQL Server | Server metadata/state | No file BLOB default | Data |
| CART-HOST-008 | SQLite | Per-profile local metadata/state | No document BLOB | Client Data |
| CART-HOST-009 | Secure Storage | Payload server/private | File/object bytes | Secure Storage |
| CART-HOST-010 | Monitoring | Safe telemetry | No business content/SOT | Observability |

## 19. Module map

| ID | Module | Canonical ownership |
|---|---|---:|
| CART-MOD-001 | Pdf | PDF 75 |
| CART-MOD-002 | Conversion | CONV 36 |
| CART-MOD-003 | Image | IMG 48 |
| CART-MOD-004 | ScannerOcr | SCAN 18 + OCR 13 |
| CART-MOD-005 | SignatureAnnotation | SIGN 13 + ANN 14 |
| CART-MOD-006 | FileProductivity | FILE/INPUT/PROD/FLOW/UX 77 |
| CART-MOD-007 | Workspace | EDIT/PROC/PROG/STOR/VALID/BACKUP/HIST/TRASH/REC/VER 134 |
| CART-MOD-008 | AccountAdministration | AUTH/ADMIN/SEC/MON 74 |
| CART-MOD-009 | Audit | AUDIT 8 |
| CART-MOD-010 | Support | SUPPORT 8 |
| CART-MOD-011 | Assistant | ASSIST 30 |
| CART-MOD-012 | ContentIntelligence | INTEL 2 |

Total 550. Mapping adalah overlay cartography pra-implementasi. Boundary data Processing/Jobs, Secured Files, History, dan Platform Configuration tetap mengikuti Dokumen 07; exact module anchor memerlukan controlled architecture confirmation dan bukan alasan cross-module write.

## 20. Component map

Komponen logical utama: Feature Registry, Processing Coordinator, Engine Resolver, module Application Services, Local Job Coordinator, Server Job Service, Secured File Service, Authorization Service, Audit Service, Issue Reporting Service, Assistant Orchestrator, Content Intelligence Resolver, Result Validation, dan Progress/Activity projection. Detail berada pada APPLICATION_COMPONENT_CATALOG.md.

## 21. Building Blocks map

| Building Block | Tujuan | Larangan |
|---|---|---|
| Contracts | ID dan typed/versioned contract | Tidak memuat business workflow/vendor/UI/ORM |
| Security | Authn/Authz context dan policy interfaces | Tidak memberi content access implisit |
| Observability | Safe log/trace/metric/correlation | Tidak menyimpan content/Audit truth |
| Storage.Abstractions | Opaque local/object operations | Tidak menjadikan path/key sebagai authority |
| Processing.Abstractions | Job/progress/cancel/checkpoint/provider ports | Tidak memuat branching Feature/vendor |

## 22. 33 Core Tool map

Semua card memakai UI Card yang bernama sama dengan Tool. D=Desktop, M/T=Mobile/Tablet curated, W=Web conditional.

| # | Tool / Feature canonical | Module | Mode; API/Worker | Risk; permission; Audit | PoC; test/UAT; runbook |
|---:|---|---|---|---|---|
| 1 | Merge PDF — FEAT-PDF-005 | Pdf | LOCAL; Local Worker optional | Low; Base; L2 | No; UAT-CORE-001; Data Integrity |
| 2 | Split PDF — FEAT-PDF-006/007 | Pdf | LOCAL; Local Worker | Low; Base; L2 | No; UAT-CORE-002; Data Integrity |
| 3 | Remove pages — FEAT-PDF-013 | Pdf | LOCAL | Low; Base; L2 | No; UAT-CORE-003; Data Integrity |
| 4 | Extract pages — FEAT-PDF-014 | Pdf | LOCAL | Low; Base; L2 | No; UAT-CORE-004; Data Integrity |
| 5 | Organize PDF — FEAT-PDF-010..016 | Pdf | LOCAL workflow | Low; Base; L2 | No; UAT-CORE-005; Data Integrity |
| 6 | Scan to PDF — FEAT-SCAN-014 | ScannerOcr | LOCAL/device | Low; OS consent; L2 | Yes; UAT-CORE-006; Local Worker |
| 7 | Compress PDF — FEAT-PDF-023 | Pdf | LOCAL; Local Worker | Low; Base; L2 | No; UAT-CORE-007; Data Integrity |
| 8 | Repair PDF — FEAT-PDF-025/026 | Pdf | LOCAL + approved fallback | Low; Base+Server; L2 | No formal; UAT-CORE-008; Malicious Document |
| 9 | OCR PDF — FEAT-OCR-004 | ScannerOcr | HYBRID; Local/Server Worker | Medium; Base+Server; L2 | Yes; UAT-CORE-009; OCR/AI |
| 10 | JPG to PDF — FEAT-CONV-007 | Conversion | LOCAL | Low; Base; L2 | No; UAT-CORE-010; Data Integrity |
| 11 | Word to PDF — FEAT-CONV-001 | Conversion | HYBRID; Worker | Low; Base+Server; L2 | Yes; UAT-CORE-011; Conversion |
| 12 | PowerPoint to PDF — FEAT-CONV-003 | Conversion | HYBRID; Worker | Low; Base+Server; L2 | Yes; UAT-CORE-012; Conversion |
| 13 | Excel to PDF — FEAT-CONV-002 | Conversion | HYBRID; Worker | Low; Base+Server; L2 | Yes; UAT-CORE-013; Conversion |
| 14 | HTML to PDF — FEAT-CONV-017 | Conversion | HYBRID; isolated Worker | Medium; Base+Server; L2 | Yes; UAT-CORE-014; Malicious Document |
| 15 | PDF to JPG — FEAT-CONV-012 | Conversion | LOCAL; Local Worker | Low; Base; L2 | No; UAT-CORE-015; Data Integrity |
| 16 | PDF to Word — FEAT-CONV-004 | Conversion | HYBRID; Worker | Low; Base+Server; L2 | Yes; UAT-CORE-016; Conversion |
| 17 | PDF to PowerPoint — FEAT-CONV-006 | Conversion | HYBRID; Worker | Low; Base+Server; L2 | Yes; UAT-CORE-017; Conversion |
| 18 | PDF to Excel — FEAT-CONV-005 | Conversion | HYBRID; Worker | Low; Base+Server; L2 | Yes; UAT-CORE-018; Conversion |
| 19 | PDF to PDF/A — FEAT-PDF-075 | Pdf | HYBRID; Worker | Medium; Base+Server; L2 | Yes; UAT-CORE-019; Data Integrity |
| 20 | Rotate PDF — FEAT-PDF-012 | Pdf | LOCAL | Low; Base; L2 | No; UAT-CORE-020; Data Integrity |
| 21 | Add page numbers — FEAT-PDF-037 | Pdf | LOCAL | Low; Base; L2 | No; UAT-CORE-021; Data Integrity |
| 22 | Add watermark — FEAT-PDF-036 | Pdf | LOCAL | Low; Base; L2 | No; UAT-CORE-022; Data Integrity |
| 23 | Crop PDF — FEAT-PDF-019 | Pdf | LOCAL | Low; Base; L2 | No; UAT-CORE-023; Data Integrity |
| 24 | Edit PDF — FEAT-PDF-031..041 | Pdf | LOCAL + approved subset | Medium; Base+Server; L2/L3 by action | Yes subset; UAT-CORE-024; PDF/Conversion |
| 25 | PDF Forms — FEAT-PDF-053..060 | Pdf | LOCAL | Low; Base; L2 | No formal; UAT-CORE-025; Malicious Document |
| 26 | Unlock PDF — FEAT-PDF-062 | Pdf | LOCAL | High; document.pdf.unlock; L2+L3 | No; UAT-CORE-026; Malicious Document |
| 27 | Protect PDF — FEAT-PDF-061 | Pdf | LOCAL | Medium; document.pdf.protect; L2+L3 | No; UAT-CORE-027; Data Integrity |
| 28 | Sign PDF — FEAT-SIGN-013 | SignatureAnnotation | LOCAL non-PKI | Medium; document.signature.apply; L2+L3 | No; UAT-CORE-028; Data Integrity |
| 29 | Redact PDF — FEAT-PDF-069 | Pdf | LOCAL | High; document.redaction.finalize; L2+L3 | No; UAT-CORE-029; Redaction |
| 30 | Compare PDF — FEAT-PDF-071/072 | Pdf | LOCAL; Local Worker | Low; Base; L2 | No; UAT-CORE-030; Data Integrity |
| 31 | AI Summarizer — FEAT-INTEL-001 | ContentIntelligence | HYBRID approved internal | Medium; intelligence; L2+AI event | Yes; UAT-CORE-031; AI Provider |
| 32 | Translate PDF — FEAT-INTEL-002 | ContentIntelligence | HYBRID approved internal | Medium; intelligence; L2+AI event | Yes; UAT-CORE-032; AI Provider |
| 33 | PDF to Markdown — FEAT-CONV-036 | Conversion | LOCAL first; optional complex | Low; Base+Server; L2 | Yes; UAT-CORE-033; Data Integrity |

Sign PDF adalah visual signature/paraf non-PKI. PKI/certificate Digital Signature tetap out of scope.

## 23. 550 Feature traceability

Coverage canonical:

| Module | Prefix/count | Total |
|---|---|---:|
| Pdf | PDF 75 | 75 |
| Conversion | CONV 36 | 36 |
| Image | IMG 48 | 48 |
| ScannerOcr | SCAN 18; OCR 13 | 31 |
| SignatureAnnotation | SIGN 13; ANN 14 | 27 |
| FileProductivity | FILE 10; INPUT 21; PROD 18; FLOW 10; UX 18 | 77 |
| Workspace | EDIT 12; PROC 15; PROG 9; STOR 16; VALID 18; BACKUP 16; HIST 15; TRASH 12; REC 15; VER 6 | 134 |
| AccountAdministration | AUTH 12; ADMIN 22; SEC 23; MON 17 | 74 |
| Audit | AUDIT 8 | 8 |
| Support | SUPPORT 8 | 8 |
| Assistant | ASSIST 30 | 30 |
| ContentIntelligence | INTEL 2 | 2 |
| Total | 30 prefix | 550 |

Exact Feature row tetap berasal dari Dokumen 03. Overlay platform, owner, component, permission, Audit, lifecycle, artifact, dan actual path masih PARTIAL sampai implementasi/release evidence tersedia.

## 24. Feature-family map

Reusable family: file intake/validation, page operations, conversion, OCR/extraction, working-state edit, security finalization, processing/job, dan resource CRUD/query. Reuse tidak mengubah Feature ID atau typed schema.

## 25. Processing-mode map

| Profile | Count | Pemetaan |
|---|---:|---|
| L1 | 247 | Local preferred |
| H1 | 18 | Hybrid capability-aware |
| L-SYS | 107 | Local/system; tidak membuat processing API |
| SYS-H | 33 | Hybrid system capability |
| E1 | 85 | Enterprise/server |
| SEC1 | 23 | Security control |
| AI-H | 30 | Assistant local deterministic + internal option |
| AI-L | 1 | Optional local model |
| AI-S | 1 | Approved internal AI service |
| FUTURE | 5 | Tidak ada implementation claim |

## 26. Local processing flow

~~~mermaid
sequenceDiagram
  actor User
  User->>Desktop: Pilih Feature dan file
  Desktop->>ApplicationService: Validasi + authorize
  ApplicationService->>EngineResolver: Pilih local profile
  EngineResolver->>LocalWorker: Typed IPC bila berat
  LocalWorker-->>ApplicationService: Staged result
  ApplicationService->>Workspace: Validate + atomic promote
  ApplicationService->>SQLite: State/history/checkpoint
~~~

Tidak ada API, SQL Server, SignalR, atau server storage untuk local-only processing.

## 27. Server processing flow

Client → preflight/upload → canonical execute API → persisted Job → Server Worker → approved provider → validated secure result → persisted terminal state → SignalR hint → REST reconcile/download → cleanup/retention.

## 28. Assistant flow

User → Assistant intent → canonical Feature ID → typed parameter validation → permission/policy/capability → confirmation bila perlu → application service → actual result. Assistant tidak mengakses SQL, filesystem, shell, engine, atau arbitrary API.

## 29. Content Intelligence flow

Selected content → extraction/OCR bila perlu → Content Intelligence Resolver → approved local/enterprise-internal provider → validate provenance/grounding → local/server result. Public AI dan silent public fallback dilarang.

## 30. Amankan ke Akun flow

Explicit user action → ownership/policy/quota/reauth → transfer bila perlu → secured metadata + secure object → version management maksimum tiga versi dapat diakses → cross-device retrieval. Ini bukan full workspace sync dan bukan Operational Backup/DR.

## 31. Issue Reporting flow

User message + optional explicitly previewed screenshot → safe diagnostics → Issue API → support metadata/private attachment → Support console. Attachment gagal tidak boleh menghilangkan pesan.

## 32. Audit flow

Business/security action → typed AuditIntent → protected local/domain outbox → Audit ingestion → append-oriented Audit Store → bounded authorized query/export. Content, password, token, OCR text, signature, clipboard, dan AI payload dilarang.

## 33. Realtime flow

Persisted state/outbox → SignalR /hubs/activity → authenticated client Activity Center. Event at-least-once dapat duplikat/terlambat. Jika disconnect/gap, client mengambil state REST yang persisted.

## 34. Interface catalog

Catalog lengkap berada pada INTEGRATION_INTERFACE_CATALOG.md. Setiap entry mempunyai producer, consumer, purpose, protocol, direction, authn/authz, version, data, retry, timeout, idempotency, error, SOT, monitoring, owner, dan status.

## 35. API map

API existing hanya mengikuti Dokumen 06:

- /api/v1/features/{featureId}/execute dan /workflows/execute;
- file transfer/upload sessions;
- jobs/results;
- account, history, secured-files, issues/support, audit-events;
- effective configuration dan approved admin policy/flag/limit;
- /api/compatibility serta minimal health.

Admin user/session/role/permission, monitoring aggregate, Activity resource, dan export retrieval contract yang belum ada tidak dibuat oleh Dokumen 18.

## 36. SignalR map

/hubs/activity memberi notification envelope versioned, scoped, deduplicated, dan recoverable. SignalR bukan command channel, source of truth, atau pengganti REST.

## 37. IPC map

Desktop → isolated Local Worker memakai Named Pipe/local IPC melalui typed transport. Wajib restrictive ACL, intended-user/profile binding, authenticated handshake, IPC compatibility, handler allowlist, job-scoped refs, timeout, cancel, heartbeat, resource limit, dan PoC. Tidak memakai localhost HTTP.

## 38. Internal-event map

Internal event harus post-commit/outbox, owner-scoped, versioned, mempunyai stable Event ID, consumer registry, ordering/dedupe/retry/retention. Exact event catalog belum dikunci; nama baru tidak dibuat di sini.

## 39. Engine/provider map

Application Service → Engine Resolver → approved adapter/provider. Resolver memeriksa capability, platform, policy, security, resource, connectivity, dan version. User/UI/Assistant tidak memilih vendor. Output provider selalu untrusted sampai divalidasi.

## 40. External dependency map

Dependency: Pertamina Identity, Azure DevOps, secure storage, approved engine/provider, approved internal Content Intelligence/translation provider, monitoring/APM, dan enterprise network/infrastructure. Exact product/endpoint/owner/status ada pada EXTERNAL_DEPENDENCY_REGISTER.md dan tetap TBD sampai disetujui.

## 41. Data-flow register

Register berwenang Dokumen 18 berada pada DATA_FLOW_REGISTER.md. Minimum flow: local processing, Local Worker, optional server, server Job, secured file, cross-device, Issue, Audit, History, telemetry, Assistant, intelligence, translate, Markdown, visual sign, redact, admin config, Audit export, dan incident diagnosis.

## 42. Data-store map

| ID | Store | Purpose | Owner/SOT |
|---|---|---|---|
| CART-DATA-001 | SQL Server | Server metadata/state | Module schema owner |
| CART-DATA-002 | SQLite | Per-profile local state | Client/Workspace |
| CART-DATA-003 | Local Workspace | Source/draft/temp/result | User + Workspace |
| CART-DATA-004 | Secure Storage | Server payload/version | Domain aggregate + object |
| CART-DATA-005 | Audit Store | Enterprise evidence | Audit |
| CART-DATA-006 | Issue Attachment Store | Explicit attachment/export | Support |
| CART-DATA-007 | Telemetry Store | Safe health signal | Observability, not business SOT |
| CART-DATA-008 | Artifact Store | Immutable release artifacts | Azure DevOps/release governance |

## 43. SQL Server schema ownership

Gunakan schema canonical Dokumen 07, termasuk secured.*. Client tidak direct SQL. Cross-schema write memakai application contract atau post-commit event. Label Processing/Jobs, Secured Files, History, dan Platform Configuration adalah data-owner boundary; exact top-level module anchor masih confirmation gap.

## 44. SQLite/local-state map

SQLite menyimpan per-profile local workspace/job/history/preference/recovery metadata dan protected Audit outbox. Tidak menyimpan document/signature BLOB, password, token, atau full OCR. Local Worker tidak direct-write SQLite.

## 45. Local Workspace map

Memuat user-selected source reference, managed draft, undo/redo, checkpoint, temp/cache/thumbnail, staged result, dan finalized local result. Cleanup tidak menghapus source/final/history. Tidak otomatis disinkronkan ke server.

## 46. Secure Storage map

Private storage dibagi purpose zone: quarantine/input, processing/temp, result, secured versions, Issue attachment, dan export. Metadata/ownership/state berada di SQL; bytes di storage. Atomic finalize, digest, expiry, tombstone, hold, dan owner-authorized delete wajib.

## 47. Source-of-truth matrix

| Subject | SOT |
|---|---|
| Feature | Canonical Feature Registry/Dokumen 03 |
| API | Typed C# + governed generated OpenAPI |
| Server Job | Persisted jobs store |
| Local Job/workspace | SQLite + protected manifest |
| Realtime | Bukan SOT; persisted REST state menang |
| Server metadata | Owned SQL schema |
| Local state | SQLite |
| Secured payload | Secure object version + secured aggregate |
| Audit | Audit Store |
| History | History Store/projection |
| Issue | Support/Issue Store |
| Deployment artifact | Immutable Azure DevOps artifact + Release Manifest |

## 48. Data classification

Klasifikasi kerja: PUBLIC_INTERNAL, INTERNAL, RESTRICTED, SENSITIVE, dan TBD_OFFICIAL. Ini bukan klasifikasi resmi. Semua keputusan data resmi memakai REQUIRES_PERTAMINA_DATA_CLASSIFICATION_CONFIRMATION.

## 49. Security-zone map

Conceptual zones:

USER DEVICE, CLIENT APPLICATION, LOCAL PROCESSING, ENTERPRISE API, SERVER PROCESSING, DATA, SECURE STORAGE, OBSERVABILITY, ADMIN/SUPPORT, dan EXTERNAL/APPROVED PROVIDER.

Nama zone/network segment resmi belum ditetapkan.

## 50. Trust-boundary map

| Boundary | Kontrol minimum |
|---|---|
| User → Client | Auth session, local profile, consent, safe file validation |
| Client → API/SignalR | TLS, SSO, permission, ownership/scope, rate/admission |
| Desktop → Local Worker | ACL, handshake, version, job binding, allowlist |
| API → Worker | Workload identity, durable Job/lease, no user-token forwarding |
| App → SQL/Storage | Least privilege, one owner, opaque refs after authorization |
| App → Provider | Allowlist, minimization, egress, timeout/circuit, no public fallback |
| Runtime → Telemetry | Closed safe-field allowlist, no content/secret |

## 51. Authentication boundary

Pertamina Identity/SSO adalah baseline. Exact protocol, issuer, audience, claims, callback, provisioning, revocation, reauthentication, offline grant, dan workload identity memerlukan Pertamina confirmation. Tidak ada app-managed password store.

## 52. Authorization boundary

Server adalah final authority untuk online resource. Keputusan memeriksa subject, permission, owner/scope, resource state/version, policy, capability, classification, consent, capacity, dan reauth. UI visibility bukan authorization. Admin/Support tidak otomatis memperoleh content access.

## 53. Deployment map

Logical deployables: Web, API, Server Worker, Desktop, Local Worker, Mobile/Tablet, SQL Server, Secure Storage, Monitoring, SQLite/local state. Separate Deployables tidak berarti microservices dan co-hosting tidak menghapus logical boundary.

## 54. Environment matrix

Local Development, Shared Development, QA/Testing, Pilot, dan Production memisahkan endpoint, identity, data, config, secret, dependency, provider, telemetry, dan authority. Detail berada pada ENVIRONMENT_DEPLOYMENT_MATRIX.md.

## 55. Hostinger temporary topology

Hostinger KVM 2 dapat menampung logical Web/API/Worker/SQL/storage/monitoring untuk Development, limited QA, atau approved limited Pilot. Ia tetap satu physical failure/security/resource domain, bukan HA, bukan segmentasi resmi, dan bukan Production baseline.

## 56. Production placeholder topology

~~~mermaid
flowchart LR
  C[Approved Clients] --> E[Approved Ingress/Identity]
  E --> W[Web/API/SignalR]
  W --> D[Internal Data/Job/Storage]
  D --> K[Server Worker]
  K --> P[Approved Provider]
  W --> M[Approved Monitoring]
~~~

Physical placement = PERTAMINA_APPROVED_PRODUCTION_INFRASTRUCTURE. Hosting, OS, DNS, IP, port, firewall, TLS, proxy, HA, capacity, backup, DR, dan owner belum dikarang.

## 57. Artifact/deployable map

Release artifact minimum: Web, API, Server Worker, Desktop, Local Worker cohort, Mobile, DB Migration Bundle, Configuration Template, OpenAPI, Release Notes, SBOM, Test/Security Evidence, dan Release Manifest. Component-to-artifact instance masih RC-TBD.

## 58. Version/compatibility map

Lacak ProductVersion, BuildVersion, CommitId, API ContractVersion, CapabilityVersion, Parameter/Result/Workflow SchemaVersion, DB MigrationVersion, ConfigurationVersion, Desktop/Mobile/LocalWorkerVersion, LocalSchemaVersion, IpcContractVersion, event/provider/checkpoint version. Unknown incompatible version fail safely.

## 59. Monitoring/health map

Setiap host/dependency mempunyai liveness/readiness/capability-degraded status, safe metrics, freshness, owner, alert, correlation, dan runbook. Health publik minimal; detail dibatasi. Unknown/stale critical telemetry bukan HEALTHY.

## 60. Audit/observability map

Audit, personal History, technical logs/telemetry, dan Issue/Support adalah empat plane terpisah. Hanya safe opaque references/Correlation ID yang menghubungkan. Telemetry sampling tidak boleh dipakai sebagai bukti Audit.

## 61. Ownership map

Fungsi yang dipetakan: Business, Product, Application, Technical, Module, Data, Security, Operations, Support L1/L2/L3, Database, Deployment Approver, Audit, Documentation. Semua penugasan resmi masih TBD pada APPLICATION_OWNERSHIP_REGISTER.md.

## 62. Support-level map

L1 menerima dan mengumpulkan safe evidence; L2 mendiagnosis application/module/config; L3 menangani code/architecture/provider/data/security defect melalui change/release governance. Access mengikuti permission dan tidak meningkat otomatis karena eskalasi.

## 63. Runbook linkage

Component/dependency mengacu pada family runbook Dokumen 16 dan deployment runbook Dokumen 17. Dedicated Image/Signature runbook baru dibuat setelah provider/RC/error/monitoring nyata tersedia. Family reference bukan bukti drill atau Production validation.

## 64. Test linkage

Dokumen 11–13 memberi Unit, Test Plan, dan UAT design. 33 Core memiliki UAT-CORE-001..033. Exact test project/path, implementation result, PoC evidence, RC regression, dan UAT sign-off masih belum tersedia.

## 65. Documentation linkage

Dokumen 14–15 menjelaskan user/admin baseline; Dokumen 16 Incident; Dokumen 17 Deployment; Dokumen 19 menerima owner, access, artifact, dependency, risk, runbook, diagram, dan evidence dari cartography ini.

## 66. Dependency map

Kritis: Feature Registry, Contracts, Pertamina Identity, permission/config, SQL, Secure Storage, Job Store/Queue, Server Worker, Local Worker, Workspace/SQLite, PDF/OCR/Conversion/Image engines, Content Intelligence, Audit, Monitoring, Support, Azure DevOps, network/time/certificate, dan client compatibility.

## 67. Criticality map

Working label HIGH, MEDIUM, LOW, atau TBD_OFFICIAL dipakai hanya untuk analisis konseptual. Official criticality, availability, SLA, RPO, dan RTO memerlukan REQUIRES_PERTAMINA_CRITICALITY_CONFIRMATION.

## 68. Single-point-of-failure review

Known candidates: temporary Hostinger node/disk/ingress, single SQL/storage/queue/Worker/provider/identity/DNS/certificate/key/monitoring/Audit/artifact/distribution/owner/backup location. Cartography mencatat impact, detection, fallback, runbook, owner gap; tidak otomatis menambah infrastruktur.

## 69. Lifecycle/status

Allowed lifecycle: PLANNED, DESIGNED, POC_REQUIRED, APPROVED_WITH_LIMITATION, IMPLEMENTING, TESTING, PILOT, PRODUCTION, DEPRECATED, RETIRED, BLOCKED. Current feature evidence: PLANNED 529, FEASIBILITY_GATE 16, FUTURE 5. Tidak ada Feature/deployable yang ditandai PRODUCTION.

## 70. Risks

Risiko utama: god module, one-card-one-service, duplicate endpoint, Assistant/provider bypass, hidden server dependency, cross-module SQL write, SignalR as truth, public AI fallback, ownerless component, stale diagram, false platform parity, false Production topology, generic runbook dianggap tervalidasi, dan mapping manual 550 drift.

## 71. Known gaps

1. FR-FILE-006..009 direferensikan melalui range pada Dokumen 03 tetapi tidak ditemukan sebagai heading canonical Dokumen 02; controlled source correction diperlukan.
2. ContentIntelligence disebut dalam desain tetapi exact repository placement belum tersedia.
3. Local Worker locked sebagai deployable, tetapi exact project path belum tersedia.
4. Jobs, Secured Files, History, dan Platform Configuration mempunyai data-owner label yang belum one-to-one dengan top-level module.
5. Beberapa admin/monitoring/activity/export API contract belum ada dan tidak dibuat.
6. IPC/internal event/provider/storage contracts belum lengkap.
7. Owner, classification, criticality, CMDB, Production topology, dan actual artifact masih TBD.

## 72. Maintenance process

~~~text
Change trigger → impact analysis → update source/register
→ regenerate/review diagrams → validate orphan/delta
→ version → publish → archive superseded baseline
~~~

Trigger wajib: module/component/Feature/API/database/data flow/provider/security/deployment/owner/version/test/runbook/lifecycle change.

## 73. Diagram register

Diagram source menggunakan Mermaid/Markdown dengan ASCII fallback. Setiap diagram mencatat ID, title, purpose, scope, legend, source, last updated, owner, version, dan validation status. Daftar berada pada CARTOGRAPHY_DIAGRAM_REGISTER.md.

## 74. Component catalog

APPLICATION_COMPONENT_CATALOG.md adalah catalog logical pre-implementation. Actual namespace, project path, class, package, health endpoint, artifact digest, owner, dan deployed version tetap TBD sampai repository/release tersedia.

## 75. Interface catalog

INTEGRATION_INTERFACE_CATALOG.md mengikat producer, consumer, protocol, security, version, data, idempotency, failure, authority, monitoring, owner, dan status. Missing interface tidak boleh diisi dengan endpoint/event baru.

## 76. External dependency register

EXTERNAL_DEPENDENCY_REGISTER.md mempertahankan provider/vendor/product sebagai placeholder sampai PoC, license, security, data governance, egress, support, fallback, exit, owner, dan environment disetujui.

## 77. Data-flow register

DATA_FLOW_REGISTER.md memisahkan local, server, secured, Issue, Audit, History, telemetry, Assistant, intelligence, admin, export, dan incident. Tidak ada direct client SQL, silent upload, public AI, atau content pada telemetry/Audit.

## 78. Environment deployment matrix

ENVIRONMENT_DEPLOYMENT_MATRIX.md memetakan Desktop, Mobile, Web, API, Workers, SQL, SQLite, Workspace, storage, monitoring, provider, artifact source, dan backup terhadap Local Dev, Shared Dev, QA, Pilot, dan Production placeholder.

## 79. Ownership register

APPLICATION_OWNERSHIP_REGISTER.md menyimpan accountable function, scope, decision authority, support level, backup/deputy, status, dan confirmation gap. Generic team label tidak mengganti assignment resmi.

## 80. Requirement traceability

Status PARTIAL. Semua 550 Feature row mempunyai Requirement reference cell, tetapi reverse trace tidak one-to-one dan gap FR-FILE-006..009 belum diselesaikan. Dokumen 18 tidak membuat requirement baru atau mengubah Dokumen 02–03.

## 81. Feature traceability

Status COMPLETE untuk 550 ID/count/uniqueness, 33 Core mapping, execution profile, dependency family, dan catalog status. Status PARTIAL untuk per-Feature module/component/host/platform/permission/Audit/test/manual/runbook/lifecycle/artifact. Range dipakai untuk laporan, bukan wildcard execution.

## 82. API traceability

Status PARTIAL pre-implementation. Existing route families terhubung ke consumer dan logical owner. Tidak ada duplicate endpoint. Local-only = API NOT_APPLICABLE. Missing admin/monitoring/activity/export details memerlukan controlled API decision, bukan invention.

## 83. Database traceability

Status SOURCE_VALIDATED untuk schema/ownership/storage principle Dokumen 07. Actual DbContext/project/migration/artifact belum tersedia. secured.* adalah canonical; backup.* bukan schema alternatif. Client direct SQL dan cross-owner write dilarang.

## 84. Security traceability

Status SOURCE_VALIDATED pada conceptual control: SSO, permission/ownership, deny-by-default, data minimization, encryption, safe telemetry, Audit separation, provider allowlist, Local Worker containment. Official network/data classification dan implementation evidence masih pending.

## 85. Test/UAT traceability

33/33 Core mempunyai Test Plan dan UAT specification. Unit/test family tersedia. PoC, automated result, QA execution, performance/security evidence, dan UAT sign-off belum ada; karena itu lifecycle tidak dinaikkan.

## 86. Manual/runbook traceability

User/Admin Manual dan family runbook terhubung. Screenshot/label/route/contact/exact command/owner/drill bergantung pada RC dan approval; generic family link bukan operational validation.

## 87. Cartography acceptance checklist

### Baseline acceptance

- [x] Dokumen 17 FINAL diverifikasi.
- [x] Dokumen 01–17 dan empat specialist report diaudit.
- [x] Baseline 550; 0 duplicate; angka lama tidak dipakai.
- [x] 33/33 Core Tool dipetakan.
- [x] Module, host, interface, data store, Local-First, REST/SignalR/IPC, security, deployment, test, dan runbook tercakup.
- [x] Provider/topology/owner resmi tidak dikarang.
- [x] Dokumen 01–17 dan specialist report tidak diubah.

### Scenario trace audit

| Scenario | Ringkasan path | Hasil desain |
|---|---|---|
| Merge PDF lokal | User→Desktop→Pdf→local engine→Workspace/SQLite→L2→Data Integrity | PASS |
| OCR optional server | User→Resolver→Local/Server Worker→OCR→result→L2→OCR/AI | PASS |
| Word to PDF | User→Conversion→approved resolver→Worker→result→L2 | PASS, POC_REQUIRED |
| AI Summarizer | User→ContentIntelligence→approved internal provider→result | PASS, no public AI |
| Sign visual | User→SignatureAnnotation→Pdf→local result→L3 | PASS, non-PKI |
| Redact | User→Pdf→forensic validation→local result→L3 | PASS |
| Amankan ke Akun | User→API→secured aggregate/storage→max three | PASS |
| Issue Reporting | User→Issue API→Support store→Support console | PASS |
| Admin permission change | Admin→authorized typed surface→owner store→Audit | PASS design; exact route gap visible |
| Audit export | Auditor→Audit query→Job/Worker→restricted result | PASS design; retrieval binding gap visible |
| Worker failure | Job/lease/checkpoint→reclaim/retry→REST state | PASS |
| Production deployment | Approved artifact→placeholder infrastructure→health | PASS as logical trace only |
| Incident investigation | Support/Operations→Issue/Telemetry/Audit/Job refs→runbook | PASS |

### Readiness boundary

Document 18 dinyatakan FINAL sebagai baseline pra-implementasi. Status CARTOGRAPHY_COMPLETE, IMPLEMENTED, TESTED, PILOT, PRODUCTION, PRODUCTION_READY, dan HANDOVER_READY belum boleh digunakan sampai gap, owner, actual inventory, artifacts, platform exposure, PoC, tests/UAT, topology, monitoring, dan operational evidence diselesaikan.

