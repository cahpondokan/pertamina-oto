# Kontrak integrasi Agent 2

Tiga assembly baru: Pertamina.Oto.Modules.Pdf.Contracts, Pertamina.Oto.Modules.Pdf.Domain, Pertamina.Oto.Modules.Pdf.Application.
Graph: Application -> Domain -> Contracts. Tidak ada reference ke UI, provider, storage implementation, server atau network package.
IMergeService adalah entry point UI. Namespace contract: Pertamina.Oto.Modules.Pdf.Contracts.
InputOccurrence memiliki OccurrenceId unik dan FileId opaque. FileId yang sama boleh mempunyai beberapa occurrence.
MergeCommand menyimpan snapshot inputs dan orderedOccurrenceIds (selected working set); removal tidak menghapus sumber.
Guid pada kontrak ini adalah semantic opaque ID, bukan proof authorization dan bukan physical path. Belum merupakan public API schema.
MergeIssue adalah semantic category module-local, bukan error code canonical baru.

Application memiliki tiga use-case ports: IMergePreflight, ILocalMergeProcessor, IMergePublication.
Ports ini adalah seam khusus use case dan tidak mengganti shared Storage/Processing contracts Agent 4.
Adapter ke shared ports dibuat saat integrasi/phase berikutnya oleh owner yang sah, bukan dengan mengubah shared lane secara paralel.

Preflight wajib memverifikasi permission/ownership setiap source, readiness/descriptor/version/capacity.
Tidak ada concrete preflight/processor/publisher pada production code: hanya fakes dalam tests.
Processor wajib membaca source tanpa mutasi, mengikat source version, dan tidak memublikasikan stage.
Processor memiliki tanggung jawab orphan cleanup/recovery jika throw sebelum stage ID dikembalikan.
Publisher wajib menguji policy/source alias/existing target sebelum stage dan kembali pada commit.
Validate true hanya boleh berarti seluruh hasil valid; PublishNewAsync wajib create-new atomik, tanpa silent overwrite.
Published + nonempty result adalah satu-satunya success. Cancellation setelah commit tidak mengubah success.
Exception saat publication (termasuk OCE) tidak membuktikan nol efek: ReconciliationRequired, tanpa automatic retry.
Receipt Indeterminate/unknown/Published dengan ID kosong juga perlu rekonsiliasi.
DiscardStageAsync tidak boleh menghapus source/final result; cleanup failure mempertahankan terminal state dan CleanupRequired=true.
Tidak ada timeout/forced cancellation pada foundation; cooperative boundary wajib bounded pada implementasi adapter.
Tidak ada persistence, exactly-once, Job, History atau Audit completion claim.

Tests membuktikan pure orchestration/semantic intent, bukan ACL/path alias safety, crash atomicity, PDF fidelity, atau OS egress denial.
Untuk feature siap pakai tetap perlu Phase 2 PoC, Phase 3 real adapters, safe source leases, physical create-new atomicity,
resource budgets, job/idempotency/recovery, applicable authorization/audit, dan network-disabled E2E evidence.
