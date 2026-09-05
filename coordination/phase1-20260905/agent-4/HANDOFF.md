# Handoff Agent 4 — P1-A4-OFFLINE-PORTS

Agent / Task ID: Agent 4 / P1-A4-OFFLINE-PORTS

Objective: foundation opaque refs, publikasi atomik tanpa overwrite, metadata workspace/history aman, serta batas state/job lokal.

Baseline commit/hash: d497ec68ea8fa3e21ca311a580a44111045cfb20 (preflight bersih).

Branch / worktree: task/P1-A4-offline-ports; worktrees/phase1-20260905/agent-4-offline-ports pada workspace dokumentasi yang ditugaskan.

## Requirement / Feature / Decision IDs

Foundation untuk FEAT-PDF-005, bukan implementasi Merge selesai.

| Exact IDs | Bukti/cakupan |
|---|---|
| FR-STOR-001, FR-STOR-002, FR-STOR-003, FR-STOR-005, FR-STOR-006 | Ref sumber/draft terpisah; kontrak no-replace, validate, capacity, cleanup; debounce/retention konkret belum diimplementasi |
| FR-PROC-002, FR-PROC-004, FR-PROC-005 | LocalDispatchPolicy hanya ExecuteLocally atau Unavailable; tidak ada transfer/route server |
| FR-HIST-001, FR-HIST-002, FR-HIST-003 | Feature/correlation/time/size/duration/status/result refs; metadata-only remove; page 1..50; display name/path melalui resolver lokal berotorisasi di fase berikutnya |
| FR-REC-006, NFR-REC-001, NFR-REC-002, NFR-REC-003, NFR-REC-004 | Source-preserving publication contract; temporary bukan final; safe failure/correlation; terminal tidak mundur |
| TECH-DEC-010, TECH-DEC-011 | Port lokal bertipe dan provider-neutral; tidak memilih wire IPC atau implementasi SQLite |
| UT-MERGE-020, UT-MERGE-021, UT-MERGE-024 | Rule locality, unavailable tidak menjadi remote; assembly allowlist |
| UT-MERGE-025, UT-MERGE-026, UT-MERGE-027, UT-MERGE-028 | Capacity denial, cancellation intent/safe acknowledgement, terminality |
| UT-MERGE-033, UT-MERGE-034, UT-MERGE-035, UT-MERGE-036, UT-MERGE-037, UT-MERGE-038 | Policy/fake publication gate dan failure; PDF structure/filesystem nyata bukan bukti lane ini |
| UT-MERGE-043, UT-MERGE-044 | Stale transition denied; safe metadata surface tanpa raw string/bytes |

FR-STOR-004 (cache), FR-REC-001/002/003/004/005 (checkpoint/resume/retry), serta autentikasi/IPC konkret tetap di luar implementasi lane ini. Port tidak mengklaim requirement penuh selesai.

## Files changed

- src/BuildingBlocks/Pertamina.Oto.Storage.Abstractions/{Pertamina.Oto.Storage.Abstractions.csproj,packages.lock.json,LocalReferences.cs,LocalPublication.cs,LocalMetadata.cs}
- src/BuildingBlocks/Pertamina.Oto.Processing.Abstractions/LocalJobs.cs
- tests/Unit/OfflineFoundation/{Pertamina.Oto.OfflineFoundation.Tests.csproj,packages.lock.json,OfflineContractTests.cs}
- coordination/phase1-20260905/agent-4/{HANDOFF.md,Verify.ps1,VERIFICATION.txt,SOURCE_SHA256.txt}

Tidak mengubah existing Processing csproj/lock, root solution, shared Contracts, PDF module, atau koordinasi pusat.

## Behavior implemented

Opaque GUID refs menolak empty ID dan tidak menerima path. Possession bukan authority; resolver/adapter harus memeriksa profile authorization. Publication policy menolak source alias, existing target, stage invalid, capacity kurang dan cancel sebelum commit. Result final hanya tersedia pada factory Published; failure tidak mempunyai final ref. Port mengharuskan stage → close/flush → validate/digest → atomic no-replace → metadata commit; ambiguous commit memerlukan reconciliation intent. Tidak ada opsi overwrite.

State machine lokal menolak semua transisi keluar terminal, memisahkan cancellation intent dari safe acknowledgement, dan menerima completed bila publication menang terhadap late cancel. Snapshot memakai failure code enum tanpa raw exception. Local route tidak mempunyai server fallback. Metadata memakai refs dan field bertipe; history removal tidak boleh menghapus file.

## Tests and exact results

18/18 PASS, 0 failed, 0 skipped. Build Release: 0 warning, 0 error. Lihat VERIFICATION.txt untuk exact command/exit. Verifier melakukan locked restore, build --no-restore, run --no-build dan git diff --check. Test SDK existing MSTest.Sdk/4.4.0; .NET SDK 10.0.400; tidak ada dependency/provider baru.

Build awal menolak tiga analyzer violations pada test (CA1861, dua CA1822); diperbaiki tanpa suppression. Run test awal menampilkan notice telemetry platform; verifier final menonaktifkan DOTNET_CLI_TELEMETRY_OPTOUT dan TESTINGPLATFORM_TELEMETRY_OPTOUT. Raw TRX tidak dipromosikan sebagai tracked evidence karena memuat machine/path; ringkasan yang disanitasi tersedia.

## Security/privacy checks

Negative tests untuk access denied meskipun ref valid, source/existing-target conflict, non-validated stage, insufficient storage, cancelled publication, failed commit, invalid enum/failure combination, terminal regression, serta page bounds. Reflection test melarang string/byte[]/URI pada metadata/ref/job snapshot surface. Assembly references di-allowlist ke System.Runtime, System.Collections, Pertamina.Oto.Contracts; tidak ada provider/network assembly. Tidak ada content/secret/production data, concrete filesystem, SQL, network transport, upload, server Job, mobile/web/iOS atau real PDF adapter.

## Performance/resource evidence

Suite pertama 721 ms untuk 18 test; ini timing test, bukan benchmark PDF. Tidak ada budget processing/corpus/adapter yang berlaku untuk port ini.

## Known limitations / open blockers and required human decision

Tidak ada blocker implementasi lane. API adalah kontrak foundation; implementer tetap harus membuktikan profile ownership, link/alias/TOCTOU defense, atomic filesystem publication, durable pointer/reconciliation, cleanup, optimistic concurrency, metadata validation, bounded retry dan actual worker recovery. Fake hanya membuktikan urutan kontrak dan failure behavior, bukan crash/physical source preservation nyata. Offline proof adalah absence of network dependency/route pada boundary; tidak mengklaim OS-network-disabled E2E. BASE-OI-013 masih memblokir exact Worker wire/auth/version implementation. BASE-OI-020 tetap mengatur provider admission. Tidak ada human approval baru yang diasumsikan.

## Evidence paths / recommended integration order

Evidence ada pada coordination/phase1-20260905/agent-4/VERIFICATION.txt; reproduksi memakai Verify.ps1 dari branch ini. SOURCE_SHA256.txt mengikat source/project/lock yang diuji sebelum commit.

Integrasi oleh Agent 0 setelah shared Contracts dan PDF domain/application; tambahkan Storage project dan OfflineFoundation test project ke root solution melalui ownership Agent 0. Processing hanya memakai referensi existing. Port PDF milik Agent 2 belum diubah/diduplikasi; penyelarasan nama/mapping dilakukan integrator. Tidak merge ke main/integration.
