# Agent1 / P2-W0J guest engine smoke

Objective: four sequential fixed benign synthetic merges, no oracle claim or host export.
Baseline: 16a53801020ab4571bfa09b3bdd39961eabf9a99; initial git status clean.
Worktree: agent-1-wave0i; no branch or commit mutation. Only new poc/wave0j/environment files.
Scope IDs: POC-PDF-001, FEAT-PDF-005, P2-W0H-EXEC-001. Throwaway harness, not product adapter or canonical Merge tests.

Files: Guest-Smoke.ps1, Test-Static.ps1, this handoff.
Guest final raw SHA256: 849A24015AE2FC0A9AE9E6ED45997760557E323AF445B22A105D5E7A63B35B34.
Test raw SHA256: 4C289FA0B812D97296B5AB3C2C5E187B7283DC2E810A9DA9B3EFCDF450479E58.

Stage entrypoint exactly C:\PocInput\smoke\Guest-Smoke.ps1, pass mandatory LedgerSha256 of the trusted generated sources.json. Corpus under C:\PocInput\corpus; runtime and app retain existing bundle paths. Root MUST independently verify complete payload closure and freeze ledger digest before run. Only readonly host mapping, no writable mapping. Guest-only guard rejects another script directory, network interfaces, existing private run directory, linked files/directories, changed ledger or sources. Private outputs C:\PocPrivate\AB, CAB, AA, AC. Exit0 means all engine exits0, nonempty outputs <=262144 bytes, unchanged source hashes; receipt status ENGINE_SMOKE_NOT_ORACLE_VALIDATED. Exit20 location,21 network,22 initial shape/digest,24 existing private directory,25 timeout reaped,26 timeout not reaped,27 other validation/exception. No raw errors emitted by wrapper.

Verification: current PowerShell `& ./poc/wave0j/environment/Test-Static.ps1` exit0, PASS7 syntax/pure outcome checks. Tests include zero/nonzero engine exit, empty/excessive/boundary output, changed sources Boolean outcome. Explicit Windows PowerShell5 `-File` could not load test due execution policy; no policy changed. Thus PS5 runtime verification remains root gate, not claimed PASS. No engine, PDF generation/parsing, Sandbox or download performed by Agent1.

Limitations: no independent page/order/content/fidelity verification; no hard memory/disk quota; post-write size bound only. Child waits15seconds, direct child kill then5second reap; no descendant-process guarantee. Outer exact-session supervision mandatory. Four cases nominal <=80seconds of child waits but filesystem/OS calls not hard bounded. Private receipts lost when guest disposed; root may rely on CLI exit receipt only for bounded engine smoke, not full evidence export. Full payload hashing and runtime trust remain root responsibility. Child stdout/stderr not captured; fixed synthetic corpus only. Existing PDFsharp security limitations remain. No approval or new dependency requested.

Root-requested interface revision supersedes staged-script requirement above: embed exact hash-bound source as `& { <source> } -LedgerSha256 <hash>` in EncodedCommand. No smoke directory or script file is required in the219-file root stage. Guard now requires SystemRoot C:\Windows and USERNAME WDAGUtilityAccount plus exact input directories. These are sanity checks, not cryptographic proof of guest identity; root's exact-session CLI isolation is mandatory. Frozen root manifest reported4D620CD59E18A6AE6E51B403E0CC84F3024417C5E7B884B80274DDC11C4B4F3C, independently verify before launch.

Additional verification: Windows PowerShell5 Parser.ParseFile via encoded read-only command returned PS5_PARSE_PASS exit0 on initialsource. Finalsource seven syntax/pure outcome checks also PASS under Windows PowerShell5 via EncodedCommand reading the test text and substituting its source directory; no execution policy setting changed. No live guest/engine/PDF execution. Earlier -File rejection accurately retained.

Integration: independently review source, freeze digest/ledger and assembled mapping, run root pure tests, then root decides bounded guest execution under standing scope. Do not claim Phase2 exit or production readiness.
