# Agent 3 — Wave0K export consumer

Baseline `ce9fb7b765bc77c2f848988c5513ba3f03dd9fc5`; clean `task/P2-A3-wave0i` before work. Only new `poc/wave0k/export/consume.py`, `test_consume.py`, and this handoff; no commit. Scope POC-PDF-001, FEAT-PDF-005, PRE-008 under P2-W0H-EXEC-001. Supports bounded count/order assertions only, not full UT/TP cases or Phase2 approval.

## Frozen interface

Run with existing Python 3.14: `python.exe -B poc/wave0k/export/consume.py --directory <closed-private-export> --expected <root-expected.json> --expected-sha256 <lowercase-sha256>`; optionally `--case AB` (CAB/AA/AC). Default requires all four cases in one directory. One-case mode requires exactly that case's six files, not siblings.

Exact six names per case: `AB.binding.json`, `AB.result.json`, `AB.version.stdout`, `AB.version.stderr`, `AB.observation.stdout`, `AB.observation.stderr`; same prefix for other cases. Binding/result preserve existing observe.py schemas. JSON <=64KiB; stdout <=1MiB; stderr <=64KiB and must actually be empty. No PDFs or subdirectories/extra files. Ancestor/member symlink and Windows reparse attributes rejected; hardlinked members rejected. Strict duplicate-key, finite numeric/schema/type/hash checks. Bounds checked before and after read. The consumer never uses a guest-provided path or subprocess command.

Root expected.json exact keys: `schema` integer1, `observerScriptSha256` lowercase SHA256, `sourceSha256` exactly A/B/C lowercase hashes, `qpdfFiles` exact selected flat qpdf member hashes. Root freezes this from approved source/runtime staging before launch, not from exports. CLI separately pins its raw bytes with --expected-sha256. Expected members validated through the existing strict binding validator. Trusted observer source is fixed relative `poc/wave0i/oracle/observe.py`; hash must match expected, then inert import reuses inspect_json, validate_binding and checked_capture. Neither observer main/capture nor PDF parser/engine executes on host. Root should include `-B` to suppress import cache writes.

Consumer verifies binding hash, result/source/tool/script relationships, exact argument template, zero exit/empty stderr, capture SHA256 and version header, and independently replays inspect_json on raw qpdf observation. Count and markers must match actual parsed ordered flat-tree content, not result claims. Rehashed reordered raw observations are rejected.

Success emits BOUNDED_EXPORT_CONSISTENT plus all export SHA256 and per-case count/output hash, while retaining ROOT_LEDGER_REVIEW_REQUIRED, fidelity NOT_ASSESSED, GUEST_ASSERTED_OUTPUT_HASH and runtimeAuthenticity NOT_PROVEN_BY_EXPORT. Output hash cannot independently be known without PDF export; this consumer does not pretend otherwise. Root must corroborate exact guest script/manifest/config/CLI receipt, zero guest exit, isolation, source hashes and session closure. Copied/forged internally consistent JSON remains possible: not signed runtime attestation. Raw version/observation are generated tool text for fixed synthetic inputs only; not arbitrary document data.

## Verification and limits

Command from agent worktree: `C:\Python314\python.exe -B -m unittest discover -s poc/wave0k/export -p test_consume.py -v`. Synthetic JSON/text fixtures only; Popen forbidden by test guard. Tests include four-case/single-case, schema/type, missing/extra/directory, capture/binding/frozen-source tamper, rehashed wrong ordering, stderr/size, duplicate/nonfinite, source hash, hardlink and mocked Windows reparse flag rejection. No package/build/engine/Sandbox/PDF read/download. Source-only test success is not actual independent runtime evidence.

Use only after root closes the guest and snapshots/controls the private export directory. Check-then-open/hash-then-import does not defeat a concurrent malicious filesystem writer; ACL isolation and closed guest are prerequisites, not proven here. Capture elapsed values are self-reported <=30 seconds, not host performance measurement or hard quotas. Host consumer has bounded aggregate bytes but no OS memory/process security boundary. General fidelity, hostile documents, content security, recovery and all remaining PoC dispositions stay outstanding.

Integration: root copies these three new files hash-identically after independent review; freeze expected manifest; collect exact reviewed exports with actual operator ledger; close guest; run consumer. No new human decision in this lane, no cost.

Compatibility revision: consumer now deliberately loads fixed `poc/wave0k/oracle/observe.py`, not historical wave0i path mentioned in the original interface paragraph. Root must pin the new source SHA. See ../oracle/HANDOFF.md for bounded optional metadata/group profile change. Historical wave0i remains unchanged; failed run evidence remains failed, not retroactively upgraded.
