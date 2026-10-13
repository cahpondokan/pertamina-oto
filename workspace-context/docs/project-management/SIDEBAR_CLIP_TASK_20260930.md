# Compact sidebar clipping — scoped task card

- Agent 0 only. User supplied a cropped sidebar screenshot and reported visibly cut-off content.
- Baseline: dirty `task/core-functional-first` at `84cbb0f`; latest feed `20260930-144154-279ce46c`. Preserve existing changes.
- Finding: compact rail 72 DIP minus two 12 DIP outer insets gives a 48 DIP button. Its two 14 DIP inner insets leave only 20 DIP for a 24 DIP FontIcon.
- Objective: give compact icons enough space and center them, preserving expanded labels, selected state, tooltips and navigation.
- Allowed paths: MainWindow layout, task documentation/evidence and verified local update feed. Do not touch document processing, Windows defaults, dependencies, installed/running app or phase gates.
- Verification: desktop build; existing native history audit and visual review at compact/expanded widths; standard local package checks and integrity binding. No new tests for this small layout-only fix.
- Delivery: existing Settings local-update flow; no forced closure or automatic installation.

Completion: local update `20260930-144755-9abaa2ed` published. Native history35 and package regressions170 PASS (205 total); wide/narrow visual captures reviewed; all368 file hashes verified and tested DLLs matched. Details `worktrees/core-functional-first/docs/development/SIDEBAR_CLIP.md`. User installation remains required; phase gates unchanged.
