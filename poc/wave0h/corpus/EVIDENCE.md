# Agent 3 Wave 0H preparation evidence

Date: 2026-09-07. Baseline d472b85e179d715b9795773bcfda01dc628e9d42.
Branch task/P2-A3-wave0h; fresh worktree worktrees/phase2-20260905/agent-3-wave0h.

Static verification via C:\Python314\python.exe, stdin script, exit 0:

```text
AST and stdlib imports PASS: generate.py
AST and stdlib imports PASS: oracle.py
Static ordered marker/count mapping PASS: AB
Static ordered marker/count mapping PASS: CAB
Static ordered marker/count mapping PASS: AA
Static ordered marker/count mapping PASS: AC
Generator literal equals independent spec PASS
No module import, generator invocation, PDF generation or PDF parsing performed.
```

Method: ast.parse each Python source (without importing it); assert import names subset of hashlib/json/pathlib/sys; json.loads spec; assert six source pages; flatten each explicit input list and compare exact expected marker sequence and counts 5/6/4/3; ast.literal_eval generator's sources literal and compare with spec sources. No generated PDF, candidate runtime result, rendering result, build or restore is claimed.

Deliverables: README.md execution/provenance/coverage protocol; spec.json exact four-case expectations and IDs; generate.py fixed benign deterministic PDF serializer; oracle.py bounded metadata/byte-hash comparator; this evidence. All changes confined to poc/wave0h/corpus/**.

Risk and next actions: emitted PDF validity and runtime exception paths have not been tested because this lane explicitly prohibits generator/parser execution. Agent 0 must audit and execute in admitted environment; compatible guest Python availability must be checked against Agent 1 plan. Freeze sources.json before candidate execution. Independent observer evidence is mandatory for an independent structural verdict and is not supplied by assertions in JSON. Fidelity, UI actions, negative cases, atomic publication, malicious inputs and performance remain outside this smoke. No human approval or Phase 2 exit is asserted by Agent 3.
