# Wave 0A harness foundation

Task P2-W0A-A1-HARNESS; authority P2-W0-ENV-001 and P2-W0-EVID-001;
ZERO_ADDITIONAL_COST. BCL only, no product references or candidate loading.
This standalone console project is excluded from the product solution graph.

Run `eng/poc/Verify-Harness.ps1` from PowerShell using installed SDK 10.0.400.
Restore uses an empty package-source configuration and the committed empty lock.
The explicit BCL self-test registry discovers 20 tests, returns nonzero on failure,
and treats zero discovery as failure. This is not MSTest or a product test suite.
The CLI supports only `self-test` and `manifest`; other commands return exit 2.

The environment collector allowlists OS/runtime/architectures and logical CPU
count. It deliberately excludes machine/user names, environment variables,
paths, command lines and document content. It reports DISABLED_REQUIRED with
NetworkDisabledVerified=false and ProcessingAuthorized=false. Configuration is
not offline proof. Full SDK/hardware/storage/image/corpus/config binding must be
added to a reviewed execution manifest later. A deterministic identity binds full
commit and environment/corpus/config SHA-256 digests plus repetition number.
Callers must hash canonical, frozen inputs, use distinct repetition numbers, and
reject reuse when reserving a run directory; this skeleton creates no directories.

OwnedPath validates local absolute root, run identity and relative descendants;
traversal, rooted paths, alternate streams, trailing dots/spaces and existing
reparse ancestors are rejected. It never authorizes an arbitrary supplied root:
the caller must obtain the exact approved root from governance. Validation is
advisory and subject to filesystem races. No cleanup/delete/write implementation
is exposed. A future executor needs handle-based containment and revalidation,
reserved-device-name rejection, ACL review and exclusive run-root reservation.

Lifecycle tests exercise queued/running/validation cancel, timeout, failure,
invalid transitions, invalid results and no late completion. PublicationAllowed
is a model property, not an actual filesystem publication capability. Cleanup
records remain RECONCILIATION_REQUIRED; no false cleanup PASS is produced.

## Supervision and cleanup design for later authorization

1. Freeze environment, candidate, corpus, policy and evidence digests; verify all
   PRE gates. Reserve distinct input/output/temp/evidence roots and deny reuse.
2. Launch only an approved executable under least privilege in an isolated guest.
   Use a Windows Job Object without breakaway, assign before allowing child work,
   and configure kill-on-close. A supervisor outside the job owns its lifetime.
3. Apply approved wall-clock timeout and cooperative-cancel grace period. On expiry
   terminate the job, wait for the process tree to exit within an approved bound,
   and record failure if it cannot be proven stopped. Do not publish after cancel.
4. Capture monotonic lifecycle timestamps and per-run/aggregate CPU, memory, GC,
   I/O, handles, threads and temp metrics. Sampling/limits remain human decisions.
5. Reconcile run-owned inventory after stop/restart; compare source/target digests.
   Never recursively delete a caller-supplied path. Refuse reparse points and
   ambiguous ownership, retain bounded sanitized failure evidence for review.
6. Prove OS network disablement and attempted-egress observation independently;
   run synthetic process-tree/watchdog tests before any approved hostile suite.

None of these process/OS controls are implemented or claimed PASS in Wave 0A.
No process launching, admin mutation, network APIs, PDF execution, public endpoint,
paid tool or Local Worker implementation is included. BASE-OI-013 remains open.

Trace: FEAT-PDF-005; PDF001-E00 preparation, PDF001-E07 path foundation,
PDF001-E08 lifecycle foundation, PDF001-E09 environment foundation,
PDF001-E10 identity foundation, PDF001-E11 isolated graph foundation.
Relevant canonical test intent: UT-MERGE-026, UT-MERGE-027, UT-MERGE-028,
UT-MERGE-030, UT-MERGE-033, UT-MERGE-034, UT-MERGE-035, UT-MERGE-038.
These self-tests do not replace those canonical tests or prove PDF experiments.
