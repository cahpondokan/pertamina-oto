# Agent 2 Wave 0H handoff

Status: static candidate packet complete; execution readiness conditional, not engine admission or Phase 2 completion.

Changed paths: README.md, EVALUATION-NOTICES.md, MergeSmoke.cs, HANDOFF.md, all under poc/wave0h/candidate/. No coordination or other lane paths changed.

Verification: reference existence checks passed for candidate-freeze.json, PDFsharp.nuspec.xml, qpdf intake-manifest.json; all three Microsoft legal JSON files were present. Static diff whitespace check passed. Primary issue reports and qpdf release notes reviewed as described in README. Archive hashes are transcribed existing intake identities, not fresh verification in this lane. No acquisition, install, compiler, restore, PDF parser or engine execution occurred. Harness is unbuilt and untested; no test pass claimed.

Risks and remaining blockers: fatal parser defects on non-benign input; unresolved exact qpdf-version applicability; incomplete embedded-component provenance; guest transfer/export, runtime/compiled output binding, supervision, independent corpus oracle and actual engine evidence remain for Agent 0 and independent reviewers. Post-write output length check is not a hard disk bound. Reviewed private guest paths and generated inputs are required.

Agent 0 should audit this packet together with environment and corpus handoffs, obtain independent Wave B reviews, and execute only after technical readiness is established. Existing user authorization must not be requested again solely because historical Wave 0G flags were false.

## Agent 0 review correction

Reviewed Agent 3's poc/wave0h/corpus/spec.json in agent-3-wave0h. B has three pages; the original two-page harness ceiling would reject the recommended CAB case. Raised only the per-input page ceiling to three in MergeSmoke.cs and README.md. The three-input and 64 KiB/input limits remain. Static comparison confirms A=2, B=3, C=1, CAB=6 and AA=4 expected pages. No generator, build, restore or engine was executed for this correction. Conditional readiness and remaining blockers above are unchanged.
