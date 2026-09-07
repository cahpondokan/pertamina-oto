# POC-PDF-001 corpus execution-freeze proposal

Task `P2-W0G-A3-CORPUS-ORACLE-FREEZE`, run `P02-20260905-1434`, binds the
existing tracked synthetic foundation to proposed manifest
`POC-PDF-001-CORPUS-MANIFEST-PROPOSAL-001`. It does not create or approve a
corpus for execution.

The A/B/C identities, byte counts, SHA-256 values, markers, and four recipe
expectations are copied exactly from the tracked Wave 0A JSON records. No PDF was
generated, opened, parsed, rendered, processed, or validated while preparing this
packet. `NOT_PRESENT_NOT_REGENERATED_BY_THIS_TASK` means the identities are
specification records only; it is not a statement that matching PDF bytes were
observed in this worktree.

`PRE-007` remains `BLOCKED`. The observed preparation label is
`SYNTHETIC_NON_SENSITIVE`, but a human classification authority, exact corpus
owner/reviewer, run-specific access, retention, expiry, disposal, and approval of
the exact manifest are still `TBC`. E03 structural variants, E04 password cases,
E05 invalid/hostile cases, E07/E08 failure fixtures, and E09/E10 resource bands are
not materialized. No numeric size, page, object, decompression, duration,
concurrency, or fidelity target is introduced.

Only `verify_static.py` in the sibling oracle freeze directory may be run for this
task. It reads JSON/text and hashes tracked files. It neither imports nor invokes
the foundation generator and it rejects any execution/materialization flag set to
true.
