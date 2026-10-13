# Reader continuous scroll — scoped task card

- Agent 0 only, under the user's resource preference. User reports scrolling stops at the bottom instead of continuing to the next PDF page.
- Baseline: existing dirty `task/core-functional-first` at `84cbb0f`; latest local package `20260930-141956-a5a4707b`. Preserve all earlier reader/search/default-app changes.
- Objective: native continuous vertical scrolling across PDF pages in both directions, synchronized page controls, working jump/search/zoom, bounded rendered-image memory and stale-render cancellation.
- Allowed paths: ReaderWorkspace, new reader scrolling implementation, native reader audit, focused evidence, development documentation, coordination records and verified local update feed.
- Do not touch: source documents, other tools/engines, installed/running application, Windows defaults, dependencies, phase/Production gates.
- Acceptance: scroll position reaches later/earlier pages without clicking navigation; jumps/search retain correct page; first/last limits safe; fit/zoom and mixed page sizes work; only nearby/visible bitmaps retained; source bytes preserved.
- Verification: native UI audit with actual ScrollViewer offsets, zoom and document replacement; screenshots across page boundary; existing standard package regression/build gates and final payload hashes. Physical touchpad/gesture UAT remains disclosed.
- Delivery: existing Settings local-update flow. No automatic installation, forced closure or Production publishing.

## Completion evidence

- Implemented, verified and published local update `20260930-144154-279ce46c`.
- Native reader63, worker41, editor63, search25, helper41 PASS; total233. 368/368 payload hashes verified; tested and packaged DLLs identical.
- Final evidence and limitations: `worktrees/core-functional-first/docs/development/READER_CONTINUOUS_SCROLL.md`.
- Installation and physical input UAT remain user actions. No phase exit or human acceptance is inferred.
