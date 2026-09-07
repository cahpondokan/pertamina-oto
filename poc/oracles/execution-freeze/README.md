# POC-PDF-001 deterministic oracle execution-freeze proposal

This packet specifies expected outcomes for `PDF001-E01..E10` without observing a
PDF result. Every experiment is `NOT_RUN` or `BLOCKED`; no product, PoC, fidelity,
security, performance, offline-processing, or publication PASS is claimed.

The deterministic foundation supplies exact page, ordered occurrence, duplicate,
and marker expectations for `CONTROL_AB`, `ORDER_CAB`, `DUPLICATE_AA`, and
`REMOVE_B`. Byte equality is deliberately not an oracle. A future authorized run
must observe actual staging bytes and require all applicable planes: structural,
render, content/marker, fresh-process reopen, source/target digest, and atomic
publication. Missing, failed, disagreeing, repaired, warned, timed-out, or
inconclusive evidence vetoes publication.

qpdf 12.4.1 remains an unexecuted structural candidate bound to exact ZIP SHA-256
`3cd016cd433ef7232e42f4c13348a49cc14907a3c7278ef4f99120593126f7a6`.
It is not a renderer, content truth, engine approval, or publication authority.
The exact command/argument profile, runtime closure, license/notice, trust/advisory,
and isolation controls remain blocked. Poppler use/copying is held and MuPDF is
deferred, so the complementary render/content oracle fields remain explicit `TBC`
placeholders.

`PRE-007`, `PRE-008`, `PRE-010`, and `PRE-011` remain `BLOCKED`. Their exact
decision questions and blockers are recorded in the proposal. `PRE-012` is outside
this writer lane and no execution authority is implied by a successful static
verification.

Run only the standard-library, text-only verifier:

```text
python -B poc/oracles/execution-freeze/verify_static.py
```

It reads JSON and hashes allowlisted tracked files. It does not import or invoke
the old generator, any PDF parser/renderer, qpdf, Poppler, MuPDF, PDFsharp, Sandbox,
restore, or build tooling. Its negative fixtures are in-memory JSON mutations.
