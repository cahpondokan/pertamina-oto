# Agent 2 — Trace FEAT-PDF-005

Run: P01-20260905-1319. Baseline: d497ec68ea8fa3e21ca311a580a44111045cfb20.
Scope: kontrak semantic lokal, domain murni, orchestration melalui fake boundary. Tidak ada real PDF adapter.
Status FOUNDATION berarti hanya bukti pada lapisan ini; bukan kelulusan seluruh kasus canonical atau FEAT-PDF-005.

| Test ID | Disposition Phase 1 | Bukti / alasan terkendali |
|---|---|---|
| UT-MERGE-001 | FOUNDATION | Execute_TooFewInputs_RejectsWithoutBoundaryCalls, data 0 |
| UT-MERGE-002 | FOUNDATION | Execute_TooFewInputs_RejectsWithoutBoundaryCalls, data 1 |
| UT-MERGE-003 | FOUNDATION | Execute_ValidInputs_PreservesExactOrderAndFinality, data 2; validitas PDF nyata Phase 3 |
| UT-MERGE-004 | FOUNDATION | Execute_ValidInputs_PreservesExactOrderAndFinality, data 3/20 |
| UT-MERGE-005 | FOUNDATION | Execute_ValidInputs_PreservesExactOrderAndFinality; tidak sort by filename/hash |
| UT-MERGE-006 | FOUNDATION | Plan_RemoveAndReorder_ChangesOnlyWorkingSelectionDeterministically |
| UT-MERGE-007 | FOUNDATION | Plan_RemoveAndReorder_ChangesOnlyWorkingSelectionDeterministically; tidak ada source-write port |
| UT-MERGE-008 | FOUNDATION | Plan_RemoveAndReorder_ChangesOnlyWorkingSelectionDeterministically; single selected occurrence invalid |
| UT-MERGE-009 | FOUNDATION | Execute_DuplicateFileOccurrences_DoesNotDeduplicate |
| UT-MERGE-010 | FOUNDATION | Execute_InvalidIdentity_RejectsBeforeProcessing, unknown/repeated/duplicateIdentity |
| UT-MERGE-011 | DEFERRED | Minimum count diuji; numeric page/size/capacity budget belum disetujui, tidak mengarang limit. Policy preflight Phase 3 |
| UT-MERGE-012 | DEFERRED | Fast/structural validator nyata membutuhkan PoC/provider; preflight None adalah admission semantic |
| UT-MERGE-013 | FOUNDATION BOUNDARY | Execute_PreflightDenied_NeverStagesOrPublishes InputRejected; signature inspector Phase 3 |
| UT-MERGE-014 | FOUNDATION BOUNDARY | InputRejected dan Execute_InvalidStage_NeverPublishes; corrupt corpus nyata Phase 3 |
| UT-MERGE-015 | FOUNDATION BOUNDARY | InputRejected/unknown issue fail closed; security inspection implementation Phase 3 |
| UT-MERGE-016 | FOUNDATION BOUNDARY | Execute_PreflightDenied_NeverStagesOrPublishes PasswordRequired |
| UT-MERGE-017 | FOUNDATION BOUNDARY | InputRejected dan Execute_BoundaryException_ReturnsOnlySafeCategory; password adapter belum ada |
| UT-MERGE-018 | DEFERRED | Tidak ada password field/storage/log. Ephemeral password capability memerlukan adapter/security contract Phase 3 |
| UT-MERGE-019 | FOUNDATION BOUNDARY | Execute_PreflightDenied_NeverStagesOrPublishes SourceChanged; version-bound source handle adapter Phase 3 |
| UT-MERGE-020 | FOUNDATION BOUNDARY | Hanya ILocalMergeProcessor; tidak ada network/server surface. Resolver nyata belum ada |
| UT-MERGE-021 | FOUNDATION BOUNDARY | Semua test in-memory tanpa I/O jaringan; bukan klaim OS network-disabled atau PDF offline E2E |
| UT-MERGE-022 | DEFERRED | Worker/IPC di luar task; BASE-OI-013 masih memerlukan governed wire/auth/version contract |
| UT-MERGE-023 | DEFERRED | Tidak memilih provider/fallback sebelum POC-PDF-001 |
| UT-MERGE-024 | FOUNDATION BOUNDARY | Execute_PreflightDenied_NeverStagesOrPublishes Unavailable; tidak membuat server path |
| UT-MERGE-025 | FOUNDATION BOUNDARY | Execute_PreflightDenied_NeverStagesOrPublishes InsufficientStorage; physical disk race Phase 3 |
| UT-MERGE-026 | FOUNDATION | Execute_PreCancelled_HasNoEffects; tidak mengklaim queue/Job implementasi |
| UT-MERGE-027 | FOUNDATION | Execute_CancelAtSafePoint_DiscardsOnlyStage; stage/validate; cleanup failure tidak mengubah terminal |
| UT-MERGE-028 | FOUNDATION | Execute_CancelAfterCommit_RemainsCompleted |
| UT-MERGE-029 | FOUNDATION BOUNDARY | Execute_BoundaryException_ReturnsOnlySafeCategory; zero automatic retry. Retry classification/budget Phase 3 |
| UT-MERGE-030 | FOUNDATION | Exception/invalid-stage tests; failed tanpa result atau automatic retry |
| UT-MERGE-031 | DEFERRED | Job/attempt persistence di luar immediate stateless foundation; Phase 3 |
| UT-MERGE-032 | DEFERRED | Tidak ada Idempotency-Key contract/persisted owner dalam task; caller tidak boleh memakai retry untuk exactly-once claim |
| UT-MERGE-033 | FOUNDATION BOUNDARY | Execute_ValidInputs_PreservesExactOrderAndFinality; fake Validate true sebelum publish |
| UT-MERGE-034 | FOUNDATION BOUNDARY | Execute_InvalidStage_NeverPublishes; empty stage/false output validation; real fidelity Phase 3 |
| UT-MERGE-035 | FOUNDATION BOUNDARY | Execute_TargetDenied_PreservesSourceAndExistingTarget TargetRejected; physical policy checks pada adapter nanti |
| UT-MERGE-036 | FOUNDATION | Execute_InvalidIdentity_RejectsBeforeProcessing sourceTarget; physical alias/reparse checks menjadi kewajiban publisher |
| UT-MERGE-037 | FOUNDATION BOUNDARY | TargetConflict sebelum stage dan publication conflict race; port hanya PublishNewAsync, tanpa overwrite switch |
| UT-MERGE-038 | FOUNDATION BOUNDARY | Execute_NonFinalPublication_ExposesNoResult, exception/empty receipt/reconciliation dan cleanup intent |
| UT-MERGE-039 | FOUNDATION | Execute_ValidInputs_PreservesExactOrderAndFinality mempertahankan correlation ke plan/outcome; History/Audit belum diimplementasikan |
| UT-MERGE-040 | FOUNDATION | Execute_CorrelationMissing_GeneratesAndPropagatesOneIdentifier |
| UT-MERGE-041 | FOUNDATION | Immediate Completed hanya receipt Published dengan nonempty result; tidak ada fake Job ID |
| UT-MERGE-042 | DEFERRED | Background Local Worker/Job di luar task dan BASE-OI-013 |
| UT-MERGE-043 | FOUNDATION | Outcome_NonFinalStates_ContainNoResultOrMutableState; outcome immutable, tidak ada progress mutation API. Event state machine lane lain |
| UT-MERGE-044 | DEFERRED | Workspace History port milik Agent 4; tidak membuat metadata persistence pada lane PDF |
| UT-MERGE-045 | DEFERRED | L2 Audit durable intent memerlukan integration/event owner, BASE-OI-010/014. Tidak mengklaim feature done |
| UT-MERGE-046 | FOUNDATION | Execute_BoundaryException_ReturnsOnlySafeCategory; safe enum/outcome, tidak ada logger/content/path string |
| UT-MERGE-047 | FOUNDATION | Plan_RemoveAndReorder_ChangesOnlyWorkingSelectionDeterministically; correlation eksplisit sama |
| UT-MERGE-048 | DEFERRED | File/Tool adapter Agent 3; Assistant tidak diimplementasikan Phase 1; tidak menduplikasi orchestrator |
| UT-MERGE-049 | FOUNDATION BOUNDARY | Preflight denial mencegah processor; actual permission evaluator/Assistant integration belum ada |
| UT-MERGE-050 | FOUNDATION BOUNDARY | Preflight denial/unknown category fail closed; descriptor registry actual integration Phase 3 |

Requirement exact yang disentuh:
- FR-FILE-010, FR-FILE-011, FR-FILE-012, FR-FILE-013: admission contract saja.
- FR-FILE-015: tidak menyediakan field password sehingga tidak masuk outcome/metadata.
- FR-PROC-002, FR-PROC-005: boundary lokal tanpa dependency server.
- FR-STOR-001, FR-STOR-002: source-preservation intent dan working occurrence selection.
- FR-STOR-005: stage-only cleanup intent.
- FR-STOR-006: capacity denial dari trusted preflight.
- NFR-ARCH-005: Domain/business rule tidak bergantung UI framework. NFR-ARCH-001..004 bukan klaim implementasi backend/Web pada lane ini.

Decision/reference: LOCAL-BOOT-001, CCR-015, P0-D05, P0-D06, EXEC-SCOPE-001, P1-A2-PDF-FOUNDATION.
Tidak memakai FR-FILE-006..009 yang bermasalah pada BASE-OI-007.
