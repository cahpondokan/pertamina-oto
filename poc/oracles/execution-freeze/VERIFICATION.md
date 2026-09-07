# Static verification evidence

Task: `P2-W0G-A3-CORPUS-ORACLE-FREEZE`

Run: `P02-20260905-1434`

Baseline: `4a78212e01d3daee9ae47f0473fb6186df0dccde`

Command run from the assigned worktree:

```text
python -B poc/oracles/execution-freeze/verify_static.py
```

Result: exit code 0.

```text
STATIC_FREEZE_VERIFICATION=PASS
SCHEMAS=2;SOURCE_BINDINGS=14;NEGATIVE_FIXTURES=7;EXPERIMENTS=10;BLOCKED_GATES=4
PDF_FILES_OPENED=0;PDF_GENERATED=0;PARSER_RENDERER_ENGINE_EXECUTIONS=0
```

`git diff --check` also exited 0. Verification was limited to JSON schema shape,
tracked text/JSON SHA-256 bindings, exact copied foundation identities, explicit
TBC/blocker semantics, false activation flags, and in-memory negative mutations.
No PDF file, generator, parser, renderer, engine, candidate binary, Sandbox,
restore, build, download, or installation was used.
